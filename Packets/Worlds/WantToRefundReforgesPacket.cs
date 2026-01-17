using System.IO;
using CalamityMod.NPCs.TownNPCs;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Packets
{
    internal sealed class WantToRefundReforgesPacket : CalamityPacket
    {
        public static WantToRefundReforgesPacket Instance { get; private set; }

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            // Only Server should handle this action!
            if (!Main.dedServ)
                return;

            int banditIdx = NPC.FindFirstNPC(ModContent.NPCType<Bandit>());
            if (banditIdx == -1)
                return;

            NPC bandit = Main.npc[banditIdx];
            if (bandit == null || !bandit.active)
                return;

            Bandit.DoRefund(bandit);
        }
    }
}
