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
        Info,
        NameChange,
        ImageChange,
        None
    }

    [Serializable]
    public class UserPacket : Packet
    {
        private UserPacketType userPacketType = UserPacketType.None;
        public UserPacketType UserPacketType { get { return userPacketType; } protected set { userPacketType = value; } }

        public UserPacket(uint senderId = 0) : base(senderId)
        {
            this.PacketCategory = PacketCategory.UserInfo;
        }
    }

    [Serializable]
    public class LoginPacket : UserPacket
    {
        private LoginDetails loginData;
        public LoginDetails LoginData { get { return loginData; } protected set { loginData = value; } }

        public LoginPacket(LoginDetails loginData) : base()
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
        public LoginResultPacket(UserInfo userInfo) : base()
        {
            this.UserPacketType = UserPacketType.LoginResult;
            this.userInfo = userInfo;
        }
    }
}
