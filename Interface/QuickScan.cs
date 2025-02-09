using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interface
{
    public partial class QuickScan : Form
    {
        Form1 f;
        public QuickScan(Form1 f)
        {
            InitializeComponent();
            this.f = f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            f.panelform(new Loading_Section(f));
        }

        private void QuickScan_Load(object sender, EventArgs e)
        {

        }
    }
}

