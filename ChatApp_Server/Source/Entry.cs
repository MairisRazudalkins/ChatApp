using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User;

namespace Server
{
    class Entry
    {
        private static void Main(string[] args)
        {
            new User(new LoginDetails("t1", "hauhwdw"), new UserInfo("Mairis")).SaveData();

            Server server = new Server("127.0.0.1", 4444);
            server.Start();
        }
    }
}
