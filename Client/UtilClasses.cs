using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class User
    {
        public string userID { get; set; }

    }

    public class Session
    {
        public string sessionID { get; set; }
        public string userID { get; set; }
    }
}
