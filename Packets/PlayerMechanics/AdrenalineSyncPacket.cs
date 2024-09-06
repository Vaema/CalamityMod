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
    public sealed class AdrenalineSyncPacket : CalamityPacket
    {
        public static AdrenalineSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.AdrenalineSync;

        public static void Send(CalamityPlayer playerToSync, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(playerToSync.Player);
            packet.Write(playerToSync.adrenaline);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var adrenaline = packet.ReadSingle();

            if (player is null)
                return;

            player.adrenaline = adrenaline;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
