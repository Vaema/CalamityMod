using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.DataStructures;
using CalamityMod.Effects;
using CalamityMod.Items.Tools;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static System.Net.WebRequestMethods;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class HorribleHog : ModNPC
    {
        public enum BehaviorState
        {
            // Non-attacks.
            EngageAnimation = -3,
            LaughAtDeadPlayer = -2,
            DeathAnimation = -1,
            Idle,
            Stunned,    // Currently unused. May try to find some use for it later.

            // Attacks.
            ChasePlayer,
            HogCharge,
            JumpAndDash,
            HorribleHoller,
            VomitBarrage,
        }

        private static Asset<Texture2D> BloomCircle;
        private static Asset<Texture2D> ShineFlare;

        private static SoundStyle HorribleHog_Death = new("CalamityMod/Sounds/NPCKilled/HorribleHogDeath");
        private static SoundStyle HorribleHog_DeathLaugh = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogDeathLaugh", 2);

        public Dictionary<BehaviorState, int> PreviousAttackCounters = [];

        public Dictionary<BehaviorState, float> AttackWeights = [];

        public bool SearchForTargetEveryFrame;

        public bool HasPlayedEngageAnimation;

        public bool HasPlayedDeathAnimation;

        public bool ReadyToPlayLaughingAnimation;

        public float EyeGlintScale;

        public float AfterimageTrailOpacity;

        public float PriitiveTrailOpacity;

        public float HorizontalShakeStrength;

        /// <summary>
        /// Used as a shader parameter to rotate the already-drawn sprite of Horrible Hog directly regardless of the rotation value of <see cref="NPC.rotation"/>. <br></br>
        /// Used in cases where Horrible Hog needs to spin while also squashing and stretching. <see cref="NPC.rotation"/> is used for the spritebatch-drawn texture <br></br>
        /// and needs to rotate towards the NPC's velocity in order for the squashed sprite to face the correct direction. This parameter is then used and a shader is <br></br>
        /// then applied to the spritebatch to rotate the sprite differently from the value passed in spritebatch's Draw call.
        /// </summary>
        public float SpriteRotation;

        public Vector2 LastPlayerPosition;

        public Vector2 SquashVector;

        public Vector2 SquashVectorTarget;

        public SlotId DeathLaughSoundSlot;

        #region Static Properties
        public static int MaxAttacks_ChasePlayer => 2;
        public static int MaxAttacks_HogCharge => Main.expertMode ? 2 : 1;
        public static int MaxAttacks_HorribleHoller => 1;
        public static int MaxAttacks_JumpAndDash => Main.expertMode ? 3 : 2;
        public static int MaxAttacks_VomitBarrage => Main.expertMode ? 2 : 1;
        public static int MaxAttacksPerCycle => CalamityWorld.revenge ? 5 : Main.expertMode ? 4 : 3;

        public static int Damage_HogCharge => 10;
        public static int Damage_ShockwaveProjectile => 10;
        public static int Damage_ShockwaveRubbleProjectile => 12;
        public static int Damage_VomitChunkProjectile => 12;
        public static int Damage_VomitBombProjectile => 18;
        public static int Damage_VomitEyeProjectile => 14;

        public static float EngageDistance => 200f;
        public static float Idle_MaxSpeed => 2f;
        public static float Idle_MaxAcceleration => 0.125f;

        public static int StunnedTime => Main.expertMode ? 180 : 240;

        public static int ChaseTime => 180;
        public static float ChasePlayer_MaxSpeed => 5f;
        public static float ChasePlayer_MaxAcceleration => 0.3f;

        public static int HogCharge_PreChargeTime => 75;
        public static int HogCharge_ChargingTime => 240;
        public static int HogCharge_ChargeCooldownTime => CalamityWorld.revenge ? -20 : Main.expertMode ? -30 : -45;
        public static int HogCharge_PreDashTime => 45;
        public static int HogCharge_DashTime => 20;
        public static int HogCharge_MaxCharges => CalamityWorld.revenge ? 4 : Main.expertMode ? 3 : 2;
        public static float HogCharge_MaxSpeed => CalamityWorld.revenge ? 18f : Main.expertMode ? 15f : 12f;
        public static float HogCharge_MaxAcceleration => CalamityWorld.revenge ? 0.425f : Main.expertMode ? 0.4f : 0.375f;
        public static float HogCharge_MaxSlowdownDistance => CalamityWorld.revenge ? 64f : Main.expertMode ? 96f : 128f;

        public static int JumpAndDash_PreJumpTime => 75;
        public static int JumpAndDash_JumpPreDashTime => 45;
        public static int JumpAndDash_DashTime => 20;
        public static int JumpAndDash_CooldownTime => 60;
        public static int JumpAndDash_PreVomitTime => 60;
        public static int JumpAndDash_PreGroundPoundTime => 45;
        public static int JumpAndDash_MaxVomitChunks => CalamityWorld.revenge ? 5 : Main.expertMode ? 4 : 3;
        public static int JumpAndDash_VomitTimeInterval => CalamityWorld.revenge ? 10 : 15;
        public static int JumpAndDash_MaxBounces => Main.expertMode ? 3 : 2;
        public static float JumpAndDash_MaxJumpHeight => 14f;
        public static float JumpAndDash_MaxDashSpeed => 20f;
        public static float JumpAndDash_MaxBounceSpeed => CalamityWorld.revenge ? 16f : Main.expertMode ? 14f : 12f;

        public static int HorribleHoller_RoarTime => 75;
        public static int HorribleHoller_PostRoarCooldownTime => 30;
        public static int HorribleHoller_MaxZombies => 3;

        public static int VomitBarrage_PreJumpTime => 75;
        public static int VomitBarrage_VomitTime => 25;
        public static int VomitBarrage_PostVomitCooldown => 100;
        public static int VomitBarrage_MaxVomitChunks => CalamityWorld.revenge ? 10 : Main.expertMode ? 8 : 6;
        public static int VomitBarrage_MinVomitBombs => CalamityWorld.death ? 3 : CalamityWorld.revenge ? 2 : 0;
        public static int VomitBarrage_MaxVomitBombs => CalamityWorld.death ? 6 : CalamityWorld.revenge ? 5 : Main.expertMode ? 3 : 4;
        #endregion

        public ref float Timer => ref NPC.ai[0];

        public ref float AIState => ref NPC.ai[1];

        public ref float LocalAIState => ref NPC.ai[2];

        public ref float MainAttackCounter => ref NPC.ai[3];

        public ref float MiscAttackCounter => ref NPC.localAI[0];

        public ref float AltAttackVariant => ref NPC.localAI[1];

        public Vector2 VelocityBasedSquashNStretch
        {
            get
            {
                float stretch = MathHelper.Clamp(NPC.velocity.Length() / 20f * 0.1f, 1f, 1.05f);
                Vector2 stretchedVector = new(1f * stretch, 1f - 1f * stretch * 0.3f);
                return stretchedVector;
            }
        }

        public bool PhaseTwo
        {
            get
            {
                if (CalamityWorld.death || Main.getGoodWorld)
                    return NPC.life <= NPC.lifeMax * 0.7f;

                if (CalamityWorld.revenge)
                    return NPC.life <= NPC.lifeMax * 0.6f;

                return Main.expertMode && NPC.life <= NPC.lifeMax * 0.5f;
            }
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                BloomCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
                ShineFlare = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ShineFlare");
            }
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 28;
            NPC.damage = 20;
            NPC.defense = 14;
            NPC.lifeMax = 600;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 5f;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = false;

            SquashVector = Vector2.One;
            SquashVectorTarget = Vector2.One;
            ResetAttackWeights();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)PreviousAttackCounters.Count);
            foreach (var pair in PreviousAttackCounters)
            {
                writer.Write((byte)pair.Key);
                writer.Write((byte)pair.Value);
            }

            writer.Write((byte)AttackWeights.Count);
            foreach (var pair in AttackWeights)
            {
                writer.Write((byte)pair.Key);
                writer.Write((byte)pair.Value);
            }

            writer.WriteFlags(SearchForTargetEveryFrame, HasPlayedDeathAnimation, HasPlayedEngageAnimation, ReadyToPlayLaughingAnimation);
            writer.WritePackedWorldPosition(LastPlayerPosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            int attackCountersLength = reader.ReadByte();
            for (int i = 0; i < attackCountersLength; i++)
                PreviousAttackCounters.Add((BehaviorState)reader.ReadByte(), (int)reader.ReadByte());

            int attackWeightsLength = reader.ReadByte();
            for (int i = 0; i < attackWeightsLength; i++)
                AttackWeights.Add((BehaviorState)reader.ReadByte(), (float)reader.ReadByte());

            reader.ReadFlags(out SearchForTargetEveryFrame, out HasPlayedDeathAnimation, out HasPlayedEngageAnimation, out ReadyToPlayLaughingAnimation);
            LastPlayerPosition = reader.ReadPackedWorldPosition();
        }

        public override bool CheckDead()
        {
            if (!HasPlayedDeathAnimation)
            {
                NPC.life = NPC.lifeMax;
                HasPlayedDeathAnimation = true;
                SwitchBehavior(specificAttack: BehaviorState.DeathAnimation);
                return false;
            }

            return true;
        }

        public override bool? CanFallThroughPlatforms()
        {
            if (NPC.HasValidTarget)
            {
                Player target = Main.player[NPC.target];
                return target.Center.Y > NPC.Center.Y;
            }
            return base.CanFallThroughPlatforms();
        }

        public override void AI()
        {
            if (!NPC.HasValidTarget || SearchForTargetEveryFrame)
                NPC.TargetClosest(false);

            if (NPC.direction == 0)
                NPC.direction = Main.rand.NextBool().ToDirectionInt();

            // Due to how much Death Mode increases Blood Moon spawn rates, this is kinda required in order for the fight to not be super overwhelming.
            if (CalamityWorld.death)
                NPC.npcSlots = 40f;

            Player target = Main.player[NPC.target];
            NPC.damage = NPC.defDamage;
            NPC.defense = NPC.defDefense;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = false;
            NPC.spriteDirection = NPC.direction;

            // If the target dies, laugh at them.
            if (target.dead && ReadyToPlayLaughingAnimation && NPC.velocity.Y == 0f)
            {
                ReadyToPlayLaughingAnimation = false;
                SwitchBehavior(specificAttack: BehaviorState.LaughAtDeadPlayer);
            }

            switch ((BehaviorState)AIState)
            {
                case BehaviorState.EngageAnimation:
                    MainBehavior_EngageAnimation(target);
                    break;

                case BehaviorState.LaughAtDeadPlayer:
                    MainBehavior_LaughADeadPlayer();
                    break;

                case BehaviorState.DeathAnimation:
                    MainBehavior_DeathAnimation();
                    break;

                case BehaviorState.Idle:
                    MainBehavior_Idle(target);
                    break;

                case BehaviorState.Stunned:
                    MainBehavior_Stunned(target);
                    break;

                case BehaviorState.ChasePlayer:
                    MainBehavior_ChasePlayer(target);
                    break;

                case BehaviorState.HogCharge:
                    MainBehavior_HogCharge(target);
                    break;

                case BehaviorState.JumpAndDash:
                    MainBehavior_JumpAndDash(target);
                    break;

                case BehaviorState.HorribleHoller:
                    MainBehavior_HorribleHoller(target);
                    break;

                case BehaviorState.VomitBarrage:
                    MainBehavior_VomitBarrage(target);
                    break;
            }

            SquashVector = Vector2.Lerp(SquashVector, SquashVectorTarget, 0.125f);
            EyeGlintScale = MathHelper.Lerp(EyeGlintScale, 0f, 0.125f);
            NPC.StepUpBlocks();
            Timer++;
        }

        #region Non-Attack Behaviors
        public void MainBehavior_EngageAnimation(Player target)
        {
            if (Timer >= 45f)
            {
                if (Timer == 45f)
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);

                if (Timer % 10f == 0f)
                {
                    PulseRing hogRoar = new(NPC.Center, Vector2.Zero, Color.Red, 0f, 1.2f, 20);
                    GeneralParticleHandler.SpawnParticle(hogRoar, true);
                }
            }

            if (Timer >= 120f)
                SwitchBehavior(specificAttack: BehaviorState.ChasePlayer);

            NPC.velocity.X *= 0.9f;
            NPC.spriteDirection = (target.Center.X > NPC.Center.X).ToDirectionInt();
        }

        public void MainBehavior_LaughADeadPlayer()
        {
            if (NPC.velocity.Y == 0f)
            {
                // LMFAOOOOOOOOOOOOOOOOOOOOOOOO
                if (!SoundEngine.TryGetActiveSound(DeathLaughSoundSlot, out var _))
                {
                    DeathLaughSoundSlot = SoundEngine.PlaySound(HorribleHog_DeathLaugh, NPC.Center, (activeSound) =>
                    {
                        activeSound.Position = NPC.Center;
                        return AIState == (int)BehaviorState.LaughAtDeadPlayer;
                    });
                }

                NPC.direction *= -1;
                NPC.velocity.Y -= 4f;
            }

            SearchForTargetEveryFrame = true;
            NPC.defense = 30;
            NPC.damage = 0;
            NPC.velocity.X *= 0.92f;

            float targetAngle = NPC.direction < 0 ? MathHelper.ToRadians(50f) : MathHelper.ToRadians(-50f);
            NPC.rotation = (NPC.velocity.Y != 0f) ? NPC.rotation.AngleLerp(targetAngle, 0.075f) : 0f;

            if (Timer >= 240f)
            {
                BehaviorState nextAttack = NPC.HasValidTarget ? BehaviorState.ChasePlayer : BehaviorState.Idle;
                SwitchBehavior(specificAttack: nextAttack);
            }
        }

        public void MainBehavior_DeathAnimation()
        {
            if (Timer == 240f)
            {
                // Create smoke and throw up a big green "5000" on death just like the pigs in Angry Birds
                for (int i = 0; i < 12; i++)
                {
                    int goreType = Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1);
                    Gore.NewGorePerfect(NPC.position, Main.rand.NextVector2Circular(2f, 2f), goreType);
                }

                CombatText.NewText(NPC.Hitbox, Color.LawnGreen, 5000);
                SoundEngine.PlaySound(HorribleHog_Death, NPC.Center);

                NPC.life = 0;
                NPC.checkDead();
                NPC.HitEffect();
                NPC.netUpdate = true;
            }

            NPC.damage = 0;
            NPC.dontTakeDamage = true;

            if (NPC.velocity.Y == 0f)
                NPC.velocity.X *= 0.98f;
        }

        public void MainBehavior_Idle(Player target)
        {
            if (target.Distance(NPC.Center) <= EngageDistance)
            {
                SwitchBehavior(specificAttack: BehaviorState.EngageAnimation);
                RunEyeGlintEffect(0.5f);
            }

            // Standing still, occasionally switch directions.
            if (LocalAIState == 0f)
            {
                if (Timer > 0f && Timer % 45f == 0f && Main.rand.NextBool(12))
                {
                    Timer = 0f;
                    LocalAIState = 1f;
                    NPC.netUpdate = true;
                }

                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.8f;

                if (Timer % 45f == 0f && Main.rand.NextBool(4))
                    NPC.direction *= -1;
            }

            // Walking around aimlessly.
            if (LocalAIState == 1f)
            {
                if (Timer > 0f && Timer % 45f == 0f && Main.rand.NextBool(6))
                {
                    Timer = 0f;
                    LocalAIState = 0f;
                    NPC.netUpdate = true;
                }

                if (MathF.Abs(NPC.velocity.X) < Idle_MaxSpeed)
                    NPC.velocity.X += Idle_MaxAcceleration * NPC.direction;

                bool shouldJump = NPC.collideX || IsNPCApproachingHole();
                if (NPC.velocity.Y == 0f && shouldJump)
                    NPC.velocity.Y -= 6f;
            }

            SearchForTargetEveryFrame = true;
        }

        public void MainBehavior_Stunned(Player target)
        {
            if (NPC.velocity.Y == 0f)
                NPC.velocity.X *= 0.98f;

            // Jump back up right before resuming regular behavior.
            if (Timer >= StunnedTime - 75 && Timer <= StunnedTime)
            {
                if (Timer == StunnedTime - 75)
                    NPC.velocity.Y -= 6f;

                NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
            }

            if (Timer >= StunnedTime)
            {
                List<BehaviorState> possibleAttacks = [BehaviorState.HogCharge, BehaviorState.HorribleHoller, BehaviorState.JumpAndDash];
                SwitchBehavior(null, null, [.. possibleAttacks]);
            }

            SearchForTargetEveryFrame = true;
        }
        #endregion

        #region Attacks
        public void MainBehavior_ChasePlayer(Player target)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                GroundedMovement(target.Center, ChasePlayer_MaxSpeed, ChasePlayer_MaxAcceleration);
            }

            bool shouldJumpOverObstacle = IsNPCApproachingHole() || NPC.collideX;
            bool jumpUpToPlayer = Main.expertMode && MathHelper.Distance(NPC.Center.Y, target.Center.Y) > 64f;
            bool closeToPlayer = MathHelper.Distance(NPC.Center.X, target.Center.X) < 112f;
            if (NPC.velocity.Y == 0f && (shouldJumpOverObstacle || (jumpUpToPlayer && closeToPlayer)))
                NPC.velocity.Y -= 8f;

            bool closeEnoughToAttack = MathHelper.Distance(NPC.Center.X, target.Center.X) <= 64f;
            if (Timer >= ChaseTime || (closeEnoughToAttack && Timer >= 120f && NPC.velocity.Y == 0f))
            {
                List<BehaviorState> possibleAttacks = [BehaviorState.HogCharge, BehaviorState.JumpAndDash];
                SwitchBehavior(BehaviorState.ChasePlayer, null, [.. possibleAttacks]);
            }

            float idealRotation = (NPC.velocity.Y != 0f) ? NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi) : 0f;
            NPC.rotation = idealRotation;
        }

        public void MainBehavior_HogCharge(Player target)
        {
            if (LocalAIState == 0f)
            {
                if (Timer <= HogCharge_PreChargeTime)
                {
                    if (NPC.velocity.Y == 0f)
                    {
                        if (MathF.Abs(NPC.velocity.X) > 0.4f)
                            NPC.velocity.X *= 0.9f;
                        else
                            NPC.velocity.X -= 0.08f * NPC.direction;
                    }

                    NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                }

                if (Timer >= HogCharge_PreChargeTime)
                {
                    Timer = 0f;
                    LocalAIState = 1f;
                    NPC.netUpdate = true;
                }

                NPC.rotation = 0f;
            }

            if (LocalAIState == 1f)
            {
                bool finishedWithCharges = Timer >= HogCharge_ChargingTime || MiscAttackCounter >= HogCharge_MaxCharges;
                if (Timer <= 0f)
                {
                    // Kill the hitbox when slowing down.
                    KillChargeHitboxProjectile();

                    if (NPC.velocity.Y == 0f)
                        NPC.velocity.X *= 0.93f;
                    NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                    NPC.rotation = 0f;

                    Vector2 dustPosition = new(NPC.Bottom.X + Main.rand.NextFloat(-NPC.width * 0.5f, NPC.width * 0.5f), NPC.Bottom.Y);
                    Dust.NewDustPerfect(dustPosition, DustID.Cloud, new Vector2(NPC.velocity.X * 0.4f, Main.rand.NextFloat(-0.38f, 0.38f)), 120, Color.LightGray, Main.rand.NextFloat(1f, 1.2f));
                    if (Timer % 2 == 0f)
                        SoundEngine.PlaySound(SoundID.Item55 with { Pitch = -0.4f, Identifier = "Horrible Hog Slowdown" }, NPC.Center);

                    AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 0f, 0.15f);
                    if (Timer == -10 && !finishedWithCharges)
                        RunEyeGlintEffect(0.4f);
                }
                else
                {
                    // Spawn an artificial hitbox that can knockback and hit both hostile and friendly enemies alongside the player.
                    if (!CalamityUtils.AnyProjectiles(ModContent.ProjectileType<HorribleHogChargeHitbox>()) && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float knockback = 20f;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<HorribleHogChargeHitbox>(), Damage_HogCharge, knockback, ai0: NPC.whoAmI);
                    }

                    float maxSpeed = MathHelper.Lerp(HogCharge_MaxSpeed, HogCharge_MaxSpeed + 3.5f, MiscAttackCounter / HogCharge_MaxCharges);
                    float maxAcceleration = MathHelper.Lerp(HogCharge_MaxAcceleration, HogCharge_MaxAcceleration + 0.35f, MiscAttackCounter / HogCharge_MaxCharges);
                    if (MathF.Abs(NPC.velocity.X) < maxSpeed)
                        NPC.velocity.X += maxAcceleration * NPC.direction;

                    // Turn around if the player jumps over to the other side of it.
                    bool turnLeft = NPC.direction == 1 && (NPC.Center.X - target.Center.X) > HogCharge_MaxSlowdownDistance;
                    bool turnRight = NPC.direction == -1 && (NPC.Center.X - target.Center.X) < -HogCharge_MaxSlowdownDistance;
                    bool hitAWall = NPC.collideX && MathF.Abs(NPC.velocity.X) >= maxSpeed * 0.8f;
                    if (((turnLeft || turnRight) && Timer > 30f) || finishedWithCharges || hitAWall)
                    {
                        if (hitAWall)
                        {
                            NPC.velocity.X = NPC.oldVelocity.X * -0.64f;
                            NPC.velocity.Y -= 5f;
                        }

                        Timer = HogCharge_ChargeCooldownTime;
                        MiscAttackCounter++;
                        if (finishedWithCharges && PhaseTwo)
                            AltAttackVariant = Main.rand.Next(2);
                        NPC.netUpdate = true;
                    }

                    Vector2 dustPosition = new(NPC.Bottom.X + Main.rand.NextFloat(-NPC.width * 0.5f, NPC.width * 0.5f), NPC.Bottom.Y);
                    Dust.NewDustPerfect(dustPosition, DustID.Cloud, new Vector2(NPC.velocity.X * 0.2f, Main.rand.NextFloat(-0.3f, 0.3f)), 120, Color.LightGray, Main.rand.NextFloat(1f, 1.2f));
                    if (Timer % 7 == 0f)
                        SoundEngine.PlaySound(SoundID.Run with { Pitch = -0.4f, Identifier = "Horrible Hog Run" }, NPC.Center);

                    AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 1f, 0.15f);

                    if (MiscAttackCounter >= HogCharge_MaxCharges + 1)
                    {
                        KillChargeHitboxProjectile();
                        if (PhaseTwo && AltAttackVariant == 1f)
                        {
                            LocalAIState = 2f;
                            Timer = 0f;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            List<BehaviorState> possibleAttacks = [BehaviorState.HorribleHoller, BehaviorState.JumpAndDash];
                            SwitchBehavior(BehaviorState.HogCharge, null, [.. possibleAttacks]);
                        }
                    }

                    NPC.damage = 0;
                    NPC.rotation = (NPC.velocity.Y != 0f) ? NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi) : 0f;
                }
            }

            // Alt attack in phase two.
            // Same jump and dash but with a smaller shockwave.
            if (LocalAIState == 2f)
            {
                if (Timer == 0f)
                {
                    NPC.velocity.Y -= JumpAndDash_MaxJumpHeight * 1.2f;
                    NPC.velocity.X += JumpAndDash_MaxDashSpeed * 0.4f * NPC.direction;
                    SpawnJumpParticles();
                }

                if (Timer <= HogCharge_PreDashTime)
                {
                    NPC.velocity.Y *= 0.98f;
                    NPC.velocity.X *= 0.96f;

                    SetSquashVectors(new Vector2(1.06f, 0.94f));
                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                    SpriteRotation -= (MathHelper.TwoPi / 20f) * NPC.direction;

                    if (Timer <= HogCharge_PreDashTime - 10)
                    {
                        NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                        LastPlayerPosition = target.Center;
                        if (Timer == HogCharge_PreDashTime - 10)
                            RunEyeGlintEffect(0.6f);
                    }
                }

                if (Timer == HogCharge_PreDashTime)
                {
                    NPC.noGravity = true;
                    NPC.velocity = NPC.SafeDirectionTo(LastPlayerPosition) * JumpAndDash_MaxDashSpeed;

                    SetSquashVectors(new Vector2(1.12f, 0.96f));
                    SpriteRotation = 0f;
                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                }

                if (Timer >= HogCharge_PreDashTime && Timer <= HogCharge_PreDashTime + HogCharge_DashTime)
                {
                    if (NPC.collideX || NPC.collideY || Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        // Spawn shockwave on tile impact.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 2; i++)
                                SpawnShockwave(2, 6, (i > 0).ToDirectionInt(), 2f);
                        }

                        CalamityUtils.AddScreenshakeAt(NPC.Center, 6f);
                        Collision.HitTiles(NPC.position, NPC.velocity, NPC.width, NPC.height);
                        NPC.velocity.X = NPC.DirectionFrom(target.Center).SafeNormalize(Vector2.UnitX).X * 3f;
                        NPC.velocity.Y = -10f;
                        LocalAIState = 3f;
                        Timer = 0f;
                        NPC.netUpdate = true;
                    }

                    NPC.noGravity = true;
                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                }

                if (Timer >= HogCharge_PreDashTime + HogCharge_DashTime)
                {
                    if (NPC.velocity.Y == 0f)
                    {
                        LocalAIState = 3f;
                        Timer = 0f;
                        NPC.netUpdate = true;
                    }

                    NPC.GravityMultiplier *= 3f;
                    NPC.velocity.X *= 0.95f;
                    SpriteRotation = 0f;
                }

                SetSquashVectors(VelocityBasedSquashNStretch);
            }

            // Rebounding, land and switch attacks.
            if (LocalAIState == 3f)
            {
                if (NPC.velocity.Y == 0f && Timer > 2f)
                {
                    NPC.rotation = 0f;
                    SpriteRotation = 0f;
                    List<BehaviorState> possibleAttacks = [BehaviorState.HorribleHoller, BehaviorState.JumpAndDash, BehaviorState.VomitBarrage];
                    SwitchBehavior(BehaviorState.HogCharge, null, [.. possibleAttacks]);
                }

                SpriteRotation -= (MathHelper.TwoPi / 20f) * (NPC.velocity.X * 0.1f) * NPC.direction;
                NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                SetSquashVectors();
            }
        }

        public void MainBehavior_JumpAndDash(Player target)
        {
            // Stop, jump and dash towards the target.
            if (LocalAIState == 0f)
            {
                if (Timer <= JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime)
                {
                    NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                    if (Timer < JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime - 15)
                        LastPlayerPosition = target.Center;
                }

                if (Timer <= JumpAndDash_PreJumpTime)
                {
                    GroundedMovement(target.Center, ChasePlayer_MaxSpeed, ChasePlayer_MaxAcceleration, slowdownDistance: 160f);
                    float targetAngle = (NPC.velocity.Y != 0f) ? NPC.velocity.X * 0.175f * (NPC.velocity.Y < 0).ToDirectionInt() : 0f;
                    NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.075f);

                    int preJumpVisualsTime = JumpAndDash_PreJumpTime - 30;
                    if (Timer >= preJumpVisualsTime)
                    {
                        NPC.velocity.X *= 0.96f;
                        float interpolant = Utils.GetLerpValue(preJumpVisualsTime, VomitBarrage_PreJumpTime, Timer, true);
                        HorizontalShakeStrength = MathHelper.Lerp(0f, 6f, interpolant);
                        SetSquashVectors(new Vector2(1.24f, 0.84f));
                    }
                }

                if (Timer == JumpAndDash_PreJumpTime)
                {
                    NPC.velocity.Y -= JumpAndDash_MaxJumpHeight;
                    NPC.velocity.X += JumpAndDash_MaxDashSpeed * 0.2f * NPC.direction;

                    SpawnJumpParticles();
                    SetSquashVectors(new Vector2(0.84f, 1.14f));
                    HorizontalShakeStrength = 0f;
                }

                if (Timer >= JumpAndDash_PreJumpTime && Timer <= JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime)
                {
                    NPC.velocity.Y *= 0.98f;
                    NPC.velocity.X *= 0.96f;

                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                    SpriteRotation -= (MathHelper.TwoPi / 20f) * NPC.direction;
                    AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 1f, 0.15f);
                    if (Timer == JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime - 10)
                        RunEyeGlintEffect(0.4f);
                }

                if (Timer == JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime)
                {
                    NPC.noGravity = true;
                    NPC.velocity = NPC.SafeDirectionTo(LastPlayerPosition) * JumpAndDash_MaxDashSpeed;

                    SpriteRotation = 0f;
                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                }

                if (Timer >= JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime && Timer <= JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime + JumpAndDash_DashTime)
                {
                    if (NPC.collideX || NPC.collideY || Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        // Spawn shockwave on tile impact.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                int shockwaveAmt = PhaseTwo ? 14 : 8;
                                SpawnShockwave(2, shockwaveAmt, (i > 0).ToDirectionInt(), 3f);
                            }
                        }

                        CalamityUtils.AddScreenshakeAt(NPC.Center, 6f);
                        Collision.HitTiles(NPC.position, NPC.velocity, NPC.width, NPC.height);
                        NPC.velocity.X = NPC.DirectionFrom(target.Center).SafeNormalize(Vector2.UnitX).X * 5f;
                        NPC.velocity.Y = PhaseTwo ? -16f : -12f;
                        LocalAIState = 1f;
                        Timer = 0f;
                        AltAttackVariant = Main.rand.Next(2);
                        NPC.netUpdate = true;
                    }

                    NPC.noGravity = true;
                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                }

                if (Timer >= JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime + JumpAndDash_DashTime)
                {
                    if (NPC.velocity.Y == 0f)
                    {
                        LocalAIState = 2f;
                        Timer = 0f;
                        NPC.netUpdate = true;
                    }

                    NPC.GravityMultiplier *= 3f;
                    NPC.velocity.X *= 0.95f;
                    AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 0f, 0.15f);
                }

                if (Timer > JumpAndDash_PreJumpTime)
                    SetSquashVectors(VelocityBasedSquashNStretch);
            }

            // Rebounding after ground collision.
            if (LocalAIState == 1f)
            {
                // Randomly perform two smaller attacks right after the main dash in Phase two.
                if (PhaseTwo)
                {
                    // Shoot vomit chunks at the player.
                    // Shoot more in higher difficulties.
                    if (AltAttackVariant == 0f)
                    {
                        if (Timer <= JumpAndDash_PreVomitTime)
                        {
                            NPC.velocity *= 0.98f;
                            NPC.GravityMultiplier *= 0.84f;
                            if (Timer == JumpAndDash_PreVomitTime - 10)
                                RunEyeGlintEffect(0.6f);
                        }

                        if (Timer >= JumpAndDash_PreVomitTime)
                        {
                            if (Timer % JumpAndDash_VomitTimeInterval == 0f && MiscAttackCounter < JumpAndDash_MaxVomitChunks)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 chunkVelocity = NPC.SafeDirectionTo(target.Center).RotatedByRandom(MathHelper.ToRadians(30f)) * 14f;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, chunkVelocity, ModContent.ProjectileType<HorribleHogVomitChunk>(), Damage_VomitChunkProjectile, 0f);
                                }

                                Vector2 knockbackVelocity = NPC.DirectionFrom(target.Center).SafeNormalize(-Vector2.UnitY);
                                NPC.velocity = knockbackVelocity * new Vector2(3f, 5f);

                                SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                MiscAttackCounter++;
                                NPC.netUpdate = true;
                            }

                            if (MiscAttackCounter >= JumpAndDash_MaxVomitChunks && NPC.velocity.Y == 0f)
                            {
                                LocalAIState = 2f;
                                Timer = 0f;
                                NPC.netUpdate = true;
                            }
                        }

                        SetSquashVectors();
                        NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                        NPC.rotation = NPC.rotation.AngleLerp(NPC.AngleTo(target.Center) + (NPC.direction > 0 ? 0f : -MathHelper.Pi), 0.15f);
                        SpriteRotation = 0f;
                        AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 0f, 0.15f);
                    }

                    // Bounce on the ground near the player predictively.
                    // In Death Mode and above, perform a large ground pound after the last bounce.
                    if (AltAttackVariant == 1f)
                    {
                        // Using plus 1 here cause the first bounce is coming off of the rebound.
                        if (MiscAttackCounter <= JumpAndDash_MaxBounces)
                        {
                            if (NPC.velocity.Y == 0f)
                            {
                                if (MiscAttackCounter == JumpAndDash_MaxBounces)
                                {
                                    if (!CalamityWorld.death)
                                    {
                                        LocalAIState = 2f;
                                    }
                                    else
                                    {
                                        // Try to bounce in front of the player for the ground pound.
                                        Vector2 bounceVelocity = (target.Center.X > NPC.Center.X) ? new Vector2(18f, -20f) : new Vector2(-18f, -20f);
                                        NPC.velocity = bounceVelocity;
                                    }

                                    Timer = 0f;
                                }
                                else
                                {
                                    float speedByDistance = Utils.Remap(MathHelper.Distance(target.Center.X, NPC.Center.X), 60f, 600f, 4f, 10f, true);
                                    float xVelocity = speedByDistance * (target.Center.X > NPC.Center.X).ToDirectionInt();
                                    NPC.velocity = new Vector2(xVelocity, -11.25f);
                                }

                                if (Main.netMode != NetmodeID.MultiplayerClient && MiscAttackCounter > 0f)
                                {
                                    for (int i = 0; i < 2; i++)
                                        SpawnShockwave(2, 4, (i > 0).ToDirectionInt(), 2f);

                                    for (int i = 0; i < MiscAttackCounter; i++)
                                    {
                                        Vector2 rubbleVelocityRight = new Vector2(2f + i * 3f, -8f - i * 1.25f);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, rubbleVelocityRight, ModContent.ProjectileType<HorribleHogRubble>(), Damage_ShockwaveProjectile, 1f);
                                        Vector2 rubbleVelocityLeft = new Vector2(-2f - i * 3f, -8f - i * 1.25f);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, rubbleVelocityLeft, ModContent.ProjectileType<HorribleHogRubble>(), Damage_ShockwaveProjectile, 1f);
                                    }
                                }

                                CalamityUtils.AddScreenshakeAt(NPC.Center, 4f);
                                MiscAttackCounter++;
                                NPC.netUpdate = true;
                            }

                            if (NPC.velocity.Y > 1f && NPC.velocity.Y < 24f && MiscAttackCounter > 0f)
                            {
                                NPC.velocity.Y *= 1.12f;
                                NPC.velocity.X *= 0.96f;
                                NPC.noGravity = true;
                            }
                        }

                        // Ground pound attack in Death Mode.
                        if (CalamityWorld.death && MiscAttackCounter >= JumpAndDash_MaxBounces + 1)
                        {
                            if (Timer < JumpAndDash_PreGroundPoundTime)
                            {
                                NPC.velocity *= 0.96f;
                                NPC.GravityMultiplier *= 0.4f;
                                if (Timer == JumpAndDash_PreGroundPoundTime - 10)
                                    RunEyeGlintEffect(0.6f);
                            }
                            else
                            {
                                NPC.noGravity = true;
                            }
    
                            if (Timer == JumpAndDash_PreGroundPoundTime)
                            {
                                NPC.velocity.Y += 30f;
                                NPC.velocity.X *= 0f;
                            }

                            if (Timer > JumpAndDash_PreGroundPoundTime && NPC.velocity.Y == 0f)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    SpawnShockwave(2, 20, (target.Center.X > NPC.Center.X).ToDirectionInt(), 8f);

                                CalamityUtils.AddScreenshakeAt(NPC.Center, 8f);
                                Collision.HitTiles(NPC.position, NPC.velocity, NPC.width, NPC.height);
                                NPC.velocity.X = NPC.DirectionFrom(target.Center).SafeNormalize(Vector2.UnitX).X * 4f;
                                NPC.velocity.Y = -12f;
                                LocalAIState = 2f;
                                Timer = 0f;
                                NPC.netUpdate = true;
                            }
                        }

                        NPC.damage = 0;
                        NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                        SpriteRotation -= (MathHelper.TwoPi / 20f) * NPC.direction;
                        SetSquashVectors(VelocityBasedSquashNStretch);
                    }
                }
                else
                {
                    if (NPC.collideY && Timer > 2f)
                    {
                        LocalAIState = 2f;
                        Timer = 0f;
                        NPC.netUpdate = true;
                    }

                    SetSquashVectors();
                    SpriteRotation -= NPC.velocity.X * 0.1f * NPC.direction;
                    NPC.rotation = NPC.velocity.ToRotation() + (NPC.direction > 0 ? 0f : -MathHelper.Pi);
                    AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 0f, 0.15f);
                }
            }

            // Landed back on the ground.
            if (LocalAIState == 2f)
            {
                NPC.velocity.X *= 0.94f;
                if (NPC.velocity.Y == 0f)
                {
                    NPC.rotation = 0f;
                    SpriteRotation = 0f;
                }

                AfterimageTrailOpacity = MathHelper.Lerp(AfterimageTrailOpacity, 0f, 0.15f);
                SetSquashVectors();

                if (Timer >= JumpAndDash_CooldownTime)
                {
                    List<BehaviorState> possibleAttacks = [BehaviorState.VomitBarrage, BehaviorState.HorribleHoller, BehaviorState.HogCharge];
                    SwitchBehavior(BehaviorState.JumpAndDash, null, [.. possibleAttacks]);
                }
            }
        }

        public void MainBehavior_HorribleHoller(Player target)
        {
            NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
            if (NPC.velocity.Y == 0f)
                NPC.velocity.X *= 0.9f;

            if (Timer <= HorribleHoller_RoarTime)
            {
                if (Timer == HorribleHoller_RoarTime - 30)
                {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    CalamityUtils.AddScreenshakeAt(NPC.Center, 6f);
                    NPC.velocity.Y -= 6f;
                }

                // Spawn the Zombie Spawner projectiles on top of tiles.
                if (Timer == HorribleHoller_RoarTime)
                {
                    for (int i = 0; i < HorribleHoller_MaxZombies; i++)
                    {
                        int maxTileRange = 12;
                        Vector2 zombieSpawnPosition = NPC.Center + new Vector2(Main.rand.Next(-maxTileRange * 16, (maxTileRange + 1) * 16), 0f);
                        Point posInTileCoords = zombieSpawnPosition.ToTileCoordinates();

                        Vector2 actualSpawnPosition = new();
                        for (int x = -maxTileRange; x < maxTileRange; x++)
                        {
                            for (int y = -maxTileRange; y < maxTileRange; y++)
                            {
                                Point tilePoint = posInTileCoords + new Point(x, y);
                                Tile tile = Main.tile[tilePoint.X, tilePoint.Y];
                                Tile tileAbove = Main.tile[tilePoint.X, tilePoint.Y - 1];
                                Tile tileBelow = Main.tile[tilePoint.X, tilePoint.Y + 1];

                                bool solidTile = tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && tile.Slope == 0;
                                if (solidTile && !tileAbove.HasTile && tileBelow.HasTile)
                                {
                                    actualSpawnPosition = tilePoint.ToWorldCoordinates() - Vector2.UnitY;
                                    break;
                                }
                            }
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int maxTime = 150 + Main.rand.Next(31);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), actualSpawnPosition, Vector2.Zero, ModContent.ProjectileType<HorribleHogZombieSpawner>(), 0, 0f, Main.myPlayer, 0f, maxTime);
                        }
                    }
                }
            }

            float targetAngle = (NPC.velocity.Y != 0f) ? NPC.velocity.X * 0.125f * (NPC.velocity.Y < 0).ToDirectionInt() : 0f;
            if (Timer >= HorribleHoller_RoarTime - 30 && Timer <= HorribleHoller_RoarTime)
                targetAngle = (NPC.velocity.Y != 0f) ? (NPC.direction < 0 ? MathHelper.ToRadians(50f) : MathHelper.ToRadians(-50f)) : 0f;

            NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.125f);

            if (Timer >= HorribleHoller_RoarTime + HorribleHoller_PostRoarCooldownTime)
            {
                List<BehaviorState> possibleAttacks = [BehaviorState.ChasePlayer, BehaviorState.VomitBarrage];
                SwitchBehavior(BehaviorState.HorribleHoller, null, [.. possibleAttacks]);
            }
        }

        public void MainBehavior_VomitBarrage(Player target)
        {
            if (LocalAIState == 0f)
            {
                if (Timer == 1f)
                {
                    SoundEngine.PlaySound(SoundID.DD2_DrakinBreathIn, NPC.Center);
                    if (PhaseTwo && CalamityWorld.revenge)
                    {
                        AltAttackVariant = Main.rand.Next(2);
                        NPC.netUpdate = true;
                    }
                }

                // Move away from the player.
                if (Timer <= VomitBarrage_PreJumpTime)
                {
                    NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
                    GroundedMovement(target.Center, ChasePlayer_MaxSpeed, ChasePlayer_MaxAcceleration, slowdownDistance: 160f);

                    float targetAngle = (NPC.velocity.Y != 0f) ? NPC.velocity.X * 0.135f * (NPC.velocity.Y < 0).ToDirectionInt() : 0f;
                    NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.125f);

                    int preJumpVisualsTime = VomitBarrage_PreJumpTime - 30;
                    if (Timer >= preJumpVisualsTime)
                    {
                        NPC.velocity.X *= 0.96f;
                        float interpolant = Utils.GetLerpValue(preJumpVisualsTime, VomitBarrage_PreJumpTime, Timer, true);
                        HorizontalShakeStrength = MathHelper.Lerp(0f, 6f, interpolant);
                        SetSquashVectors(new Vector2(1.24f, 0.84f));
                    }
                }

                // Jump and shoot a bunch of vomit projectiles shortly afterwards.
                if (Timer == VomitBarrage_PreJumpTime)
                {
                    SetSquashVectors(squashVector: new Vector2(0.84f, 1.14f));
                    NPC.velocity.Y -= JumpAndDash_MaxJumpHeight;
                    NPC.velocity.X -= 6f * NPC.direction;
                    HorizontalShakeStrength = 0f;
                    SpawnJumpParticles(6, 9);
                }

                if (Timer >= VomitBarrage_PreJumpTime && Timer <= VomitBarrage_PreJumpTime + VomitBarrage_VomitTime)
                {
                    NPC.velocity.Y *= 0.98f;
                    NPC.velocity.X *= 0.96f;

                    float targetAngle = NPC.direction < 0 ? MathHelper.ToRadians(50f) : MathHelper.ToRadians(-50f);
                    NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.075f);
                }

                if (Timer == JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime)
                {
                    bool spawnEyeballs = PhaseTwo && CalamityWorld.revenge && AltAttackVariant == 1f;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < VomitBarrage_MaxVomitChunks; i++)
                        {
                            int projectileType = ModContent.ProjectileType<HorribleHogVomitChunk>();
                            int damage = Damage_VomitChunkProjectile;

                            // Randomly replace chunks with bombs in Expert and above.
                            bool vomitBombFloorNotReached = MiscAttackCounter < VomitBarrage_MinVomitBombs;
                            bool vomitBombCeilingNotReached = Main.rand.NextBool(4) && MiscAttackCounter >= VomitBarrage_MinVomitBombs && MiscAttackCounter < VomitBarrage_MaxVomitBombs;
                            if (Main.expertMode && (vomitBombFloorNotReached || vomitBombCeilingNotReached))
                            {
                                projectileType = ModContent.ProjectileType<HorribleHogVomitBomb>();
                                damage = Damage_VomitBombProjectile;
                                MiscAttackCounter++;
                            }

                            // Replace chunks with homing Demon Eyes in Rev+ during phase two when the alt attack is picked.
                            if (spawnEyeballs)
                            {
                                projectileType = ModContent.ProjectileType<HorribleHogVomitEye>();
                                damage = Damage_VomitEyeProjectile;
                            }

                            Vector2 vomitVelocity = CalamityUtils.GetProjectilePhysicsFiringVelocity(NPC.Center, target.Center + new Vector2(Main.rand.NextFloat(-16f, 16f), 0f), 0.125f, Main.rand.NextFloat(5f, 8f));
                            vomitVelocity.X *= Main.rand.NextFloat(0.25f, 2.25f);
                            int vomit = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vomitVelocity, projectileType, damage, 0f);
                            if (spawnEyeballs)
                                Main.projectile[vomit].ai[0] = Main.rand.Next(45, 75);
                        }
                    }

                    SoundEngine.PlaySound(SoundID.DD2_DrakinShot, NPC.Center);
                    NPC.velocity.X = 6f * -NPC.direction;
                    NPC.velocity.Y += 2f;
                }

                if (Timer > JumpAndDash_JumpPreDashTime + JumpAndDash_PreJumpTime && NPC.velocity.Y == 0f)
                {
                    LocalAIState = 1f;
                    Timer = 0f;
                    NPC.netUpdate = true;
                }
            }

            if (LocalAIState == 1f)
            {
                if (Timer >= VomitBarrage_PostVomitCooldown)
                {
                    List<BehaviorState> possibleAttacks = [BehaviorState.HorribleHoller, BehaviorState.HogCharge];
                    SwitchBehavior(BehaviorState.VomitBarrage, null, [.. possibleAttacks]);
                }

                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.9f;

                float targetAngle = (NPC.velocity.Y != 0f) ? NPC.velocity.X * 0.135f * (NPC.velocity.Y < 0).ToDirectionInt() : 0f;
                NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.125f);
                NPC.direction = (target.Center.X > NPC.Center.X).ToDirectionInt();
            }
        }
        #endregion

        public void SwitchBehavior(BehaviorState? attackToRecord = null, BehaviorState? specificAttack = null, params BehaviorState[] attacksToChooseFrom)
        {
            // Reset all the previous attack counters and weights in order to start a new cycle once the maximum amount of attacks overall has been reached.
            if (MainAttackCounter >= MaxAttacksPerCycle)
            {
                foreach (BehaviorState attack in PreviousAttackCounters.Keys)
                    PreviousAttackCounters[attack] = 0;
                ResetAttackWeights();
                MainAttackCounter = 0f;
            }

            if (attackToRecord.HasValue)
            {
                // Record the last attack to a dictionary and increment how many times it has been performed.
                if (!PreviousAttackCounters.ContainsKey(attackToRecord.Value))
                    PreviousAttackCounters[attackToRecord.Value] = 0;
                PreviousAttackCounters[attackToRecord.Value]++;

                // Increase the weight of other attacks and lower this one.
                AttackWeights[attackToRecord.Value] -= 0.33f;
                foreach (BehaviorState attack in AttackWeights.Keys)
                    AttackWeights[attack] += 0.33f;

                MainAttackCounter++;
            }

            // Default to returning to idling in the event there are no nearby targets after performing an attack.
            BehaviorState nextAttack = BehaviorState.Idle;

            // Switch to a specific behavior state if one is specified.
            // Otherwise, pick from the random attack array.
            if (specificAttack.HasValue)
            {
                nextAttack = specificAttack.Value;
            }
            else if (NPC.HasValidTarget && attacksToChooseFrom.Length > 0)
            { 
                WeightedRandom<BehaviorState> possibleAttacks = new();
                for (int i = 0; i < attacksToChooseFrom.Length; i++)
                {
                    // Don't add attacks to the attack pool if they've been performed their maximum amount of times.
                    if (PreviousAttackCounters.TryGetValue(attacksToChooseFrom[i], out var timesPerformed) && timesPerformed >= GetMaxAttackValue(attacksToChooseFrom[i]))
                        continue;

                    possibleAttacks.Add(attacksToChooseFrom[i], AttackWeights[attacksToChooseFrom[i]]);
                }

                // Pick a random attack.
                nextAttack = possibleAttacks;
            }

            // Reset so this once Hog has another player to target so it doesn't loop. 
            if (NPC.HasValidTarget)
                ReadyToPlayLaughingAnimation = true;

            // Switch and reset certain fields.
            AIState = (int)nextAttack;
            LocalAIState = 0f;
            Timer = 0f;
            MiscAttackCounter = 0f;
            AltAttackVariant = 0f;
            SpriteRotation = 0f;
            AfterimageTrailOpacity = 0f;
            SearchForTargetEveryFrame = false;

            SetSquashVectors();
            KillChargeHitboxProjectile();

            NPC.netUpdate = true;
        }

        private int GetMaxAttackValue(BehaviorState attack)
        {
            int maxValue = attack switch
            {
                BehaviorState.ChasePlayer => MaxAttacks_ChasePlayer,
                BehaviorState.HogCharge => MaxAttacks_HogCharge,
                BehaviorState.JumpAndDash => MaxAttacks_JumpAndDash,
                BehaviorState.HorribleHoller => MaxAttacks_HorribleHoller,
                BehaviorState.VomitBarrage => MaxAttacks_VomitBarrage,
                _ => 1
            };

            // Every attack can be used one more time in phase two.
            if (PhaseTwo)
                maxValue += 1;

            return maxValue;
        }

        private void ResetAttackWeights()
        {
            AttackWeights[BehaviorState.ChasePlayer] = 1f;
            AttackWeights[BehaviorState.HogCharge] = 0.33f;
            AttackWeights[BehaviorState.JumpAndDash] = 1f;
            AttackWeights[BehaviorState.HorribleHoller] = 0.33f;
            AttackWeights[BehaviorState.VomitBarrage] = 0.66f;
        }

        private void GroundedMovement(Vector2 targetPosition, float maxSpeed, float maxAcceleration, float jumpHeight = 6f, float? slowdownDistance = null)
        {
            float distanceToPlayer = MathF.Abs(NPC.Center.X - targetPosition.X);
            if (targetPosition.X > NPC.Center.X)
            {
                if (slowdownDistance.HasValue)
                {
                    if (distanceToPlayer > slowdownDistance.Value)
                        NPC.velocity.X += maxAcceleration;
                    else
                        NPC.velocity.X -= maxAcceleration;
                }
                else
                {
                    NPC.velocity.X += maxAcceleration;
                }

            }
            else if (targetPosition.X < NPC.Center.X)
            {
                if (slowdownDistance.HasValue)
                {
                    if (distanceToPlayer > slowdownDistance.Value)
                        NPC.velocity.X -= maxAcceleration;
                    else
                        NPC.velocity.X += maxAcceleration;
                }
                else
                {
                    NPC.velocity.X -= maxAcceleration;
                }
            }

            if (NPC.velocity.Y == 0f)
            {
                if (NPC.collideX || IsNPCApproachingHole())
                    NPC.velocity.Y -= jumpHeight;
            }

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
        }

        private void SpawnShockwave(int spawnImterval, int maxShockwaves, int direction, float heightMultiplier)
        {
            int spawnerIndex = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, Vector2.Zero, ModContent.ProjectileType<HorribleHogShockwaveSpawner>(), Damage_ShockwaveProjectile, 0f);
            Main.projectile[spawnerIndex].ai[1] = spawnImterval;
            Main.projectile[spawnerIndex].ai[2] = maxShockwaves;
            Main.projectile[spawnerIndex].localAI[1] = direction;
            Main.projectile[spawnerIndex].localAI[2] = heightMultiplier;
            Main.projectile[spawnerIndex].ModProjectile<HorribleHogShockwaveSpawner>().OwnerIndex = NPC.whoAmI;
            Main.projectile[spawnerIndex].netUpdate = true;
        }

        private void SpawnJumpParticles(int dustCloudMin = 10, int dustCloudMax = 14, int dirtDustMin = 14, int dirtDustMax = 18)
        {
            int dustCloudAmt = Main.rand.Next(dustCloudMin, dustCloudMax + 1);
            for (int i = 0; i < dustCloudAmt; i++)
            {
                Vector2 spawnPosition = NPC.Bottom + Main.rand.NextVector2Circular(32f, 0f);
                Vector2 velocity = NPC.velocity * (Main.rand.NextFloat(0.1f, 0.2f) + i * 0.1f);
                Color color = Color.Lerp(Color.SaddleBrown, Color.SandyBrown, Main.rand.NextFloat());
                float rotationSpeed = Main.rand.NextFloat(0.01f, 0.03f) * Main.rand.NextBool().ToDirectionInt();

                TimedSmokeParticle launchCloud = new(spawnPosition, velocity, color, color, Main.rand.NextFloat(0.4f, 0.6f), Main.rand.NextFloat(0.6f, 0.8f), Main.rand.Next(30, 45), rotationSpeed);
                GeneralParticleHandler.SpawnParticle(launchCloud, true, Enums.GeneralDrawLayer.BeforeSolidTiles);
            }

            int dustAmt = Main.rand.Next(dirtDustMin, dirtDustMax + 1);
            for (int i = 0; i < dustAmt; i++)
            {
                Vector2 velocity = NPC.velocity * (Main.rand.NextFloat(0.1f, 0.2f) + i * 0.025f);
                Dust.NewDust(NPC.Bottom, 0, 0, DustID.Dirt, velocity.X, velocity.Y);
            }
        }

        private void KillChargeHitboxProjectile()
        {
            if (CalamityUtils.AnyProjectiles(ModContent.ProjectileType<HorribleHogChargeHitbox>()))
            {
                int hitboxIndex = CalamityUtils.FindFirstProjectile(ModContent.ProjectileType<HorribleHogChargeHitbox>());
                if (Main.projectile.IndexInRange(hitboxIndex) && Main.projectile[hitboxIndex].ai[0] == NPC.whoAmI)
                {
                    Main.projectile[hitboxIndex].Kill();
                    Main.projectile[hitboxIndex].netUpdate = true;
                }
            }
        }

        private bool IsNPCApproachingHole()
        {
            int npcWidthInTiles = NPC.width / 16;
            int tileX = (int)(NPC.Center.X / 16f) - npcWidthInTiles;
            if (NPC.velocity.X > 0)
                tileX += npcWidthInTiles;

            int tileY = (int)((NPC.position.Y + NPC.height) / 16f);
            for (int y = tileY; y < tileY + 2; y++)
            {
                for (int x = tileX; x < tileX + npcWidthInTiles; x++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<DisgustingMeat>(), 1, 1, 1);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            Texture2D baseTexture = TextureAssets.Npc[Type].Value;
            SpriteEffects effects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPosition = NPC.Center + Vector2.UnitY * NPC.gfxOffY - screenPos;
            Vector2 scale = NPC.scale * SquashVector;

            // Horrible Hog and its afterimage trail.
            using (spriteBatch.Scope())
            {
                Effect rotateSpriteShader = CalamityShaders.RotateSprite.Value;
                rotateSpriteShader.Parameters["rotation"].SetValue(SpriteRotation);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, rotateSpriteShader, Main.GameViewMatrix.TransformationMatrix);

                if (CalamityClientConfig.Instance.Afterimages && AfterimageTrailOpacity > 0.05f)
                {
                    for (int i = 0; i < NPCID.Sets.TrailCacheLength[Type]; i++)
                    {
                        Color afterimageColor = Color.Red * AfterimageTrailOpacity * 0.76f;
                        afterimageColor *= (NPCID.Sets.TrailCacheLength[Type] - i) / 5f;
                        Vector2 afterimageDrawPosition = NPC.oldPos[i] + NPC.Size * 0.5f - screenPos;
                        spriteBatch.Draw(baseTexture, afterimageDrawPosition, NPC.frame, NPC.GetAlpha(afterimageColor), NPC.rotation, NPC.frame.Size() * 0.5f, scale, effects, 0f);
                    }
                }
                spriteBatch.Draw(baseTexture, drawPosition + Main.rand.NextVector2Circular(HorizontalShakeStrength, 0f), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, scale, effects, 0f);

                spriteBatch.End();
            }

            //string attack = $"{((BehaviorState)AIState).ToString()}\n{MainAttackCounter}";
            //Vector2 stringDrawPosition = drawPosition - Vector2.UnitY * 64f;
            //ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, attack, stringDrawPosition, Color.Green, 0f, Vector2.One, Vector2.One);

            // Eye glint.
            if (EyeGlintScale > 0.05f)
            {
                Vector2 eyeGlintDrawPosition = drawPosition + new Vector2(6f * NPC.spriteDirection, -2f).RotatedBy(NPC.rotation) + Main.rand.NextVector2Circular(HorizontalShakeStrength, 0f);

                spriteBatch.SetBlendState(CalamityUtils.SubtractiveBlending);
                for (int i = 0; i < 2; i++)
                    spriteBatch.Draw(ShineFlare.Value, eyeGlintDrawPosition, null, NPC.GetAlpha(Color.White) * 0.7f, NPC.rotation, ShineFlare.Size() * 0.5f, EyeGlintScale, 0, 0f);
                spriteBatch.SetBlendState(BlendState.AlphaBlend);

                spriteBatch.Draw(ShineFlare.Value, eyeGlintDrawPosition, null, NPC.GetAlpha(Color.Red) with { A = 0 }, NPC.rotation, ShineFlare.Size() * 0.5f, EyeGlintScale * 0.8f, 0, 0f);
                spriteBatch.Draw(ShineFlare.Value, eyeGlintDrawPosition, null, NPC.GetAlpha(Color.White) with { A = 0 }, NPC.rotation, ShineFlare.Size() * 0.5f, EyeGlintScale * 0.4f, 0, 0f);
            }

            return false;
        }

        private void SetSquashVectors(Vector2? squashVectorTarget = null, Vector2? squashVector = null)
        {
            SquashVectorTarget = squashVectorTarget ?? Vector2.One;
            if (squashVector.HasValue)
                SquashVector = squashVector.Value;
        }

        private void RunEyeGlintEffect(float scale)
        {
            EyeGlintScale = scale;
            float pitch = Utils.Remap(scale, 0.4f, 1f, 0.7f, 0.9f, true);
            float volume = Utils.Remap(scale, 0.4f, 1f, 1.3f, 1.6f, true);
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = volume, Pitch = pitch }, NPC.Center);
        }
    }
}
