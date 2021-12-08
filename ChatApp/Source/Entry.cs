//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System;
using User;
using Packets;

namespace ChatApp
{
    class Entry
    {
        [STAThread]
        static void Main(string[] args)
        {
            Client client = Client.GetInst();
            client.Connect("127.0.0.1", 4444);

            client.CreateAccount(new LoginDetails("mairis", "password"), new UserInfo("Mairis"));

            Console.Read();
        }
    }
}
