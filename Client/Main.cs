using SocketIOClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Client
{
    public partial class Main : Form
    {
        SocketIO client;
        List<User> users;
        string textRichTextBox;
        string ip = "https://notechat-server.herokuapp.com/";
        bool offlineMode = false;
        string room = "doc";

        public Main()
        {
            Form.CheckForIllegalCrossThreadCalls = false; // Безумный костыль для исправления ошибки когда мы не можем с потока изменять текст в RichTextBox, лучше найти норм решение на сайте
                                                          // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls?view=netframeworkdesktop-4.8
            InitializeComponent();

            Connect();
        }

        void Connect() 
        {
            string localip = ip;

            client = new SocketIO(ip);

            try
            {
                Thread ThreadConnect = new Thread(Listen);
                ThreadConnect.Start();
            }
            catch (Exception ex)
            {
                offlineMode = true;
            }

            while (true)
            {
                if (client.Attempts > 1 && client.Disconnected)
                {
                    offlineMode = true;
                    break;
                }
                else if (client.Connected)
                {
                    offlineMode = false;
                    break;
                }
            }

            updateToolStripStatusLabel2();
        }

        async private void Listen()
        {
            client.On("user_connected", response =>
            {
                Console.WriteLine(response);

                User u = response.GetValue<User>();
                users.Add(u);

                updateToolStripStatusLabel1();
            });

            client.On("users", response =>
            {
                Console.WriteLine(response);

                users = response.GetValue<List<User>>();

                Console.WriteLine(users);

                updateToolStripStatusLabel1();

            });

            client.On("user_disconnected", response =>
            {
                Console.WriteLine(response);

                User u = response.GetValue<User>();
                users.RemoveAll(user => user.userID == u.userID);

                updateToolStripStatusLabel1();
            });

            client.On("past_history", response =>
            {
                Console.WriteLine(response);

                string text = response.GetValue<string>();

                richTextBox.Text = text;
            });

            client.On("message", response =>
            {
                Console.WriteLine(response);

                string text = response.GetValue<string>();

                textRichTextBox = text;
                richTextBox.Text = text;
            });

            client.OnConnected += async (sender, e) =>
            {
                // Emit a string
                // await client.EmitAsync("hi", "socket.io");

                // Emit a string and an object
                // var dto = new TestDTO { Id = 123, Name = "bob" };
                // await client.EmitAsync("register", "source", dto);
            };
            
            await client.ConnectAsync();
        }

        // TODO: сравнения текста в текстбоксе и нахождение новых измненеий, чтобы отправлять на сервер не все сразу, а только измненную часть
        void processText()
        {
            string oldText = textRichTextBox;
            string newText = richTextBox.Text;


        }

        // Лейбл с кол-вом юзеров
        void updateToolStripStatusLabel1()
        {
            toolStripStatusLabel1.Text = users.Count.ToString();

            string temp = "";
            foreach (User u in users)
            {
                temp += (u.userID + '\n');
            }

            toolStripStatusLabel1.ToolTipText = temp;
        }
        // Лейбл с адресом сервера
        void updateToolStripStatusLabel2()
        {
            if (offlineMode == true) 
            {
                toolStripStatusLabel2.Text = "offline";
                return;
            }
            
            toolStripStatusLabel2.Text = ip;
        }


        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            //int i = richTextBox.SelectionStart;
            Console.WriteLine(i);
            //Console.WriteLine(richTextBox.Text.IndexOf(richTextBox.Text,i-1));
            Console.WriteLine(e.ToString());
            //processText();
            string text = richTextBox.Text;

            if (text == textRichTextBox)
                return;

            if (offlineMode == true)
                return;

            client.EmitAsync("message", richTextBox.Text);

            textRichTextBox = text;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new Сведения();
            about.ShowDialog();
        }

        private void подключитьсяКСерверуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ip = richTextBox.Text;

            Connect();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            offlineMode = true;

            ip = "http://127.0.0.1:3000/";

            Connect();
        }

        private void посмотретьСправкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/zoxione/Notechat-desktop");
        }

        private void оставитьОтзывToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/zoxione/Notechat-desktop/issues");
        }
    }
}