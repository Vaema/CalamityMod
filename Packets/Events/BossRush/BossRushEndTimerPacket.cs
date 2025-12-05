using System.IO;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    public sealed class BossRushEndTimerPacket : CalamityPacket
    {
        public static BossRushEndTimerPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.BossRushEndTimer;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(BossRushEvent.EndTimer);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            BossRushEvent.EndTimer = packet.ReadInt32();
        }
    }
}
