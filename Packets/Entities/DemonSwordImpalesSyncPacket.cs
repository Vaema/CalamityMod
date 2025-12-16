using System.IO;
using Terraria;

namespace CalamityMod.Packets.Entities
{
    internal sealed class DemonSwordImpalesSyncPacket : CalamityPacket
    {
        public static DemonSwordImpalesSyncPacket Instance { get; private set; }
        public override byte MessageType => (byte)CalamityModMessageType.SyncNPCDemonSwordImpales;

        public static void Send(NPC npc, int toClient = -1, int ignoreClient = -1)
        {
            if (npc is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(npc);
            packet.Write(npc.Calamity().demonSwordImpales);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var npc = packet.ReadNPC();
            var impales = packet.ReadInt32();

            if (npc is null)
                return;

            npc.Calamity().demonSwordImpales = impales;
        }
    }
}
