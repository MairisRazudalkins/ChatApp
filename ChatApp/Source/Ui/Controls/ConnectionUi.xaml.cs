using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ConnectionUi.xaml
    /// </summary>
    public partial class ConnectionUi : UserControl
    {
        //private MainWindow window;

        private string ip = "127.0.0.1";
        private int port = 4444;
        private int failedAttempts = 0;

        public ConnectionUi()
        {
            InitializeComponent();
        }

        private void TryConnect()
        {
            if (Client.GetInst().Connect(ip, port))
            {
                (Application.Current.MainWindow as ChatApp.MainWindow)?.ChangePage(Page.Login);
                return;
            }

            OnFailedConnect();
        }

        private void OnFailedConnect()
        {
            Dispatcher.Invoke(() =>
            {
                InputGrid.Visibility = Visibility.Visible;
                InfoText.Text = string.Format("Failed to connect! ({0})", failedAttempts++);
            });
        }

        private void OnConnectClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                if (!String.IsNullOrEmpty(IpInputField.Text) && !String.IsNullOrEmpty(PortInputField.Text))
                {
                    InputGrid.Visibility = Visibility.Hidden;
                    InfoText.Text = "Connecting...";
                    
                    ip = IpInputField.Text;
                    int.TryParse(PortInputField.Text, out port);

                    TryConnect();
                    return;
                }

                OnFailedConnect();
            });
        }
    }
}
