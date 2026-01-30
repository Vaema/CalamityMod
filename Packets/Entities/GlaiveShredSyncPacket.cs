using System.IO;
using Terraria;

namespace CalamityMod.Packets.Entities
{
    internal sealed class GlaiveShredPacket : CalamityPacket
    {
        public static GlaiveShredPacket Instance { get; private set; }

        public static void Send(NPC npc, int toClient = -1, int ignoreClient = -1)
        {
            if (npc is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(npc);
            packet.Write(npc.Calamity().glaiveShredTimer);
            packet.Write(npc.Calamity().blazingStarShredTimer);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var npc = packet.ReadNPC();
            var glaive = packet.ReadInt32();
            var blazingstar = packet.ReadInt32();

            if (npc is null)
                return;

            npc.Calamity().glaiveShredTimer = glaive;
            npc.Calamity().blazingStarShredTimer = blazingstar;
        }
    }
}
