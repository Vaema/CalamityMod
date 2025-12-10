using System;
using System.IO;
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
        private static CalamityPacket[] _PacketRegistry = new CalamityPacket[256]; // This should allow to use 0-255 range (full byte range)

        internal static void RegisterHandler(CalamityPacket handler)
        {
            var msgType = handler.MessageType;
            var existingHandler = _PacketRegistry[msgType];

            if (existingHandler != null)
            {
                CalamityMod.Log.Error($"Packet instance has already registered by other type!" +
                    $" [Failed On: '{handler.GetType().FullName}'" +
                    $" Current Owner: '{existingHandler.GetType().FullName}'," +
                    $" msgTypeToRegister: '{msgType}']");
                return;
            }

            _PacketRegistry[msgType] = handler;
        }

        public override void OnModUnload()
        {
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
                    //
                    // Default case: with no idea how long the packet is, we can't safely read data.
                    // Throw an exception now instead of allowing the network stream to corrupt.
                    //

                    CalamityMod.Log.Error($"Failed to parse Calamity packet: No Calamity packet exists with ID {msgType}.");
                    throw new Exception("Failed to parse Calamity packet: Invalid Calamity packet ID.");
                }
            }
            catch (Exception e)
            {
                if (e is EndOfStreamException eose)
                    CalamityMod.Log.Error("Failed to parse Calamity packet: Packet was too short, missing data, or otherwise corrupt.", eose);
                else if (e is ObjectDisposedException ode)
                    CalamityMod.Log.Error("Failed to parse Calamity packet: Packet reader disposed or destroyed.", ode);
                else if (e is IOException ioe)
                    CalamityMod.Log.Error("Failed to parse Calamity packet: An unknown I/O error occurred.", ioe);
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
        MouseRotationSync,
        MousePositionSync,

        // World state sync
        SwitchToDifficulty,

        // Music events
        MusicEventSyncRequest,
        MusicEventSyncResponse,

        // Bandit Reforge Refund
        BanditStolenMoneySync,
        WantToRefundReforges,

        // Player Draw Effect Parameters
        SyncPlayerDrawParameter
    }
}
