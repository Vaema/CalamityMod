using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Sharkoon : SunkenSeaNPC
    {
        #region Fields & Properties

        #region Static Fields

        //public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 200;
        public static int IdleMaxPathDistance = 400;

        public static int FleeTileAnticipationDistance = 64;

        public static int ExplosionDistance = 80;
        public static int ExplosionRadius = 80;
        public static int ExplosionCooldown = 600;
        public static int RecoveringTime = 120;

        #endregion

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
            }
        }

        /// <summary>
        /// The amount of frames that one column of sprites has.<br/>
        /// Defaults to 7 since that's the first column's amount of frames.
        /// </summary>
        private int _animationFrames = 7;

        /// <summary>
        /// The amount, in frames, that it takes to go to the next animation frame.<br/>
        /// Defaults to 7.
        /// </summary>
        private int _timePerAnimationFrame = 7;

        /// <summary>
        /// Whether or not the animation loops or not.<br/>
        /// Defaults to <see langword="true"/>.
        /// </summary>
        private bool _animationLoop = true;

        /// <summary>
        /// The squish of this NPC while drawing.
        /// </summary>
        private Vector2 ScaleSquish = Vector2.One;

        private Entity _avoidedEntity;

        /// <summary>
        /// A timer used to know when the NPC should recover after it explodes.
        /// </summary>
        private ref float RecoverTimer => ref NPC.ai[2];

        /// <summary>
        /// Whether or not the Sharkoon is capable of exploding.
        /// </summary>
        private bool CanExplode => Animation == AnimationState.Normal;

        /// <summary>
        /// Whether or not the Sharkoon is currently exploding.
        /// </summary>
        private bool IsExploding => CurrentBehavior == ExplodingBehavior;

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

        private int HuntCooldown
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        #region SunkenSeaNPC Implementation

        protected override List<int> PreyIDs =>
        [
            // NPCType<GildedAxolotl>(),
            NPCType<SeaFloaty>(),
            NPCType<Probesnout>(),
            NPCType<ProbesnoutGold>(),
            NPCType<SeaMinnow>(),
            NPCType<AlphaSeaMinnow>(),
            NPCType<SeaMinnowGold>(),
            NPCType<AlphaSeaMinnowGold>(),
            NPCType<EutrophicRay>(),
        ];

        protected override List<int> PredatorIDs =>
        [
            // NPCType<Polyperil>(),
            // NPCType<Snailord>(),
            NPCType<PrismBack>(),
            // NPCType<Hermititan>,
        ];

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs;

        #endregion

        #endregion

        #region SetDefaults

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 16;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            BannerItem = ItemType<SharkoonBanner>();

            NPC.damage = 20;
            NPC.lifeMax = 350;
            NPC.defense = 5;
            NPC.knockBackResist = 0.15f;
            NPC.chaseable = false;

            NPC.width = 52;
            NPC.height = 46;
            NPC.noGravity = true;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(silver: 5);

            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        #endregion

        #region Behavior

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
            pathfinding = new PathfindingManager(this);
            Acceleration = 0.3f;
            MaxSpeed = 6f;
            CurrentBehavior = IdlingBehavior;
            NPC.spriteDirection = Main.rand.NextBool().ToDirectionInt();
            NPC.GravityMultiplier *= 2f;
            NPC.MaxFallSpeedMultiplier *= 2f;
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(this);
                Acceleration = 0.3f;
                MaxSpeed = 6f;
            }
            CurrentBehavior?.Invoke();

            // Leans the Sharkoon towards the direction it's going.
            NPC.rotation = MathHelper.ToRadians(NPC.velocity.X * 3f);
            NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);

            // When it has already exploded, a timer will start, which when finished it'll make the Sharkoon recover.
            if (!CanExplode)
            {
                RecoverTimer++;
                if (RecoverTimer > ExplosionCooldown)
                    CurrentBehavior = RecoveringBehavior;
            }

            // When it gets outside of water, it'll try to gravitate downards towards the water.
            if (!NPC.wet && !IsExploding && CurrentBehavior != OutsideWaterBehavior)
                CurrentBehavior = OutsideWaterBehavior;


            // Reset any squish that is done to the Sharkoon, and clamps its upper limit to prevent it from becoming too tall
            if (ScaleSquish.Y > 1f)
                ScaleSquish.Y = MathHelper.Clamp(ScaleSquish.Y, 1f, 1.5f);
                ScaleSquish.Y = Math.Max(1f, ScaleSquish.Y - 0.025f);
        }

        private void IdlingBehavior()
        {
            // At random, the mob will choose a random nearby point and pathfind there.
            pathfinding.DoPathfinding(new(this, NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(IdleMinPathDistance, IdleMaxPathDistance), SunkenSeaTileValidity));
        }

        private void HuntingBehavior()
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
            pathfinding.DoPathfinding(new(this, NPC.Center, CurrentPrey.Center, SunkenSeaTileValidity), forceNewTask: huntReady);

            HuntCooldown--;
        }

        private void FleeingBehavior()
        {
            // Check who is the avoided entity specifically.
            _avoidedEntity = _avoidedEntity is NPC ? CurrentPredator : CurrentPlayer;

            // If the avoided entity is gone, go back to idling.
            if (_avoidedEntity == null)
            {
                CurrentBehavior = IdlingBehavior;
                return;
            }

            // While it doesn't have any obstacles in front of it, run away in a straight line.
            // Try to manuever if there are any obstacles.
            if (!Main.tile[(NPC.Center + NPC.DirectionFrom(_avoidedEntity.Center) * FleeTileAnticipationDistance).ToTileCoordinates()].IsTileSolid())
            {
                NPC.velocity += NPC.DirectionFrom(_avoidedEntity.Center) * Acceleration;
                pathfinding.ClearResults();

                // Cap the speed if MaxSpeed has been surpassed.
                if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
            }
            else
            {
                float distanceFromAvoided = Vector2.Distance(NPC.Center, _avoidedEntity.Center);
                _randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);
                NPC.netUpdate = true;
                pathfinding.DoPathfinding(new(this, NPC.Center, _randomPathPoint, SunkenSeaTileValidity));
            }

            // If it's capable of exploding and the predator's within distance, kaboom.
            if (CanExplode && NPC.DistanceSQ(_avoidedEntity.Center) < ExplosionDistance * ExplosionDistance && _avoidedEntity is NPC predator && PredatorIDs.Contains(predator.type))
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
                CurrentBehavior = _previousBehavior;
            }
        }

        private void RecoveringBehavior()
        {
            if (RecoverTimer >= RecoveringTime)
            {
                Animation = AnimationState.Normal;
                CurrentBehavior = _previousBehavior;
            }

            // Deaccelerates to stay still.
            NPC.velocity *= 0.95f;

            // VFX and sound effects go here,
            // a dedicated server doesn't need to load these.
            if (!Main.dedServ)
            {
                float fade = Utils.GetLerpValue(RecoveringTime, 0, RecoverTimer);
                if (Main.rand.NextBool())
                {
                    Vector2 velOffset = RandomVelocity(50f, 20f, 70f, 0.04f);
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
                CurrentBehavior = _previousBehavior;
            }
        }

        private void OnBehaviorChange(Action newBehavior)
        {
            // Obviously, when it gets to the exploding behaivor, it should make the explosion animation.
            if (newBehavior == ExplodingBehavior)
                Animation = AnimationState.Explosion;

            // Resets the timer when it needs to use it again.
            if (newBehavior == RecoveringBehavior || CurrentBehavior == RecoveringBehavior)
                RecoverTimer = 0f;

            // When it gets outside of water, it'll try to gravitate downards towards the water.
            if (newBehavior == OutsideWaterBehavior)
                NPC.noGravity = false;

            MinimumPointDistance = newBehavior == HuntingBehavior ? 20f : 48f;
        }

        protected override void OnPreyDetection(NPC prey)
        {
            // If it's not a small fish and it's not being chased by a predator, the Sharkoon may hunt.
            if (CanExplode && CurrentPredator is null && !IsExploding)
            {
                CurrentBehavior = HuntingBehavior;
                ScaleSquish.Y += 0.4f;
            }
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            // If it detects a predator, time to run.
            if (!IsExploding)
            {
                CurrentBehavior = FleeingBehavior;
                _avoidedEntity = predator;
                ScaleSquish.Y += 0.4f;
            }
        }

        protected override void OnPlayerDetection(Player player)
        {
            // When there's a predator, player's shouldn't matter.
            if (CurrentPredator is not null && !IsExploding)
                return;

            CurrentBehavior = FleeingBehavior;
            _avoidedEntity = player;
            ScaleSquish.Y += 0.4f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 25; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                if (CanExplode)
                {
                    SpawnGores(NPC, "Sharkoon", 6);
                }
                else
                {
                    SpawnGores(NPC, "SharkoonSmall", 2);
                }
            }
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override bool CanHitNPC(NPC target) => PreyIDs.Contains(target.type);

        public override void OnHitByNPC(NPC attacker)
        {
            if (CanExplode)
                CurrentBehavior = ExplodingBehavior;
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (CanExplode)
                CurrentBehavior = ExplodingBehavior;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (CanExplode)
                CurrentBehavior = ExplodingBehavior;
        }

        private void Kaboom()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(
                    NPC.GetSource_FromThis(),
                    NPC.Center,
                    Vector2.Zero,
                    ProjectileType<SharkoonExplosion>(),
                    NPC.damage * 10,
                    10f,
                    ai0: NPC.whoAmI);
            }

            AddScreenshakeAt(NPC.Center, 15f);

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
        }

        public override void AwaitingPathBehavior()
        {
            if (CurrentBehavior == HuntingBehavior)
            {
                if (CurrentPrey != null)
                {
                    NPC.velocity += NPC.DirectionTo(CurrentPrey.Center) * Acceleration;

                    // Cap the speed if MaxSpeed has been surpassed.
                    if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                        NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
                }
                else
                    NPC.velocity *= 0.95f;
            }
            else
                base.AwaitingPathBehavior();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZoneRadiantReefs)
                    return SpawnCondition.CaveJellyfish.Chance * 0.2f;
            }
            return 0f;
        }

        #endregion

        #region Drawing & Animation

        /// <summary>
        /// A quick method to change all the animation's properties.
        /// </summary>
        private void ChangeAnimation(int animationFrames, int timePerAnimationFrame, bool loops)
        {
            _animationFrames = animationFrames;
            _timePerAnimationFrame = timePerAnimationFrame;
            _animationLoop = loops;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > _timePerAnimationFrame)
            {
                NPC.frame.Y += frameHeight;
                NPC.frame.Y = Math.Min(NPC.frame.Y, _animationFrames * frameHeight);
                NPC.frameCounter = 0;
                if (_animationLoop)
                    NPC.frame.Y = NPC.frame.Y % (_animationFrames * frameHeight);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(horizontalFrames: 3, verticalFrames: 16, frameX: (int)Animation, frameY: NPC.frame.Y / NPC.height);
            Vector2 anchorPoint = frame.Size() * 0.5f;
            SpriteEffects flip = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, anchorPoint, ScaleSquish, flip, 0f);

            return false;
        }

        #endregion

        #region Sycning

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
}
