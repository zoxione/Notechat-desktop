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
        public string Ip { get; set; } = "http://26.23.217.120:3000/";
        public bool OfflineMode { get; set; } = false;

            // text data
        public string OldText { get; set; } = "";
        public string CurrentText { get; set; } = "";
        public List<Patch> Patches { get; set; }
    }
}
