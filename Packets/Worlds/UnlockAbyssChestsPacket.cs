using System.IO;
using CalamityMod.World;

namespace CalamityMod.Packets;

internal sealed class UnlockAbyssChestsPacket : CalamityPacket
{
    public static UnlockAbyssChestsPacket Instance { get; private set; }

    public static void Send(int toClient = -1, int ignoreClient = -1)
    {
        var packet = Instance.CreateBasePacket();
        packet.Send(toClient, ignoreClient);
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        Abyss.DoUnlockAllAbyssChests();
    }
}
