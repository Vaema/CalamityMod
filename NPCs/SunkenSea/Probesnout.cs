using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Probesnout : SunkenSeaNPC, IPathFinder
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
                if (value != _currentBehavior)
                    OnBehaviorChange(value);
                _previousBehavior = _currentBehavior;
                _currentBehavior = value;
            }
        }

        private Action _previousBehavior;

        public Task<List<Vector2>> Paths { get; set; }

        public Vector2 Position => NPC.Center;

        public Vector2 Velocity { get => NPC.velocity; set => NPC.velocity = value; }

        public float Acceleration => 0.4f;

        public float MaxSpeed => 8f;

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
        }

        public override void AI()
        {
            CurrentBehavior?.Invoke();

            NPC.rotation = MathHelper.ToRadians(NPC.velocity.X * 3f);
            NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);



            if (!NPC.wet)
                CurrentBehavior = OutsideWaterBehavior;

            if (ScaleSquish.Y > 1f)
            {
                ScaleSquish.Y -= 0.025f;
                if (ScaleSquish.Y < 1f)
                    ScaleSquish.Y = 1f;
            }
        }

        private void OnBehaviorChange(Action newBehavior) => NPC.noGravity = newBehavior != OutsideWaterBehavior;

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
            PathfindingParams parameters = null;
            if (Main.rand.NextBool(IdleRandomMovementUnlikeliness))
            {
                _randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(IdleMinPathDistance, IdleMaxPathDistance);
                NPC.netUpdate = true;
                parameters = new PathfindingParams(NPC.Center, _randomPathPoint, SunkenSeaTileValidity);
            }
            this.DoPathfinding(parameters);
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
            if (!Collision.CanHitLine(NPC.Center, 1, 1, NPC.Center + CurrentPredator.DirectionTo(NPC.Center) * 120f, 1, 1))
            {
                _randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * 80f;
                NPC.netUpdate = true;
                this.DoPathfinding(new PathfindingParams(NPC.Center, _randomPathPoint, SunkenSeaTileValidity));
            }
            else
            {
                NPC.velocity += NPC.DirectionFrom(CurrentPredator.Center) * Acceleration;
                Paths = null;

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

            // With sight, just go straight at him. Without it, try to pathfind over them.
            if (!NPC.HasSight(CurrentPrey.Center))
                this.DoPathfinding(new PathfindingParams(NPC.Center, CurrentPrey.Center, SunkenSeaTileValidity));
            else
                NPC.velocity += NPC.DirectionTo(CurrentPrey.Center) * Acceleration;
        }

        private void OutsideWaterBehavior()
        {
            if (NPC.wet)
            {
                NPC.noGravity = true;
                CurrentBehavior = _previousBehavior;
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
                NPC.frame.Y = NPC.frame.Y % (AnimationFrames * frameHeight);
                NPC.frameCounter = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(horizontalFrames: 2, verticalFrames: 12, frameX: (int)Animation, frameY: NPC.frame.Y / 44);
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
            Main.npcFrameCount[Type] = 15;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 5;

            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.chaseable = false;

            NPC.width = 32;
            NPC.height = 32;
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
            for (int i = 0; i < 5; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f);
        }
    }
}
