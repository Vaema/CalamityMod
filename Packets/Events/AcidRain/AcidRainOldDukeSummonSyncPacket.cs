using System.IO;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    internal sealed class AcidRainOldDukeSummonSyncPacket : CalamityPacket
    {
        public static AcidRainOldDukeSummonSyncPacket Instance { get; private set; }

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
