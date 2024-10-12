using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.BiomeManagers;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace CalamityMod.NPCs.SunkenSea
{
    /// <summary>
    /// An abstract class that gives an NPC the memebers needed to be a Sunken Sea NPC.<br/>
    /// These have the ability to hunt both players and NPCs, and also have a list of NPCs they hunt and avoid.
    /// </summary>
    public abstract class SunkenSeaNPC : ModNPC
    {
        #region Biome Designation

        [Flags]
        protected enum BiomeFlags : byte
        {
            None = 0,
            UndergroundDesert = 1,
            TimelessShores = 2,
            RadiantReefs = 4,
            PolypForest = 8,
            GleamingBurrows = 16,
            BasaltGully = 32,
        }

        protected abstract BiomeFlags BiomeDesignation { get; }

        protected readonly Dictionary<BiomeFlags, (Func<NPCSpawnInfo, bool> SpawnCondition, int BiomeType)> BiomeCorrespondentValues = new()
        {
            { BiomeFlags.UndergroundDesert, (spawnInfo => spawnInfo.Player.ZoneDesert, -1 /* None needed. */) },
            { BiomeFlags.TimelessShores, (spawnInfo => spawnInfo.Player.Calamity().ZoneTimelessShores, GetInstance<TimelessShoresBiome>().Type) },
            { BiomeFlags.RadiantReefs, (spawnInfo => spawnInfo.Player.Calamity().ZoneRadiantReefs, GetInstance<RadiantReefsBiome>().Type) },
            { BiomeFlags.PolypForest, (spawnInfo => spawnInfo.Player.Calamity().ZonePolypForest, GetInstance<PolypForestBiome>().Type) },
            { BiomeFlags.GleamingBurrows, (spawnInfo => spawnInfo.Player.Calamity().ZoneGleamingBurrows, GetInstance<GleamingBurrowsBiome>().Type) },
            { BiomeFlags.BasaltGully, (spawnInfo => spawnInfo.Player.Calamity().ZoneBasaltGully, GetInstance<BasaltGullyBiome>().Type) },
        };

        protected List<Func<NPCSpawnInfo, bool>> BiomeSpawnConditions { get; private set; } = new();

        #endregion

        #region Fields & Properties

        /// <summary>
        /// A list which stores the NPC's IDs that this creature hunts.
        /// </summary>
        protected abstract List<int> HuntNPCs { get; }

        /// <summary>
        /// A list which stores the NPC's IDs that this creature avoids.
        /// </summary>
        protected abstract List<int> AvoidNPCs { get; }

        protected abstract float SpawningChance { get; }

        /// <summary>
        /// Since a lot of these NPCs change behaviors a lot, this abstract class provides an <see cref="Action"/> property to store and trigger behaviors.<br/>
        /// Automatically resets <see cref="PathfindingPoints"/> and <see cref="PathTimer"/> to not cause errors.
        /// </summary>
        protected Action CurrentBehavior
        {
            get => _currentBehavior;
            set
            {
                PreviousBehavior = _currentBehavior;
                PathfindingPoints = null;
                PathPointIndex = 0;
                OnBehaviorChange(value);
                _currentBehavior = value;
                NetUpdate();
            }
        }
        private Action _currentBehavior;

        /// <summary>
        /// The previous behavior that was used for <see cref="CurrentBehavior"/>.
        /// </summary>
        protected Action PreviousBehavior { get; private set; }

        /// <summary>
        /// The current NPC that this creature has detected as prey.
        /// </summary>
        protected NPC CurrentPrey
        {
            get => _currentPrey;
            private set
            {
                if (value != _currentPrey && value != null)
                    OnPreyDetection(value);

                _currentPrey = value;
            }
        }
        private NPC _currentPrey;

        /// <summary>
        /// The current NPC that this creature has detected as a predator.
        /// </summary>
        protected NPC CurrentPredator
        {
            get => _currentPredator;
            private set
            {
                if (value != _currentPredator && value != null)
                    OnPredatorDetection(value);

                _currentPredator = value;
            }
        }
        private NPC _currentPredator;

        /// <summary>
        /// The current Player that this creature has detected.
        /// </summary>
        protected Player CurrentPlayer
        {
            get => _currentPlayer;
            private set
            {
                if (value != _currentPlayer && value != null)
                    OnPlayerDetection(value);

                _currentPlayer = value;
            }
        }
        private Player _currentPlayer;

        /// <summary>
        /// The nearest entity detected, could be an NPC or Player.
        /// </summary>
        protected Entity NearestEntity { get; private set; }

        /// <summary>
        /// Whether or not the creature has detected anything at all.
        /// </summary>
        protected bool HasAnyTargets => CurrentPrey != null || CurrentPredator != null || CurrentPlayer != null;

        /// <summary>
        /// The distance detection radius.<br/>
        /// Defaults to 300 pixels.
        /// </summary>
        protected float DetectionDistance { get; set; } = 300f;

        /// <summary>
        /// The direction of the cone used to detect targets.<br/>
        /// Defaults to <see cref="Vector2.UnitX"/> times the direction.<br/>
        /// When set, it normalizes the vector.
        /// </summary>
        protected Vector2 ConeDetectionDirection
        {
            get => _coneDetectionDirection;
            set
            {
                _coneDetectionDirection = value.SafeNormalize(Vector2.UnitX * NPC.direction);
                NetUpdate();
            }
        }
        private Vector2 _coneDetectionDirection;

        /// <summary>
        /// How open the cone detection is -- given an angle.<br/>
        /// Defaults to 45 degrees.
        /// </summary>
        protected float ConeDetectionAngle { get; set; } = MathHelper.ToRadians(45f);

        /// <summary>
        /// When a this creature has lost sight of a target, it'll still have a sense of permanence.<br/>
        /// This means that, even if it can't see it, it can know it's there.<br/>
        /// This property states how much seconds it can know it's there.<br/>
        /// Defaults to 3 seconds.
        /// </summary>
        protected virtual float PermanenceSenseTime => CalamityUtils.SecondsToFrames(3f);

        private int _permanenceSenseTimer;

        /// <summary>
        /// A lot of these Sunken Sea creatures use the <see cref="CalamityUtils.AStar"/> pathfinding algorithm,<br/>
        /// so this abstract class provides a <see cref="List{T}"/> so you can store the paths.<br/>
        /// Defaults to <see langword="null"/>.
        /// </summary>
        protected List<Vector2> PathfindingPoints { get; set; }

        protected int PathPointIndex { get; set; }

        private bool _hasSpawned;

        #endregion

        #region ModNPC Overrides

        public override void SetStaticDefaults()
        {
            NPCID.Sets.UsesNewTargetting[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
        }

        public override void SetDefaults()
        {
            List<int> biomeTypes = [];
            foreach (var flag in Enum.GetValues<BiomeFlags>())
            {
                if (flag == BiomeFlags.None || !BiomeDesignation.HasFlag(flag))
                    continue;
                BiomeSpawnConditions.Add(BiomeCorrespondentValues[flag].SpawnCondition);
                if (flag == BiomeFlags.UndergroundDesert)
                    continue;
                biomeTypes.Add(BiomeCorrespondentValues[flag].BiomeType);
            }
            SpawnModBiomes = [.. biomeTypes];
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.Info.AddRange([new FlavorTextBestiaryInfoElement($"Mods.CalamityMod.Bestiary.{Name}")]);

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (BiomeSpawnConditions.Any(f => f.Invoke(spawnInfo)) && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
                return SpawningChance;
            return 0f;
        }

        public sealed override void AI()
        {
            if (!_hasSpawned)
            {
                _coneDetectionDirection = Vector2.UnitX * NPC.direction;
                BehaviorOnSpawn();
                _hasSpawned = true;
                NetUpdate();
            }

            UpdateTargets();

            CreatureAI();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Same function as OnSpawn, but this one actually syncs to the server.
        /// </summary>
        protected virtual void BehaviorOnSpawn() { }

        /// <summary>
        /// A method that is called on <see cref="AI"/> every frame, to put your actual enemy AI.
        /// </summary>
        protected virtual void CreatureAI() { }

        /// <summary>
        /// A method that is triggered when this NPC's behavior is changed.
        /// </summary>
        protected virtual void OnBehaviorChange(Action newBehavior) { }

        /// <summary>
        /// A custom method for when this NPC is hit by another NPC.
        /// </summary>
        /// <param name="attacker"></param>
        public virtual void OnHitByNPC(NPC attacker) { }

        /// <summary>
        /// A method that is triggered when this creature has detected a prey.
        /// </summary>
        protected virtual void OnPreyDetection(NPC prey) { }

        /// <summary>
        /// A method that is triggered when this creature has detected a predator.
        /// </summary>
        protected virtual void OnPredatorDetection(NPC predator) { }

        /// <summary>
        /// A method that is triggered when this creature detects a player.
        /// </summary>
        protected virtual void OnPlayerDetection(Player player) { }

        /// <summary>
        /// Whether or not a detected player is valid to be set as a target given some conditions.
        /// </summary>
        protected virtual bool PlayerSearchFilter(Player p)
        {
            if (!HasLineOfSight(p.Center))
                return false;

            bool isInsideCone = Vector2.Dot(_coneDetectionDirection, NPC.DirectionTo(p.Center)) >= MathF.Cos(ConeDetectionAngle) && NPC.DistanceSQ(p.Center) < DetectionDistance * DetectionDistance;
            bool isInsideCirlceWhenAware = HasAnyTargets && NPC.DistanceSQ(p.Center) <= DetectionDistance * DetectionDistance;
            if (isInsideCone || isInsideCirlceWhenAware)
                return true;

            return false;
        }

        /// <summary>
        /// Whether or not a detected NPC is valid to be set as a target given some conditions.
        /// </summary>
        protected virtual bool NPCSearchFilter(NPC n)
        {
            if (!HasLineOfSight(n.Center))
                return false;

            bool isInsideCone = Vector2.Dot(_coneDetectionDirection, NPC.DirectionTo(n.Center)) >= MathF.Cos(ConeDetectionAngle) && NPC.DistanceSQ(n.Center) < DetectionDistance * DetectionDistance;
            bool isInsideCirlceWhenAware = HasAnyTargets && NPC.DistanceSQ(n.Center) <= DetectionDistance * DetectionDistance;
            if (isInsideCone || isInsideCirlceWhenAware)
                return true;

            return false;
        }

        /// <summary>
        /// A method that finds and updates the current detected targets of this creature.
        /// </summary>
        protected void UpdateTargets()
        {
            var searchResults = SearchForTarget(NPC, playerFilter: PlayerSearchFilter, npcFilter: NPCSearchFilter);
            if (searchResults.FoundTarget)
            {
                CurrentPlayer = searchResults.NearestTankOwner;
                NearestEntity = searchResults.NearestTankOwner;

                if (searchResults.FoundNPC)
                {
                    if (AvoidNPCs.Contains(searchResults.NearestNPC.type))
                        CurrentPredator = searchResults.NearestNPC;

                    else if (HuntNPCs.Contains(searchResults.NearestNPC.type))
                        CurrentPrey = searchResults.NearestNPC;

                    if (searchResults.NearestNPCDistance < searchResults.NearestTankDistance)
                        NearestEntity = searchResults.NearestNPC;
                }

                NetUpdate();
            }
            else
            {
                _permanenceSenseTimer++;
                if (_permanenceSenseTimer > PermanenceSenseTime)
                {
                    CurrentPlayer = null;
                    NearestEntity = null;
                    CurrentPredator = null;
                    CurrentPrey = null;
                    _permanenceSenseTimer = 0;
                }
            }
        }

        #endregion

        #region Helper Methods

        protected bool HasPath => PathfindingPoints is not null;

        protected bool IsPointAbleToNavigate(Point point) =>
            Main.tile[point].LiquidAmount > 125 && Main.tile[point].LiquidType == LiquidID.Water && NPC.DoesEntityFitInPath(point, fluffX: 16, fluffY: 16);

        protected void SunkenSeaPathfinding(Vector2 goal) => PathfindingPoints = NPC.Center.DoPathfinding(goal, IsPointAbleToNavigate);

        protected void SunkenSeaPathfinding() => PathfindingPoints = NPC.Center.DoPathfinding(tileValidation: IsPointAbleToNavigate);

        /// <summary>
        /// A quickhand method to follow a path found.
        /// </summary>
        protected void GenericPathFollowing(float acceleration)
        {
            Vector2 currentlyFollowedPathPoint = PathfindingPoints[PathPointIndex];

            NPC.velocity += NPC.DirectionTo(currentlyFollowedPathPoint) * acceleration;

            if (NPC.DistanceSQ(currentlyFollowedPathPoint) < 48f * 48f)
            {
                PathPointIndex++;
                PathPointIndex = (int)MathHelper.Clamp(PathPointIndex, 0, PathfindingPoints.Count - 1);
            }

            if (PathPointIndex == PathfindingPoints.Count - 1)
            {
                PathPointIndex = 0;
                PathfindingPoints = null;
            }
        }

        /// <summary>
        /// A quick method to check whether this NPC has line of sight with something.
        /// </summary>
        protected bool HasLineOfSight(Vector2 position) => Collision.CanHitLine(NPC.Center, 0, 0, position, 0, 0);

        /// <summary>
        /// Spawns an emote particle.
        /// </summary>
        protected void React(Color emoteColor, EmoteExpressionParticle.EmoteType emoteImage, SoundStyle? sound = null)
        {
            if (!Main.dedServ)
            {
                var emoteDirection = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 3f);
                Particle emote = new EmoteExpressionParticle(
                    NPC.Center + emoteDirection * 2f,
                    emoteDirection,
                    2.2f,
                    emoteColor,
                    Main.rand.Next(30, 46),
                    emoteImage);
                GeneralParticleHandler.SpawnParticle(emote);

                if (sound is not null)
                    SoundEngine.PlaySound(sound, NPC.Center);
            }
        }

        #endregion

        #region Syncing

        /// <summary>
        /// A quick method to both do <see cref="NPC.netUpdate"/> and set <see cref="NPC.netSpam"/>.<br/>
        /// Defaults <see cref="NPC.netSpam"/> to 0.
        /// </summary>
        protected void NetUpdate(int netSpam = 0)
        {
            NPC.netUpdate = true;
            NPC.netSpam = netSpam;
        }

        public sealed override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(CurrentPrey.whoAmI);
            writer.Write7BitEncodedInt(CurrentPredator.whoAmI);
            writer.Write7BitEncodedInt(CurrentPlayer.whoAmI);
            writer.Write7BitEncodedInt(NearestEntity.whoAmI);

            writer.Write(DetectionDistance);
            writer.WritePackedVector2(_coneDetectionDirection);
            writer.Write(ConeDetectionAngle);
            writer.Write7BitEncodedInt(_permanenceSenseTimer);

            writer.Write7BitEncodedInt(PathfindingPoints.Count);
            foreach (var point in PathfindingPoints)
                writer.WritePackedVector2(point);
            writer.Write7BitEncodedInt(PathPointIndex);

            writer.Write(_hasSpawned);

            SendMoreExtraAI(writer);
        }

        public sealed override void ReceiveExtraAI(BinaryReader reader)
        {
            CurrentPrey.whoAmI = reader.Read7BitEncodedInt();
            CurrentPredator.whoAmI = reader.Read7BitEncodedInt();
            CurrentPlayer.whoAmI = reader.Read7BitEncodedInt();
            NearestEntity.whoAmI = reader.Read7BitEncodedInt();

            DetectionDistance = reader.ReadSingle();
            _coneDetectionDirection = reader.ReadPackedVector2();
            ConeDetectionAngle = reader.ReadSingle();
            _permanenceSenseTimer = reader.Read7BitEncodedInt();

            int count = reader.Read7BitEncodedInt();
            for (int i = 0; i < count; i++)
                PathfindingPoints.Add(reader.ReadPackedVector2());
            PathPointIndex = reader.Read7BitEncodedInt();

            _hasSpawned = reader.ReadBoolean();

            ReceiveMoreExtraAI(reader);
        }

        /// <summary>
        /// Same function as <see cref="SendExtraAI(BinaryWriter)"/>, but since that one already sends information and to avoid accidents,<br/>
        /// <see cref="SendExtraAI(BinaryWriter)"/> is <see langword="sealed"/> and instead this is available.
        /// </summary>
        protected virtual void SendMoreExtraAI(BinaryWriter writer) { }

        /// <summary>
        /// Same function as <see cref="ReceiveExtraAI(BinaryReader)"/>, but since that one already sends information and to avoid accidents,<br/>
        /// <see cref="ReceiveExtraAI(BinaryReader)"/> is <see langword="sealed"/> and instead this is available.
        /// </summary>
        protected virtual void ReceiveMoreExtraAI(BinaryReader reader) { }

        #endregion
    }
}
