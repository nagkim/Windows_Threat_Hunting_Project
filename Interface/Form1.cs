using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interface
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int iparam);
        public void panelform(object frm)
        {
            if (pHome.Controls.Count > 0)
            {
                pHome.Controls.RemoveAt(0);
            }
            Form f = frm as Form;
            f.TopLevel = false;
            f.Dock = DockStyle.Fill;
            pHome.Controls.Add(f);
            pHome.Tag = f;
            f.Show();
        }
        private void pbmaxim_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pbmin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            pbmax.Visible = true;
            pbmin.Visible = false;
        }

        private void pbmax_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            pbmax.Visible = false;
            pbmin.Visible = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Maximized)
            {
                pbmax.Visible = false;
                pbmin.Visible = true;
            }
            panelform(new QuickScan(this));
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button1.BackColor = Color.White;
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.interfacepositioning == "0")
            {
                panelform(new QuickScan(this));
            }
            else
            {
                panelform(new ResultForm(this));
            }
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button1.BackColor = Color.White;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panelform(new File_Scan(""));
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button3.BackColor = Color.White;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            panelform(new Services());
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button4.BackColor = Color.White;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            panelform(new Ask_Gpt("","","","","","","","","",""));
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button6.BackColor = Color.White;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            panelform(new Smart_Scan(this));
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button5.BackColor = Color.White;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
           
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            panelform(new Tasks());
            foreach (Control ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            button2.BackColor = Color.White;
        }
    }
}
