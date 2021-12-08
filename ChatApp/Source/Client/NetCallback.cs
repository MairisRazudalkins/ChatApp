using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Packets;

namespace ChatApp
{
    class NetCallback
    {
        static private List<NetCallback> pendingCallbacks = new List<NetCallback>(); // ID - callback;

        private int packetId;
        private bool bIsHandled;
        private float validityLength;

        private Action<Packet> callback;

        public int GetPacketId() { return packetId; }

        public NetCallback(Action<Packet> callback, int packetId, float validityLength = 10f) 
        {
            this.callback = callback;
            this.packetId = packetId;
            this.validityLength = validityLength;
            this.bIsHandled = false;

            pendingCallbacks.Add(this);

            if (validityLength > 0f)
                BeginValidity();
        }

        public bool TryRun(Packet packet)
        {
            if (packet.PacketId == packetId && callback != null)
            {
                callback(packet);
                return bIsHandled = true;
            }

            return false;
        }

        private async void BeginValidity()
        {
            await Task.Run(() => 
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                while (stopWatch.ElapsedMilliseconds < (int)validityLength * 1000 && !bIsHandled) { } // Hate while loop... Replace

                DisposeCallback(this);
            });
        }

        public static void DisposeCallback(NetCallback data)
        {
            pendingCallbacks.Remove(data);
            Console.WriteLine("Callback disposed!");
        }

        public static bool OnRecievedPacket(Packet packet)
        {
            for (int i = 0; i < pendingCallbacks.Count; i++)
                if (pendingCallbacks[i].TryRun(packet))
                    return true;

            return false;
        }
    }
}
