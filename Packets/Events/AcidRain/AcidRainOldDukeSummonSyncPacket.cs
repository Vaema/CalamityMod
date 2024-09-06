using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    public sealed class AcidRainOldDukeSummonSyncPacket : CalamityPacket
    {
        public static AcidRainOldDukeSummonSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.AcidRainOldDukeSummonSync;
        
        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(AcidRainEvent.HasTriedToSummonOldDuke);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            AcidRainEvent.HasTriedToSummonOldDuke = packet.ReadBoolean();
        }
    }
}
