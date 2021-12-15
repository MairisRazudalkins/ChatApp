using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Packets;
using User;

namespace Server
{
    public class ConnectedClient
    {
        private Socket socket;
        private NetworkStream netStream;

        private BinaryReader reader;
        private BinaryWriter writer;
        private BinaryFormatter formatter;

        private bool bIsLoggedIn;
        private bool bIsGlobalChat;
        public User user;

        private object readLock, writeLock;

        public bool IsConnected() { return !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0); } // Socket.Connected only returns if the connection is open based on the previous data recieved or sent.
        public bool IsLoggedIn() { return bIsLoggedIn; }
        public bool IsGlobalChat() { return bIsGlobalChat; }

        public ConnectedClient(Socket socket, bool bIsGlobalChat = false)
        {
            if (socket != null)
            {
                this.socket = socket;

                readLock = new object();
                writeLock = new object();

                formatter = new BinaryFormatter();
                netStream = new NetworkStream(socket, true);
                reader = new BinaryReader(netStream, Encoding.UTF8);
                writer = new BinaryWriter(netStream, Encoding.UTF8);

                Console.WriteLine("Client Connected");
            }

            this.bIsGlobalChat = bIsGlobalChat;
        }

        public void OnLogin(User user)
        {
            bIsLoggedIn = true;
            this.user = user;
        }

        public void OnDisconnect()
        {
            socket.Close();
            socket.Dispose();

            netStream.Dispose();

            reader.Dispose();
            writer.Dispose();

            Console.WriteLine("Client disconnected");
        }

        public Packet Read()
        {
            if (bIsGlobalChat)
                return null;

            lock (readLock)
            {
                if (socket.Available == 0)
                    return null;

                int packetSize = -1;

                if ((packetSize = reader.ReadInt32()) != -1)
                {
                    byte[] buffer = reader.ReadBytes(packetSize);

                    MemoryStream stream = new MemoryStream(buffer);
                    return formatter.Deserialize(stream) as Packet;
                }

                return null;
            }
        }

        public void SendPacket(Packet packet)
        {
            if (bIsGlobalChat)
                return;

            lock (writeLock)
            {
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, packet);

                byte[] buffer = stream.GetBuffer();

                writer.Write(buffer.Length);
                writer.Write(buffer);
                writer.Flush();
            }
        }
    }
}
