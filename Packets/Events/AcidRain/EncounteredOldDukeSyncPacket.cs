using System.IO;
using CalamityMod.Events;

namespace CalamityMod.Packets
{
    internal sealed class EncounteredOldDukeSyncPacket : CalamityPacket
    {
        public static EncounteredOldDukeSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.EncounteredOldDukeSync;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(AcidRainEvent.OldDukeHasBeenEncountered);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            AcidRainEvent.OldDukeHasBeenEncountered = packet.ReadBoolean();
        }
    }
}
