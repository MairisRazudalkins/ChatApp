using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public enum Page
    {
        Connect,
        Messenger,
        Login,
        ProfileSetup,
        Settings,
        None
    }

    public partial class MainWindow : Window
    {
        private Page currentPage = Page.None;

        public MainWindow()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            if (Client.GetInst().Connect("127.0.0.1", 4444))
            {
                ChangePage(Page.Login);
            }
            else
            {
                ChangePage(Page.Connect);
            }
            // add some loading screen if not connected and try again
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Client.GetInst().Disconnect();
            Environment.Exit(0);
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void ChangePage(Page pageType)
        {
            if (currentPage == pageType)
                return;

            Dispatcher.Invoke(() =>
            {
                Content.Child = null;

                switch (pageType)
                {
                    case Page.Connect:
                        Content.Child = new ConnectionUi();
                        break;
                    case Page.ProfileSetup:
                        Content.Child = new ProfileSetup(this);
                        break;
                    case Page.Settings:

                        break;
                    case Page.Messenger:
                        Content.Child = Messenger.GetInst();
                        break;
                    case Page.Login:
                        Content.Child = new Login(this);
                        break;
                }

                currentPage = pageType;
            });
        }

        private void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Client.GetInst().Disconnect();
                Close();
            }
        }

        private void OnMaximizeClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnMinimizeClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                WindowState = System.Windows.WindowState.Minimized;
        }
    }
}