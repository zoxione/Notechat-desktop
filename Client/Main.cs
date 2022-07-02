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

            Connect(_clientStorage.Ip, _clientStorage.Room);

            updateToolStripStatusLabel2();
            updateToolStripStatusLabel3();


            //Properties.Settings.Default.IsWelcomeHide = true;
            //Properties.Settings.Default.Save();

            if (Properties.Settings.Default.IsWelcomeHide == false) 
            {
                var welcome = new Welcome();
                welcome.Show();
                welcome.StartPosition = FormStartPosition.CenterParent;
            }
        }

        void Connect(string server, string room)
        {
                    // ���� ���� ����������� -> ���������
            if (_clientStorage.Client != null && _clientStorage.Client.Connected)
            {
                _clientStorage.Client.DisconnectAsync();
            }

                    // ���������� ������
            _clientStorage.Ip = server;
            _clientStorage.Room = room;

                    // �������� �����
            if (_clientStorage.Room == "")
            {
                this.Text = "�������";
            }
            else 
            {
                this.Text = _clientStorage.Room + " - �������";
            }

                    // Restore last sessionID from properties
            _clientStorage.Session.sessionID = Properties.Settings.Default.SessionID;

                    // ��������� �����������
            var options = new SocketIOOptions
            {
                Query = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("room", _clientStorage.Room)
                },
                Auth = new Dictionary<string, string>
                {
                    {"sessionID", _clientStorage.Session.sessionID},
                    {"client", "notechatdesktop"},
                    {"appToken", "abc123"} // todo
                }
            };

                    // �������� ������� � ��� �����������
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

                    // ���� �������� �����������
            while (true)
            {
                if (_clientStorage.Client.Attempts >= 1 && _clientStorage.Client.Disconnected)
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

                    // ���������� �������
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
            if (_clientStorage.Ip == "")
            {
                toolStripStatusLabel2.Text = "offline";
            }
            else
            {
                toolStripStatusLabel2.Text = _clientStorage.Ip;
            }
        }

                // ����� � ��������� �������
        void updateToolStripStatusLabel3()
        {
            if (_clientStorage.OfflineMode == true)
            {
                toolStripStatusLabel3.Text = "";
            }
            else
            {
                toolStripStatusLabel3.Text = _clientStorage.Room;
            }  
        }

                // ��������� ��� ��������� ���������� �� 1 ������
        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            string text = textBox1.Text;

            if (_clientStorage.OfflineMode == true)
            {
                _clientStorage.CurrentText = text;
                return;
            }

            if (_clientStorage.OldText == text)
            {
                return;
            }

            if (_clientStorage.OldText == null)
            {
                _clientStorage.OldText = "";
            }

            _clientStorage.CurrentText = text;

                    // ������������ � �������� �����
            var diffs = DiffMatchPatchModule.Default.DiffMain(_clientStorage.OldText, _clientStorage.CurrentText);
            var patchs = DiffMatchPatchModule.Default.PatchMake(_clientStorage.OldText, diffs);
            var tt = DiffMatchPatchModule.Default.PatchToText(patchs);
            _clientStorage.Client.EmitAsync("patch", tt);

            _clientStorage.OldText = _clientStorage.CurrentText;
        }

                // ������������ � �������
        private void ��������������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var connectServer = new ConnectServer(_clientStorage, Connect, updateToolStripStatusLabel2);
            connectServer.ShowDialog();
        }

                // ����������� �� �������
        private void �����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _clientStorage.OfflineMode = true;
            _clientStorage.Client.DisconnectAsync();

            //updateToolStripStatusLabel1();

            _clientStorage.Ip = "";
            updateToolStripStatusLabel2();
            
            _clientStorage.Room = "";
            updateToolStripStatusLabel3();
            
            this.Text = "�������";
            textBox1.Text = "";
        }

                // ������� ����
        private void �������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var createRoom = new CreateRoom(_clientStorage, Connect);
            createRoom.ShowDialog();
        }

                // ������� ����
        private void ���������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clientStorage.Room != "")
            {
                _clientStorage.OfflineMode = true;
                _clientStorage.Client.DisconnectAsync();

                updateToolStripStatusLabel1();

                _clientStorage.Room = "";
                updateToolStripStatusLabel3();

                this.Text = "�������";
                textBox1.Text = "";

                Connect(_clientStorage.Ip, _clientStorage.Room);
            }
        }

        private void ����������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new ��������();
            about.ShowDialog();
        }

        private void �����������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/zoxione/Notechat-desktop");
        }

        private void �������������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/zoxione/Notechat-desktop/issues");
        }

                // F1 ���
        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.F1)
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
                //updateToolStripStatusLabel3();
            }
            */
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
       
        
                // ������� ������
        private void ����������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dt = new DateTime();
            textBox1.Text += dt;
        }

        private void �����������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Focus();
        }
    }
}