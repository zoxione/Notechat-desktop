namespace Client
{
    public partial class CreateRoom : Form
    {
        private Action<string, string> _connect;
        private Storage _clientStorage;

        public CreateRoom(Storage _cs, Action<string, string> con)
        {
            this._clientStorage = _cs;
            this._connect = con;

            InitializeComponent();
            textBox1.Text = _clientStorage.Room;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_clientStorage.OfflineMode)
                {
                    _connect(_clientStorage.Ip, textBox1.Text);
                    this.Close();
                }
                else 
                {
                    textBox1.Text = "Необходим сервер";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally 
            {
                
            }
        }
    }
}
