using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public enum MessageType
    {
        Msg,
        Image,
        File
    }

    [Serializable]
    public class MsgPacket : Packet
    {
        private MessageType msgCategory = MessageType.Msg;
        public MessageType MsgCategory { get { return msgCategory; } protected set { msgCategory = value; } }

        private string msg;
        public string GetMsg() { return msg; }

        public MsgPacket(int senderId, int targetId, string message) : base(senderId, targetId) { this.PacketCategory = PacketCategory.Message; msg = message; }
    }

    [Serializable]
    public class ImgMsgPacket : MsgPacket
    {
        private byte[] imgData;
        public byte[] ImgData { get { return imgData; } protected set { imgData = value; } }

        public ImgMsgPacket(int senderId, int targetId, byte[] imgData, string message = "") : base(senderId, targetId, message) { this.MsgCategory = MessageType.Image; this.imgData = imgData; }
    }
}
