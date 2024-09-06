using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class RightClickSyncPacket : CalamityPacket
    {
        public static RightClickSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.RightClickSync;

        public static void Send(CalamityPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write(player.mouseRight);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var rightClick = packet.ReadBoolean();

            if (player is null)
                return;

            player.mouseRight = rightClick;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
