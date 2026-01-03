using System.IO;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Packets
{
    internal sealed class CooldownAdditionPacket : CalamityPacket
    {
        public static CooldownAdditionPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.CooldownAddition;

        public static void Send(CalamityPlayer player, CooldownInstance cd, int toClient = -1, int ignoreClient = -1)
        {
            Send(player, cd.netID, cd.duration, cd.timeLeft, toClient, ignoreClient);
        }

        public static void Send(CalamityPlayer player, ushort netID, int duration, int timeLeft, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write(netID);
            packet.Write(duration);
            packet.Write(timeLeft);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var netID = packet.ReadUInt16();
            var duration = packet.ReadInt32();
            var timeLeft = packet.ReadInt32();

            if (player is null)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                CooldownInstance instance = new CooldownInstance(player.Player, netID, duration, timeLeft);

                // Actually assign this freshly synced cooldown to the appropriate player.
                string id = CooldownRegistry.registry[instance.netID].ID;
                player.cooldowns[id] = instance;
            }
            else if (Main.dedServ)
            {
                // Server should NOT handle cooldown itself!
                // Server doesn't have information for cooldown netIDs!
                Send(player, netID, duration, timeLeft, ignoreClient: sender);
            }
        }
    }
}
