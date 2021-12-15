using System;
using System.Collections.Generic;
using System.IO;
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
using Packets;
using User;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for Messenger.xaml
    /// </summary>
    public partial class Messenger : UserControl
    {
        private static Messenger inst = null;

        private static Dictionary<Contact, ContactUi> contactDictionary = new Dictionary<Contact, ContactUi>();
        private BitmapImage defaultImageCache;
        private Contact curOpenContact;
        private byte[] pendingImgData;

        public static Messenger GetInst() { return inst != null ? inst : inst = new Messenger(); }

        private Messenger()
        {
            inst = this;
            Contact.AssignCallback(UpdateContact);

            InitializeComponent();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<UserInfo>(SetupProfile), Client.GetInst().GetInfo());

            //UserNameText.Text = Client.GetInst().userInfo.GetName();
            //BitmapImage userImg = UserInfo.BytesToImage(Client.GetInst().userInfo.GetImageBytes());

            //ProfilePic.ImageSource = userImg != null ? userImg : ); TODO: Load default image if loaded image is null;
        }

        public void SetCurrentContact(Contact contact)
        {
            if (curOpenContact == contact)
                return;

            curOpenContact = contact;

            ClearCurMsg();

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                MessageInputField.Visibility = Visibility.Visible;
                CurContactGrid.Visibility = Visibility.Visible;
                MessageScroller.ScrollToBottom();
                CurContactNameText.Text = contact.GetInfo().name;
                MessageGrid.Visibility = Visibility.Visible;

                if (contact.GetInfo().image != null)
                    CurContactProfilePic.ImageSource = FileImporter.CacheImage(contact.GetInfo().image);
                else
                    CurContactProfilePic.ImageSource = FileImporter.CacheImage(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Source\\Ui\\Image\\DefaultProfilePic.jpg");

                //foreach (Message msg in contact.GetLatestMessages())
                //    MessageStack.Children.Add(new MessageUi(new Message(msg.senderName, msg.senderId, msg.msg)));

                foreach (Message msg in contact.GetMessages())
                {
                    if (msg.msgType == MsgType.Img)
                        MessageStack.Children.Add(new MessageUi(new ImgMessage(msg.senderName, msg.senderId, msg.msg, (msg as ImgMessage)?.imgData)));
                    else
                        MessageStack.Children.Add(new MessageUi(new Message(msg.senderName, msg.senderId, msg.msg)));
                }
            });
        }

        private void SetupProfile(UserInfo info)
        {
            UserNameText.Text = info.name;
            ChangeImage(info.image);
        }

        private void ChangeImage(byte[] imgData)
        {
            if (imgData != null)
            {
                ProfilePic.ImageSource = FileImporter.CacheImage(imgData);
            }
        }

        private void OnChangeProfilePicClicked(object sender, RoutedEventArgs e)
        {
            Client client = Client.GetInst();

            FileImporter.ImportFile(FileType.Image, out string imgPath);
            BitmapImage img = FileImporter.CacheImage(imgPath);
            
            if (!string.IsNullOrEmpty(imgPath) && img != null)
            {
                ProfilePic.ImageSource = img;
                client.ChangeImage(File.ReadAllBytes(imgPath));
            }
        }

        private void UpdateContact(bool bShouldAdd, Contact contact)
        {
            if (bShouldAdd)
                AddContact(contact);
            else
                RemoveContact(contact);
        }

        public void AddContact(Contact contact)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                contactDictionary.Add(contact, new ContactUi(this, contact));
                ContactStack.Children.Add(contactDictionary[contact]);
            });
        }

        public void RemoveContact(Contact contact)
        {
            if (curOpenContact == contact)
                ClearCurMsg();

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                ContactStack.Children.Remove(contactDictionary[contact]);
                contactDictionary.Remove(contact);
            });
        }

        private void SendMessage(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !String.IsNullOrEmpty(MessageInputField.Text) && curOpenContact != null)
            {
                if (pendingImgData != null)
                {
                    ImgMessage imgMsg = new ImgMessage(Client.GetInst().GetInfo().name, Client.GetInst().GetInfo().uniqueId, MessageInputField.Text, pendingImgData);
                    curOpenContact.SendImgMessage(imgMsg);
                    pendingImgData = null;

                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                    {
                        MessageStack.Children.Add(new MessageUi(imgMsg));
                        ImgAddedIndicator.Visibility = Visibility.Hidden;
                        MessageScroller.ScrollToBottom();
                        MessageInputField.Text = "";
                    });
                }
                else
                {
                    Message msg = new Message(Client.GetInst().GetInfo().name, Client.GetInst().GetInfo().uniqueId, MessageInputField.Text);
                    curOpenContact.SendMessage(msg);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                    {
                        MessageStack.Children.Add(new MessageUi(msg));
                        MessageScroller.ScrollToBottom();
                        MessageInputField.Text = "";
                    });
                }

                //Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                //{
                //
                //    curOpenContact.SendMessage(msg);
                //    MessageStack.Children.Add(new MessageUi(msg));
                //    MessageInputField.Text = "";
                //});
            }
        }

        public void AddImgMessage(int senderId, ImgMessage msg)
        {
            if (curOpenContact == null)
                return;

            if (curOpenContact.GetInfo().uniqueId == senderId)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    MessageStack.Children.Add(new MessageUi(msg));
                    MessageScroller.ScrollToBottom();
                });
            }
        }

        public void AddMessage(int senderId, Message msg)
        {
            if (curOpenContact == null)
                return;

            if (curOpenContact.GetInfo().uniqueId == senderId)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    if (msg.msgType == MsgType.Img)
                        MessageStack.Children.Add(new MessageUi((ImgMessage)msg));
                    else
                        MessageStack.Children.Add(new MessageUi(msg));

                    MessageScroller.ScrollToBottom();
                });
            }
        }

        public void UpdateContactImage(Contact contact, byte[] img)
        {
            if (contact != null)
                if (contactDictionary.ContainsKey(contact))
                    contactDictionary[contact].ChangeImg(img);
        }

        private void ClearCurMsg()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                CurContactGrid.Visibility = Visibility.Hidden;
                MessageInputField.Visibility = Visibility.Hidden;
                MessageStack.Children.Clear();
            });
        }

        private void OnAddFileClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                FileImporter.ImportFile(FileType.Image, out string imgPath);

                if (!string.IsNullOrEmpty(imgPath))
                {
                    pendingImgData = File.ReadAllBytes(imgPath);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                    {
                        ImgAddedIndicator.Visibility = Visibility.Visible;
                    });
                }
            }
        }

        private void DeletePendingImg(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                pendingImgData = null;

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    ImgAddedIndicator.Visibility = Visibility.Hidden;
                });
            }
        }
    }
}
