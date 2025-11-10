using System;
using System.IO;
using System.Reflection;
using CalamityMod.Packets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod
{
    public class CalamityNetcode : ModSystem
    {
        private static CalamityPacket[] _PacketRegistry;

        public override void OnModLoad()
        {
            _PacketRegistry = new CalamityPacket[256]; // This should allow to use 0-255 range (full byte range)

            ReflectionHelper.IterateEveryModsTypes<CalamityPacket>(action: type =>
            {
                try
                {
                    if (Activator.CreateInstance(type) is not CalamityPacket packetHandler)
                        return;

                    var msgType = packetHandler.MessageType;
                    var existingHandler = _PacketRegistry[msgType];
                    if (existingHandler != null)
                    {
                        CalamityMod.Instance.Logger.Error($"Packet instance has already registered by other type!" +
                            $" [Failed On: '{type.FullName}'" +
                            $" Current Owner: '{existingHandler.GetType().FullName}'," +
                            $" msgTypeToRegister: '{msgType}']");
                        return;
                    }

                    _PacketRegistry[packetHandler.MessageType] = packetHandler;

                    var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                    if (instanceProperty is not null)
                    {
                        if (instanceProperty.PropertyType.IsAssignableFrom(type))
                        {
                            instanceProperty.SetValue(null, packetHandler);
                            packetHandler._Prop_Static_Instance = instanceProperty; // We saving this for Unload Steps
                        }
                        else
                        {
                            CalamityMod.Instance.Logger.Error($"Packet instance's 'Instance' property is not asssignable with given type!" +
                                $" [Failed On: '{type.FullName}']");
                        }
                    }

                    // We should not print error message if "Instance" property is missing
                    // Addons still can assign them with OnLoaded overload, and it's up to their implementation
                    // Still, Calamity's Standard is having "Instance" property for every packet types

                    packetHandler.OnLoaded();
                }
                catch (Exception e)
                {
                    CalamityMod.Instance.Logger.Error($"Exception was thrown while loading for Packets! {e}");
                    return;
                }
            });
        }

        public override void OnModUnload()
        {
            if (_PacketRegistry is not null)
            {
                foreach (var packetHandler in _PacketRegistry)
                {
                    if (packetHandler is null)
                        continue;

                    packetHandler.OnUnloaded();
                    packetHandler._Prop_Static_Instance?.SetValue(null, null);
                    packetHandler._Prop_Static_Instance = null;
                }

                _PacketRegistry = null;
            }
        }

        public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
        {
            try
            {
                CalamityModMessageType msgType = (CalamityModMessageType)reader.ReadByte();
                var packetHandler = _PacketRegistry[(byte)msgType];
                if (packetHandler is not null)
                {
                    packetHandler.HandlePacket(in reader, whoAmI);
                }
                else
                {
                    //
                    // Default case: with no idea how long the packet is, we can't safely read data.
                    // Throw an exception now instead of allowing the network stream to corrupt.
                    //

                    CalamityMod.Instance.Logger.Error($"Failed to parse Calamity packet: No Calamity packet exists with ID {msgType}.");
                    throw new Exception("Failed to parse Calamity packet: Invalid Calamity packet ID.");
                }
            }
            catch (Exception e)
            {
                if (e is EndOfStreamException eose)
                    CalamityMod.Instance.Logger.Error("Failed to parse Calamity packet: Packet was too short, missing data, or otherwise corrupt.", eose);
                else if (e is ObjectDisposedException ode)
                    CalamityMod.Instance.Logger.Error("Failed to parse Calamity packet: Packet reader disposed or destroyed.", ode);
                else if (e is IOException ioe)
                    CalamityMod.Instance.Logger.Error("Failed to parse Calamity packet: An unknown I/O error occurred.", ioe);
                else
                    throw; // this either will crash the game or be caught by TML's packet policing
            }
        }

        public static void SyncWorld()
        {
            if (Main.dedServ)
                NetMessage.SendData(MessageID.WorldData);
        }

        /// <summary>
        /// Shorthand Method for SyncNPC
        /// <code>
        /// This Equals to:
        /// 
        /// if (Main.dedServ and npc != null)
        ///     NetMessage.SendData(MessageID.SyncNPC, ...)
        /// </code>
        /// </summary>
        public static void SyncNPC(NPC npcToSync, int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            if (npcToSync is null)
                return;

            var npcWhoAmI = npcToSync.whoAmI;
            if (npcWhoAmI < 0 || npcWhoAmI >= Main.maxNPCs)
                return;

            NetMessage.SendData(MessageID.SyncNPC, toClient, ignoreClient, null, npcWhoAmI);
        }

        /// <summary>
        /// Shorthand Method for SyncNPC
        /// <code>
        /// This Equals to:
        /// 
        /// if (Main.dedServ and npcWhoAmI in valid range)
        ///     NetMessage.SendData(MessageID.SyncNPC, ...)
        /// </code>
        /// </summary>
        public static void SyncNPC(int npcWhoAmI, int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            if (npcWhoAmI < 0 || npcWhoAmI >= Main.maxNPCs)
                return;

            NetMessage.SendData(MessageID.SyncNPC, toClient, ignoreClient, null, npcWhoAmI);
        }

        public static void SyncCalamityWorldDifficulties(int sender)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            SyncDifficultiesPacket.Send();
        }

        public static void NewNPC_ClientSide(Vector2 spawnPosition, int npcType, Player player)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                NPC.NewNPC(new EntitySource_WorldEvent(), (int)spawnPosition.X, (int)spawnPosition.Y, npcType, Target: player.whoAmI);
                return;
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                SpawnNPCOnPlayerPacket.Send(player, (int)spawnPosition.X, (int)spawnPosition.Y, npcType);
            }
        }
    }

    public enum CalamityModMessageType : byte
    {
        // Player mechanic syncs
        DefenseDamageSync, // TODO -- this can't be synced every 60 frames, it needs to be synced when the player gets hit, or every time it heals up
        RageSync, // TODO -- this can't be synced every 60 frames, it needs to be synced every time the player is
        AdrenalineSync, // TODO -- this can't be synced every 60 frames, it needs to be synced every time the player is
        CooldownAddition,
        CooldownRemoval,
        SyncCooldownDictionary,
        ExaltationDirection,

        // Syncs for specific bosses or entities
        SyncDestroyerLaserColor,
        SyncCalamityNPCAIArray,
        SyncVanillaNPCLocalAIArray,
        SpawnSuperDummy,
        DeleteAllSuperDummies,
        SyncAndroombaSolution,
        SyncAndroombaAI,
        SyncSlabCrabAI,
        PlaceAltCritter,
        ProvidenceDyeConditionSync, // TODO -- this packetstorms if you hit Provi with spam weapons. It should ONLY send a packet if the status changes.
        PSCChallengeSync, // TODO -- once you've failed the PSC challenge this packetstorms

        // General things for entities
        SpawnNPCOnPlayer,
        SpawnBossOnPosition,
        SyncNPCMotionDataToServer,
        SyncNPCPosAndRotOnly,
        SyncNPCDemonicFlamesDamage,
        SyncNPCDemonSwordImpales,

        // Tile Entities
        PowerCellFactory,
        ChargingStationStandard,
        ChargingStationItemChange,
        Turret,
        LabHologramProjector,
        UpdateCodebreakerConstituents,
        UpdateCodebreakerContainedStuff,
        UpdateCodebreakerDecryptCountdown,
        UnlockAbyssChests,
        UpdateCanvasPainting,

        // Draedon Summoner
        CodebreakerSummonStuff,
        ExoMechSelection,

        // Boss Rush
        BossRushStage,
        BossRushStartTimer,
        BossRushEndTimer,
        EndBossRush,
        BRHostileProjKillSync, // TODO -- Simplify this. Only one packet needs be sent: "kill all hostile projectiles for N frames".

        // Acid Rain
        AcidRainSync,
        AcidRainOldDukeSummonSync,
        EncounteredOldDukeSync,

        // Mouse Controls syncs
        RightClickSync,
        MousePositionSync,

        // World state sync
        SyncDifficulties,
        SwitchToDifficulty,

        // Music events
        MusicEventSyncRequest,
        MusicEventSyncResponse,

        // Bandit Reforge Refund
        BanditStolenMoneySync,
        WantToRefundReforges,

        // Player Draw Effect Parameters
        SyncPlayerDrawParameter,

        Reserved = 150
    }
}
