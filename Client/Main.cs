using DiffMatchPatch;
using SocketIOClient;

namespace Client
{
    public partial class Main : Form
    {
        private Storage _clientStorage;             // client and client data
        private FontDialog _fontDialog;             // font dialog for textbox

        public Main()
        {
            Form.CheckForIllegalCrossThreadCalls = false;   // �������� ������� ��� ����������� ������ ����� �� �� ����� � ������ �������� ����� � textBox1, ����� ����� ���� ������� �� �����
                                                            // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls?view=netframeworkdesktop-4.8

            _clientStorage = new Storage();
            _fontDialog = new FontDialog();

            InitializeComponent();

            Connect("doc");
        }

        void Connect(string room = "doc") 
        {
            if(room != "doc")
                this.Text = room+" - �������";
            else
               this.Text = " �������";

            // Restore last sessionID from properties
            _clientStorage.Session.sessionID = Properties.Settings.Default.SessionID;

            var options = new SocketIOOptions
            {
                Query = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("room", room)
                },
                Auth = new Dictionary<string, string>
                {
                    {"sessionID", _clientStorage.Session.sessionID},
                    {"client", "notechatdesktop"},
                    {"appToken", "abc123"} // todo
                }
            };

            try
            {
                _clientStorage.Client = new SocketIO(_clientStorage.Ip, options);

                Thread ThreadConnect = new Thread(Listen);
                ThreadConnect.Start();
            }
            catch (Exception ex)
            {
                _clientStorage.OfflineMode = true;
                Console.WriteLine(ex.ToString());
            }

            while (true)
            {
                if (_clientStorage.Client.Attempts > 1 && _clientStorage.Client.Disconnected)
                {
                    _clientStorage.OfflineMode = true;
                    break;
                }
                else if (_clientStorage.Client.Connected)
                {
                    _clientStorage.OfflineMode = false;
                    break;
                }
            }

            updateToolStripStatusLabel2();
            updateToolStripStatusLabel3();
        }

        async private void Listen()
        {
            _clientStorage.Client.On("user_connected", response =>
            {
                Console.WriteLine(response);

                User u = response.GetValue<User>();
                _clientStorage.Users.Add(u);

                updateToolStripStatusLabel1();
            });

            _clientStorage.Client.On("session", response =>
            {
                _clientStorage.Session = response.GetValue<Session>();
                Properties.Settings.Default.SessionID = _clientStorage.Session.sessionID;
                // TODO: save user id and other data
                Properties.Settings.Default.Save();
            });

            _clientStorage.Client.On("users", response =>
            {
                Console.WriteLine(response);

                _clientStorage.Users = response.GetValue<List<User>>();

                updateToolStripStatusLabel1();
            });

            _clientStorage.Client.On("user_disconnected", response =>
            {
                Console.WriteLine(response);

                User u = response.GetValue<User>();
                _clientStorage.Users.RemoveAll(user => user.userID == u.userID);

                updateToolStripStatusLabel1();
            });

            _clientStorage.Client.On("past_history", response =>
            {
                Console.WriteLine(response);

                string text = response.GetValue<string>();

                _clientStorage.OldText = text;
                _clientStorage.CurrentText = text;
                textBox1.Text = text;
            });

            _clientStorage.Client.On("patch", response =>
            {
                if (_clientStorage.OfflineMode)
                    return;

                try 
                {
                    var text = response.GetValue<string?>();

                    var patch = DiffMatchPatchModule.Default.PatchFromText(text);
                    var arr = DiffMatchPatchModule.Default.PatchApply(patch, _clientStorage.OldText);
                    
                    _clientStorage.OldText = arr.First().ToString();
                    textBox1.Text = _clientStorage.OldText;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            _clientStorage.Client.OnConnected += async (sender, e) =>
            {
                // Emit a string
                // await client.EmitAsync("hi", "socket.io");

                // Emit a string and an object
                // var dto = new TestDTO { Id = 123, Name = "bob" };
                // await client.EmitAsync("register", "source", dto);
            };
            
            await _clientStorage.Client.ConnectAsync();
        }

        // ����� � ���-��� ������
        void updateToolStripStatusLabel1()
        {
            toolStripStatusLabel1.Text = _clientStorage.Users.Count.ToString();

            string temp = "";
            foreach (User u in _clientStorage.Users)
            {
                temp += (u.userID + '\n');
            }

            toolStripStatusLabel1.ToolTipText = temp;
        }

        // ����� � ������� �������
        void updateToolStripStatusLabel2()
        {
            if (_clientStorage.OfflineMode == true) 
            {
                toolStripStatusLabel2.Text = "offline";
                return;
            }
            
            toolStripStatusLabel2.Text = _clientStorage.Ip;
        }

        void updateToolStripStatusLabel3()
        {
            if (_clientStorage.OfflineMode == true)
                toolStripStatusLabel3.Text = "Offline";
            else
                toolStripStatusLabel3.Text = "";
        }

        // ��������� ��� ��������� ���������� �� 1 ������
        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            //int i = textBox1.SelectionStart;
            //Console.WriteLine(i);
            //Console.WriteLine(textBox1.Text.IndexOf(textBox1.Text,i-1));
            //Console.WriteLine(e.ToString());
            //processText();

            string text = textBox1.Text;

            if (_clientStorage.OfflineMode == true)
            {
                _clientStorage.CurrentText = text;
                return;
            }

            if (text == _clientStorage.OldText)
                return;

            _clientStorage.CurrentText = text;

                    // ������������ � �������� �����
            var dmp = DiffMatchPatchModule.Default;
            var diffs = dmp.DiffMain(_clientStorage.OldText, _clientStorage.CurrentText);
            var patchs = dmp.PatchMake(_clientStorage.OldText, diffs);
            var tt = dmp.PatchToText(patchs);
            _clientStorage.Client.EmitAsync("patch", tt);

            _clientStorage.OldText = _clientStorage.CurrentText;
        }

        /*
         * ���� ����� �������
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
       
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }
        */

        private void ����������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new ��������();
            about.ShowDialog();
        }

        private void ��������������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _clientStorage.Ip = textBox1.Text;
                Connect("doc");
            }
            catch (Exception)
            {
                // TODO: ���������� ����� ������
            }
            
        }

        private void �����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _clientStorage.OfflineMode = true;
            _clientStorage.Client.DisconnectAsync();
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
            if(_clientStorage.Client.Connected)
                _clientStorage.Client.DisconnectAsync();
            string room = textBox1.Text;
            _clientStorage.OldText = "";
            Connect(room);
        }

        private void ���������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _clientStorage.Client.DisconnectAsync();
            _clientStorage.OldText = "";
            Connect("doc");
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.KeyCode == Keys.F1 )
            {
                if (_clientStorage.OfflineMode == false)
                {

                    _clientStorage.OfflineMode = true;
                    textBox1.Text = _clientStorage.CurrentText;
                }
                else
                {
                    _clientStorage.CurrentText = textBox1.Text;
                    _clientStorage.OfflineMode = false;
                    textBox1.Text = _clientStorage.OldText;
                }
                updateToolStripStatusLabel3();
            }
               
        }

                // ������� ������
        private void �����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fontDialog.ShowDialog() == DialogResult.OK)
            { 
               textBox1.Font = _fontDialog.Font;
            }
        }

        private void ���������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.WordWrap = !textBox1.WordWrap;
            ���������������ToolStripMenuItem.Checked = textBox1.WordWrap;
        }

                // ������� ���
        private void ���������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = !statusStrip1.Visible;
            ���������������ToolStripMenuItem.Checked = statusStrip1.Visible;
        }
    }
}