using System;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class TwinsAI
    {
        public static bool BuffedRetinazerAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool masterMode = Main.masterMode || bossRush;
            bool death = CalamityWorld.death || bossRush;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            float enrageScale = bossRush ? 0.5f : masterMode ? 0.3f : 0f;
            if (Main.IsItDay() || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 1f;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Easier to send info to Spazmatism
            CalamityGlobalNPC.laserEye = npc.whoAmI;

            // Check for Spazmatism
            bool spazAlive = false;
            if (CalamityGlobalNPC.fireEye != -1)
                spazAlive = Main.npc[CalamityGlobalNPC.fireEye].active;

            // I'm not commenting this entire fucking thing, already did spaz, I'm not doing ret
            Vector2 hoverDestination = new Vector2(npc.Center.X - Main.player[npc.target].position.X - (Main.player[npc.target].width / 2), npc.position.Y + npc.height - 59f - Main.player[npc.target].position.Y - (Main.player[npc.target].height / 2));
            int direction = (npc.Center.X < Main.player[npc.target].position.X + Main.player[npc.target].width) ? -1 : 1;

            float hoverRotation = hoverDestination.ToRotation() + MathHelper.PiOver2;
            if (hoverRotation < 0f)
                hoverRotation += MathHelper.TwoPi;
            else if (hoverRotation > MathHelper.TwoPi)
                hoverRotation -= MathHelper.TwoPi;

            float rotationRate = 0.15f;
            npc.rotation = npc.rotation.AngleTowards(hoverRotation, rotationRate);

            if (Main.rand.NextBool(5))
            {
                int retiDust = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, (int)(npc.height * 0.5f), DustID.Blood, npc.velocity.X, 2f, 0, default, 1f);
                Dust dust = Main.dust[retiDust];
                dust.velocity.X *= 0.5f;
                dust.velocity.Y *= 0.1f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && !Main.player[npc.target].dead && npc.timeLeft < 10)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (i != npc.whoAmI && Main.npc[i].active && (Main.npc[i].type == NPCID.Retinazer || Main.npc[i].type == NPCID.Spazmatism || Main.npc[i].type == ModContent.NPCType<Foveanator>()) && Main.npc[i].timeLeft - 1 > npc.timeLeft)
                        npc.timeLeft = Main.npc[i].timeLeft - 1;
                }
            }

            // Check for Oblivion in Master Mode
            bool oblivionAlive = false;
            if (masterMode && !bossRush && npc.localAI[3] == 1f)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && (Main.npc[i].type == ModContent.NPCType<SkeletronPrime2>() || Main.npc[i].type == NPCID.SkeletronPrime))
                    {
                        oblivionAlive = true;
                        break;
                    }
                }
            }

            // Foveanator spawn
            if (!oblivionAlive && masterMode && calamityGlobalNPC.newAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient && !bossRush)
            {
                NPC.SpawnOnPlayer(npc.FindClosestPlayer(), ModContent.NPCType<Foveanator>());
                calamityGlobalNPC.newAI[0] = 1f;
                npc.SyncExtraAI();
            }

            bool foveanatorAlive = NPC.AnyNPCs(ModContent.NPCType<Foveanator>());

            // Phase HP ratios
            float phase2LifeRatio = oblivionAlive ? 0.5f : masterMode ? 0.85f : 0.7f;
            float finalPhaseLifeRatio = masterMode ? 0.4f : 0.25f;

            // Movement variables
            float phase1MaxSpeedIncrease = masterMode ? 2f : 4f;
            float phase1MaxAccelerationIncrease = masterMode ? 0.025f : 0.05f;
            float phase1MaxChargeSpeedIncrease = masterMode ? 3f : 6f;

            // Phase duration variables
            float phase1MaxLaserPhaseDurationDecrease = masterMode ? 120f : 300f;

            // If Oblivion is alive reduce aggression of all attacks
            if (oblivionAlive)
            {
                phase1MaxSpeedIncrease = masterMode ? 1f : 2f;
                phase1MaxAccelerationIncrease = masterMode ? 0.0125f : 0.025f;
                phase1MaxChargeSpeedIncrease = masterMode ? 1.5f : 3f;

                phase1MaxLaserPhaseDurationDecrease = masterMode ? 60f : 150f;
            }

            // Phase checks
            bool phase2 = lifeRatio < phase2LifeRatio;
            bool finalPhase = lifeRatio < finalPhaseLifeRatio;

            Vector2 mechQueenSpacing = Vector2.Zero;
            if (NPC.IsMechQueenUp)
            {
                NPC nPC = Main.npc[NPC.mechQueen];
                Vector2 mechQueenCenter = nPC.GetMechQueenCenter();
                Vector2 eyePosition = new Vector2(-150f, -250f);
                eyePosition *= 0.75f;
                float mechdusaRotation = nPC.velocity.X * 0.025f;
                mechQueenSpacing = mechQueenCenter + eyePosition;
                mechQueenSpacing = mechQueenSpacing.RotatedBy(mechdusaRotation, mechQueenCenter);
            }

            npc.reflectsProjectiles = false;

            // Despawn
            bool oblivionFightDespawn = ((oblivionAlive && lifeRatio < 0.8f) || (oblivionAlive && !spazAlive && lifeRatio < 0.95f)) && npc.localAI[3] == 1f;
            if (Main.player[npc.target].dead || oblivionFightDespawn)
            {
                npc.velocity.Y -= 0.04f;
                if (npc.timeLeft > 10)
                {
                    npc.timeLeft = 10;
                    return false;
                }
            }

            else if (npc.ai[0] == 0f)
            {
                if (npc.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    float maxVelocity = 8.25f;
                    float acceleration = 0.115f;
                    maxVelocity += 4f * enrageScale;
                    acceleration += 0.05f * enrageScale;

                    if (death)
                    {
                        maxVelocity += phase1MaxSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                        acceleration += phase1MaxAccelerationIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    }

                    if (Main.getGoodWorld)
                    {
                        maxVelocity *= 1.15f;
                        acceleration *= 1.15f;
                    }

                    float distanceFromTarget = 300f;
                    Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * distanceFromTarget * direction - Vector2.UnitY * distanceFromTarget;
                    float distanceFromDestination = (destination - npc.Center).Length();
                    Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);

                    if (NPC.IsMechQueenUp)
                    {
                        maxVelocity = 14f;

                        destination = mechQueenSpacing;
                        distanceFromDestination = (destination - npc.Center).Length();
                        idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitY);

                        if (distanceFromDestination > maxVelocity)
                            idealVelocity *= maxVelocity / distanceFromDestination;

                        float inertia = 60f;
                        npc.velocity = (npc.velocity * (inertia - 1f) + idealVelocity) / inertia;
                    }
                    else
                        npc.SimpleFlyMovement(idealVelocity * maxVelocity, acceleration);
                    
                    float phaseGateValue = (masterMode ? 300f : 450f) - (death ? phase1MaxLaserPhaseDurationDecrease * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    float laserGateValue = oblivionAlive ? 60f : foveanatorAlive ? 45f : 30f;
                    if (NPC.IsMechQueenUp)
                    {
                        phaseGateValue = 900f;
                        laserGateValue = ((!NPC.npcsFoundForCheckActive[NPCID.TheDestroyerBody]) ? 60f : 90f);
                    }

                    npc.ai[2] += 1f;
                    if (npc.ai[2] >= phaseGateValue)
                    {
                        npc.ai[1] = 1f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        
                        CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                        options.aggroRatio = -1f;
                        CalamityUtils.CalamityTargeting(npc, options);

                        npc.netUpdate = true;
                    }
                    else if (distanceFromDestination < (death ? 960f : 800f))
                    {
                        if (!Main.player[npc.target].dead)
                        {
                            npc.ai[3] += 1f;
                            if (Main.getGoodWorld)
                                npc.ai[3] += 0.5f;
                        }

                        if (npc.ai[3] >= laserGateValue)
                        {
                            npc.ai[3] = 0f;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float laserSpeed = 10.5f;
                                laserSpeed += 3f * enrageScale;

                                int type = ProjectileID.EyeLaser;
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                Vector2 laserVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserSpeed;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 90f, laserVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                else if (npc.ai[1] == 1f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.rotation = hoverRotation;

                    float chargeSpeed = 15f;
                    chargeSpeed += 10f * enrageScale;
                    if (death)
                        chargeSpeed += phase1MaxChargeSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    if (Main.getGoodWorld)
                        chargeSpeed += 2f;

                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;

                    npc.ai[1] = 2f;
                }
                else if (npc.ai[1] == 2f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.ai[2] += 1f;
                    float decelerateGateValue = (masterMode ? 36f : 32f) + (death ? (masterMode ? 6f : 12f) * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (npc.ai[2] >= decelerateGateValue)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        float decelerationMultiplier = (masterMode ? 0.84f : 0.92f) - (death ? (masterMode ? 0.16f : 0.32f) * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                        npc.velocity *= decelerationMultiplier;

                        if (Math.Abs(npc.velocity.X) < 0.1)
                            npc.velocity.X = 0f;
                        if (Math.Abs(npc.velocity.Y) < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

                    float delayBeforeChargingAgain = (masterMode ? 48f : 56f) - (death ? (masterMode ? 3f : 6f) * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (npc.ai[2] >= delayBeforeChargingAgain + (oblivionAlive ? 15f : 0f))
                    {
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;

                        npc.rotation = hoverRotation;

                        float totalCharges = death ? 6f : 5f;
                        if (foveanatorAlive)
                            totalCharges -= 1f;

                        if (npc.ai[3] >= totalCharges)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[3] = 0f;

                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            CalamityUtils.CalamityTargeting(npc, options);
                        }
                        else
                            npc.ai[1] = 1f;
                    }
                }

                // Enter phase 2
                if (phase2)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;

                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.aggroRatio = -1f;
                    CalamityUtils.CalamityTargeting(npc, options);

                    npc.netUpdate = true;
                }
            }

            else if (npc.ai[0] == 1f || npc.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                if (NPC.IsMechQueenUp)
                    npc.reflectsProjectiles = true;

                if (npc.ai[0] == 1f)
                {
                    npc.ai[2] += 0.005f;
                    if (npc.ai[2] > 0.5)
                        npc.ai[2] = 0.5f;
                }
                else
                {
                    npc.ai[2] -= 0.005f;
                    if (npc.ai[2] < 0f)
                        npc.ai[2] = 0f;
                }

                npc.rotation += npc.ai[2];

                npc.ai[1] += 1f;
                if (masterMode && npc.ai[2] >= 0.2f)
                {
                    if (npc.ai[1] % 10f == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item33, npc.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            bool shootLaser = npc.ai[1] % 20f == 0f;
                            int type = shootLaser ? ProjectileID.DeathLaser : ModContent.ProjectileType<HomingLaserDart>();
                            int damage = npc.GetProjectileDamage(type);

                            // Reduce mech boss projectile damage depending on the new ore progression changes
                            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                            {
                                double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                if (!NPC.downedMechBossAny)
                                    damage = (int)(damage * firstMechMultiplier);
                                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                    damage = (int)(damage * secondMechMultiplier);
                            }

                            Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * (oblivionAlive ? 6f : 7f);
                            int numProj = shootLaser ? 6 : 2;
                            int spread = shootLaser ? 20 : 80;
                            float rotation = MathHelper.ToRadians(spread);
                            float offset = shootLaser ? 90f : 50f;
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * offset, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                if (npc.ai[1] == 100f)
                {
                    npc.ai[0] += 1f;
                    npc.ai[1] = 0f;
                    if (npc.ai[0] == 3f)
                    {
                        npc.ai[2] = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);

                        if (!Main.dedServ)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 143, 1f);
                                Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 7, 1f);
                                Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 6, 1f);
                            }
                        }

                        for (int j = 0; j < 20; j++)
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                    }
                }

                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                npc.velocity *= 0.98f;
                if (Math.Abs(npc.velocity.X) < 0.1)
                    npc.velocity.X = 0f;
                if (Math.Abs(npc.velocity.Y) < 0.1)
                    npc.velocity.Y = 0f;
            }
            else
            {
                // If in phase 2 but Spaz isn't
                bool spazInPhase1 = false;
                if (CalamityGlobalNPC.fireEye != -1)
                {
                    if (Main.npc[CalamityGlobalNPC.fireEye].active)
                        spazInPhase1 = Main.npc[CalamityGlobalNPC.fireEye].ai[0] == 1f || Main.npc[CalamityGlobalNPC.fireEye].ai[0] == 2f || Main.npc[CalamityGlobalNPC.fireEye].ai[0] == 0f;
                }

                npc.chaseable = !spazInPhase1;

                int setDamage = (int)Math.Round(npc.defDamage * 1.5);
                npc.defense = npc.defDefense + 10;
                calamityGlobalNPC.DR = spazInPhase1 ? 0.9999f : 0.2f;
                calamityGlobalNPC.unbreakableDR = spazInPhase1;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = spazInPhase1;

                npc.HitSound = SoundID.NPCHit4;

                if (npc.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    float maxVelocity = 9.5f + (death ? 3f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    float acceleration = 0.175f + (death ? 0.05f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    maxVelocity += 4.5f * enrageScale;
                    acceleration += 0.075f * enrageScale;

                    if (Main.getGoodWorld)
                    {
                        maxVelocity *= 1.15f;
                        acceleration *= 1.15f;
                    }

                    float distanceFromTarget = oblivionAlive ? 480f : 420f;
                    Vector2 destination = Main.player[npc.target].Center - Vector2.UnitY * distanceFromTarget;
                    float distanceFromDestination = (destination - npc.Center).Length();
                    Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);

                    if (NPC.IsMechQueenUp)
                    {
                        maxVelocity = 14f;

                        destination = mechQueenSpacing;
                        distanceFromDestination = (destination - npc.Center).Length();
                        idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitY);

                        if (distanceFromDestination > maxVelocity)
                            idealVelocity *= maxVelocity / distanceFromDestination;

                        float inertia = 5f;
                        npc.velocity = (npc.velocity * (inertia - 1f) + idealVelocity) / inertia;
                    }
                    else
                        npc.SimpleFlyMovement(idealVelocity * maxVelocity, acceleration);

                    npc.ai[2] += spazAlive ? 1f : 1.5f;
                    float phaseGateValue = NPC.IsMechQueenUp ? 900f : 300f - (death ? 120f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    if (npc.ai[2] >= phaseGateValue)
                    {
                        npc.ai[1] = 1f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        
                        CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                        options.aggroRatio = -1f;
                        CalamityUtils.CalamityTargeting(npc, options);

                        npc.netUpdate = true;
                    }

                    npc.rotation = (Main.player[npc.target].Center - npc.Center).ToRotation() - MathHelper.PiOver2;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.localAI[1] += 1f + (death ? (phase2LifeRatio - lifeRatio) / phase2LifeRatio : 0f);
                        if (npc.localAI[1] >= (spazAlive ? (oblivionAlive ? 76f : foveanatorAlive ? 64f : 52f) : foveanatorAlive ? 39f : 26f))
                        {
                            if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                            {
                                npc.localAI[1] = 0f;

                                float laserSpeed = 10f;
                                laserSpeed += enrageScale;
                                int type = ProjectileID.DeathLaser;
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                Vector2 laserVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserSpeed;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 120f, laserVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }
                else
                {
                    if (npc.ai[1] == 1f)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        float maxVelocity = 9.5f + (death ? 3f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float acceleration = 0.25f + (death ? 0.075f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        maxVelocity += 4.5f * enrageScale;
                        acceleration += 0.15f * enrageScale;

                        if (Main.getGoodWorld)
                        {
                            maxVelocity *= 1.15f;
                            acceleration *= 1.15f;
                        }

                        float distanceFromTarget = oblivionAlive ? 480f : 420f;
                        Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * distanceFromTarget * direction;
                        float distanceFromDestination = (destination - npc.Center).Length();
                        Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);
                        npc.SimpleFlyMovement(idealVelocity * maxVelocity, acceleration);

                        npc.rotation = (Main.player[npc.target].Center - npc.Center).ToRotation() - MathHelper.PiOver2;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            npc.localAI[1] += 1f + (death ? (phase2LifeRatio - lifeRatio) / phase2LifeRatio : 0f);
                            if (npc.localAI[1] > (spazAlive ? (oblivionAlive ? 30f : foveanatorAlive ? 25f : 20f) : foveanatorAlive ? 15f : 10f))
                            {
                                if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                                {
                                    npc.localAI[1] = 0f;

                                    float laserSpeed = 9f;
                                    laserSpeed += enrageScale;
                                    int type = ProjectileID.DeathLaser;
                                    int damage = (int)Math.Round(npc.GetProjectileDamage(type) * 0.75);

                                    // Reduce mech boss projectile damage depending on the new ore progression changes
                                    if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                    {
                                        double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                        double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                        if (!NPC.downedMechBossAny)
                                            damage = (int)(damage * firstMechMultiplier);
                                        else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                            damage = (int)(damage * secondMechMultiplier);
                                    }

                                    Vector2 laserVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserSpeed;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 120f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                }
                            }
                        }

                        npc.ai[2] += spazAlive ? 1f : 1.5f;
                        if (npc.ai[2] >= (masterMode ? 150f : 180f) - (death ? (masterMode ? 60f : 90f) * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
                        {
                            npc.ai[1] = finalPhase ? 4f : 0f;
                            npc.ai[2] = 0f;
                            npc.ai[3] = 0f;
                            
                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            CalamityUtils.CalamityTargeting(npc, options);

                            npc.netUpdate = true;
                        }
                    }

                    // Charge
                    else if (npc.ai[1] == 2f)
                    {
                        // Set damage
                        npc.damage = setDamage;

                        // Set rotation and velocity
                        npc.rotation = hoverRotation;

                        float chargeSpeed = 22f + (death ? 8f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        chargeSpeed += 10f * enrageScale;
                        if (!spazAlive)
                            chargeSpeed += 2f;
                        if (Main.getGoodWorld)
                            chargeSpeed += 2f;

                        npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * chargeSpeed;

                        npc.ai[1] = 3f;
                    }

                    else if (npc.ai[1] == 3f)
                    {
                        // Set damage
                        npc.damage = setDamage;

                        npc.ai[2] += 1f;

                        float chargeTime = spazAlive ? 45f : 30f;
                        if (npc.ai[3] % 3f == 0f)
                            chargeTime = spazAlive ? 90f : 60f;
                        if (death)
                            chargeTime -= chargeTime * 0.25f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio);
                        chargeTime -= chargeTime / 5 * enrageScale;

                        // Slow down
                        if (npc.ai[2] >= chargeTime)
                        {
                            // Avoid cheap bullshit
                            npc.damage = 0;

                            npc.velocity *= 0.93f;

                            if (Math.Abs(npc.velocity.X) < 0.1)
                                npc.velocity.X = 0f;
                            if (Math.Abs(npc.velocity.Y) < 0.1)
                                npc.velocity.Y = 0f;
                        }
                        else
                        {
                            npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

                            if (npc.ai[3] % 3f == 0f)
                            {
                                float fireRate = spazAlive ? 13f : foveanatorAlive ? 11f : 9f;
                                if (npc.ai[2] % fireRate == 0f)
                                {
                                    SoundEngine.PlaySound(SoundID.Item33, npc.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        float laserDartSpeed = (death ? 9f : 6f) * (spazAlive ? 1f : 1.5f);
                                        int type = ModContent.ProjectileType<HomingLaserDart>();
                                        int damage = npc.GetProjectileDamage(type);

                                        // Reduce mech boss projectile damage depending on the new ore progression changes
                                        if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                        {
                                            double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                            double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                            if (!NPC.downedMechBossAny)
                                                damage = (int)(damage * firstMechMultiplier);
                                            else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                                damage = (int)(damage * secondMechMultiplier);
                                        }

                                        Vector2 laserDartVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserDartSpeed;
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + npc.velocity.SafeNormalize(Vector2.UnitY) * 60f, laserDartVelocity, type, damage, 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }

                        // Charge four times
                        float chargeGateValue = 30f;
                        chargeGateValue -= chargeGateValue / 4 * enrageScale;
                        if (npc.ai[2] >= chargeTime + chargeGateValue)
                        {
                            npc.ai[2] = 0f;

                            float chargeIncrement = 1f;
                            if (masterMode && Main.rand.NextBool() && npc.ai[3] < (spazAlive ? 1f : 3f))
                            {
                                chargeIncrement = 2f;

                                // Net update due to the randomness in Master Mode
                                npc.netUpdate = true;
                            }

                            npc.rotation = hoverRotation;

                            npc.ai[3] += chargeIncrement;
                            float maxChargeAmt = spazAlive ? 2f : 4f;
                            if (npc.ai[3] >= maxChargeAmt)
                            {
                                npc.ai[1] = 0f;
                                npc.ai[3] = 0f;

                                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                                options.aggroRatio = -1f;
                                CalamityUtils.CalamityTargeting(npc, options);
                            }
                            else
                                npc.ai[1] = 4f;
                        }
                    }

                    // Get in position for charge
                    else if (npc.ai[1] == 4f)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        float chargeLineUpDistance = spazAlive ? 600f : 500f;
                        float chargeSpeed = 18f + (death ? 6f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float chargeAcceleration = 0.45f + (death ? 0.15f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        chargeSpeed += 6f * enrageScale;
                        chargeAcceleration += 0.15f * enrageScale;

                        if (spazAlive)
                        {
                            chargeSpeed *= 0.75f;
                            chargeAcceleration *= 0.75f;
                        }

                        if (Main.getGoodWorld)
                        {
                            chargeSpeed *= 1.15f;
                            chargeAcceleration *= 1.15f;
                        }

                        Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * chargeLineUpDistance * direction;
                        Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;
                        npc.SimpleFlyMovement(idealVelocity, chargeAcceleration);

                        // Take 1.25 or 1 second to get in position, then charge
                        npc.ai[2] += 1f;
                        if (npc.ai[2] >= (spazAlive ? 75f : 60f) - (death ? 20f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
                        {
                            npc.ai[1] = 2f;
                            npc.ai[2] = 0f;
                            npc.netUpdate = true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool BuffedSpazmatismAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool masterMode = Main.masterMode || bossRush;
            bool death = CalamityWorld.death || bossRush;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            float enrageScale = bossRush ? 0.5f : masterMode ? 0.3f : 0f;
            if (Main.IsItDay() || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 1f;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Easier to send info to Retinazer
            CalamityGlobalNPC.fireEye = npc.whoAmI;

            // Check for Retinazer
            bool retAlive = false;
            if (CalamityGlobalNPC.laserEye != -1)
                retAlive = Main.npc[CalamityGlobalNPC.laserEye].active;

            // Rotation
            Vector2 hoverDestination = new Vector2(npc.Center.X - Main.player[npc.target].position.X - (Main.player[npc.target].width / 2), npc.position.Y + npc.height - 59f - Main.player[npc.target].position.Y - (Main.player[npc.target].height / 2));
            int direction = (npc.Center.X < Main.player[npc.target].position.X + Main.player[npc.target].width) ? -1 : 1;

            float hoverRotation = hoverDestination.ToRotation() + MathHelper.PiOver2;
            if (hoverRotation < 0f)
                hoverRotation += MathHelper.TwoPi;
            else if (hoverRotation > MathHelper.TwoPi)
                hoverRotation -= MathHelper.TwoPi;

            float rotationRate = 0.15f;
            if (NPC.IsMechQueenUp && npc.ai[0] == 3f && npc.ai[1] == 0f)
                rotationRate *= 0.25f;

            npc.rotation = npc.rotation.AngleTowards(hoverRotation, rotationRate);

            // Dust
            if (Main.rand.NextBool(5))
            {
                int spazDust = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, (int)(npc.height * 0.5f), DustID.Blood, npc.velocity.X, 2f, 0, default, 1f);
                Dust dust = Main.dust[spazDust];
                dust.velocity.X *= 0.5f;
                dust.velocity.Y *= 0.1f;
            }

            // Despawn Twins at the same time
            if (Main.netMode != NetmodeID.MultiplayerClient && !Main.player[npc.target].dead && npc.timeLeft < 10)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (i != npc.whoAmI && Main.npc[i].active && (Main.npc[i].type == NPCID.Retinazer || Main.npc[i].type == NPCID.Spazmatism || Main.npc[i].type == ModContent.NPCType<Foveanator>()) && Main.npc[i].timeLeft - 1 > npc.timeLeft)
                        npc.timeLeft = Main.npc[i].timeLeft - 1;
                }
            }

            // Check for Oblivion in Master Mode
            bool oblivionAlive = false;
            if (masterMode && !bossRush && npc.localAI[3] == 1f)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && (Main.npc[i].type == ModContent.NPCType<SkeletronPrime2>() || Main.npc[i].type == NPCID.SkeletronPrime))
                    {
                        oblivionAlive = true;
                        break;
                    }
                }
            }

            bool foveanatorAlive = NPC.AnyNPCs(ModContent.NPCType<Foveanator>());

            // Phase HP ratios
            float phase2LifeRatio = oblivionAlive ? 0.5f : masterMode ? 0.85f : 0.7f;
            float finalPhaseLifeRatio = masterMode ? 0.3f : 0.15f;

            // Movement variables
            float phase1MaxSpeedIncrease = masterMode ? 2.25f : 4.5f;
            float phase1MaxAccelerationIncrease = masterMode ? 0.075f : 0.15f;
            float phase1MaxChargeSpeedIncrease = masterMode ? 3f : 6f;

            // Phase duration variables
            float phase1MaxCursedFlamePhaseDurationDecrease = masterMode ? 80f : 200f;
            float phase1MaxChargesDecrease = masterMode ? 2f : 4f;

            // If Oblivion is alive reduce aggression of all attacks
            if (oblivionAlive)
            {
                phase1MaxSpeedIncrease = masterMode ? 1.125f : 2.25f;
                phase1MaxAccelerationIncrease = masterMode ? 0.0375f : 0.075f;
                phase1MaxChargeSpeedIncrease = masterMode ? 1.5f : 3f;

                phase1MaxCursedFlamePhaseDurationDecrease = masterMode ? 40f : 100f;
                phase1MaxChargesDecrease = masterMode ? 1f : 2f;
            }

            // Phase checks
            bool phase2 = lifeRatio < phase2LifeRatio;
            bool finalPhase = lifeRatio < finalPhaseLifeRatio;

            Vector2 mechQueenSpacing = Vector2.Zero;
            if (NPC.IsMechQueenUp)
            {
                NPC nPC2 = Main.npc[NPC.mechQueen];
                Vector2 mechQueenCenter2 = nPC2.GetMechQueenCenter();
                Vector2 mechdusaSpacingVector = new Vector2(150f, -250f);
                mechdusaSpacingVector *= 0.75f;
                float mechdusaSpacingVel = nPC2.velocity.X * 0.025f;
                mechQueenSpacing = mechQueenCenter2 + mechdusaSpacingVector;
                mechQueenSpacing = mechQueenSpacing.RotatedBy(mechdusaSpacingVel, mechQueenCenter2);
            }

            npc.reflectsProjectiles = false;

            // Despawn
            bool oblivionFightDespawn = ((oblivionAlive && lifeRatio < 0.8f) || (oblivionAlive && !retAlive && lifeRatio < 0.95f)) && npc.localAI[3] == 1f;
            if (Main.player[npc.target].dead || oblivionFightDespawn)
            {
                npc.velocity.Y -= 0.04f;
                if (npc.timeLeft > 10)
                {
                    npc.timeLeft = 10;
                    return false;
                }
            }

            // Phase 1
            else if (npc.ai[0] == 0f)
            {
                // Cursed fireball phase
                if (npc.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    // Velocity
                    float maxVelocity = 12f;
                    float acceleration = 0.4f;
                    maxVelocity += 6f * enrageScale;
                    acceleration += 0.2f * enrageScale;

                    if (death)
                    {
                        maxVelocity += phase1MaxSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                        acceleration += phase1MaxAccelerationIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    }

                    if (Main.getGoodWorld)
                    {
                        maxVelocity *= 1.15f;
                        acceleration *= 1.15f;
                    }

                    float distanceFromTarget = oblivionAlive ? 480f : 400f;
                    Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * distanceFromTarget * direction;
                    float distanceFromDestination = (destination - npc.Center).Length();
                    Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);

                    if (NPC.IsMechQueenUp)
                    {
                        maxVelocity = 14f;

                        destination = mechQueenSpacing;
                        distanceFromDestination = (destination - npc.Center).Length();
                        idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitY);

                        if (distanceFromDestination > maxVelocity)
                            idealVelocity *= maxVelocity / distanceFromDestination;

                        float inertia = 5f;
                        npc.velocity = (npc.velocity * (inertia - 1f) + idealVelocity) / inertia;
                    }
                    else
                        npc.SimpleFlyMovement(idealVelocity * maxVelocity, acceleration);

                    // Fire cursed flames for 5 seconds
                    npc.ai[2] += 1f;
                    float phaseGateValue = NPC.IsMechQueenUp ? 900f : 300f - (death ? phase1MaxCursedFlamePhaseDurationDecrease * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (npc.ai[2] >= phaseGateValue)
                    {
                        // Reset AI array and go to charging phase
                        npc.ai[1] = 1f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        // Fire cursed flame every half second
                        if (!Main.player[npc.target].dead)
                        {
                            npc.ai[3] += 1f;
                            if (Main.getGoodWorld)
                                npc.ai[3] += 0.4f;
                        }

                        if (npc.ai[3] >= (oblivionAlive ? 60f : foveanatorAlive ? 45f : 30f))
                        {
                            npc.ai[3] = 0f;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float cursedFireballSpeed = 15f;
                                cursedFireballSpeed += 3f * enrageScale;
                                int type = ProjectileID.CursedFlameHostile;
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                Vector2 fireballVelocity = ((Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * cursedFireballSpeed) + new Vector2(Main.rand.Next(-10, 11), Main.rand.Next(-10, 11)) * 0.05f;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + fireballVelocity.SafeNormalize(Vector2.UnitY) * 50f, fireballVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                // Charging phase
                else if (npc.ai[1] == 1f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    // Rotation and velocity
                    npc.rotation = hoverRotation;

                    float chargeSpeed = 18f;
                    chargeSpeed += 8f * enrageScale;
                    if (death)
                        chargeSpeed += phase1MaxChargeSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    if (Main.getGoodWorld)
                        chargeSpeed *= 1.2f;

                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;

                    npc.ai[1] = 2f;
                }
                else if (npc.ai[1] == 2f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.ai[2] += 1f;

                    float timeBeforeSlowDown = masterMode ? 30f : 10f;
                    if (npc.ai[2] >= timeBeforeSlowDown)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        // Slow down
                        npc.velocity *= 0.8f;

                        if (Math.Abs(npc.velocity.X) < 0.1)
                            npc.velocity.X = 0f;
                        if (Math.Abs(npc.velocity.Y) < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

                    // Charge 8 times
                    float chargeTime = masterMode ? 45f : 25f;
                    if (npc.ai[2] >= chargeTime + (oblivionAlive ? 15f : 0f))
                    {
                        // Reset AI array and go to cursed fireball phase
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;

                        npc.rotation = hoverRotation;

                        float totalCharges = foveanatorAlive ? 6f : 8f;
                        if (death)
                            totalCharges -= (float)Math.Round(phase1MaxChargesDecrease * ((1f - lifeRatio) / (1f - phase2LifeRatio)));

                        if (npc.ai[3] >= totalCharges)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[3] = 0f;
                        }
                        else
                            npc.ai[1] = 1f;
                    }
                }

                // Enter phase 2
                if (phase2)
                {
                    // Reset AI array and go to transition phase
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;

                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.finishThemOff = true;
                    CalamityUtils.CalamityTargeting(npc, options);

                    npc.netUpdate = true;
                }
            }

            // Transition phase
            else if (npc.ai[0] == 1f || npc.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                if (NPC.IsMechQueenUp)
                    npc.reflectsProjectiles = true;

                // AI timer for rotation
                if (npc.ai[0] == 1f)
                {
                    npc.ai[2] += 0.005f;
                    if (npc.ai[2] > 0.5)
                        npc.ai[2] = 0.5f;
                }
                else
                {
                    npc.ai[2] -= 0.005f;
                    if (npc.ai[2] < 0f)
                        npc.ai[2] = 0f;
                }

                // Spin around like a moron while flinging blood and gore everywhere
                npc.rotation += npc.ai[2];

                npc.ai[1] += 1f;
                if (masterMode && npc.ai[2] >= 0.2f)
                {
                    if (npc.ai[1] % 10f == 0f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = npc.ai[1] % 20f == 0f ? ProjectileID.CursedFlameHostile : ModContent.ProjectileType<ShadowflameFireball>();
                            int damage = npc.GetProjectileDamage(type);

                            // Reduce mech boss projectile damage depending on the new ore progression changes
                            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                            {
                                double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                if (!NPC.downedMechBossAny)
                                    damage = (int)(damage * firstMechMultiplier);
                                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                    damage = (int)(damage * secondMechMultiplier);
                            }

                            Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * (oblivionAlive ? 12f : 16f) + Main.rand.NextVector2CircularEdge(3f, 3f);
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 50f, projectileVelocity, type, damage, 0f, Main.myPlayer, 0f, 1f);
                            Main.projectile[proj].tileCollide = false;
                        }
                    }
                }

                if (npc.ai[1] == 100f)
                {
                    npc.ai[0] += 1f;
                    npc.ai[1] = 0f;

                    if (npc.ai[0] == 3f)
                    {
                        npc.ai[2] = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);

                        if (!Main.dedServ)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 144, 1f);
                                Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 7, 1f);
                                Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 6, 1f);
                            }
                        }

                        for (int j = 0; j < 20; j++)
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                    }
                }

                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                npc.velocity *= 0.98f;
                if (Math.Abs(npc.velocity.X) < 0.1)
                    npc.velocity.X = 0f;
                if (Math.Abs(npc.velocity.Y) < 0.1)
                    npc.velocity.Y = 0f;
            }

            // Phase 2
            else
            {
                // If in phase 2 but Ret isn't
                bool retInPhase1 = false;
                if (CalamityGlobalNPC.laserEye != -1)
                {
                    if (Main.npc[CalamityGlobalNPC.laserEye].active)
                        retInPhase1 = Main.npc[CalamityGlobalNPC.laserEye].ai[0] == 1f || Main.npc[CalamityGlobalNPC.laserEye].ai[0] == 2f || Main.npc[CalamityGlobalNPC.laserEye].ai[0] == 0f;
                }

                npc.chaseable = !retInPhase1;

                // Increase defense and damage
                int setDamage = (int)Math.Round(npc.defDamage * 1.5);
                int reducedSetDamage = (int)Math.Round(setDamage * 0.5);
                npc.defense = npc.defDefense + 18;
                calamityGlobalNPC.DR = retInPhase1 ? 0.9999f : 0.2f;
                calamityGlobalNPC.unbreakableDR = retInPhase1;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = retInPhase1;

                // Change hit sound to metal
                npc.HitSound = SoundID.NPCHit4;

                // Shadowflamethrower phase
                if (npc.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = reducedSetDamage;

                    float maxVelocity = 6.2f + (death ? 2f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    float acceleration = 0.1f + (death ? 0.03f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    maxVelocity += 3f * enrageScale;
                    acceleration += 0.06f * enrageScale;

                    float distanceFromTarget = 180f;
                    Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * distanceFromTarget * direction;
                    float distanceFromDestination = (destination - npc.Center).Length();
                    Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);

                    if (!NPC.IsMechQueenUp)
                    {
                        // Boost speed if too far from target
                        if (distanceFromDestination > distanceFromTarget)
                            maxVelocity += MathHelper.Lerp(0f, oblivionAlive ? 3f : masterMode ? 8f : 6f, MathHelper.Clamp((distanceFromDestination - distanceFromTarget) / 1000f, 0f, 1f));

                        if (Main.getGoodWorld)
                        {
                            maxVelocity *= 1.15f;
                            acceleration *= 1.15f;
                        }

                        npc.SimpleFlyMovement(idealVelocity * maxVelocity, acceleration);
                    }

                    // Fire flamethrower for x seconds
                    npc.ai[2] += retAlive ? 1f : 2f;
                    float phaseGateValue = NPC.IsMechQueenUp ? 900f : 180f - (death ? 60f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    if (npc.ai[2] >= phaseGateValue)
                    {
                        npc.ai[1] = finalPhase ? 5f : 1f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }

                    // Fire fireballs and flamethrower
                    if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                    {
                        // Play flame sound on timer
                        npc.localAI[2] += 1f;
                        if (npc.localAI[2] > 22f)
                        {
                            npc.localAI[2] = 0f;
                            SoundEngine.PlaySound(SoundID.Item34, npc.Center);
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            npc.localAI[1] += 1f;
                            if (npc.localAI[1] > 2f)
                            {
                                npc.ai[3] += 1f;
                                npc.localAI[1] = 0f;

                                float flamethrowerSpeed = foveanatorAlive ? 5f : 6f;
                                flamethrowerSpeed += 3f * enrageScale;
                                float timeForFlamethrowerToReachMaxVelocity = 60f;
                                float flamethrowerSpeedScalar = MathHelper.Clamp(npc.ai[2] / timeForFlamethrowerToReachMaxVelocity, 0f, 1f);
                                flamethrowerSpeed = MathHelper.Lerp(0.1f, flamethrowerSpeed, flamethrowerSpeedScalar);
                                int type = npc.ai[3] % 2f == 0f ? ModContent.ProjectileType<CursedFire>() : ModContent.ProjectileType<Shadowflamethrower>();
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                Vector2 flamethrowerVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * flamethrowerSpeed + npc.velocity * 0.5f;

                                if (NPC.IsMechQueenUp)
                                    flamethrowerVelocity = (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * flamethrowerSpeed + npc.velocity * 0.5f;

                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + flamethrowerVelocity.SafeNormalize(Vector2.UnitY) * 25f, flamethrowerVelocity, type, damage, 0f, Main.myPlayer);
                                if (masterMode && npc.ai[3] % 30f == 0f)
                                {
                                    type = npc.ai[3] % 60f == 0f ? ModContent.ProjectileType<ShadowflameFireball>() : ProjectileID.CursedFlameHostile;
                                    damage = npc.GetProjectileDamage(type);
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + flamethrowerVelocity.SafeNormalize(Vector2.UnitY) * 50f, flamethrowerVelocity * 2f, type, damage, 0f, Main.myPlayer);
                                }
                            }
                        }
                    }

                    if (NPC.IsMechQueenUp)
                    {
                        maxVelocity = 14f;

                        destination = mechQueenSpacing;
                        distanceFromDestination = (destination - npc.Center).Length();
                        idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitY);

                        if (distanceFromDestination > maxVelocity)
                            idealVelocity *= maxVelocity / distanceFromDestination;

                        float inertia = 60f;
                        npc.velocity = (npc.velocity * (inertia - 1f) + idealVelocity) / inertia;
                    }
                }

                // Charging phase
                else
                {
                    // Charge
                    if (npc.ai[1] == 1f)
                    {
                        // Set damage
                        npc.damage = setDamage;

                        // Play charge sound
                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                        // Set rotation and velocity
                        npc.rotation = hoverRotation;

                        float chargeSpeed = 18f + (death ? 5f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        chargeSpeed += 16f * enrageScale;
                        if (Main.getGoodWorld)
                            chargeSpeed *= 1.2f;

                        npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;

                        npc.ai[1] = 2f;

                        return false;
                    }

                    if (npc.ai[1] == 2f)
                    {
                        // Set damage
                        npc.damage = setDamage;

                        npc.ai[2] += retAlive ? 1f : 1.25f;

                        float chargeTime = 30f - (death ? 10f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

                        // Slow down
                        if (npc.ai[2] >= chargeTime)
                        {
                            // Avoid cheap bullshit
                            npc.damage = reducedSetDamage;

                            float deceleration = 0.85f - (death ? 0.1f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                            npc.velocity *= deceleration;

                            if (Math.Abs(npc.velocity.X) < 0.1)
                                npc.velocity.X = 0f;
                            if (Math.Abs(npc.velocity.Y) < 0.1)
                                npc.velocity.Y = 0f;
                        }
                        else
                            npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

                        // Charges 5 times
                        if (npc.ai[2] >= (chargeTime * 1.6f) + (oblivionAlive ? 15f : 0f))
                        {
                            npc.ai[3] += 1f;
                            npc.ai[2] = 0f;

                            npc.rotation = hoverRotation;

                            float totalCharges = foveanatorAlive ? 4f : 5f;
                            if (npc.ai[3] >= totalCharges)
                            {
                                npc.ai[1] = 0f;
                                npc.ai[3] = 0f;
                                return false;
                            }

                            npc.ai[1] = 1f;
                        }
                    }

                    // Crazy charge
                    else if (npc.ai[1] == 3f)
                    {
                        // Avoid cheap bullshit
                        npc.damage = reducedSetDamage;

                        // Reset AI array and go to shadowflamethrower phase or fireball phase if ret is dead
                        float secondFastCharge = 4f;
                        if (npc.ai[3] >= (retAlive ? secondFastCharge : secondFastCharge + 1f))
                        {
                            npc.ai[1] = retAlive ? 0f : 5f;
                            npc.ai[2] = 0f;
                            npc.ai[3] = 0f;

                            if (npc.ai[1] == 0f)
                                npc.localAI[1] = -20f;

                            npc.netUpdate = true;

                            if (npc.netSpam > 10)
                                npc.netSpam = 10;
                        }

                        // Set charging velocity
                        else if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            // Velocity
                            float spazmatismPhase3ChargeSpeed = 20f + (death ? 6f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                            spazmatismPhase3ChargeSpeed += 10f * enrageScale;
                            if (npc.ai[2] == -1f || (!retAlive && npc.ai[3] == secondFastCharge))
                                spazmatismPhase3ChargeSpeed *= 1.3f;
                            if (Main.getGoodWorld)
                                spazmatismPhase3ChargeSpeed *= 1.2f;

                            Vector2 distanceVector = Main.player[npc.target].Center + (!retAlive && bossRush ? Main.player[npc.target].velocity * 20f : Vector2.Zero) - npc.Center;
                            npc.velocity = distanceVector.SafeNormalize(Vector2.UnitY) * spazmatismPhase3ChargeSpeed;

                            if (retAlive)
                            {
                                if (npc.Distance(Main.player[npc.target].Center) < 100f)
                                {
                                    if (Math.Abs(npc.velocity.X) > Math.Abs(npc.velocity.Y))
                                    {
                                        float absoluteSpazXVel = Math.Abs(npc.velocity.X);
                                        float absoluteSpazYVel = Math.Abs(npc.velocity.Y);

                                        if (npc.Center.X > Main.player[npc.target].Center.X)
                                            absoluteSpazYVel *= -1f;
                                        if (npc.Center.Y > Main.player[npc.target].Center.Y)
                                            absoluteSpazXVel *= -1f;

                                        npc.velocity = new Vector2(absoluteSpazYVel, absoluteSpazXVel);
                                    }
                                }
                            }

                            if (death)
                            {
                                float projectileSpeed = spazmatismPhase3ChargeSpeed * 0.5f;
                                int type = (!retAlive && npc.ai[3] % 2f == 0f) ? ModContent.ProjectileType<ShadowflameFireball>() : ProjectileID.CursedFlameHostile;
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                Vector2 projectileVelocity = (Main.player[npc.target].Center + ((!retAlive && bossRush) ? Main.player[npc.target].velocity * 20f : Vector2.Zero) - npc.Center).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                                int numProj = 3;
                                int spread = 15;
                                float rotation = MathHelper.ToRadians(spread);

                                for (int i = 0; i < numProj; i++)
                                {
                                    Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 25f, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                                }
                            }

                            npc.ai[1] = 4f;
                            npc.netUpdate = true;

                            if (npc.netSpam > 10)
                                npc.netSpam = 10;
                        }
                    }

                    // Crazy charge
                    else if (npc.ai[1] == 4f)
                    {
                        // Set damage
                        npc.damage = setDamage;

                        if (npc.ai[2] == 0f)
                            SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                        float spazmatismRetDeadChargeSpeed = ((!retAlive && npc.ai[3] == 4f) ? 75f : 50f) - (float)Math.Round(death ? ((!retAlive && npc.ai[3] == 4f) ? 15f : 10f) * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

                        npc.ai[2] += 1f;

                        if (npc.ai[2] == spazmatismRetDeadChargeSpeed && Vector2.Distance(npc.position, Main.player[npc.target].position) < (retAlive ? 200f : 150f))
                            npc.ai[2] -= 1f;

                        // Slow down
                        if (npc.ai[2] >= spazmatismRetDeadChargeSpeed)
                        {
                            // Avoid cheap bullshit
                            npc.damage = reducedSetDamage;

                            npc.velocity *= 0.93f;
                            if (Math.Abs(npc.velocity.X) < 0.1)
                                npc.velocity.X = 0f;
                            if (Math.Abs(npc.velocity.Y) < 0.1)
                                npc.velocity.Y = 0f;
                        }
                        else
                            npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

                        // Charge 3 times
                        float spazmatismRetDeadChargeTimer = spazmatismRetDeadChargeSpeed + 25f;
                        if (npc.ai[2] >= spazmatismRetDeadChargeTimer)
                        {
                            npc.netUpdate = true;

                            if (npc.netSpam > 10)
                                npc.netSpam = 10;

                            float chargeIncrement = 1f;
                            if (masterMode && Main.rand.NextBool() && npc.ai[3] < (retAlive ? 2f : 3f))
                                chargeIncrement = 2f;

                            npc.ai[3] += chargeIncrement;
                            npc.ai[2] = 0f;
                            npc.ai[1] = 3f;
                        }
                    }

                    // Get in position for charge
                    else if (npc.ai[1] == 5f)
                    {
                        // Avoid cheap bullshit
                        npc.damage = reducedSetDamage;

                        float chargeLineUpDistance = retAlive ? 600f : 500f;
                        float chargeSpeed = 16f + (death ? 5f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float chargeAcceleration = 0.4f + (death ? 0.1f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        chargeSpeed += 5.333f * enrageScale;
                        chargeAcceleration += 0.133f * enrageScale;

                        if (retAlive)
                        {
                            chargeSpeed *= 0.75f;
                            chargeAcceleration *= 0.75f;
                        }

                        if (Main.getGoodWorld)
                        {
                            chargeSpeed *= 1.15f;
                            chargeAcceleration *= 1.15f;
                        }

                        Vector2 destination = Main.player[npc.target].Center + Vector2.UnitY * chargeLineUpDistance;
                        Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;
                        npc.SimpleFlyMovement(idealVelocity, chargeAcceleration);

                        npc.ai[2] += 1f;

                        // Fire shadowflames and cursed fireballs
                        float fireRate = retAlive ? 30f : foveanatorAlive ? 25f : 20f;
                        if (npc.ai[2] % fireRate == 0f)
                        {
                            npc.ai[3] += 1f;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float projectileSpeed = 16f;
                                int type = npc.ai[3] % 2f == 0f ? ProjectileID.CursedFlameHostile : ModContent.ProjectileType<ShadowflameFireball>();
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert;
                                    double secondMechMultiplier = CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                Vector2 projectileVelocity = (Main.player[npc.target].Center + (!retAlive && bossRush ? Main.player[npc.target].velocity * 20f : Vector2.Zero) - npc.Center).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 25f, projectileVelocity, type, damage, 0f, Main.myPlayer, 0f, retAlive ? 0f : 1f);
                            }
                        }

                        // Take 3 seconds to get in position, then charge
                        if (npc.ai[2] >= (retAlive ? 180f : 135f) - (death ? 45f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
                        {
                            npc.ai[1] = 3f;
                            npc.ai[2] = -1f;
                            npc.ai[3] = 0f;
                        }

                        npc.netUpdate = true;

                        if (npc.netSpam > 10)
                            npc.netSpam = 10;
                    }
                }
            }

            return false;
        }

        public static bool VanillaRetinazerAI(NPC npc, Mod mod)
        {
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            float phase2LifeRatio = Main.masterMode ? 0.6f : 0.4f;

            bool dead2 = Main.player[npc.target].dead;
            float num412 = npc.Center.X - Main.player[npc.target].position.X - (float)(Main.player[npc.target].width / 2);
            float num413 = npc.position.Y + (float)npc.height - 59f - Main.player[npc.target].position.Y - (float)(Main.player[npc.target].height / 2);
            float num414 = (float)Math.Atan2(num413, num412) + MathHelper.PiOver2;
            if (num414 < 0f)
                num414 += MathHelper.TwoPi;
            else if ((double)num414 > MathHelper.TwoPi)
                num414 -= MathHelper.TwoPi;

            float num415 = 0.1f;
            if (npc.rotation < num414)
            {
                if ((double)(num414 - npc.rotation) > MathHelper.Pi)
                    npc.rotation -= num415;
                else
                    npc.rotation += num415;
            }
            else if (npc.rotation > num414)
            {
                if ((double)(npc.rotation - num414) > MathHelper.Pi)
                    npc.rotation += num415;
                else
                    npc.rotation -= num415;
            }

            if (npc.rotation > num414 - num415 && npc.rotation < num414 + num415)
                npc.rotation = num414;

            if (npc.rotation < 0f)
                npc.rotation += MathHelper.TwoPi;
            else if ((double)npc.rotation > MathHelper.TwoPi)
                npc.rotation -= MathHelper.TwoPi;

            if (npc.rotation > num414 - num415 && npc.rotation < num414 + num415)
                npc.rotation = num414;

            if (Main.rand.NextBool(5))
            {
                int num416 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + (float)npc.height * 0.25f), npc.width, (int)((float)npc.height * 0.5f), DustID.Blood, npc.velocity.X, 2f);
                Main.dust[num416].velocity.X *= 0.5f;
                Main.dust[num416].velocity.Y *= 0.1f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && !Main.IsItDay() && !dead2 && npc.timeLeft < 10)
            {
                for (int num417 = 0; num417 < Main.maxNPCs; num417++)
                {
                    if (num417 != npc.whoAmI && Main.npc[num417].active && (Main.npc[num417].type == NPCID.Retinazer || Main.npc[num417].type == NPCID.Spazmatism))
                        npc.DiscourageDespawn(Main.npc[num417].timeLeft - 1);
                }
            }

            Vector2 vector41 = Vector2.Zero;
            if (NPC.IsMechQueenUp)
            {
                NPC nPC = Main.npc[NPC.mechQueen];
                Vector2 mechQueenCenter = nPC.GetMechQueenCenter();
                Vector2 vector42 = new Vector2(-150f, -250f);
                vector42 *= 0.75f;
                float num418 = nPC.velocity.X * 0.025f;
                vector41 = mechQueenCenter + vector42;
                vector41 = vector41.RotatedBy(num418, mechQueenCenter);
            }

            npc.reflectsProjectiles = false;
            if (Main.IsItDay() || dead2)
            {
                npc.velocity.Y -= 0.04f;
                npc.EncourageDespawn(10);
                return false;
            }

            if (npc.ai[0] == 0f)
            {
                if (npc.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    float num419 = 7f;
                    float num420 = 0.1f;
                    if (Main.expertMode)
                    {
                        num419 = Main.masterMode ? 9.5f : 8.25f;
                        num420 = Main.masterMode ? 0.13f : 0.115f;
                    }

                    if (Main.getGoodWorld)
                    {
                        num419 *= 1.15f;
                        num420 *= 1.15f;
                    }

                    int num421 = 1;
                    if (npc.Center.X < Main.player[npc.target].position.X + (float)Main.player[npc.target].width)
                        num421 = -1;

                    Vector2 vector43 = npc.Center;
                    float num422 = Main.player[npc.target].Center.X + (float)(num421 * 300) - vector43.X;
                    float num423 = Main.player[npc.target].Center.Y - 300f - vector43.Y;
                    if (NPC.IsMechQueenUp)
                    {
                        num419 = 14f;
                        num422 = vector41.X;
                        num423 = vector41.Y;
                        num422 -= vector43.X;
                        num423 -= vector43.Y;
                    }

                    float num424 = (float)Math.Sqrt(num422 * num422 + num423 * num423);
                    float num425 = num424;
                    if (NPC.IsMechQueenUp)
                    {
                        if (num424 > num419)
                        {
                            num424 = num419 / num424;
                            num422 *= num424;
                            num423 *= num424;
                        }

                        float num426 = 60f;
                        npc.velocity.X = (npc.velocity.X * (num426 - 1f) + num422) / num426;
                        npc.velocity.Y = (npc.velocity.Y * (num426 - 1f) + num423) / num426;
                    }
                    else
                    {
                        num424 = num419 / num424;
                        num422 *= num424;
                        num423 *= num424;
                        if (npc.velocity.X < num422)
                        {
                            npc.velocity.X += num420;
                            if (npc.velocity.X < 0f && num422 > 0f)
                                npc.velocity.X += num420;
                        }
                        else if (npc.velocity.X > num422)
                        {
                            npc.velocity.X -= num420;
                            if (npc.velocity.X > 0f && num422 < 0f)
                                npc.velocity.X -= num420;
                        }

                        if (npc.velocity.Y < num423)
                        {
                            npc.velocity.Y += num420;
                            if (npc.velocity.Y < 0f && num423 > 0f)
                                npc.velocity.Y += num420;
                        }
                        else if (npc.velocity.Y > num423)
                        {
                            npc.velocity.Y -= num420;
                            if (npc.velocity.Y > 0f && num423 < 0f)
                                npc.velocity.Y -= num420;
                        }
                    }

                    int num427 = 600;
                    int num428 = 60;
                    if (NPC.IsMechQueenUp)
                    {
                        num427 = 1200;
                        num428 = ((!NPC.npcsFoundForCheckActive[NPCID.TheDestroyerBody]) ? 90 : 120);
                    }

                    if (Main.expertMode)
                        num428 = (int)(num428 * 0.8f);

                    if (Main.masterMode)
                        num427 /= 2;

                    npc.ai[2] += 1f;
                    if (npc.ai[2] >= (float)num427)
                    {
                        npc.ai[1] = 1f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        
                        CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                        options.aggroRatio = -1f;
                        CalamityUtils.CalamityTargeting(npc, options);

                        npc.netUpdate = true;
                    }
                    else if (npc.position.Y + (float)npc.height < Main.player[npc.target].position.Y && num425 < (Main.masterMode ? 720f : Main.expertMode ? 560f : 400f))
                    {
                        if (!Main.player[npc.target].dead)
                        {
                            npc.ai[3] += 1f;

                            if (Main.expertMode)
                                npc.ai[3] += MathHelper.Lerp(0f, Main.masterMode ? 2.5f : 1.5f, 1f - (npc.life / (float)npc.lifeMax - phase2LifeRatio) / (1f - phase2LifeRatio));

                            if (Main.getGoodWorld)
                                npc.ai[3] += 0.5f;
                        }

                        if (npc.ai[3] >= (float)num428)
                        {
                            npc.ai[3] = 0f;
                            vector43 = npc.Center;
                            num422 = Main.player[npc.target].Center.X - vector43.X;
                            num423 = Main.player[npc.target].Center.Y - vector43.Y;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float num429 = Main.expertMode ? 10.5f : 9f;
                                int type = ProjectileID.EyeLaser;
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                                    double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                num424 = (float)Math.Sqrt(num422 * num422 + num423 * num423);
                                num424 = num429 / num424;
                                num422 *= num424;
                                num423 *= num424;
                                int inaccuracy = Main.masterMode ? 3 : Main.expertMode ? 6 : 9;
                                num422 += (float)Main.rand.Next(-inaccuracy, inaccuracy + 1) * 0.08f;
                                num423 += (float)Main.rand.Next(-inaccuracy, inaccuracy + 1) * 0.08f;

                                Vector2 laserVelocity = new Vector2(num422, num423);
                                Projectile.NewProjectile(npc.GetSource_FromAI(), vector43 + laserVelocity.SafeNormalize(Vector2.UnitY) * 90f, laserVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }
                else if (npc.ai[1] == 1f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.rotation = num414;
                    float num432 = 12f;
                    if (Main.expertMode)
                        num432 = Main.masterMode ? 18f : 15f;

                    if (Main.getGoodWorld)
                        num432 += 2f;

                    Vector2 vector44 = npc.Center;
                    float num433 = Main.player[npc.target].Center.X - vector44.X;
                    float num434 = Main.player[npc.target].Center.Y - vector44.Y;
                    float num435 = (float)Math.Sqrt(num433 * num433 + num434 * num434);
                    num435 = num432 / num435;
                    npc.velocity.X = num433 * num435;
                    npc.velocity.Y = num434 * num435;
                    npc.ai[1] = 2f;
                }
                else if (npc.ai[1] == 2f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.ai[2] += 1f;
                    float decelerateGateValue = Main.masterMode ? 38f : Main.expertMode ? 32f : 25f;
                    if (npc.ai[2] >= decelerateGateValue)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        float decelerationMultiplier = Main.masterMode ? 0.78f : Main.expertMode ? 0.92f : 0.96f;
                        npc.velocity *= decelerationMultiplier;
                        if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                            npc.velocity.X = 0f;

                        if ((double)npc.velocity.Y > -0.1 && (double)npc.velocity.Y < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - MathHelper.PiOver2;

                    float delayBeforeChargingAgain = Main.masterMode ? 47f : Main.expertMode ? 56f : 70f;
                    if (npc.ai[2] >= delayBeforeChargingAgain)
                    {
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;
                        npc.rotation = num414;
                        float numCharges = Main.masterMode ? 6f : Main.expertMode ? 5f : 4f;
                        if (npc.ai[3] >= numCharges)
                        {
                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            CalamityUtils.CalamityTargeting(npc, options);

                            npc.ai[1] = 0f;
                            npc.ai[3] = 0f;
                        }
                        else
                            npc.ai[1] = 1f;
                    }
                }

                if (npc.life < npc.lifeMax * phase2LifeRatio)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    
                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.aggroRatio = -1f;
                    CalamityUtils.CalamityTargeting(npc, options);
                    
                    npc.netUpdate = true;
                }

                return false;
            }

            if (npc.ai[0] == 1f || npc.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                if (NPC.IsMechQueenUp)
                    npc.reflectsProjectiles = true;

                if (npc.ai[0] == 1f)
                {
                    npc.ai[2] += 0.005f;
                    if ((double)npc.ai[2] > 0.5)
                        npc.ai[2] = 0.5f;
                }
                else
                {
                    npc.ai[2] -= 0.005f;
                    if (npc.ai[2] < 0f)
                        npc.ai[2] = 0f;
                }

                npc.rotation += npc.ai[2];

                npc.ai[1] += 1f;
                if (Main.masterMode && npc.ai[2] >= 0.2f)
                {
                    if (npc.ai[1] % 10f == 0f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.EyeLaser;
                            int damage = npc.GetProjectileDamage(type);

                            // Reduce mech boss projectile damage depending on the new ore progression changes
                            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                            {
                                double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                                double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                                if (!NPC.downedMechBossAny)
                                    damage = (int)(damage * firstMechMultiplier);
                                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                    damage = (int)(damage * secondMechMultiplier);
                            }

                            Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * 7f;
                            int numProj = 3;
                            int spread = 10;
                            float rotation = MathHelper.ToRadians(spread);
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 90f, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                                Main.projectile[proj].tileCollide = false;
                            }
                        }
                    }
                }

                if (npc.ai[1] >= 100f)
                {
                    npc.ai[0] += 1f;
                    npc.ai[1] = 0f;
                    if (npc.ai[0] == 3f)
                    {
                        npc.ai[2] = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);
                        for (int num436 = 0; num436 < 2; num436++)
                        {
                            Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 143);
                            Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 7);
                            Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 6);
                        }

                        for (int num437 = 0; num437 < 20; num437++)
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, (float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f);

                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                    }
                }

                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, (float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f);

                npc.velocity *= 0.98f;
                if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                    npc.velocity.X = 0f;

                if ((double)npc.velocity.Y > -0.1 && (double)npc.velocity.Y < 0.1)
                    npc.velocity.Y = 0f;

                return false;
            }

            // Avoid cheap bullshit
            npc.damage = 0;

            npc.defense = npc.defDefense + 10;
            npc.HitSound = SoundID.NPCHit4;

            if (npc.ai[1] == 0f)
            {
                float num438 = 8f;
                float num439 = 0.15f;
                if (Main.expertMode)
                {
                    num438 = Main.masterMode ? 11f : 9.5f;
                    num439 = Main.masterMode ? 0.2f : 0.175f;
                }

                if (Main.getGoodWorld)
                {
                    num438 *= 1.15f;
                    num439 *= 1.15f;
                }

                Vector2 vector45 = npc.Center;
                float num440 = Main.player[npc.target].Center.X - vector45.X;
                float num441 = Main.player[npc.target].Center.Y - 420f - vector45.Y;
                if (NPC.IsMechQueenUp)
                {
                    num438 = 14f;
                    num440 = vector41.X;
                    num441 = vector41.Y;
                    num440 -= vector45.X;
                    num441 -= vector45.Y;
                }

                float num442 = (float)Math.Sqrt(num440 * num440 + num441 * num441);
                if (NPC.IsMechQueenUp)
                {
                    if (num442 > num438)
                    {
                        num442 = num438 / num442;
                        num440 *= num442;
                        num441 *= num442;
                    }

                    npc.velocity.X = (npc.velocity.X * 4f + num440) / 5f;
                    npc.velocity.Y = (npc.velocity.Y * 4f + num441) / 5f;
                }
                else
                {
                    num442 = num438 / num442;
                    num440 *= num442;
                    num441 *= num442;
                    if (npc.velocity.X < num440)
                    {
                        npc.velocity.X += num439;
                        if (npc.velocity.X < 0f && num440 > 0f)
                            npc.velocity.X += num439;
                    }
                    else if (npc.velocity.X > num440)
                    {
                        npc.velocity.X -= num439;
                        if (npc.velocity.X > 0f && num440 < 0f)
                            npc.velocity.X -= num439;
                    }

                    if (npc.velocity.Y < num441)
                    {
                        npc.velocity.Y += num439;
                        if (npc.velocity.Y < 0f && num441 > 0f)
                            npc.velocity.Y += num439;
                    }
                    else if (npc.velocity.Y > num441)
                    {
                        npc.velocity.Y -= num439;
                        if (npc.velocity.Y > 0f && num441 < 0f)
                            npc.velocity.Y -= num439;
                    }
                }

                int num443 = 300;
                if (NPC.IsMechQueenUp)
                    num443 = 1200;

                if (Main.masterMode)
                    num443 = (int)(num443 * 0.8f);

                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)num443)
                {
                    npc.ai[1] = 1f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    
                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.aggroRatio = -1f;
                    CalamityUtils.CalamityTargeting(npc, options);

                    npc.netUpdate = true;
                }

                vector45 = npc.Center;
                num440 = Main.player[npc.target].Center.X - vector45.X;
                num441 = Main.player[npc.target].Center.Y - vector45.Y;
                npc.rotation = (float)Math.Atan2(num441, num440) - MathHelper.PiOver2;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return false;

                npc.localAI[1] += 1f;

                npc.localAI[1] += MathHelper.Lerp(0f, Main.masterMode ? 7f : Main.expertMode ? 6f : 5f, 1f - (npc.life / (float)npc.lifeMax) / phase2LifeRatio);

                if (npc.localAI[1] > 180f && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    npc.localAI[1] = 0f;
                    float num444 = Main.expertMode ? 10f : 8.5f;
                    int type = ProjectileID.DeathLaser;
                    int damage = npc.GetProjectileDamage(type);

                    // Reduce mech boss projectile damage depending on the new ore progression changes
                    if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                    {
                        double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                        double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                        if (!NPC.downedMechBossAny)
                            damage = (int)(damage * firstMechMultiplier);
                        else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                            damage = (int)(damage * secondMechMultiplier);
                    }

                    num442 = (float)Math.Sqrt(num440 * num440 + num441 * num441);
                    num442 = num444 / num442;
                    num440 *= num442;
                    num441 *= num442;

                    Vector2 laserVelocity = new Vector2(num440, num441);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), vector45 + laserVelocity.SafeNormalize(Vector2.UnitY) * 120f, laserVelocity, type, damage, 0f, Main.myPlayer);
                }

                return false;
            }

            int num447 = 1;
            if (npc.Center.X < Main.player[npc.target].position.X + (float)Main.player[npc.target].width)
                num447 = -1;

            float num448 = 8f;
            float num449 = 0.2f;
            if (Main.expertMode)
            {
                num448 = Main.masterMode ? 11f : 9.5f;
                num449 = Main.masterMode ? 0.3f : 0.25f;
            }

            if (Main.getGoodWorld)
            {
                num448 *= 1.15f;
                num449 *= 1.15f;
            }

            Vector2 vector46 = npc.Center;
            float num450 = Main.player[npc.target].Center.X + (num447 * 420f) - vector46.X;
            float num451 = Main.player[npc.target].Center.Y - vector46.Y;
            float num452 = (float)Math.Sqrt(num450 * num450 + num451 * num451);
            num452 = num448 / num452;
            num450 *= num452;
            num451 *= num452;
            if (npc.velocity.X < num450)
            {
                npc.velocity.X += num449;
                if (npc.velocity.X < 0f && num450 > 0f)
                    npc.velocity.X += num449;
            }
            else if (npc.velocity.X > num450)
            {
                npc.velocity.X -= num449;
                if (npc.velocity.X > 0f && num450 < 0f)
                    npc.velocity.X -= num449;
            }

            if (npc.velocity.Y < num451)
            {
                npc.velocity.Y += num449;
                if (npc.velocity.Y < 0f && num451 > 0f)
                    npc.velocity.Y += num449;
            }
            else if (npc.velocity.Y > num451)
            {
                npc.velocity.Y -= num449;
                if (npc.velocity.Y > 0f && num451 < 0f)
                    npc.velocity.Y -= num449;
            }

            vector46 = npc.Center;
            num450 = Main.player[npc.target].Center.X - vector46.X;
            num451 = Main.player[npc.target].Center.Y - vector46.Y;
            npc.rotation = (float)Math.Atan2(num451, num450) - MathHelper.PiOver2;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.localAI[1] += 1f;

                npc.localAI[1] += MathHelper.Lerp(0f, Main.masterMode ? 6f : Main.expertMode ? 5.25f : 3.75f, 1f - (npc.life / (float)npc.lifeMax) / phase2LifeRatio);

                if (npc.localAI[1] > 60f && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    npc.localAI[1] = 0f;
                    float num453 = 9f;
                    int type = ProjectileID.DeathLaser;
                    int damage = (int)Math.Round(npc.GetProjectileDamage(type) * 0.75);

                    // Reduce mech boss projectile damage depending on the new ore progression changes
                    if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                    {
                        double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                        double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                        if (!NPC.downedMechBossAny)
                            damage = (int)(damage * firstMechMultiplier);
                        else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                            damage = (int)(damage * secondMechMultiplier);
                    }

                    num452 = (float)Math.Sqrt(num450 * num450 + num451 * num451);
                    num452 = num453 / num452;
                    num450 *= num452;
                    num451 *= num452;

                    Vector2 laserVelocity = new Vector2(num450, num451);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), vector46 + laserVelocity.SafeNormalize(Vector2.UnitY) * 120f, laserVelocity, type, damage, 0f, Main.myPlayer);
                }
            }

            npc.ai[2] += 1f;
            float rapidLaserPhaseGateValue = Main.masterMode ? 120f : Main.expertMode ? 150f : 180f;
            if (npc.ai[2] >= rapidLaserPhaseGateValue)
            {
                npc.ai[1] = 0f;
                npc.ai[2] = 0f;
                npc.ai[3] = 0f;
                
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                CalamityUtils.CalamityTargeting(npc, options);

                npc.netUpdate = true;
            }

            return false;
        }

        public static bool VanillaSpazmatismAI(NPC npc, Mod mod)
        {
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            float phase2LifeRatio = Main.masterMode ? 0.6f : 0.4f;

            bool dead3 = Main.player[npc.target].dead;
            float num456 = npc.Center.X - Main.player[npc.target].position.X - (float)(Main.player[npc.target].width / 2);
            float num457 = npc.position.Y + (float)npc.height - 59f - Main.player[npc.target].position.Y - (float)(Main.player[npc.target].height / 2);
            float num458 = (float)Math.Atan2(num457, num456) + MathHelper.PiOver2;
            if (num458 < 0f)
                num458 += MathHelper.TwoPi;
            else if ((double)num458 > MathHelper.TwoPi)
                num458 -= MathHelper.TwoPi;

            float num459 = 0.15f;
            if (NPC.IsMechQueenUp && npc.ai[0] == 3f && npc.ai[1] == 0f)
                num459 *= 0.25f;

            if (npc.rotation < num458)
            {
                if ((double)(num458 - npc.rotation) > MathHelper.Pi)
                    npc.rotation -= num459;
                else
                    npc.rotation += num459;
            }
            else if (npc.rotation > num458)
            {
                if ((double)(npc.rotation - num458) > MathHelper.Pi)
                    npc.rotation += num459;
                else
                    npc.rotation -= num459;
            }

            if (npc.rotation > num458 - num459 && npc.rotation < num458 + num459)
                npc.rotation = num458;

            if (npc.rotation < 0f)
                npc.rotation += MathHelper.TwoPi;
            else if ((double)npc.rotation > MathHelper.TwoPi)
                npc.rotation -= MathHelper.TwoPi;

            if (npc.rotation > num458 - num459 && npc.rotation < num458 + num459)
                npc.rotation = num458;

            if (Main.rand.NextBool(5))
            {
                int num460 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + (float)npc.height * 0.25f), npc.width, (int)((float)npc.height * 0.5f), DustID.Blood, npc.velocity.X, 2f);
                Main.dust[num460].velocity.X *= 0.5f;
                Main.dust[num460].velocity.Y *= 0.1f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && !Main.IsItDay() && !dead3 && npc.timeLeft < 10)
            {
                for (int num461 = 0; num461 < Main.maxNPCs; num461++)
                {
                    if (num461 != npc.whoAmI && Main.npc[num461].active && (Main.npc[num461].type == NPCID.Retinazer || Main.npc[num461].type == NPCID.Spazmatism))
                        npc.DiscourageDespawn(Main.npc[num461].timeLeft - 1);
                }
            }

            Vector2 vector47 = Vector2.Zero;
            if (NPC.IsMechQueenUp)
            {
                NPC nPC2 = Main.npc[NPC.mechQueen];
                Vector2 mechQueenCenter2 = nPC2.GetMechQueenCenter();
                Vector2 vector48 = new Vector2(150f, -250f);
                vector48 *= 0.75f;
                float num462 = nPC2.velocity.X * 0.025f;
                vector47 = mechQueenCenter2 + vector48;
                vector47 = vector47.RotatedBy(num462, mechQueenCenter2);
            }

            npc.reflectsProjectiles = false;
            if (Main.IsItDay() || dead3)
            {
                npc.velocity.Y -= 0.04f;
                npc.EncourageDespawn(10);
                return false;
            }

            if (npc.ai[0] == 0f)
            {
                if (npc.ai[1] == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    float num463 = 12f;
                    float num464 = 0.4f;
                    if (Main.expertMode)
                    {
                        num463 = Main.masterMode ? 13.2f : 12.6f;
                        num464 = Main.masterMode ? 0.48f : 0.44f;
                    }

                    if (Main.getGoodWorld)
                    {
                        num463 *= 1.15f;
                        num464 *= 1.15f;
                    }

                    int num465 = 1;
                    if (npc.Center.X < Main.player[npc.target].position.X + (float)Main.player[npc.target].width)
                        num465 = -1;

                    Vector2 vector49 = npc.Center;
                    float num466 = Main.player[npc.target].Center.X + (float)(num465 * 400) - vector49.X;
                    float num467 = Main.player[npc.target].Center.Y - vector49.Y;
                    if (NPC.IsMechQueenUp)
                    {
                        num463 = 14f;
                        num466 = vector47.X;
                        num467 = vector47.Y;
                        num466 -= vector49.X;
                        num467 -= vector49.Y;
                    }

                    float num468 = (float)Math.Sqrt(num466 * num466 + num467 * num467);
                    float num469 = num468;
                    if (NPC.IsMechQueenUp)
                    {
                        if (num468 > num463)
                        {
                            num468 = num463 / num468;
                            num466 *= num468;
                            num467 *= num468;
                        }

                        npc.velocity.X = (npc.velocity.X * 4f + num466) / 5f;
                        npc.velocity.Y = (npc.velocity.Y * 4f + num467) / 5f;
                    }
                    else
                    {
                        num468 = num463 / num468;
                        num466 *= num468;
                        num467 *= num468;
                        if (npc.velocity.X < num466)
                        {
                            npc.velocity.X += num464;
                            if (npc.velocity.X < 0f && num466 > 0f)
                                npc.velocity.X += num464;
                        }
                        else if (npc.velocity.X > num466)
                        {
                            npc.velocity.X -= num464;
                            if (npc.velocity.X > 0f && num466 < 0f)
                                npc.velocity.X -= num464;
                        }

                        if (npc.velocity.Y < num467)
                        {
                            npc.velocity.Y += num464;
                            if (npc.velocity.Y < 0f && num467 > 0f)
                                npc.velocity.Y += num464;
                        }
                        else if (npc.velocity.Y > num467)
                        {
                            npc.velocity.Y -= num464;
                            if (npc.velocity.Y > 0f && num467 < 0f)
                                npc.velocity.Y -= num464;
                        }
                    }

                    int num470 = 600;
                    if (NPC.IsMechQueenUp)
                        num470 = 1200;

                    if (Main.masterMode)
                        num470 = (int)(num470 * 0.75f);

                    npc.ai[2] += 1f;
                    if (npc.ai[2] >= (float)num470)
                    {
                        npc.ai[1] = 1f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        if (!Main.player[npc.target].dead)
                        {
                            npc.ai[3] += 1f;

                            if (Main.expertMode)
                                npc.ai[3] += MathHelper.Lerp(0f, Main.masterMode ? 1.8f : 1.2f, 1f - (npc.life / (float)npc.lifeMax - phase2LifeRatio) / (1f - phase2LifeRatio));

                            if (Main.getGoodWorld)
                                npc.ai[3] += 0.4f;
                        }

                        if (npc.ai[3] >= 60f)
                        {
                            npc.ai[3] = 0f;
                            vector49 = npc.Center;
                            num466 = Main.player[npc.target].Center.X - vector49.X;
                            num467 = Main.player[npc.target].Center.Y - vector49.Y;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float num471 = Main.expertMode ? 14f : 12f;
                                int type = ProjectileID.CursedFlameHostile;
                                int damage = npc.GetProjectileDamage(type);

                                // Reduce mech boss projectile damage depending on the new ore progression changes
                                if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                                {
                                    double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                                    double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                                    if (!NPC.downedMechBossAny)
                                        damage = (int)(damage * firstMechMultiplier);
                                    else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                        damage = (int)(damage * secondMechMultiplier);
                                }

                                num468 = (float)Math.Sqrt(num466 * num466 + num467 * num467);
                                num468 = num471 / num468;
                                num466 *= num468;
                                num467 *= num468;
                                int inaccuracy = Main.masterMode ? 10 : Main.expertMode ? 15 : 20;
                                num466 += (float)Main.rand.Next(-inaccuracy, inaccuracy + 1) * 0.05f;
                                num467 += (float)Main.rand.Next(-inaccuracy, inaccuracy + 1) * 0.05f;

                                Vector2 fireballVelocity = new Vector2(num466, num467);
                                Projectile.NewProjectile(npc.GetSource_FromAI(), vector49 + fireballVelocity.SafeNormalize(Vector2.UnitY) * 50f, fireballVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }
                else if (npc.ai[1] == 1f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.rotation = num458;
                    float num474 = 13f;

                    if (Main.expertMode)
                        num474 += MathHelper.Lerp(0f, Main.masterMode ? 11.2f : 5.6f, 1f - (npc.life / (float)npc.lifeMax - phase2LifeRatio) / (1f - phase2LifeRatio));

                    if (Main.getGoodWorld)
                        num474 *= 1.2f;

                    Vector2 vector50 = npc.Center;
                    float num475 = Main.player[npc.target].Center.X - vector50.X;
                    float num476 = Main.player[npc.target].Center.Y - vector50.Y;
                    float num477 = (float)Math.Sqrt(num475 * num475 + num476 * num476);
                    num477 = num474 / num477;
                    npc.velocity.X = num475 * num477;
                    npc.velocity.Y = num476 * num477;
                    npc.ai[1] = 2f;
                }
                else if (npc.ai[1] == 2f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.ai[2] += 1f;
                    float decelerateGateValue = Main.masterMode ? 12f : Main.expertMode ? 10f : 8f;
                    if (npc.ai[2] >= decelerateGateValue)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        float decelerationMultiplier = Main.masterMode ? 0.78f : Main.expertMode ? 0.85f : 0.9f;
                        npc.velocity *= decelerationMultiplier;
                        if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                            npc.velocity.X = 0f;

                        if ((double)npc.velocity.Y > -0.1 && (double)npc.velocity.Y < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - MathHelper.PiOver2;

                    float delayBeforeChargingAgain = Main.masterMode ? 28f : Main.expertMode ? 34f : 42f;
                    if (npc.ai[2] >= delayBeforeChargingAgain)
                    {
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;
                        npc.rotation = num458;
                        float numCharges = Main.masterMode ? 6f : Main.expertMode ? 8f : 10f;
                        if (npc.ai[3] >= numCharges)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[3] = 0f;
                        }
                        else
                            npc.ai[1] = 1f;
                    }
                }

                if (npc.life < npc.lifeMax * phase2LifeRatio)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;

                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.finishThemOff = true;
                    CalamityUtils.CalamityTargeting(npc, options);

                    npc.netUpdate = true;
                }

                return false;
            }

            if (npc.ai[0] == 1f || npc.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                if (NPC.IsMechQueenUp)
                    npc.reflectsProjectiles = true;

                if (npc.ai[0] == 1f)
                {
                    npc.ai[2] += 0.005f;
                    if ((double)npc.ai[2] > 0.5)
                        npc.ai[2] = 0.5f;
                }
                else
                {
                    npc.ai[2] -= 0.005f;
                    if (npc.ai[2] < 0f)
                        npc.ai[2] = 0f;
                }

                npc.rotation += npc.ai[2];

                npc.ai[1] += 1f;
                if (Main.masterMode && npc.ai[2] >= 0.2f)
                {
                    if (npc.ai[1] % 10f == 0f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.CursedFlameHostile;
                            int damage = npc.GetProjectileDamage(type);

                            // Reduce mech boss projectile damage depending on the new ore progression changes
                            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                            {
                                double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                                double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                                if (!NPC.downedMechBossAny)
                                    damage = (int)(damage * firstMechMultiplier);
                                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                    damage = (int)(damage * secondMechMultiplier);
                            }

                            Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * 16f + Main.rand.NextVector2CircularEdge(3f, 3f);
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 50f, projectileVelocity, type, damage, 0f, Main.myPlayer);
                            Main.projectile[proj].tileCollide = false;
                        }
                    }
                }

                if (npc.ai[1] >= 100f)
                {
                    npc.ai[0] += 1f;
                    npc.ai[1] = 0f;
                    if (npc.ai[0] == 3f)
                    {
                        npc.ai[2] = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);
                        for (int num478 = 0; num478 < 2; num478++)
                        {
                            Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 144);
                            Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 7);
                            Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 6);
                        }

                        for (int num479 = 0; num479 < 20; num479++)
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, (float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f);

                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                    }
                }

                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, (float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f);

                npc.velocity *= 0.98f;
                if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                    npc.velocity.X = 0f;

                if ((double)npc.velocity.Y > -0.1 && (double)npc.velocity.Y < 0.1)
                    npc.velocity.Y = 0f;

                return false;
            }

            npc.HitSound = SoundID.NPCHit4;
            int setDamage = (int)Math.Round(npc.defDamage * 1.5);
            int reducedSetDamage = (int)Math.Round(setDamage * 0.5);
            npc.defense = npc.defDefense + 18;

            if (npc.ai[1] == 0f)
            {
                // Bites your fucking ass
                npc.damage = reducedSetDamage;

                float num480 = 4f;
                float num481 = Main.masterMode ? 0.12f : 0.1f;
                int num482 = 1;
                if (npc.Center.X < Main.player[npc.target].position.X + (float)Main.player[npc.target].width)
                    num482 = -1;

                Vector2 vector51 = npc.Center;
                int flamethrowerDistance = 180;
                float num483 = Main.player[npc.target].Center.X + (float)(num482 * flamethrowerDistance) - vector51.X;
                float num484 = Main.player[npc.target].Center.Y - vector51.Y;
                float num485 = (float)Math.Sqrt(num483 * num483 + num484 * num484);
                if (!NPC.IsMechQueenUp)
                {
                    if (num485 > flamethrowerDistance)
                        num480 += MathHelper.Lerp(0f, Main.masterMode ? 8f : 6f, MathHelper.Clamp((num485 - flamethrowerDistance) / 1000f, 0f, 1f));

                    if (Main.getGoodWorld)
                    {
                        num480 *= 1.15f;
                        num481 *= 1.15f;
                    }

                    num485 = num480 / num485;
                    num483 *= num485;
                    num484 *= num485;
                    if (npc.velocity.X < num483)
                    {
                        npc.velocity.X += num481;
                        if (npc.velocity.X < 0f && num483 > 0f)
                            npc.velocity.X += num481;
                    }
                    else if (npc.velocity.X > num483)
                    {
                        npc.velocity.X -= num481;
                        if (npc.velocity.X > 0f && num483 < 0f)
                            npc.velocity.X -= num481;
                    }

                    if (npc.velocity.Y < num484)
                    {
                        npc.velocity.Y += num481;
                        if (npc.velocity.Y < 0f && num484 > 0f)
                            npc.velocity.Y += num481;
                    }
                    else if (npc.velocity.Y > num484)
                    {
                        npc.velocity.Y -= num481;
                        if (npc.velocity.Y > 0f && num484 < 0f)
                            npc.velocity.Y -= num481;
                    }
                }

                int num486 = 400;
                if (NPC.IsMechQueenUp)
                    num486 = 1200;

                if (Main.masterMode)
                    num486 = (int)(num486 * 0.8f);

                npc.ai[2] += 1f;
                if (npc.ai[2] >= (float)num486)
                {
                    npc.ai[1] = 1f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;
                }

                if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    npc.localAI[2] += 1f;
                    if (npc.localAI[2] > 22f)
                    {
                        npc.localAI[2] = 0f;
                        SoundEngine.PlaySound(SoundID.Item34, npc.Center);
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.localAI[1] += 1f;

                        npc.localAI[1] += MathHelper.Lerp(0f, 5f, 1f - (npc.life / (float)npc.lifeMax) / phase2LifeRatio);

                        if (npc.localAI[1] > 8f)
                        {
                            npc.localAI[1] = 0f;
                            float num487 = 6f + (Main.expertMode ? MathHelper.Lerp(0f, Main.masterMode ? 3f : 1.5f, 1f - (npc.life / (float)npc.lifeMax) / phase2LifeRatio) : 0f);
                            float timeForFlamethrowerToReachMaxVelocity = 60f;
                            float flamethrowerSpeedScalar = MathHelper.Clamp(npc.ai[2] / timeForFlamethrowerToReachMaxVelocity, 0f, 1f);
                            num487 = MathHelper.Lerp(0.1f, num487, flamethrowerSpeedScalar);
                            int type = ModContent.ProjectileType<CursedFire>();
                            int damage = npc.GetProjectileDamage(type);

                            // Reduce mech boss projectile damage depending on the new ore progression changes
                            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
                            {
                                double firstMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkFirstMechStatMultiplier_Classic;
                                double secondMechMultiplier = Main.expertMode ? CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Expert : CalamityGlobalNPC.EarlyHardmodeProgressionReworkSecondMechStatMultiplier_Classic;
                                if (!NPC.downedMechBossAny)
                                    damage = (int)(damage * firstMechMultiplier);
                                else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
                                    damage = (int)(damage * secondMechMultiplier);
                            }

                            vector51 = npc.Center;
                            num483 = Main.player[npc.target].Center.X - vector51.X;
                            num484 = Main.player[npc.target].Center.Y - vector51.Y;
                            num485 = (float)Math.Sqrt(num483 * num483 + num484 * num484);
                            num485 = num487 / num485;
                            num483 *= num485;
                            num484 *= num485;
                            int inaccuracy = Main.masterMode ? 10 : Main.expertMode ? 15 : 20;
                            num484 += (float)Main.rand.Next(-inaccuracy, inaccuracy + 1) * 0.01f;
                            num483 += (float)Main.rand.Next(-inaccuracy, inaccuracy + 1) * 0.01f;
                            num484 += npc.velocity.Y * 0.5f;
                            num483 += npc.velocity.X * 0.5f;
                            
                            if (NPC.IsMechQueenUp)
                            {
                                Vector2 vector52 = (npc.rotation + (float)Math.PI / 2f).ToRotationVector2() * num487 + npc.velocity * 0.5f;
                                num483 = vector52.X;
                                num484 = vector52.Y;
                                vector51 = npc.Center - vector52 * 3f;
                            }

                            Vector2 flamethrowerVelocity = new Vector2(num483, num484);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), vector51 + flamethrowerVelocity.SafeNormalize(Vector2.UnitY) * 50f, flamethrowerVelocity, type, damage, 0f, Main.myPlayer);
                        }
                    }
                }

                if (NPC.IsMechQueenUp)
                {
                    num480 = 14f;
                    num483 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector51.X;
                    num484 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - 300f - vector51.Y;
                    num483 = vector47.X;
                    num484 = vector47.Y;
                    num483 -= vector51.X;
                    num484 -= vector51.Y;
                    num485 = (float)Math.Sqrt(num483 * num483 + num484 * num484);
                    if (num485 > num480)
                    {
                        num485 = num480 / num485;
                        num483 *= num485;
                        num484 *= num485;
                    }

                    int num490 = 60;
                    npc.velocity.X = (npc.velocity.X * (float)(num490 - 1) + num483) / (float)num490;
                    npc.velocity.Y = (npc.velocity.Y * (float)(num490 - 1) + num484) / (float)num490;
                }
            }
            else if (npc.ai[1] == 1f)
            {
                // Set damage
                npc.damage = setDamage;

                SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                npc.rotation = num458;
                float num491 = 14f;
                if (Main.expertMode)
                    num491 += 2.5f;
                if (Main.masterMode)
                    num491 += 3.5f;

                Vector2 vector53 = npc.Center;
                float num492 = Main.player[npc.target].Center.X - vector53.X;
                float num493 = Main.player[npc.target].Center.Y - vector53.Y;
                float num494 = (float)Math.Sqrt(num492 * num492 + num493 * num493);
                num494 = num491 / num494;
                npc.velocity.X = num492 * num494;
                npc.velocity.Y = num493 * num494;
                npc.ai[1] = 2f;
            }
            else
            {
                if (npc.ai[1] != 2f)
                    return false;

                // Set damage
                npc.damage = setDamage;

                npc.ai[2] += 1f;
                if (Main.expertMode)
                    npc.ai[2] += 0.5f;
                if (Main.masterMode)
                    npc.ai[2] += 0.5f;

                if (npc.ai[2] >= 50f)
                {
                    // Bites your fucking ass
                    npc.damage = reducedSetDamage;

                    npc.velocity *= 0.93f;
                    if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                        npc.velocity.X = 0f;

                    if ((double)npc.velocity.Y > -0.1 && (double)npc.velocity.Y < 0.1)
                        npc.velocity.Y = 0f;
                }
                else
                    npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - MathHelper.PiOver2;

                if (npc.ai[2] >= 80f)
                {
                    npc.ai[3] += 1f;
                    npc.ai[2] = 0f;
                    npc.rotation = num458;
                    float numCharges = Main.masterMode ? Main.rand.Next(4, 7) : 6f;
                    if (npc.ai[3] >= numCharges)
                    {
                        npc.ai[1] = 0f;
                        npc.ai[3] = 0f;
                    }
                    else
                        npc.ai[1] = 1f;

                    // Due to the random number of charges in Master Mode
                    if (Main.masterMode)
                        npc.netUpdate = true;
                }
            }

            return false;
        }
    }
}
