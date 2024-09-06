using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.NPCs.Providence;

namespace CalamityMod.Packets
{
    public sealed class ProvidenceDyeConditionSyncPacket : CalamityPacket
    {
        public static ProvidenceDyeConditionSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.ProvidenceDyeConditionSync;

        public static void Send(Providence providence, int toClient = -1, int ignoreClient = -1)
        {
            if (providence is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(providence.NPC);
            packet.Write(providence.hasTakenDaytimeDamage);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var providence = packet.ReadModNPC<Providence>();
            var hasTakenDaytimeDmg = packet.ReadBoolean();

            if (providence is null)
                return;

            providence.hasTakenDaytimeDamage = hasTakenDaytimeDmg;
        }
    }
}
