using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.DataStructures;
using CalamityMod.Effects;
using CalamityMod.Items.Materials;
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
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace CalamityMod.NPCs.NormalNPCs.HorribleHog
{
    public partial class HorribleHog : ModNPC
    {
        public enum BehaviorState
        {
            // Transitional behaviors.
            EngageAnimation = -4,
            LaughAtDeadPlayer = -3,
            DespawnAnimation = -2,
            DeathAnimation = -1,
            Idle,
            DigTowardsTarget,

            // Attacks.
            ChasePlayer,
            HogCharge,
            JumpAndDash,
            HorribleHoller,
            VomitBarrage,
        }

        private static Asset<Texture2D> BloomCircle;
        private static Asset<Texture2D> ShineFlare;
        private static Asset<Texture2D> VortexTexture;
        private static Asset<Texture2D> VortexTextureSecondary;
        private static Asset<Texture2D> VortexDistortionTexture;

        private static SoundStyle HitSound = new("CalamityMod/Sounds/NPCHit/HorribleHogHit", 3);
        private static SoundStyle DeathSound = new("CalamityMod/Sounds/NPCKilled/HorribleHogDeath");
        private static SoundStyle CackleSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogCackle");
        private static SoundStyle DashGruntSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogDashGrunt");
        private static SoundStyle JumpSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogJump", 2);
        private static SoundStyle GroundImpactSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogGroundImpact", 2);
        private static SoundStyle VomitChargeUpSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogVomitChargeUp", 2);
        private static SoundStyle VomitSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogVomit", 2);
        private static SoundStyle RetreatSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogRetreat");
        private static SoundStyle IdleSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogIdle", 2)
        {
            PitchVariance = 0.25f,
        };
        private static SoundStyle DiggingSlowSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogDiggingSlow")
        {
            IsLooped = true,
            PauseBehavior = PauseBehavior.PauseWithGame,
            MaxInstances = 0
        };
        private static SoundStyle DiggingFastSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogDiggingFast")
        {
            IsLooped = true,
            PauseBehavior = PauseBehavior.PauseWithGame,
            MaxInstances = 0
        };
        private static SoundStyle DevilsTongueLoopingSound = new("CalamityMod/Sounds/Custom/HorribleHog/HorribleHogNearbyLoop")
        {
            IsLooped = true,
            PauseBehavior = PauseBehavior.PauseWithGame,
            MaxInstances = 0
        };

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

        public float DevilsTongueVolumeMultiplier;

        /// <summary>
        /// Used as a shader parameter to rotate the already-drawn sprite of Horrible Hog directly regardless of the rotation value of <see cref="NPC.rotation"/>. <br></br>
        /// Used in cases where Horrible Hog needs to spin while also squashing and stretching. <see cref="NPC.rotation"/> is used for the spritebatch-drawn texture <br></br>
        /// and needs to rotate towards the NPC's velocity in order for the squashed sprite to face the correct direction. This parameter is then used and a shader is <br></br>
        /// then applied to the spritebatch to rotate the sprite differently from the value passed in spritebatch's Draw call.
        /// </summary>
        public float SpriteRotation;

        public Vector2 LastPlayerPosition;

        public Vector2 DiggingEmergeSpot;

        public Vector2 SquashVector;

        public Vector2 SquashVectorTarget;

        public SlotId DeathLaughSoundSlot;

        public SlotId DiggingSoundSlot;

        public SlotId DevilsTongueSlot;

        #region Static Behavior Properties
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

        public static float EngageDistance => 300f;
        public static float Idle_MaxSpeed => 2f;
        public static float Idle_MaxAcceleration => 0.125f;

        public static int MaxTimeToStartDigging => 300;

        public static int DigTowardsTarget_PreJumpTime => 30;
        public static int DigTowardsTarget_FindSuitablePositionTime => 120;
        public static int DigTowardsTraget_MaxDiggingTime => 180;

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
        public static float VomitBarrage_MaxSpeed => 8f;
        public static float VomitBarrage_MaxAcceleration => 0.45f;
        #endregion

        public ref float Timer => ref NPC.ai[0];

        public ref float AIState => ref NPC.ai[1];

        public ref float LocalAIState => ref NPC.ai[2];

        public ref float MainAttackCounter => ref NPC.ai[3];

        public ref float MiscAttackCounter => ref NPC.localAI[0];

        public ref float AltAttackVariant => ref NPC.localAI[1];

        public ref float DigTimer => ref NPC.localAI[2];

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
                VortexTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2");
                VortexTextureSecondary = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak");
                VortexDistortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Perlin");
            }
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
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
            NPC.rarity = 4;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.HitSound = HitSound;
            NPC.DeathSound = DeathSound;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = false;

            SquashVector = Vector2.One;
            SquashVectorTarget = Vector2.One;
            ResetAttackWeights();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.HorribleHog")
            });
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

            writer.WriteFlags(SearchForTargetEveryFrame, HasPlayedEngageAnimation, HasPlayedDeathAnimation, ReadyToPlayLaughingAnimation);
            writer.WritePackedWorldPosition(LastPlayerPosition);
            writer.WritePackedWorldPosition(DiggingEmergeSpot);

            for (int i = 0; i < 3; i++)
                writer.Write(NPC.localAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            int attackCountersLength = reader.ReadByte();
            for (int i = 0; i < attackCountersLength; i++)
                PreviousAttackCounters.Add((BehaviorState)reader.ReadByte(), (int)reader.ReadByte());

            int attackWeightsLength = reader.ReadByte();
            for (int i = 0; i < attackWeightsLength; i++)
                AttackWeights.Add((BehaviorState)reader.ReadByte(), (float)reader.ReadByte());

            reader.ReadFlags(out SearchForTargetEveryFrame, out HasPlayedEngageAnimation, out HasPlayedDeathAnimation, out ReadyToPlayLaughingAnimation);
            LastPlayerPosition = reader.ReadPackedWorldPosition();
            DiggingEmergeSpot = reader.ReadPackedWorldPosition();

            for (int i = 0; i < 3; i++)
                NPC.localAI[i] = reader.ReadSingle();
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
            // If Hog is not in any of the transitional behavior states, don't fall through platforms.
            bool inTransitionalBehaviorState = AIState <= (int)BehaviorState.Idle;
            if (inTransitionalBehaviorState)
                return false;

            if (NPC.HasValidTarget)
            {
                Player target = Main.player[NPC.target];
                return target.Center.Y - 8 > NPC.Center.Y;
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
            NPC.chaseable = true;
            NPC.spriteDirection = NPC.direction;

            // Despawn immediately if it's morning.
            if (Main.dayTime && AIState != (int)BehaviorState.DespawnAnimation)
                SwitchBehavior(specificAttack: BehaviorState.DespawnAnimation);

            // If the current target dies, laugh at them.
            if (target.dead && ReadyToPlayLaughingAnimation && NPC.velocity.Y == 0f)
            {
                ReadyToPlayLaughingAnimation = false;
                SwitchBehavior(specificAttack: BehaviorState.LaughAtDeadPlayer);
            }

            // If the target is unable to be reached, count up to 5 seconds before switching behavior states and digging towards them.
            int tileRange = 64;
            Vector2 npcCanHitPosition = NPC.position - new Vector2(tileRange, tileRange);
            Vector2 targetCanHitPosition = target.position - new Vector2(tileRange, tileRange);
            bool currentlyDoingAnAttack = AIState > (int)BehaviorState.DigTowardsTarget;
            bool targetTooHigh = (NPC.Center.Y - target.Center.Y >= 160f && target.velocity.Y == 0f) || NPC.Center.Y - target.Center.Y >= 320f;
            bool targetTooFar = NPC.Distance(target.Center) >= 1280f;
            bool cantHitTarget = !Collision.CanHit(npcCanHitPosition, NPC.width + tileRange, NPC.height + tileRange, targetCanHitPosition, target.width + tileRange, target.height + tileRange);
            if (NPC.HasValidTarget && currentlyDoingAnAttack && (targetTooHigh || targetTooFar || cantHitTarget))
            {
                DigTimer++;
                if (DigTimer >= 300f && NPC.velocity.Y == 0f)
                    SwitchBehavior(specificAttack: BehaviorState.DigTowardsTarget);
            }
            else
            {
                if (DigTimer > 0f)
                    DigTimer--;
            }

            // Adjust volume correctly depending on the behavior state and play the nearby loop sound.
            float volumeTarget = AIState == (int)BehaviorState.Idle ? 1f : 0f;
            DevilsTongueVolumeMultiplier = MathHelper.Lerp(DevilsTongueVolumeMultiplier, volumeTarget, 0.075f);
            PlayNearbyLoopingSound();

            switch ((BehaviorState)AIState)
            {
                case BehaviorState.EngageAnimation:
                    MainBehavior_EngageAnimation(target);
                    break;

                case BehaviorState.LaughAtDeadPlayer:
                    MainBehavior_LaughADeadPlayer();
                    break;

                case BehaviorState.DespawnAnimation:
                    MainBehavior_DespawnAnimation();
                    break;

                case BehaviorState.DeathAnimation:
                    MainBehavior_DeathAnimation();
                    break;

                case BehaviorState.Idle:
                    MainBehavior_Idle(target);
                    break;

                case BehaviorState.DigTowardsTarget:
                    MainBehavior_DigTowardsTarget(target);
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

        public void SwitchBehavior(BehaviorState? attackToRecord = null, BehaviorState? specificAttack = null, params BehaviorState[] attacksToChooseFrom)
        {
            // Reset all the previous attack counters and weights in order to start a new cycle once the maximum amount of attacks overall has been reached.
            if (MainAttackCounter >= MaxAttacksPerCycle)
            {
                // Also search for the nearest target again once the attack cycle resets.
                NPC.TargetClosest(false);

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

        private void GroundedMovement(Vector2 targetPosition, float maxSpeed, float maxAcceleration, float jumpHeight = 10f, float? slowdownDistance = null)
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

        private Point FindSuitableGround(Point basePoint)
        {
            // Tile is solid. Check to ensure the tile above is also isn't solid and move up if it is.
            if (WorldGen.ActiveAndWalkableTile(basePoint.X, basePoint.Y))
            {
                while (WorldGen.ActiveAndWalkableTile(basePoint.X, basePoint.Y - 1) && basePoint.Y >= 1)
                    basePoint.Y--;
            }
            // Tile isn't solid. Check to ensure the tile under it is solid and move down if it isn't.
            else
            {
                while (!WorldGen.ActiveAndWalkableTile(basePoint.X, basePoint.Y + 1) && basePoint.Y < Main.maxTilesY)
                    basePoint.Y++;
            }

            return basePoint;
        }

        private void PlayNearbyLoopingSound()
        {
            if (DevilsTongueVolumeMultiplier < 0.05f)
                return;

            // "Devil's Tongue" looping sound.
            // Similar to Divine Swine; gets louder and lowers music volume based on proximity.
            if (!SoundEngine.TryGetActiveSound(DevilsTongueSlot, out _))
                DevilsTongueSlot = SoundEngine.PlaySound(DevilsTongueLoopingSound, NPC.Center, DevilsTongueLoopCallback);

            float musicVolumeInterpolant = Utils.Remap(NPC.Distance(Main.LocalPlayer.Center), 600f, 100f, 1f, 0.05f, true);
            Main.musicFade[Main.curMusic] = MathHelper.Lerp(1f, musicVolumeInterpolant, DevilsTongueVolumeMultiplier);
        }

        private bool DevilsTongueLoopCallback(ActiveSound soundInstance)
        {
            soundInstance.Position = NPC.Center;
            float volumeInterpolant = Utils.Remap(NPC.Distance(Main.LocalPlayer.Center), 600f, 100f, 0.1f, 0.7f, true) * DevilsTongueVolumeMultiplier;
            soundInstance.Volume = volumeInterpolant;
            return NPC.active && DevilsTongueVolumeMultiplier >= 0.05f;
        }

        private void SpawnJumpParticles(int dustCloudMin = 10, int dustCloudMax = 14, int dirtDustMin = 14, int dirtDustMax = 18)
        {
            // Do fart in a jar visuals when Hog does a jump mid-air.
            float tileCollisionDistance = CalamityUtils.DistanceToTileCollisionHit(NPC.Bottom, Vector2.UnitY, 10) ?? 9999f;
            if (tileCollisionDistance > 18f)
            {
                int fartCloudAmt = Main.rand.Next(dustCloudMin, dustCloudMax + 1);
                for (int i = 0; i < fartCloudAmt; i++)
                {
                    Vector2 spawnPosition = NPC.Bottom + Main.rand.NextVector2Circular(32f, 0f);
                    Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                    int goreType = Main.rand.Next(GoreID.FartCloud1, GoreID.FartCloud3 + 1);
                    Gore.NewGore(spawnPosition, velocity, goreType);
                }

                int fartDustAmt = Main.rand.Next(dirtDustMin, dirtDustMax + 1);
                for (int i = 0; i < fartDustAmt; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                    Dust.NewDust(NPC.Bottom, 0, 0, DustID.FartInAJar, velocity.X, velocity.Y, Scale: Main.rand.NextFloat(0.8f, 1.2f));
                }

                SoundEngine.PlaySound(SoundID.Item16, NPC.Center);
            }
            else
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
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && NPC.downedBoss1 && NPC.CountNPCS(Type) < 1)
            {
                float spawnChanceMultiplier = CalamityWorld.death ? 0.0075f : 0.025f;
                return SpawnCondition.OverworldNightMonster.Chance * spawnChanceMultiplier;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Death effects spawned in MainBehavior_DeathAnimation.
            if (NPC.life <= 0)
                return;

            int dustAmt = Main.rand.Next(3, 7);
            if (hit.Crit)
                dustAmt += Main.rand.Next(2, 5);

            for (int i = 0; i < dustAmt; i++)
            {
                int dustType = Utils.SelectRandom(Main.rand, DustID.ToxicBubble, DustID.GreenBlood, DustID.Blood);
                Vector2 velocity = new Vector2(hit.HitDirection * Main.rand.NextFloat(1f, 2f), Main.rand.NextFloat(-2f, 2f));
                float scale = Main.rand.NextFloat(0.8f, 1.2f);
                if (hit.Crit)
                {
                    velocity *= Main.rand.NextFloat(1.25f, 1.75f);
                    scale += Main.rand.NextFloat(0.25f, 0.5f);
                }

                Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, velocity.X, velocity.Y, Scale: scale);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<DisgustingMeat>());
            LeadingConditionRule postEoC = npcLoot.DefineConditionalDropSet(DropHelper.PostEoC());
            postEoC.Add(ModContent.ItemType<BloodOrb>(), 1, 10, 12);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            Texture2D baseTexture = TextureAssets.Npc[Type].Value;
            SpriteEffects effects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 scale = NPC.scale * SquashVector;

            float yOffset = scale.Y * baseTexture.Height * 0.05f;
            Vector2 drawPosition = NPC.Center + new Vector2(0f, NPC.gfxOffY - yOffset) - screenPos;

            // Background effect when Horrible Hog is idling and its nearby loop is playing.
            if (DevilsTongueVolumeMultiplier > 0.05f)
            {
                using (spriteBatch.Scope())
                {
                    float radiusBasedOpacity = Utils.Remap(Main.LocalPlayer.Distance(NPC.Center), 250f, 750f, 1f, 0.15f, true);

                    Effect auraShader = CalamityShaders.HorribleHogAuraShader.Value;
                    auraShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
                    auraShader.Parameters["colorPaletteLimit"].SetValue(16f);
                    auraShader.Parameters["spiralArms"].SetValue(5f);
                    auraShader.Parameters["spiralAdditionalAngle"].SetValue(6f);
                    auraShader.Parameters["minPixelFadeDistance"].SetValue(0.125f);
                    auraShader.Parameters["maxPixelFadeDistance"].SetValue(0.485f);
                    auraShader.Parameters["pixelationFactor"].SetValue(Main.ScreenSize.ToVector2() * 0.25f);
                    auraShader.Parameters["spiralTimeOffset"].SetValue(new Vector2(-0.08f, -0.05f));
                    auraShader.Parameters["vortexDarkColor"].SetValue(new Color(40, 40, 40).ToVector3());
                    auraShader.Parameters["vortexBrightColor"].SetValue(Color.Crimson.ToVector3());

                    Main.graphics.GraphicsDevice.Textures[1] = VortexTextureSecondary.Value;
                    Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

                    Main.graphics.GraphicsDevice.Textures[2] = VortexDistortionTexture.Value;
                    Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, auraShader, Main.GameViewMatrix.TransformationMatrix);

                    spriteBatch.Draw(VortexTexture.Value, drawPosition, null, Color.White * radiusBasedOpacity * DevilsTongueVolumeMultiplier, 0f, VortexTexture.Size() * 0.5f, 1f, 0, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                    float bloomCircleOpacity = MathHelper.Lerp(0.6f, 0.9f, MathF.Sin((float)Main.timeForVisualEffects / 75f + NPC.whoAmI * 0.5f + 0.5f)) * radiusBasedOpacity;
                    spriteBatch.Draw(BloomCircle.Value, drawPosition, null, Color.Crimson with { A = 0 } * bloomCircleOpacity * DevilsTongueVolumeMultiplier, 0f, BloomCircle.Size() * 0.5f, 1.2f, 0, 0f);

                    spriteBatch.End();
                }
            }

            // Horrible Hog and its afterimage trail.
            using (spriteBatch.Scope())
            {
                Effect rotateSpriteShader = CalamityShaders.RotateSprite.Value;
                rotateSpriteShader.Parameters["rotation"].SetValue(SpriteRotation);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, rotateSpriteShader, Main.GameViewMatrix.TransformationMatrix);

                if (CalamityClientConfig.Instance.Afterimages && AfterimageTrailOpacity > 0.05f)
                {
                    for (int i = 0; i < NPC.oldPos.Length; i++)
                    {
                        Color afterimageColor = Color.Red * AfterimageTrailOpacity * 0.76f;
                        afterimageColor *= (float)(NPC.oldPos.Length - i) / (float)NPC.oldPos.Length;
                        Vector2 afterimageDrawPosition = NPC.oldPos[i] - Vector2.UnitY * yOffset + NPC.Size * 0.5f - screenPos;
                        spriteBatch.Draw(baseTexture, afterimageDrawPosition, NPC.frame, NPC.GetAlpha(afterimageColor), NPC.rotation, NPC.frame.Size() * 0.5f, scale, effects, 0f);
                    }
                }
                spriteBatch.Draw(baseTexture, drawPosition + Main.rand.NextVector2Circular(HorizontalShakeStrength, 0f), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, scale, effects, 0f);

                spriteBatch.End();
            }

            // Debugging text for tracking attacks.
            //string attack = $"{(BehaviorState)AIState}";
            //Vector2 stringDrawPosition = drawPosition - Vector2.UnitY * 64f;
            //ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, attack, stringDrawPosition, Color.LawnGreen, 0f, Vector2.One, Vector2.One);

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
