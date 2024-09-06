using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class SyncVanillaNPCLocalAIArrayPacket : CalamityPacket
    {
        public static SyncVanillaNPCLocalAIArrayPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.SyncVanillaNPCLocalAIArray;

        public static void Send(NPC npc, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(npc);
            packet.Write(npc.localAI[0]);
            packet.Write(npc.localAI[1]);
            packet.Write(npc.localAI[2]);
            packet.Write(npc.localAI[3]);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var npc = packet.ReadNPC();
            var ai0 = packet.ReadSingle();
            var ai1 = packet.ReadSingle();
            var ai2 = packet.ReadSingle();
            var ai3 = packet.ReadSingle();

            if (npc is null)
                return;

            npc.localAI[0] = ai0;
            npc.localAI[1] = ai1;
            npc.localAI[2] = ai2;
            npc.localAI[3] = ai3;
        }
    }
}
