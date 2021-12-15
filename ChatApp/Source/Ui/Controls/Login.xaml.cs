using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Packets;
using User;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ProfileSetup.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private MainWindow window;

        public Login(MainWindow window)
        {
            InitializeComponent();

            this.window = window;

            //string profilePicPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Images\\ProfilePic.jpg";
            //
            //if (File.Exists(profilePicPath))
            //{
            //    BitmapImage img = new BitmapImage();
            //
            //    img.BeginInit();
            //    img.CacheOption = BitmapCacheOption.OnLoad;
            //    img.UriSource = new Uri(profilePicPath);
            //    img.EndInit();
            //
            //    ProfilePic.ImageSource = img;
            //}
        }

        private void LoginPressed(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), "");

            Client.GetInst().Login(new LoginDetails(NameText.Text, Password.Text), OnLogin);
        }

        private void CreateAccountPressed(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<Page>(window.ChangePage), Page.ProfileSetup);
        }

        private void OnLogin(Packet packet)
        {
            if (packet == null)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), "Connection error(Request Timeout)");
                return;
            }

            LoginResultPacket result = (LoginResultPacket)packet;

            if (result.UserInfo != null)
            {
                Client.GetInst().OnLogin(result.UserInfo);

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<Page>(window.ChangePage), Page.Messenger);
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), result.ResultMsg);
            }
        }

        private void UpdateResultMsg(string msg)
        {
            ResultMsg.Text = msg;
        }
    }
}
