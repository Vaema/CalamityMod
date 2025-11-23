using System;
using System.IO;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
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

            var pos = player.mouseWorld;
            int tileX = Math.DivRem((int)pos.X, 16, out int remX);
            int tileY = Math.DivRem((int)pos.Y, 16, out int remY);
            byte remByte = (byte)(remX << 4 | remY);
            packet.Write((ushort)Math.Clamp(tileX, 0, ushort.MaxValue)); // If you actually have world size above 65535 tiles in axis, Good luck on that
            packet.Write((ushort)Math.Clamp(tileY, 0, ushort.MaxValue));
            packet.Write(remByte);

            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var tileX = (int)packet.ReadUInt16();
            var tileY = (int)packet.ReadUInt16();
            var remByte = packet.ReadByte();
            var remX = remByte >> 4;
            var remY = remByte & 0b1111;

            var mouseWorldPos = new Vector2((tileX * 16) + remX, (tileY * 16) + remY);

            if (player is null)
                return;

            player.mouseWorld = mouseWorldPos;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
