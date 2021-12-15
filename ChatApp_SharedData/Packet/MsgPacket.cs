using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User;

namespace Packets
{
    public enum MessageType
    {
        MsgList,
        Msg,
        Image,
        File
    }

    [Serializable]
    public class MsgPacket : Packet
    {
        private Message msg;

        private MessageType msgCategory = MessageType.Msg;
        public MessageType MsgCategory { get { return msgCategory; } protected set { msgCategory = value; } }
        public Message Msg { get { return msg; } protected set { msg = value; } }
        public MsgPacket(int senderId, int targetId, Message msg) : base(0, senderId, targetId) { this.PacketCategory = PacketCategory.Message; this.msg = msg; }
    }

    [Serializable]
    public class ImgMsgPacket : MsgPacket
    {
        private byte[] imgData;
        public byte[] ImgData { get { return imgData; } protected set { imgData = value; } }

        public ImgMsgPacket(int senderId, int targetId, byte[] imgData, Message msg = null) : base(senderId, targetId, msg)
        {
            this.MsgCategory = MessageType.Image; 
            this.imgData = imgData;
        }
    }
}
