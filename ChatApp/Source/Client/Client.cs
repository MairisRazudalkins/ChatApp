using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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

        private TcpClient tpcClient;
        private NetworkStream netStream;

        private BinaryWriter writer;
        private BinaryReader reader;
        private BinaryFormatter formatter;

        private UserInfo userInfo;

        private Client() 
        {
        }

        public bool IsConnected() { return !(tpcClient.Client.Poll(0, SelectMode.SelectRead) && tpcClient.Client.Available == 0); }

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
            tpcClient.Close();
            tpcClient.Dispose();

            netStream.Dispose();

            writer.Dispose();
            reader.Dispose();
        }

        public void CreateAccount(LoginDetails newDetails, UserInfo info)
        {
            NetCallback callback = new NetCallback(CreateAccountResultCallback, new Random().Next(), 30f);
            SendPacket(new CreateAccPacket(newDetails, info, callback.GetPacketId()));
        }

        private void CreateAccountResultCallback(Packet packet) // MOVE CALLBACK TO XMAL CODE 
        {
            CreateAccResultPacket result = (CreateAccResultPacket)packet;

            if (result != null)
            {
                if (result.Succeeded)
                {
                    // Do something
                }
            }

            Console.WriteLine(result.ResultMsg);
        }

        public void Login(LoginDetails details)
        {
            NetCallback callback = new NetCallback(LoginResultCallback, new Random().Next(), 30f);
            SendPacket(new LoginPacket(details, callback.GetPacketId()));
        }

        private void LoginResultCallback(Packet packet) 
        {
            LoginResultPacket result = (LoginResultPacket)packet;

            if (result?.UserInfo != null)
            {
                this.userInfo = result.UserInfo;

                Console.WriteLine("Logged in");
            }
            else
            {
                Console.WriteLine("Failed to log in");
            }
        }

        private void Run()
        {
            Thread readThread = new Thread(() =>
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
            if (tpcClient.Client.Available != 0)
            {
                int packetSize = -1;

                if ((packetSize = reader.ReadInt32()) != -1)
                    OnRecievePacket(formatter.Deserialize(new MemoryStream(reader.ReadBytes(packetSize))) as Packet);
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

                        break;
                    case PacketCategory.UserInfo:
                        HandleUserPacket(packet);
                        break;
                }
            }
        }

        private void HandleUserPacket(Packet packet)
        {
            switch ((packet as UserPacket)?.UserPacketType)
            {
                case UserPacketType.Login:

                    break;
                case UserPacketType.LoginResult:
                    LoginResultPacket result = packet as LoginResultPacket;

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

                    break;
                case UserPacketType.None:

                    break;
            }
        }
    }
}
