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
        string localText;
        string ip = "https://notechat-server.herokuapp.com/" ;
        bool offlineMode = false;
        //string room = "doc";

        public Main()
        {
            Form.CheckForIllegalCrossThreadCalls = false; // �������� ������� ��� ����������� ������ ����� �� �� ����� � ������ �������� ����� � RichTextBox, ����� ����� ���� ������� �� �����
                                                          // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls?view=netframeworkdesktop-4.8
            InitializeComponent();

            Connect("doc");
        }

        void Connect(string room = "doc") 
        {

            if(room != "doc")
                this.Text = room+" - �������";
            else
               this.Text = " �������";

            var options = new SocketIOOptions
            {
                Query = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("token", "abc123"), // todo
                    new KeyValuePair<string, string>("client", "notechatdesktop"),
                    new KeyValuePair<string, string>("room", room)
                }
            };

            client = new SocketIO(ip, options);

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
            updateToolStripStatusLabel3();
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
                if (offlineMode)
                    return;

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

        // TODO: ��������� ������ � ���������� � ���������� ����� ���������, ����� ���������� �� ������ �� ��� �����, � ������ ��������� �����
        void processText()
        {
            string oldText = textRichTextBox;
            string newText = richTextBox.Text;


        }

        // ����� � ���-��� ������
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
        // ����� � ������� �������
        void updateToolStripStatusLabel2()
        {
            if (offlineMode == true) 
            {
                toolStripStatusLabel2.Text = "offline";
                return;
            }
            
            toolStripStatusLabel2.Text = ip;
        }

        void updateToolStripStatusLabel3()
        {
            if (offlineMode == true)
                toolStripStatusLabel3.Text = "Offline";
            else
                toolStripStatusLabel3.Text = "";
        }


        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            //int i = richTextBox.SelectionStart;
            //Console.WriteLine(i);
            //Console.WriteLine(richTextBox.Text.IndexOf(richTextBox.Text,i-1));
            //Console.WriteLine(e.ToString());
            //processText();
            string text = richTextBox.Text;

            if (offlineMode == true)
            {
                localText = text;
                return;
            }

            if (text == textRichTextBox)
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

        private void ����������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new ��������();
            about.ShowDialog();
        }

        private void ��������������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ip = richTextBox.Text;
            Connect("doc");
        }

        private void �����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            offlineMode = true;
            client.DisconnectAsync();
            //ip = "http://127.0.0.1:3000/";

            //Connect();
        }

        private void �����������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/zoxione/Notechat-desktop");
        }

        private void �������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/zoxione/Notechat-desktop/issues");
        }

        private void �������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(client.Connected)
                client.DisconnectAsync();
            string room = richTextBox.Text;
            textRichTextBox = "";
            Connect(room);
        }

        private void ���������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client.DisconnectAsync();
            textRichTextBox = "";
            Connect("doc");
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.KeyCode == Keys.F1 )
            {
                if (offlineMode == false)
                {
                    
                    offlineMode = true;
                    richTextBox.Text = localText;
                }
                else
                {
                    localText = richTextBox.Text;
                    offlineMode = false;
                    richTextBox.Text = textRichTextBox;
                }
                updateToolStripStatusLabel3();
            }
               
        }
    }
}