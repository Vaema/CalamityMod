using System.IO;
using CalamityMod.CalPlayer;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class ExaltationDirectionSyncPacket : CalamityPacket
    {
        public static ExaltationDirectionSyncPacket Instance { get; private set; }

        public static void Send(CalamityPlayer playerToSync, int toClient = -1, int ignoreClient = -1)
        {
            if (playerToSync is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(playerToSync);
            packet.Write(playerToSync.InvertExaltationLineRotationDirections);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var invertDir = packet.ReadBoolean();

            if (player is null)
                return;

            player.InvertExaltationLineRotationDirections = invertDir;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
