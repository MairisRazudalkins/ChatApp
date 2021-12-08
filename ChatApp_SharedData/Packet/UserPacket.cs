using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User;

namespace Packets
{
    public enum UserPacketType
    {
        Login,
        LoginResult,
        CreateAcc,
        CreateAccResult,
        NameChange,
        ImageChange,
        None
    }

    [Serializable]
    public class UserPacket : Packet
    {
        private UserPacketType userPacketType = UserPacketType.None;
        public UserPacketType UserPacketType { get { return userPacketType; } protected set { userPacketType = value; } }

        public UserPacket(int packetId = 0) : base(packetId)
        {
            this.PacketCategory = PacketCategory.UserInfo;
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
        public UserInfo UserInfo { get { return userInfo; } protected set { userInfo = value; } }
        public LoginResultPacket(UserInfo userInfo, int packetId = 0) : base(packetId)
        {
            this.UserPacketType = UserPacketType.LoginResult;
            this.userInfo = userInfo;
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
}
