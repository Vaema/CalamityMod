using System.IO;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    internal sealed class BRHostileProjKillSyncPacket : CalamityPacket
    {
        public static BRHostileProjKillSyncPacket Instance { get; private set; }

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(BossRushEvent.HostileProjectileKillCounter);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            BossRushEvent.HostileProjectileKillCounter = packet.ReadInt32();
        }
    }
}
