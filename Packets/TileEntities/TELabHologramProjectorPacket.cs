using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.TileEntities;
using Terraria.DataStructures;

namespace CalamityMod.Packets
{
    public sealed class TELabHologramProjectorPacket : CalamityPacket
    {
        public static TELabHologramProjectorPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.LabHologramProjector;

        public static void Send(TELabHologramProjector projector, bool poppingUp, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.WriteTileEntityID(projector);
            packet.Write(poppingUp);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var projector = packet.ReadTileEntity<TELabHologramProjector>();
            bool pop = packet.ReadBoolean();

            if (projector is null)
                return;

            projector.PoppingUp = pop;
        }
    }
}
