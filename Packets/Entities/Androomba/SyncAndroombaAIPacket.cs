using System.IO;
using CalamityMod.NPCs.TownNPCs;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class SyncAndroombaAIPacket : CalamityPacket
    {
        public static SyncAndroombaAIPacket Instance { get; private set; }

        public static void Send(AndroombaFriendly roomba, int phase = -1, int toClient = -1, int ignoreClient = -1)
        {
            if (roomba is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(roomba);
            packet.Write(phase != -1 ? phase : (int)roomba.NPC.ai[0]); // Phase
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var roomba = packet.ReadModNPC<AndroombaFriendly>();
            var phase = packet.ReadInt32();

            if (roomba is null)
                return;

            if (Main.dedServ)
                AndroombaFriendly.ChangeAI(roomba.NPC.whoAmI, phase);
        }
    }
}
