using System;
using System.IO;
using CalamityMod.CalPlayer;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class SyncPlayerDrawParameterPacket : CalamityPacket
    {
        public static SyncPlayerDrawParameterPacket Instance { get; set; }

        public static void Send(CalamityPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write((Half)player.drawingParameters.RoverShieldCharge); // 2b
            packet.Write((Half)player.drawingParameters.LunicShieldCharge); // 2b
            packet.Write((Half)player.drawingParameters.ProfanedShieldCharge); // 2b
            packet.WriteRGB(player.drawingParameters.ProfanedShieldColor); // 3b
            packet.Write((Half)player.drawingParameters.SpongeShieldCharge); // 2b
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var roverCharge = (float)packet.ReadHalf();
            var lunicCharge = (float)packet.ReadHalf();
            var profanedCharge = (float)packet.ReadHalf();
            var profanedColor = packet.ReadRGB();
            var spongeCharge = (float)packet.ReadHalf();

            if (player is null)
                return;

            player.drawingParameters.RoverShieldCharge = roverCharge;
            player.drawingParameters.LunicShieldCharge = lunicCharge;
            player.drawingParameters.ProfanedShieldCharge = profanedCharge;
            player.drawingParameters.ProfanedShieldColor = profanedColor;
            player.drawingParameters.SpongeShieldCharge = spongeCharge;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
