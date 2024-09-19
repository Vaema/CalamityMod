using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    public sealed class BossRushStagePacket : CalamityPacket
    {
        public static BossRushStagePacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.BossRushStage;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(BossRushEvent.BossRushStage);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            BossRushEvent.BossRushStage = packet.ReadInt32();
        }
    }
}
