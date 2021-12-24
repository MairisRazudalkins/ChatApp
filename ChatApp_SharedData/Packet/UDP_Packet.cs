using System;
using System.Net;

namespace Packets
{
    [Serializable]
    public class UDP_LoginPacket : Packet
    {
        private IPEndPoint ipEndPoint;
        public IPEndPoint IpEndPoint { get { return ipEndPoint; } protected set { ipEndPoint = value; } }

        public UDP_LoginPacket(IPEndPoint ipEndPoint) 
        {
            this.ipEndPoint = ipEndPoint;
            this.PacketCategory = PacketCategory.UDP_Login;
        }
    }

    [Serializable]
    public class UDP_PositionPacket : Packet 
    {
        private float x, y;
        private float r, g, b;

        public float X { get { return x; } protected set { x = value; } }
        public float Y { get { return y; } protected set { y = value; } }

        public float R { get { return r; } protected set { r = value; } }
        public float G { get { return g; } protected set { g = value; } }
        public float B { get { return b; } protected set { b = value; } }

        public UDP_PositionPacket(float x, float y, float r, float g, float b, int senderId = 0) : base(0, senderId) 
        {
            this.x = x;
            this.y = y;
            this.r = r;
            this.g = g;
            this.b = b;

            this.PacketCategory = PacketCategory.UDP_PositionUpdate;
        }
    }
}
