using System.IO;
using CalamityMod.CalPlayer;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class RageSyncPacket : CalamityPacket
    {
        public static RageSyncPacket Instance { get; private set; }

        public static void Send(CalamityPlayer playerToSync, int toClient = -1, int ignoreClient = -1)
        {
            if (playerToSync is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(playerToSync);
            packet.Write(playerToSync.rage);
            packet.Write(playerToSync.rageCombatFrames);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var rage = packet.ReadSingle();
            var rageCombatFrames = packet.ReadInt32();

            if (player is null)
                return;

            player.rage = rage;
            player.rageCombatFrames = rageCombatFrames;

            if (Main.dedServ)
                Send(player, ignoreClient: sender);
        }
    }
}
