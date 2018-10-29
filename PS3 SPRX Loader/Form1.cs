using System;
using System.Threading;
using System.Windows.Forms;

namespace PS3_SPRX_Loader {
    public partial class Form1 : Form {
        private static TMAPI PS3 = new TMAPI();
        private static RPC RPC = new RPC(PS3);

        public Form1() {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Module;
            comboBox1.Text = Properties.Settings.Default.Game;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.Module = textBox1.Text;
            Properties.Settings.Default.Game = comboBox1.Text;
            Properties.Settings.Default.Save();

            if(PS3.IsConnected)
                RPC.Disable();
        }

        private void button2_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/skiffaw/PS3-SPRX-Loader/blob/master/README.md");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/skiffaw/PS3-SPRX-Loader");
        }

        private void connectToPS3Button_Click(object sender, EventArgs e) {
            try {
                if (PS3.ConnectTarget() && PS3.AttachProcess()) {
                    button6.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;

                    button1.Enabled = false;
                    comboBox1.Enabled = true;

                    refreshModules();
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
            button5.Enabled = false;

            button1.Enabled = true;
            comboBox1.Enabled = false;
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
            if (RPC.Enable(comboBox1.Text)) {
                string modulePath = textBox1.Text;
                if (!modulePath.Contains("hdd0"))
                    modulePath = "/host_root/" + textBox1.Text;

                modulePath.Replace("\\", "/");

                uint error = RPC.LoadModule(modulePath);
                if (error != 0x0)
                    MessageBox.Show("Load Module Error: 0x" + error.ToString("X"));

                Thread.Sleep(100);

                refreshModules();

                RPC.Disable();
            }
            else {
                MessageBox.Show("Game is not supported");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 4) {
                uint moduleId = Convert.ToUInt32(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), 16);

                if (RPC.Enable(comboBox1.Text)) {
                    uint error = RPC.UnloadModule(moduleId);
                    if (error != 0x0)
                        MessageBox.Show("Unload Module Error: 0x" + error.ToString("X"));

                    Thread.Sleep(100);

                    refreshModules();

                    RPC.Disable();
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
