using System.IO;
using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Packets
{
    internal sealed class SpawnSuperDummyPacket : CalamityPacket
    {
        public static SpawnSuperDummyPacket Instance { get; private set; }

        public static void Send(int x, int y, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(x);
            packet.Write(y);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var x = packet.ReadInt32();
            var y = packet.ReadInt32();

            // Not strictly necessary, but helps prevent unnecessary packetstorm in MP
            if (Main.dedServ)
                NPC.NewNPC(new EntitySource_WorldEvent(), x, y, ModContent.NPCType<SuperDummyNPC>());
        }
    }
}
