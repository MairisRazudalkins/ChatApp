using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public enum PacketCategory
    {
        Message,
        UserInfo
    }

    [Serializable]
    public class Packet
    {
        private int packetId;
        private int senderId, targetId;
        private PacketCategory packetCategory;

        public PacketCategory PacketCategory { get { return packetCategory; } protected set { packetCategory = value; } }
        public int SenderID { get { return senderId; } protected set { senderId = value; } }
        public int TargetID { get { return targetId; } protected set { targetId = value; } }
        public int PacketId { get { return packetId; } protected set { packetId = value; } }

        public Packet(int packetId = 0) { this.packetId = packetId; }

        public Packet(int senderId, int packetId = 0) { this.senderId = senderId; this.packetId = packetId; }
        public Packet(int senderId, int targetId, int packetId = 0) { this.senderId = senderId; this.targetId = targetId; this.packetId = packetId; }
    }
}
