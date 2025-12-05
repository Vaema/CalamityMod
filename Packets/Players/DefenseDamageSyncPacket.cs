using System.IO;
using CalamityMod.CalPlayer;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class DefenseDamageSyncPacket : CalamityPacket
    {
        public static DefenseDamageSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.DefenseDamageSync;

        public static void Send(CalamityPlayer playerToSync, int toClient = -1, int ignoreClient = -1)
        {
            if (playerToSync is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(playerToSync);
            packet.Write(playerToSync.totalDefenseDamage);
            packet.Write(playerToSync.defenseDamageRecoveryFrames);
            packet.Write(playerToSync.totalDefenseDamageRecoveryFrames);
            packet.Write(playerToSync.defenseDamageDelayFrames);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var totalDefDamage = packet.ReadInt32();
            var defDamageRecoverFrames = packet.ReadInt32();
            var totalDefDamageRecoverFrames = packet.ReadInt32();
            var defDamageDelayFrames = packet.ReadInt32();

            if (player is null)
                return;

            player.totalDefenseDamage = totalDefDamage;
            player.defenseDamageRecoveryFrames = defDamageRecoverFrames;
            player.totalDefenseDamageRecoveryFrames = totalDefDamageRecoverFrames;
            player.defenseDamageDelayFrames = defDamageDelayFrames;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
