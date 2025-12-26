using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.DataStructures;
using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu.BrainOfCthulhuAI;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;

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

                        if (WorldGen.SolidOrSlopedTile(goalLocation.ToTileCoordinates().X, goalLocation.ToTileCoordinates().Y))
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
                        if (Time == 0)
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
        if (NPC.life <= 0)
        {
            List<VerletSimulatedSegment> verletTendril = BrainOfCthulhuSystem.VerletTendrils[NPC.AIOverride<CreeperAI>().CreeperID];
            if (verletTendril is null)
                return;

            for (int i = 0; i < verletTendril.Count; i++)
            {
                Vector2 start = verletTendril[i].position;
                Vector2 end = i == verletTendril.Count - 1 ? NPC.Center : verletTendril[i + 1].position;

                float rotation = (end - start).ToRotation();

                float dist = Vector2.Distance(start, end);
                Vector2 scale = new(1f, dist / BrainOfCthulhuSystem.tendril.Height());

                BrokenTendril gore = new(start, Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * -Main.rand.NextFloat(2f, 4f), rotation, scale, 60);
                GeneralParticleHandler.SpawnParticle(gore);

            }

            BrainOfCthulhuSystem.VerletTendrils[NPC.AIOverride<CreeperAI>().CreeperID] = [];
        }
    }

    public override bool PreDraw(Mod mod, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
}

