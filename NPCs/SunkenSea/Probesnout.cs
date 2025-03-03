using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Probesnout : SunkenSeaNPC
    {
        #region Members

        public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 200;
        public static int IdleMaxPathDistance = 400;

        public static int FleeTileAnticipationDistance = 5 * 16;
        public static int FleeMinPathDistance = 80;
        public static int FleeMaxPathDistance = 160;

        protected override List<int> PreyIDs =>
        [
            // NPCType<Slugbun>(),
        ];

        protected override List<int> PredatorIDs =>
        [
            // NPCType<IlmerianAxolotl>(),
            NPCType<Sharkoon>(),
            // NPCType<Polyperil>(),
            // NPCType<CrestedStalker>(),
            // NPCType<Hermititan>(),
        ];

        private enum AnimationState { Idle, Eating }

        private AnimationState Animation
        {
            get => (AnimationState)NPC.ai[1];
            set
            {
                if (value != Animation)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                }

                switch (value)
                {
                    case AnimationState.Idle:
                        AnimationFrames = 8;
                        TimePerAnimationFrame = 5;
                        break;
                    case AnimationState.Eating:
                        AnimationFrames = 12;
                        TimePerAnimationFrame = 7;
                        break;
                }

                NPC.ai[1] = (float)value;
            }
        }

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs;

        private int AnimationFrames = 8;

        private int TimePerAnimationFrame = 5;

        private Vector2 ScaleSquish = Vector2.One;

        private Action _currentBehavior;
        private Action CurrentBehavior
        {
            get => _currentBehavior;
            set
            {
                _previousBehavior = _currentBehavior;
                _currentBehavior = value;
            }
        }

        private Action _previousBehavior;

        private int HuntCooldown
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        public Vector2 Position => NPC.Center;

        public Vector2 Velocity { get => NPC.velocity; set => NPC.velocity = value; }

        public float Acceleration => 0.4f;

        public float MaxSpeed => 8f;

        private Vector2 SpongeFoundPosition;

        private bool HasEatenSponge;

        #endregion

        #region AI

        protected override bool PlayerSearchFilter(Player p)
        {
            return base.PlayerSearchFilter(p) || p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 960f * 960f;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || (n == CurrentPrey || n == CurrentPredator) && Vector2.DistanceSquared(NPC.Center, n.Center) < 960f * 960f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            CurrentBehavior = IdlingBehavior;
            NPC.spriteDirection = Main.rand.NextBool().ToDirectionInt();
            NPC.GravityMultiplier *= 2f;
            NPC.MaxFallSpeedMultiplier *= 2f;
            pathfinding = new PathfindingManager(NPC)
            {
                Acceleration = 0.4f,
                MaxSpeed = 8f,
                MinimumPointDistance = 60f
            };
        }

        public override void AI()
        {
            CurrentBehavior?.Invoke();

            NPC.rotation = MathHelper.ToRadians(NPC.velocity.X * 3f);
            if (CurrentBehavior == EatingBehavior && Vector2.DistanceSquared(NPC.Center, SpongeFoundPosition) <= 64f * 64f)
                NPC.spriteDirection = NPC.direction = MathF.Sign(SpongeFoundPosition.X - NPC.Center.X);
            else if (NPC.velocity.LengthSquared() != 0f)
                NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);

            if (Main.rand.NextBool(50) && !HasEatenSponge && CurrentBehavior == IdlingBehavior)
                ThreadPool.QueueUserWorkItem(_ => DetectSponges(), null);

            if (!NPC.wet && CurrentBehavior != OutsideWaterBehavior)
            {
                NPC.noGravity = false;
                CurrentBehavior = OutsideWaterBehavior;
            }

            if (ScaleSquish.Y > 1f)
                ScaleSquish.Y = Math.Max(1f, ScaleSquish.Y - 0.025f);
        }

        protected override void OnPreyDetection(NPC prey)
        {
            // If it's not a small fish and it's not being chased by a predator, the Probesnout may hunt.
            if (CurrentPredator != null)
                return;

            CurrentBehavior = HuntBehavior;
            ScaleSquish.Y += 0.4f;
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            // Regardless of anything, if it detects a predator, time to run.
            CurrentBehavior = FleeingBehavior;
            ScaleSquish.Y += 0.4f;
        }

        protected override void OnPlayerDetection(Player player)
        {
            if (CurrentPredator != null)
                return;

            ScaleSquish.Y += 0.4f;
        }

        private void IdlingBehavior()
        {
            // At random, the mob will choose a random nearby point and pathfind there.
            pathfinding.FindPath(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(500f, 3000f), SunkenSeaTileValidity));
        }

        private void FleeingBehavior()
        {
            // If the avoided entity is gone, go back to idling.
            if (CurrentPredator == null)
            {
                CurrentBehavior = IdlingBehavior;
                return;
            }

            // While it doesn't have any obstacles in front of it, run away in a straight line.
            // Try to manuever if there are any obstacles.
            var headedDirection = CurrentPredator.DirectionTo(NPC.Center) * 200f;
            bool tileNotWater = GetIntersectingPointsInLine(NPC.Center, NPC.Center + headedDirection).Any(point => Main.tile[point].IsTileSolid() || Main.tile[point].LiquidAmount < 255);

            if (tileNotWater)
            {
                do
                {
                    var randomNormalDirection = headedDirection.RotatedBy(MathHelper.PiOver2 * Main.rand.NextBool().ToDirectionInt());
                    _randomPathPoint = randomNormalDirection.RotatedByRandom(MathHelper.PiOver4);
                    if (!Main.tile[(NPC.Center + _randomPathPoint).ToTileCoordinates()].IsTileSolid())
                        break;
                }
                while (!Main.tile[(NPC.Center + _randomPathPoint).ToTileCoordinates()].IsTileSolid());
                NPC.netUpdate = true;
                pathfinding.FindPath(new(NPC.Center, NPC.Center + _randomPathPoint, SunkenSeaTileValidity));
            }
            else
            {
                NPC.velocity += NPC.DirectionFrom(CurrentPredator.Center) * Acceleration;

                // Cap the speed if MaxSpeed has been surpassed.
                if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
            }
        }

        private void HuntBehavior()
        {
            // If there's no more prey, go back to idling.
            if (CurrentPrey is null)
            {
                CurrentBehavior = IdlingBehavior;
                return;
            }

            bool huntReady = HuntCooldown == 0;
            if (huntReady)
                HuntCooldown = Main.rand.Next(13, 30);

            // With sight, just go straight at him. Without it, try to pathfind over them.
            pathfinding.DoPathfinding(new(NPC.Center, CurrentPrey.Center, tileValidity: SunkenSeaTileValidity), forceNewTask: huntReady);

            HuntCooldown--;
        }

        private void EatingBehavior()
        {
            if (Vector2.DistanceSquared(NPC.Center, SpongeFoundPosition) <= 64f * 64f)
            {
                NPC.velocity *= 0.9f;
                Animation = AnimationState.Eating;
                if (NPC.frame.Y >= NPC.height * AnimationFrames)
                {
                    HasEatenSponge = true;
                    Animation = AnimationState.Idle;
                    CurrentBehavior = IdlingBehavior;
                    return;
                }
            }
            else
                pathfinding.FindPath(new(NPC.Center, SpongeFoundPosition, SunkenSeaTileValidity));
        }

        private void OutsideWaterBehavior()
        {
            if (NPC.wet)
            {
                NPC.noGravity = true;
                CurrentBehavior = _previousBehavior;
            }
        }

        private void DetectSponges()
        {
            Vector2? closestSpongeFound = null;
            float closestDistanceSquared = float.MaxValue;

            foreach (var direction in Directions)
            {
                // var points = GetIntersectingPoints(NPC.Center, NPC.Center + direction * 360f);
                var points = new Point[1];
                foreach (var point in points)
                {
                    // Check if the tile coordinates are within the valid range
                    if (!WorldGen.InWorld(point.X, point.Y))
                        continue;

                    // Check if the current tile is AerialiteBrick
                    if (Main.tile[point].TileType == TileType<AerialiteBrick>())
                    {
                        // Check all adjacent directions for a non-solid tile with line of sight
                        foreach (var adjacentDirection in Directions)
                        {
                            Point adjacentPoint = point + adjacentDirection.ToPoint();

                            // Validate adjacent tile coordinates
                            if (!WorldGen.InWorld(adjacentPoint.X, adjacentPoint.Y))
                                continue;

                            // Skip if the adjacent tile is solid
                            if (Main.tile[adjacentPoint].IsTileSolid())
                                continue;

                            Vector2 worldPos = adjacentPoint.ToWorldCoordinates();
                            if (NPC.HasSight(worldPos))
                            {
                                // Calculate the squared distance from NPC to the adjacent tile
                                float distanceSquared = Vector2.DistanceSquared(NPC.Center, worldPos);
                                if (distanceSquared < closestDistanceSquared)
                                {
                                    closestDistanceSquared = distanceSquared;
                                    closestSpongeFound = worldPos;
                                }
                            }
                        }

                        // Break after checking the first sponge nutrient in this direction
                        break;
                    }
                }
            }

            // Update behavior if a valid tile was found
            if (closestSpongeFound.HasValue)
            {
                CurrentBehavior = EatingBehavior;
                SpongeFoundPosition = closestSpongeFound.Value;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 25; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 4; i++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, new Vector2(hit.HitDirection, -1f), Mod.Find<ModGore>($"{Name}{i + 1}").Type);
            }
        }

        #endregion

        #region Drawing & Animation

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > TimePerAnimationFrame)
            {
                NPC.frame.Y += frameHeight;
                NPC.frame.Y = Math.Min(NPC.frame.Y, AnimationFrames * frameHeight);
                NPC.frameCounter = 0;
                if (Animation == AnimationState.Idle)
                    NPC.frame.Y %= AnimationFrames * frameHeight;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(horizontalFrames: 2, verticalFrames: 12, frameX: (int)Animation, frameY: NPC.frame.Y / NPC.height);
            Vector2 anchorPoint = frame.Size() * 0.5f;
            SpriteEffects flip = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, anchorPoint, ScaleSquish, flip, 0f);

            return false;
        }

        #endregion

        #region Other ModNPC Overrides

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 12;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            
            BannerItem = ItemType<ProbesnoutBanner>();

            NPC.lifeMax = 5;

            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.chaseable = false;

            NPC.width = 44;
            NPC.height = 50;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);

            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        #endregion

        #region Syncing

        private Vector2 _randomPathPoint;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(_randomPathPoint);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            _randomPathPoint = reader.ReadVector2();
        }

        #endregion
    }

    public class ProbesnoutGold : Probesnout
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            this.HideFromBestiary();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.rarity = 3;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 25; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 4; i++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, new Vector2(hit.HitDirection, -1f), Mod.Find<ModGore>($"{Name}{i + 1}").Type);
            }
        }
    }
}
