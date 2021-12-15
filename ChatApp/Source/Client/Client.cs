using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Packets;
using User;

namespace ChatApp
{
    public class Client
    {
        private static Client inst;
        public static Client GetInst() { return inst == null ? inst = new Client() : inst; }

        private TcpClient tpcClient;
        private NetworkStream netStream;

        private BinaryWriter writer;
        private BinaryReader reader;
        private BinaryFormatter formatter;

        private Thread readThread; // Needs to be global var to abort when disconnect happens to fix () Error

        private UserInfo userInfo;

        private Client() { }

        public UserInfo GetInfo() { return userInfo; }

        public bool IsConnected() { return tpcClient.Client.Connected && !(tpcClient.Client.Poll(0, SelectMode.SelectRead) && tpcClient.Client.Available == 0); }

        public bool Connect(string ip, int port)
        {
            try
            {
                tpcClient = new TcpClient();
                tpcClient.Connect(ip, port);

                Initialize();
                OnConnected();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private void Initialize()
        {
            netStream = tpcClient.GetStream();
            reader = new BinaryReader(netStream, Encoding.UTF8);
            writer = new BinaryWriter(netStream, Encoding.UTF8);
            formatter = new BinaryFormatter();
        }

        public void Disconnect()
        {
            if (tpcClient == null)
                return;

            if (IsConnected())
            {
                readThread.Abort();

                tpcClient.Close();
                tpcClient.Dispose();

                netStream.Dispose();

                writer.Dispose();
                reader.Dispose();
            }

            tpcClient = null;
        }

        public void CreateAccount(LoginDetails newDetails, UserInfo info, Action<Packet> funcCallback)
        {
            NetCallback callback = new NetCallback(funcCallback, new Random().Next(), 15f);
            SendPacket(new CreateAccPacket(newDetails, info, callback.GetPacketId()));
        }

        public void Login(LoginDetails details, Action<Packet> funcCallback)
        {
            NetCallback callback = new NetCallback(funcCallback, new Random().Next(), 15f);
            SendPacket(new LoginPacket(details, callback.GetPacketId()));
        }

        public void OnLogin(UserInfo info)
        {
            this.userInfo = info;

            // load contacts...
        }

        private void Run()
        {
            readThread = new Thread(() =>
            {
                while (IsConnected())
                    ReadPacket();

                Disconnect();

                Console.WriteLine("Lost connection");
            });

            readThread.Start();
        }

        public void ReadPacket()
        {
            if (tpcClient != null)
            {
                if (tpcClient.Client.Connected && tpcClient.Client.Available != 0)
                {
                    int packetSize = -1;

                    if ((packetSize = reader.ReadInt32()) != -1)
                        OnRecievePacket(formatter.Deserialize(new MemoryStream(reader.ReadBytes(packetSize))) as Packet);
                }
            }
        }

        public void SendPacket(Packet packet)
        {
            if (IsConnected())
            {
                MemoryStream stream = new MemoryStream();

                formatter.Serialize(stream, packet);

                byte[] buffer = stream.GetBuffer();

                writer.Write(buffer.Length);
                writer.Write(buffer);
                writer.Flush();
            }
        }

        private void OnConnected()
        {
            Console.WriteLine("Connected successfully");

            Run();
        }

        private void OnRecievePacket(Packet packet)
        {
            // TODO: Handle packets

            if (!NetCallback.OnRecievedPacket(packet))
            {
                switch (packet?.PacketCategory)
                {
                    case PacketCategory.Message:
                        HandleMsgPacket(packet);
                        break;
                    case PacketCategory.UserInfo:
                        HandleUserPacket(packet);
                        break;
                }
            }
        }

        private void HandleMsgPacket(Packet packet)
        {
            MsgPacket msgPacket = (MsgPacket)packet;

            Contact contact = Contact.FindContact(packet.SenderID);

            if (contact != null)
            {
                if (msgPacket.Msg.senderId != Client.GetInst().userInfo.uniqueId)
                    contact.AddMessage(msgPacket.Msg);

                //switch (msgPacket?.MsgCategory)
                //{
                //    case MessageType.Msg:
                //
                //        if (msgPacket.Msg.senderId != Client.GetInst().userInfo.uniqueId)
                //            contact.AddMessage(msgPacket.Msg);
                //        break;
                //    case MessageType.Image:
                //
                //        ImgMsgPacket imgPacket = (ImgMsgPacket)packet;
                //
                //        if (imgPacket.Msg.senderId != Client.GetInst().userInfo.uniqueId)
                //            contact.AddMessage(msgPacket.Msg);
                //
                //        break;
                //    case MessageType.File:
                //
                //        break;
                //}
            }
        }

        private void HandleUserPacket(Packet packet)
        {
            switch ((packet as UserPacket)?.UserPacketType)
            {
                case UserPacketType.LoginResult:
                    LoginResultPacket result = (LoginResultPacket)packet;

                    if (result?.UserInfo != null)
                    {
                        this.userInfo = result.UserInfo;
                        Console.WriteLine(string.Format("Logged in: {0} {1} {2}", userInfo.name, userInfo.uniqueId, userInfo.image));
                    }
                    else
                    {
                        Console.WriteLine("Failed to Log in");
                    }
                    break;
                case UserPacketType.NameChange:

                    break;
                case UserPacketType.ImageChange:

                    ChangeImagePacket changeImage = (ChangeImagePacket)packet;

                    Contact.FindContact(changeImage.SenderID).UpdateImage(changeImage.ImgData);

                    break;
                case UserPacketType.Contact:
                    ContactPacket contact = (ContactPacket)packet;

                    if (contact != null)
                        Contact.AddContact(new Contact(contact.UserInfo));

                    break;
                case UserPacketType.Disconnect:
                    DisconnectPacket dcPacket = (DisconnectPacket)packet;

                    if (dcPacket != null)
                    {
                       Contact.RemoveContact(dcPacket.SenderID); 
                    }

                    break;
            }
        }

        //public static BitmapImage ImportImg(ref string imgPath)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "jpg (*.jpg)|*.jpg";
        //    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //
        //    if (openFileDialog.ShowDialog() == true)
        //        return CacheImage((imgPath = openFileDialog.FileName));
        //
        //    return null;
        //}
        //
        //public static BitmapImage CacheImage(string imgPath)
        //{
        //    BitmapImage img = new BitmapImage();
        //
        //    img.BeginInit();
        //    img.CacheOption = BitmapCacheOption.OnLoad;
        //    img.UriSource = new Uri(imgPath);
        //    img.EndInit();
        //
        //    return img;
        //}

        //public static BitmapImage CacheImage(byte[] imgData)
        //{
        //    if (imgData == null)
        //        return null;
        //
        //    BitmapImage img = new BitmapImage();
        //
        //    using (MemoryStream stream = new MemoryStream(imgData))
        //    {
        //        img.BeginInit();
        //        img.CacheOption = BitmapCacheOption.OnLoad;
        //        img.StreamSource = stream;
        //        img.EndInit();
        //    }
        //
        //    return img;
        //}

        public void ChangeImage(byte[] data)
        {
            SendPacket(new ChangeImagePacket((this.userInfo.image = data)));
        }
    }
}
