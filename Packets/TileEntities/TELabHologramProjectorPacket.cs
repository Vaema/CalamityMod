using System.IO;
using CalamityMod.TileEntities;

namespace CalamityMod.Packets;

internal sealed class TELabHologramProjectorPacket : CalamityPacket
{
    public static TELabHologramProjectorPacket Instance { get; private set; }

    public static void Send(TELabHologramProjector projector, bool poppingUp, int toClient = -1, int ignoreClient = -1)
    {
        if (projector is null)
            return;

        var packet = Instance.CreateBasePacket();
        packet.WriteTileEntityID(projector);
        packet.Write(poppingUp);
        packet.Send(toClient, ignoreClient);
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        var projector = packet.ReadTileEntity<TELabHologramProjector>();
        bool pop = packet.ReadBoolean();

        if (projector is null)
            return;

        projector.PoppingUp = pop;
    }
}
