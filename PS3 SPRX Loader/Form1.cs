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

        private void button2_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/skiffaw/PS3-SPRX-Loader");
        }

        private void connectToPS3Button_Click(object sender, EventArgs e) {
            try {
                if (PS3.ConnectTarget() && PS3.AttachProcess()) {
                    refreshModules();

                    label1.Text = "Current Game: " + PS3.GetCurrentGame();
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
            ulong ret = PS3RPC.SystemCall(0x1);
            MessageBox.Show(ret.ToString("X"));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            object ret = PS3RPC.FunctionCall(0x399CC8, new object[] { 0, "fastrestart;" });
        }
    }
}
