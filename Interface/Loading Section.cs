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
    public partial class Loading_Section : Form
    {
        Form1 f;
        public Loading_Section(Form1 f)
        {
            InitializeComponent();
            this.f = f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            f.panelform(new ResultForm(f));
        }

        private void Loading_Section_Load(object sender, EventArgs e)
        {
            
        }



    }
}
