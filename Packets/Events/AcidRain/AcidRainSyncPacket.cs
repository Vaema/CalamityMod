using System.IO;
using CalamityMod.Events;

namespace CalamityMod.Packets;

internal sealed class AcidRainSyncPacket : CalamityPacket
{
    public static AcidRainSyncPacket Instance { get; private set; }

    public static void Send(int toClient = -1, int ignoreClient = -1)
    {
        var packet = Instance.CreateBasePacket();
        packet.Write(AcidRainEvent.AcidRainEventIsOngoing);
        packet.Write(AcidRainEvent.AccumulatedKillPoints);
        packet.Write(AcidRainEvent.TimeSinceLastAcidRainKill);
        packet.Send(toClient, ignoreClient);
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        AcidRainEvent.AcidRainEventIsOngoing = packet.ReadBoolean();
        AcidRainEvent.AccumulatedKillPoints = packet.ReadInt32();
        AcidRainEvent.TimeSinceLastAcidRainKill = packet.ReadInt32();
    }
}
