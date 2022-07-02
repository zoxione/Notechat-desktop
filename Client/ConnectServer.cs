namespace Client
{
    public partial class ConnectServer : Form
    {
        private Action<string, string> _connect;
        private Storage _clientStorage;
        private Action _updateToolStripStatusLabel2;

        public ConnectServer(Storage _cs, Action<string, string> con, Action updTSS2)
        {
            this._clientStorage = _cs;
            this._connect = con;
            this._updateToolStripStatusLabel2 = updTSS2;

            InitializeComponent();
            textBox1.Text = _clientStorage.Ip;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _connect(textBox1.Text, _clientStorage.Room);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                this.Close();
            }
        }
    }
}
