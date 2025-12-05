using System.IO;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using Terraria.ID;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class CooldownRemovalPacket : CalamityPacket
    {
        public static CooldownRemovalPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.CooldownRemoval;

        public static void Send(CalamityPlayer player, ushort[] netIDsToRemove, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write(netIDsToRemove.Length);
            for (int i = 0; i < netIDsToRemove.Length; i++)
            {
                packet.Write(netIDsToRemove[i]);
            }
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            var count = packet.ReadInt32();
            var netIDsToRemove = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                netIDsToRemove[i] = packet.ReadUInt16();
            }

            if (player is null)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                foreach (var netID in netIDsToRemove)
                {
                    player.cooldowns.Remove(CooldownRegistry.registry[netID].ID);
                }
            }
            else if (Main.dedServ)
            {
                // Server should NOT handle cooldown itself!
                // Server doesn't have information for cooldown netIDs!
                Send(player, netIDsToRemove, ignoreClient: sender);
            }
        }
    }
}
