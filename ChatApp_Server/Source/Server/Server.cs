using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Packets;

namespace Server
{
    public class Server
    {
        private ConcurrentDictionary<int, ConnectedClient> clients;
        private TcpListener tpcListener;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            tpcListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            int clientIndex = 0;

            clients = new ConcurrentDictionary<int, ConnectedClient>();
            tpcListener.Start();

            while (true)
            {
                Socket socket = tpcListener.AcceptSocket();

                if (socket != null)
                {
                    int index = clientIndex;
                    clientIndex++;

                    clients.TryAdd(index, new ConnectedClient(socket));

                    Thread t = new Thread(() => { ClientMethod(index); });
                    t.Start();
                }
            }
        }

        private void ClientMethod(int index)
        {
            ConnectedClient client = clients[index];
            Packet packetReceived;

            while (client.IsConnected())
                if ((packetReceived = client.Read()) != null)
                    HandlePacket(client, packetReceived);

            client.OnDisconnect();

            if (!clients.TryRemove(index, out client))
                Console.WriteLine("Failed to remove client {0} from connected users", index);
        }

        private bool ValidatePacket(ConnectedClient sender, Packet packet)
        {
            return true;
        }

        private void HandlePacket(ConnectedClient sender, Packet packet)
        {
            if (ValidatePacket(sender, packet))
            {
                switch (packet.PacketCategory)
                {
                    case PacketCategory.Message:

                        break;
                    case PacketCategory.UserInfo:
                        HandleUserInfoPacket(sender, packet);
                        break;
                }
            }
        }

        private void HandleUserInfoPacket(ConnectedClient sender, Packet packet)
        {
            if ((packet as UserPacket) != null)
            {
                switch ((packet as UserPacket).UserPacketType)
                {
                    case UserPacketType.Login:
                        LoginPacket loginPacket = (packet as LoginPacket);

                        User userData = User.TryLoadUserInfo(loginPacket?.LoginData.userName);

                        if (userData != null && loginPacket != null)
                        {
                            if (userData.loginDetails.password == loginPacket.LoginData.password)
                            {
                                sender.SendPacket(new LoginResultPacket(userData.info, packet.PacketId));
                                break;
                            }
                        }

                        sender.SendPacket(new LoginResultPacket(null, packet.PacketId));

                        break;
                    case UserPacketType.CreateAcc:
                        CreateAccPacket createAccPacket = (packet as CreateAccPacket);

                        if (createAccPacket.UserInfo != null && createAccPacket.LoginData != null)
                        {
                            if (User.CreateAccount(createAccPacket.LoginData, createAccPacket.UserInfo, out User user))
                            {
                                user.SaveData();
                                sender.userInfo = user.info;
                                sender.SendPacket(new CreateAccResultPacket(true, "Successfuly created profile.", packet.PacketId));
                            }
                            else
                                sender.SendPacket(new CreateAccResultPacket(false, "User name already taken.", packet.PacketId));
                        }

                        break;
                    case UserPacketType.NameChange:

                        break;
                    case UserPacketType.ImageChange:

                        break;
                    case UserPacketType.None:

                        break;
                    case UserPacketType.LoginResult:
                        break;
                }
            }
        }
    }
}
