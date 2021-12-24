using System;
using User;

namespace Packets
{
    public enum UserPacketType
    {
        Login,
        LoginResult,
        Disconnect,
        CreateAcc,
        CreateAccResult,
        Contact,
        NameChange,
        ImageChange,
        None
    }

    [Serializable]
    public class UserPacket : Packet
    {
        private UserPacketType userPacketType = UserPacketType.None;
        public UserPacketType UserPacketType { get { return userPacketType; } protected set { userPacketType = value; } }

        public UserPacket(int packetId = 0, int senderId = 0, int targerId = 0) : base(packetId, senderId, targerId)
        {
            this.PacketCategory = PacketCategory.UserInfo;
        }
    }

    [Serializable]
    public class DisconnectPacket : UserPacket
    {
        public DisconnectPacket(int disconnectedId) : base(0, disconnectedId, 0)
        {
            this.UserPacketType = UserPacketType.Disconnect;
        }
    }

    [Serializable]
    public class LoginPacket : UserPacket
    {
        private LoginDetails loginData;
        public LoginDetails LoginData { get { return loginData; } protected set { loginData = value; } }

        public LoginPacket(LoginDetails loginData, int packetId = 0) : base(packetId)
        {
            this.UserPacketType = UserPacketType.Login;
            this.loginData = loginData;
        }
    }

    [Serializable]
    public class LoginResultPacket : UserPacket
    {
        private UserInfo userInfo;
        private string resultMsg;
        public UserInfo UserInfo { get { return userInfo; } protected set { userInfo = value; } }
        public string ResultMsg { get { return resultMsg; } protected set { resultMsg = value; } }
        public LoginResultPacket(UserInfo userInfo, string resultMsg, int packetId = 0) : base(packetId)
        {
            this.UserPacketType = UserPacketType.LoginResult;
            this.userInfo = userInfo;
            this.resultMsg = resultMsg;
        }
    }

    [Serializable]
    public class CreateAccPacket : UserPacket
    {
        private LoginDetails loginData;
        private UserInfo userInfo;

        public LoginDetails LoginData { get { return loginData; } protected set { loginData = value; } }
        public UserInfo UserInfo { get { return userInfo; } protected set { userInfo = value; } }

        public CreateAccPacket(LoginDetails LoginData, UserInfo userInfo, int packetId = 0) : base(packetId)
        {
            this.UserPacketType = UserPacketType.CreateAcc;
            this.LoginData = LoginData;
            this.userInfo = userInfo;
        }
    }

    [Serializable]
    public class CreateAccResultPacket : UserPacket 
    {
        private bool bSuccess;
        private string resultMsg;

        public bool Succeeded { get { return bSuccess; } protected set { bSuccess = value; } }
        public string ResultMsg { get { return resultMsg; } protected set { resultMsg = value; } }

        public CreateAccResultPacket(bool bSuccess, string resultMsg, int packetId = 0) : base(packetId)
        {
            this.UserPacketType = UserPacketType.CreateAccResult;
            this.bSuccess = bSuccess;
            this.resultMsg = resultMsg;
        }
    }

    [Serializable]
    public class ChangeImagePacket : UserPacket
    {
        private byte[] imgData;

        public byte[] ImgData { get { return imgData; } protected set { imgData = value; } }

        public ChangeImagePacket(byte[] imgData, int senderId = 0) : base(0, senderId)
        {
            this.UserPacketType = UserPacketType.ImageChange;
            this.imgData = imgData;
        }
    }

    [Serializable]
    public class ContactPacket : UserPacket
    {
        private UserInfo userInfo;

        public UserInfo UserInfo { get { return userInfo; } protected set { userInfo = value; } }

        public ContactPacket(UserInfo userInfo, int senderId = 0) : base(0, senderId)
        {
            this.UserPacketType = UserPacketType.Contact;
            this.userInfo = userInfo;
        }
    }
}
