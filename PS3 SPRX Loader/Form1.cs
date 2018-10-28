using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS3_SPRX_Loader {
    public partial class Form1 : Form {
        private static TMAPI PS3 = new TMAPI();
        private static RPC RPC = new RPC();

        public Form1() {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Module;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.Module = textBox1.Text;
            Properties.Settings.Default.Save();

            if(PS3.IsConnected) {
                RPC.Disable();
                PS3.DisconnectTarget();
            }
        }

        private void connectToPS3Button_Click(object sender, EventArgs e) {
            try {
                if (PS3.ConnectTarget() && PS3.AttachProcess()) {
                    button6.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                }
            }
            catch {
                MessageBox.Show("Unable to connect and attach to PS3");
            }
        }

        private void disconnectFromPS3Button_Click(object sender, EventArgs e) {
            RPC.Disable();

            button6.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button1.Enabled = true;
        }

        private void enableRPCButton_Click(object sender, EventArgs e) {
            if (RPC.Enable(PS3, comboBox1.Text)) {
                button4.Enabled = true;
                button5.Enabled = true;
            }
            else {
                MessageBox.Show("Game is not supported");
            }
        }

        private void disableRPCButton_Click(object sender, EventArgs e) {
            RPC.Disable();

            button4.Enabled = false;
            button5.Enabled = false;
        }

        private void refreshModules() {
            dataGridView1.Rows.Clear();

            uint[] modules = RPC.GetModules();
            for (int i = 0; i < 10; i++) {
                if (modules[i] != 0x0) {
                    string Name = PS3.GetModuleName(modules[i]);
                    string ID = "0x" + modules[i].ToString("X");
                    string Start = "0x" + PS3.GetModuleStartAddress(modules[i]).ToString("X");
                    string Size = "0x" + PS3.GetModuleSize(modules[i]).ToString("X");
                    dataGridView1.Rows.Add(new object[] { Name, ID, Start, Size, null });
                }
            }
        }

        private void button5_Click(object sender, EventArgs e) => refreshModules();

        private void button4_Click(object sender, EventArgs e) {
            if (RPC.GetModuleCount() < 10) {
                uint error = RPC.LoadModule(textBox1.Text);
                if (error != 0x0)
                    MessageBox.Show("Load Module Error: 0x" + error.ToString("X"));

                refreshModules();
            }
            else {
                MessageBox.Show("This tool only supports up to 10 modules");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            uint moduleId = Convert.ToUInt32(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), 16);

            uint error = RPC.UnloadModule(moduleId);
            if (error != 0x0)
                MessageBox.Show("Unload Module Error: 0x" + error.ToString("X"));

            refreshModules();
        }
    }
}
