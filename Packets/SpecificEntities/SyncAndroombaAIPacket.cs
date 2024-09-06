using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.NPCs.DraedonLabThings;
using CalamityMod.NPCs.TownNPCs;
using Microsoft.Build.Execution;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class SyncAndroombaAIPacket : CalamityPacket
    {
        public static SyncAndroombaAIPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.SyncAndroombaAI;

        public static void Send(AndroombaFriendly roomba, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(roomba.NPC);
            packet.Write((int)roomba.NPC.ai[0]); // Phase
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
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
