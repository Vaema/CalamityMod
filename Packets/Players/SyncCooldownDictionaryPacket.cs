using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using Terraria;
using Terraria.ID;

using CooldownInfoTuple = (ushort netID, int duration, int timeLeft);

namespace CalamityMod.Packets
{
    public sealed class SyncCooldownDictionaryPacket : CalamityPacket
    {
        public static SyncCooldownDictionaryPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.SyncCooldownDictionary;

        public static void Send(CalamityPlayer player, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            var cooldowns = player.cooldowns.Values.Select(cd => (cd.netID, cd.duration, cd.timeLeft));
            Send(player, cooldowns, toClient, ignoreClient);
        }

        public static void Send(CalamityPlayer player, IEnumerable<CooldownInfoTuple> cooldowns, int toClient = -1, int ignoreClient = -1)
        {
            if (player is null)
                return;

            if (cooldowns is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(player);
            packet.Write(player.cooldowns.Count);
            foreach (CooldownInfoTuple cd in cooldowns)
            {
                packet.Write(cd.netID);
                packet.Write(cd.duration);
                packet.Write(cd.timeLeft);
            }
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var player = packet.ReadCalamityPlayer();
            int count = packet.ReadInt32();

            Dictionary<ushort, CooldownInfoTuple> receivedCooldowns = new(count);
            for (int i = 0; i < count; ++i)
            {
                var netID = packet.ReadUInt16();
                var duration = packet.ReadInt32();
                var timeLeft = packet.ReadInt32();
                receivedCooldowns[netID] = (netID, duration, timeLeft);
            }

            if (player is null)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                HashSet<ushort> localIDs = [.. player.cooldowns.Values.Select(cd => cd.netID)];
                HashSet<ushort> receivedIDs = [.. receivedCooldowns.Keys];

                HashSet<ushort> combinedIDSet = [];
                combinedIDSet.UnionWith(localIDs);
                combinedIDSet.UnionWith(receivedIDs);

                foreach (ushort netID in combinedIDSet)
                {
                    bool existsLocally = localIDs.Contains(netID);
                    bool existsRemotely = receivedIDs.Contains(netID);
                    string id = CooldownRegistry.registry[netID].ID;

                    // Exists locally but not remotely = cull -- destroy the local copy.
                    if (existsLocally && !existsRemotely)
                    {
                        player.cooldowns.Remove(id);
                    }
                    // Exists remotely but not locally = add -- insert into the dictionary.
                    else if (existsRemotely && !existsLocally)
                    {
                        var cdToAdd = receivedCooldowns[netID];
                        player.cooldowns[id] = new CooldownInstance(player.Player, cdToAdd.netID, cdToAdd.duration, cdToAdd.timeLeft);
                    }
                    // Exists in both places = update -- update timing fields but don't replace the instance.
                    else if (existsLocally && existsRemotely)
                    {
                        CooldownInstance localInstance = player.cooldowns[id];
                        localInstance.duration = receivedCooldowns[netID].duration;
                        localInstance.timeLeft = receivedCooldowns[netID].timeLeft;
                    }
                }
            }
            else if (Main.dedServ)
            {
                // Server should NOT handle cooldown itself!
                // Server doesn't have information for cooldown netIDs!
                Send(player, receivedCooldowns.Values, ignoreClient: sender);
            }
        }
    }
}
