using System.IO;
using CalamityMod.TileEntities;

namespace CalamityMod.Packets
{
    internal sealed class TEUpdateCodebreakerContainedStuffPacket : CalamityPacket
    {
        public static TEUpdateCodebreakerContainedStuffPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.UpdateCodebreakerContainedStuff;

        public static void Send(TECodebreaker codeBreaker, int toClient = -1, int ignoreClient = -1)
        {
            if (codeBreaker is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteTileEntityID(codeBreaker);
            packet.Write(codeBreaker.InputtedCellCount);
            packet.Write(codeBreaker.InitialCellCountBeforeDecrypting);
            packet.Write(codeBreaker.HeldSchematicID);
            packet.Write(codeBreaker.ContainsBloodyVein);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var codeBreaker = packet.ReadTileEntity<TECodebreaker>();
            int cellCount = packet.ReadInt32();
            int cellCountBeforeDecrypting = packet.ReadInt32();
            int schematicID = packet.ReadInt32();
            bool containsBloodyVein = packet.ReadBoolean();

            // Verify to ensure that the tile entity is a valid one.
            if (codeBreaker is null)
                return;

            codeBreaker.InputtedCellCount = cellCount;
            codeBreaker.InitialCellCountBeforeDecrypting = cellCountBeforeDecrypting;
            codeBreaker.HeldSchematicID = schematicID;
            codeBreaker.ContainsBloodyVein = containsBloodyVein;
        }
    }
}
