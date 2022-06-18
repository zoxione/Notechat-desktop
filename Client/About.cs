using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Сведения : Form
    {
        public Сведения()
        {
            InitializeComponent();

            main_label.Text = "Как жизнь" + "\r" + "молодая?";
        }

        private void About_Load(object sender, EventArgs e)
        {

        }
    }
}
