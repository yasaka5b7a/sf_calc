using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sf_calc
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            Form1 sForm = (Form1)this.Owner;
            dataGridView1.DataSource = sForm.bindingSource1;
        }

        private void Btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
