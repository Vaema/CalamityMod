using System.IO;
using CalamityMod.NPCs.Abyss;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class SyncSlabCrabAIPacket : CalamityPacket
    {
        public static SyncSlabCrabAIPacket Instance { get; private set; }

        public static void Send(SlabCrab crab, int phase = -1, int toClient = -1, int ignoreClient = -1)
        {
            if (crab is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(crab);
            packet.Write(phase != -1 ? phase : (int)crab.NPC.ai[0]); // Phase
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var crab = packet.ReadModNPC<SlabCrab>();
            var phase = packet.ReadInt32();

            if (crab is null)
                return;

            if (Main.dedServ)
                crab.ChangePhase(phase);
        }
    }
}
