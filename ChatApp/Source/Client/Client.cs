using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Packets;
using User;

namespace ChatApp
{
    public class Client
    {
        private static Client inst;
        public static Client GetInst() { return inst == null ? inst = new Client() : inst; }

        private UdpClient udpClient;
        private TcpClient tpcClient;
        private NetworkStream netStream;

        private BinaryWriter writer;
        private BinaryReader reader;
        private BinaryFormatter formatter;

        private Thread readThread, udp_readThread; // Needs to be global var to abort when disconnect happens to fix () Error
        
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

                UDP_Connect(ip, port);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void UDP_Connect(string ip, int port) 
        {
            try
            {
                udpClient = new UdpClient();
                udpClient.Connect(ip, port);

                Thread udpThread = new Thread(new ThreadStart(UDP_ReadPacket));
                udpThread.Start();

                UDP_Login();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
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
            if (readThread != null)
                readThread.Abort();

            if (udp_readThread != null)
                udp_readThread.Abort();

            tpcClient.Close();
            tpcClient.Dispose();

            netStream.Dispose();

            writer.Dispose();
            reader.Dispose();

            udpClient.Close();

            udpClient = null;
            tpcClient = null;
        }

        public void CreateAccount(LoginDetails newDetails, UserInfo info, Action<Packet> funcCallback)
        {
            NetCallback callback = new NetCallback(funcCallback, new Random().Next(), 15f);
            TCP_SendPacket(new CreateAccPacket(newDetails, info, callback.GetPacketId()));
        }

        public void Login(LoginDetails details, Action<Packet> funcCallback)
        {
            NetCallback callback = new NetCallback(funcCallback, new Random().Next(), 15f);
            TCP_SendPacket(new LoginPacket(details, callback.GetPacketId()));
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
                    TCP_ReadPacket();

                Console.WriteLine("Lost connection");
            });

            udp_readThread = new Thread(() =>
            {
                UDP_ReadPacket();
            });

            readThread.Start();
            udp_readThread.Start();
        }

        private void TCP_ReadPacket()
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

        public void TCP_SendPacket(Packet packet)
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
                        TCP_HandleMsgPacket(packet);
                        break;
                    case PacketCategory.UserInfo:
                        TCP_HandleUserPacket(packet);
                        break;
                    case PacketCategory.UDP_PositionUpdate:
                        UDP_PositionPacket posPacket = (UDP_PositionPacket)packet;
                        GameWindow.OnReceivePosUpdate(packet.SenderID, new Vector2(posPacket.X, posPacket.Y), posPacket.R, posPacket.G, posPacket.B);
                        break;
                }
            }
        }

        private void TCP_HandleMsgPacket(Packet packet)
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

        private void TCP_HandleUserPacket(Packet packet)
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
                        Contact.RemoveContact(dcPacket.SenderID);

                    break;
            }
        }

        private void UDP_Login() 
        {
            TCP_SendPacket(new UDP_LoginPacket((IPEndPoint)udpClient.Client.LocalEndPoint));
        }

        private void UDP_ReadPacket()
        {
            try
            {
                IPEndPoint defaultIp = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] buffer = udpClient.Receive(ref defaultIp);

                    if (buffer != null)
                        OnRecievePacket(formatter.Deserialize(new MemoryStream(buffer)) as Packet);
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("UDP ERROR: " + e.Message);
            }
        }

        public void UDP_SendPacket(Packet packet) 
        {
            if (IsConnected())
            {
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, packet);

                byte[] buffer = stream.GetBuffer();

                udpClient.Send(buffer, buffer.Length);
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
            TCP_SendPacket(new ChangeImagePacket((this.userInfo.image = data)));
        }
    }
}
