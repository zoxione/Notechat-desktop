using DiffMatchPatch;
using SocketIOClient;

namespace Client
{
    public class Storage
    {
            // main data
        public SocketIO Client { get; set; }
        public List<User> Users { get; set; }
        public Session Session { get; set; } = new Session();

        // secondary data
        //public string Ip { get; set; } = "https://notechat-server.herokuapp.com/";
        //public string Ip { get; set; } = "http://localhost:3000/";
        //public string Ip { get; set; } = "http://26.23.217.120:3000/";
        public string Ip { get; set; } = Properties.Settings.Default.Ip;
        public string Room { get; set; } = "";
        public bool OfflineMode { get; set; } = true;

            // text data
        public string OldText { get; set; } = "";
        public string CurrentText { get; set; } = "";
        public List<Patch> Patches { get; set; }
    }
}
