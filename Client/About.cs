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

            label2.Text = "Version: "+Application.ProductVersion.ToString();
        }

        private async void About_Load(object sender, EventArgs e)
        {
            // Получение актуальной версии приложения
            try
            {
                using var client = new HttpClient();
                var content = await client.GetStringAsync("https://notechat-server.herokuapp.com/version/clientdesktop");

                if (content != null && content != Application.ProductVersion.ToString())
                    label5.Text = "Последняя версия: " + content + "\nНадо бы обновиться";
            }
            catch (Exception ex)
            {

            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        // Пасхалка
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            label4.Text = "Таганрог";
            pictureBox1.Load("https://c.tenor.com/2roX3uxz_68AAAAM/cat-space.gif");
        }
    }
}
