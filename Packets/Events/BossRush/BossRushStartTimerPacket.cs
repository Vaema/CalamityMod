using System.IO;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    internal sealed class BossRushStartTimerPacket : CalamityPacket
    {
        public static BossRushStartTimerPacket Instance { get; private set; }

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(BossRushEvent.StartTimer);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            BossRushEvent.StartTimer = packet.ReadInt32();
        }
    }
}
