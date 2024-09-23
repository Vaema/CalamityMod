using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            packet.WriteVector2(player.mouseWorld);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var mouseWorldPos = packet.ReadVector2();

            if (player is null)
                return;

            player.mouseWorld = mouseWorldPos;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
