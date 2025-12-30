using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.DataStructures;
using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public class BrainOfCthulhuAI : VanillaAIOverride
    {
        private static SoundStyle StunnedHit = new("CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/BoC_Rev_Stun_Hit", 3);
        private static SoundStyle ShieldDown = new("CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/BoC_Rev_Shield_Down");
        private static SoundStyle ShieldUp = new("CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/BoC_Rev_Shield_Up");
        private static SoundStyle IntroRoar = new("CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/BoC_Rev_Roar");
        private static SoundStyle Roar = new("CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/BoC_Rev_Short_Roar");

        public static bool SummonedViaItem = false;
        internal List<Particle> BoCAfterImages = [];
        internal float ShieldOpacity = 1f;
        internal float ShieldScale = 1f;
        private Vector2 BoCDrawOffset = Vector2.Zero;
        private Rectangle BoCFrame = new(0, 0, 198, 180);

        #region Balancing Values

        #region Projectile Damage Values
        internal static int BloodShotDamage => 12; // 48
        internal static int BloodScytheDamage => 12; // 48
        internal static int IchorShotDamage => 12; // 48
        internal static int CrimsonEyeDamage => 12; // 48
        #endregion

        internal static float Phase1DefenseMultiplier => 1.5f; //Multiplies BoC's default defense value by this amount when in Phase 1.

        #region Health Gates
        internal static float DesperateOnslaughtCreeperHealthGate => 0.1f; //When the cumulative health % of all creepers falls below this value, BoC will begin its pre-stun attack upon entering its idle phase.
        internal static float Phase2HealthGate => 0.5f; //When BoC's health % falls below this value, it will begin entering Phase 2
        #endregion

        internal static float DespawnRange => 6000f;

        #region Phase 1 Attack Values

        #region Idle Period
        internal static int IdlePeriodDuration => 180;
        internal static int CreeperChargeDelayMin => 70;
        internal static int CreeperChargeDelayMax => 100;
        internal static int CreeperChargePositioningTime => 60;
        internal static int CreeperChargeWindUpTime => 22;
        #endregion

        internal static int StunDuration => 480;

        #region Creeper Swipes
        internal static int SwipesStartupDuration => 120;
        internal int SwipeDuration => 60 + SwipeDelay;
        internal int SwipeDelay => AttackFlag ? 30 : CalamityWorld.death ? 30 : 45;
        internal static int SwipeAmount => 4;
        internal int SwipeIchorDelay => 30 + SwipeDelay;
        #endregion

        #region Creeper Crush        
        internal static int LightSwipeDelay => 60;
        internal static int LightSwipeAmount => CalamityWorld.death ? 8 : 6;
        internal static int LightSwipeTravelTime => 30;
        internal static int LightSwipeAttackDelay => 10;
        internal static int LightSwipeDuration => CalamityWorld.death ? 45 : 60;
        internal static int StrongSwipeDelay => 90;
        internal static int StrongSwipeAmount => CalamityWorld.death ? 7 : 5;
        internal static int StrongSwipeTravelTime => 45;
        internal static int StrongSwipeAttackDelay => 15;
        internal static int StrongSwipeDuration => CalamityWorld.death ? 60 : 80;
        #endregion

        #region Creeper Orbit
        internal static int OrbitSetupDuration => 60;
        internal static int OrbitDuration => 720;
        internal static int OrbitAttackInterval => 120;
        internal static int OrbitAttackParticipantCount => CalamityWorld.death ? 4 : 3;
        internal static float OrbitStandardRadius => 320f;
        internal static float OrbitTelegraphRadius => 480f;
        internal static float BaseRotationSpeed => 0.0175f;
        #endregion

        #region Creeper Spiral
        internal static int SpiralDuration => 720;
        internal static int SpiralSetupTime => 90;
        internal static int TendrilCount => 3;
        internal static float TendrilLength => 512;
        internal static float TendrilStartDistance => 64;
        internal static float MaxCreeperSway => 64;
        internal static int StartingTimePerRevolutionMax => 270;
        internal static int StartingTimePerRevolutionMin => 180;
        internal static int EndingTimePerRevolutionMax => 210;
        internal static int EndingTimePerRevolutionMin => 120;
        internal static int SpeedUpDelayTime => 120;
        internal static int SpeedUpExtensionTime => 120;
        internal static float TurnAroundRatio => 0.6f; //In the second creeper phase, the creeper spiral will turn around at this completion percentage of the attack;
        internal static float TurnAroundDurationRatio => 0.1f; //The amount of time that it'll take for the creeper spiral to turn around

        #endregion

        #endregion

        #region Phase 2 Attack Values

        internal static float DefaultTeleportDistance => 360f;

        #region Idle Period
        internal static int ChaseTime => 160;
        internal static int MaxChases => 3;
        internal static int IdleTeleportDuration => CalamityWorld.death ? 36 : 44;
        #endregion

        #region Bloodletting
        internal static int BloodlettingDuration => 765;
        internal static Vector2 HoverDistance => new (420f, 270f);
        internal static float HoverEndHeight => 300f;
        internal static int IchorRate => CalamityWorld.death ? 8 : 10;
        internal static float IchorSpread => 1.5f;
        internal static float IchorVelocity => 3f;
        internal static int BloodshotRate => 90;
        internal static float BloodshotVelocity => 10f;
        internal static int DashPrepTime => 90;
        internal static int DashReelbackTime => 20;
        internal static int DashDuration => 30;
        internal static float DashVelocity => 32f;
        internal static int DashScytheRate => CalamityWorld.death ? 5 : 6;
        #endregion

        #region Sanguine Scythes
        internal static int SanguineTeleportCount => 5;
        internal static int SanguineScytheCount => CalamityWorld.death ? 12 : 10;
        internal static int SanguineTeleportDuration => 30;
        internal static float SanguineTeleportDistance => 440f;
        internal static Vector2 SanguineFinalTeleportOffset => new(720, 300);
        internal static int SanguineAttackEndDelay => 30;
        internal static int SanguineAttackEndDuration => 100;
        internal static int SanguineAttackEndIchorRate => 10;

        #endregion

        #region Crimson Eyes
        internal static int CrimsonEyeAttackIdleDuration => 210;
        internal static int CrimsonEyeAttackSetUpDuration => 30;
        internal static int CrimsonEyeAttackBuildUpDuration => 120;

        internal static int CrimsonEyeRate => 60;
        internal static int CrimsonEyeCap => 40;

        internal static int CrimsonEyeAttackDuration => 960;
        internal static int CrimsonEyeAttackEndDuration => 210;
        internal static float TurnAccelerationMultiplier => 0.01f; //Base multiplier for the boss' course correction rotation
        internal static float TurnAccelerationDistanceBuffer => 160f; //Determines the minimum distance it must be at before it can begin redirecting.
        internal static float TurnAccelerationDistanceDivisor => 72f; //Determines how much the distance affects its turn amount. Larger number means it must get further from the player in order to correct its course
        #endregion

        #region Illusion Dash
        internal static float IllusionDashTeleportDistance => 300f;
        internal static int IllusionDashTeleportDuration => 30;
        internal static float IllusionDashCloseInDistance => 300f;
        internal static float IllusionDashStartingSpinSpeed => 0.125f;
        internal static int IllusionDashSpinDuration => 100;
        internal static int IllusionDashFakeoutTeleportDuration => 16;
        internal static float IllusionDashVelocity => 30f;

        #endregion

        #region Illusion Trick
        internal static int IllusionTrickAngleGroups => CalamityWorld.death ? 8 : 6;
        internal static int IllusionTrickGroupSize => CalamityWorld.death ? 5 : 4;
        internal static int IllusionTrickStunDuration => 120;
        internal static int IllusionTrickTimeLimit => 960;

        #endregion

        #endregion

        #endregion

        internal enum BrainAIState
        {
            //Spawn Animation
            UndergroundSpawnAnimation = -2,
            SurfaceSpawnAnimation,
            //Phase 1
            Idle,
            CreeperSwipes,
            CreeperCrush,
            CreeperOrbit,
            CreeperSpiral,
            DesperateOnslaught,
            Stunned,
            //Phase Transition
            Phase2TransitionClosed,
            Phase2TransitionOpen,
            //Phase 2
            Phase2Idle,
            CrimsonEyes,
            SanguineScythes,
            Bloodletting,
            IllusionDash,
            IllusionTrick,
            //Defeat
            DeathAnimation
        }

        internal BrainAIState AIState { get => (BrainAIState)NPC.ai[0]; set => NPC.ai[0] = (float)value; }
        internal BrainAIState PreviousAttack = BrainAIState.Idle;
        internal ref float Time => ref NPC.ai[1];
        internal ref float DespawnTime => ref NPC.ai[2];
        internal ref float AnimationTime => ref NPC.ai[3];
        internal float TeleportTime = 0;
        internal float TeleportDuration = 0;
        internal float SpawnTime = 0;
        internal int SpawnDelay = 0;
        internal bool OnSecondCreeperPhase = false;

        internal float CachedRatio = 0f;
        private bool isNegative = false;
        internal int AttackSign { get => isNegative ? -1 : 1; set => isNegative = value == -1 ? true : false; }
        internal float AttackRotation = 0;
        internal float AttackTime = 0;
        internal int AttackCounter = 0;
        internal bool AttackFlag = false;
        internal Vector2 AttackPosition = Vector2.Zero;
        internal List<BrainAIState> availableAttacks = [];
        internal List<float> AttackList = [];

        public override void SetDefaults(Mod mod)
        {
            DisableMultiplayerSmoothing = true;

            BoCDrawOffset = Vector2.Zero;
            ShieldOpacity = 1f;
            ShieldScale = 1f;
            RevBoCSystem.ScreenBlurStrength = 0f;

            if (!Main.dedServ)
            {
                int brainOfCthuluCreepersCount = GetBrainOfCthuluCreepersCountRevDeath();
                RevBoCSystem.VerletTendrils = new List<VerletSimulatedSegment>[brainOfCthuluCreepersCount];

                for (int i = 0; i < brainOfCthuluCreepersCount; i++)
                {
                    List<VerletSimulatedSegment> tendril = [];
                    for (int j = 0; j < 28; j++)
                        tendril.Add(new(NPC.Center));

                    RevBoCSystem.VerletTendrils[i] = tendril;
                }
            }
        }

        public override void OnSpawn(Mod mod)
        {
            DisableMultiplayerSmoothing = true;

            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                // Ignore tank players, target low HP players, Brain is smart
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(NPC, options);
            }
            Player target = Main.player[NPC.target];
            bool onSurface = target.Center.Y / 16 < Main.worldSurface;

            NPC.Center = target.Center + Vector2.UnitY * (onSurface ? 900 : -900);
            NPC.dontTakeDamage = true;

            AIState = onSurface ? BrainAIState.SurfaceSpawnAnimation : BrainAIState.UndergroundSpawnAnimation;
            PreviousAttack = BrainAIState.Idle;
            SpawnDelay = SummonedViaItem ? 2 : 60;
            if (SummonedViaItem)
                SpawnTime = -1;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int brainOfCthuluCreepersCount = GetBrainOfCthuluCreepersCountRevDeath();
                for (int i = 0; i < brainOfCthuluCreepersCount; i++)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.Creeper, NPC.whoAmI, i, ai2: -1);
            }
        }

        public override bool AI(Mod mod)
        {
            // whoAmI variable
            NPC.crimsonBoss = NPC.whoAmI;
            bool phase2 = AIState >= BrainAIState.Phase2TransitionClosed;

            //Takes more damage in Phase 1 to account for invulnerability phases
            if (phase2)
            {
                NPC.knockBackResist = 0f;
                NPC.defense = NPC.defDefense;

                NPC.chaseable = (AIState != BrainAIState.IllusionDash && AIState != BrainAIState.IllusionTrick);
            }
            else
                NPC.defense = (int)(NPC.defDefense * Phase1DefenseMultiplier);

            if (AIState < 0)
                NPC.damage = 0;
            else
                NPC.damage = NPC.defDamage;

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            #region Targeting
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                // Ignore tank players, target low HP players, Brain is smart
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(NPC, options);
            }

            Player target = Main.player[NPC.target];
            #endregion

            #region Despawn
            // Despawn check
            bool despawn = (target.dead || !target.ZoneCrimson) && !BossRushEvent.BossRushActive;

            // Despawn
            if (despawn)
            {
                if (DespawnTime < 90)
                    DespawnTime += 1f;

                if (DespawnTime == 90)
                    NPC.velocity.Y += 0.1f;
            }
            else if (DespawnTime > 0f)
                DespawnTime -= 1f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (target.Distance(NPC.Center) > DespawnRange)
                {
                    NPC.active = false;
                    NPC.life = 0;

                    if (Main.dedServ)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            if(DespawnTime > 60)
                return false;
            #endregion

            #region Hit Sounds
            if (AIState == BrainAIState.Stunned)
                NPC.HitSound = StunnedHit;
            else
                NPC.HitSound = SoundID.NPCHit9;
            #endregion

            #region Health Ratios
            float CreeperHPRatio = 0f;
            foreach (NPC creeper in Main.npc.Where(n => n.active && n.type == NPCID.Creeper))
                CreeperHPRatio += creeper.life / (float)creeper.lifeMax;
            if (CreeperHPRatio != 0f)
                CreeperHPRatio /= GetBrainOfCthuluCreepersCountRevDeath();

            float CreeperAmountRatio = NPC.CountNPCS(NPCID.Creeper) / (float)GetBrainOfCthuluCreepersCountRevDeath();
            #endregion

            #region Forced State Changes
            if (!phase2 && CreeperHPRatio == 0f && AIState != BrainAIState.Stunned && AIState >= 0)
            {
                AIState = BrainAIState.Stunned;
                Time = 0;
                ResetAttackValues();
                foreach(Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type != ModContent.ProjectileType<TelekineticEnemyGrab>())
                        continue;

                    p.ai[1] = 0;
                }
            }

            if (AIState == BrainAIState.Stunned && (NPC.life / (float)NPC.lifeMax) < Phase2HealthGate)
            {
                AIState = BrainAIState.Phase2TransitionClosed;
                Time = 0;
                TeleportTime = 0;
            }
            #endregion

            switch (AIState)
            {
                #region Spawn Animations
                case BrainAIState.UndergroundSpawnAnimation:
                case BrainAIState.SurfaceSpawnAnimation:
                    if (SpawnTime != 0) //BoC should begin appearing
                    {
                        float d = Main.LocalPlayer.DistanceSQ(NPC.Center);
                        float distanceScaleFactor = 1;
                        if (d > 592900) //770^2
                            distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(d) - 770) / 32f));

                        float spawnCounter = Time - Math.Abs(SpawnTime);

                        if (spawnCounter < 180)
                        {
                            float shakeIntensity = CalamityUtils.CircOutEasing(spawnCounter / 180f, 1) * 3f * distanceScaleFactor;
                            Main.LocalPlayer.SetScreenshake(shakeIntensity);
                            for (int i = 0; i < shakeIntensity; i++)
                            {
                                Point start = target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                                for (int j = 0; j < 96; j++)
                                {
                                    Point current = start - new Point(0, j);
                                    if (!Main.tile[current].IsTileSolid() && Main.tile[current - new Point(0, 1)].TileType == TileID.Crimstone)
                                        Dust.NewDust(current.ToWorldCoordinates(0, -16), 16, 16, DustID.Crimstone, 0, 3);
                                }
                            }
                        }
                        if (spawnCounter == 180)
                            NPC.velocity = Vector2.UnitY * (AIState == BrainAIState.UndergroundSpawnAnimation ? 32 : -50);
                        else if (spawnCounter > 180)
                        {
                            NPC.velocity *= 0.955f;

                            if (spawnCounter == 240)
                            {
                                SoundEngine.PlaySound(IntroRoar, NPC.Center);

                                if (Main.netMode == NetmodeID.SinglePlayer)
                                    Main.NewText(Language.GetTextValue("Announcement.HasAwoken", NPC.TypeName), 175, 75);
                                else if (Main.dedServ)
                                    ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", NPC.TypeName), new Color(175, 75, 255));
                            }

                            if (spawnCounter > 240 && spawnCounter < 390)
                            {
                                RevBoCSystem.ScreenBlurStrength = 0.5f;// 0.5f;
                                if (spawnCounter < 250)
                                    RevBoCSystem.ScreenBlurStrength = MathHelper.Lerp(0, 0.5f, (spawnCounter - 240) / 10f);

                                NPC.frameCounter += 1f;

                                BoCDrawOffset = Main.rand.NextVector2Circular(4, 4);

                                for (int i = 0; i < 3; i++)
                                {
                                    Point start = target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                                    for (int j = 0; j < 96; j++)
                                    {
                                        Point current = start - new Point(0, j);
                                        if (!Main.tile[current].IsTileSolid() && Main.tile[current - new Point(0, 1)].TileType == TileID.Crimstone)
                                            Dust.NewDust(current.ToWorldCoordinates(0, -16), 16, 16, DustID.Crimstone, 0, 3);
                                    }
                                }

                                Main.LocalPlayer.SetScreenshake(6 * RevBoCSystem.ScreenBlurStrength * distanceScaleFactor);

                                if (spawnCounter % 15 == 0)
                                {
                                    BossRoar pulse = new(NPC.Center, Color.Black, Main.rand.NextFloatDirection(), 0.1f, 3f, 30);
                                    GeneralParticleHandler.SpawnParticle(pulse);
                                }
                            }
                            else if (spawnCounter >= 390 && spawnCounter <= 420)
                            {
                                RevBoCSystem.ScreenBlurStrength = MathHelper.Lerp(0.5f, 0f, (spawnCounter - 390) / 30f);
                                BoCDrawOffset *= 0.75f;
                            }
                            else if (spawnCounter > 420)
                            {
                                RevBoCSystem.ScreenBlurStrength = 0f;
                                BoCDrawOffset = Vector2.Zero;
                                AIState = BrainAIState.Idle;
                                ResetAttackValues();
                                Time = -1;
                                SpawnTime = -1;
                                Main.musicFade[Main.curMusic] = 1f;
                                break;
                            }
                        }
                    }
                    if (AttackCounter < GetBrainOfCthuluCreepersCountRevDeath())
                    {
                        if (SpawnTime == 0)
                            NPC.Center = target.Center + Vector2.UnitY * (AIState == BrainAIState.UndergroundSpawnAnimation ? -900 : 900);

                        if (SpawnDelay <= 0)
                        {
                            foreach (NPC creeper in Main.ActiveNPCs)
                            {
                                if (creeper.type != NPCID.Creeper)
                                    continue;
                                creeper.netUpdate = true;
                            }

                            bool targetLeft = AttackCounter % 2 == 0;
                            List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.AIOverride<CreeperAI>().Time == -1 && n.AIOverride<CreeperAI>().CreeperID % 2 == (targetLeft ? 0 : 1)).ToList();

                            int rand;

                            if (creepers.Count > 0)
                                rand = Main.rand.Next(creepers.Count);
                            else
                            {
                                creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.AIOverride<CreeperAI>().Time == -1).ToList();
                                rand = Main.rand.Next(creepers.Count);
                            }

                            if (creepers.Count > 0)
                            {
                                NPC creeper = creepers[rand];
                                creeper.AIOverride<CreeperAI>().Time = 0;

                                AttackCounter++;

                                if (SummonedViaItem)
                                    SpawnDelay = 2;
                                else
                                    switch (AttackCounter)
                                    {
                                        case 1:
                                            SpawnDelay = 90;
                                            break;
                                        case 2:
                                        case 3:
                                        case 4:
                                            SpawnDelay = 24;
                                            break;
                                        case 5:
                                            SpawnDelay = 60;
                                            break;
                                        case 6:
                                        case 7:
                                        case 8:
                                            SpawnDelay = 24;
                                            break;
                                        case 9:
                                            SpawnDelay = 60;
                                            break;
                                        default:
                                            SpawnDelay = 4;
                                            break;
                                    }
                            }
                            else
                                SpawnTime = Time;
                        }
                        else
                            SpawnDelay--;
                    }
                    else if (SpawnTime == 0)
                        SpawnTime = Time;
                    break;
                #endregion

                #region Phase 1
                case BrainAIState.Idle:
                    float speed;

                    #region Attack Selection
                    if (CreeperHPRatio <= DesperateOnslaughtCreeperHealthGate)
                    {
                        Time = -1;
                        AIState = BrainAIState.DesperateOnslaught;
                        AttackSign = Main.rand.NextBool() ? -1 : 1;
                        NPC.netUpdate = true;
                    }
                    else if (Time > IdlePeriodDuration)
                    {
                        Time = -1;

                        ResetAttackValues();
                        
                        if (availableAttacks.Count == 0)
                        {
                            availableAttacks = [BrainAIState.CreeperSwipes, BrainAIState.CreeperCrush, BrainAIState.CreeperOrbit, BrainAIState.CreeperSpiral];
                            if (PreviousAttack != BrainAIState.Idle)
                                availableAttacks.Remove(PreviousAttack);
                        }
                        
                        int pick = Main.rand.Next(availableAttacks.Count);
                        AIState = availableAttacks[pick];
                        availableAttacks.RemoveAt(pick);
                        PreviousAttack = AIState;

                        foreach (NPC creep in Main.npc.Where(n => n.active && n.type == NPCID.Creeper))
                            creep.AIOverride<CreeperAI>().Time = -1;

                        NPC.netUpdate = true;
                        foreach (NPC creeper in Main.ActiveNPCs)
                        {
                            if (creeper.type != NPCID.Creeper)
                                continue;
                            creeper.netUpdate = true;
                        }
                    }
                    #endregion

                    /*
                    #region Creeper Attacks
                    if (AttackCounter > 0)
                        AttackCounter--;
                    else if (Time < IdlePeriodDuration - 120)
                    {
                        if (Time >= 30)
                        {
                            bool targetLeft = target.Center.X < NPC.Center.X;
                            List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.AIOverride<CreeperAI>().Time == -1 && n.AIOverride<CreeperAI>().CreeperID % 2 == (targetLeft ? 0 : 1)).ToList();
                            if (creepers.Count == 0)
                                creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.AIOverride<CreeperAI>().Time == -1).ToList();

                            if (creepers.Count > 0)
                            {
                                int rand = Main.rand.Next(creepers.Count);

                                NPC creeper = creepers[rand];
                                creeper.AIOverride<CreeperAI>().Time = 0;
                            }
                        }
                        AttackCounter = (int)MathHelper.Lerp(CreeperChargeDelayMax, CreeperChargeDelayMin, 1 - CreeperAmountRatio);
                    }
                    #endregion
                    */

                    #region Movement
                    if (Time == 0)
                    {
                        AttackSign = Main.rand.NextBool() ? -1 : 1;
                        NPC.netUpdate = true;
                        foreach (NPC creeper in Main.ActiveNPCs)
                        {
                            if (creeper.type != NPCID.Creeper)
                                continue;
                            creeper.netUpdate = true;
                        }
                    }

                    float rotateDir = AttackSign;
                    Vector2 fromTarget = NPC.DirectionFrom(target.Center);
                    Vector2 dir = fromTarget.RotatedBy(Math.Sin(Time / 60f) * rotateDir) * new Vector2(2, 1);
                    float rayDist = CalamityUtils.PreciseDistanceToTileCollisionHit(target.Center, dir.ToRotation(), 360, 1);
                    Vector2 offset = dir * (rayDist - NPC.width);
                    Vector2 goalPos = target.Center + offset;
                    float distSQ = NPC.DistanceSQ(goalPos);
                    if (distSQ > 129600)
                    {
                        NPC.velocity = NPC.DirectionTo(goalPos) * (4 + (NPC.Distance(goalPos) - 360) / 64f);
                    }
                    else if (distSQ <= 2048)
                    {
                        NPC.velocity *= 0.9f;
                    }
                    else if (NPC.velocity.LengthSquared() < 16f)
                    {
                        NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 0.15f;
                    }
                    else
                    {
                        NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 6f;
                    }
                    #endregion

                    break;
                case BrainAIState.DesperateOnslaught:
                    #region Movement
                    if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        NPC.velocity = NPC.DirectionTo(target.Center) * 4f;
                        Time = -1;
                    }
                    else if (NPC.DistanceSQ(target.Center) > 230400)
                        NPC.velocity = NPC.DirectionTo(target.Center) * (NPC.Distance(target.Center) - 480) / 128f;
                    else
                        NPC.velocity *= 0.9f;
                    #endregion

                    float wrappedCounter = Time % 90;

                    if (Time <= 60)
                    {
                        if (Time == 0)
                            SoundEngine.PlaySound(Roar, NPC.Center);
                        if (Time < 30)
                        {
                            RevBoCSystem.ScreenBlurStrength = 0.5f;

                            NPC.frameCounter += 1f;

                            for (int i = 0; i < 3; i++)
                            {
                                Point start = target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                                for (int j = 0; j < 96; j++)
                                {
                                    Point current = start - new Point(0, j);
                                    if (!Main.tile[current].IsTileSolid() && Main.tile[current - new Point(0, 1)].TileType == TileID.Crimstone)
                                        Dust.NewDust(current.ToWorldCoordinates(0, -16), 16, 16, DustID.Crimstone, 0, 3);
                                }
                            }

                            float d = Main.LocalPlayer.DistanceSQ(NPC.Center);
                            float distanceScaleFactor = 1;
                            if (d > 592900) //770^2
                                distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(d) - 770) / 32f));

                            Main.LocalPlayer.SetScreenshake(4 * RevBoCSystem.ScreenBlurStrength * distanceScaleFactor);

                            if (Time % 15 == 0)
                            {
                                BossRoar pulse = new(NPC.Center, Color.Black, Main.rand.NextFloatDirection(), 0.1f, 3f, 30);
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                        }
                        else
                        {
                            RevBoCSystem.ScreenBlurStrength = MathHelper.Lerp(0.5f, 0, CalamityUtils.CircOutEasing((Time - 30) / 30f, 1));
                        }
                    }
                    else
                    {
                        RevBoCSystem.ScreenBlurStrength = 0f;
                        if (wrappedCounter == 65)
                        {
                            int checkCount = 8;
                            float wallDist = CalamityUtils.PreciseDistanceToTileCollisionHit(NPC.Center, AttackSign == -1 ? MathHelper.Pi : 0, 480 + NPC.width) - NPC.width;
                            Vector2[] starts = new Vector2[checkCount];
                            for (int i = 0; i < checkCount; i++)
                            {
                                float completion = (i + 1) / (float)(checkCount + 1);
                                starts[i] = NPC.Center + (Vector2.UnitX * ((wallDist * completion) + NPC.width) * AttackSign);
                            }

                            Vector2[] ends = new Vector2[checkCount];
                            List<Vector2> goodEnds = [];
                            List<Vector2> farEnds = [];
                            List<Vector2> closeEnds = [];

                            for (int i = 0; i < checkCount; i++)
                            {
                                float maxDist = 960;
                                float floorDist = CalamityUtils.PreciseDistanceToTileCollisionHit(NPC.Center, Vector2.UnitY.ToRotation(), maxDist);
                                ends[i] = starts[i] + (Vector2.UnitY * (floorDist + 48));
                                if (floorDist >= 600)
                                    farEnds.Add(ends[i]);
                                else if (floorDist > 240)
                                    goodEnds.Add(ends[i]);
                                else
                                    closeEnds.Add(ends[i]);
                            }

                            Vector2 chosenEnd;

                            if (goodEnds.Count > 0)
                                chosenEnd = goodEnds[Main.rand.Next(goodEnds.Count)];
                            else if (closeEnds.Count > 0)
                                chosenEnd = closeEnds[Main.rand.Next(closeEnds.Count)];
                            else
                                chosenEnd = farEnds[Main.rand.Next(farEnds.Count)];

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), chosenEnd, Vector2.Zero, ModContent.ProjectileType<TelekineticEnemyGrab>(), 10, 0.5f);

                            AttackSign *= -1;
                        }
                    }
                    break;
                case BrainAIState.Stunned:
                    #region Movement
                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 6f);
                    fromTarget = (NPC.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    if (Time == 0)
                    {
                        NPC.velocity = fromTarget * 4f;
                        SoundEngine.PlaySound(ShieldDown, NPC.Center);
                    }

                    if (NPC.velocity != Vector2.Zero)
                    {
                        if (Time >= StunDuration)
                            NPC.velocity *= 0.8f;
                        else
                            NPC.velocity *= 0.93f;
                    }

                    if (Time < StunDuration)
                        NPC.position.Y += (float)Math.Sin(Time / 8f) * 2 * (1 - MathHelper.Clamp((Time - (StunDuration - 30)) / 30f, 0f, 1f));

                    if (Time <= StunDuration - 30)
                    {
                        if (AttackTime > 0)
                        {
                            float kbCounter = 30 - AttackTime;
                            if (kbCounter < 10)
                            {
                                float lerp = CalamityUtils.SineOutEasing(kbCounter / 10f, 1);
                                NPC.rotation = AttackRotation.AngleLerp(-AttackRotation, lerp);
                            }
                            else
                            {
                                float lerp = CalamityUtils.SineInOutEasing((kbCounter - 10) / 20f, 1);
                                NPC.rotation = (-AttackRotation).AngleLerp(0, lerp);
                            }

                        }
                        else if (Math.Abs(NPC.oldVelocity.X) < Math.Abs(NPC.velocity.X) || Time <= 0)
                        {
                            AttackRotation = NPC.rotation;
                            TeleportTime = 0;
                        }
                        else
                        {
                            TeleportTime++;
                            NPC.rotation = MathHelper.Lerp(AttackRotation, MathHelper.Pi / 24f * NPC.oldVelocity.X, CalamityUtils.CircOutEasing(MathHelper.Clamp(TeleportTime / 30f, 0f, 1f), 1));
                        }
                    }
                    #endregion

                    #region Tile Collision
                    if (Time < StunDuration)
                    {
                        if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                        {
                            NPC.velocity = NPC.DirectionTo(target.Center) * 4f;
                        }
                        else if (Collision.SolidCollision(NPC.position + NPC.velocity, NPC.width, NPC.height))
                        {
                            if (NPC.velocity.X != NPC.oldVelocity.X)
                                NPC.velocity.X = -NPC.oldVelocity.X;
                            if (NPC.velocity.Y != NPC.oldVelocity.Y)
                                NPC.velocity.Y = -NPC.oldVelocity.Y;
                            //NPC.velocity *= 2f;
                            NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                            AttackTime = 30;
                            AttackRotation = NPC.rotation;
                        }

                        if (AttackTime > 0)
                        {
                            NPC.knockBackResist = 0f;
                            AttackTime--;
                            if (AttackTime == 0)
                            {
                                NPC.velocity = Vector2.Zero;
                                AttackRotation = 0;
                            }
                        }
                        else
                            NPC.knockBackResist = 1f;
                    }
                    #endregion

                    RevBoCSystem.ScreenBlurStrength = 0f;

                    NPC.dontTakeDamage = false;

                    if (Time <= 15)
                    {
                        float lerp = Time / 15f;
                        ShieldOpacity = 1 - CalamityUtils.CircOutEasing(lerp, 1);
                        ShieldScale = MathHelper.Lerp(1f, 1.5f, lerp);
                    }

                    #region Recovery
                    if (OnSecondCreeperPhase && Time == StunDuration - 30)
                    {
                        AIState = BrainAIState.Phase2TransitionClosed;
                        Time = -1;
                        TeleportTime = 0;
                        break;
                    }

                    if (Time > StunDuration - 30)
                    {
                        NPC.rotation = NPC.rotation.AngleLerp(0f, CalamityUtils.SineInOutEasing((Time - (StunDuration - 30)) / 30f, 1));
                        if (Time == StunDuration - 15)
                            SoundEngine.PlaySound(ShieldUp, NPC.Center);
                    }

                    if (Time >= StunDuration)
                    {
                        if (NPC.velocity.X < 0.001f && NPC.velocity.X < 0.001f)
                            NPC.velocity = Vector2.Zero;

                        int creeperRate = 5;
                        wrappedCounter = (Time - StunDuration) % creeperRate;
                        int spawnTime = GetBrainOfCthuluCreepersCountRevDeath() / 2 * creeperRate;

                        if (Time == StunDuration)
                            AttackCounter = GetBrainOfCthuluCreepersCountRevDeath() - 1;

                        NPC.knockBackResist = 0f;
                        NPC.dontTakeDamage = true;
                        NPC.rotation = 0f;

                        float shieldAppearTime = 15f;

                        float lerp = MathHelper.Clamp((Time - StunDuration) / shieldAppearTime, 0f, 1f);
                        if (lerp >= 1)
                        {
                            ShieldOpacity = 1f;
                            ShieldScale = 1f;
                        }
                        else
                        {
                            ShieldOpacity = CalamityUtils.CircOutEasing(lerp, 1);
                            ShieldScale = MathHelper.Lerp(1.5f, 1f, CalamityUtils.SineOutEasing(lerp, 1));
                        }

                        if (AttackCounter == -1 && Time > StunDuration + spawnTime + 30)
                        {
                            OnSecondCreeperPhase = true;
                            AIState = BrainAIState.Idle;
                            Time = -1;
                        }
                        else if (AttackCounter > -1 && wrappedCounter == 0)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                dir = Vector2.UnitY.RotatedBy((AttackCounter % 2 == 0 ? 1 : -1) * (MathHelper.Pi / 16f + ((MathHelper.Pi - MathHelper.Pi / 8f) * (AttackCounter / 2f / (GetBrainOfCthuluCreepersCountRevDeath() / 2f)))));
                                Vector2 spawnPos = NPC.Center + (dir * 72f);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC creeper = NPC.NewNPCDirect(NPC.GetSource_FromAI(), spawnPos, NPCID.Creeper, NPC.whoAmI, AttackCounter, ai2: -1, ai3: 1);
                                    creeper.velocity = dir * 24f;
                                }

                                for (int j = 0; j < 3; j++)
                                {
                                    BloodParticle p = new(spawnPos, dir.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(8f, 12f), 32, 1f, Color.Red);
                                    GeneralParticleHandler.SpawnParticle(p);
                                }
                                BloodParticle2 p2 = new(spawnPos, dir * 10f, 16, 0.5f, Color.Red);
                                GeneralParticleHandler.SpawnParticle(p2);

                                if (!Main.dedServ)
                                {
                                    RevBoCSystem.VerletTendrils[(int)AttackCounter] = [];
                                    for (int j = 0; j < 28; j++)
                                        RevBoCSystem.VerletTendrils[(int)AttackCounter].Add(new(NPC.Center));
                                }

                                AttackCounter--;
                                if (AttackCounter <= -1)
                                    break;
                            }

                            SoundEngine.PlaySound(SoundID.NPCHit9, NPC.Center);
                        }
                    }
                    #endregion
                    
                    break;
                case BrainAIState.CreeperSwipes:
                    //Hand Size check to determine if 1 hand variant should be used
                    if (Time == 0)
                    {
                        int leftAmt = 0;
                        int rightAmt = 0;
                        foreach (NPC creeper in Main.ActiveNPCs)
                        {
                            if (creeper.type != NPCID.Creeper)
                                continue;

                            if (creeper.AIOverride<CreeperAI>().CreeperID % 2 == 0)
                                leftAmt++;
                            else
                                rightAmt++;
                        }

                        if (leftAmt > 5 && rightAmt > 5)
                        {
                            AttackSign = Main.rand.NextBool() ? -1 : 1;
                            AttackFlag = false;
                        }
                        else
                            AttackFlag = true;
                        
                        NPC.netUpdate = true;
                        AttackList.Clear();
                    }

                    float wrappedCount = Time % (SwipeDuration + SwipeDelay);

                    if (Time >= SwipesStartupDuration)
                    {
                        NPC.damage = NPC.defDamage;
                        if (wrappedCount == 0)
                        {
                            bool useEven = Main.rand.NextBool();

                            bool anyActivated = false;

                            foreach (NPC Npc in Main.ActiveNPCs)
                            {
                                if (Npc.type != NPCID.Creeper)
                                    continue;

                                if (AttackFlag || Npc.AIOverride<CreeperAI>().CreeperID % 2 == 0! ^ useEven)
                                {
                                    AttackList.Add(Npc.whoAmI);
                                    //Npc.AIOverride<CreeperAI>().Time = 0;
                                    anyActivated = true;
                                }
                            }

                            if (!anyActivated)
                                foreach (NPC Npc in Main.ActiveNPCs)
                                {
                                    if (Npc.type != NPCID.Creeper)
                                        continue;

                                    AttackList.Add(Npc.whoAmI);
                                    //Npc.AIOverride<CreeperAI>().Time = 0;
                                }

                            NPC.netUpdate = true;
                        }
                        else if (wrappedCount == SwipeDuration)
                        {
                            AttackSign *= -1;
                            AttackList.Clear();
                            NPC.netUpdate = true;
                        }

                        if (wrappedCount > 1 && wrappedCount <= SwipeIchorDelay && Time % 3 == 0) // Telegraphs ichor rain w/ dripping particles
                        {
                            Vector2 spawnPosition = NPC.Center;
                            spawnPosition.Y += Main.rand.NextFloat(38, 50);
                            spawnPosition.X += Main.rand.NextFloat(-56, 56);

                            BloodParticle blood = new BloodParticle(spawnPosition, Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 2.5f), Main.rand.Next(30, 40), Main.rand.NextFloat(0.6f, 1f), Color.Gold);
                            GeneralParticleHandler.SpawnParticle(blood);              
                        }

                        if (wrappedCount < 60f) // Vibrates during telegraph
                        {
                            Vector2 vibrationVector = Main.rand.NextVector2CircularEdge(1, 1) * MathHelper.Lerp(0f, 12f, CalamityUtils.CircInEasing(wrappedCount / 80f, 1));

                            BoCDrawOffset = vibrationVector;
                        }
                        else if (wrappedCount > 60f && wrappedCount < 80f) // Droops down when starting to fire ichor shots
                        {
                            float progress = (wrappedCount - 60f) / 20f;
                            BoCDrawOffset = new Vector2(0, MathHelper.Lerp(10, 0, 1f - (float)Math.Pow(1f - progress, 3f)));
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            if (wrappedCount > SwipeIchorDelay && wrappedCount <= SwipeDuration && Time % 2 == 0)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(Main.rand.NextFloat(-72, 72), 56), Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 4f, MathHelper.Pi / 4f)) * 4f, ProjectileID.GoldenShowerHostile, IchorShotDamage, 0.5f);

                        if (Time >= SwipesStartupDuration + ((SwipeDuration + SwipeDelay) * SwipeAmount))
                        {
                            Time = 0;
                            AIState = BrainAIState.Idle;
                            NPC.netUpdate = true;
                            AttackList.Clear();
                        }
                    }
                    else
                        NPC.damage = 0;

                    #region Movement
                    goalPos = target.Center + (Vector2.UnitY * -270);
                    distSQ = NPC.DistanceSQ(goalPos);
                    if ((Time > SwipesStartupDuration && wrappedCount > 30 && wrappedCount <= SwipeDuration) || NPC.DistanceSQ(goalPos) <= 2048)
                        NPC.velocity *= 0.9f;
                    else if (distSQ > 14400)
                        NPC.velocity = NPC.DirectionTo(goalPos) * (8 + (NPC.Distance(goalPos) - 120) / 16f);
                    else
                        NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.UnitX * -target.direction) * (8f * distSQ / 14400f);
                    #endregion
                    break;
                case BrainAIState.CreeperCrush:
                    #region Movement
                    Vector2 goalDir = Vector2.Zero;
                    fromTarget = NPC.Center - target.Center;
                    if (Math.Abs(fromTarget.X) > Math.Abs(fromTarget.Y))
                        goalDir = Vector2.UnitX * Math.Sign(fromTarget.X);
                    else
                        goalDir = Vector2.UnitY * Math.Sign(fromTarget.Y);

                    goalPos = target.Center + (goalDir * 360) - (Vector2.UnitY * 32);
                    if (NPC.DistanceSQ(goalPos) <= 2048)
                        NPC.velocity *= 0.9f;
                    else if (NPC.velocity.LengthSquared() <= 56.25f) //7.5^2
                        NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.UnitX * -target.direction) * 0.5f;
                    else
                        NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.UnitX * -target.direction) * 8f;
                    #endregion

                    float delay = OnSecondCreeperPhase ? StrongSwipeDelay : LightSwipeDelay;
                    if(Time == 0)
                        AttackList.Clear();

                    if (Time > delay)
                    {
                        foreach (NPC creeper in Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.AIOverride<CreeperAI>().Time == -1 && !AttackList.Contains(n.whoAmI)))
                            creeper.position += NPC.velocity;

                        int crushCount;
                        int attackDelay;
                        if (!OnSecondCreeperPhase)
                        {
                            crushCount = LightSwipeAmount;
                            attackDelay = LightSwipeDuration;
                        }
                        else
                        {
                            crushCount = StrongSwipeAmount;
                            attackDelay = StrongSwipeDuration;
                        }
                        int attackDur = attackDelay * crushCount;

                        if (Time < delay + attackDur && Time % attackDelay == 0)
                        {
                            List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.AIOverride<CreeperAI>().Time == -1 && !AttackList.Contains(n.whoAmI)).ToList();
                            if (creepers.Count > 1)
                            {
                                float rotation;
                                if (!OnSecondCreeperPhase)
                                {
                                    rotation = target.velocity.ToRotation();
                                    if (target.velocity == Vector2.Zero)
                                        rotation = target.direction == 1 ? 0 : MathHelper.Pi;
                                    rotation += Main.rand.NextFloat(-MathHelper.PiOver4 / 2f, MathHelper.PiOver4 / 2f);
                                }
                                else
                                    rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);

                                int rand = Main.rand.Next(creepers.Count);
                                NPC first = creepers[rand];
                                CreeperAI creeper1 = first.AIOverride<CreeperAI>();
                                AttackList.Add(first.whoAmI);
                                //creeper1.Time = 0;
                                creeper1.AttackAngle = rotation;
                                creepers.RemoveAt(rand);

                                NPC second = creepers[Main.rand.Next(creepers.Count)];
                                CreeperAI creeper2 = second.AIOverride<CreeperAI>();
                                AttackList.Add(second.whoAmI);
                                //creeper2.Time = 0;
                                creeper2.AttackAngle = rotation + MathHelper.Pi;

                                creeper1.PartnerIndex = second.whoAmI;
                                creeper2.PartnerIndex = first.whoAmI;
                            }

                            NPC.netUpdate = true;
                            foreach (NPC creeper in Main.ActiveNPCs)
                            {
                                if (creeper.type != NPCID.Creeper)
                                    continue;
                                creeper.netUpdate = true;
                            }
                        }

                        if (Time > delay + attackDur + attackDelay)
                        {
                            Time = 0;
                            AIState = BrainAIState.Idle;
                            AttackList.Clear();
                            //foreach (NPC creep in Main.npc.Where(n => n.active && n.type == NPCID.Creeper))
                            //    creep.AIOverride<CreeperAI>().Time = -1;
                        }
                    }
                    break;
                case BrainAIState.CreeperOrbit:
                    #region Movement
                    fromTarget = (NPC.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    goalPos = target.Center + (fromTarget * 440) - (Vector2.UnitY * 32);
                    distSQ = NPC.DistanceSQ(goalPos);
                    if (distSQ > 14400)
                        NPC.velocity = NPC.DirectionTo(goalPos) * (4 + (NPC.Distance(goalPos) - 120) / 16f);
                    else
                        NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * (2 + (2f * (distSQ / 14400)));
                    #endregion

                    if (Time == 0)
                    {
                        AttackSign = Main.rand.NextBool() ? -1 : 1;
                        NPC.netUpdate = true;
                        foreach (NPC creeper in Main.ActiveNPCs)
                        {
                            if (creeper.type != NPCID.Creeper)
                                continue;
                            creeper.netUpdate = true;
                        }
                    }

                    if (Time >= OrbitDuration + 30)
                    {
                        Time = -1;
                        AIState = BrainAIState.Idle;
                        AttackList.Clear();
                        foreach (NPC creep in Main.ActiveNPCs)
                        {
                            if (creep.type == NPCID.Creeper)
                                creep.AIOverride<CreeperAI>().Time = -1;
                        }
                    }
                    else if (Time >= OrbitAttackInterval && Time < OrbitDuration && Time % OrbitAttackInterval == 0)
                    {
                        List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper).ToList();
                        if (creepers.Count > 0)
                        {
                            int rand = Main.rand.Next(creepers.Count);
                            for (int i = 0; i < OrbitAttackParticipantCount; i++)
                            {
                                if (rand >= creepers.Count)
                                    rand -= creepers.Count;
                                NPC creeper = creepers[rand];
                                AttackList.Add(creeper.whoAmI);
                                //creeper.AIOverride<CreeperAI>().Time = 0;
                                rand += (int)Math.Round(creepers.Count / (float)OrbitAttackParticipantCount);
                            }

                            NPC.netUpdate = true;
                            foreach (NPC creeper in Main.ActiveNPCs)
                            {
                                if (creeper.type != NPCID.Creeper)
                                    continue;
                                creeper.netUpdate = true;
                            }
                        }
                    }
                    break;
                case BrainAIState.CreeperSpiral:
                    if (Time == 0)
                    {
                        AttackSign = Main.rand.NextBool() ? -1 : 1;
                        AttackRotation = 0;
                        NPC.netUpdate = true;
                        foreach (NPC creeper in Main.ActiveNPCs)
                        {
                            if (creeper.type != NPCID.Creeper)
                                continue;
                            creeper.netUpdate = true;
                        }
                    }
                    float startTimePerRev = MathHelper.Lerp(StartingTimePerRevolutionMax, StartingTimePerRevolutionMin, 1 - CreeperAmountRatio);
                    float endTimePerRev = MathHelper.Lerp(EndingTimePerRevolutionMax, EndingTimePerRevolutionMin, 1 - CreeperAmountRatio);
                    float timePerRev = MathHelper.Lerp(startTimePerRev, endTimePerRev, MathHelper.Clamp((Time - SpeedUpDelayTime) / (SpiralDuration - SpeedUpDelayTime - SpeedUpExtensionTime), 0f, 1f));
                    if (Time > SpiralDuration - 30)
                        timePerRev *= MathHelper.Lerp(1f, 10f, CalamityUtils.CircOutEasing((Time - (SpiralDuration - 30)) / 30f, 1));
                    else if (Time < SpiralSetupTime)
                        timePerRev *= MathHelper.Lerp(1f, 10f, CalamityUtils.CircInEasing(Time / SpiralSetupTime, 1));

                    float rotToAdd = MathHelper.TwoPi / timePerRev * AttackSign;
                    if (OnSecondCreeperPhase)
                    {
                        float attackComplationRatio = Time / SpiralDuration;
                        float lerp = Utils.GetLerpValue(TurnAroundRatio - (TurnAroundDurationRatio / 2f), TurnAroundRatio + (TurnAroundDurationRatio / 2f), attackComplationRatio, true);
                        rotToAdd *= MathHelper.Lerp(1, -1, lerp);
                    }
                    AttackRotation += rotToAdd;

                    if (NPC.DistanceSQ(target.Center) > 57600)
                        NPC.velocity = NPC.DirectionTo(target.Center) * (NPC.Distance(target.Center) - 240) / 32f;
                    else
                        NPC.velocity *= 0.9f;

                    if (Time > SpiralDuration)
                    {
                        Time = -1;
                        AIState = BrainAIState.Idle;
                        foreach (NPC creep in Main.ActiveNPCs)
                        {
                            if (creep.type == NPCID.Creeper)
                                creep.AIOverride<CreeperAI>().Time = -1;
                        }
                    }
                    break;
                #endregion

                #region Phase Transition
                case BrainAIState.Phase2TransitionClosed:
                case BrainAIState.Phase2TransitionOpen:
                    NPC.dontTakeDamage = true;
                    NPC.rotation *= 0.9f;
                    TeleportTime = 0;

                    float animCounter = Time - 60;
                    if (animCounter >= 0)
                    {
                        if (animCounter == 0)
                            NPC.velocity = Vector2.UnitY * 2f;
                        else if (animCounter < 60)
                            NPC.velocity *= 0.99f;
                        else if (animCounter == 60)
                            NPC.velocity = Vector2.UnitY * -8f;
                        else if (animCounter == 65)
                        {
                            AIState = BrainAIState.Phase2TransitionOpen;
                            PreviousAttack = BrainAIState.Idle;
                            availableAttacks.Clear();

                            SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);

                            if (!Main.dedServ)
                            {
                                //Spawns all of BoC's Phase Transition Gores (GoreIDs 392 -> 395)
                                for (int i = 392; i <= 395; i++)
                                    Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, Main.rand.NextVector2Circular(6f, 6f), i);
                            }

                            for (int j = 0; j < 20; j++)
                                Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(NPC.Hitbox), DustID.Blood, Main.rand.NextVector2Circular(6f, 6f));

                            for (int i = 1; i <= 3; i++)
                            {
                                Color color = i switch
                                {
                                    1 => Color.Yellow,
                                    2 => Color.Orange,
                                    _ => Color.Red,
                                };
                                PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                                GeneralParticleHandler.SpawnParticle(ring);
                            }

                            BoCAfterImages = [];

                            SoundEngine.PlaySound(Roar, NPC.Center);
                        }
                        else
                            NPC.velocity *= 0.9f;

                        if (animCounter < 60f)
                            BoCDrawOffset = Main.rand.NextVector2CircularEdge(1, 1) * MathHelper.Lerp(0f, 16f, CalamityUtils.CircInEasing(animCounter / 60f, 1));
                        else if (animCounter < 70f)
                            BoCDrawOffset = Main.rand.NextVector2CircularEdge(1, 1) * MathHelper.Lerp(16f, 0f, CalamityUtils.CircOutEasing((animCounter - 60) / 10f, 1));

                        if (animCounter >= 120f)
                        {
                            Time = 0;
                            ResetAttackValues();
                            AIState = BrainAIState.Phase2Idle;
                            NPC.dontTakeDamage = false;
                        }
                    }
                    else
                    {
                        NPC.velocity *= 0.8f;

                        #region Tile Collision
                        if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                        {
                            NPC.velocity = NPC.DirectionTo(target.Center) * 4f;
                        }
                        else if (Collision.SolidCollision(NPC.position + NPC.velocity, NPC.width, NPC.height))
                        {
                            if (NPC.velocity.X != NPC.oldVelocity.X)
                                NPC.velocity.X = -NPC.oldVelocity.X;
                            if (NPC.velocity.Y != NPC.oldVelocity.Y)
                                NPC.velocity.Y = -NPC.oldVelocity.Y;
                            NPC.velocity *= 2f;
                            NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Phase 2
                case BrainAIState.Phase2Idle:
                    //goes from 3 at full health to 1 at low health
                    int chases = (int)Math.Ceiling(MaxChases * (NPC.life / (float)NPC.lifeMax));

                    NPC.rotation = NPC.velocity.X / 6f * MathHelper.Pi / 8f;

                    if(Time == ChaseTime - 5)
                        AttackCounter++;

                    if (Time <= ChaseTime)
                    {
                        float speedUp = MathHelper.Clamp((Time - 10) / 10f, 0f, 1f);
                        float slowDown = 1 - MathHelper.Clamp((Time - (ChaseTime - 15)) / 15f, 0f, 1f);
                        float angleChange = MathHelper.Lerp(MathHelper.Pi / 24f, 0f, MathHelper.Clamp(Time / (ChaseTime * 0.666f), 0f, 1f));
                        NPC.velocity = NPC.velocity.RotateDirectionTowards(NPC.DirectionTo(target.Center).ToRotation(), angleChange) * (MathHelper.Lerp(3f, 18f, Time / ChaseTime) * speedUp * slowDown);

                        if (Time == ChaseTime)
                        {
                            Vector2 direction = target.velocity.SafeNormalize(Vector2.UnitX * target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
                            AttackPosition = target.Center + (direction * DefaultTeleportDistance);
                            BoCAfterImages = [];
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            TeleportTime = 0;
                            NPC.Opacity = 1f;
                        }
                    }
                    else
                    {
                        TeleportDuration = IdleTeleportDuration;

                        Vector2 endPoint = AttackPosition;

                        NPC.velocity = Vector2.Zero;

                        if (Time < ChaseTime + (TeleportDuration / 2f))
                        {
                            if (Time % 4 == 0)
                            {
                                Vector2 startPoint = NPC.Center;

                                Vector2 direction = endPoint - startPoint;
                                float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                                Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);

                                Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                                Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);

                                BezierCurve path = new BezierCurve(startPoint, controlPoint1, controlPoint2, endPoint);

                                BrainofCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)(ChaseTime + (TeleportDuration * 0.75f) - Time), BoCFrame);
                                BoCAfterImages.Add(afterimage);
                                GeneralParticleHandler.SpawnParticle(afterimage);
                            }
                            TeleportTime++;
                        }
                        else if (Time == ChaseTime + (TeleportDuration / 2f) && !AttackFlag)
                        {
                            Vector2 start = NPC.Center;
                            NPC.Center = endPoint;
                            AttackFlag = true;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            TeleportTime--;
                            if (TeleportTime < 0)
                            {
                                TeleportTime = 0;
                                Time = -1;
                                AttackFlag = false;

                                #region Attack Selection
                                if (AttackCounter >= chases) //Pick attack
                                {
                                    NPC.rotation = 0;
                                    ResetAttackValues();

                                    if (availableAttacks.Count == 0)
                                    {
                                        bool quickChoice = Main.rand.NextBool();
                                        availableAttacks = [
                                            BrainAIState.Bloodletting,
                                            quickChoice ? BrainAIState.SanguineScythes : BrainAIState.IllusionDash,
                                            Main.rand.NextBool() ? BrainAIState.Phase2Idle : BrainAIState.Bloodletting,
                                            quickChoice ? BrainAIState.IllusionDash : BrainAIState.SanguineScythes,
                                            BrainAIState.IllusionTrick
                                        ];
                                    }

                                    AIState = availableAttacks[0];
                                    availableAttacks.RemoveAt(0);
                                    PreviousAttack = AIState;

                                    if (AIState == BrainAIState.SanguineScythes)
                                    {
                                        Time = -31;
                                        BoCAfterImages = [];
                                    }

                                    NPC.netUpdate = true;
                                }
                                #endregion
                            }
                        }

                        NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
                    }
                    break;
                case BrainAIState.Bloodletting:
                    float endTime = Time - BloodlettingDuration;

                    #region Main Attack
                    if (endTime < 0)
                    {
                        NPC.rotation = (float)Math.Sin(Time / 8f) * MathHelper.Pi / 8f;
                        BoCDrawOffset = Vector2.Zero;

                        if (Time > BloodshotRate) //Doesnt fire first bloodshot
                        {
                            if (Time % IchorRate == 0)
                            {
                                if (Time % (IchorRate * 2) == 0)
                                    SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + NPC.velocity + Main.rand.NextVector2Circular(72, 72), new Vector2(Main.rand.NextFloat(-IchorSpread, IchorSpread), -IchorVelocity), ModContent.ProjectileType<IchorShower>(), IchorShotDamage, 0.5f);
                            }

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                if (Time % BloodshotRate == 0)
                                {
                                    dir = NPC.DirectionTo(target.Center);

                                    for (int i = -2; i <= 2; i++)
                                    {
                                        Vector2 initialDir = dir.RotatedBy(i * MathHelper.Pi / 4f);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, initialDir * BloodshotVelocity, ProjectileID.BloodNautilusShot, BloodShotDamage, 0.5f, ai0: dir.ToRotation() + MathHelper.TwoPi, ai1: initialDir.ToRotation());
                                    }

                                    if (death)
                                    {
                                        Vector2 initialDir = dir.RotatedBy(MathHelper.Pi / 6f);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, initialDir * BloodshotVelocity / 2f, ProjectileID.BloodNautilusShot, BloodShotDamage, 0.5f, ai0: dir.ToRotation() + MathHelper.TwoPi, ai1: initialDir.ToRotation());
                                        initialDir = dir.RotatedBy(-MathHelper.Pi / 6f);
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, initialDir * BloodshotVelocity / 2f, ProjectileID.BloodNautilusShot, BloodShotDamage, 0.5f, ai0: dir.ToRotation() + MathHelper.TwoPi, ai1: initialDir.ToRotation());
                                    }
                                }

                            
                        }
                    }
                    #endregion

                    #region Attack End
                    else
                    {
                        NPC.rotation *= 0.9f;

                        if (endTime == 0)
                            SoundEngine.PlaySound(Roar, NPC.Center);
                        if (endTime >= DashPrepTime)
                        {
                            if (endTime < DashPrepTime + DashReelbackTime)
                            {
                                float reelBackSpeedExponent = 2.6f;
                                float reelBackCompletion = Utils.GetLerpValue(0f, 30, endTime - DashPrepTime, true);
                                float reelBackSpeed = MathHelper.Lerp(4f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                Vector2 reelBackVelocity = Vector2.UnitY * -reelBackSpeed;
                                NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                            }
                            else if (endTime == DashPrepTime + 20)
                                NPC.velocity = Vector2.UnitY * DashVelocity;

                            if (endTime >= DashPrepTime + DashReelbackTime && Time % DashScytheRate == 0)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX * 16f, ModContent.ProjectileType<BloodScythe>(), BloodScytheDamage, 0.5f);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX * -16f, ModContent.ProjectileType<BloodScythe>(), BloodScytheDamage, 0.5f);
                                }
                            }

                            if (endTime > DashPrepTime + DashReelbackTime + DashDuration)
                            {
                                NPC.rotation = 0;
                                SetupForNextAttack();
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Movement

                    if (endTime < 0)
                    {
                        if (Time == 0)
                        {
                            AttackPosition = NPC.Center;
                            if (NPC.Center.X < target.Center.X)
                                AttackSign = -1;
                            else
                                AttackSign = 1;
                        }

                        float waveValue = Time * MathHelper.Pi / BloodshotRate;
                        goalPos = target.Center + new Vector2((float)Math.Cos(waveValue) * HoverDistance.X * AttackSign, (float)(-0.5f * Math.Cos(2 * waveValue) + 0.5f) * -HoverDistance.Y);
                        NPC.velocity = Vector2.Zero;
                        if (Time < 30f)
                            NPC.Center = Vector2.Lerp(AttackPosition, goalPos, CalamityUtils.SineOutEasing(Time / 30f, 1));
                        else
                            NPC.Center = goalPos;
                    }
                    else if (endTime < DashPrepTime)
                    {
                        if (endTime == 0)
                            NPC.velocity = Vector2.UnitX * AttackSign * 8f;
                        else
                        {
                            goalPos = target.Center - Vector2.UnitY * HoverEndHeight;
                            Vector2 accel = new Vector2(0.5f, 1.5f);

                            NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * accel;
                            NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                        }
                    }
                    #endregion

                    break;
                case BrainAIState.SanguineScythes:
                    #region Attack Ending
                    if (AttackCounter > SanguineTeleportCount)
                    {
                        if (Time == SanguineAttackEndDelay)
                        {
                            SoundEngine.PlaySound(Roar, NPC.Center);
                            bool left = target.Center.X > NPC.Center.X;
                            NPC.velocity = Vector2.UnitX * (left ? 18 : -18);
                            NPC.rotation = MathHelper.Pi / 8f * (left ? 1 : -1);
                        }
                        if (Time > SanguineAttackEndDelay + SanguineAttackEndDuration)
                        {
                            NPC.velocity *= 0.8f;
                            NPC.rotation *= 0.8f;
                        }
                        else if (Time >= SanguineAttackEndDelay && Time % SanguineAttackEndIchorRate == 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(Math.Sign(NPC.velocity.X), -2f), ModContent.ProjectileType<IchorShower>(), IchorShotDamage, 0.5f);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(Math.Sign(NPC.velocity.X), -6f), ModContent.ProjectileType<IchorShower>(), IchorShotDamage, 0.5f);
                            }
                        }

                        if (Time > SanguineAttackEndDelay + SanguineAttackEndDuration + 10)
                            SetupForNextAttack();
                    }
                    #endregion
                    #region Attack Start
                    else if (Time < 0)
                    {
                        if(Time == -25)
                            NPC.netUpdate = true;

                        if (Time == -24)
                        {
                            SoundEngine.PlaySound(Roar, NPC.Center);

                            for (int i = 1; i <= 3; i++)
                            {
                                Color color = i switch
                                {
                                    1 => Color.Yellow,
                                    2 => Color.Orange,
                                    _ => Color.Red,
                                };
                                PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                                GeneralParticleHandler.SpawnParticle(ring);
                            }
                        }
                        if (Time == -1)
                        {
                            Vector2 direction = target.velocity.SafeNormalize(Vector2.UnitX * target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
                            float distance = SanguineTeleportDistance;
                            AttackPosition = target.Center + (direction * distance);
                            NPC.netUpdate = true;
                        }
                    }
                    #endregion
                    #region Teleport
                    else
                    {
                        TeleportDuration = SanguineTeleportDuration;

                        Vector2 endPoint = AttackPosition;

                        if (Time < (TeleportDuration / 2f))
                        {
                            if (Time % 4 == 0)
                            {
                                Vector2 startPoint = NPC.Center;

                                Vector2 direction = endPoint - startPoint;
                                float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                                Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);

                                Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                                Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);

                                BezierCurve path = new BezierCurve(startPoint, controlPoint1, controlPoint2, endPoint);

                                BrainofCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)((TeleportDuration * 0.75f) - Time), BoCFrame);
                                BoCAfterImages.Add(afterimage);
                                GeneralParticleHandler.SpawnParticle(afterimage);
                            }
                            TeleportTime++;
                        }
                        else if (Time == (TeleportDuration / 2f) && !AttackFlag)
                        {
                            Vector2 start = NPC.Center;
                            NPC.Center = endPoint;
                            AttackFlag = true;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            TeleportTime--;
                            if (TeleportTime < 0)
                            {
                                TeleportTime = 0;
                                Time = -1;
                                AttackFlag = false;
                                AttackCounter++;

                                if (AttackCounter <= SanguineTeleportCount)
                                {
                                    SoundStyle explosion = new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion")
                                    {
                                        Volume = 0.5f
                                    };
                                    SoundEngine.PlaySound(explosion, NPC.Center);

                                    for (int i = 0; i < SanguineScytheCount; i++)
                                    {
                                        float initalSpeed = 16f;
                                        if (death && i % 2 == 0)
                                            initalSpeed /= 2f;

                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / SanguineScytheCount * i) * initalSpeed, ModContent.ProjectileType<BloodScythe>(), BloodScytheDamage, 0.5f);
                                    }
                                }

                                Vector2 direction;
                                if (AttackCounter < SanguineTeleportCount)
                                {
                                    direction = Main.rand.NextFloat(0f, MathHelper.TwoPi).ToRotationVector2();
                                    AttackPosition = target.Center + (direction * SanguineTeleportDistance);
                                }
                                else
                                {
                                    direction = Vector2.UnitX * (Main.rand.NextBool() ? -1 : 1);
                                    AttackPosition = target.Center + (direction * SanguineFinalTeleportOffset.X) + (Vector2.UnitY * -SanguineFinalTeleportOffset.Y);
                                }

                                NPC.netUpdate = true;

                                BoCAfterImages = [];
                                NPC.Opacity = 1f;
                            }
                        }

                        NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
                    }
                    #endregion
                    break;
                case BrainAIState.CrimsonEyes:
                    #region Attack Start
                    if (Time <= 30)
                    {
                        if (Time >= 6 && Time <= 12)
                        {
                            if (Time == 6)
                            {
                                SoundEngine.PlaySound(Roar, NPC.Center);

                                for (int i = 1; i <= 3; i++)
                                {
                                    Color color = i switch
                                    {
                                        1 => Color.Yellow,
                                        2 => Color.Orange,
                                        _ => Color.Red,
                                    };
                                    PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                                    GeneralParticleHandler.SpawnParticle(ring);
                                }

                                CalamityUtils.AddScreenshakeAt(NPC.Center, 10f);
                            }

                            for (int i = 0; i < 12; i++)
                            {
                                Point start = target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                                for (int j = 0; j < 96; j++)
                                {
                                    Point current = start - new Point(0, j);
                                    if (!Main.tile[current].IsTileSolid() && Main.tile[current - new Point(0, 1)].IsTileSolid())
                                        Dust.NewDust(current.ToWorldCoordinates(0, 0), 16, 16, DustID.Crimstone, 0, 3);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Attack
                    else
                    {
                        #region Eye Spawning
                        if (Time < CrimsonEyeAttackDuration && CalamityUtils.CountProjectiles(ModContent.ProjectileType<CrimsonEye>()) < CrimsonEyeCap && Time % CrimsonEyeRate == 0)
                        {
                            Vector2 spawnPos = target.Center;
                            int i = 0;
                            for (i = 0; i <= 32; i++)
                            {
                                spawnPos = target.Center + Main.rand.NextVector2Circular(256, 256);

                                if (Collision.IsWorldPointSolid(spawnPos))
                                    continue;

                                bool alreadyFilled = false;
                                foreach (Projectile p in Main.ActiveProjectiles)
                                {
                                    if (p.type != ModContent.ProjectileType<CrimsonEye>())
                                        continue;

                                    Rectangle hitbox = new((int)spawnPos.X - 50, (int)spawnPos.Y - 18, 100, 36);

                                    if (p.Hitbox.Intersects(hitbox))
                                    {
                                        alreadyFilled = true;
                                        break;
                                    }
                                }

                                if (alreadyFilled)
                                    continue;

                                if (Collision.CanHitLine(spawnPos, 1, 1, target.position, target.width, target.height))
                                    break;
                            }

                            if (i == 32)
                                spawnPos = target.Center;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<CrimsonEye>(), CrimsonEyeDamage, 0f);
                        }
                        #endregion

                        #region Early Movement
                        if (Time < 180)
                        {
                            float dist = 210;

                            fromTarget = (NPC.Center - target.Center).SafeNormalize(Vector2.UnitX);
                            goalPos = target.Center + (fromTarget * dist) - (Vector2.UnitY * 32);
                            if (NPC.DistanceSQ(goalPos) <= 2048)
                                NPC.velocity *= 0.9f;
                            else if (NPC.velocity.LengthSquared() < 16f) //7.5^2
                                NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 0.1f;
                            else
                                NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 4f;
                        }
                        #endregion

                        #region Scythe Movement + Attack
                        else
                        {
                            if (Time < CrimsonEyeAttackIdleDuration)
                                NPC.velocity *= 0.9f;
                            if (Time >= CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration && Time < CrimsonEyeAttackDuration)
                            {
                                if (Time == CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration)
                                    NPC.velocity = NPC.DirectionTo(target.Center);

                                bool onSurface = target.Center.Y / 16f < Main.worldSurface;
                                speed = onSurface ? 10f : 8f;
                                if (Time < CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration + CrimsonEyeAttackBuildUpDuration)
                                {
                                    float lerp = CalamityUtils.SineInEasing((Time - (CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration)) / CrimsonEyeAttackBuildUpDuration, 1);
                                    speed = MathHelper.Lerp(0f, speed, lerp);
                                }

                                float turnAmt = TurnAccelerationMultiplier * ((NPC.Distance(target.Center) - TurnAccelerationDistanceBuffer) / TurnAccelerationDistanceDivisor);

                                NPC.velocity = NPC.velocity.RotateDirectionTowards(NPC.AngleTo(target.Center), turnAmt) * speed;
                            }
                            else
                            {
                                NPC.velocity *= 0.9f;

                                if (Time == CrimsonEyeAttackDuration)
                                {
                                    foreach (Projectile p in Main.ActiveProjectiles)
                                    {
                                        if (p.type != ModContent.ProjectileType<CrimsonEye>())
                                            continue;

                                        p.timeLeft = 60;
                                    }
                                }

                                if (Time > CrimsonEyeAttackDuration + CrimsonEyeAttackEndDuration)
                                    SetupForNextAttack();
                            }

                            if (Time == CrimsonEyeAttackIdleDuration)
                            {
                                SoundStyle explosion = new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion");
                                explosion.Volume = 0.5f;

                                SoundEngine.PlaySound(explosion, NPC.Center);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    float projCount = 10;
                                    for (int i = 0; i < projCount; i++)
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CirclingBloodScythe>(), BloodScytheDamage, 0.5f, -1, MathHelper.TwoPi / projCount * i);
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                    break;
                case BrainAIState.IllusionDash:
                    #region Attack Start
                    if (Time < IllusionDashTeleportDuration)
                    {
                        if (Time == 0)
                        {
                            AttackPosition = target.Center;
                            AttackRotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
                            NPC.netUpdate = true;
                        }

                        TeleportDuration = IllusionDashTeleportDuration;

                        Vector2 endPoint = AttackPosition + (AttackRotation.ToRotationVector2() * IllusionDashTeleportDistance);

                        if (Time < (TeleportDuration / 2f))
                        {
                            if (Time % 4 == 0)
                            {
                                Vector2 startPoint = NPC.Center;

                                for (int i = 0; i < (death ? 8 : 4); i++)
                                {
                                    Vector2 myEndPoint = AttackPosition + ((AttackRotation + (death ? MathHelper.PiOver4 : MathHelper.PiOver2) * i).ToRotationVector2() * IllusionDashTeleportDistance);
                                    Vector2 direction = myEndPoint - startPoint;

                                    float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                                    Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);

                                    Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                                    Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);

                                    BezierCurve path = new BezierCurve(startPoint, controlPoint1, controlPoint2, myEndPoint);

                                    BrainofCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)((TeleportDuration * 0.75f) - Time), BoCFrame);
                                    BoCAfterImages.Add(afterimage);
                                    GeneralParticleHandler.SpawnParticle(afterimage);
                                }
                            }
                            TeleportTime++;
                        }
                        else if (Time == (TeleportDuration / 2f) && !AttackFlag)
                        {
                            AttackFlag = true;

                            for (int i = 0; i < (death ? 8 : 4); i++)
                            {
                                float rot = AttackRotation + (death ? MathHelper.PiOver4 : MathHelper.PiOver2) * i;
                                Vector2 spawnPos = AttackPosition + (rot.ToRotationVector2() * IllusionDashTeleportDistance);
                                if (i == 0)
                                    NPC.Center = spawnPos;
                                else if (Main.netMode != NetmodeID.MultiplayerClient)
                                    NPC.NewNPCDirect(NPC.GetSource_FromThis(), spawnPos, ModContent.NPCType<BrainIllusion>(), 0, 15, 30, rot).netUpdate = true;
                            }

                            NPC.netUpdate = true;
                        }
                        else
                            TeleportTime--;

                        NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
                    }
                    #endregion
                    else
                    {
                        float startTime = Time - IllusionDashTeleportDuration;

                        if (startTime == 0)
                        {
                            AttackPosition = NPC.Center;
                            TeleportTime = 0;
                            AttackFlag = false;

                            BoCAfterImages = [];
                            NPC.Opacity = 1f;

                            GenericSparkle sparkle = new(NPC.Center, Vector2.Zero, Color.Yellow, Color.Orange, 2f, 12, needed: true);
                            GeneralParticleHandler.SpawnParticle(sparkle);

                            NPC.netUpdate = true;

                            foreach (NPC n in Main.ActiveNPCs)
                                if (n.type == ModContent.NPCType<BrainIllusion>())
                                    n.netUpdate = true;
                        }
                        if (startTime < 30)
                        {
                            float lerp = startTime / 30f;
                            float circleDist = MathHelper.Lerp(IllusionDashTeleportDistance, IllusionDashCloseInDistance, CalamityUtils.SineOutEasing(lerp, 1));
                            NPC.Center = Vector2.Lerp(AttackPosition, target.Center + (AttackRotation.ToRotationVector2() * circleDist), lerp);
                            AttackRotation += MathHelper.Lerp(0f, IllusionDashStartingSpinSpeed, CalamityUtils.SineInEasing(lerp, 1));
                        }
                        else if (startTime <= 30 + IllusionDashSpinDuration)
                        {
                            NPC.Center = target.Center + AttackRotation.ToRotationVector2() * IllusionDashCloseInDistance;

                            AttackRotation += MathHelper.Lerp(IllusionDashStartingSpinSpeed, 0f, CalamityUtils.SineOutEasing((startTime - 30) / (float)IllusionDashSpinDuration, 1));
                        }
                        else if (startTime < 30 + IllusionDashSpinDuration + 30)
                        {
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, 30, startTime - 130, true);
                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = (AttackRotation + MathHelper.Pi).ToRotationVector2() * -reelBackSpeed;
                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                        }
                        else if (startTime <= 30 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration) //176
                        {
                            NPC.velocity = Vector2.Zero;

                            if (startTime == 30 + IllusionDashSpinDuration + 30)
                            {
                                AttackPosition = target.Center;
                                AttackRotation = AttackRotation + MathHelper.Pi + Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
                                NPC.netUpdate = true;
                            }

                            wrappedCounter = startTime - (30 + IllusionDashSpinDuration + 30);

                            TeleportDuration = IllusionDashFakeoutTeleportDuration;

                            Vector2 endPoint = AttackPosition + (AttackRotation.ToRotationVector2() * 270);

                            if (wrappedCounter < (TeleportDuration / 2f))
                            {
                                if (wrappedCounter % 2 == 0)
                                {
                                    Vector2 startPoint = NPC.Center;

                                    Vector2 direction = endPoint - startPoint;

                                    float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                                    Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);

                                    Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                                    Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);

                                    BezierCurve path = new BezierCurve(startPoint, controlPoint1, controlPoint2, endPoint);

                                    BrainofCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)((TeleportDuration * 0.75f) - wrappedCounter), BoCFrame);
                                    BoCAfterImages.Add(afterimage);
                                    GeneralParticleHandler.SpawnParticle(afterimage);
                                }
                                TeleportTime++;
                            }
                            else if (wrappedCounter == (int)(TeleportDuration / 2f) && !AttackFlag)
                            {
                                AttackFlag = true;
                                NPC.Center = endPoint;
                            }
                            else
                            {
                                TeleportTime--;
                                if (TeleportTime <= 0)
                                {
                                    TeleportTime = 0;
                                    BoCAfterImages = [];
                                    ResetAttackValues();
                                    NPC.Opacity = 1f;
                                    NPC.netUpdate = true;
                                    AttackRotation = NPC.AngleTo(target.Center);
                                }
                            }

                            NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
                        }
                        else if (startTime < 90 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration + 30)
                        {
                            NPC.velocity *= 0.9f;
                            if(startTime % 15 == 0)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    dir = NPC.DirectionTo(target.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        Vector2 initialDir = dir.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 4f, MathHelper.Pi / 4f));
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, initialDir * Main.rand.NextFloat(10f, 25f), ProjectileID.BloodNautilusShot, BloodShotDamage, 0.5f, ai0: dir.ToRotation() + MathHelper.TwoPi, ai1: initialDir.ToRotation());
                                    }
                                        NPC.velocity -= dir * 2f;
                                }
                            }
                        }
                        else if(startTime >= 135 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration + 30)
                        {
                            foreach (NPC n in Main.ActiveNPCs)
                                if (n.type == ModContent.NPCType<BrainIllusion>())
                                    n.active = false;

                            SetupForNextAttack();
                        }
                    }

                    
                    break;
                case BrainAIState.IllusionTrick:
                    if (Time >= 90)
                    {
                        if (Time == 90)
                        {
                            foreach(Projectile p in Main.ActiveProjectiles)
                            {
                                if (!p.friendly)
                                    continue;

                                p.Calamity().IgnoreBoCIllusions = true;
                            }

                            int brainAngleSlot = Main.rand.Next(0, IllusionTrickAngleGroups);
                            int brainDistSlot = Main.rand.Next(0, IllusionTrickGroupSize);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                for (int a = 0; a < IllusionTrickAngleGroups; a++)
                                    for (int d = 0; d < IllusionTrickGroupSize; d++)
                                    {
                                        if (a != brainAngleSlot || d != brainDistSlot)
                                            NPC.NewNPC(NPC.GetSource_FromThis(), 0, 0, ModContent.NPCType<FalseBrain>(), 0, MathHelper.TwoPi / IllusionTrickAngleGroups * a, FalseBrain.TimeDivisor / IllusionTrickGroupSize * d);
                                    }

                            AttackTime = (int)(FalseBrain.TimeDivisor / IllusionTrickGroupSize * brainDistSlot);
                            AttackRotation = MathHelper.TwoPi / IllusionTrickAngleGroups * brainAngleSlot;
                            AttackFlag = false;
                            AttackPosition = target.Center;

                            NPC.ShowNameOnHover = false;
                            NPC.netUpdate = true;
                        }

                        NPC.damage = 0;

                        if (AttackFlag)
                        {
                            if (AttackCounter == 0)
                            {
                                NPC.ShowNameOnHover = true;
                                foreach (NPC npc in Main.npc)
                                {
                                    if (npc.type != ModContent.NPCType<FalseBrain>())
                                        continue;
                                    npc.ModNPC<FalseBrain>().BeenHit = true;
                                }

                                NPC.velocity = NPC.DirectionFrom(target.Center) * 8f;
                            }
                            else
                                NPC.velocity *= 0.95f;

                            if (AttackCounter >= IllusionTrickStunDuration)
                            {
                                NPC.damage = NPC.defDamage;
                                SetupForNextAttack();
                                NPC.Opacity = 1f;
                                TeleportTime = 0;                             
                                break;
                            }

                            AttackCounter++;
                        }
                        else if(Time >= IllusionTrickTimeLimit) //Players have failed to find the real BoC within the time limit
                        {
                            if (Time == IllusionTrickTimeLimit)
                            {
                                foreach (Player p in Main.ActivePlayers)
                                {
                                    if (p.Distance(NPC.Center) > DespawnRange)
                                        continue;

                                    AttackList.Add(p.whoAmI);
                                }

                                foreach (NPC n in Main.ActiveNPCs)
                                {
                                    if (n.type != ModContent.NPCType<FalseBrain>())
                                        continue;

                                    n.ModNPC<FalseBrain>().BeenHit = true;
                                }

                                for (int i = 1; i <= 3; i++)
                                {
                                    Color color = i switch
                                    {
                                        1 => Color.Yellow,
                                        2 => Color.Orange,
                                        _ => Color.Red,
                                    };
                                    PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                                    GeneralParticleHandler.SpawnParticle(ring);
                                }
                            }
                            else if(Time % 30 == 0)
                            {
                                if (AttackList.Count > 0)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<TelekineticBlast>(), 0, 0f, -1, AttackList[0], 2f);
                                    AttackList.RemoveAt(0);
                                }
                                else
                                {
                                    Vector2 direction = target.velocity.SafeNormalize(Vector2.UnitX * target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
                                    float distance = DefaultTeleportDistance;
                                    AttackPosition = target.Center + (direction * distance);
                                    BoCAfterImages = [];
                                    NPC.Opacity = 1f;
                                    TeleportTime = 0;
                                    Time = ChaseTime - 1;
                                    NPC.damage = NPC.defDamage;
                                    NPC.netUpdate = true;
                                    ResetAttackValues();

                                    AIState = BrainAIState.Phase2Idle;
                                }
                            }
                        }
                        else
                        {
                            CachedRatio = (float)Math.Cos(AttackTime * MathHelper.TwoPi / FalseBrain.TimeDivisor);

                            float lerp = CalamityUtils.SineInOutEasing(MathHelper.Clamp((Time - 150) / 30f, 0f, 1f), 1);
                            float baseDist = 240;
                            float circleDist = 480;
                            if (Time - 150 < 30f)
                            {
                                baseDist = MathHelper.Lerp(480, 240, lerp);
                                circleDist = MathHelper.Lerp(240, 480, lerp);
                            }
                            NPC.Center = AttackPosition + Vector2.UnitX.RotatedBy(AttackRotation) * (baseDist + (circleDist * ((float)Math.Sin(-AttackTime * MathHelper.TwoPi / FalseBrain.TimeDivisor) / 2f + 0.5f)));
                            NPC.Center += Vector2.UnitX.RotatedBy(AttackRotation + MathHelper.PiOver2) * (90 * (float)Math.Cos(AttackTime * MathHelper.TwoPi / FalseBrain.TimeDivisor));
                            NPC.Opacity = 1f;
                            AttackCounter = 0;
                        }                    
                    }
                    else if (Time <= 60)
                    {
                        TeleportDuration = 60;
                        TeleportTime++;
                        NPC.Opacity = 1 - (Time / 60f);

                        if (Time < 50)
                        {
                            Vector2 startPoint = NPC.Center;
                            Vector2 endPoint = NPC.Center + Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, MathHelper.TwoPi)) * Main.rand.NextFloat(240, 480);

                            Vector2 direction = endPoint - startPoint;
                            float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                            Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);

                            Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                            Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);

                            BezierCurve path = new BezierCurve(startPoint, controlPoint1, controlPoint2, endPoint);

                            BrainofCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)(60 - Time), BoCFrame);
                            BoCAfterImages.Add(afterimage);
                            GeneralParticleHandler.SpawnParticle(afterimage);
                        }
                    }
                    else
                    {
                        NPC.Opacity = 0f;
                        BoCAfterImages = [];
                    }

                    if (Time >= 180)
                        AttackTime++;
                    else if (Time >= 150)
                        AttackTime += (Time - 150) / 30f;
                    break;
                #endregion

                case BrainAIState.DeathAnimation:
                    if (Time == 0)
                        NPC.velocity = NPC.DirectionFrom(target.Center) * 6f;
                    else
                        NPC.velocity *= 0.95f;

                    NPC.rotation = MathHelper.Pi / 24f * NPC.oldVelocity.X;
                    TeleportTime *= 0.6f;
                    if (TeleportTime < 0.005f)
                        TeleportTime = 0;
                    BoCDrawOffset *= 0.6f;
                    NPC.Opacity = BringOpacityTo(NPC.Opacity, 1, 0.1f);

                    (float angle, Vector2 offset, int time)[] bloodGushingData = [
                        (MathHelper.Pi / 6f, new(18, 12), 90),
                        (MathHelper.Pi, new(-30, -10), 150),
                        (-MathHelper.Pi / 4f, new(20, -40), 180),
                        (MathHelper.Pi + MathHelper.Pi / 4f, new(-20, -40), 200),
                        (MathHelper.Pi / 1.75f, new(-5, 22), 210)
                    ];

                    for (int i = 0; i < bloodGushingData.Length; i++)
                    {
                        if (Time >= bloodGushingData[i].time)
                        {
                            if (Time == bloodGushingData[i].time)
                            {
                                Vector2 bloodDir = bloodGushingData[i].angle.ToRotationVector2();
                                BloodParticle2 p2 = new(NPC.Center + bloodGushingData[i].offset.RotatedBy(NPC.rotation), bloodDir * 7.5f, 16, 0.5f, Color.Red);
                                GeneralParticleHandler.SpawnParticle(p2);
                                NPC.velocity = bloodDir * -4f;

                                SoundStyle explosion = new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion")
                                {
                                    Volume = 0.5f,
                                    Pitch = i / (float)(bloodGushingData.Length - 1)
                                };
                                SoundEngine.PlaySound(explosion, NPC.Center);

                                Main.LocalPlayer.SetScreenshake(1f);
                            }

                            for (int j = 0; j < 2; j++)
                            {
                                BloodParticle p = new(NPC.Center + bloodGushingData[i].offset.RotatedBy(NPC.rotation), (bloodGushingData[i].angle + Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f)).ToRotationVector2() * Main.rand.NextFloat(5f, 10f), 32, 1f, Color.Red);
                                GeneralParticleHandler.SpawnParticle(p);
                            }
                        }

                    }

                    if (Time >= 215)
                    {
                        SoundStyle explosion = new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion");
                        SoundEngine.PlaySound(explosion, NPC.Center);

                        Main.LocalPlayer.SetScreenshake(2f);

                        int pCount = 10;
                        for (int i = 0; i < pCount; i++)
                        {
                            float initalSpeed = 24f;
                            Vector2 pVelo = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / pCount * i) * initalSpeed;

                            for (int j = 0; j < 2; j++)
                            {
                                BloodParticle p = new(NPC.Center, pVelo.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
                                GeneralParticleHandler.SpawnParticle(p);
                            }
                            BloodParticle2 p2 = new(NPC.Center, pVelo * 0.75f, 16, 0.5f, Color.Red);
                            GeneralParticleHandler.SpawnParticle(p2);
                        }

                        NPC.dontTakeDamage = false;
                        if(Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.StrikeInstantKill();
                    }

                    float animationCompletion = Time / 215f;
                    NPC.frameCounter += 2 * animationCompletion;

                    if (Main.rand.NextFloat(0.5f, 1f) < animationCompletion)
                    {
                        Vector2 edgeBloodDir = Main.rand.NextVector2CircularEdge(1, 1);
                        BloodParticle b = new(NPC.Center + (edgeBloodDir * NPC.Size * 0.75f), edgeBloodDir.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f)) * Main.rand.NextFloat(2f, 4f), 16, 0.75f, Color.Red);
                        GeneralParticleHandler.SpawnParticle(b);
                    }
                    break;
            }

            #region Projectile Altering
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type != ProjectileID.BloodNautilusShot || p.ai[0] == 0)
                    continue;

                int startUpTime = 20;
                float speedUpTime = 30;
                float slowDownMult = 0.96f;
                float speedUpMult = 1.025f;
                if(AIState == BrainAIState.IllusionDash)
                {
                    startUpTime = 20;
                    speedUpTime = 30;
                    slowDownMult = 0.96f;
                    speedUpMult = 1.025f;
                }

                if (p.ai[2] <= startUpTime)
                    p.velocity *= slowDownMult;
                else
                {
                    p.velocity *= speedUpMult;
                    if (p.ai[2] <= startUpTime + speedUpTime)
                    {
                        float newAngle = p.ai[1].AngleLerp(p.ai[0] - MathHelper.TwoPi, (p.ai[2] - startUpTime) / speedUpTime);

                        p.velocity = newAngle.ToRotationVector2() * p.velocity.Length();
                    }
                }
                p.ai[2]++;
            }
            #endregion

            NPC.oldVelocity = NPC.velocity;
            Time++;

            return false;
        }

        public override void SendExtraAI(BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write((int)PreviousAttack);

            binaryWriter.Write(TeleportTime);
            binaryWriter.Write(TeleportDuration);

            binaryWriter.Write(SpawnTime);
            binaryWriter.Write(SpawnDelay);

            binaryWriter.Write(CachedRatio);

            binaryWriter.WriteFlags(OnSecondCreeperPhase, isNegative, AttackFlag);

            binaryWriter.Write(AttackRotation);
            binaryWriter.Write(AttackTime);
            binaryWriter.Write(AttackCounter);

            binaryWriter.WritePackedVector2(AttackPosition);

            binaryWriter.Write(availableAttacks.Count);
            for (int i = 0; i < availableAttacks.Count; i++)
                binaryWriter.Write((int)availableAttacks[i]);

            binaryWriter.Write(AttackList.Count);
            for (int i = 0; i < AttackList.Count; i++)
                binaryWriter.Write(AttackList[i]);
        }

        public override void ReceiveExtraAI(BitReader bitReader, BinaryReader binaryReader)
        {
            PreviousAttack = (BrainAIState)binaryReader.ReadInt32();

            TeleportTime = binaryReader.ReadSingle();
            TeleportDuration = binaryReader.ReadSingle();

            SpawnTime = binaryReader.ReadSingle();
            SpawnDelay = binaryReader.ReadInt32();

            CachedRatio = binaryReader.ReadSingle();

            binaryReader.ReadFlags(out OnSecondCreeperPhase, out isNegative, out AttackFlag);

            AttackRotation = binaryReader.ReadSingle();
            AttackTime = binaryReader.ReadSingle();
            AttackCounter = binaryReader.ReadInt32();

            AttackPosition = binaryReader.ReadPackedVector2();

            int availableLength = binaryReader.ReadInt32();
            availableAttacks.Clear();
            for (int i = 0; i < availableLength; i++)
                availableAttacks.Add((BrainAIState)binaryReader.ReadInt32());

            int attackLength = binaryReader.ReadInt32();
            AttackList.Clear();
            for (int i = 0; i < attackLength; i++)
                AttackList.Add(binaryReader.ReadSingle());
        }

        public override bool? CanBeHitByProjectile(Mod mod, Projectile projectile)
        {
            if (AIState == BrainAIState.IllusionTrick && !AttackFlag && projectile.Calamity().IgnoreBoCIllusions)
                return false;
            return base.CanBeHitByProjectile(mod, projectile);
        }

        public override void ModifyHitByItem(Mod mod, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (AIState != BrainAIState.DeathAnimation)
                modifiers.SetMaxDamage(NPC.life - 1);
        }

        public override void ModifyHitByProjectile(Mod mod, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (AIState != BrainAIState.DeathAnimation)
                modifiers.SetMaxDamage(NPC.life - 1);
        }

        public override void HitEffect(Mod mod, NPC.HitInfo hit)
        {
            if (AIState != BrainAIState.DeathAnimation && (NPC.life + 1) <= hit.Damage)
            {
                NPC.life = 1;
                NPC.BossBar = null;
                NPC.dontTakeDamage = true;

                if (AIState == BrainAIState.Stunned)
                    TeleportTime = 0;

                AIState = BrainAIState.DeathAnimation;
                ResetAttackValues();
                Time = 0;
                return;
            }

            if (AIState == BrainAIState.IllusionTrick && Time < 960)
                AttackFlag = true;
        }

        public override bool PreKill(Mod mod)
        {
            return AIState == BrainAIState.DeathAnimation && !NPC.dontTakeDamage;
        }

        public override void FindFrame(Mod mod, int frameHeight)
        {
            if (BoCFrame == Rectangle.Empty)
                BoCFrame = TextureAssets.Npc[NPCID.BrainofCthulhu].Frame(verticalFrames: 8);

            if (NPC.frameCounter == 0)
                BoCFrame.Y += frameHeight;

            if (AIState <= BrainAIState.Phase2TransitionClosed)
            {
                if (BoCFrame.Y > frameHeight * 3)
                    BoCFrame.Y = 0;
                return;
            }
            if (BoCFrame.Y < frameHeight * 4)
                BoCFrame.Y = frameHeight * 4;

            if (BoCFrame.Y > frameHeight * 7)
                BoCFrame.Y = frameHeight * 4;
        }

        public override bool PreDraw(Mod mod, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool phase1 = AIState < BrainAIState.Phase2TransitionClosed;
            bool drawBrain = true;

            if (phase1)
            {
                List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper).ToList();
                creepers.Sort((a, b) => b.DistanceSQ(NPC.Center).CompareTo(a.DistanceSQ(NPC.Center)));

                int c = 0;
                foreach (NPC creeper in creepers)
                {
                    List<VerletSimulatedSegment> curvePoints = RevBoCSystem.VerletTendrils[creeper.AIOverride<CreeperAI>().CreeperID];
                    if (curvePoints is null)
                        continue;

                    float glowIntensity = creeper.AIOverride<CreeperAI>().ConnectionOpacity;
                    Color ichorLess = Color.Lerp(Color.Transparent, Color.OrangeRed * 0.333f, glowIntensity);
                    Color ichorful = Color.Lerp(Color.OrangeRed * 0.25f, Color.Orange * 0.666f, glowIntensity);

                    for (int i = 0; i < curvePoints.Count; i++)
                    {
                        Vector2 start = curvePoints[i].position;
                        Vector2 end = i == curvePoints.Count - 1 ? creeper.Center : curvePoints[i + 1].position;
                        Vector2 center = (end + start) / 2f;
                        start -= Main.screenPosition;
                        end -= Main.screenPosition;

                        float rotation = (end - start).ToRotation() - MathHelper.PiOver2;

                        float flowTime = creeper.AIOverride<CreeperAI>().FlowTime;
                        float flowAmt = creeper.AIOverride<CreeperAI>().FlowAmount;
                        float ichorRatio = CalamityUtils.ExpInEasing((float)Math.Sin((flowTime + (i * flowAmt)) * 2f) / 2f + 0.5f, 1);

                        Color glowColor = Color.Lerp(ichorLess, ichorful, ichorRatio);

                        float dist = Vector2.Distance(start, end);
                        Vector2 tendrilScale = new(1f + (0.5f * (ichorRatio * (1 + (glowIntensity * 0.5f)))), dist / RevBoCSystem.tendril.Height());

                        spriteBatch.Draw(RevBoCSystem.tendril.Value, start, null, Lighting.GetColor(center.ToTileCoordinates()) * NPC.Opacity, rotation, RevBoCSystem.tendril.Size() * Vector2.UnitX * 0.5f, tendrilScale, SpriteEffects.None, 0f);
                        spriteBatch.Draw(RevBoCSystem.GetTendrilGlow(), start, null, glowColor * NPC.Opacity, rotation, RevBoCSystem.GetTendrilGlow().Size() * Vector2.UnitX * 0.5f, tendrilScale, SpriteEffects.None, 0f);
                    }

                    c++;
                }

                foreach (NPC creeper in creepers)
                {
                    float glowIntensity = creeper.AIOverride<CreeperAI>().ConnectionOpacity;

                    spriteBatch.Draw(RevBoCSystem.GetCreeperGlow(), creeper.Center - Main.screenPosition, null, Color.Orange * glowIntensity, creeper.rotation, TextureAssets.Npc[NPCID.Creeper].Size() * 0.5f, creeper.scale * 1.15f, 0, 0);

                    spriteBatch.Draw(TextureAssets.Npc[NPCID.Creeper].Value, creeper.Center - Main.screenPosition, null, Lighting.GetColor(creeper.Center.ToTileCoordinates()).MultiplyRGB(Color.Lerp(Color.White, new Color(255, 180, 180), glowIntensity)), creeper.rotation, TextureAssets.Npc[NPCID.Creeper].Size() * 0.5f, creeper.scale, 0, 0);
                }
            }
            else
            {
                List<NPC> falseBrains = Main.npc.Where(n => n.active && n.type == ModContent.NPCType<FalseBrain>()).ToList();
                if (falseBrains.Count > 0)
                {
                    drawBrain = false;

                    falseBrains.Add(NPC);
                    falseBrains.Sort((a, b) =>
                    {
                        float aValue;
                        float bValue;

                        if (a.ModNPC is FalseBrain falseA)
                            aValue = falseA.DrawPriority;
                        else
                            aValue = a.AIOverride<BrainOfCthulhuAI>().CachedRatio;

                        if (b.ModNPC is FalseBrain falseB)
                            bValue = falseB.DrawPriority;
                        else
                            bValue = b.AIOverride<BrainOfCthulhuAI>().CachedRatio;

                        return aValue.CompareTo(bValue);
                    });

                    foreach (NPC n in falseBrains)
                    {
                        if (n.ModNPC is FalseBrain falseBrain)
                            falseBrain.DrawSelf(spriteBatch, screenPos, Lighting.GetColor(n.Center.ToTileCoordinates()));
                        else
                            DrawBrainLikeFakes(spriteBatch, n);
                    }
                }
            }

            if (drawBrain)
                DrawBrain(spriteBatch, NPC);

            return false;
        }

        private static void DrawBrain(SpriteBatch spriteBatch, NPC brain)
        {
            BrainOfCthulhuAI ai = brain.AIOverride<BrainOfCthulhuAI>();
            bool phase1 = ai.AIState < BrainAIState.Phase2TransitionClosed;

            Vector2 drawPos = brain.Center + ai.BoCDrawOffset + (Vector2.UnitY * 16) - Main.screenPosition;
            Vector2 scale = Vector2.One;

            if (!phase1 && ai.TeleportTime != 0)
            {
                ai.BoCAfterImages.RemoveAll(p => p.Time > p.Lifetime);
                foreach (Particle p in ai.BoCAfterImages)
                    p.CustomDraw(spriteBatch);

                //Color glowColor = Color.White * (teleportCounter / 30f);
                scale = Vector2.Lerp(Vector2.One, new Vector2(0.5f + ((float)Math.Cos(ai.Time / (ai.TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f), 0.5f + ((float)Math.Sin(ai.Time / (ai.TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f)), CalamityUtils.SineInOutEasing(ai.TeleportTime / (ai.TeleportDuration / 2f), 1));

                spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, drawPos, ai.BoCFrame, Lighting.GetColor(brain.Center.ToTileCoordinates()) * brain.Opacity, brain.rotation, ai.BoCFrame.Size() * 0.5f, scale * brain.scale, 0, 0);
                //spriteBatch.Draw(GetBrainGlow(), drawPos, NPC.frame, glowColor, NPC.rotation, NPC.frame.Size() * 0.5f, scale, 0, 0);
            }
            else
                spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, drawPos, ai.BoCFrame, Lighting.GetColor(brain.Center.ToTileCoordinates()) * brain.Opacity, brain.rotation, ai.BoCFrame.Size() * 0.5f, scale * brain.scale, 0, 0);
        }

        private static void DrawBrainLikeFakes(SpriteBatch spriteBatch, NPC brain)
        {
            BrainOfCthulhuAI ai = brain.AIOverride<BrainOfCthulhuAI>();

            Vector2 scaleDistort = new Vector2((float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f);

            int spawnTime = 150 - ((int)ai.Time);
            float startLerp = spawnTime / 60f;

            Color drawColor = Lighting.GetColor(brain.Center.ToTileCoordinates());

            if (spawnTime > 0)
            {
                drawColor *= (1 - startLerp);
                scaleDistort *= startLerp;
            }
            else
                scaleDistort = Vector2.Zero;

            spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, brain.Center + (Vector2.UnitY * 16) - Main.screenPosition, ai.BoCFrame, drawColor, brain.rotation, ai.BoCFrame.Size() * 0.5f, (Vector2.One + scaleDistort) * brain.scale, 0, 0);
        }

        private void SetupForNextAttack()
        {
            Player target = Main.player[NPC.target];

            Vector2 direction = target.velocity.SafeNormalize(Vector2.UnitX * target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
            float distance = DefaultTeleportDistance;
            AttackPosition = target.Center + (direction * distance);
            BoCAfterImages = [];
            Time = ChaseTime - 1;
            ResetAttackValues();
            NPC.netUpdate = true;

            if (availableAttacks.Count != 0)
            {
                if (availableAttacks[0] != BrainAIState.Phase2Idle)
                    AttackCounter = 4;
                else
                    availableAttacks.RemoveAt(0);
            }

            AIState = BrainAIState.Phase2Idle;
        }

        private void ResetAttackValues()
        {
            isNegative = false;
            AttackRotation = 0;
            AttackTime = 0;
            AttackFlag = false;
            AttackPosition = Vector2.Zero;
            AttackCounter = 0;
        }

        public static int GetBrainOfCthuluCreepersCountRevDeath() => CalamityWorld.death ? 30 : 21;

        private static float BringOpacityTo(float currentOpacity, float goalOpacity, float changeAmount = 0.025f)
        {
            if (currentOpacity == goalOpacity)
                return goalOpacity;

            if (currentOpacity < goalOpacity)
            {
                currentOpacity += changeAmount;
                if (currentOpacity >= goalOpacity)
                    return goalOpacity;
                else
                    return currentOpacity;
            }
            else
            {
                currentOpacity -= changeAmount;
                if (currentOpacity <= goalOpacity)
                    return goalOpacity;
                else
                    return currentOpacity;
            }
        }

        public class CreeperAI : VanillaAIOverride
        {
            internal float FlowTime = 0;
            internal float FlowAmount => MathHelper.Lerp(0.1f, 0.15f, NPC.localAI[3]);

            internal enum CreeperAIState
            {
                Idle,
                Charge
            }

            internal int CreeperID { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
            internal CreeperAIState AIState { get => (CreeperAIState)NPC.ai[1]; set => NPC.ai[1] = (float)value; }
            internal int Time = -1;
            internal ref float AttackAngle => ref NPC.ai[2];
            internal ref float CachedValue1 => ref NPC.ai[3];
            internal int CachedValue2 = 0;
            internal int PartnerIndex = -1;
            internal Vector2 AttackPosition = Vector2.Zero;
            internal float ConnectionOpacity = 0f;

            public override bool AI(Mod mod)
            {
                #region Despawn
                if (NPC.crimsonBoss < 0)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                    return false;
                }
                #endregion

                #region Targetting
                if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                    CalamityUtils.CalamityTargeting(NPC, default);
                #endregion

                #region Creeper Count Variables
                int creeperCount = NPC.CountNPCS(NPC.type);
                if (creeperCount > GetBrainOfCthuluCreepersCountRevDeath())
                    creeperCount = GetBrainOfCthuluCreepersCountRevDeath();
                int localCreeperID = Main.npc.Where(n => n.active && n.type == NPCID.Creeper).ToList().IndexOf(NPC);
                float CreeperAmountRatio = creeperCount / (float)GetBrainOfCthuluCreepersCountRevDeath();
                #endregion

                NPC brain = Main.npc[NPC.crimsonBoss];
                BrainOfCthulhuAI bocAI = brain.AIOverride<BrainOfCthulhuAI>();

                bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

                float bossCounter = bocAI.Time;

                bool evenID = CreeperID % 2 == 0;

                if (bocAI.AIState < 0)
                    NPC.damage = 0;

                List<BrainAIState> bossAIStatesToUse = [
                    BrainAIState.UndergroundSpawnAnimation,
                    BrainAIState.SurfaceSpawnAnimation,
                    BrainAIState.Stunned,
                    BrainAIState.CreeperSwipes,
                    BrainAIState.CreeperCrush,
                    BrainAIState.CreeperOrbit,
                    BrainAIState.CreeperSpiral,
                    BrainAIState.DesperateOnslaught
                ];
                bool useBossAIState = bossAIStatesToUse.Contains(bocAI.AIState) && bossCounter >= 0;

                if (bocAI.AIState == BrainAIState.UndergroundSpawnAnimation || bocAI.AIState == BrainAIState.SurfaceSpawnAnimation)
                    NPC.dontTakeDamage = true;
                else
                    NPC.dontTakeDamage = false;

                if (useBossAIState)
                    switch (bocAI.AIState)
                    {
                        case BrainAIState.UndergroundSpawnAnimation:
                        case BrainAIState.SurfaceSpawnAnimation:
                            float baseRotation = MathHelper.TwoPi / (GetBrainOfCthuluCreepersCountRevDeath() / 2f) * (CreeperID / 2f);
                            Vector2 goalLocation;
                            float speedCap = 8f;
                            float accel = 1f;
                            float acceptableDist = 9216;
                            Player player = Main.player[NPC.target];

                            if (bocAI.SpawnTime != 0 && Time >= 0) //Brain has appeared
                            {
                                float brainTime = bossCounter - Math.Abs(bocAI.SpawnTime);

                                if (brainTime < 180)
                                {
                                    Vector2 baseOffset = new Vector2(evenID ? -360 : 360, 0);
                                    Vector2 rotationOffset = Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 120f * (evenID ? -1 : 1))) * 128;
                                    goalLocation = player.Center + baseOffset + rotationOffset;
                                }
                                else if (brainTime < 240)
                                {
                                    Vector2 baseOffset = new Vector2(evenID ? -200 : 200, 64);
                                    Vector2 rotationOffset = Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 60f * (evenID ? -1 : 1))) * 32;
                                    goalLocation = brain.Center + baseOffset + rotationOffset;
                                    speedCap = 12;
                                    accel = 2f;
                                    acceptableDist = 4096;
                                }
                                else
                                {
                                    Vector2 baseOffset = new Vector2(evenID ? -360 : 360, -64);
                                    Vector2 rotationOffset = Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 60f * (evenID ? -1 : 1))) * 128;
                                    goalLocation = brain.Center + baseOffset + rotationOffset;
                                    speedCap = 12;
                                    accel = 2f;
                                    acceptableDist = 4096;
                                }
                                Time++;
                            }
                            else
                            {
                                if (Time >= 0) //Has been brought down
                                {
                                    Vector2 baseOffset = new Vector2(evenID ? -360 : 360, 0);
                                    Vector2 rotationOffset = Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 120f * (evenID ? -1 : 1))) * 128;
                                    goalLocation = player.Center + baseOffset + rotationOffset;
                                    Time++;
                                }
                                else //Hasnt been brought down
                                {
                                    Vector2 baseOffset = new Vector2(evenID ? -256 : 256, 0);
                                    Vector2 rotationOffset = Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 120f * (evenID ? -1 : 1))) * 64;
                                    goalLocation = brain.Center + baseOffset + rotationOffset;
                                }
                            }

                            if (NPC.DistanceSQ(goalLocation) > acceptableDist)
                            {
                                NPC.velocity += NPC.DirectionTo(goalLocation) * accel;
                                NPC.velocity = NPC.velocity.ClampMagnitude(0f, speedCap);
                            }

                            break;
                        case BrainAIState.DesperateOnslaught:
                            if (Time == 0 || AttackPosition == Vector2.Zero)
                            {
                                Vector2 dir = Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-MathHelper.TwoPi / 3f, MathHelper.TwoPi / 3f)) * (evenID ? -1 : 1);
                                AttackAngle = dir.ToRotation();
                                float rayDist = CalamityUtils.PreciseDistanceToTileCollisionHit(brain.Center, AttackAngle, 800, 4);
                                AttackPosition = (dir * (rayDist - 64));
                                NPC.netUpdate = true;
                            }

                            if (Time > 0)
                            {
                                goalLocation = brain.Center + AttackPosition;

                                if(WorldGen.SolidOrSlopedTile(goalLocation.ToTileCoordinates().X, goalLocation.ToTileCoordinates().Y))
                                {
                                    float rayDist = CalamityUtils.PreciseDistanceToTileCollisionHit(brain.Center, AttackAngle, 800, 4);
                                    AttackPosition = (AttackAngle.ToRotationVector2() * (rayDist - 64));
                                    goalLocation = brain.Center + AttackPosition;
                                }

                                if (NPC.DistanceSQ(goalLocation) > 4096)
                                {
                                    NPC.velocity += NPC.DirectionTo(goalLocation);
                                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 10f);
                                }
                                else
                                    NPC.velocity *= 0.8f;
                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
                                NPC.damage = 0;
                            }
                            else
                                useBossAIState = false;

                            Time++;
                            break;
                        case BrainAIState.Stunned:
                            NPC.velocity *= 0.9f;
                            NPC.damage = 0;
                            break;
                        case BrainAIState.CreeperSwipes:
                            if (bossCounter < 15)
                            {
                                useBossAIState = false;
                                Time = -1;
                                AIState = CreeperAIState.Idle;
                                break;
                            }

                            if (brain.AIOverride<BrainOfCthulhuAI>().AttackList.Contains(NPC.whoAmI))
                            {
                                if (Time < 0)
                                    Time = 0;
                            }
                            else
                            {
                                if (Time >= 0)
                                    Time = -1;
                            }

                            baseRotation = MathHelper.TwoPi / (GetBrainOfCthuluCreepersCountRevDeath() / 2f) * (CreeperID / 2f);
                            bool singleHand = bocAI.AttackFlag;
                            int handSide = bocAI.AttackSign;

                            if (Time == -1)
                            {
                                NPC.damage = 0;
                                NPC.knockBackResist = 0.72f;

                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);

                                if (!singleHand)
                                    goalLocation = brain.Center + (Vector2.UnitY * -32f) + (Vector2.UnitX * (evenID ? -256 : 256)) + (Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 30f * (evenID ? -1 : 1))) * new Vector2(32, 96));
                                else
                                    goalLocation = brain.Center + (Vector2.UnitY * -32f) + (Vector2.UnitX * 256 * handSide) + (Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 30f * handSide)) * new Vector2(32, 96));

                                goalLocation += Main.player[brain.target].velocity * 24f;
                                float distToGoal = NPC.Center.Distance(goalLocation);
                                NPC.velocity = NPC.DirectionTo(goalLocation) * (2f + MathHelper.Clamp(distToGoal / 24f, 0f, 64f));
                            }
                            else
                            {
                                if(Time == 0)
                                    NPC.netUpdate = true;

                                NPC.damage = NPC.defDamage;
                                NPC.knockBackResist = 0f;

                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);

                                accel = 2f;
                                float speed = 12f;

                                if (Time < brain.AIOverride<BrainOfCthulhuAI>().SwipeDelay)
                                {
                                    if (!singleHand)
                                        goalLocation = brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * (evenID ? -300 : 300)) + (Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 30f * (evenID ? -1 : 1))) * new Vector2(32, 96));
                                    else
                                        goalLocation = brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * 300 * handSide) + (Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 30f * handSide)) * new Vector2(32, 96));

                                    goalLocation += Main.player[brain.target].velocity * 24f;
                                    NPC.velocity += NPC.DirectionTo(goalLocation) * accel;
                                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, speed);
                                }
                                else
                                {
                                    if (!singleHand)
                                        goalLocation = brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * (evenID ? -256 : 256)) + (Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 30f * (evenID ? -1 : 1))) * new Vector2(32, 96));
                                    else
                                        goalLocation = brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * 256 * handSide) + (Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 30f * handSide)) * new Vector2(32, 96));

                                    goalLocation.Y += MathHelper.Lerp(0, 420, CalamityUtils.SineInOutEasing(MathHelper.Clamp((Time - brain.AIOverride<BrainOfCthulhuAI>().SwipeDelay) / 20f, 0f, 1f), 1));

                                    if (!singleHand)
                                        goalLocation.X += MathHelper.Lerp(0f, 900f, CalamityUtils.SineInOutEasing(MathHelper.Clamp((Time - (brain.AIOverride<BrainOfCthulhuAI>().SwipeDelay + 5)) / 55f, 0f, 1f), 1)) * (evenID ? 1 : -1);
                                    else
                                        goalLocation.X += MathHelper.Lerp(0f, 900f, CalamityUtils.SineInOutEasing(MathHelper.Clamp((Time - (brain.AIOverride<BrainOfCthulhuAI>().SwipeDelay + 5)) / 55f, 0f, 1f), 1)) * (handSide == -1 ? 1 : -1);

                                    NPC.Center = Vector2.Lerp(NPC.Center, goalLocation, MathHelper.Clamp((Time - brain.AIOverride<BrainOfCthulhuAI>().SwipeDelay) / 10f, 0f, 1f));
                                }

                                Time++;
                            }

                            break;
                        case BrainAIState.CreeperCrush:
                            if(brain.AIOverride<BrainOfCthulhuAI>().AttackList.Contains(NPC.whoAmI))
                            {
                                if (Time < 0)
                                    Time = 0;
                            }
                            else
                            {
                                if (Time >= 0)
                                    Time = -1;
                            }

                            if (Time < 0)
                            {
                                baseRotation = (brain.Center - Main.player[brain.target].Center).ToRotation();
                                evenID = CreeperID % 2 == 0;
                                goalLocation = brain.Center + Vector2.UnitX.RotatedBy(baseRotation + Math.Sin(bossCounter / 60f + CreeperID) * (evenID ? -1 : 1)) * (evenID ? 175 : 250);
                                if (NPC.DistanceSQ(goalLocation) > 9216)
                                {
                                    NPC.velocity += NPC.DirectionTo(goalLocation);
                                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                                }
                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
                            }
                            else
                            {
                                Player sharedTarget = Main.player[brain.target];
                                Vector2 dashDir = AttackAngle.ToRotationVector2();
                                if (!bocAI.OnSecondCreeperPhase) // Swipe Attack
                                {
                                    float positioningTime = LightSwipeTravelTime + LightSwipeAttackDelay;

                                    if (Time <= positioningTime)
                                    {
                                        if (AttackPosition == Vector2.Zero)
                                            AttackPosition = NPC.Center;
                                        Vector2 goalPosition = sharedTarget.Center - dashDir * 128;
                                        goalPosition += dashDir.RotatedBy(MathHelper.PiOver2) * 16;
                                        NPC.Center = Vector2.Lerp(AttackPosition, goalPosition, CalamityUtils.SineOutEasing(MathHelper.Clamp(Time / (float)LightSwipeTravelTime, 0f, 1f), 1));
                                        NPC.velocity = Vector2.Zero;
                                    }
                                    else
                                    {
                                        AttackPosition = Vector2.Zero;
                                        NPC.damage = NPC.defDamage;
                                        NPC.knockBackResist = 0f;
                                        int reelbackTime = 22;

                                        if (Time < (positioningTime + reelbackTime))
                                        {
                                            float reelBackSpeedExponent = 2.6f;
                                            float reelBackCompletion = Utils.GetLerpValue(0f, reelbackTime, Time - positioningTime, true);
                                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                            Vector2 reelBackVelocity = dashDir * -reelBackSpeed;
                                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                            if (Time == positioningTime + reelbackTime - 5)
                                                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, NPC.Center);
                                        }
                                        else if (Time == (positioningTime + reelbackTime))
                                            NPC.velocity = dashDir * 32;
                                        else
                                        {
                                            NPC.velocity *= 0.9f;
                                            if (Time >= positioningTime + reelbackTime + 15)
                                            {
                                                NPC.damage = 0;
                                                Time = -1;
                                                brain.AIOverride<BrainOfCthulhuAI>().AttackList.Remove(NPC.whoAmI);
                                                AttackPosition = Vector2.Zero;
                                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else //Crush Attack
                                {
                                    float positioningTime = StrongSwipeTravelTime + StrongSwipeAttackDelay;

                                    if (Time <= positioningTime)
                                    {
                                        if (AttackPosition == Vector2.Zero)
                                            AttackPosition = NPC.Center;
                                        Vector2 goalPosition = sharedTarget.Center - dashDir * 128;
                                        NPC.Center = Vector2.Lerp(AttackPosition, goalPosition, CalamityUtils.SineOutEasing(MathHelper.Clamp(Time / (float)StrongSwipeTravelTime, 0f, 1f), 1));
                                        NPC.velocity = Vector2.Zero;
                                    }
                                    else
                                    {
                                        NPC.damage = NPC.defDamage;
                                        NPC.knockBackResist = 0f;
                                        int reelbackTime = 18;
                                        float swingTime = 10f;

                                        if (Time <= (positioningTime + reelbackTime))
                                        {
                                            float reelBackSpeedExponent = 2.6f;
                                            float reelBackCompletion = Utils.GetLerpValue(0f, reelbackTime, Time - positioningTime, true);
                                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                            Vector2 reelBackVelocity = dashDir * -reelBackSpeed;
                                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                            if (Time == positioningTime + reelbackTime - 5)
                                                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, NPC.Center);

                                            if (Time == (positioningTime + reelbackTime))
                                                AttackPosition = NPC.Center;
                                        }
                                        else if (Time <= (positioningTime + reelbackTime + swingTime))
                                        {
                                            if (Main.npc[PartnerIndex].active)
                                            {
                                                float moveDist = 196;
                                                NPC.Center = Vector2.Lerp(AttackPosition, AttackPosition + dashDir * moveDist, (Time - (positioningTime + reelbackTime)) / swingTime);
                                                NPC.velocity = Vector2.Zero;
                                                if (Time == (positioningTime + reelbackTime + swingTime))
                                                {
                                                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);
                                                    SoundEngine.PlaySound(NPC.HitSound, NPC.Center);
                                                    NPC.velocity = dashDir * -8;
                                                    Vector2 attackCenter = AttackPosition + dashDir * 200;
                                                    if (dashDir.X <= 0)
                                                    {
                                                        //WaterFoam (0.25 -> 1), SoftRoundExplosion (0.025 -> 0.1), SmokeExplosion(0.05 -> 0.15)
                                                        CustomPulse splatter = new(attackCenter, Vector2.Zero, Color.Red, "CalamityMod/Particles/SmokeExplosion", Vector2.One, Main.rand.NextFloatDirection(), 0.05f, 0.15f, 24);
                                                        GeneralParticleHandler.SpawnParticle(splatter);
                                                    }

                                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                                        for (int i = -1; i <= 1; i++)
                                                            Projectile.NewProjectile(NPC.GetSource_FromThis(), attackCenter, dashDir.RotatedBy(MathHelper.PiOver2 + (MathHelper.Pi / 6f * i)) * 8f, ProjectileID.BloodShot, BloodShotDamage, 0.5f);
                                                }
                                            }
                                            else
                                                NPC.velocity = dashDir * 19.6f;
                                        }
                                        else if (Time < (positioningTime + reelbackTime + swingTime + 30))
                                        {
                                            NPC.velocity *= 0.96f;
                                        }
                                        else
                                        {
                                            NPC.damage = 0;
                                            Time = -1;
                                            brain.AIOverride<BrainOfCthulhuAI>().AttackList.Remove(NPC.whoAmI);
                                            AttackPosition = Vector2.Zero;
                                            ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0.5f);
                                            break;
                                        }
                                    }
                                }

                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
                                Time++;
                            }
                            break;
                        case BrainAIState.CreeperOrbit:
                            if (bossCounter == 0)
                            {
                                CachedValue1 = MathHelper.TwoPi / creeperCount * localCreeperID;
                                AttackAngle = 0;
                            }
                            float dist = OrbitStandardRadius + ((float)Math.Sin((CachedValue1 * 7) + bossCounter / 20f) * 24);

                            if (brain.AIOverride<BrainOfCthulhuAI>().AttackList.Contains(NPC.whoAmI))
                            {
                                if (Time < 0)
                                    Time = 0;
                            }
                            else
                            {
                                if (Time >= 0)
                                    Time = -1;
                            }

                            if (Time >= 0)
                            {
                                float telegraphPeriod = OrbitAttackInterval * 0.25f;
                                float shiftPeriod = (OrbitAttackInterval - telegraphPeriod) / 2f;
                                if (Time < telegraphPeriod)
                                    dist = MathHelper.Lerp(dist, OrbitTelegraphRadius, CalamityUtils.SineOutEasing(Time / telegraphPeriod, 1));
                                else if (Time < shiftPeriod + telegraphPeriod)
                                    dist = MathHelper.Lerp(OrbitTelegraphRadius, 16, CalamityUtils.SineInOutEasing((Time - telegraphPeriod) / shiftPeriod, 1));
                                else
                                {
                                    dist = MathHelper.Lerp(16, dist, CalamityUtils.SineInOutEasing((Time - telegraphPeriod - shiftPeriod) / shiftPeriod, 1));
                                    if (Time > OrbitAttackInterval)
                                    {
                                        Time = -2;
                                        brain.AIOverride<BrainOfCthulhuAI>().AttackList.Remove(NPC.whoAmI);
                                    }
                                }
                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f, 0.1f);
                                Time++;
                            }
                            else
                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f, 0.05f);

                            float slowDown = (1 - MathHelper.Clamp((bossCounter - OrbitDuration) / 30f, 0f, 1f));
                            AttackAngle += (BaseRotationSpeed * (MathHelper.Lerp(1f, 0.5f, CreeperAmountRatio) + (bocAI.OnSecondCreeperPhase ? 1f : 0.5f))) * slowDown * bocAI.AttackSign;
                            Vector2 rotation = Vector2.UnitX.RotatedBy(CachedValue1 + AttackAngle) * dist;
                            Player target = Main.player[brain.target];

                            if (bossCounter < OrbitSetupDuration)
                            {
                                NPC.damage = 0;

                                if (bossCounter == 0)
                                    AttackPosition = NPC.Center;
                                goalLocation = target.Center + rotation;
                                if (bossCounter < OrbitSetupDuration)
                                    NPC.Center = Vector2.Lerp(AttackPosition, goalLocation, CalamityUtils.SineOutEasing(bossCounter / OrbitSetupDuration, 1));
                                else
                                    NPC.Center = goalLocation;
                            }
                            else
                            {
                                NPC.damage = NPC.defDamage;

                                if (bossCounter <= OrbitSetupDuration + 10)
                                {
                                    float lerp = CalamityUtils.SineOutEasing((bossCounter - OrbitSetupDuration) / 10f, 1);
                                    AttackPosition = Vector2.Lerp(target.Center, AttackPosition, lerp);
                                }
                                else
                                {
                                    float prox = target.Distance(AttackPosition);
                                    if (prox > 256)
                                        AttackPosition += target.DirectionFrom(AttackPosition) * ((prox - 256) / 16f);
                                }

                                goalLocation = AttackPosition + rotation;
                                NPC.Center = goalLocation;
                            }
                            break;
                        case BrainAIState.CreeperSpiral:
                            int tendrilID = (CreeperID % TendrilCount) + 1;

                            if (bossCounter == 0)
                            {
                                AttackPosition = Vector2.Zero;
                                List<NPC> myGroup = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && (n.ai[0] % TendrilCount) + 1 == tendrilID).ToList();
                                CachedValue1 = myGroup.IndexOf(NPC);
                                CachedValue2 = myGroup.Count;
                                AttackAngle = 0;
                            }

                            int index = (int)CachedValue1;
                            int groupCount = CachedValue2;
                            float placementRatio = (index + 1) / (float)(groupCount + 1);

                            float spiralAngle = bocAI.AttackRotation;
                            float goalAngle = spiralAngle + ((MathHelper.TwoPi / TendrilCount) * tendrilID);

                            float angularVelocity = (goalAngle - AttackAngle) / (16f + (placementRatio * 16f));
                            
                            AttackAngle += angularVelocity;

                            float goalRadius = TendrilStartDistance + (TendrilLength * placementRatio);
                            goalRadius += (float)Math.Sin(bossCounter / 20f) * MathHelper.Lerp(MaxCreeperSway, 0, groupCount / (float)(GetBrainOfCthuluCreepersCountRevDeath() / 3)) * (index % 2 == 0 ? -1 : 1);

                            Vector2 position = brain.Center + (AttackAngle.ToRotationVector2() * goalRadius);
                            if (bossCounter >= SpiralSetupTime)
                            {
                                NPC.Center = position;

                                NPC.damage = NPC.defDamage;
                                NPC.knockBackResist = 0f;

                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
                            }
                            else if (bossCounter < SpiralSetupTime / 10f)
                                NPC.velocity *= 0.66f;
                            else
                            {
                                if (AttackPosition == Vector2.Zero)
                                    AttackPosition = NPC.Center;
                                NPC.velocity = Vector2.Zero;
                                float lerp = (bossCounter - (SpiralSetupTime / 10f)) / (float)(SpiralSetupTime * 0.9f);
                                NPC.Center = Vector2.Lerp(AttackPosition, position, CalamityUtils.SineInOutEasing(MathHelper.Clamp(lerp, 0f, 1f), 1));
                            }
                            break;
                    }

                else
                    switch (AIState)
                    {
                        case CreeperAIState.Idle:
                            if (Time == 0)
                            {
                                AIState = CreeperAIState.Charge;
                                break;
                            }

                            NPC.knockBackResist = 0.72f;
                            NPC.damage = 0;

                            float baseRotation = MathHelper.TwoPi / (GetBrainOfCthuluCreepersCountRevDeath() / 2f) * (CreeperID / 2f);
                            Vector2 goalLocation = brain.Center + (Vector2.UnitX * (evenID ? -256 : 256)) + Vector2.UnitX.RotatedBy(baseRotation + (bossCounter / 60f * (evenID ? -1 : 1))) * 64;
                            if (NPC.DistanceSQ(goalLocation) > 9216)
                            {
                                NPC.velocity += NPC.DirectionTo(goalLocation);
                                NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                            }
                            ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
                            Time = -1;
                            AttackPosition = Vector2.Zero;
                            break;
                        case CreeperAIState.Charge:
                            Player target = Main.player[NPC.target];

                            if (Time < CreeperChargePositioningTime)
                            {
                                goalLocation = target.Center + ((NPC.Center - target.Center).SafeNormalize(-Vector2.UnitY).RotatedBy((evenID ? -MathHelper.PiOver4 : MathHelper.PiOver4)) * 96);
                                if (NPC.DistanceSQ(goalLocation) > 4096)
                                {
                                    NPC.velocity += NPC.DirectionTo(goalLocation);
                                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 12f);
                                }
                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
                            }
                            else
                            {
                                NPC.damage = NPC.defDamage;
                                NPC.knockBackResist = 0f;

                                if (Time < (CreeperChargePositioningTime + CreeperChargeWindUpTime))
                                {
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, CreeperChargeWindUpTime, Time - CreeperChargePositioningTime, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = NPC.DirectionTo(Main.player[brain.target].Center) * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    if (Time == CreeperChargePositioningTime + CreeperChargeWindUpTime - 5)
                                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, NPC.Center);
                                }
                                else if (Time == CreeperChargePositioningTime + CreeperChargeWindUpTime)
                                    NPC.velocity = NPC.DirectionTo(Main.player[brain.target].Center) * 24;
                                else
                                {
                                    NPC.velocity *= 0.975f;
                                    if (Time >= CreeperChargePositioningTime + CreeperChargeWindUpTime + 30)
                                    {
                                        NPC.damage = 0;
                                        Time = -10;
                                        AIState = CreeperAIState.Idle;
                                        break;
                                    }
                                }
                            }
                            Time++;
                            break;
                    }

                FlowTime += 0.01f * (2 * (1 + ConnectionOpacity));

                return false;
            }

            public override void SendExtraAI(BitWriter bitWriter, BinaryWriter binaryWriter)
            {
                binaryWriter.Write(Time);
                binaryWriter.Write(CachedValue2);
                binaryWriter.Write(PartnerIndex);

                binaryWriter.WritePackedVector2(AttackPosition);

            }

            public override void ReceiveExtraAI(BitReader bitReader, BinaryReader binaryReader)
            {
                Time = binaryReader.ReadInt32();
                CachedValue2 = binaryReader.ReadInt32();
                PartnerIndex = binaryReader.ReadInt32();

                AttackPosition = binaryReader.ReadPackedVector2();
            }

            public override void HitEffect(Mod mod, NPC.HitInfo hit)
            {
                if(NPC.life <= 0)
                {
                    List<VerletSimulatedSegment> verletTendril = RevBoCSystem.VerletTendrils[NPC.AIOverride<CreeperAI>().CreeperID];
                    if (verletTendril is null)
                        return;

                    for (int i = 0; i < verletTendril.Count; i++)
                    {
                        Vector2 start = verletTendril[i].position;
                        Vector2 end = i == verletTendril.Count - 1 ? NPC.Center : verletTendril[i + 1].position;

                        float rotation = (end - start).ToRotation();

                        float dist = Vector2.Distance(start, end);
                        Vector2 scale = new(1f, dist / RevBoCSystem.tendril.Height());

                        BrokenTendril gore = new(start, Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * -Main.rand.NextFloat(2f, 4f), rotation, scale, 60);
                        GeneralParticleHandler.SpawnParticle(gore);

                    }

                    RevBoCSystem.VerletTendrils[NPC.AIOverride<CreeperAI>().CreeperID] = [];
                }
            }

            public override bool PreDraw(Mod mod, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
        }
    }

    public class RevBoCSystem : ModSystem
    {
        internal static Asset<Texture2D> tendril;
        private static Texture2D tendrilGlow = null;
        private static Texture2D brainGlow = null;
        private static Texture2D creeperGlow = null;

        internal static float ScreenBlurStrength = 0f;

        internal static List<VerletSimulatedSegment>[] VerletTendrils = new List<VerletSimulatedSegment>[BrainOfCthulhuAI.GetBrainOfCthuluCreepersCountRevDeath()];

        private static int previousMusic = -1;
        public static int PreviousMusic => previousMusic;

        public override void OnModLoad()
        {
            if (!Main.dedServ)
            {
                tendril = ModContent.Request<Texture2D>("Terraria/Images/Chain12");
            }

            On_NPC.SpawnBoss += SpawnBrainNoMessage;
            On_Player.ItemCheck_UseBossSpawners += BlockRoar;
            On_Main.UpdateAudio_DecideOnNewMusic += StopBoss3FromStarting;
            On_Main.UpdateAudio_DecideOnTOWMusic += StopOWBoss1FromStarting;
        }

        private void StopBoss3FromStarting(On_Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
        {
            orig(self);

            if (NPC.crimsonBoss == -1 || !CalamityWorld.revenge)
                return;

            if (previousMusic < 0 || previousMusic >= Main.musicFade.Length)
                return;

            var brainAI = Main.npc[NPC.crimsonBoss].AIOverride<BrainOfCthulhuAI>();
            // The last part leaves one frame at the end of the spawn animation where the boss music starts playing, so that it can be instantly maxed out
            if (brainAI.AIState < 0 && (brainAI.Time - Math.Abs(brainAI.SpawnTime) < 420))
            {
                if (Main.newMusic == MusicID.Boss3)
                    Main.newMusic = previousMusic;

                if (Main.curMusic == MusicID.Boss3)
                    Main.curMusic = previousMusic;
            }
        }

        private void StopOWBoss1FromStarting(On_Main.orig_UpdateAudio_DecideOnTOWMusic orig, Main self)
        {
            orig(self);

            if (NPC.crimsonBoss == -1 || !CalamityWorld.revenge)
                return;

            if (previousMusic < 0 || previousMusic >= Main.musicFade.Length)
                return;

            var brainAI = Main.npc[NPC.crimsonBoss].AIOverride<BrainOfCthulhuAI>();
            // The last part leaves one frame at the end of the spawn animation where the boss music starts playing, so that it can be instantly maxed out
            if (brainAI.AIState < 0 && (brainAI.Time - Math.Abs(brainAI.SpawnTime) < 420))
            {
                if (Main.newMusic == MusicID.OtherworldlyBoss1)
                    Main.newMusic = previousMusic;

                if (Main.curMusic == MusicID.OtherworldlyBoss1)
                    Main.curMusic = previousMusic;
            }
        }

        private void BlockRoar(On_Player.orig_ItemCheck_UseBossSpawners orig, Player self, int onWhichPlayer, Item sItem)
        {
            if (sItem.type != ItemID.BloodySpine || !CalamityWorld.revenge || !self.ItemTimeIsZero || self.itemAnimation <= 0)
            {
                orig(self, onWhichPlayer, sItem);
                return;
            }

            SoundEngine.PlaySound(SoundID.NPCDeath1, Main.LocalPlayer.Center);

            if (self.ZoneCrimson)
            {
                self.ApplyItemTime(sItem);
                CalamityUtils.SpawnBossUsingItem(self, NPCID.BrainofCthulhu);
            }
        }

        private void SpawnBrainNoMessage(On_NPC.orig_SpawnBoss orig, int spawnPositionX, int spawnPositionY, int Type, int targetPlayerIndex)
        {
            if (Type != NPCID.BrainofCthulhu || !CalamityWorld.revenge)
            {
                orig(spawnPositionX, spawnPositionY, Type, targetPlayerIndex);
                return;
            }

            int num = NPC.NewNPC(NPC.GetBossSpawnSource(targetPlayerIndex), spawnPositionX, spawnPositionY, Type, 1);

            if (num == 200 || num == -1)
                return;

            if (Main.player[targetPlayerIndex].HeldItem.type == ItemID.BloodySpine)
                BrainOfCthulhuAI.SummonedViaItem = true;

            NPC.crimsonBoss = num;
            Main.npc[num].target = targetPlayerIndex;
            Main.npc[num].timeLeft *= 20;

            previousMusic = Main.curMusic;

            if (Main.netMode == NetmodeID.Server && num < 200)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num);
        }

        internal static Texture2D GetTendrilGlow()
        {
            if(tendrilGlow == null)
            {
                var tex = new Texture2D(Main.graphics.GraphicsDevice, tendril.Value.Width, tendril.Value.Height);

                var BaseArray = new Color[tex.Width * tex.Height];
                var ColorArray = new Color[tex.Width * tex.Height];
                tendril.Value.GetData(BaseArray);
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
                }
                tex.SetData(ColorArray);
                tendrilGlow = tex;
            }
            return tendrilGlow;
        }

        internal static Texture2D GetBrainGlow()
        {
            if (brainGlow == null)
            {
                var tex = new Texture2D(Main.graphics.GraphicsDevice, TextureAssets.Npc[NPCID.BrainofCthulhu].Value.Width, TextureAssets.Npc[NPCID.BrainofCthulhu].Value.Height);

                var BaseArray = new Color[tex.Width * tex.Height];
                var ColorArray = new Color[tex.Width * tex.Height];
                TextureAssets.Npc[NPCID.BrainofCthulhu].Value.GetData(BaseArray);
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
                }
                tex.SetData(ColorArray);
                brainGlow = tex;
            }
            return brainGlow;
        }

        internal static Texture2D GetCreeperGlow()
        {
            if (creeperGlow == null)
            {
                var tex = new Texture2D(Main.graphics.GraphicsDevice, TextureAssets.Npc[NPCID.Creeper].Value.Width, TextureAssets.Npc[NPCID.Creeper].Value.Height);

                var BaseArray = new Color[tex.Width * tex.Height];
                var ColorArray = new Color[tex.Width * tex.Height];
                TextureAssets.Npc[NPCID.Creeper].Value.GetData(BaseArray);
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
                }
                tex.SetData(ColorArray);
                creeperGlow = tex;
            }
            return creeperGlow;
        }

        public override void PostUpdateNPCs()
        {
            if (!NPC.AnyNPCs(NPCID.BrainofCthulhu))
                BrainOfCthulhuAI.SummonedViaItem = false;

            if (Main.netMode != NetmodeID.Server)
            {
                if (NPC.crimsonBoss != -1 && CalamityWorld.revenge && Main.npc[NPC.crimsonBoss].ai[0] < (float)BrainOfCthulhuAI.BrainAIState.Phase2TransitionClosed)
                {
                    List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper).ToList();

                    foreach (NPC creeper in creepers)
                    {
                        int creeperID = (int)creeper.ai[0];

                        Vector2 startPoint = Main.npc[NPC.crimsonBoss].Center + Vector2.UnitY * 32;

                        float creeperRatio = creeperID / (float)BrainOfCthulhuAI.GetBrainOfCthuluCreepersCountRevDeath();
                        if (creeperID % 2 == 0)
                            startPoint += new Vector2(MathHelper.Lerp(-24, 0, creeperRatio), 0);
                        else
                            startPoint += new Vector2(MathHelper.Lerp(24, 0, creeperRatio), 0);

                        Vector2 endPoint = creeper.Center;

                        List<VerletSimulatedSegment> tendril = VerletTendrils[creeperID];
                        if (tendril is null || tendril.Count == 0)
                            continue;

                        tendril[0].position = startPoint;
                        tendril[0].locked = true;
                        tendril[^1].position = endPoint;
                        tendril[^1].locked = true;

                        VerletSimulatedSegment.SimpleSimulation(tendril, 16, 10, 3);
                    }
                }
            }
        }

        public override void PostDrawTiles()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (NPC.crimsonBoss == -1 || !CalamityWorld.revenge || Main.npc[NPC.crimsonBoss].ai[0] >= (float)BrainOfCthulhuAI.BrainAIState.Phase2TransitionClosed)
                {
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseOpacity(0);
                    if (Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].IsActive())
                        Filters.Scene.Deactivate("CalamityMod:BrainOfCthulhuForcefield");
                }
                else
                {
                    if (!Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].IsActive())
                        Filters.Scene.Activate("CalamityMod:BrainOfCthulhuForcefield");

                    NPC target = Main.npc[NPC.crimsonBoss];
                    Vector2 targetPos = target.Center;
                    float shieldOpacity = target.AIOverride<BrainOfCthulhuAI>().ShieldOpacity;
                    float shieldScale = target.AIOverride<BrainOfCthulhuAI>().ShieldScale;
                    targetPos = Vector2.Transform(targetPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.ScreenSize.ToVector2();

                    Texture2D voronoi = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes3").Value;
                    Texture2D depthNoise = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Veins").Value;

                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["voronoi"].SetValue(voronoi);
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["depthNoise"].SetValue(depthNoise);
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["uScreenResolution"].SetValue(new Vector2(Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height));
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseProgress(0.15f * shieldScale);
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseOpacity(shieldOpacity);
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseColor(Color.Red);
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseSecondaryColor(new Color(255, 0, 90)); //Crimson color (R:220,G:20,B:60,A:255). // Magenta color (R:255,G:0,B:255,A:255).
                    Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseDirection(targetPos);
                }

                if (ScreenBlurStrength == 0f)
                {
                    Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseIntensity(0);
                    if (Filters.Scene["CalamityMod:RadialBlurShader"].IsActive())
                        Filters.Scene.Deactivate("CalamityMod:RadialBlurShader");
                    return;
                }

                if (NPC.crimsonBoss == -1)
                {
                    ScreenBlurStrength = Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().Intensity * 0.9f;
                    Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseIntensity(ScreenBlurStrength);
                    if (ScreenBlurStrength < 0.01f)
                        ScreenBlurStrength = 0f;
                    return;
                }

                if (Filters.Scene["CalamityMod:RadialBlurShader"].IsLoaded)
                {
                    if (!Filters.Scene["CalamityMod:RadialBlurShader"].IsActive())
                        Filters.Scene.Activate("CalamityMod:RadialBlurShader");
                    NPC boss = Main.npc[NPC.crimsonBoss];
                    float counter = boss.ai[1] - boss.ai[2] - 240;
                    float distSQ = Main.LocalPlayer.DistanceSQ(boss.Center);
                    float distanceScaleFactor = 1;
                    if (distSQ > 592900) //770^2
                        distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(distSQ) - 770) / 32f));

                    Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseIntensity((ScreenBlurStrength + (((float)Math.Cos(counter * MathHelper.TwoPi / 15f) / 2f + 0.5f) * (0.4f * ScreenBlurStrength))) * distanceScaleFactor);
                    Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().Shader.Parameters["uSaturation"].SetValue(20);

                    Vector2 targetPos = Vector2.Transform(boss.Center - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.ScreenSize.ToVector2();

                    Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseDirection(targetPos);

                }
            }
        }

        public override void PostUpdateEverything()
        {
            if (Main.curMusic != MusicID.Boss3 && Main.curMusic != MusicID.OtherworldlyBoss1)
                previousMusic = Main.curMusic;

            if (previousMusic == -1)
                previousMusic = MusicID.Crimson;

        }
    }

    public class BrokenTendril : Particle
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool UseCustomDraw => true;

        private int TimeLeft;
        private float Opacity = 1f;
        private Vector2 InitalScale = Vector2.One;

        public BrokenTendril(Vector2 position, Vector2 velocity, float rotation, Vector2 scale, int lifeTime)
        {
            Position = position;
            Velocity = velocity;
            Scale = 1f;
            Rotation = rotation;
            TimeLeft = lifeTime;
            InitalScale = scale - Vector2.One;
        }

        public override void Update()
        {
            if (InitalScale != Vector2.Zero)
            {
                InitalScale *= 0.98f;
                if (InitalScale.X < 0.05f)
                    InitalScale.X = 0;
                if (InitalScale.Y < 0.05f)
                    InitalScale.Y = 0;
            }

            Point tilePos = Position.ToTileCoordinates();
            if(!WorldGen.InWorld(tilePos.X, tilePos.Y))
            {
                Kill();
                return;
            }

            if (Main.tile[tilePos].IsTileSolid() || TileID.Sets.Platforms[Main.tile[tilePos].TileType])
            {
                Velocity.Y = 0;
                if (Velocity.X > 0.05f)
                    Velocity.X *= 0.9f;
                else
                    Velocity.X = 0;
            }
            else
            {
                Velocity.Y += 0.25f;
                Velocity.X *= 0.975f;
            }

            Rotation += Velocity.X * 0.025f;

            if (Velocity == Vector2.Zero)
            {
                if (TimeLeft < 30)
                {
                    Opacity = TimeLeft / 30f;
                    if (TimeLeft <= 0)
                        Kill();
                }
                TimeLeft--;
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = RevBoCSystem.tendril.Value;
            float rot = Rotation - MathHelper.PiOver2;
            Vector2 center = Position + rot.ToRotationVector2() * tex.Size().Y * InitalScale.Y * 0.5f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Lighting.GetColor(center.ToTileCoordinates()) * Opacity, rot, tex.Size() * Vector2.UnitX * 0.5f, Vector2.One + InitalScale, SpriteEffects.None, 0f);
        }
    }

    public class BrainofCthulhuAfterImage : Particle
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool UseCustomDraw => false;

        public override bool SetLifetime => true;

        private float StartFade;
        private float Opacity = 1f;
        List<Vector2> Path;
        private Vector2 MyScale = Vector2.One;
        private Rectangle Frame;

        public BrainofCthulhuAfterImage(BezierCurve path, float rotation, Vector2 scale, int lifeTime, Rectangle frame, float startFadeRatio = 0f)
        {
            Path = path.GetPoints(lifeTime);
            Position = Path[0];
            Rotation = rotation;
            StartFade = startFadeRatio;
            MyScale = scale;
            Frame = frame;
            Lifetime = lifeTime + 1;
        }

        public override void Update()
        {
            float timeRatio = Time / (float)Lifetime;
            Opacity = timeRatio;
            if (StartFade != 0f)
                Opacity = Utils.GetLerpValue(StartFade, 1f, timeRatio, true);

            Opacity = 1 - CalamityUtils.SineInEasing(Opacity, 1);

            List<Vector2> pathPosition = Path;

            if(Time >= pathPosition.Count)
            {
                Kill();
                return;
            }

            Position = pathPosition[Time];
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float timeRatio = Time / (float)Lifetime;
            spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, Position - Main.screenPosition, Frame, Lighting.GetColor(Position.ToTileCoordinates()) * Opacity * 0.5f, Rotation, Frame.Size() * 0.5f, MyScale, 0, 0);
        }
    }

    public class BossRoar : Particle
    {
        public override string Texture => "CalamityMod/Particles/RoarPulse";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private float OriginalScale;
        private float FinalScale;
        private float BaseOpacity;
        private float opacity;
        private Color BaseColor;

        public BossRoar(Vector2 position, Color color, float rotation, float originalScale, float finalScale, int lifeTime, float baseOpacity = 1f)
        {
            Position = position;
            BaseColor = color;
            OriginalScale = originalScale;
            FinalScale = finalScale;
            Scale = originalScale;
            Lifetime = lifeTime;
            BaseOpacity = baseOpacity;
            Rotation = rotation;
        }

        public override void Update()
        {
            Scale = MathHelper.Lerp(OriginalScale, FinalScale, LifetimeCompletion);

            opacity = 1f;
            if (LifetimeCompletion < 0.1f)
                opacity = MathHelper.Lerp(0f, 1f, LifetimeCompletion * 10);

            Color = BaseColor * opacity;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * BaseOpacity, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    [AutoloadBossHead]
    public class BrainIllusion : ModNPC, ILocalizedModType
    {
        public new string LocalizationCategory => "NPCs";
        public override string Texture => "Terraria/Images/NPC_266";
        public override string BossHeadTexture => "Terraria/Images/NPC_Head_Boss_23";

        ref float Time => ref NPC.ai[0];

        ref float AttackValue => ref NPC.ai[1];

        ref float Angle => ref NPC.ai[2];

        ref float TeleportDuration => ref NPC.localAI[0];

        ref float TeleportTime => ref NPC.ai[3];

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.width = 160;
            NPC.height = 110;
            NPC.damage = 20;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            NPC.npcSlots = 0f;
            NPC.netAlways = true;
            Music = MusicID.Boss3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            TeleportDuration = AttackValue;
            AttackValue = 0;
            TeleportTime = Time;
        }

        public override void AI()
        {
            if(NPC.crimsonBoss == -1)
            {
                NPC.active = false;
                return;
            }
            NPC brain = Main.npc[NPC.crimsonBoss];
            Player target = Main.player[brain.target];
            NPC.GivenName = brain.GivenOrTypeName + $": {brain.life}/{brain.lifeMax}";

            #region Attack Start
            if (Time < 30)
            {
                TeleportDuration = 30;

                if (Time == (TeleportDuration / 2f) && AttackValue == 0)
                {
                    AttackValue = 1;
                }
                else
                {
                    TeleportTime--;
                }

                NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
            }
            #endregion
            else
            {
                if (Time == 30)
                {
                    NPC.oldPos[0] = NPC.Center;
                    TeleportTime = 0;
                    AttackValue = 0;
                    NPC.Opacity = 1f;
                }
                if (Time < 60)
                {
                    float lerp = (Time - 30) / 30f;
                    float circleDist = MathHelper.Lerp(BrainOfCthulhuAI.IllusionDashTeleportDistance, BrainOfCthulhuAI.IllusionDashCloseInDistance, CalamityUtils.SineInOutEasing(lerp, 1));
                    NPC.Center = Vector2.Lerp(NPC.oldPos[0], target.Center + (Angle.ToRotationVector2() * circleDist), lerp);
                    Angle += MathHelper.Lerp(0f, BrainOfCthulhuAI.IllusionDashStartingSpinSpeed, CalamityUtils.SineInEasing(lerp, 1));
                }
                else if (Time <= 60f + BrainOfCthulhuAI.IllusionDashSpinDuration)
                {
                    NPC.Center = target.Center + Angle.ToRotationVector2() * BrainOfCthulhuAI.IllusionDashCloseInDistance;

                    Angle += MathHelper.Lerp(BrainOfCthulhuAI.IllusionDashStartingSpinSpeed, 0f, CalamityUtils.SineOutEasing((Time - 60) / (float)BrainOfCthulhuAI.IllusionDashSpinDuration, 1));
                }
                else if (Time <= 90 + BrainOfCthulhuAI.IllusionDashSpinDuration)
                {
                    float reelBackSpeedExponent = 2.6f;
                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time - (60 + BrainOfCthulhuAI.IllusionDashSpinDuration), true);
                    float reelBackSpeed = MathHelper.Lerp(4f, 20f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                    Vector2 reelBackVelocity = (Angle + MathHelper.Pi).ToRotationVector2() * -reelBackSpeed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                }
                else if (Time == 91 + BrainOfCthulhuAI.IllusionDashSpinDuration)
                    NPC.velocity = (Angle + MathHelper.Pi).ToRotationVector2() * BrainOfCthulhuAI.IllusionDashVelocity;
                else if (Time <= 91 + BrainOfCthulhuAI.IllusionDashSpinDuration + BrainOfCthulhuAI.IllusionDashTeleportDuration)
                {
                    NPC.Opacity = MathHelper.Lerp(1f, 0.25f, CalamityUtils.SineInEasing((Time - (91 + BrainOfCthulhuAI.IllusionDashSpinDuration)) / (float)BrainOfCthulhuAI.IllusionDashTeleportDuration, 1));
                    if (NPC.Opacity < 0.666f)
                        NPC.damage = 0;
                }
                else
                    NPC.active = false;
            }

            Time++;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter > 6.0)
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y += frameHeight;
            }

            if (NPC.frame.Y < frameHeight * 4)
            {
                NPC.frame.Y = frameHeight * 4;
            }
            if (NPC.frame.Y > frameHeight * 7)
            {
                NPC.frame.Y = frameHeight * 4;
            }
        }

        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 drawPos = NPC.Center + (Vector2.UnitY * 16) - Main.screenPosition;
            Vector2 scale = Vector2.One;

            if (TeleportTime != 0)
            {
                //Color glowColor = Color.White * (teleportCounter / 30f);
                scale = Vector2.Lerp(Vector2.One, new Vector2(0.5f + ((float)Math.Cos(Time / (TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f), 0.5f + ((float)Math.Sin(Time / (TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f)), CalamityUtils.SineInOutEasing(TeleportTime / (TeleportDuration / 2f), 1));

                spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, drawPos, NPC.frame, Lighting.GetColor(NPC.Center.ToTileCoordinates()) * NPC.Opacity, NPC.rotation, NPC.frame.Size() * 0.5f, scale * NPC.scale, 0, 0);
                //spriteBatch.Draw(GetBrainGlow(), drawPos, npc.frame, glowColor, npc.rotation, npc.frame.Size() * 0.5f, scale, 0, 0);
            }
            else
                spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, drawPos, NPC.frame, Lighting.GetColor(NPC.Center.ToTileCoordinates()) * NPC.Opacity, NPC.rotation, NPC.frame.Size() * 0.5f, scale * NPC.scale, 0, 0);
            return false;
        }
    }

    [AutoloadBossHead]
    public class FalseBrain : ModNPC, ILocalizedModType
    {
        public new string LocalizationCategory => "NPCs";
        public override string Texture => "CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/FalseBrain";
        public override string BossHeadTexture => "Terraria/Images/NPC_Head_Boss_23";

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 160;
            NPC.height = 110;
            NPC.damage = 0;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            NPC.npcSlots = 0f;
            NPC.netAlways = true;
            NPC.ShowNameOnHover = false;
            Music = MusicID.Boss3;

            NPC.localAI[0] = Main.rand.Next(5);
            NPC.localAI[1] = 1 + Main.rand.NextFloat(-0.5f, 0.5f);
        }
        private int Variant => (int)NPC.localAI[0];
        private float Angle => NPC.ai[0];
        private ref float Time => ref NPC.ai[1];
        internal bool BeenHit = false;
        int AttackTime = 0;
        int SpawnTime = 60;

        internal static float TimeDivisor => 360f;
        internal float DrawPriority => (float)Math.Cos(-Time * MathHelper.TwoPi / TimeDivisor);
        internal static List<int> blackListedProjectiles = [];

        public override void AI()
        {
            if(NPC.crimsonBoss == -1)
            {
                NPC.active = false;
                return;
            }    

            NPC brain = Main.npc[NPC.crimsonBoss];
            NPC.GivenName = brain.GivenOrTypeName + $": {brain.life}/{brain.lifeMax}";

            if (BeenHit)
            {
                NPC.dontTakeDamage = true;

                if(AttackTime == 60)
                    NPC.active = false;

                AttackTime++;
            }
            else
            {
                float lerp = CalamityUtils.SineInOutEasing(MathHelper.Clamp(SpawnTime / -30f, 0f, 1f), 1);
                float baseDist = 240;
                float circleDist = 480;
                if (SpawnTime != -30)
                {
                    baseDist = MathHelper.Lerp(480, 240, lerp);
                    circleDist = MathHelper.Lerp(240, 480, lerp);
                }
                NPC.Center = brain.AIOverride<BrainOfCthulhuAI>().AttackPosition + Vector2.UnitX.RotatedBy(Angle) * (baseDist + (circleDist * ((float)Math.Sin(-Time * MathHelper.TwoPi / TimeDivisor) / 2f + 0.5f)));
                NPC.Center += Vector2.UnitX.RotatedBy(Angle + MathHelper.PiOver2) * (90 * (float)Math.Cos(Time * MathHelper.TwoPi / TimeDivisor));
            }

            if (SpawnTime > 0)
            {
                if (--SpawnTime == 0)
                    NPC.dontTakeDamage = false;
            }
            else if (SpawnTime > -30)
                Time += --SpawnTime / -30f;
            else
                Time++;
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            SmiteFool(player);
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (!projectile.Calamity().IgnoreBoCIllusions && projectile.owner != -1)
                SmiteFool(Main.player[projectile.owner]);
        }

        private void SmiteFool(Player fool)
        {
            if (!BeenHit)
            {
                for(int i = 0; i < 6; i++)
                {
                    Vector2 dir = fool.Center - NPC.Center;
                    int lifeTime = 24;
                    dir /= lifeTime / 2f * 5f;
                    dir *= i;
                    DirectionalPulseRing pulse = new(NPC.Center, dir, i % 2 == 0 ? Color.Red : Color.Orange, new Vector2(0.5f, 1), dir.ToRotation(), 0f, i / 5f, lifeTime + 8);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }    

                BeenHit = true;
                SoundEngine.PlaySound(SoundID.Zombie105, NPC.Center); //LC Laugh
                fool.AddBuff(BuffID.Darkness, 900);
                fool.AddBuff(BuffID.Bleeding, 900);
                fool.AddBuff(BuffID.Confused, 60);
                int timeToAdd = 600;
                int bbIndex = fool.buffType.ToList().IndexOf(ModContent.BuffType<BurningBlood>());
                if (bbIndex != -1)
                {
                    timeToAdd /= 2;
                    timeToAdd += fool.buffTime[bbIndex];
                }
                fool.AddBuff(ModContent.BuffType<BurningBlood>(), timeToAdd);

                fool.Calamity().adrenaline = 0;

                NPC.dontTakeDamage = true;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.2f * NPC.localAI[1];
            if ((int)NPC.frameCounter > 3)
                NPC.frameCounter = 0;
        }

        internal void DrawSelf(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            Rectangle frame = tex.Frame(5, 4, Variant, (int)NPC.frameCounter);

            Vector2 scaleDistort = new Vector2((float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f);

            float endLerp = AttackTime / 60f;
            float startLerp = SpawnTime / 60f;

            if (SpawnTime > 0)
            {
                drawColor *= (1 - startLerp);
                scaleDistort *= startLerp;
            }
            else
            {
                drawColor = Color.Lerp(drawColor, Color.Red, endLerp) * (1 - endLerp);
                scaleDistort *= endLerp;
            }

            spriteBatch.Draw(tex, NPC.Center + (Vector2.UnitY * 16) - screenPos, frame, drawColor, NPC.rotation, frame.Size() * 0.5f, (Vector2.One + scaleDistort) * NPC.scale, 0, 0);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
    }

    public class BloodScythe : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Particles/VerticalSmearRagged";

        private Vector2 InitialVelocity = Vector2.Zero;
        private Vector2 AcceleratingVelocity = Vector2.Zero;
        private static float RotationSpeed => MathHelper.Pi / 8f;
        private static float Acceleration => 0.175f;
        private static int Lifetime => 240;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.damage = 10;
            Projectile.scale = 0.1f;
            Projectile.hostile = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            InitialVelocity = Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation();
            for (int i = 0; i < 3; i++)
            {
                BloodParticle p = new(Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
                GeneralParticleHandler.SpawnParticle(p);
            }
            BloodParticle2 p2 = new(Projectile.Center, Projectile.velocity * 0.75f, 16, 0.5f, Color.Red);
            GeneralParticleHandler.SpawnParticle(p2);
        }

        public override void AI()
        {
            int UpTime = Lifetime - Projectile.timeLeft;
            InitialVelocity *= 0.925f;
            if (UpTime > 15)
            {
                AcceleratingVelocity += Projectile.velocity.SafeNormalize(InitialVelocity.SafeNormalize(Vector2.UnitX)) * Acceleration;
                if (Main.rand.NextBool(1 + Projectile.timeLeft / 32))
                {
                    BloodParticle p = new(Projectile.Center + Main.rand.NextVector2CircularEdge(32, 32), (-Projectile.velocity).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.25f, 0.75f), Main.rand.Next(10, 17), 1f, Color.Red);
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }
            Projectile.velocity = InitialVelocity + AcceleratingVelocity;
            Projectile.rotation += RotationSpeed;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WritePackedVector2(InitialVelocity);
            writer.WritePackedVector2(AcceleratingVelocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            InitialVelocity = reader.ReadPackedVector2();
            AcceleratingVelocity = reader.ReadPackedVector2();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = Color.Red;
            if (!ChildSafety.Disabled)
                drawColor = Main.DiscoColor;

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                {
                    float afterimageRot = Projectile.oldRot[i];
                    drawPos = Projectile.oldPos[i] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    if (i != 0)
                        drawColor *= 0.9f;

                    // DO NOT REMOVE THESE "UNNECESSARY" FLOAT CASTS. THIS WILL BREAK THE AFTERIMAGES.
                    float interpolant = ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                    Main.spriteBatch.Draw(tex, drawPos, null, drawColor, afterimageRot, tex.Size() * 0.5f, Projectile.scale * interpolant, SpriteEffects.None, 0f);
                }
            }
            //else
            //    Main.EntitySpriteDraw(tex, drawPos, tex.Frame(), drawColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);

            return false;
        }
    }

    public class CirclingBloodScythe : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Particles/VerticalSmearRagged";

        private static float RotationSpeed => MathHelper.Pi / 8f;
        private static int Lifetime => BrainOfCthulhuAI.CrimsonEyeAttackDuration;
        private static float MaxCircleSpeed => MathHelper.Pi / 30f; //1 rev per second

        ref float CircleAngle => ref Projectile.ai[0];
        ref float CircleRadius => ref Projectile.ai[1];
        ref float CircleRadiusVelocity => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.damage = 10;
            Projectile.scale = 0.1f;
            Projectile.hostile = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = CircleAngle;
            for (int i = 0; i < 3; i++)
            {
                BloodParticle p = new(Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
                GeneralParticleHandler.SpawnParticle(p);
            }
            BloodParticle2 p2 = new(Projectile.Center, Projectile.velocity * 0.75f, 16, 0.5f, Color.Red);
            GeneralParticleHandler.SpawnParticle(p2);
        }

        public override void AI()
        {
            if(NPC.crimsonBoss == -1)
            {
                Projectile.active = false;
                return;
            }
            int UpTime = Lifetime - Projectile.timeLeft;

            if (UpTime < 30f)
                CircleRadius = MathHelper.Lerp(0f, 128f, CalamityUtils.CircOutEasing(UpTime / 30f, 1));
            else if(UpTime < Lifetime - 180)
                CircleRadius = 128f;
            else
            {
                if (UpTime == Lifetime - 180)
                    CircleRadiusVelocity = -3f;
                CircleRadius += CircleRadiusVelocity;
                CircleRadiusVelocity += 0.1f;
            }

            if (UpTime >= 15f)
            {
                if (UpTime < 30f)
                    CircleAngle += MathHelper.Lerp(0f, MaxCircleSpeed, CalamityUtils.SineInEasing((UpTime - 15) / 15f, 1));
                else if (UpTime < 870)
                    CircleAngle += MaxCircleSpeed;
                else if (UpTime < 900)
                    CircleAngle += MathHelper.Lerp(MaxCircleSpeed, MaxCircleSpeed / 3f, CalamityUtils.SineOutEasing((UpTime - 870) / 30f, 1));
                else
                    CircleAngle += MaxCircleSpeed / 3f;

                if (Main.rand.NextBool(6))
                {
                    BloodParticle p = new(Projectile.Center + Main.rand.NextVector2CircularEdge(32, 32), (Projectile.DirectionTo(Main.npc[NPC.crimsonBoss].Center).RotatedBy(MathHelper.PiOver2) * 16f).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.25f, 0.75f), Main.rand.Next(10, 17), 1f, Color.Red);
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }

            NPC boss = Main.npc[NPC.crimsonBoss];

            Projectile.Center = boss.Center + (CircleAngle.ToRotationVector2() * CircleRadius);
            Projectile.rotation += RotationSpeed;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = Color.Red;
            if (!ChildSafety.Disabled)
                drawColor = Main.DiscoColor;

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                {
                    float afterimageRot = Projectile.oldRot[i];
                    drawPos = Projectile.oldPos[i] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    if (i != 0)
                        drawColor *= 0.9f;
                    // DO NOT REMOVE THESE "UNNECESSARY" FLOAT CASTS. THIS WILL BREAK THE AFTERIMAGES.
                    float interpolant = ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                    Main.spriteBatch.Draw(tex, drawPos, null, drawColor, afterimageRot, tex.Size() * 0.5f, Projectile.scale * interpolant, SpriteEffects.None, 0f);
                }
            }
            else
                Main.EntitySpriteDraw(tex, drawPos, tex.Frame(), drawColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);

            return false;
        }
    }

    public class IchorShower : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 480;
            Projectile.damage = 10;
            Projectile.scale = 1;
            Projectile.hostile = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            for(int i = 0; i < 3; i++)
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.Ichor, Vector2.Zero).noGravity = true;
            if(Main.rand.NextBool(8))
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.GoldFlame, Vector2.UnitX * Projectile.velocity / 10f, Scale: 0.75f);
            Projectile.velocity.Y += 0.075f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Ichor, 600);
        }
    }

    public class CrimsonEye : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/CrimsonEye";

        private ref float Time => ref Projectile.ai[0];
        private ref float DistanceRatio => ref Projectile.ai[1];
        private ref float ExplosionTime => ref Projectile.ai[2];

        private static float TimeToExplode => 30f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 36;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.scale = 1;
            Projectile.hostile = false;
            Projectile.hide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 32; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Crimstone);
        }

        public override void AI()
        {
            if (Time > 45)
            {
                Player closest = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
                DistanceRatio = 1 - MathHelper.Clamp((Projectile.Center.Distance(closest.Center) - 128) / 120f, 0f, 1f);

                if (DistanceRatio >= 1 && Projectile.frame == 4)
                {
                    if (Projectile.timeLeft < 120)
                        Projectile.timeLeft = 120;

                    if (ExplosionTime == TimeToExplode)
                    {
                        Projectile.hostile = true;
                        DetailedExplosion explosion = new(Projectile.Center, Vector2.Zero, Color.Orange, Vector2.One, Main.rand.NextFloatDirection(), 0.3f, 0.9f, 24);
                        GeneralParticleHandler.SpawnParticle(explosion);

                        Projectile.Resize(256, 256);
                    }
                    else if (Projectile.width == 256)
                        Projectile.active = false;

                    ExplosionTime++;
                }
                else
                {
                    if (ExplosionTime > 0)
                        ExplosionTime--;

                    if (Projectile.width == 256)
                        Projectile.active = false;
                }

                if (Time > 65)
                {
                    if (Projectile.timeLeft <= 20)
                        Projectile.frame = (Projectile.timeLeft - 5) / 5;
                    else
                        Projectile.frame = 4;
                }
                else if (Time % 5 == 0)
                    Projectile.frame++;
            }
            Time++;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ExplosionTime >= TimeToExplode)
                return false;

            if (ExplosionTime > 0)
            {
                float ratio = ExplosionTime / TimeToExplode;

                Vector3 lightLevels = lightColor.ToVector3();
                float lightLevel = (lightLevels.X + lightLevels.Y + lightLevels.Z) / 3f;
                lightColor = Color.Lerp(lightColor, Color.Lerp(Color.Red, Color.Gold, (float)Math.Sin(ExplosionTime / 4f) / 2f + 0.5f) * lightLevel, ratio);
                lightColor.A = 255;
            }

            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            if (ExplosionTime >= TimeToExplode)
                return;

            float ratio = CalamityUtils.SineOutEasing(ExplosionTime / TimeToExplode, 1);

            if (Projectile.frame == 4)
            {
                Texture2D tex = ModContent.Request<Texture2D>(Texture + "Pupil").Value;
                Vector2 rangeScale = new Vector2(2.25f, 0.75f);
                Vector2 dir = Projectile.DirectionTo(Main.LocalPlayer.Center);
                if(ExplosionTime > 0)
                {
                    if (ratio < 0.25f)
                        dir = Vector2.Lerp(dir, Vector2.Zero, ratio * 4f);
                    else
                        dir = Main.rand.NextVector2CircularEdge(1, 1);
                }

                if (Projectile.timeLeft <= 50)
                    dir *= (Projectile.timeLeft - 20) / 30f;

                float range = 6f;
                if (ratio > 0.25f)
                {
                    range = MathHelper.Lerp(0f, 6f, (ratio - 0.25f) * 0.75f);
                }
                else if (Time < 85)
                {
                    float lerp = CalamityUtils.SineOutEasing((Time - 65) / 20f, 1);
                    range = MathHelper.Lerp(0f, range, lerp);
                }

                Main.EntitySpriteDraw(tex, Projectile.Center + (dir * rangeScale * range) - Main.screenPosition, null, lightColor, 0, tex.Size() * 0.5f, 1f, 0);
            }

            if (DistanceRatio > 0)
            {
                /*
                Main.spriteBatch.SetBlendState(BlendState.Additive);

                Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/HollowCircleHardEdge").Value;
                Color color = Color.Red * DistanceRatio;
                if (Time <= 65)
                    color *= (Time - 45) / 20f;

                if (Projectile.timeLeft <= 20)
                    color *= Projectile.timeLeft / 20f;

                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, 0f, tex.Size() * 0.5f, 1.75f, 0);

                Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
                */

                float opacity = DistanceRatio;
                if (Time <= 65)
                    opacity *= (Time - 45) / 20f;

                if (Projectile.timeLeft <= 20)
                    opacity *= Projectile.timeLeft / 20f;

                Main.spriteBatch.EnterShaderRegion();
                Texture2D telegraphBase = ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

                GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseOpacity(opacity);
                GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseColor(Color.Lerp(Color.Red, Color.OrangeRed, 0.7f * (float)Math.Pow(0.5 + 0.5 * Math.Sin(Main.GlobalTimeWrappedHourly), 3)));
                GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSecondaryColor(Color.Lerp(Color.Yellow, Color.White, 0.5f));
                GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSaturation(ratio * 0.5f + 0.5f);

                GameShaders.Misc["CalamityMod:CircularAoETelegraph"].Apply();

                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                Main.EntitySpriteDraw(telegraphBase, drawPosition, null, lightColor, 0, telegraphBase.Size() / 2f, 248f, 0, 0);
                Main.spriteBatch.ExitShaderRegion();
            }
        }
    }

    public class TelekineticEnemyGrab : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Particles/BloomRing";

        BezierCurve curve;
        int throwSign = 0;
        Vector2 throwPos = Vector2.Zero;
        Vector2 holdPos;

        ref float Time => ref Projectile.ai[0];
        ref float StunTime => ref Projectile.ai[1];

        private static Dictionary<int, Texture2D> EnemyGlowTextures = [];
        int enemyID { get => (int)Projectile.ai[2]; set => Projectile.ai[2] = (float)value; }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1200;
            Projectile.damage = 10;
            Projectile.hostile = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            holdPos = new Vector2(Projectile.Center.X, Main.npc[NPC.crimsonBoss].Center.Y - 128) - Main.npc[NPC.crimsonBoss].Center;

            Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);

            StunTime = -1;

            enemyID = Main.rand.Next(0, 3);
            int[] enemyIDs = [NPCID.FaceMonster, NPCID.Crimera, NPCID.BloodCrawler];
            enemyID = enemyIDs[enemyID];

            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            if(NPC.crimsonBoss == -1)
            {
                Projectile.active = false;
                return;
            }

            if (Time <= 90)
                Projectile.hostile = false;
            else
                Projectile.hostile = true;

            bool throwing = Time > 180;
            float throwTime = Time - 180;
            if (throwing)
            {
                if (throwSign == 0)
                    throwSign = Math.Sign(Projectile.Center.X - Main.npc[NPC.crimsonBoss].Center.X) * -Math.Sign(Main.player[Main.npc[NPC.crimsonBoss].target].Center.Y - Main.npc[NPC.crimsonBoss].Center.Y);
            }
            bool thrown = throwTime > 90;

            if (!thrown)
            {
                Vector2 startPoint = Main.npc[NPC.crimsonBoss].Center;// Main.npc[NPC.crimsonBoss].Center;
                Vector2 endPoint = Projectile.Center;

                if (StunTime == -1)
                {
                    if (!throwing)
                    {
                        if (Time >= 150)
                            Projectile.velocity = ((holdPos + Main.npc[NPC.crimsonBoss].Center) - Projectile.Center) / 30f;
                        else if (Time != 0)
                        {

                            if (Time <= 90)
                            {
                                if (Time == 90)
                                    Projectile.velocity = Vector2.UnitY * -24f;
                                if (Time % 30 == 0)
                                    Projectile.velocity = Vector2.UnitY * -16f;
                                else
                                    Projectile.velocity *= 0.33f;
                            }
                            else
                                Projectile.velocity *= 0.966f;
                        }
                    }
                    else
                    {
                        if (throwTime <= 30)
                            throwPos = (Projectile.Center + Projectile.velocity) - (Main.npc[NPC.crimsonBoss].Center + Main.npc[NPC.crimsonBoss].velocity);
                        Vector2 target = Main.player[Main.npc[NPC.crimsonBoss].target].Center;
                        Vector2 throwDir = (target - (throwPos + (Main.npc[NPC.crimsonBoss].Center + Main.npc[NPC.crimsonBoss].velocity))).SafeNormalize(Vector2.UnitY);

                        if (throwTime >= 30 && throwTime <= 90)
                        {
                            if (throwTime <= 60f)
                            {
                                Projectile.Center = Vector2.Lerp(throwPos, throwPos - throwDir * 56f, CalamityUtils.SineInOutEasing((throwTime - 30) / 30f, 1)) + (Main.npc[NPC.crimsonBoss].Center + Main.npc[NPC.crimsonBoss].velocity);
                                Projectile.velocity = Vector2.Zero;
                            }
                            else
                            {
                                Projectile.velocity += throwDir * 0.9f;
                                if (throwTime == 90)
                                {
                                    float dist = Projectile.Center.Distance(target) / 2f;
                                    dist /= Projectile.velocity.Length();
                                    Projectile.velocity.Y -= dist * 0.6f;
                                    Projectile.tileCollide = true;
                                }
                            }
                        }
                    }
                }

                Vector2 direction = endPoint - startPoint;
                float distance = Vector2.Distance(startPoint, endPoint);

                float lerp = CalamityUtils.SineInOutEasing(MathHelper.Clamp((throwTime) / 60f, 0f, 1f), 1);
                float xMult = MathHelper.Lerp(-Math.Clamp(direction.X / 256f, -1, 1), throwSign, lerp);
                float yMult = Math.Clamp(direction.Y / 256f, -1, 1);
                float curveIntensity = Math.Clamp(distance / 420f, 0f, 0.66f) * xMult * yMult;
                //Main.NewText("Completion: " + lerp + ", Intensity: " + curveIntensity);
                Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);

                Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);

                curve = new BezierCurve(startPoint, controlPoint1, controlPoint2, endPoint);
            }
            else
            {
                Projectile.velocity.Y += 0.6f;
            }

            if (StunTime >= 0)
            {
                if (StunTime < 30f)
                    StunTime++;
                else
                {
                    Projectile.velocity.Y += 0.6f;
                    Projectile.tileCollide = true;
                }
            }
            else
            {
                if (Time > 90)
                    Projectile.rotation += (Projectile.velocity.X / 100f) - (Math.Sign(Main.npc[NPC.crimsonBoss].Center.X - Projectile.Center.X) * 0.025f);
                Time++;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(throwSign);

            writer.WritePackedVector2(throwPos);

            writer.WritePackedVector2(holdPos);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            throwSign = reader.ReadInt32();

            throwPos = reader.ReadPackedVector2();

            holdPos = reader.ReadPackedVector2();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.Pi) / 8f;
            Vector2 pos = Projectile.Center;

            switch (enemyID)
            {
                case NPCID.FaceMonster:
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)), 237);
                    
                    for (int i = 0; i < 24; i++)
                        Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(16, 32), DustID.Blood, Scale: Main.rand.NextFloat(1, 2));
                    
                    SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.25f }, pos);
                    break;
                case NPCID.Crimera:
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)), 223);
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)), 224);
                    
                    for (int i = 0; i < 24; i++)
                        Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(16, 32), DustID.Blood, Scale: Main.rand.NextFloat(1, 2));
                    
                    SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.25f }, pos);
                    break;
                case NPCID.BloodCrawler:
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)), 351);
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)), 352);
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)), 353);
                    
                    for (int i = 0; i < 24; i++)
                        Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(16, 32), DustID.Blood, Scale: Main.rand.NextFloat(1, 2));
                    
                    SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.25f }, pos);
                    break;
                default:
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)), 42);
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)), 43);
                    Gore.NewGore(Projectile.GetSource_Death(), pos - (Projectile.velocity / 2f), velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2)), 44);

                    SoundEngine.PlaySound(SoundID.NPCDeath2 with { Volume = 0.175f }, pos);
                    break;
            }

            return true;
        }

        bool evenRed = false;
        public override bool PreDraw(ref Color lightColor)
        {
            //Handles getting the glow textures
            if (EnemyGlowTextures.Count == 0 || !EnemyGlowTextures.ContainsKey(enemyID))
            {
                EnemyGlowTextures.Clear();
                int[] enemyIDs = [NPCID.FaceMonster, NPCID.Crimera, NPCID.BloodCrawler];
                foreach (int id in enemyIDs)
                {
                    var baseTex = TextureAssets.Npc[id];
                    var glow = new Texture2D(Main.graphics.GraphicsDevice, baseTex.Value.Width, baseTex.Value.Height);

                    var BaseArray = new Color[glow.Width * glow.Height];
                    var ColorArray = new Color[glow.Width * glow.Height];
                    baseTex.Value.GetData(BaseArray);
                    for (var i = 0; i < BaseArray.Length; i++)
                    {
                        if (BaseArray[i].A != 0)
                            ColorArray[i] = new Color(255, 255, 255);
                    }
                    glow.SetData(ColorArray);

                    EnemyGlowTextures.Add(id, glow);
                }
            }

            int type = enemyID;
            int frameCount;
            int wrapFrame = -1;
            int startFrame = 0;
            float mult = 0.75f;

            switch(type)
            {
                case NPCID.FaceMonster:
                    frameCount = 16;
                    startFrame = 2;
                    wrapFrame = 16;
                    break;
                case NPCID.Crimera:
                    frameCount = 2;
                    mult = 0.25f;
                    break;
                case NPCID.BloodCrawler:
                    frameCount = 5;
                    mult = 0.25f;
                    break;
                default:
                    frameCount = 15;
                    type = NPCID.Skeleton;
                    startFrame = 1;
                    break;
            }

            if (wrapFrame == -1)
                wrapFrame = frameCount;

            Texture2D tex = TextureAssets.Npc[type].Value;

            int currentFrame = ((int)((1200 - Projectile.timeLeft) * mult)) % (wrapFrame - startFrame);

            Rectangle frame = tex.Frame(1, frameCount, 0, startFrame + currentFrame);

            float throwTime = Time - 180;

            float opacity = 1f;
            if (Time < 10)
                opacity = Time / 10f;

            if (throwTime <= 90)
            {
                int pCount = 12;

                float wrapTime = 60;
                float wrappedTime = MathHelper.Clamp((Time % (wrapTime + 1)) / wrapTime, 0f, 1f);
                if (wrappedTime == 0)
                    evenRed = !evenRed;

                float glowOpacity = MathHelper.Clamp(1 - ((throwTime - 75) / 15f), 0f, 1f);
                if (StunTime >= 0)
                    glowOpacity = 1 - (StunTime / 30f);
                if (Time < 15)
                    glowOpacity = Time / 15f;

                Main.spriteBatch.End(out var snapshot);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D ring = TextureAssets.Projectile[Type].Value;

                List<Vector2> points = curve.GetPoints(pCount);
                for (int i = 1; i < pCount; i++)
                {
                    float scale1 = MathHelper.Lerp(0.1f, 1f, i / (float)(pCount - 1));
                    float scale2 = MathHelper.Lerp(0.1f, 1f, (i + 1) / (float)(pCount - 1));
                    float scale = MathHelper.Lerp(scale1, scale2, wrappedTime);

                    float rot1 = (points[i] - points[i - 1]).ToRotation();

                    float rot2;
                    if (i == pCount - 1)
                        rot2 = points[i].ToRotation();
                    else
                        rot2 = (points[i + 1] - points[i]).ToRotation();

                    float rot;
                    if (i == pCount - 1)
                        rot = rot1;
                    else
                        rot = rot1.AngleLerp(rot2, wrappedTime);

                    Vector2 pos;
                    if (i == pCount - 1)
                        pos = Vector2.Lerp(points[i], points[i] + rot.ToRotationVector2() * Vector2.Distance(points[^1], points[^2]), wrappedTime);
                    else
                        pos = Vector2.Lerp(points[i], points[i + 1], wrappedTime);

                    Color color;
                    if (evenRed)
                        color = (i % 2 == 0 ? Color.Red : Color.Magenta) * 0.666f;
                    else
                        color = (i % 2 == 0 ? Color.Magenta : Color.Red) * 0.666f;

                    if (i == pCount - 1)
                        color *= 1 - wrappedTime;
                    else if (i == 1)
                        color *= wrappedTime;

                    Main.spriteBatch.Draw(ring, pos + Projectile.velocity - Main.screenPosition, null, color * glowOpacity, rot, ring.Size() * 0.5f, new Vector2(0.5f, 1f) * scale, 0, 0);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(snapshot);

                Vector2[] offsets = [Vector2.UnitX * 2, Vector2.UnitX * -2, Vector2.UnitY * 2, Vector2.UnitY * -2];
                for(int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(EnemyGlowTextures[enemyID], Projectile.Center - Main.screenPosition + offsets[i].RotatedBy(Projectile.rotation), frame, Color.Lerp(Color.Red, Color.Magenta, ((float)Math.Sin((Main.GlobalTimeWrappedHourly * 5f)) / 2f + 0.5f)) * glowOpacity, Projectile.rotation, frame.Size() * 0.5f, 1f, 0);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor * opacity, Projectile.rotation, frame.Size() * 0.5f, 1f, 0);

            return false;
        }
    }

    public class TelekineticBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1;
            Projectile.damage = 0;
            Projectile.scale = 1;
            Projectile.hostile = true;
        }

        Player target => Main.player[(int)Projectile.ai[0]];
        float debuffMultiplier => Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 dir = target.Center - Projectile.Center;
                int lifeTime = 24;
                dir /= lifeTime / 2f * 5f;
                dir *= i;
                DirectionalPulseRing pulse = new(Projectile.Center, dir, i % 2 == 0 ? Color.Red : Color.Orange, new Vector2(0.5f, 1), dir.ToRotation(), 0f, i / 5f, lifeTime + 8);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            SoundEngine.PlaySound(SoundID.Zombie105, Projectile.Center); //LC Laugh
            target.AddBuff(BuffID.Darkness, (int)Math.Round(900 * debuffMultiplier));
            target.AddBuff(BuffID.Bleeding, (int)Math.Round(900 * debuffMultiplier));
            target.AddBuff(BuffID.Confused, (int)Math.Round(60 * debuffMultiplier));
            int timeToAdd = (int)Math.Round(600 * debuffMultiplier);
            int bbIndex = target.buffType.ToList().IndexOf(ModContent.BuffType<BurningBlood>());
            if (bbIndex != -1)
                timeToAdd += target.buffTime[bbIndex];
            target.AddBuff(ModContent.BuffType<BurningBlood>(), timeToAdd);

            target.Calamity().adrenaline = 0;
        }
    }
}
