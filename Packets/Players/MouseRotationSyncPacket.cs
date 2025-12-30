using System;
using System.IO;
using CalamityMod.CalPlayer;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class MouseRotationSyncPacket : CalamityPacket
    {
        public static MouseRotationSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.MouseRotationSync;

        public static void Send(CalamityPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write((Half)player.mouseRotationFromPlayer);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var rotation = (float)packet.ReadHalf();

            if (player is null)
                return;

            player.mouseRotationFromPlayer = rotation;
            player.mouseWorldDeltaFromPlayer = rotation.ToRotationVector2();

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
