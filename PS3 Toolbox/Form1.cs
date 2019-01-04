using System;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace PS3_SPRX_Loader {
    public partial class Form1 : Form {
        private static TMAPI PS3 = new TMAPI();
        private static PS3RPC PS3RPC = new PS3RPC(PS3);

        public Form1() {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Module;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.Module = textBox1.Text;
            Properties.Settings.Default.Save();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/skiffaw/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/egatobaS");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/skiffaw/");
        }

        private void button3_Click(object sender, EventArgs e) {
            try {
                if (!PS3.ConnectTarget())
                    return;

                string IP = PS3.GetTargetName();
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(new IPEndPoint(IPAddress.Parse(IP), 80));
                socket.Send(System.Text.Encoding.ASCII.GetBytes("GET /restart.ps3 HTTP/1.1\nHost: localhost\nContent-Length: 512\n\r\n\r\n"));
                socket.Close();
            }
            catch {
                MessageBox.Show("You must use webman and have the PS3 IP as the target name in target manager");
            }
        }

        private void connectToPS3Button_Click(object sender, EventArgs e) {
            try {
                if (PS3.ConnectTarget() && PS3.AttachProcess()) {
                    refreshModules();

                    label1.Text = "Current Game: " + PS3.GetCurrentGame();
                    label13.Text = TMAPI.Parameters.Info;
                }
            }
            catch {
                MessageBox.Show("Unable to connect and attach to PS3");
            }
        }

        private void disconnectFromPS3Button_Click(object sender, EventArgs e) {
            dataGridView1.Rows.Clear();

            label1.Text = "Current Game: No Game Detected";
        }

        private void refreshModules() {
            dataGridView1.Rows.Clear();

            uint[] modules = PS3RPC.GetModules();
            for (int i = 0; i < modules.Length; i++) {
                if (modules[i] != 0x0) {
                    string Name = PS3.GetModuleName(modules[i]);
                    string ID = "0x" + modules[i].ToString("X");
                    string Start = "0x" + PS3.GetModuleStartAddress(modules[i]).ToString("X");
                    string Size = "0x" + PS3.GetModuleSize(modules[i]).ToString("X");
                    dataGridView1.Rows.Add(new object[] { Name, ID, Start, Size, null });
                }
            }
        }

        private void loadModuleBtn_Click(object sender, EventArgs e) {
            string modulePath = textBox1.Text;
            if (!modulePath.Contains("hdd0"))
                modulePath = "/host_root/" + textBox1.Text;

            modulePath.Replace("\\", "/");

            ulong error = PS3RPC.LoadModule(modulePath);

            Thread.Sleep(150);

            refreshModules();

            if (error != 0x0)
                MessageBox.Show("Load Module Error: 0x" + error.ToString("X"));
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 4) {
                uint moduleId = Convert.ToUInt32(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), 16);

                ulong error = PS3RPC.UnloadModule(moduleId);

                Thread.Sleep(150);

                refreshModules();

                if (error != 0x0)
                    MessageBox.Show("Unload Module Error: 0x" + error.ToString("X"));
            }
        }

        private void browseFilesBtn(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "SPRX Files|*.sprx";
            openFileDialog1.Title = "Select a File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void systemCallBtn_Click(object sender, EventArgs e) {
            ulong R3 = Convert.ToUInt64(textBox2.Text, 16);
            ulong R4 = Convert.ToUInt64(textBox3.Text, 16);
            ulong R5 = Convert.ToUInt64(textBox4.Text, 16);
            ulong R6 = Convert.ToUInt64(textBox5.Text, 16);
            ulong R7 = Convert.ToUInt64(textBox6.Text, 16);
            ulong R8 = Convert.ToUInt64(textBox7.Text, 16);
            ulong R9 = Convert.ToUInt64(textBox8.Text, 16);
            ulong R10 = Convert.ToUInt64(textBox9.Text, 16);

            uint ID = Convert.ToUInt32(textBox10.Text, 16);

            ulong RET = PS3RPC.SystemCall(ID, R3, R4, R5, R6, R7, R8, R9, R10);

            label11.Text = String.Format("Return Value: 0x{0}", RET.ToString("X"));
        }

        private object GetParameter(string typeName, string value) {
            try {
                if (typeName.Equals("long"))
                    return Convert.ToUInt64(value, 16);
                if (typeName.Equals("float"))
                    return Convert.ToSingle(value);
                if (typeName.Equals("string"))
                    return value;
                return 0;
            }
            catch {
                return 0;
            }
        }

        private void callFunctionBtn_Click(object sender, EventArgs e) {
            object[] registers = new object[9];

            registers[0] = GetParameter(comboBox1.Text, textBox11.Text);
            registers[1] = GetParameter(comboBox2.Text, textBox12.Text);
            registers[2] = GetParameter(comboBox3.Text, textBox13.Text);
            registers[3] = GetParameter(comboBox4.Text, textBox14.Text);
            registers[4] = GetParameter(comboBox5.Text, textBox15.Text);
            registers[5] = GetParameter(comboBox6.Text, textBox16.Text);
            registers[6] = GetParameter(comboBox7.Text, textBox17.Text);
            registers[7] = GetParameter(comboBox8.Text, textBox18.Text);
            registers[8] = GetParameter(comboBox9.Text, textBox19.Text);

            uint Address = Convert.ToUInt32(textBox20.Text, 16);
            int Count = Convert.ToInt32(numericUpDown1.Value);

            object[] parameters = new object[Count];
            for (int i = 0; i < Count; i++)
                parameters[i] = registers[i];

            if(comboBox10.Text.Equals("long")) {
                long ret = PS3RPC.Call<long>(Address, parameters);
                label14.Text = String.Format("Return Value: 0x{0}", ret.ToString("X"));
            }
            else if (comboBox10.Text.Equals("float")) {
                float ret = PS3RPC.Call<float>(Address, parameters);
                label14.Text = String.Format("Return Value: 0x{0}", ret);
            }
            else if (comboBox10.Text.Equals("string")) {
                string ret = PS3RPC.Call<string>(Address, parameters);
                label14.Text = String.Format("Return Value: 0x{0}", ret);
            }
            else {
                PS3RPC.Call<int>(Address, parameters);
                label14.Text = "Return Value: No Return";
            }
        }
    }
}
