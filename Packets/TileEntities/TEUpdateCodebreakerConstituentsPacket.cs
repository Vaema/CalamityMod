using System.IO;
using CalamityMod.TileEntities;
using Terraria;

namespace CalamityMod.Packets
{
    internal class TEUpdateCodebreakerConstituentsPacket : CalamityPacket
    {
        public static TEUpdateCodebreakerConstituentsPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.UpdateCodebreakerConstituents;

        public static void Send(TECodebreaker codeBreaker, int toClient = -1, int ignoreClient = -1)
        {
            if (codeBreaker is null)
                return;

            var packet = Instance.CreateBasePacket();
            BitsByte containmentFlagWrapper = new BitsByte();
            containmentFlagWrapper[0] = codeBreaker.ContainsDecryptionComputer;
            containmentFlagWrapper[1] = codeBreaker.ContainsSensorArray;
            containmentFlagWrapper[2] = codeBreaker.ContainsAdvancedDisplay;
            containmentFlagWrapper[3] = codeBreaker.ContainsVoltageRegulationSystem;
            containmentFlagWrapper[4] = codeBreaker.ContainsCoolingCell;

            packet.WriteTileEntityID(codeBreaker);
            packet.Write(containmentFlagWrapper);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var codeBreaker = packet.ReadTileEntity<TECodebreaker>();
            BitsByte containmentFlagWrapper = packet.ReadByte();
            bool containsDecryptionComputer = containmentFlagWrapper[0];
            bool containsSensorArray = containmentFlagWrapper[1];
            bool containsAdvancedDisplay = containmentFlagWrapper[2];
            bool containsVoltageRegulationSystem = containmentFlagWrapper[3];
            bool containsCoolingCell = containmentFlagWrapper[4];

            // Verify to ensure that the tile entity is a valid one.
            if (codeBreaker is null)
                return;

            codeBreaker.ContainsDecryptionComputer = containsDecryptionComputer;
            codeBreaker.ContainsSensorArray = containsSensorArray;
            codeBreaker.ContainsAdvancedDisplay = containsAdvancedDisplay;
            codeBreaker.ContainsVoltageRegulationSystem = containsVoltageRegulationSystem;
            codeBreaker.ContainsCoolingCell = containsCoolingCell;

            // Send the packet again to the other clients if this packet was received on the server.
            // Since ModPackets go solely to the server when sent by a client this is necesssary
            // to ensure that all clients are informed of what happened.
            if (Main.dedServ)
                Send(codeBreaker, ignoreClient: sender);
        }
    }
}
