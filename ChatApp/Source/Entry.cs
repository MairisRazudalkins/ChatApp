//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System;
using System.Windows;
using User;
using Packets;

namespace ChatApp
{
    class Entry
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application application = new Application();
            MainWindow window = new MainWindow();
            
            window.Show();
            application.MainWindow = window;
            application.Run();

            //GameWindow gameWindow = new GameWindow();
            //gameWindow.Show();
            //
            //application.MainWindow = gameWindow;
            //application.Run();
        }
    }
}
