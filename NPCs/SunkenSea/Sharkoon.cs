using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Sharkoon : SunkenSeaNPC
    {
        public static float DistanceToKaboom = 80f;
        public static float IdleMovementMaxRange = 300f;
        public static int ExplosionRadius = 80;
        public static float TimeToRecover = 600f;
        public static float TimeRecovering = 120f;

        #region Fields & Properties

        protected override List<int> HuntNPCs => new()
        {
            // NPCType<GildedAxolotl>(),
            NPCType<SeaFloaty>(),
            NPCType<Probesnout>(),
            NPCType<SeaMinnow>(),
            NPCType<EutrophicRay>(),
        };

        protected override List<int> AvoidNPCs => new()
        {
            // NPCType<Polyperil>(),
            // NPCType<Snailord>(),
            NPCType<PrismBack>(),
            // NPCType<Hermititan>,
        };

        /// <summary>
        /// The different types of personality this NPC can have.
        /// </summary>
        private enum PersonalityType { Shy, Curious, Paranoid }

        /// <summary>
        /// The current personality of this NPC.
        /// </summary>
        private PersonalityType Personality
        {
            get => (PersonalityType)NPC.ai[0];
            set
            {
                NPC.ai[0] = (float)value;
                NetUpdate();
            }
        }

        /// <summary>
        /// The different types of animation this enemy can have.
        /// </summary>
        private enum AnimationState { Normal, Explosion, Shrunk }

        /// <summary>
        /// The current animation being played right now.
        /// </summary>
        private AnimationState Animation
        {
            get => (AnimationState)NPC.ai[1];
            set
            {
                // In case an animation is being set constantly,
                // so we don't have it reset to the first frame every time,
                // it'll only reset the current frame when actually changing animations.
                if (value != Animation)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                }

                NPC.ai[1] = (float)value;

                switch (value)
                {
                    case AnimationState.Normal:
                        ChangeAnimation(animationFrames: 7, timePerAnimationFrame: 7, loops: true);
                        break;
                    case AnimationState.Explosion:
                        ChangeAnimation(animationFrames: 16, timePerAnimationFrame: 8, loops: false);
                        break;
                    case AnimationState.Shrunk:
                        ChangeAnimation(animationFrames: 6, timePerAnimationFrame: 6, loops: true);
                        break;
                }

                NetUpdate();
            }
        }

        /// <summary>
        /// The amount of frames that one column of sprites has.<br/>
        /// Defaults to 7 since that's the first column's amount of frames.
        /// </summary>
        private int AnimationFrames = 7;

        /// <summary>
        /// The amount, in frames, that it takes to go to the next animation frame.<br/>
        /// Defaults to 7.
        /// </summary>
        private int TimePerAnimationFrame = 7;

        /// <summary>
        /// Whether or not the animation loops or not.<br/>
        /// Defaults to <see langword="true"/>.
        /// </summary>
        private bool AnimationLoop = true;

        /// <summary>
        /// A timer used to know when the NPC should recover after it explodes.
        /// </summary>
        private ref float RecoverTimer => ref NPC.ai[2];

        /// <summary>
        /// The maximum speed this NPC can have, it cannot be faster than this number.
        /// </summary>
        private float MaximumSpeed = 4f;

        /// <summary>
        /// The acceleration of the Sharkoon.
        /// </summary>
        private const float Acceleration = 0.25f;

        /// <summary>
        /// How unlikely is the Sharkoon going to move normally.<br/>
        /// Defaults to 250.
        /// </summary>
        private int RandomIdleMovementUnlikeliness = 250;

        /// <summary>
        /// Whether or not the Sharkoon is capable of exploding.
        /// </summary>
        private bool IsBig => Animation == AnimationState.Normal;

        /// <summary>
        /// Whether or not the Sharkoon is currently exploding.
        /// </summary>
        private bool IsExploding => CurrentBehavior == ExplodingBehavior;

        /// <summary>
        /// The squish of this NPC while drawing.
        /// </summary>
        private Vector2 ScaleSquish = Vector2.One;

        #endregion

        #region Other Overridden Methods

        protected override void ExtraSetStaticDefaults() => Main.npcFrameCount[Type] = 16;

        public override void SetDefaults()
        {
            NPC.damage = 20;
            NPC.lifeMax = 350;
            NPC.defense = 5;
            NPC.knockBackResist = 0.15f;
            NPC.chaseable = false;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.width = 52;
            NPC.height = 46;
            NPC.noGravity = true;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            // Banner = NPC.type;
            // BannerItem = ModContent.ItemType<SharkoonBanner>();

            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };

            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.9f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedMoss, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedMoss, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            if (attacker.whoAmI == NPC.whoAmI)
                return false;

            return true;
        }

        public override void OnHitByNPC(NPC attacker)
        {
            if (IsBig)
                CurrentBehavior = ExplodingBehavior;
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (IsBig)
                CurrentBehavior = ExplodingBehavior;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (IsBig)
                CurrentBehavior = ExplodingBehavior;
        }

        #endregion

        #region AI

        protected override void CreatureOnSpawn()
        {
            CurrentBehavior = IdlingBehavior;

            WeightedRandom<PersonalityType> randomPersonality = new();
            randomPersonality.Add(PersonalityType.Shy, 0.4f);
            randomPersonality.Add(PersonalityType.Curious, 0.4f);
            randomPersonality.Add(PersonalityType.Paranoid, 0.2f);
            Personality = randomPersonality.Get();

            DetectionDistance = 320f;
            ConeDetectionAngle = MathHelper.ToRadians(30f);

            if (Personality == PersonalityType.Curious)
            {
                DetectionDistance *= 1.5f;
                ConeDetectionAngle *= 1.5f;
                RandomIdleMovementUnlikeliness /= 2;
            }

            if (Personality == PersonalityType.Paranoid)
                MaximumSpeed *= 1.5f;

            NPC.spriteDirection = Main.rand.NextBool().ToDirectionInt();
            NPC.GravityMultiplier *= 2f;
            NPC.MaxFallSpeedMultiplier *= 2f;
        }

        protected override void CreatureAI()
        {
            CurrentBehavior?.Invoke();

            // Sets the direction at which it looks at the cone.
            // This makes it look at the direction it's going.
            if (!NPC.velocity.HasNaNs())
                ConeDetectionDirection = (NPC.rotation * MathF.Sign(NPC.velocity.Y) + (NPC.spriteDirection == -1 ? MathHelper.Pi : 0f)).ToRotationVector2();

            // Leans the Sharkoon towards the direction it's going.
            NPC.rotation = MathHelper.ToRadians(NPC.velocity.Length() * 3f) * MathF.Sign(NPC.velocity.X);

            // When the Sharkoon is fast enough in one direction, it'll turn to that direction.
            if (MathF.Abs(NPC.velocity.X) > 1f)
                NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);

            // When the Sharkoon's paranoid, it'll explode at anything that isn't prey which gets close to the Sharkoon.
            bool isEntityPlayerOrNonPreyNPC = NearestEntity is not null && (NearestEntity is Player || NearestEntity is NPC npc && !HuntNPCs.Contains(npc.type));
            if (isEntityPlayerOrNonPreyNPC)
            {
                bool isWithinDistance = NPC.DistanceSQ(NearestEntity.Center) < DistanceToKaboom * DistanceToKaboom;
                if (IsBig && Personality == PersonalityType.Paranoid && isEntityPlayerOrNonPreyNPC && isWithinDistance)
                    CurrentBehavior = ExplodingBehavior;
            }

            // When it has already exploded, a timer will start, which when finished it'll make the Sharkoon recover.
            if (!IsBig)
            {
                RecoverTimer++;
                if (RecoverTimer > TimeToRecover)
                    CurrentBehavior = RecoveringBehavior;
            }

            // When it gets outside of water, it'll try to gravitate downards towards the water.
            if (!NPC.wet && !IsExploding)
                CurrentBehavior = OutsideWaterBehavior;

            // Clamps the velocity at all times.
            if (NPC.velocity.LengthSquared() > MaximumSpeed * MaximumSpeed)
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * MaximumSpeed;

            if (ScaleSquish.Y > 1f)
            {
                ScaleSquish.Y -= 0.025f;
                if (ScaleSquish.Y < 1f)
                    ScaleSquish.Y = 1f;
            }
        }

        protected override void OnBehaviorChange(Action newBehavior)
        {
            // Obviously, when it gets to the exploding behaivor, it should make the explosion animation.
            if (newBehavior == ExplodingBehavior)
                Animation = AnimationState.Explosion;

            // Resets the timer when it needs to use it again.
            if (newBehavior == RecoveringBehavior || PreviousBehavior == RecoveringBehavior)
                RecoverTimer = 0f;

            // When it gets outside of water, it'll try to gravitate downards towards the water.
            if (newBehavior == OutsideWaterBehavior)
                NPC.noGravity = false;
        }

        protected override void OnPreyDetection(NPC prey)
        {
            // If it's not a small fish and it's not being chased by a predator, the Sharkoon may hunt.
            if (IsBig && CurrentPredator is null && !IsExploding)
            {
                CurrentBehavior = HuntBehavior;
                ScaleSquish.Y += 0.4f;
                React(Color.Orange * 0.6f, EmoteExpressionParticle.EmoteType.Exclamation, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });
            }
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            // If it detects a predator, time to run.
            if (!IsExploding)
            {
                CurrentBehavior = AvoidBehavior;
                React(Color.Red * 0.6f, EmoteExpressionParticle.EmoteType.DoubleExclamation, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });
                ScaleSquish.Y += 0.4f;
            }
        }

        protected override void OnPlayerDetection(Player player)
        {
            // When there's a predator, player's shouldn't matter.
            if (CurrentPredator is not null)
                return;

            // If the Sharkoon is shy, it'll run away from the player.
            if (Personality == PersonalityType.Shy)
            {
                CurrentBehavior = AvoidBehavior;
                React(Color.Orange, EmoteExpressionParticle.EmoteType.QuestionExclamation, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });
                ScaleSquish.Y += 0.4f;
            }

            if (Personality == PersonalityType.Curious)
            {
                React(Color.Green, EmoteExpressionParticle.EmoteType.Question, new("CalamityMod/Sounds/Custom/ur") { PitchVariance = 0.2f });
                ScaleSquish.Y += 0.4f;
            }
        }

        private void IdlingBehavior()
        {
            if (PathfindingPoints is null)
            {
                // While it hasn't decided yet which path to follow, it'll deaccelerate and stand still.
                NPC.velocity *= 0.95f;

                // Randomly, it'll decide a path whose destination is somewhere around him.
                if (Main.rand.NextBool(RandomIdleMovementUnlikeliness))
                    SunkenSeaPathfinding();
            }
            else
                GenericPathFollowing(Acceleration);
        }

        private void HuntBehavior()
        {
            // If there's no more prey, go back to idling.
            if (CurrentPrey is null)
            {
                CurrentBehavior = IdlingBehavior;
                return;
            }

            if (!HasLineOfSight(CurrentPrey.Center))
            {
                if (HasPath)
                    GenericPathFollowing(Acceleration);
                else
                    SunkenSeaPathfinding(CurrentPrey.Center);
            }
            else
                NPC.velocity += NPC.DirectionTo(CurrentPrey.Center) * Acceleration;
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
                case PersonalityType.Paranoid:
                    entityToAvoid = CurrentPredator;
                    break;
            }

            // If there aren't any more targets detected, go back to idling.
            if (entityToAvoid is null)
            {
                CurrentBehavior = IdlingBehavior;
                return;
            }

            // When it's going in a direction and it detects that it'll stumble into tiles, it'll try to path-find a way to continue avoiding whichever entity.
            const int TileAnticipationDistance = 5 * 16;
            const float AngleVariance = 2.2f; // In radians.
            const int MaxAttemptsToFindPath = 20;
            const int MinimumEscapeDistance = 160;
            const int MaximumEscapeDistance = 320;
            if (Main.tile[(NPC.Center + NPC.DirectionFrom(entityToAvoid.Center) * TileAnticipationDistance).ToTileCoordinates()].IsTileSolid() && !HasPath)
            {
                Vector2 randomEscapePoint;
                int attempts = 0;
                do
                {
                    randomEscapePoint = NPC.Center + NPC.DirectionFrom(entityToAvoid.Center).RotatedByRandom(AngleVariance) * Main.rand.NextFloat(MinimumEscapeDistance, MaximumEscapeDistance);
                    attempts++;
                }
                while (Main.tile[randomEscapePoint.ToTileCoordinates()].IsTileSolid() || attempts < MaxAttemptsToFindPath);

                SunkenSeaPathfinding(randomEscapePoint);
            }

            if (HasPath)
                GenericPathFollowing(Acceleration);
            else
                NPC.velocity += NPC.DirectionFrom(entityToAvoid.Center) * Acceleration;

            // If it's capable of exploding and the predator's within distance, kaboom.
            if (IsBig && NPC.DistanceSQ(entityToAvoid.Center) < DistanceToKaboom * DistanceToKaboom && entityToAvoid is NPC predator && AvoidNPCs.Contains(predator.type))
                CurrentBehavior = ExplodingBehavior;
        }

        private void ExplodingBehavior()
        {
            // Deaccelerate.
            NPC.velocity *= 0.95f;

            // Produces light while exploding.
            Lighting.AddLight(NPC.Center, Color.Orange.ToVector3() * Utils.GetLerpValue(NPC.height, NPC.height * 9, NPC.frame.Y, true));

            // On the exact frame that the NPC explodes, it'll will, indeed, explode.
            if (NPC.frame.Y == NPC.height * 9 && NPC.frameCounter == 0)
                Kaboom();

            // When the animation's finished, go back to being a normal fish.
            if (NPC.frame.Y >= NPC.height * 16)
            {
                Animation = AnimationState.Shrunk;
                CurrentBehavior = PreviousBehavior;
            }
        }

        private void RecoveringBehavior()
        {
            if (RecoverTimer >= TimeRecovering)
            {
                Animation = AnimationState.Normal;
                CurrentBehavior = PreviousBehavior;
            }

            // Deaccelerates to stay still.
            NPC.velocity *= 0.95f;

            // VFX and sound effects go here,
            // a dedicated server doesn't need to load these.
            if (!Main.dedServ)
            {
                float fade = Utils.GetLerpValue(TimeRecovering, 0, RecoverTimer);
                float rotFactor = 360f;
                if (Main.rand.NextBool())
                {
                    float rot = MathHelper.ToRadians(rotFactor);
                    Vector2 velOffset = CalamityUtils.RandomVelocity(50f, 20f, 70f, 0.04f);
                    velOffset *= Main.rand.NextFloat(15, 20) * fade;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + velOffset * 2.5f, 267, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, 0, default, Main.rand.NextFloat(0.8f, 0.95f));
                    dust.noGravity = true;
                    dust.color = Color.Aqua;
                }
            }

            RecoverTimer++;
        }

        private void OutsideWaterBehavior()
        {
            if (NPC.wet)
            {
                NPC.noGravity = true;
                CurrentBehavior = PreviousBehavior;
            }
        }

        private void Kaboom()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectileDirect(
                    NPC.GetSource_FromThis(),
                    NPC.Center,
                    Vector2.Zero,
                    ProjectileType<SharkoonExplosion>(),
                    NPC.damage,
                    10f,
                    Main.myPlayer);
            }

            // VFX and sound effects go here,
            // a dedicated server doesn't need to load these.
            if (!Main.dedServ)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center, 278, new Vector2(8, 8).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f), 0, default, Main.rand.NextFloat(0.9f, 1.1f));
                    dust.noGravity = false;
                    dust.color = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                }
                for (int i = 0; i < 18; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center, 267, new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f), 0, default, Main.rand.NextFloat(0.9f, 1.1f));
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                }
                for (int i = 0; i < 8; i++)
                {
                    Vector2 randVel = new Vector2(7, 7).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f);
                    Particle smoke = new HeavySmokeParticle(NPC.Center + randVel, randVel, Color.Lerp(Color.SlateGray, Color.Black, Main.rand.NextFloat(0.2f, 0.45f)), Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.9f, 2.3f), 0.4f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                SoundStyle boom = new("CalamityMod/Sounds/Custom/SharkoonBoom");
                SoundEngine.PlaySound(boom with { Volume = 0.7f, PitchVariance = 0.15f }, NPC.Center);
            }

            NetUpdate();
        }

        #endregion

        #region Drawing & Animation

        /// <summary>
        /// A quick method to change all the animation's properties.
        /// </summary>
        private void ChangeAnimation(int animationFrames, int timePerAnimationFrame, bool loops)
        {
            AnimationFrames = animationFrames;
            TimePerAnimationFrame = timePerAnimationFrame;
            AnimationLoop = loops;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > TimePerAnimationFrame)
            {
                NPC.frame.Y += frameHeight;
                NPC.frame.Y = Math.Min(NPC.frame.Y, AnimationFrames * frameHeight);
                NPC.frameCounter = 0;
                if (AnimationLoop)
                    NPC.frame.Y = NPC.frame.Y % (AnimationFrames * frameHeight);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(horizontalFrames: 3, verticalFrames: 16, frameX: (int)Animation, frameY: NPC.frame.Y / NPC.height);
            Vector2 anchorPoint = frame.Size() * 0.5f;
            SpriteEffects flip = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (Personality == PersonalityType.Paranoid)
                drawPosition += Main.rand.NextVector2Circular(1.5f, 1.5f);

            spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, anchorPoint, ScaleSquish, flip, 0f);

            return false;
        }

        #endregion

        #region Syncing

        protected override void SendMoreExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(AnimationFrames);
            writer.Write7BitEncodedInt(TimePerAnimationFrame);
            writer.Write(AnimationLoop);
            writer.Write(MaximumSpeed);
            writer.Write7BitEncodedInt(RandomIdleMovementUnlikeliness);
        }

        protected override void ReceiveMoreExtraAI(BinaryReader reader)
        {
            AnimationFrames = reader.Read7BitEncodedInt();
            TimePerAnimationFrame = reader.Read7BitEncodedInt();
            AnimationLoop = reader.ReadBoolean();
            MaximumSpeed = reader.ReadSingle();
            RandomIdleMovementUnlikeliness = reader.Read7BitEncodedInt();
        }

        #endregion
    }
}
