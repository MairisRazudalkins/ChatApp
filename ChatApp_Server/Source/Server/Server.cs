using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using Packets;
using User;

namespace Server
{
    public class Server
    {
        private ConcurrentDictionary<int, ConnectedClient> clients;
        private ConcurrentDictionary<int, RPSGame> activeGames;

        private TcpListener tpcListener;
        private UdpClient udpListener;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            tpcListener = new TcpListener(ip, port);
            udpListener = new UdpClient(port);
        }

        private void UDP_SendPacket(ConnectedClient client, Packet packet) 
        {
            if (client.ipEndPoint == null || packet == null)
                return;

            MemoryStream stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, packet);
            byte[] buffer = stream.GetBuffer();

            udpListener.Send(buffer, buffer.Length, client.ipEndPoint);
        }

        private void UDP_Listen()
        {
            try
            {
                while (true)
                {
                    IPEndPoint defaultIp = new IPEndPoint(IPAddress.Any, 0);

                    byte[] buffer = udpListener.Receive(ref defaultIp);

                    if (buffer == null)
                        continue;

                    foreach (ConnectedClient client in clients.Values)
                    {
                        if (client.ipEndPoint != null)
                        {
                            if (client.ipEndPoint.ToString() == defaultIp.ToString())
                            {
                                HandlePacket(client, new BinaryFormatter().Deserialize(new MemoryStream(buffer)) as Packet);
                                break;
                            }
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("UDP ERROR: " + e.Message);
            }
        }

        public void Start()
        {
            int clientIndex = 1;

            clients = new ConcurrentDictionary<int, ConnectedClient>();
            activeGames = new ConcurrentDictionary<int, RPSGame>();
            tpcListener.Start();
            CreateGlobalChat();

            Thread udpListenThread = new Thread(new ThreadStart(UDP_Listen));
            udpListenThread.Start();

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

            if (client.IsLoggedIn())
                Broadcast(new DisconnectPacket(client.user.info.uniqueId));

            client.OnDisconnect();

            if (!clients.TryRemove(index, out client))
                Console.WriteLine("Failed to remove client {0} from connected users", index);
        }

        private void OnConnect(ConnectedClient client)
        {
            Broadcast(new ContactPacket(client.user.info, client.user.info.uniqueId));

            foreach (var contacts in clients)
            {
                if (contacts.Value.IsLoggedIn())
                {
                    if (contacts.Value.user.info.uniqueId != client.user.info.uniqueId)
                        client.SendPacket(new ContactPacket(contacts.Value.user.info));
                }
            }
        }

        private void CreateGlobalChat()
        {
            ConnectedClient globalChat = new ConnectedClient(null, true);
            globalChat.OnLogin(new User(null, new UserInfo("Global Chat", User.GenerateUniqueId(), File.ReadAllBytes(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Data\\GlobalChatIcon.jpg"))));

            clients.TryAdd(0, globalChat);
        }

        private void Broadcast(Packet packet, bool bUseUDP = false)
        {
            foreach (ConnectedClient client in clients.Values)
            {
                if (client.IsLoggedIn())
                {
                    if (client.user.info.uniqueId != packet.SenderID)
                    {
                        if (bUseUDP)
                            UDP_SendPacket(client, packet);
                        else
                            client.SendPacket(packet);
                    }
                }
            }
        }

        private RPSGame FindGame(int p1Id, int p2Id) 
        {
            foreach (var game in activeGames)
                if (game.Value.DoesContainPlayers(p1Id, p2Id))
                    return game.Value;

            return null;
        }

        private void HandlePacket(ConnectedClient sender, Packet packet)
        {
            switch (packet.PacketCategory)
            {
                case PacketCategory.Message:
                    HandleMessagePacket(sender, packet);
                    break;
                case PacketCategory.UserInfo:
                    HandleUserInfoPacket(sender, packet);
                    break;
                case PacketCategory.UDP_Login:
                    sender.ipEndPoint = ((UDP_LoginPacket)packet).IpEndPoint;
                    break;
                case PacketCategory.UDP_PositionUpdate:
                    UDP_PositionPacket posPacket = (UDP_PositionPacket)packet;
                    Broadcast(new UDP_PositionPacket(posPacket.X, posPacket.Y, posPacket.R, posPacket.G, posPacket.B, sender.user.info.uniqueId));
                    break;
            }
        }

        private ConnectedClient FindClient(int id)
        {
            foreach (var client in clients)
                if (client.Value.user.info.uniqueId == id)
                    return client.Value;

            return null;
        }

        public void HandleMessagePacket(ConnectedClient sender, Packet packet)
        {
            MsgPacket msgPacket = (MsgPacket)packet;

            if (msgPacket != null)
            {
                switch (msgPacket.MsgCategory)
                {
                    case MessageType.Msg:
                        if (packet.TargetID == clients[0].user.info.uniqueId)
                        {
                            Broadcast(new MsgPacket(clients[0].user.info.uniqueId, 0, msgPacket.Msg));
                        }
                        else
                        {
                            ConnectedClient target = FindClient(packet.TargetID);

                            if (target != null) 
                            {
                                if (msgPacket.Msg.msg[0] == '/')
                                {
                                    RPSGame game = FindGame(sender.user.info.uniqueId, packet.TargetID);

                                    if (msgPacket.Msg.msg.ToLower() == "/play" && game == null)
                                    {
                                        int gameId = new Random().Next();
                                        activeGames.TryAdd(gameId, new RPSGame(sender, target, gameId));
                                        return;
                                    }

                                    if (game != null)
                                    {
                                        game.ProccessCommand(sender.user.info.uniqueId, msgPacket.Msg.msg);

                                        if (game.ShouldClose() == true)
                                            activeGames.TryRemove(game.GetId(), out game);
                                    }
                                        //game.OnRecieveSelection(sender.user.info.uniqueId, msgPacket.Msg.msg.Substring(1));
                                }
                                else
                                {
                                    target.SendPacket(new MsgPacket(sender.user.info.uniqueId, msgPacket.TargetID, msgPacket.Msg));
                                }
                            }
                        }

                        break;
                    case MessageType.Image:
                        ImgMsgPacket imgPacket = (ImgMsgPacket)packet;

                        if (imgPacket != null)
                        {
                            if (packet.TargetID == clients[0].user.info.uniqueId)
                            {
                                Broadcast(new ImgMsgPacket(clients[0].user.info.uniqueId, 0, imgPacket.ImgData, imgPacket.Msg));
                            }
                            else
                            {
                                ConnectedClient client = FindClient(packet.TargetID);

                                if (client != null)
                                    client.SendPacket(new ImgMsgPacket(sender.user.info.uniqueId, msgPacket.TargetID, imgPacket.ImgData, imgPacket.Msg));
                            }
                        }
                        break;
                    case MessageType.File:

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
                        LoginPacket loginPacket = (LoginPacket)packet;

                        User userData = User.TryLoadUserInfo(loginPacket?.LoginData.userName);

                        if (userData != null && loginPacket != null)
                        {
                            if (userData.loginDetails.password == loginPacket.LoginData.password)
                            {
                                sender.OnLogin(userData);
                                sender.SendPacket(new LoginResultPacket((sender.user = userData).info, "Login Successful",packet.PacketId));
                                OnConnect(sender);
                                break;
                            }
                        }

                        sender.SendPacket(new LoginResultPacket(null, "Incorrect login details", packet.PacketId));

                        break;
                    case UserPacketType.CreateAcc:
                        CreateAccPacket createAccPacket = (CreateAccPacket)packet;

                        if (createAccPacket.UserInfo != null && createAccPacket.LoginData != null)
                        {
                            if (User.CreateAccount(createAccPacket.LoginData, createAccPacket.UserInfo, out User user))
                            {
                                sender.OnLogin(user);
                                user.SaveData();
                                sender.SendPacket(new CreateAccResultPacket(true, "Successfuly created profile.", packet.PacketId));
                                OnConnect(sender);
                            }
                            else
                                sender.SendPacket(new CreateAccResultPacket(false, "User name already taken.", packet.PacketId));
                        }

                        break;
                    case UserPacketType.NameChange:

                        break;
                    case UserPacketType.ImageChange:
                        ChangeImagePacket imagePacket = (ChangeImagePacket)packet;

                        sender.user.info.image = imagePacket.ImgData;
                        sender.user.SaveData();

                        Broadcast(new ChangeImagePacket(imagePacket.ImgData, sender.user.info.uniqueId));
                        // TODO: Broadcast changes!

                        break;
                }
            }
        }
    }
}
