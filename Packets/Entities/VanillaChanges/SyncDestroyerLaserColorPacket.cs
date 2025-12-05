using System.IO;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class SyncDestroyerLaserColorPacket : CalamityPacket
    {
        public static SyncDestroyerLaserColorPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.SyncDestroyerLaserColor;

        public static void Send(NPC npc, int toClient = -1, int ignoreClient = -1)
        {
            if (npc is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(npc);
            packet.Write(npc.Calamity().destroyerLaserColor);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var npc = packet.ReadNPC();
            var laserColor = packet.ReadInt32();

            if (npc is null)
                return;

            npc.Calamity().destroyerLaserColor = laserColor;
        }
    }
}
