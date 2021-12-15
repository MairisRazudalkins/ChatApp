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
    public partial class ProfileSetup : UserControl
    {
        private MainWindow window;
        private UserInfo info;
        private byte[] image;

        public ProfileSetup(MainWindow window)
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

        private void ChangeImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPEG (*.jpg *.jpeg)|*.jpg;*.jpeg;";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            if (openFileDialog.ShowDialog() == true)
            {
                string imgPath = openFileDialog.FileName;
            
                BitmapImage img = new BitmapImage();
            
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(imgPath);
                img.EndInit();
            
                ProfilePic.ImageSource = img;

                image = File.ReadAllBytes(imgPath);
            }
        }

        private void CreateProfileClick(object sender, RoutedEventArgs e)
        {
            if (NameText.Text.Length < 4)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), "User name is less than 4 characters");
                return;
            }

            if (Password.Text.Length < 4)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), "Password is less than 4 characters");
                return;
            }

            if (String.IsNullOrEmpty(DisplayName.Text))
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), "Invalid display name");
                return;
            }

            info = new UserInfo(DisplayName.Text, image);
            Client.GetInst().CreateAccount(new LoginDetails(NameText.Text, Password.Text), info, OnCreateProfileResult);
        }

        private void OnCreateProfileResult(Packet packet)
        {
            if (packet == null)
                return;

            CreateAccResultPacket result = (CreateAccResultPacket)packet;

            if (result.Succeeded)
            {
                Client.GetInst().OnLogin(info);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<Page>(window.ChangePage), Page.Messenger);
            }
            else
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(UpdateResultMsg), result.ResultMsg);
        }

        private void UpdateResultMsg(string msg)
        {
            ResultMsg.Text = msg;
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<Page>(window.ChangePage), Page.Login);
        }
    }
}
