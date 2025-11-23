using System.IO;
using CalamityMod.CalPlayer;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class MousePositionSyncPacket : CalamityPacket
    {
        public static MousePositionSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.MousePositionSync;

        public static void Send(CalamityPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write((short)player.mouseWorldDeltaFromPlayer.X);
            packet.Write((short)player.mouseWorldDeltaFromPlayer.Y);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var deltaX = packet.ReadInt16();
            var deltaY = packet.ReadInt16();

            if (player is null)
                return;

            player.mouseWorldDeltaFromPlayer = new(deltaX, deltaY);

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
