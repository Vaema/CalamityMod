using System;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.DataStructures;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    [AutoloadBossHead]
    public class Foveanator : ModNPC
    {
        public static int phase1IconIndex;
        public static int phase2IconIndex;

        internal static void LoadHeadIcons()
        {
            string phase1IconPath = "CalamityMod/NPCs/NormalNPCs/Foveanator_Head_Boss";
            string phase2IconPath = "CalamityMod/NPCs/NormalNPCs/Foveanator_Phase2_Head_Boss";

            CalamityMod.Instance.AddBossHeadTexture(phase1IconPath, -1);
            phase1IconIndex = ModContent.GetModBossHeadSlot(phase1IconPath);

            CalamityMod.Instance.AddBossHeadTexture(phase2IconPath, -1);
            phase2IconIndex = ModContent.GetModBossHeadSlot(phase2IconPath);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.aiStyle = NPCAIStyleID.Retinazer;
            NPC.GetNPCDamage();
            NPC.DR_NERD(0.2f);

            NPC.width = 100;
            NPC.height = 100;
            if (Main.tenthAnniversaryWorld)
                NPC.scale *= 0.5f;
            if (Main.getGoodWorld)
                NPC.scale *= 0.8f;

            NPC.defense = 10;
            NPC.lifeMax = 24000;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.SpawnWithHigherTime(30);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = true;
            AnimationType = NPCID.Retinazer;
            Music = MusicID.Boss2;

            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC, true);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            for (int i = 0; i < 4; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void BossHeadSlot(ref int index) => index = NPC.ai[0] >= 3f ? phase2IconIndex : phase1IconIndex;

        public override void AI()
        {
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                CalamityUtils.CalamityTargeting(NPC, options);
            }

            float enrageScale = bossRush ? 0.5f : 0.3f;
            if (Main.IsItDay() || bossRush)
            {
                NPC.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 1f;
            }

            // Percent life remaining
            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            // Check for Retinazer
            bool retAlive = false;
            if (CalamityGlobalNPC.laserEye != -1)
                retAlive = Main.npc[CalamityGlobalNPC.laserEye].active;

            // Check for Spazmatism or Retinazer
            bool spazAlive = false;
            if (CalamityGlobalNPC.fireEye != -1)
                spazAlive = Main.npc[CalamityGlobalNPC.fireEye].active;

            // Explode if ret and spaz are dead
            if (!retAlive && !spazAlive)
            {
                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            float foveanatorHoverXDest = NPC.Center.X - Main.player[NPC.target].position.X - (Main.player[NPC.target].width / 2);
            float foveanatorHoverYDest = NPC.position.Y + NPC.height - 59f - Main.player[NPC.target].position.Y - (Main.player[NPC.target].height / 2);

            float foveanatorHoverRotation = (float)Math.Atan2(foveanatorHoverYDest, foveanatorHoverXDest) + MathHelper.PiOver2;
            if (foveanatorHoverRotation < 0f)
                foveanatorHoverRotation += MathHelper.TwoPi;
            else if (foveanatorHoverRotation > MathHelper.TwoPi)
                foveanatorHoverRotation -= MathHelper.TwoPi;

            float foveanatorRotationSpeed = 0.1f;
            if (NPC.rotation < foveanatorHoverRotation)
            {
                if ((foveanatorHoverRotation - NPC.rotation) > MathHelper.Pi)
                    NPC.rotation -= foveanatorRotationSpeed;
                else
                    NPC.rotation += foveanatorRotationSpeed;
            }
            else if (NPC.rotation > foveanatorHoverRotation)
            {
                if ((NPC.rotation - foveanatorHoverRotation) > MathHelper.Pi)
                    NPC.rotation += foveanatorRotationSpeed;
                else
                    NPC.rotation -= foveanatorRotationSpeed;
            }

            if (NPC.rotation > foveanatorHoverRotation - foveanatorRotationSpeed && NPC.rotation < foveanatorHoverRotation + foveanatorRotationSpeed)
                NPC.rotation = foveanatorHoverRotation;

            if (NPC.rotation < 0f)
                NPC.rotation += MathHelper.TwoPi;
            else if (NPC.rotation > MathHelper.TwoPi)
                NPC.rotation -= MathHelper.TwoPi;

            if (NPC.rotation > foveanatorHoverRotation - foveanatorRotationSpeed && NPC.rotation < foveanatorHoverRotation + foveanatorRotationSpeed)
                NPC.rotation = foveanatorHoverRotation;

            if (Main.rand.NextBool(5))
            {
                int foveaDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height * 0.25f), NPC.width, (int)(NPC.height * 0.5f), DustID.Blood, NPC.velocity.X, 2f, 0, default, 1f);
                Dust dust = Main.dust[foveaDust];
                dust.velocity.X *= 0.5f;
                dust.velocity.Y *= 0.1f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && !Main.player[NPC.target].dead && NPC.timeLeft < 10)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (i != NPC.whoAmI && Main.npc[i].active && (Main.npc[i].type == NPCID.Retinazer || Main.npc[i].type == NPCID.Spazmatism || Main.npc[i].type == NPC.type) && Main.npc[i].timeLeft - 1 > NPC.timeLeft)
                        NPC.timeLeft = Main.npc[i].timeLeft - 1;
                }
            }

            // Phase HP ratios
            float phase2LifeRatio = 0.85f;
            float finalPhaseLifeRatio = 0.4f;

            // Movement variables
            float phase1MaxSpeedIncrease = 2f;
            float phase1MaxAccelerationIncrease = 0.025f;
            float phase1MaxChargeSpeedIncrease = 3f;

            // Phase duration variables
            float phase1MaxLaserPhaseDurationDecrease = 120f;

            // Go to phase 2 early if Spaz and Ret health total goes below 50%
            float retAndSpazHPRatio = 0f;
            if (CalamityGlobalNPC.fireEye != -1)
            {
                if (Main.npc[CalamityGlobalNPC.fireEye].active)
                    retAndSpazHPRatio += Main.npc[CalamityGlobalNPC.fireEye].life / (float)Main.npc[CalamityGlobalNPC.fireEye].lifeMax;
            }
            if (CalamityGlobalNPC.laserEye != -1)
            {
                if (Main.npc[CalamityGlobalNPC.laserEye].active)
                    retAndSpazHPRatio += Main.npc[CalamityGlobalNPC.laserEye].life / (float)Main.npc[CalamityGlobalNPC.laserEye].lifeMax;
            }

            // Phase checks
            bool phase2 = lifeRatio < phase2LifeRatio || retAndSpazHPRatio < 0.5f;
            bool finalPhase = lifeRatio < finalPhaseLifeRatio;

            Vector2 mechQueenSpacing = Vector2.Zero;
            if (NPC.IsMechQueenUp)
            {
                NPC NPC = Main.npc[NPC.mechQueen];
                Vector2 mechQueenCenter = NPC.GetMechQueenCenter();
                Vector2 eyePosition = new Vector2(-150f, -250f);
                eyePosition *= 0.75f;
                float mechdusaRotation = NPC.velocity.X * 0.025f;
                mechQueenSpacing = mechQueenCenter + eyePosition;
                mechQueenSpacing = mechQueenSpacing.RotatedBy(mechdusaRotation, mechQueenCenter);
            }

            NPC.reflectsProjectiles = false;

            // Despawn
            if (Main.player[NPC.target].dead)
            {
                NPC.velocity.Y -= 0.04f;
                if (NPC.timeLeft > 10)
                {
                    NPC.timeLeft = 10;
                    return;
                }
            }

            else if (NPC.ai[0] == 0f)
            {
                if (NPC.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    float foveanatorPhase1MaxSpeed = 8.25f;
                    float foveanatorPhase1Acceleration = 0.115f;
                    foveanatorPhase1MaxSpeed += 4f * enrageScale;
                    foveanatorPhase1Acceleration += 0.05f * enrageScale;

                    if (death)
                    {
                        foveanatorPhase1MaxSpeed += phase1MaxSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                        foveanatorPhase1Acceleration += phase1MaxAccelerationIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    }

                    if (Main.getGoodWorld)
                    {
                        foveanatorPhase1MaxSpeed *= 1.15f;
                        foveanatorPhase1Acceleration *= 1.15f;
                    }

                    int foveanatorFaceDirection = 1;
                    if (NPC.Center.X < Main.player[NPC.target].position.X + Main.player[NPC.target].width)
                        foveanatorFaceDirection = -1;

                    Vector2 foveanatorPosition = NPC.Center;
                    float distanceFromTarget = 300f;
                    float foveanatorTargetX = Main.player[NPC.target].Center.X + (foveanatorFaceDirection * distanceFromTarget) - foveanatorPosition.X;
                    float foveanatorTargetY = Main.player[NPC.target].Center.Y - distanceFromTarget - foveanatorPosition.Y;

                    if (NPC.IsMechQueenUp)
                    {
                        foveanatorPhase1MaxSpeed = 14f;
                        foveanatorTargetX = mechQueenSpacing.X;
                        foveanatorTargetY = mechQueenSpacing.Y;
                        foveanatorTargetX -= foveanatorPosition.X;
                        foveanatorTargetY -= foveanatorPosition.Y;
                    }

                    float foveanatorTargetDist = (float)Math.Sqrt(foveanatorTargetX * foveanatorTargetX + foveanatorTargetY * foveanatorTargetY);
                    float foveanatorTargetDistCopy = foveanatorTargetDist;

                    if (NPC.IsMechQueenUp)
                    {
                        if (foveanatorTargetDist > foveanatorPhase1MaxSpeed)
                        {
                            foveanatorTargetDist = foveanatorPhase1MaxSpeed / foveanatorTargetDist;
                            foveanatorTargetX *= foveanatorTargetDist;
                            foveanatorTargetY *= foveanatorTargetDist;
                        }

                        NPC.velocity.X = (NPC.velocity.X * 59f + foveanatorTargetX) / 60f;
                        NPC.velocity.Y = (NPC.velocity.Y * 59f + foveanatorTargetY) / 60f;
                    }
                    else
                    {
                        foveanatorTargetDist = foveanatorPhase1MaxSpeed / foveanatorTargetDist;
                        foveanatorTargetX *= foveanatorTargetDist;
                        foveanatorTargetY *= foveanatorTargetDist;

                        if (NPC.velocity.X < foveanatorTargetX)
                        {
                            NPC.velocity.X += foveanatorPhase1Acceleration;
                            if (NPC.velocity.X < 0f && foveanatorTargetX > 0f)
                                NPC.velocity.X += foveanatorPhase1Acceleration;
                        }
                        else if (NPC.velocity.X > foveanatorTargetX)
                        {
                            NPC.velocity.X -= foveanatorPhase1Acceleration;
                            if (NPC.velocity.X > 0f && foveanatorTargetX < 0f)
                                NPC.velocity.X -= foveanatorPhase1Acceleration;
                        }
                        if (NPC.velocity.Y < foveanatorTargetY)
                        {
                            NPC.velocity.Y += foveanatorPhase1Acceleration;
                            if (NPC.velocity.Y < 0f && foveanatorTargetY > 0f)
                                NPC.velocity.Y += foveanatorPhase1Acceleration;
                        }
                        else if (NPC.velocity.Y > foveanatorTargetY)
                        {
                            NPC.velocity.Y -= foveanatorPhase1Acceleration;
                            if (NPC.velocity.Y > 0f && foveanatorTargetY < 0f)
                                NPC.velocity.Y -= foveanatorPhase1Acceleration;
                        }
                    }

                    NPC.ai[2] += 1f;
                    float phaseGateValue = 300f - (death ? phase1MaxLaserPhaseDurationDecrease * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    float laserGateValue = 30f;
                    if (NPC.IsMechQueenUp)
                    {
                        phaseGateValue = 900f;
                        laserGateValue = ((!NPC.npcsFoundForCheckActive[NPCID.TheDestroyerBody]) ? 60f : 90f);
                    }
                    if (NPC.ai[2] >= phaseGateValue)
                    {
                        NPC.ai[1] = 1f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;

                        CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                        options.aggroRatio = -1f;
                        CalamityUtils.CalamityTargeting(NPC, options);

                        NPC.netUpdate = true;
                    }

                    else if (foveanatorTargetDistCopy < (death ? 960f : 800f))
                    {
                        if (!Main.player[NPC.target].dead)
                        {
                            NPC.ai[3] += 1f;
                            if (Main.getGoodWorld)
                                NPC.ai[3] += 0.5f;
                        }

                        if (NPC.ai[3] >= laserGateValue)
                        {
                            NPC.ai[3] = 0f;
                            foveanatorPosition = NPC.Center;
                            foveanatorTargetX = Main.player[NPC.target].Center.X - foveanatorPosition.X;
                            foveanatorTargetY = Main.player[NPC.target].Center.Y - foveanatorPosition.Y;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float foveanatorSpeed = 10.5f;
                                foveanatorSpeed += 3f * enrageScale;
                                int type = ProjectileID.EyeLaser;
                                int damage = NPC.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                foveanatorTargetDist = (float)Math.Sqrt(foveanatorTargetX * foveanatorTargetX + foveanatorTargetY * foveanatorTargetY);
                                foveanatorTargetDist = foveanatorSpeed / foveanatorTargetDist;
                                foveanatorTargetX *= foveanatorTargetDist;
                                foveanatorTargetY *= foveanatorTargetDist;

                                Vector2 laserVelocity = new Vector2(foveanatorTargetX, foveanatorTargetY);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), foveanatorPosition + laserVelocity.SafeNormalize(Vector2.UnitY) * 150f, laserVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                else if (NPC.ai[1] == 1f)
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    NPC.rotation = foveanatorHoverRotation;
                    float foveanatorChargeSpeed = 15f;
                    foveanatorChargeSpeed += 10f * enrageScale;
                    if (death)
                        foveanatorChargeSpeed += phase1MaxChargeSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    if (Main.getGoodWorld)
                        foveanatorChargeSpeed += 2f;

                    Vector2 foveanatorChargePos = NPC.Center;
                    float foveanatorChargeTargetX = Main.player[NPC.target].Center.X - foveanatorChargePos.X;
                    float foveanatorChargeTargetY = Main.player[NPC.target].Center.Y - foveanatorChargePos.Y;
                    float foveanatorChargeTargetDist = (float)Math.Sqrt(foveanatorChargeTargetX * foveanatorChargeTargetX + foveanatorChargeTargetY * foveanatorChargeTargetY);
                    foveanatorChargeTargetDist = foveanatorChargeSpeed / foveanatorChargeTargetDist;
                    NPC.velocity.X = foveanatorChargeTargetX * foveanatorChargeTargetDist;
                    NPC.velocity.Y = foveanatorChargeTargetY * foveanatorChargeTargetDist;
                    NPC.ai[1] = 2f;
                }
                else if (NPC.ai[1] == 2f)
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    NPC.ai[2] += 1f;
                    float decelerateGateValue = 36f + (death ? 6f * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (NPC.ai[2] >= decelerateGateValue)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        float decelerationMultiplier = 0.84f - (death ? 0.16f * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                        NPC.velocity *= decelerationMultiplier;
                        if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1)
                            NPC.velocity.X = 0f;
                        if (NPC.velocity.Y > -0.1 && NPC.velocity.Y < 0.1)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) - MathHelper.PiOver2;

                    float delayBeforeChargingAgain = 48f - (death ? 3f * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (NPC.ai[2] >= delayBeforeChargingAgain)
                    {
                        NPC.ai[3] += 1f;
                        NPC.ai[2] = 0f;
                        NPC.rotation = foveanatorHoverRotation;
                        float totalCharges = death ? 6f : 5f;
                        if (NPC.ai[3] >= totalCharges)
                        {
                            NPC.ai[1] = 0f;
                            NPC.ai[3] = 0f;

                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            CalamityUtils.CalamityTargeting(NPC, options);
                        }
                        else
                            NPC.ai[1] = 1f;
                    }
                }

                // Enter phase 2
                if (phase2)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;

                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.aggroRatio = -1f;
                    CalamityUtils.CalamityTargeting(NPC, options);

                    NPC.netUpdate = true;
                }
            }

            else if (NPC.ai[0] == 1f || NPC.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                if (NPC.IsMechQueenUp)
                    NPC.reflectsProjectiles = true;

                if (NPC.ai[0] == 1f)
                {
                    NPC.ai[2] += 0.005f;
                    if (NPC.ai[2] > 0.5)
                        NPC.ai[2] = 0.5f;
                }
                else
                {
                    NPC.ai[2] -= 0.005f;
                    if (NPC.ai[2] < 0f)
                        NPC.ai[2] = 0f;
                }

                NPC.rotation += NPC.ai[2];

                NPC.ai[1] += 1f;
                if (NPC.ai[2] >= 0.2f)
                {
                    if (NPC.ai[1] % 10f == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item33, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            bool shootLaser = NPC.ai[1] % 20f == 0f;
                            int type = shootLaser ? ProjectileID.DeathLaser : ModContent.ProjectileType<ScavengerLaser>();
                            int damage = NPC.GetProjectileDamage(type);

                            // Reduce mech boss projectile damage depending on the new ore progression changes
                            if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                            {
                                double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                if (!NPC.downedMechBossAny)
                                    damage = (int)(damage * firstMechMultiplier);
                                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                    damage = (int)(damage * secondMechMultiplier);
                            }

                            Vector2 projectileVelocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7f;
                            int numProj = shootLaser ? 6 : 2;
                            int spread = shootLaser ? 20 : 80;
                            float rotation = MathHelper.ToRadians(spread);
                            float offset = shootLaser ? 150f : 50f;
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * offset, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                if (NPC.ai[1] == 100f)
                {
                    NPC.ai[0] += 1f;
                    NPC.ai[1] = 0f;
                    if (NPC.ai[0] == 3f)
                    {
                        NPC.ai[2] = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);

                        if (Main.netMode != NetmodeID.Server)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 143, 1f);
                                Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 7, 1f);
                                Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 6, 1f);
                            }
                        }

                        for (int j = 0; j < 20; j++)
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                        SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    }
                }

                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                NPC.velocity *= 0.98f;
                if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1)
                    NPC.velocity.X = 0f;
                if (NPC.velocity.Y > -0.1 && NPC.velocity.Y < 0.1)
                    NPC.velocity.Y = 0f;
            }
            else
            {
                // If in phase 2 but Spaz or Ret aren't
                bool spazOrRetInPhase1 = false;
                if (CalamityGlobalNPC.fireEye != -1)
                {
                    if (Main.npc[CalamityGlobalNPC.fireEye].active)
                        spazOrRetInPhase1 = Main.npc[CalamityGlobalNPC.fireEye].ai[0] == 1f || Main.npc[CalamityGlobalNPC.fireEye].ai[0] == 2f || Main.npc[CalamityGlobalNPC.fireEye].ai[0] == 0f;
                }
                if (CalamityGlobalNPC.laserEye != -1)
                {
                    if (Main.npc[CalamityGlobalNPC.laserEye].active)
                        spazOrRetInPhase1 = Main.npc[CalamityGlobalNPC.laserEye].ai[0] == 1f || Main.npc[CalamityGlobalNPC.laserEye].ai[0] == 2f || Main.npc[CalamityGlobalNPC.laserEye].ai[0] == 0f;
                }

                NPC.chaseable = !spazOrRetInPhase1;

                int setDamage = (int)Math.Round(NPC.defDamage * 1.5);
                NPC.defense = NPC.defDefense + 10;
                calamityGlobalNPC.DR = spazOrRetInPhase1 ? 0.9999f : 0.2f;
                calamityGlobalNPC.unbreakableDR = spazOrRetInPhase1;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = spazOrRetInPhase1;

                NPC.HitSound = SoundID.NPCHit4;

                if (NPC.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    float foveanatorPhase2MaxSpeed = 9.5f + (death ? 3f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    float foveanatorPhase2Accel = 0.175f + (death ? 0.05f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    foveanatorPhase2MaxSpeed += 4.5f * enrageScale;
                    foveanatorPhase2Accel += 0.075f * enrageScale;

                    if (Main.getGoodWorld)
                    {
                        foveanatorPhase2MaxSpeed *= 1.15f;
                        foveanatorPhase2Accel *= 1.15f;
                    }

                    Vector2 eyePosition = NPC.Center;
                    float foveanatorPhase2TargetX = Main.player[NPC.target].Center.X - eyePosition.X;
                    float distanceFromTarget = 420f;
                    float foveanatorPhase2TargetY = Main.player[NPC.target].Center.Y - distanceFromTarget - eyePosition.Y;

                    if (NPC.IsMechQueenUp)
                    {
                        foveanatorPhase2MaxSpeed = 14f;
                        foveanatorPhase2TargetX = mechQueenSpacing.X;
                        foveanatorPhase2TargetY = mechQueenSpacing.Y;
                        foveanatorPhase2TargetX -= eyePosition.X;
                        foveanatorPhase2TargetY -= eyePosition.Y;
                    }

                    float foveanatorPhase2TargetDist = (float)Math.Sqrt(foveanatorPhase2TargetX * foveanatorPhase2TargetX + foveanatorPhase2TargetY * foveanatorPhase2TargetY);

                    if (NPC.IsMechQueenUp)
                    {
                        if (foveanatorPhase2TargetDist > foveanatorPhase2MaxSpeed)
                        {
                            foveanatorPhase2TargetDist = foveanatorPhase2MaxSpeed / foveanatorPhase2TargetDist;
                            foveanatorPhase2TargetX *= foveanatorPhase2TargetDist;
                            foveanatorPhase2TargetY *= foveanatorPhase2TargetDist;
                        }

                        NPC.velocity.X = (NPC.velocity.X * 4f + foveanatorPhase2TargetX) / 5f;
                        NPC.velocity.Y = (NPC.velocity.Y * 4f + foveanatorPhase2TargetY) / 5f;
                    }
                    else
                    {
                        foveanatorPhase2TargetDist = foveanatorPhase2MaxSpeed / foveanatorPhase2TargetDist;
                        foveanatorPhase2TargetX *= foveanatorPhase2TargetDist;
                        foveanatorPhase2TargetY *= foveanatorPhase2TargetDist;

                        if (NPC.velocity.X < foveanatorPhase2TargetX)
                        {
                            NPC.velocity.X += foveanatorPhase2Accel;
                            if (NPC.velocity.X < 0f && foveanatorPhase2TargetX > 0f)
                                NPC.velocity.X += foveanatorPhase2Accel;
                        }
                        else if (NPC.velocity.X > foveanatorPhase2TargetX)
                        {
                            NPC.velocity.X -= foveanatorPhase2Accel;
                            if (NPC.velocity.X > 0f && foveanatorPhase2TargetX < 0f)
                                NPC.velocity.X -= foveanatorPhase2Accel;
                        }
                        if (NPC.velocity.Y < foveanatorPhase2TargetY)
                        {
                            NPC.velocity.Y += foveanatorPhase2Accel;
                            if (NPC.velocity.Y < 0f && foveanatorPhase2TargetY > 0f)
                                NPC.velocity.Y += foveanatorPhase2Accel;
                        }
                        else if (NPC.velocity.Y > foveanatorPhase2TargetY)
                        {
                            NPC.velocity.Y -= foveanatorPhase2Accel;
                            if (NPC.velocity.Y > 0f && foveanatorPhase2TargetY < 0f)
                                NPC.velocity.Y -= foveanatorPhase2Accel;
                        }
                    }

                    NPC.ai[2] += (!spazAlive || !retAlive) ? 1.5f : 1f;
                    float phaseGateValue = NPC.IsMechQueenUp ? 900f : 300f - (death ? 120f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    if (NPC.ai[2] >= phaseGateValue)
                    {
                        NPC.ai[1] = 1f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;

                        CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                        options.aggroRatio = -1f;
                        CalamityUtils.CalamityTargeting(NPC, options);

                        NPC.netUpdate = true;
                    }

                    eyePosition = NPC.Center;
                    foveanatorPhase2TargetX = Main.player[NPC.target].Center.X - eyePosition.X;
                    foveanatorPhase2TargetY = Main.player[NPC.target].Center.Y - eyePosition.Y;
                    NPC.rotation = (float)Math.Atan2(foveanatorPhase2TargetY, foveanatorPhase2TargetX) - MathHelper.PiOver2;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.localAI[1] += 1f + (death ? (phase2LifeRatio - lifeRatio) / phase2LifeRatio : 0f);
                        if (NPC.localAI[1] >= ((spazAlive && retAlive) ? 52f : 26f))
                        {
                            bool canHit = Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height);
                            if (canHit || !spazAlive || !retAlive || finalPhase)
                            {
                                NPC.localAI[1] = 0f;
                                float foveanatorPhase2LaserSpeed = 10f;
                                foveanatorPhase2LaserSpeed += enrageScale;
                                int type = ProjectileID.DeathLaser;
                                int damage = NPC.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                foveanatorPhase2TargetDist = (float)Math.Sqrt(foveanatorPhase2TargetX * foveanatorPhase2TargetX + foveanatorPhase2TargetY * foveanatorPhase2TargetY);
                                foveanatorPhase2TargetDist = foveanatorPhase2LaserSpeed / foveanatorPhase2TargetDist;
                                foveanatorPhase2TargetX *= foveanatorPhase2TargetDist;
                                foveanatorPhase2TargetY *= foveanatorPhase2TargetDist;

                                Vector2 laserVelocity = new Vector2(foveanatorPhase2TargetX, foveanatorPhase2TargetY);
                                if (canHit)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), eyePosition + laserVelocity.SafeNormalize(Vector2.UnitY) * 150f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                }
                                else
                                {
                                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), eyePosition + laserVelocity.SafeNormalize(Vector2.UnitY) * 150f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                    Main.projectile[proj].tileCollide = false;
                                    Main.projectile[proj].timeLeft = 300;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (NPC.ai[1] == 1f)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        int foveanatorPhase2FaceDirection = 1;
                        if (NPC.Center.X < Main.player[NPC.target].position.X + Main.player[NPC.target].width)
                            foveanatorPhase2FaceDirection = -1;

                        float foveanatorPhase2RapidFireMaxSpeed = 9.5f + (death ? 3f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float foveanatorPhase2RapidFireAccel = 0.25f + (death ? 0.075f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        foveanatorPhase2RapidFireMaxSpeed += 4.5f * enrageScale;
                        foveanatorPhase2RapidFireAccel += 0.15f * enrageScale;

                        if (Main.getGoodWorld)
                        {
                            foveanatorPhase2RapidFireMaxSpeed *= 1.15f;
                            foveanatorPhase2RapidFireAccel *= 1.15f;
                        }

                        Vector2 foveanatorPhase2RapidFirePos = NPC.Center;
                        float distanceFromTarget = 420f;
                        float foveanatorPhase2RapidFireTargetX = Main.player[NPC.target].Center.X + (foveanatorPhase2FaceDirection * distanceFromTarget) - foveanatorPhase2RapidFirePos.X;
                        float foveanatorPhase2RapidFireTargetY = Main.player[NPC.target].Center.Y - foveanatorPhase2RapidFirePos.Y;
                        float foveanatorPhase2RapidFireTargetDist = (float)Math.Sqrt(foveanatorPhase2RapidFireTargetX * foveanatorPhase2RapidFireTargetX + foveanatorPhase2RapidFireTargetY * foveanatorPhase2RapidFireTargetY);
                        foveanatorPhase2RapidFireTargetDist = foveanatorPhase2RapidFireMaxSpeed / foveanatorPhase2RapidFireTargetDist;
                        foveanatorPhase2RapidFireTargetX *= foveanatorPhase2RapidFireTargetDist;
                        foveanatorPhase2RapidFireTargetY *= foveanatorPhase2RapidFireTargetDist;

                        if (NPC.velocity.X < foveanatorPhase2RapidFireTargetX)
                        {
                            NPC.velocity.X += foveanatorPhase2RapidFireAccel;
                            if (NPC.velocity.X < 0f && foveanatorPhase2RapidFireTargetX > 0f)
                                NPC.velocity.X += foveanatorPhase2RapidFireAccel;
                        }
                        else if (NPC.velocity.X > foveanatorPhase2RapidFireTargetX)
                        {
                            NPC.velocity.X -= foveanatorPhase2RapidFireAccel;
                            if (NPC.velocity.X > 0f && foveanatorPhase2RapidFireTargetX < 0f)
                                NPC.velocity.X -= foveanatorPhase2RapidFireAccel;
                        }
                        if (NPC.velocity.Y < foveanatorPhase2RapidFireTargetY)
                        {
                            NPC.velocity.Y += foveanatorPhase2RapidFireAccel;
                            if (NPC.velocity.Y < 0f && foveanatorPhase2RapidFireTargetY > 0f)
                                NPC.velocity.Y += foveanatorPhase2RapidFireAccel;
                        }
                        else if (NPC.velocity.Y > foveanatorPhase2RapidFireTargetY)
                        {
                            NPC.velocity.Y -= foveanatorPhase2RapidFireAccel;
                            if (NPC.velocity.Y > 0f && foveanatorPhase2RapidFireTargetY < 0f)
                                NPC.velocity.Y -= foveanatorPhase2RapidFireAccel;
                        }

                        foveanatorPhase2RapidFirePos = NPC.Center;
                        foveanatorPhase2RapidFireTargetX = Main.player[NPC.target].Center.X - foveanatorPhase2RapidFirePos.X;
                        foveanatorPhase2RapidFireTargetY = Main.player[NPC.target].Center.Y - foveanatorPhase2RapidFirePos.Y;
                        NPC.rotation = (float)Math.Atan2(foveanatorPhase2RapidFireTargetY, foveanatorPhase2RapidFireTargetX) - MathHelper.PiOver2;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.localAI[1] += 1f + (death ? (phase2LifeRatio - lifeRatio) / phase2LifeRatio : 0f);
                            if (NPC.localAI[1] > ((spazAlive && retAlive) ? 20f : 10f))
                            {
                                bool canHit = Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height);
                                if (canHit || !spazAlive || !retAlive || finalPhase)
                                {
                                    NPC.localAI[1] = 0f;
                                    int type = ProjectileID.DeathLaser;
                                    int damage = (int)Math.Round(NPC.GetProjectileDamage(type) * 0.75);

                                    // Reduce mech boss projectile damage depending on the new ore progression changes
                                    if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                    {
                                        double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                        double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                        if (!NPC.downedMechBossAny)
                                            damage = (int)(damage * firstMechMultiplier);
                                        else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                            damage = (int)(damage * secondMechMultiplier);
                                    }

                                    foveanatorPhase2RapidFireTargetDist = (float)Math.Sqrt(foveanatorPhase2RapidFireTargetX * foveanatorPhase2RapidFireTargetX + foveanatorPhase2RapidFireTargetY * foveanatorPhase2RapidFireTargetY);
                                    foveanatorPhase2RapidFireTargetDist = 9f / foveanatorPhase2RapidFireTargetDist;
                                    foveanatorPhase2RapidFireTargetX *= foveanatorPhase2RapidFireTargetDist;
                                    foveanatorPhase2RapidFireTargetY *= foveanatorPhase2RapidFireTargetDist;

                                    Vector2 laserVelocity = new Vector2(foveanatorPhase2RapidFireTargetX, foveanatorPhase2RapidFireTargetY);
                                    if (canHit)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), foveanatorPhase2RapidFirePos + laserVelocity.SafeNormalize(Vector2.UnitY) * 150f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                    }
                                    else
                                    {
                                        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), foveanatorPhase2RapidFirePos + laserVelocity.SafeNormalize(Vector2.UnitY) * 150f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                        Main.projectile[proj].tileCollide = false;
                                        Main.projectile[proj].timeLeft = 300;
                                    }
                                }
                            }
                        }

                        NPC.ai[2] += (spazAlive && retAlive) ? 1f : 1.5f;
                        if (NPC.ai[2] >= 150f - (death ? 60f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
                        {
                            NPC.ai[1] = (!spazAlive || !retAlive || finalPhase) ? 4f : 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = 0f;

                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            CalamityUtils.CalamityTargeting(NPC, options);

                            NPC.netUpdate = true;
                        }
                    }

                    // Charge
                    else if (NPC.ai[1] == 2f)
                    {
                        // Set damage
                        NPC.damage = setDamage;

                        // Set rotation and velocity
                        NPC.rotation = foveanatorHoverRotation;
                        float foveanatorPhase3ChargeSpeed = 22f + (death ? 8f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        foveanatorPhase3ChargeSpeed += 10f * enrageScale;

                        if (!spazAlive || !retAlive)
                            foveanatorPhase3ChargeSpeed += 2f;

                        if (Main.getGoodWorld)
                            foveanatorPhase3ChargeSpeed += 2f;

                        Vector2 foveanatorPhase3ChargePos = NPC.Center;
                        float foveanatorPhase3ChargeTargetX = Main.player[NPC.target].Center.X - foveanatorPhase3ChargePos.X;
                        float foveanatorPhase3ChargeTargetY = Main.player[NPC.target].Center.Y - foveanatorPhase3ChargePos.Y;
                        float foveanatorPhase3ChargeTargetDist = (float)Math.Sqrt(foveanatorPhase3ChargeTargetX * foveanatorPhase3ChargeTargetX + foveanatorPhase3ChargeTargetY * foveanatorPhase3ChargeTargetY);
                        foveanatorPhase3ChargeTargetDist = foveanatorPhase3ChargeSpeed / foveanatorPhase3ChargeTargetDist;
                        NPC.velocity.X = foveanatorPhase3ChargeTargetX * foveanatorPhase3ChargeTargetDist;
                        NPC.velocity.Y = foveanatorPhase3ChargeTargetY * foveanatorPhase3ChargeTargetDist;
                        NPC.ai[1] = 3f;
                    }

                    else if (NPC.ai[1] == 3f)
                    {
                        // Set damage
                        NPC.damage = setDamage;

                        NPC.ai[2] += 1f;

                        float chargeTime = (spazAlive && retAlive) ? 45f : 30f;
                        if (NPC.ai[3] % 3f == 0f)
                            chargeTime = (spazAlive && retAlive) ? 90f : 60f;
                        if (death)
                            chargeTime -= chargeTime * 0.25f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio);
                        chargeTime -= chargeTime / 5 * enrageScale;

                        // Slow down
                        if (NPC.ai[2] >= chargeTime)
                        {
                            // Avoid cheap bullshit
                            NPC.damage = 0;

                            NPC.velocity *= 0.93f;
                            if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1)
                                NPC.velocity.X = 0f;
                            if (NPC.velocity.Y > -0.1 && NPC.velocity.Y < 0.1)
                                NPC.velocity.Y = 0f;
                        }
                        else
                        {
                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) - MathHelper.PiOver2;

                            if (NPC.ai[3] % 3f == 0f)
                            {
                                float fireRate = (spazAlive && retAlive) ? 13f : 9f;

                                if (NPC.ai[2] % fireRate == 0f)
                                {
                                    Vector2 foveanatorPhase3ChargeLaserPos = NPC.Center;
                                    float foveanatorPhase3ChargeLaserTargetX = Main.player[NPC.target].Center.X - foveanatorPhase3ChargeLaserPos.X;
                                    float foveanatorPhase3ChargeLaserTargetY = Main.player[NPC.target].Center.Y - foveanatorPhase3ChargeLaserPos.Y;
                                    float foveanatorPhase3ChargeLaserTargetDist = (float)Math.Sqrt(foveanatorPhase3ChargeLaserTargetX * foveanatorPhase3ChargeLaserTargetX + foveanatorPhase3ChargeLaserTargetY * foveanatorPhase3ChargeLaserTargetY);

                                    SoundEngine.PlaySound(SoundID.Item33, NPC.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        int type = ModContent.ProjectileType<ScavengerLaser>();
                                        int damage = NPC.GetProjectileDamage(type);

                                        // Reduce mech boss projectile damage depending on the new ore progression changes
                                        if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                        {
                                            double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                            double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                            if (!NPC.downedMechBossAny)
                                                damage = (int)(damage * firstMechMultiplier);
                                            else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                                damage = (int)(damage * secondMechMultiplier);
                                        }

                                        float laserDartVelocity = (death ? 9f : 6f) * ((spazAlive && retAlive) ? 1f : 1.5f);
                                        foveanatorPhase3ChargeLaserPos = NPC.Center;
                                        foveanatorPhase3ChargeLaserTargetX = Main.player[NPC.target].Center.X - foveanatorPhase3ChargeLaserPos.X;
                                        foveanatorPhase3ChargeLaserTargetY = Main.player[NPC.target].Center.Y - foveanatorPhase3ChargeLaserPos.Y;
                                        foveanatorPhase3ChargeLaserTargetDist = (float)Math.Sqrt(foveanatorPhase3ChargeLaserTargetX * foveanatorPhase3ChargeLaserTargetX + foveanatorPhase3ChargeLaserTargetY * foveanatorPhase3ChargeLaserTargetY);
                                        foveanatorPhase3ChargeLaserTargetDist = laserDartVelocity / foveanatorPhase3ChargeLaserTargetDist;
                                        foveanatorPhase3ChargeLaserTargetX *= foveanatorPhase3ChargeLaserTargetDist;
                                        foveanatorPhase3ChargeLaserTargetY *= foveanatorPhase3ChargeLaserTargetDist;

                                        Vector2 laserVelocity = new Vector2(foveanatorPhase3ChargeLaserTargetX, foveanatorPhase3ChargeLaserTargetY);
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), foveanatorPhase3ChargeLaserPos + NPC.velocity.SafeNormalize(Vector2.UnitY) * 50f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }

                        // Charge four times
                        float chargeGateValue = 30f;
                        chargeGateValue -= chargeGateValue / 4 * enrageScale;
                        if (NPC.ai[2] >= chargeTime + chargeGateValue)
                        {
                            NPC.ai[2] = 0f;

                            float chargeIncrement = 1f;
                            if (Main.rand.NextBool() && NPC.ai[3] < ((spazAlive && retAlive) ? 1f : 3f))
                            {
                                chargeIncrement = 2f;

                                // Net update due to the randomness in Master Mode
                                NPC.netUpdate = true;
                            }

                            NPC.ai[3] += chargeIncrement;

                            NPC.rotation = foveanatorHoverRotation;
                            float maxChargeAmt = (spazAlive && retAlive) ? 2f : 4f;
                            if (NPC.ai[3] >= maxChargeAmt)
                            {
                                NPC.ai[1] = 0f;
                                NPC.ai[3] = 0f;

                                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                                options.aggroRatio = -1f;
                                CalamityUtils.CalamityTargeting(NPC, options);
                            }
                            else
                                NPC.ai[1] = 4f;
                        }
                    }

                    // Get in position for charge
                    else if (NPC.ai[1] == 4f)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        int chargeLineUpDist = (spazAlive && retAlive) ? 600 : 500;
                        float chargeSpeed = 18f + (death ? 6f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float chargeAccel = 0.45f + (death ? 0.15f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        chargeSpeed += 6f * enrageScale;
                        chargeAccel += 0.15f * enrageScale;

                        if (spazAlive && retAlive)
                        {
                            chargeSpeed *= 0.75f;
                            chargeAccel *= 0.75f;
                        }

                        if (Main.getGoodWorld)
                        {
                            chargeSpeed *= 1.15f;
                            chargeAccel *= 1.15f;
                        }

                        int foveanatorPhase2FaceDirection = 1;
                        if (NPC.Center.X < Main.player[NPC.target].position.X + Main.player[NPC.target].width)
                            foveanatorPhase2FaceDirection = -1;

                        Vector2 spazmatismRetDeadChargePos = NPC.Center;
                        float chargeTargetX = Main.player[NPC.target].Center.X + (chargeLineUpDist * foveanatorPhase2FaceDirection) - spazmatismRetDeadChargePos.X;
                        float chargeTargetY = Main.player[NPC.target].Center.Y - spazmatismRetDeadChargePos.Y;
                        float chargeTargetDist = (float)Math.Sqrt(chargeTargetX * chargeTargetX + chargeTargetY * chargeTargetY);

                        chargeTargetDist = chargeSpeed / chargeTargetDist;
                        chargeTargetX *= chargeTargetDist;
                        chargeTargetY *= chargeTargetDist;

                        if (NPC.velocity.X < chargeTargetX)
                        {
                            NPC.velocity.X += chargeAccel;
                            if (NPC.velocity.X < 0f && chargeTargetX > 0f)
                                NPC.velocity.X += chargeAccel;
                        }
                        else if (NPC.velocity.X > chargeTargetX)
                        {
                            NPC.velocity.X -= chargeAccel;
                            if (NPC.velocity.X > 0f && chargeTargetX < 0f)
                                NPC.velocity.X -= chargeAccel;
                        }
                        if (NPC.velocity.Y < chargeTargetY)
                        {
                            NPC.velocity.Y += chargeAccel;
                            if (NPC.velocity.Y < 0f && chargeTargetY > 0f)
                                NPC.velocity.Y += chargeAccel;
                        }
                        else if (NPC.velocity.Y > chargeTargetY)
                        {
                            NPC.velocity.Y -= chargeAccel;
                            if (NPC.velocity.Y > 0f && chargeTargetY < 0f)
                                NPC.velocity.Y -= chargeAccel;
                        }

                        // Take 1.25 or 1 second to get in position, then charge
                        NPC.ai[2] += 1f;
                        if (NPC.ai[2] >= ((spazAlive && retAlive) ? 75f : 60f) - (death ? 20f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
                        {
                            NPC.ai[1] = 2f;
                            NPC.ai[2] = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                }
            }
        }

        public override void BossLoot(ref string name, ref int potionType) => potionType = ItemID.GreaterHealingPotion;

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance * bossAdjustment);
            NPC.damage = (int)(NPC.damage * NPC.GetExpertDamageMultiplier());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, spriteEffects, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100D; i++)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);

                return;
            }

            for (int i = 0; i < 150; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2 * hit.HitDirection, -2f);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 2);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 7);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 9);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), Mod.Find<ModGore>("Foveanator2").Type);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 31, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 1.4f;
            }

            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, 0f, 0f, 100, default, 2.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 5f;
                dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 3f;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                int gore = Gore.NewGore(NPC.GetSource_Death(), NPC.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity *= 0.4f;
                Main.gore[gore].velocity.X += 1f;
                Main.gore[gore].velocity.Y += 1f;
                gore = Gore.NewGore(NPC.GetSource_Death(), NPC.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity *= 0.4f;
                Main.gore[gore].velocity.X -= 1f;
                Main.gore[gore].velocity.Y += 1f;
                gore = Gore.NewGore(NPC.GetSource_Death(), NPC.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity *= 0.4f;
                Main.gore[gore].velocity.X += 1f;
                Main.gore[gore].velocity.Y -= 1f;
                gore = Gore.NewGore(NPC.GetSource_Death(), NPC.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity *= 0.4f;
                Main.gore[gore].velocity.X -= 1f;
                Main.gore[gore].velocity.Y -= 1f;
            }
        }
    }
}
