using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User
{
    public enum MsgType
    {
        Regular,
        Img,
    }

    [Serializable]
    public class Message
    {
        public readonly string senderName;
        public readonly int senderId;
        public readonly string msg;
        public readonly MsgType msgType;

        public Message(string senderName, int senderId, string msg, MsgType msgType = MsgType.Regular)
        {
            this.senderName = senderName;
            this.senderId = senderId;
            this.msgType = msgType;
            this.msg = msg;
        }
    }

    [Serializable]
    public class ImgMessage : Message
    {
        public readonly byte[] imgData;

        public ImgMessage(string senderName, int senderId, string msg, byte[] imgData) : base(senderName, senderId, msg, MsgType.Img)
        {
            this.imgData = imgData;
        }
    }
}
