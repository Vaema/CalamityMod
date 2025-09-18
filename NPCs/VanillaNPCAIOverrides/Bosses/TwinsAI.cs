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
        // Vanilla values
        public static float RapidFireDamageMult = 0.75f; // 80 (buffed)
        public static float RetinazerPhase2ContactDamageMult = 1.5f; // 114
        public static float SpazmatismPhase2ContactDamageMult = 1.5f; // 127
        public static int FlamethrowerDamage = 27; // 108; Applies to both flamethrower types
        public static int FireballDamage = 22; // 88; Applies to both fireball types

        // Rev+ exclusive
        public static int LaserDamage = 21; // 84 (buffed)
        public static int RedLaserDamage = 27; // 108 (buffed)
        public static int HomingDartDamage = 27; // 108

        public static int CalculateMechDamage(this int damage)
        {
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
            return damage;
        }

        public static bool BuffedRetinazerAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                CalamityUtils.CalamityTargeting(npc, options);
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
                    if (i != npc.whoAmI && Main.npc[i].active && (Main.npc[i].type == NPCID.Retinazer || Main.npc[i].type == NPCID.Spazmatism) && Main.npc[i].timeLeft - 1 > npc.timeLeft)
                        npc.timeLeft = Main.npc[i].timeLeft - 1;
                }
            }

            // Phase HP ratios
            float phase2LifeRatio = death ? 0.85f : 0.7f;
            float finalPhaseLifeRatio = death ? 0.4f : 0.25f;

            // Movement variables
            float phase1MaxSpeedIncrease = death ? 2f : 4f;
            float phase1MaxAccelerationIncrease = death ? 0.025f : 0.05f;
            float phase1MaxChargeSpeedIncrease = death ? 3f : 6f;

            // Phase duration variables
            float phase1MaxLaserPhaseDurationDecrease = death ? 120f : 300f;

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
            if ((Main.player[npc.target].dead || Main.IsItDay()) && !BossRushEvent.BossRushActive)
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
                    float maxVelocity = (death ? 9.45f : 8.25f);
                    float acceleration = (death ? 0.13f : 0.115f);

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
                    
                    float phaseGateValue = death ? (300f - phase1MaxLaserPhaseDurationDecrease * ((1f - lifeRatio) / (1f - phase2LifeRatio))) : 450f;
                    float laserGateValue = 30f;
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
                                float laserSpeed = death ? 11.4f : 10.5f;

                                int type = ProjectileID.EyeLaser;
                                int damage = LaserDamage.CalculateMechDamage();
                                Vector2 laserVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserSpeed;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 90f, laserVelocity, type, damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                else if (npc.ai[1] == 1f)
                {
                    npc.rotation = hoverRotation;

                    float chargeSpeed = death ? 18f : 15f;
                    if (death)
                        chargeSpeed += phase1MaxChargeSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio));
                    if (Main.getGoodWorld)
                        chargeSpeed += 2f;

                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;

                    npc.ai[1] = 2f;
                }
                else if (npc.ai[1] == 2f)
                {
                    npc.ai[2] += 1f;
                    float decelerateGateValue = (death ? 36f : 32f) + (death ? 6f * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (npc.ai[2] >= decelerateGateValue)
                    {
                        float decelerationMultiplier = (death ? 0.84f : 0.92f) - (death ? 0.16f * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                        npc.velocity *= decelerationMultiplier;

                        if (Math.Abs(npc.velocity.X) < 0.1)
                            npc.velocity.X = 0f;
                        if (Math.Abs(npc.velocity.Y) < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

                    float delayBeforeChargingAgain = (death ? 48f : 56f) - (death ? 3f * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 0f);
                    if (npc.ai[2] >= delayBeforeChargingAgain)
                    {
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;

                        npc.rotation = hoverRotation;

                        float totalCharges = death ? 6f : 5f;

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
                if (death && npc.ai[2] >= 0.2f)
                {
                    if (npc.ai[1] % 10f == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item33, npc.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.DeathLaser;

                            Vector2 projectileVelocity = npc.rotation.ToRotationVector2() * 7f;
                            float offset = 90f;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + Vector2.Normalize(projectileVelocity) * offset, projectileVelocity, type, LaserDamage.CalculateMechDamage(), 0f, Main.myPlayer);
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

                npc.damage = (int)Math.Round(npc.defDamage * RetinazerPhase2ContactDamageMult);
                npc.defense = npc.defDefense + 10;
                calamityGlobalNPC.DR = spazInPhase1 ? 0.9999f : 0.2f;
                calamityGlobalNPC.unbreakableDR = spazInPhase1;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = spazInPhase1;

                npc.HitSound = SoundID.NPCHit4;

                if (npc.ai[1] == 0f)
                {
                    float maxVelocity = death ? 10.85f + 3f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 9.5f;
                    float acceleration = death ? 0.1975f + 0.05f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0.175f;

                    if (Main.getGoodWorld)
                    {
                        maxVelocity *= 1.15f;
                        acceleration *= 1.15f;
                    }

                    float distanceFromTarget = 420f;
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
                        if (npc.localAI[1] >= (spazAlive ? 52f : 26f))
                        {
                            if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                            {
                                npc.localAI[1] = 0f;

                                float laserSpeed = 10f;
                                int type = ProjectileID.DeathLaser;

                                Vector2 laserVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserSpeed;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 120f, laserVelocity, type, RedLaserDamage.CalculateMechDamage(), 0f, Main.myPlayer);
                            }
                        }
                    }
                }
                else
                {
                    if (npc.ai[1] == 1f)
                    {
                        float maxVelocity = death ? 10.85f + 3f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 9.5f;
                        float acceleration = death ? 0.295f + 0.075f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0.25f;

                        if (Main.getGoodWorld)
                        {
                            maxVelocity *= 1.15f;
                            acceleration *= 1.15f;
                        }

                        float distanceFromTarget = 420f;
                        Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * distanceFromTarget * direction;
                        float distanceFromDestination = (destination - npc.Center).Length();
                        Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);
                        npc.SimpleFlyMovement(idealVelocity * maxVelocity, acceleration);

                        npc.rotation = (Main.player[npc.target].Center - npc.Center).ToRotation() - MathHelper.PiOver2;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            npc.localAI[1] += 1f + (death ? (phase2LifeRatio - lifeRatio) / phase2LifeRatio : 0f);
                            if (npc.localAI[1] > (spazAlive ? 20f : 10f))
                            {
                                if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                                {
                                    npc.localAI[1] = 0f;

                                    float laserSpeed = 9f;
                                    int type = ProjectileID.DeathLaser;
                                    int damage = (int)Math.Round(RedLaserDamage.CalculateMechDamage() * RapidFireDamageMult);

                                    Vector2 laserVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * laserSpeed;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 120f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                }
                            }
                        }

                        npc.ai[2] += spazAlive ? 1f : 1.5f;
                        if (npc.ai[2] >= (death ? 150f : 180f) - (death ? 60f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
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
                        // Set rotation and velocity
                        npc.rotation = hoverRotation;

                        float chargeSpeed = (death ? 25f : 22f) + (death ? 8f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        if (!spazAlive)
                            chargeSpeed += 2f;
                        if (Main.getGoodWorld)
                            chargeSpeed += 2f;

                        npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * chargeSpeed;

                        npc.ai[1] = 3f;
                    }

                    else if (npc.ai[1] == 3f)
                    {
                        npc.ai[2] += 1f;

                        float chargeTime = spazAlive ? 45f : 30f;
                        if (npc.ai[3] % 3f == 0f)
                            chargeTime = spazAlive ? 90f : 60f;
                        if (death)
                            chargeTime -= chargeTime * 0.25f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio);
                        chargeTime -= chargeTime * (death ? 0.06f : 0f);

                        // Slow down
                        if (npc.ai[2] >= chargeTime)
                        {
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
                                float fireRate = spazAlive ? 13f : 9f;
                                if (npc.ai[2] % fireRate == 0f)
                                {
                                    SoundEngine.PlaySound(SoundID.Item33, npc.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        float laserDartSpeed = (death ? 9f : 6f) * (spazAlive ? 1f : 1.5f);
                                        int type = ModContent.ProjectileType<HomingLaserDart>();
                                        int damage = HomingDartDamage.CalculateMechDamage();

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
                        chargeGateValue -= chargeGateValue * (death ? 0.07f : 0f);
                        if (npc.ai[2] >= chargeTime + chargeGateValue)
                        {
                            npc.ai[2] = 0f;

                            float chargeIncrement = 1f;
                            if (death && Main.rand.NextBool() && npc.ai[3] < (spazAlive ? 1f : 3f))
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
                        float chargeLineUpDistance = spazAlive ? 600f : 500f;
                        float chargeSpeed = (death ? 19.8f : 18f) + (death ? 6f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float chargeAcceleration = (death ? 0.495f : 0.45f) + (death ? 0.15f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

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

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(npc, options);
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
                    if (i != npc.whoAmI && Main.npc[i].active && (Main.npc[i].type == NPCID.Retinazer || Main.npc[i].type == NPCID.Spazmatism) && Main.npc[i].timeLeft - 1 > npc.timeLeft)
                        npc.timeLeft = Main.npc[i].timeLeft - 1;
                }
            }

            // Phase HP ratios
            float phase2LifeRatio = death ? 0.85f : 0.7f;
            float finalPhaseLifeRatio = death ? 0.3f : 0.15f;

            // Movement variables
            float phase1MaxSpeedIncrease = death ? 2.25f : 4.5f;
            float phase1MaxAccelerationIncrease = death ? 0.075f : 0.15f;
            float phase1MaxChargeSpeedIncrease = death ? 3f : 6f;

            // Phase duration variables
            float phase1MaxCursedFlamePhaseDurationDecrease = death ? 80f : 200f;
            float phase1MaxChargesDecrease = death ? 2f : 4f;

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
            if ((Main.player[npc.target].dead || Main.IsItDay()) && !BossRushEvent.BossRushActive)
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
                    // Velocity
                    float maxVelocity = death ? 13.8f : 12f;
                    float acceleration = death ? 0.46f : 0.4f;

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

                    float distanceFromTarget = 400f;
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

                        if (npc.ai[3] >= 30f)
                        {
                            npc.ai[3] = 0f;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float cursedFireballSpeed = death ? 15.9f : 15f;
                                int type = ProjectileID.CursedFlameHostile;

                                Vector2 fireballVelocity = ((Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * cursedFireballSpeed) + new Vector2(Main.rand.Next(-10, 11), Main.rand.Next(-10, 11)) * 0.05f;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + fireballVelocity.SafeNormalize(Vector2.UnitY) * 50f, fireballVelocity, type, FireballDamage.CalculateMechDamage(), 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                // Charging phase
                else if (npc.ai[1] == 1f)
                {
                    // Rotation and velocity
                    npc.rotation = hoverRotation;

                    float chargeSpeed = death ? 20.4f + phase1MaxChargeSpeedIncrease * ((1f - lifeRatio) / (1f - phase2LifeRatio)) : 18f;
                    if (Main.getGoodWorld)
                        chargeSpeed *= 1.2f;

                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;

                    npc.ai[1] = 2f;
                }
                else if (npc.ai[1] == 2f)
                {
                    npc.ai[2] += 1f;

                    float timeBeforeSlowDown = death ? 30f : 10f;
                    if (npc.ai[2] >= timeBeforeSlowDown)
                    {
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
                    float chargeTime = death ? 45f : 25f;
                    if (npc.ai[2] >= chargeTime)
                    {
                        // Reset AI array and go to cursed fireball phase
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;

                        npc.rotation = hoverRotation;

                        float totalCharges = 8f;
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
                if (death && npc.ai[2] >= 0.2f)
                {
                    if (npc.ai[1] % 10f == 0f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = npc.ai[1] % 20f == 0f ? ProjectileID.CursedFlameHostile : ModContent.ProjectileType<ShadowflameFireball>();

                            Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * 16f + Main.rand.NextVector2CircularEdge(3f, 3f);
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 50f, projectileVelocity, type, FireballDamage.CalculateMechDamage(), 0f, Main.myPlayer, 0f, 1f);
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
                npc.damage = (int)Math.Round(npc.defDamage * SpazmatismPhase2ContactDamageMult);
                npc.defense = npc.defDefense + 18;
                calamityGlobalNPC.DR = retInPhase1 ? 0.9999f : 0.2f;
                calamityGlobalNPC.unbreakableDR = retInPhase1;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = retInPhase1;

                // Change hit sound to metal
                npc.HitSound = SoundID.NPCHit4;

                // Shadowflamethrower phase
                if (npc.ai[1] == 0f)
                {
                    float maxVelocity = (death ? 7.1f : 6.2f) + (death ? 2f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                    float acceleration = (death ? 0.118f : 0.1f) + (death ? 0.03f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

                    float distanceFromTarget = 180f;
                    Vector2 destination = Main.player[npc.target].Center + Vector2.UnitX * distanceFromTarget * direction;
                    float distanceFromDestination = (destination - npc.Center).Length();
                    Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitX * direction);

                    if (!NPC.IsMechQueenUp)
                    {
                        // Boost speed if too far from target
                        if (distanceFromDestination > distanceFromTarget)
                            maxVelocity += MathHelper.Lerp(0f, death ? 8f : 6f, MathHelper.Clamp((distanceFromDestination - distanceFromTarget) / 1000f, 0f, 1f));

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

                                float flamethrowerSpeed = death ? 6.9f : 6f;
                                float timeForFlamethrowerToReachMaxVelocity = 60f;
                                float flamethrowerSpeedScalar = MathHelper.Clamp(npc.ai[2] / timeForFlamethrowerToReachMaxVelocity, 0f, 1f);
                                flamethrowerSpeed = MathHelper.Lerp(0.1f, flamethrowerSpeed, flamethrowerSpeedScalar);
                                int type = npc.ai[3] % 2f == 0f ? ModContent.ProjectileType<CursedFire>() : ModContent.ProjectileType<Shadowflamethrower>();

                                Vector2 flamethrowerVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * flamethrowerSpeed + npc.velocity * 0.5f;

                                if (NPC.IsMechQueenUp)
                                    flamethrowerVelocity = (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * flamethrowerSpeed + npc.velocity * 0.5f;

                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + flamethrowerVelocity.SafeNormalize(Vector2.UnitY) * 25f, flamethrowerVelocity, type, FlamethrowerDamage.CalculateMechDamage(), 0f, Main.myPlayer);
                                if (death && npc.ai[3] % 30f == 0f)
                                {
                                    type = npc.ai[3] % 60f == 0f ? ModContent.ProjectileType<ShadowflameFireball>() : ProjectileID.CursedFlameHostile;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + flamethrowerVelocity.SafeNormalize(Vector2.UnitY) * 50f, flamethrowerVelocity * 2f, type, FireballDamage.CalculateMechDamage(), 0f, Main.myPlayer);
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
                        // Play charge sound
                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                        // Set rotation and velocity
                        npc.rotation = hoverRotation;

                        float chargeSpeed = (death ? 22.8f : 18f) + (death ? 5f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        if (Main.getGoodWorld)
                            chargeSpeed *= 1.2f;

                        npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitX * direction) * chargeSpeed;

                        npc.ai[1] = 2f;

                        return false;
                    }

                    if (npc.ai[1] == 2f)
                    {
                        npc.ai[2] += retAlive ? 1f : 1.25f;

                        float chargeTime = 30f - (death ? 10f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

                        // Slow down
                        if (npc.ai[2] >= chargeTime)
                        {
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
                        if (npc.ai[2] >= chargeTime * 1.6f)
                        {
                            npc.ai[3] += 1f;
                            npc.ai[2] = 0f;

                            npc.rotation = hoverRotation;

                            float totalCharges = 5f;
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
                        // Reset AI array and go to shadowflamethrower phase or fireball phase if ret is dead
                        float secondFastCharge = 4f;
                        if (npc.ai[3] >= (retAlive ? secondFastCharge : secondFastCharge + 1f))
                        {
                            npc.ai[1] = retAlive ? 0f : 5f;
                            npc.ai[2] = 0f;
                            npc.ai[3] = 0f;

                            if (npc.ai[1] == 0f)
                                npc.localAI[1] = -20f;

                            npc.ForceNetUpdate(false);
                        }

                        // Set charging velocity
                        else if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            // Velocity
                            float spazmatismPhase3ChargeSpeed = (death ? 23f : 20f) + (death ? 6f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                            if (npc.ai[2] == -1f || (!retAlive && npc.ai[3] == secondFastCharge))
                                spazmatismPhase3ChargeSpeed *= 1.3f;
                            if (Main.getGoodWorld)
                                spazmatismPhase3ChargeSpeed *= 1.2f;

                            Vector2 distanceVector = Main.player[npc.target].Center - npc.Center;
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

                                Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                                int numProj = 3;
                                int spread = 15;
                                float rotation = MathHelper.ToRadians(spread);

                                for (int i = 0; i < numProj; i++)
                                {
                                    Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 25f, perturbedSpeed, type, FireballDamage.CalculateMechDamage(), 0f, Main.myPlayer);
                                }
                            }

                            npc.ai[1] = 4f;
                            npc.ForceNetUpdate(false);
                        }
                    }

                    // Crazy charge
                    else if (npc.ai[1] == 4f)
                    {
                        if (npc.ai[2] == 0f)
                            SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                        float spazmatismRetDeadChargeSpeed = ((!retAlive && npc.ai[3] == 4f) ? 75f : 50f) - (float)Math.Round(death ? ((!retAlive && npc.ai[3] == 4f) ? 15f : 10f) * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

                        npc.ai[2] += 1f;

                        if (npc.ai[2] == spazmatismRetDeadChargeSpeed && Vector2.Distance(npc.position, Main.player[npc.target].position) < (retAlive ? 200f : 150f))
                            npc.ai[2] -= 1f;

                        // Slow down
                        if (npc.ai[2] >= spazmatismRetDeadChargeSpeed)
                        {
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
                            npc.ForceNetUpdate(false);

                            float chargeIncrement = 1f;
                            if (death && Main.rand.NextBool() && npc.ai[3] < (retAlive ? 2f : 3f))
                                chargeIncrement = 2f;

                            npc.ai[3] += chargeIncrement;
                            npc.ai[2] = 0f;
                            npc.ai[1] = 3f;
                        }
                    }

                    // Get in position for charge
                    else if (npc.ai[1] == 5f)
                    {
                        float chargeLineUpDistance = retAlive ? 600f : 500f;
                        float chargeSpeed = (death ? 17.5f : 16f) + (death ? 5f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);
                        float chargeAcceleration = (death ? 0.44f : 0.4f) + (death ? 0.1f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f);

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
                        float fireRate = retAlive ? 30f : 20f;
                        if (npc.ai[2] % fireRate == 0f)
                        {
                            npc.ai[3] += 1f;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float projectileSpeed = 16f;
                                int type = npc.ai[3] % 2f == 0f ? ProjectileID.CursedFlameHostile : ModContent.ProjectileType<ShadowflameFireball>();

                                Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 25f, projectileVelocity, type, FireballDamage.CalculateMechDamage(), 0f, Main.myPlayer, 0f, retAlive ? 0f : 1f);
                            }
                        }

                        // Take 3 seconds to get in position, then charge
                        if (npc.ai[2] >= (retAlive ? 180f : 135f) - (death ? 45f * ((phase2LifeRatio - lifeRatio) / phase2LifeRatio) : 0f))
                        {
                            npc.ai[1] = 3f;
                            npc.ai[2] = -1f;
                            npc.ai[3] = 0f;
                        }

                        npc.ForceNetUpdate(false);
                    }
                }
            }

            return false;
        }
    }
}
