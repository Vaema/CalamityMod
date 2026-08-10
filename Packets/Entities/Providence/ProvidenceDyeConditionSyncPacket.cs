using System.IO;
using CalamityMod.NPCs.Providence;

namespace CalamityMod.Packets;

internal sealed class ProvidenceDyeConditionSyncPacket : CalamityPacket
{
    public static ProvidenceDyeConditionSyncPacket Instance { get; private set; }

    public static void Send(Providence providence, int toClient = -1, int ignoreClient = -1)
    {
        if (providence is null)
            return;

        var packet = Instance.CreateBasePacket();
        packet.WriteWhoAmI(providence);
        packet.Write(providence.hasBeenGivenFullPower);
        packet.Send(toClient, ignoreClient);
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        var providence = packet.ReadModNPC<Providence>();
        var hasBeenEnraged = packet.ReadBoolean();

        if (providence is null)
            return;

        providence.hasBeenGivenFullPower = hasBeenEnraged;
    }
}
