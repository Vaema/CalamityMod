using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Probesnout : SunkenSeaNPC
    {
        public static float PathDetectionSize = 300f;

        #region Members

        protected override List<int> HuntNPCs => new()
        {
            // NPCType<Slugbun>(),
        };

        protected override List<int> AvoidNPCs => new()
        {
            // NPCType<IlmerianAxolotl>(),
            NPCType<Sharkoon>(),
            // NPCType<Polyperil>(),
            // NPCType<CrestedStalker>(),
            // NPCType<Hermititan>(),
        };

        private enum PersonalityType { Curious, Shy, Paranoid }

        private PersonalityType Personality
        {
            get => (PersonalityType)NPC.ai[0];
            set
            {
                NPC.ai[0] = (float)value;
                NetUpdate();
            }
        }

        private enum AnimationState { Idle, Attack }

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
                    case AnimationState.Attack:
                        AnimationFrames = 12;
                        TimePerAnimationFrame = 7;
                        break;
                }

                NPC.ai[1] = (float)value;
                NetUpdate();
            }
        }

        protected override BiomeFlags BiomeDesignation => BiomeFlags.RadiantReefs;

        protected override float SpawningChance => 0f;

        private int AnimationFrames = 8;

        private int TimePerAnimationFrame = 5;

        private float MaximumSpeed = 6f;

        private Vector2 ScaleSquish = Vector2.One;

        #endregion

        #region AI

        protected override void BehaviorOnSpawn()
        {
            CurrentBehavior = IdleBehavior;

            WeightedRandom<PersonalityType> randomPersonality = new();
            randomPersonality.Add(PersonalityType.Curious, 0.6f);
            randomPersonality.Add(PersonalityType.Shy, 0.3f);
            randomPersonality.Add(PersonalityType.Paranoid, 0.1f);
            Personality = randomPersonality.Get();

            NPC.spriteDirection = Main.rand.NextBool().ToDirectionInt();
            NPC.GravityMultiplier *= 2f;
            NPC.MaxFallSpeedMultiplier *= 2f;
        }

        protected override void CreatureAI()
        {
            CurrentBehavior.Invoke();

            if (!NPC.velocity.HasNaNs())
                ConeDetectionDirection = (NPC.rotation * MathF.Sign(NPC.velocity.Y) + (NPC.spriteDirection == -1 ? MathHelper.Pi : 0f)).ToRotationVector2();

            NPC.rotation = MathHelper.ToRadians(NPC.velocity.Length() * 3f) * MathF.Sign(NPC.velocity.X);

            if (MathF.Abs(NPC.velocity.X) > 2f)
                NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);

            if (!NPC.wet)
                CurrentBehavior = OutsideWaterBehavior;

            if (NPC.velocity.LengthSquared() > MaximumSpeed * MaximumSpeed)
                NPC.velocity = NPC.velocity.SafeNormalize(-Vector2.UnitY) * MaximumSpeed;

            if (ScaleSquish.Y > 1f)
                ScaleSquish.Y -= 0.025f;

            if (ScaleSquish.Y < 1f)
                ScaleSquish.Y = 1f;
        }

        protected override void OnBehaviorChange(Action newBehavior) => NPC.noGravity = newBehavior != OutsideWaterBehavior;

        protected override void OnPreyDetection(NPC prey)
        {
            // If it's not a small fish and it's not being chased by a predator, the Probesnout may hunt.
            if (CurrentPredator is null)
            {
                CurrentBehavior = HuntBehavior;
                React(Color.Orange * 0.6f, EmoteExpressionParticle.EmoteType.Exclamation, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });
            }

            ScaleSquish.Y += 1.4f;
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            // Regardless of anything, if it detects a predator, time to run.
            CurrentBehavior = AvoidBehavior;

            React(Color.Red * 0.6f, EmoteExpressionParticle.EmoteType.DoubleExclamation, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });

            ScaleSquish.Y += 1.4f;
        }

        protected override void OnPlayerDetection(Player player)
        {
            // If the Probesnout is shy, it'll run away from the player.
            if (Personality == PersonalityType.Shy)
            {
                CurrentBehavior = AvoidBehavior;
                React(Color.Orange, EmoteExpressionParticle.EmoteType.QuestionExclamation, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });
            }

            if (Personality == PersonalityType.Curious)
                React(Color.Green, EmoteExpressionParticle.EmoteType.Question, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });

            ScaleSquish.Y += 0.4f;
        }

        private void IdleBehavior()
        {
            if (!HasPath)
            {
                NPC.velocity *= 0.95f;

                if (Main.rand.NextBool(125))
                    SunkenSeaPathfinding(NPC.Center + Main.rand.NextVector2CircularEdge(PathDetectionSize, PathDetectionSize) * Main.rand.NextFloat(0.75f, 1f));
            }
            else
                GenericPathFollowing(acceleration: 0.03f);
        }

        private void AvoidBehavior()
        {
            // Depending on the personality, the Sharkoon will choose who to avoid.
            Entity entityToAvoid = null;
            switch (Personality)
            {
                case PersonalityType.Shy:
                    entityToAvoid = (NearestEntity is NPC nearestNPC && HuntNPCs.Contains(nearestNPC.type)) ? null : NearestEntity;
                    break;
                case PersonalityType.Curious:
                    entityToAvoid = CurrentPredator;
                    break;
                case PersonalityType.Paranoid:
                    entityToAvoid = CurrentPredator;
                    break;
            }

            // If there aren't any more targets detected, go back to idling.
            if (entityToAvoid is null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            bool isAvoidingPredator = entityToAvoid is NPC npc && npc.whoAmI == CurrentPredator.whoAmI;

            if (HasPath)
            {
                float fleeingDistance = isAvoidingPredator ? 600f : 300f;

                Vector2 randomEscapePoint = NPC.Center + NPC.DirectionFrom(entityToAvoid.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(fleeingDistance, fleeingDistance + 100f);
                while (Main.tile[randomEscapePoint.ToTileCoordinates()].IsTileSolid())
                    randomEscapePoint = NPC.Center + NPC.DirectionFrom(entityToAvoid.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(fleeingDistance, fleeingDistance + 100f);

                SunkenSeaPathfinding(randomEscapePoint);
            }
            else
                GenericPathFollowing(acceleration: 0.14f);
        }

        private void HuntBehavior()
        {
            // If there's no more prey, go back to idling.
            if (CurrentPrey is null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            if (!HasLineOfSight(CurrentPrey.Center))
            {
                if (HasPath)
                    GenericPathFollowing(acceleration: 0.14f);
                else
                    SunkenSeaPathfinding(CurrentPrey.Center);
            }
            else
                NPC.velocity += NPC.DirectionTo(CurrentPrey.Center) * 0.14f;
        }

        private void OutsideWaterBehavior()
        {
            if (NPC.wet)
                CurrentBehavior = PreviousBehavior;
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
            Main.npcFrameCount[Type] = 15;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 5;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.chaseable = false;

            NPC.width = 44;
            NPC.height = 55;
        }

        #endregion

        #region Syncing

        protected override void SendMoreExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(AnimationFrames);
            writer.Write7BitEncodedInt(TimePerAnimationFrame);
        }

        protected override void ReceiveMoreExtraAI(BinaryReader reader)
        {
            AnimationFrames = reader.Read7BitEncodedInt();
            TimePerAnimationFrame = reader.Read7BitEncodedInt();
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
