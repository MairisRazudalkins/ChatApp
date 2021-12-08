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
        private uint senderId, targetId;
        private PacketCategory packetCategory;

        public PacketCategory PacketCategory { get { return packetCategory; } protected set { packetCategory = value; } }
        public uint SenderID { get { return senderId; } protected set { senderId = value; } }
        public uint TargetID { get { return targetId; } protected set { targetId = value; } }

        public Packet() { }

        public Packet(uint senderId) { this.senderId = senderId; }
        public Packet(uint senderId, uint targetId) { this.senderId = senderId; this.targetId = targetId; }
    }
}
