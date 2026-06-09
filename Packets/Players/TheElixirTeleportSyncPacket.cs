using System.IO;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Packets
{
    internal class TheElixirTeleportSyncPacket : CalamityPacket
    {
        public static TheElixirTeleportSyncPacket Instance { get; private set; }

        public static void Send(Player player, int locationIndex = -1, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null || locationIndex == -1)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write(locationIndex);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var player = packet.ReadPlayer();
            var locationIndex = packet.ReadInt32();

            if (player is null)
                return;

            Vector2? location = null;
            if (locationIndex == 0)
                location = CalamityPlayer.GetDungeonArchivesTeleportPosition(player);
            if (locationIndex == 1)
                location = CalamityPlayer.GetAbyssVoidTeleportPosition(player);
            if (locationIndex == 2)
                location = CalamityPlayer.GetTempleTeleportPosition(player);

            if (!location.HasValue)
                return;

            player.AddBuff(BuffID.ChaosState, 300, false);
            player.AddBuff(BuffID.Cursed, 300, false);
            CalamityPlayer.ModTeleport(player, location.Value, false, TeleportationStyleID.RecallPotion);
        }
    }
}
