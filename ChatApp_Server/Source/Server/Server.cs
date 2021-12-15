using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Packets;
using User;

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
            int clientIndex = 1;

            clients = new ConcurrentDictionary<int, ConnectedClient>();
            tpcListener.Start();
            CreateGlobalChat();

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

            Console.WriteLine("Disconnected");

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
            ConnectedClient globalChat = new ConnectedClient(null, true); // BAD IMPL. BUT IT WORKS (Tarkov wiped... I got lazy :D )
            globalChat.OnLogin(new User(null, new UserInfo("Global Chat", User.GenerateUniqueId(), File.ReadAllBytes(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Data\\GlobalChatIcon.jpg"))));

            clients.TryAdd(0, globalChat);
        }

        private void Broadcast(Packet packet)
        {
            foreach (var client in clients)
            {
                if (client.Value.IsLoggedIn())
                {
                    if (client.Value.user.info.uniqueId != packet.SenderID) 
                        client.Value.SendPacket(packet);
                }
            }
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
                            ConnectedClient client = FindClient(packet.TargetID);

                            if (client != null)
                                client.SendPacket(new MsgPacket(sender.user.info.uniqueId, msgPacket.TargetID, msgPacket.Msg));
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
                    case UserPacketType.None:

                        break;
                    case UserPacketType.LoginResult:
                        break;
                }
            }
        }
    }
}
