using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CalamityMod.Events;
using CalamityMod.Items;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.NPCs;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Packets;
using CalamityMod.Systems;
using CalamityMod.TileEntities;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace CalamityMod
{
    public class CalamityNetcode : ModSystem
    {
        private static CalamityPacket[] _PacketRegistry;

        public override void OnModLoad()
        {
            _PacketRegistry = new CalamityPacket[256]; // This should allow to use 0-255 range (full byte range)

            var types = ModLoader.Mods.SelectMany(mod => AssemblyManager.GetLoadableTypes(mod.Code));
            foreach (var type in types)
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(CalamityPacket)))
                    continue;

                if (Activator.CreateInstance(type) is not CalamityPacket packetHandler)
                    continue;

                var msgType = packetHandler.MessageType;
                var existingHandler = _PacketRegistry[msgType];
                if (existingHandler != null)
                {
                    CalamityMod.Instance.Logger.Error($"Packet instance has already registered by other type!" +
                        $" [Failed: '{type.FullName}'" +
                        $" Current Owner: '{existingHandler.GetType().FullName}'," +
                        $" msgTypeToRegister: '{msgType}']");
                    continue;
                }

                _PacketRegistry[packetHandler.MessageType] = packetHandler;

                var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (instanceProperty?.PropertyType.IsAssignableFrom(type) ?? false)
                {
                    instanceProperty.SetValue(null, packetHandler);
                    packetHandler._Prop_Static_Instance = instanceProperty;
                }

                packetHandler.OnLoaded();
            }
        }

        public override void OnModUnload()
        {
            foreach (var packetHandler in _PacketRegistry ?? Enumerable.Empty<CalamityPacket>())
            {
                if (packetHandler is null)
                    continue;

                packetHandler.OnUnloaded();
                packetHandler._Prop_Static_Instance?.SetValue(null, null);
                packetHandler._Prop_Static_Instance = null;
            }

            _PacketRegistry = null;
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
                    CalamityMod.Instance.Logger.Error($"Failed to parse Calamity packet: No Calamity packet exists with ID {msgType}.");
                    throw new Exception("Failed to parse Calamity packet: Invalid Calamity packet ID.");
                }

                switch (msgType)
                {
                    //
                    // Player mechanic syncs
                    //
                    case CalamityModMessageType.CooldownAddition:
                        Main.player[reader.ReadInt32()].Calamity().HandleCooldownAddition(reader);
                        break;
                    case CalamityModMessageType.CooldownRemoval:
                        Main.player[reader.ReadInt32()].Calamity().HandleCooldownRemoval(reader);
                        break;
                    case CalamityModMessageType.SyncCooldownDictionary:
                        Main.player[reader.ReadInt32()].Calamity().HandleCooldownDictionary(reader);
                        break;

                    //
                    // Tile Entities
                    //

                    case CalamityModMessageType.UnlockAbyssChests:
                        Abyss.UnlockAllAbyssChests();
                        break;
                    case CalamityModMessageType.PowerCellFactory:
                        TEPowerCellFactory.ReadSyncPacket(mod, reader);
                        break;
                    case CalamityModMessageType.ChargingStationStandard:
                        TEChargingStation.ReadSyncPacket(mod, reader);
                        break;
                    case CalamityModMessageType.ChargingStationItemChange:
                        TEChargingStation.ReadItemSyncPacket(mod, reader);
                        break;
                    case CalamityModMessageType.Turret:
                        TEBaseTurret.ReadSyncPacket(mod, reader);
                        break;
                    case CalamityModMessageType.LabHologramProjector:
                        TELabHologramProjector.ReadSyncPacket(mod, reader);
                        break;
                    case CalamityModMessageType.UpdateCodebreakerConstituents:
                        TECodebreaker.ReadConstituentsUpdateSync(mod, reader);
                        break;
                    case CalamityModMessageType.UpdateCodebreakerContainedStuff:
                        TECodebreaker.ReadContainmentSync(mod, reader);
                        break;
                    case CalamityModMessageType.UpdateCodebreakerDecryptCountdown:
                        TECodebreaker.ReadDecryptCountdownSync(mod, reader);
                        break;

                    //
                    // Boss Rush
                    //

                    case CalamityModMessageType.BossRushStage:
                        int stage = reader.ReadInt32();
                        BossRushEvent.BossRushStage = stage;
                        break;
                    case CalamityModMessageType.BossRushStartTimer:
                        BossRushEvent.StartTimer = reader.ReadInt32();
                        break;
                    case CalamityModMessageType.BossRushEndTimer:
                        BossRushEvent.EndTimer = reader.ReadInt32();
                        break;
                    case CalamityModMessageType.EndBossRush:
                        BossRushEvent.EndEffects();
                        break;
                    case CalamityModMessageType.BRHostileProjKillSync:
                        int countdown3 = reader.ReadInt32();
                        BossRushEvent.HostileProjectileKillCounter = countdown3;
                        break;

                    //
                    // Acid Rain
                    //

                    case CalamityModMessageType.AcidRainSync:
                        AcidRainEvent.AcidRainEventIsOngoing = reader.ReadBoolean();
                        AcidRainEvent.AccumulatedKillPoints = reader.ReadInt32();
                        AcidRainEvent.TimeSinceLastAcidRainKill = reader.ReadInt32();
                        break;
                    case CalamityModMessageType.AcidRainOldDukeSummonSync:
                        AcidRainEvent.HasTriedToSummonOldDuke = reader.ReadBoolean();
                        break;
                    case CalamityModMessageType.EncounteredOldDukeSync:
                        AcidRainEvent.OldDukeHasBeenEncountered = reader.ReadBoolean();
                        break;

                    //
                    // Draedon Summoner stuff
                    //
                    case CalamityModMessageType.CodebreakerSummonStuff:
                        CalamityWorld.DraedonSummonCountdown = reader.ReadInt32();
                        CalamityWorld.DraedonSummonPosition = reader.ReadVector2();
                        CalamityWorld.DraedonMechdusa = reader.ReadBoolean();
                        break;
                    case CalamityModMessageType.ExoMechSelection:
                        CalamityWorld.DraedonMechToSummon = (ExoMech)reader.ReadInt32();
                        break;

                    //
                    // Mouse control syncs
                    //

                    case CalamityModMessageType.RightClickSync:
                        Main.player[reader.ReadInt32()].Calamity().HandleRightClick(reader);
                        break;
                    case CalamityModMessageType.MousePositionSync:
                        Main.player[reader.ReadInt32()].Calamity().HandleMousePosition(reader);
                        break;

                    //
                    // Music event syncs
                    //
                    case CalamityModMessageType.MusicEventSyncRequest:
                        MusicEventSystem.FulfillSyncRequest(whoAmI);
                        break;

                    case CalamityModMessageType.MusicEventSyncResponse:
                        MusicEventSystem.ReceiveSyncResponse(reader);
                        break;

                    //
                    // Default case: with no idea how long the packet is, we can't safely read data.
                    // Throw an exception now instead of allowing the network stream to corrupt.
                    //
                    default:
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
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        /// <summary>
        /// Shorthand for NetMessage.SendData(MessageID.SyncNPC)
        /// </summary>
        public static void SyncNPC(NPC npcToSync, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            if (npcToSync is null)
                return;

            NetMessage.SendData(MessageID.SyncNPC, toClient, ignoreClient, null, npcToSync.whoAmI);
        }

        /// <summary>
        /// Shorthand for NetMessage.SendData(MessageID.SyncNPC)
        /// </summary>
        public static void SyncNPC(int npcWhoAmI, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
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

            var netMessage = CalamityMod.Instance.GetPacket();
            netMessage.Write((byte)CalamityModMessageType.SpawnNPCOnPlayer);
            netMessage.Write((int)spawnPosition.X);
            netMessage.Write((int)spawnPosition.Y);
            netMessage.Write(npcType);
            netMessage.Write(player.whoAmI);
            netMessage.Send();
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

        // Music events
        MusicEventSyncRequest,
        MusicEventSyncResponse,
        
        // Bandit Reforge Refund
        ScammedByTinkerer,
        WantToRefundReforges,

        Reserved = 150
    }
}
