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
using System.Windows.Threading;
using User;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class MessageUi : UserControl
    {
        public MessageUi()
        {
            InitializeComponent();
        }

        public MessageUi(Message msg)
        {
            InitializeComponent();
            SetMsgProperties(msg);
        }

        public MessageUi(ImgMessage msg)
        {
            InitializeComponent();
            SetMsgProperties(msg);

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                if (msg.imgData != null)
                {
                    HorizontalAlignment alignment = Client.GetInst().GetInfo().uniqueId == msg.senderId ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                    BitmapImage img = FileImporter.CacheImage(msg.imgData);
                    float widthScale = (float)(img.Width / 1920);
                    float padding = 1f - (50 * (1 - widthScale) + 500 * widthScale);

                    Img.Source = img;
                    Img.HorizontalAlignment = alignment;
                    Img.Margin = Client.GetInst().GetInfo().uniqueId == msg.senderId ? new Thickness(padding, 0, 0, 5) : new Thickness(0, 0, padding, 5);
                }
            });
        }

        private void SetMsgProperties(Message msg)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                HorizontalAlignment alignment = Client.GetInst().GetInfo().uniqueId == msg.senderId ? HorizontalAlignment.Right : HorizontalAlignment.Left;

                MessageText.Text = msg.msg;
                SenderName.Text = msg.senderName;

                SenderName.HorizontalAlignment = alignment;
                MessageBorder.HorizontalAlignment = alignment;
                MessageBorder.Margin = Client.GetInst().GetInfo().uniqueId == msg.senderId ? new Thickness(50, 0, 0, 0) : new Thickness(0, 0, 50, 0);
                MessageBorder.CornerRadius = Client.GetInst().GetInfo().uniqueId == msg.senderId ? new CornerRadius(15, 15, 0, 15) : new CornerRadius(15, 15, 15, 0);
            });
        }
    }
}
