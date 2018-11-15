using System;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace PS3_SPRX_Loader {
    public partial class Form1 : Form {
        private static TMAPI PS3 = new TMAPI();
        private static RPC RPC = new RPC(PS3);

        public Form1() {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Module;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.Module = textBox1.Text;
            Properties.Settings.Default.Save();

            if(PS3.IsConnected)
                RPC.Disable();
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
                    button6.Enabled = true;
                    button4.Enabled = true;

                    button1.Enabled = false;

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

            button6.Enabled = false;
            button4.Enabled = false;

            button1.Enabled = true;

            label1.Text = "Current Game: No Game Detected";
        }

        private void refreshModules() {
            dataGridView1.Rows.Clear();

            uint[] modules = RPC.GetModules();
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

        private void button4_Click(object sender, EventArgs e) {
            if (RPC.Enable(label1.Text)) {
                string modulePath = textBox1.Text;
                if (!modulePath.Contains("hdd0"))
                    modulePath = "/host_root/" + textBox1.Text;

                modulePath.Replace("\\", "/");

                uint error = RPC.LoadModule(modulePath);

                Thread.Sleep(150);

                RPC.Disable();

                refreshModules();

                if (error != 0x0)
                    MessageBox.Show("Load Module Error: 0x" + error.ToString("X"));
            }
            else {
                MessageBox.Show("Game is not supported");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 4) {
                uint moduleId = Convert.ToUInt32(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), 16);

                if (RPC.Enable(label1.Text)) {
                    uint error = RPC.UnloadModule(moduleId);

                    Thread.Sleep(150);

                    RPC.Disable();

                    refreshModules();

                    if (error != 0x0)
                        MessageBox.Show("Unload Module Error: 0x" + error.ToString("X"));
                }
                else {
                    MessageBox.Show("Game is not supported");
                }
            }
        }

        private void button5_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "SPRX Files|*.sprx";
            openFileDialog1.Title = "Select a File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                textBox1.Text = openFileDialog1.FileName;
            }
        }
    }
}
