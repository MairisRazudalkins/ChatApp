//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System;
using User;

namespace ChatApp
{
    class Entry
    {
        [STAThread]
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Connect("127.0.0.1", 4444);

            Console.WriteLine("Enter user name");
            string userName = Console.ReadLine();

            Console.WriteLine("Enter user password");
            string password = Console.ReadLine();

            client.Login(new LoginDetails(userName, password));

            Console.Read();
        }
    }
}
