using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using User;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for Contact.xaml
    /// </summary>
    public partial class ContactUi : UserControl
    {
        private Messenger msgr;
        private Contact contact;

        public ContactUi(Messenger msgr, Contact contact)
        {
            this.msgr = msgr;
            this.contact = contact;

            InitializeComponent();

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                ChangeImg(contact.GetInfo().image);
                ContactName.Text = contact.GetInfo().name;
            });
        }

        public void ChangeImg(byte[] img)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                if (img == null)
                {
                    ProfilePic.ImageSource = FileImporter.CacheImage(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Source\\Ui\\Image\\DefaultProfilePic.jpg");
                    return;
                }

                if (ProfilePic != null)
                    ProfilePic.ImageSource = FileImporter.CacheImage(img);
            });
        }

        private void OnContactClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            msgr.SetCurrentContact(contact);
        }
    }
}
