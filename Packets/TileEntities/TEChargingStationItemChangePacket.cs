using System.IO;
using CalamityMod.TileEntities;
using Terraria;
using Terraria.ModLoader.IO;

namespace CalamityMod.Packets
{
    public sealed class TEChargingStationItemChangePacket : CalamityPacket
    {
        public static TEChargingStationItemChangePacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.ChargingStationItemChange;

        public static void Send(TEChargingStation chargingStn, Item pluggedItem, int toClient = -1, int ignoreClient = -1)
        {
            if (chargingStn is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteTileEntityID(chargingStn);
            ItemIO.Send(pluggedItem, packet, writeStack: true, writeFavorite: true);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var chargingStn = packet.ReadTileEntity<TEChargingStation>();
            Item thePlug = ItemIO.Receive(packet, readStack: true, readFavorite: true);

            if (chargingStn is null)
                return;

            chargingStn.PluggedItem = thePlug;

            // When a server gets this packet, it immediately sends an equivalent packet to all clients.
            if (Main.dedServ)
                Send(chargingStn, thePlug);
        }
    }
}
