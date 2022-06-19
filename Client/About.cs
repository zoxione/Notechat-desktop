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

            //main_label.Text = "Как жизнь" + "\r" + "молодая?";
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
            }catch (Exception ex)
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
            label1.Text = "Оаооаоаоамммммм";
            label4.Text = "Таганрог";
            pictureBox1.Load("https://s0.rbk.ru/v6_top_pics/media/img/2/84/754598886185842.jpeg");
        }
    }
}
