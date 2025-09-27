using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class SkeletronPrimeAI
    {
        // Vanilla values
        public static int SpinDamageMult = 2; // 158
        public static int LaserDamage = 25; // 100

        // Rev+ exclusive
        public static int SkullDamage = 22; // 88
        public static int RocketDamage = 30; // 120

        public static bool BuffedSkeletronPrimeAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            if (npc.ai[3] != 0f)
                NPC.mechQueen = npc.whoAmI;

            // Spawn arms
            if (calamityGlobalNPC.newAI[1] == 0f)
            {
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                calamityGlobalNPC.newAI[1] = 1f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int arm = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeCannon, npc.whoAmI);
                    Main.npc[arm].ai[0] = -1f;
                    Main.npc[arm].ai[1] = npc.whoAmI;
                    Main.npc[arm].target = npc.target;
                    Main.npc[arm].netUpdate = true;

                    arm = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeSaw, npc.whoAmI);
                    Main.npc[arm].ai[0] = 1f;
                    Main.npc[arm].ai[1] = npc.whoAmI;
                    Main.npc[arm].target = npc.target;
                    Main.npc[arm].netUpdate = true;

                    arm = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeVice, npc.whoAmI);
                    Main.npc[arm].ai[0] = -1f;
                    Main.npc[arm].ai[1] = npc.whoAmI;
                    Main.npc[arm].target = npc.target;
                    Main.npc[arm].ai[3] = 150f;
                    Main.npc[arm].netUpdate = true;

                    arm = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeLaser, npc.whoAmI);
                    Main.npc[arm].ai[0] = 1f;
                    Main.npc[arm].ai[1] = npc.whoAmI;
                    Main.npc[arm].target = npc.target;
                    Main.npc[arm].netUpdate = true;
                    Main.npc[arm].ai[3] = 150f;
                }

                npc.netUpdate = true;
                npc.SyncExtraAI();
            }

            // Check if arms are alive
            bool cannonAlive = false;
            bool laserAlive = false;
            bool viceAlive = false;
            bool sawAlive = false;
            if (CalamityGlobalNPC.primeCannon != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeCannon].active)
                    cannonAlive = true;
            }
            if (CalamityGlobalNPC.primeLaser != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeLaser].active)
                    laserAlive = true;
            }
            if (CalamityGlobalNPC.primeVice != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeVice].active)
                    viceAlive = true;
            }
            if (CalamityGlobalNPC.primeSaw != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeSaw].active)
                    sawAlive = true;
            }
            bool allArmsDead = !cannonAlive && !laserAlive && !viceAlive && !sawAlive;
            npc.chaseable = allArmsDead;

            npc.defense = npc.defDefense;
            npc.damage = npc.defDamage;

            // Phases
            bool phase2 = lifeRatio < 0.66f;
            bool phase3 = lifeRatio < 0.33f;

            // Despawn
            if (npc.ai[1] != 3f)
            {
                int despawnDistanceInTiles = 500;
                if (Main.player[npc.target].dead || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > despawnDistanceInTiles)
                {
                    CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                    if (Main.player[npc.target].dead || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > despawnDistanceInTiles)
                        npc.ai[1] = 3f;
                }
                else if (npc.timeLeft < 1800)
                    npc.timeLeft = 1800;
            }

            // Activate daytime enrage
            if (Main.IsItDay() && !BossRushEvent.BossRushActive && npc.ai[1] != 3f && npc.ai[1] != 2f)
            {
                // Heal
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int healAmt = npc.life - 300;
                    if (healAmt < 0)
                    {
                        int absHeal = Math.Abs(healAmt);
                        npc.life += absHeal;
                        npc.HealEffect(absHeal, true);
                        npc.netUpdate = true;
                    }
                }

                npc.ai[1] = 2f;
                SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
            }

            // Adjust slowing debuff immunity
            bool immuneToSlowingDebuffs = npc.ai[1] == 5f;
            npc.buffImmune[ModContent.BuffType<GlacialState>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<TemporalSadness>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<Eutrophication>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<TimeDistortion>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<GalvanicCorrosion>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<Vaporfied>()] = immuneToSlowingDebuffs;
            npc.buffImmune[BuffID.Webbed] = immuneToSlowingDebuffs;

            bool normalLaserRotation = npc.localAI[1] % 2f == 0f;

            // Float near player
            if (npc.ai[1] == 0f || npc.ai[1] == 4f)
            {
                // Start other phases; if arms are dead, start with spin phase
                if (phase2 || Main.getGoodWorld || allArmsDead)
                {
                    // Start spin phase after 1.5 seconds
                    npc.ai[2] += phase3 ? 1.5f : 1f;
                    if (npc.ai[2] >= (90f - (death ? 15f * (1f - lifeRatio) : 0f)))
                    {
                        bool shouldSpinAround = npc.ai[1] == 4f && npc.position.Y < Main.player[npc.target].position.Y - 320f &&
                            Vector2.Distance(Main.player[npc.target].Center, npc.Center) < 600f && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 400f;

                        if (shouldSpinAround || npc.ai[1] != 4f)
                        {
                            if (shouldSpinAround)
                            {
                                npc.localAI[3] = 300f;
                                npc.SyncVanillaLocalAI();
                            }

                            npc.ai[2] = 0f;
                            npc.ai[1] = shouldSpinAround ? 5f : 1f;
                            CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                            npc.netUpdate = true;
                        }
                    }
                }

                if (NPC.IsMechQueenUp)
                    npc.rotation = npc.rotation.AngleLerp(npc.velocity.X / 15f * 0.5f, 0.75f);
                else
                    npc.rotation = npc.velocity.X / 15f;

                float acceleration = death ? (0.12f + 0.05f * (1f - lifeRatio)) : 0.1f;
                float accelerationMult = 1f;
                if (!cannonAlive)
                {
                    acceleration += 0.0125f;
                    accelerationMult += 0.25f;
                }
                if (!laserAlive)
                {
                    acceleration += 0.0125f;
                    accelerationMult += 0.25f;
                }
                if (!viceAlive)
                    acceleration += 0.0125f;
                if (!sawAlive)
                    acceleration += 0.0125f;
                if (death)
                    acceleration *= accelerationMult;

                float topVelocity = acceleration * 100f;
                float deceleration = death ? 0.7f : 0.85f;

                float headDecelerationUpDist = 0f;
                float headDecelerationDownDist = 0f;
                float headDecelerationHorizontalDist = 0f;
                int headHorizontalDirection = ((!(Main.player[npc.target].Center.X < npc.Center.X)) ? 1 : (-1));
                if (NPC.IsMechQueenUp)
                {
                    headDecelerationHorizontalDist = -150f * (float)headHorizontalDirection;
                    headDecelerationUpDist = -100f;
                    headDecelerationDownDist = -100f;
                }

                if (npc.position.Y > Main.player[npc.target].position.Y - (320f + headDecelerationUpDist))
                {
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y *= deceleration;

                    npc.velocity.Y -= acceleration;

                    if (npc.velocity.Y > topVelocity)
                        npc.velocity.Y = topVelocity;
                }
                else if (npc.position.Y < Main.player[npc.target].position.Y - (360f + headDecelerationDownDist))
                {
                    if (npc.velocity.Y < 0f)
                        npc.velocity.Y *= deceleration;

                    npc.velocity.Y += acceleration;

                    if (npc.velocity.Y < -topVelocity)
                        npc.velocity.Y = -topVelocity;
                }

                if (npc.Center.X > Main.player[npc.target].Center.X + (400f + headDecelerationHorizontalDist))
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X *= deceleration;

                    npc.velocity.X -= acceleration;

                    if (npc.velocity.X > topVelocity)
                        npc.velocity.X = topVelocity;
                }
                if (npc.Center.X < Main.player[npc.target].Center.X - (400f + headDecelerationHorizontalDist))
                {
                    if (npc.velocity.X < 0f)
                        npc.velocity.X *= deceleration;

                    npc.velocity.X += acceleration;

                    if (npc.velocity.X < -topVelocity)
                        npc.velocity.X = -topVelocity;
                }
            }

            else
            {
                // Spinning
                if (npc.ai[1] == 1f)
                {
                    npc.defense = npc.defDefense * 2;
                    npc.damage = npc.defDamage * SpinDamageMult;

                    calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = true;

                    if (phase2 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.localAI[0] += 1f;
                        if (npc.localAI[0] >= 45f)
                        {
                            npc.localAI[0] = 0f;

                            int totalProjectiles = death ? 15 : 12;
                            float radians = MathHelper.TwoPi / totalProjectiles;
                            int type = ProjectileID.DeathLaser;

                            float velocity = 3f;
                            double angleA = radians * 0.5;
                            double angleB = MathHelper.ToRadians(90f) - angleA;
                            float velocityX = (float)(velocity * Math.Sin(angleA) / Math.Sin(angleB));
                            Vector2 spinningPoint = normalLaserRotation ? new Vector2(0f, -velocity) : new Vector2(-velocityX, -velocity);
                            for (int k = 0; k < totalProjectiles; k++)
                            {
                                Vector2 laserFireDirection = spinningPoint.RotatedBy(radians * k);
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserFireDirection.SafeNormalize(Vector2.UnitY) * 100f, laserFireDirection, type, LaserDamage.CalculateMechDamage(), 0f, Main.myPlayer, 1f, 0f);
                                Main.projectile[proj].timeLeft = 900;
                            }
                            npc.localAI[1] += 1f;
                        }
                    }

                    npc.ai[2] += 1f;
                    if (npc.ai[2] == 2f)
                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                    // Spin for 3 seconds then return to floating phase
                    float phaseTimer = 240f;
                    if (phase2 && !phase3)
                        phaseTimer += 60f;

                    if (npc.ai[2] >= (phaseTimer - (death ? 60f * (1f - lifeRatio) : 0f)))
                    {
                        CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                        npc.ai[2] = 0f;
                        npc.ai[1] = 4f;
                        npc.localAI[0] = 0f;
                    }

                    if (NPC.IsMechQueenUp)
                        npc.rotation = npc.rotation.AngleLerp(npc.velocity.X / 15f * 0.5f, 0.75f);
                    else
                        npc.rotation += npc.direction * 0.3f;

                    Vector2 headPosition = npc.Center;
                    float headTargetX = Main.player[npc.target].Center.X - headPosition.X;
                    float headTargetY = Main.player[npc.target].Center.Y - headPosition.Y;
                    float headTargetDistance = (float)Math.Sqrt(headTargetX * headTargetX + headTargetY * headTargetY);

                    float speed = death ? 8f : 6f;
                    if (phase2)
                        speed += 0.5f;
                    if (phase3)
                        speed += 0.5f;

                    if (headTargetDistance > 150f)
                    {
                        float baseDistanceVelocityMult = 1f + MathHelper.Clamp((headTargetDistance - 150f) * 0.0015f, 0.05f, 1.5f);
                        speed *= baseDistanceVelocityMult;
                    }

                    if (NPC.IsMechQueenUp)
                    {
                        float mechdusaSpeedMult = (NPC.npcsFoundForCheckActive[NPCID.TheDestroyerBody] ? 0.6f : 0.75f);
                        speed *= mechdusaSpeedMult;
                    }

                    headTargetDistance = speed / headTargetDistance;
                    npc.velocity.X = headTargetX * headTargetDistance;
                    npc.velocity.Y = headTargetY * headTargetDistance;

                    if (NPC.IsMechQueenUp)
                    {
                        float mechdusaAccelMult = Vector2.Distance(npc.Center, Main.player[npc.target].Center);
                        if (mechdusaAccelMult < 0.1f)
                            mechdusaAccelMult = 0f;

                        if (mechdusaAccelMult < speed)
                            npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * mechdusaAccelMult;
                    }
                }

                // Daytime enrage
                if (npc.ai[1] == 2f)
                {
                    npc.damage = 1000;
                    calamityGlobalNPC.DR = 0.9999f;
                    calamityGlobalNPC.unbreakableDR = true;

                    calamityGlobalNPC.CurrentlyEnraged = true;
                    calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = true;

                    if (NPC.IsMechQueenUp)
                        npc.rotation = npc.rotation.AngleLerp(npc.velocity.X / 15f * 0.5f, 0.75f);
                    else
                        npc.rotation += npc.direction * 0.3f;

                    Vector2 enragedHeadPosition = npc.Center;
                    float enragedHeadTargetX = Main.player[npc.target].Center.X - enragedHeadPosition.X;
                    float enragedHeadTargetY = Main.player[npc.target].Center.Y - enragedHeadPosition.Y;
                    float enragedHeadTargetDist = (float)Math.Sqrt(enragedHeadTargetX * enragedHeadTargetX + enragedHeadTargetY * enragedHeadTargetY);

                    float enragedHeadSpeed = 10f;
                    enragedHeadSpeed += enragedHeadTargetDist / 100f;
                    if (enragedHeadSpeed < 8f)
                        enragedHeadSpeed = 8f;
                    if (enragedHeadSpeed > 32f)
                        enragedHeadSpeed = 32f;

                    enragedHeadTargetDist = enragedHeadSpeed / enragedHeadTargetDist;
                    npc.velocity.X = enragedHeadTargetX * enragedHeadTargetDist;
                    npc.velocity.Y = enragedHeadTargetY * enragedHeadTargetDist;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.localAI[0] += 1f;
                        if (npc.localAI[0] >= 60f)
                        {
                            npc.localAI[0] = 0f;
                            Vector2 headCenter = npc.Center;
                            if (Collision.CanHit(headCenter, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                            {
                                enragedHeadSpeed = 7f;
                                float enragedHeadSkullTargetX = Main.player[npc.target].Center.X - headCenter.X + Main.rand.Next(-20, 21);
                                float enragedHeadSkullTargetY = Main.player[npc.target].Center.Y - headCenter.Y + Main.rand.Next(-20, 21);
                                float enragedHeadSkullTargetDist = (float)Math.Sqrt(enragedHeadSkullTargetX * enragedHeadSkullTargetX + enragedHeadSkullTargetY * enragedHeadSkullTargetY);
                                enragedHeadSkullTargetDist = enragedHeadSpeed / enragedHeadSkullTargetDist;
                                enragedHeadSkullTargetX *= enragedHeadSkullTargetDist;
                                enragedHeadSkullTargetY *= enragedHeadSkullTargetDist;

                                Vector2 value = new Vector2(enragedHeadSkullTargetX * 1f + Main.rand.Next(-50, 51) * 0.01f, enragedHeadSkullTargetY * 1f + Main.rand.Next(-50, 51) * 0.01f).SafeNormalize(Vector2.UnitY);
                                value *= enragedHeadSpeed;
                                value += npc.velocity;
                                enragedHeadSkullTargetX = value.X;
                                enragedHeadSkullTargetY = value.Y;

                                int type = ProjectileID.Skull;
                                headCenter += value * 5f;
                                int enragedSkulls = Projectile.NewProjectile(npc.GetSource_FromAI(), headCenter.X, headCenter.Y, enragedHeadSkullTargetX, enragedHeadSkullTargetY, type, 250, 0f, Main.myPlayer, -3f, 0f);
                                Main.projectile[enragedSkulls].timeLeft = 300;
                            }
                        }
                    }
                }

                // Despawning
                if (npc.ai[1] == 3f)
                {
                    if (NPC.IsMechQueenUp)
                    {
                        int mechdusaBossDespawning = NPC.FindFirstNPC(NPCID.Retinazer);
                        if (mechdusaBossDespawning >= 0)
                            Main.npc[mechdusaBossDespawning].EncourageDespawn(5);

                        mechdusaBossDespawning = NPC.FindFirstNPC(NPCID.Spazmatism);
                        if (mechdusaBossDespawning >= 0)
                            Main.npc[mechdusaBossDespawning].EncourageDespawn(5);

                        if (!NPC.AnyNPCs(NPCID.Retinazer) && !NPC.AnyNPCs(NPCID.Spazmatism))
                        {
                            mechdusaBossDespawning = NPC.FindFirstNPC(NPCID.TheDestroyer);
                            if (mechdusaBossDespawning >= 0)
                                Main.npc[mechdusaBossDespawning].Transform(NPCID.TheDestroyerTail);

                            npc.EncourageDespawn(5);
                        }

                        npc.velocity.Y += 0.1f;
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= 0.95f;

                        npc.velocity.X *= 0.95f;
                        if (npc.velocity.Y > 13f)
                            npc.velocity.Y = 13f;
                    }
                    else
                    {
                        npc.velocity.Y += 0.1f;
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= 0.9f;

                        npc.velocity.X *= 0.9f;

                        if (npc.timeLeft > 500)
                            npc.timeLeft = 500;
                    }
                }

                // Fly around in a circle
                if (npc.ai[1] == 5f)
                {
                    npc.ai[2] += 1f;

                    npc.rotation = npc.velocity.X / 50f;

                    float skullSpawnDivisor = death ? 15f - (float)Math.Round(3f * (1f - lifeRatio)) : 15f;
                    float totalSkulls = 12f;
                    int skullSpread = death ? 125 : 100;

                    // Spin for about 3 seconds
                    // Decreasing this number will INCREASE how fast he moves while spinning
                    float spinVelocity = 30f;
                    if (npc.ai[2] == 2f)
                    {
                        // Play angry noise
                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                        // Set spin direction
                        if (Main.player[npc.target].velocity.X > 0f)
                            calamityGlobalNPC.newAI[0] = 1f;
                        else if (Main.player[npc.target].velocity.X < 0f)
                            calamityGlobalNPC.newAI[0] = -1f;
                        else
                            calamityGlobalNPC.newAI[0] = Main.player[npc.target].direction;

                        // Set spin velocity
                        npc.velocity.X = MathHelper.Pi * npc.localAI[3] / spinVelocity;
                        npc.velocity *= -calamityGlobalNPC.newAI[0];
                        npc.SyncExtraAI();
                        npc.netUpdate = true;
                    }

                    // Maintain velocity and spit skulls
                    else if (npc.ai[2] > 2f)
                    {
                        npc.velocity = npc.velocity.RotatedBy(MathHelper.Pi / spinVelocity * -calamityGlobalNPC.newAI[0]);
                        if (npc.ai[2] == 3f)
                            npc.velocity *= 0.6f;

                        if (npc.ai[2] % skullSpawnDivisor == 0f)
                        {
                            npc.localAI[0] += 1f;

                            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 96f)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 headCenter = npc.Center;
                                    float enragedHeadSpeed = death ? (5f + (1f - lifeRatio)) : 4f;
                                    float enragedHeadSkullTargetX = Main.player[npc.target].Center.X - headCenter.X + Main.rand.Next(-20, 21);
                                    float enragedHeadSkullTargetY = Main.player[npc.target].Center.Y - headCenter.Y + Main.rand.Next(-20, 21);
                                    float enragedHeadSkullTargetDist = (float)Math.Sqrt(enragedHeadSkullTargetX * enragedHeadSkullTargetX + enragedHeadSkullTargetY * enragedHeadSkullTargetY);
                                    enragedHeadSkullTargetDist = enragedHeadSpeed / enragedHeadSkullTargetDist;
                                    enragedHeadSkullTargetX *= enragedHeadSkullTargetDist;
                                    enragedHeadSkullTargetY *= enragedHeadSkullTargetDist;

                                    Vector2 value = new Vector2(enragedHeadSkullTargetX + Main.rand.Next(-skullSpread, skullSpread + 1) * 0.01f, enragedHeadSkullTargetY + Main.rand.Next(-skullSpread, skullSpread + 1) * 0.01f).SafeNormalize(Vector2.UnitY);
                                    value *= enragedHeadSpeed;
                                    enragedHeadSkullTargetX = value.X;
                                    enragedHeadSkullTargetY = value.Y;

                                    int type = ProjectileID.Skull;

                                    int enragedSkulls = Projectile.NewProjectile(npc.GetSource_FromAI(), headCenter.X, headCenter.Y + 30f, enragedHeadSkullTargetX, enragedHeadSkullTargetY, type, SkullDamage.CalculateMechDamage(), 0f, Main.myPlayer, -3f, 0f);
                                    Main.projectile[enragedSkulls].timeLeft = 480;
                                    Main.projectile[enragedSkulls].tileCollide = false;
                                }
                            }

                            // Go to floating phase, or spinning phase if in phase 2
                            if (npc.localAI[0] >= totalSkulls)
                            {
                                npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY);

                                // Fly overhead and spit missiles if on low health
                                npc.ai[1] = phase3 ? 6f : 1f;
                                npc.ai[2] = 0f;
                                npc.localAI[3] = 0f;
                                npc.localAI[0] = 0f;
                                calamityGlobalNPC.newAI[0] = 0f;
                                npc.SyncVanillaLocalAI();
                                npc.SyncExtraAI();
                                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                                npc.netUpdate = true;
                            }
                        }
                    }
                }

                // Fly overhead and spit missiles
                if (npc.ai[1] == 6f)
                {
                    npc.rotation = npc.velocity.X / 15f;

                    float flightVelocity = death ? 25f : 18f;
                    float flightAcceleration = death ? 0.96f : 0.6f;

                    Vector2 destination = new Vector2(Main.player[npc.target].Center.X, Main.player[npc.target].Center.Y - 420f);
                    npc.SimpleFlyMovement((destination - npc.Center).SafeNormalize(Vector2.UnitY) * flightVelocity, flightAcceleration);

                    // Spit homing missiles and then go to floating phase
                    npc.localAI[3] += 1f;
                    if (Vector2.Distance(npc.Center, destination) < 80f || npc.ai[2] > 0f || npc.localAI[3] > 120f)
                    {
                        float missileSpawnDivisor = 12f;
                        float totalMissiles = 10f;
                        npc.ai[2] += 1f;
                        if (npc.ai[2] % missileSpawnDivisor == 0f)
                        {
                            npc.localAI[0] += 1f;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 velocity = (-Vector2.UnitY * 3f).RotatedByRandom(MathHelper.Pi / 8f);
                                int type = ProjectileID.RocketSkeleton;

                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X + Main.rand.Next(npc.width / 2), npc.Center.Y + 30f, velocity.X, velocity.Y, type, RocketDamage.CalculateMechDamage(), 0f, Main.myPlayer, npc.target, 1f);
                                Main.projectile[proj].timeLeft = 540;
                            }

                            SoundEngine.PlaySound(SoundID.Item62, npc.Center);

                            if (npc.localAI[0] >= totalMissiles)
                            {
                                npc.ai[1] = 0f;
                                npc.ai[2] = 0f;
                                npc.localAI[3] = 0f;
                                calamityGlobalNPC.newAI[0] = 0f;
                                npc.localAI[0] = 0f;
                                npc.SyncVanillaLocalAI();
                                npc.SyncExtraAI();
                                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                                npc.netUpdate = true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool BuffedPrimeLaserAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            // Set direction
            npc.spriteDirection = -(int)npc.ai[0];

            // Despawn if head is gone
            if (!Main.npc[(int)npc.ai[1]].active || Main.npc[(int)npc.ai[1]].aiStyle != NPCAIStyleID.SkeletronPrimeHead)
            {
                npc.ai[2] += 10f;
                if (npc.ai[2] > 50f || !Main.dedServ)
                {
                    npc.life = -1;
                    npc.HitEffect(0, 10.0);
                    npc.active = false;
                }
            }

            CalamityGlobalNPC.primeLaser = npc.whoAmI;

            // Check if arms are alive
            bool cannonAlive = false;
            bool viceAlive = false;
            bool sawAlive = false;
            if (CalamityGlobalNPC.primeCannon != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeCannon].active)
                    cannonAlive = true;
            }
            if (CalamityGlobalNPC.primeVice != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeVice].active)
                    viceAlive = true;
            }
            if (CalamityGlobalNPC.primeSaw != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeSaw].active)
                    sawAlive = true;
            }

            // Inflict 0 damage for 3 seconds after spawning
            float timeToNotAttack = 180f;
            bool dontAttack = npc.Calamity().newAI[2] < timeToNotAttack;
            if (dontAttack)
            {
                npc.Calamity().newAI[2] += 1f;
                if (npc.Calamity().newAI[2] >= timeToNotAttack)
                    npc.SyncExtraAI();
            }

            bool normalLaserRotation = npc.localAI[1] % 2f == 0f;

            // Movement
            float acceleration = death ? 0.375f : 0.25f;
            float accelerationMult = 1f;
            if (!cannonAlive)
            {
                acceleration += 0.025f;
                accelerationMult += 0.5f;
            }
            if (!viceAlive)
                acceleration += 0.025f;
            if (!sawAlive)
                acceleration += 0.025f;
            if (death)
                acceleration *= accelerationMult;

            float topVelocity = acceleration * 100f;
            float deceleration = death ? 0.6f : 0.8f;

            if (npc.position.Y > Main.npc[(int)npc.ai[1]].position.Y - 70f)
            {
                if (npc.velocity.Y > 0f)
                    npc.velocity.Y *= deceleration;

                npc.velocity.Y -= acceleration;

                if (npc.velocity.Y > topVelocity)
                    npc.velocity.Y = topVelocity;
            }
            else if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y - 100f)
            {
                if (npc.velocity.Y < 0f)
                    npc.velocity.Y *= deceleration;

                npc.velocity.Y += acceleration;

                if (npc.velocity.Y < -topVelocity)
                    npc.velocity.Y = -topVelocity;
            }

            if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X - 130f * npc.ai[0])
            {
                if (npc.velocity.X > 0f)
                    npc.velocity.X *= deceleration;

                npc.velocity.X -= acceleration;

                if (npc.velocity.X > topVelocity)
                    npc.velocity.X = topVelocity;
            }
            if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 160f * npc.ai[0])
            {
                if (npc.velocity.X < 0f)
                    npc.velocity.X *= deceleration;

                npc.velocity.X += acceleration;

                if (npc.velocity.X < -topVelocity)
                    npc.velocity.X = -topVelocity;
            }

            // Phase 1
            if (npc.ai[2] == 0f)
            {
                // Despawn if head is despawning
                if (Main.npc[(int)npc.ai[1]].ai[1] == 3f && npc.timeLeft > 10)
                    npc.timeLeft = 10;

                // Go to other phase after 13.3 seconds (change this as each arm dies)
                npc.ai[3] += 1f;
                if (!cannonAlive)
                    npc.ai[3] += 1f;
                if (!viceAlive)
                    npc.ai[3] += 1f;
                if (!sawAlive)
                    npc.ai[3] += 1f;

                if (npc.ai[3] >= (death ? 200f : 800f))
                {
                    npc.target = Main.npc[(int)npc.ai[1]].target;
                    npc.localAI[0] = 0f;
                    npc.ai[2] = 1f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;
                }

                Vector2 laserArmPosition = npc.Center;
                float laserArmTargetX = Main.player[npc.target].Center.X - laserArmPosition.X;
                float laserArmTargetY = Main.player[npc.target].Center.Y - laserArmPosition.Y;
                float laserArmTargetDist = (float)Math.Sqrt(laserArmTargetX * laserArmTargetX + laserArmTargetY * laserArmTargetY);
                npc.rotation = (float)Math.Atan2(laserArmTargetY, laserArmTargetX) - MathHelper.PiOver2;

                if (Main.netMode != NetmodeID.MultiplayerClient && !dontAttack)
                {
                    // Fire laser every 0.8 seconds (change this as each arm dies to fire more aggressively)
                    npc.localAI[0] += 1f;
                    if (!cannonAlive)
                        npc.localAI[0] += 1f;
                    if (!viceAlive)
                        npc.localAI[0] += 1f;
                    if (!sawAlive)
                        npc.localAI[0] += 1f;

                    if (npc.localAI[0] >= 48f)
                    {
                        npc.localAI[0] = 0f;
                        float laserSpeed = 4f;
                        int type = ProjectileID.DeathLaser;

                        laserArmTargetDist = laserSpeed / laserArmTargetDist;
                        laserArmTargetX *= laserArmTargetDist;
                        laserArmTargetY *= laserArmTargetDist;
                        Vector2 laserVelocity = new Vector2(laserArmTargetX, laserArmTargetY);
                        Projectile.NewProjectile(npc.GetSource_FromAI(), laserArmPosition + laserVelocity.SafeNormalize(Vector2.UnitY) * 100f, laserVelocity, type, LaserDamage.CalculateMechDamage(), 0f, Main.myPlayer, 1f, 0f);
                    }
                }
            }

            // Other phase, get closer to the player and fire ring of lasers
            else if (npc.ai[2] == 1f)
            {
                // Go to phase 1 after 2 seconds (change this as each arm dies to stay in this phase for longer)
                npc.ai[3] += 1f;

                float timeLimit = 135f;
                float timeMult = 1.882075f;
                if (!cannonAlive)
                    timeLimit *= timeMult;
                if (!viceAlive)
                    timeLimit *= timeMult;
                if (!sawAlive)
                    timeLimit *= timeMult;

                if (npc.ai[3] >= timeLimit)
                {
                    npc.target = Main.npc[(int)npc.ai[1]].target;
                    npc.localAI[0] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;
                }

                Vector2 laserRingArmPosition = npc.Center;
                float laserRingTargetX = Main.player[npc.target].Center.X - laserRingArmPosition.X;
                float laserRingTargetY = Main.player[npc.target].Center.Y - laserRingArmPosition.Y;
                npc.rotation = (float)Math.Atan2(laserRingTargetY, laserRingTargetX) - MathHelper.PiOver2;

                if (Main.netMode != NetmodeID.MultiplayerClient && !dontAttack)
                {
                    // Fire laser every 1.5 seconds (change this as each arm dies to fire more aggressively)
                    npc.localAI[0] += 1f;
                    if (!cannonAlive)
                        npc.localAI[0] += 0.5f;
                    if (!viceAlive)
                        npc.localAI[0] += 0.5f;
                    if (!sawAlive)
                        npc.localAI[0] += 0.5f;

                    if (npc.localAI[0] >= 120f)
                    {
                        npc.localAI[0] = 0f;
                        int totalProjectiles = death ? 24 : 16;
                        float radians = MathHelper.TwoPi / totalProjectiles;
                        int type = ProjectileID.DeathLaser;

                        float velocity = 3f;
                        double angleA = radians * 0.5;
                        double angleB = MathHelper.ToRadians(90f) - angleA;
                        float laserVelocityX = (float)(velocity * Math.Sin(angleA) / Math.Sin(angleB));
                        Vector2 spinningPoint = normalLaserRotation ? new Vector2(0f, -velocity) : new Vector2(-laserVelocityX, -velocity);
                        for (int k = 0; k < totalProjectiles; k++)
                        {
                            Vector2 laserFireDirection = spinningPoint.RotatedBy(radians * k);
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserFireDirection.SafeNormalize(Vector2.UnitY) * 100f, laserFireDirection, type, LaserDamage.CalculateMechDamage(), 0f, Main.myPlayer, 1f, 0f);
                            Main.projectile[proj].timeLeft = 900;
                        }
                        npc.localAI[1] += 1f;
                    }
                }
            }

            return false;
        }

        public static bool BuffedPrimeCannonAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            npc.spriteDirection = -(int)npc.ai[0];

            if (!Main.npc[(int)npc.ai[1]].active || Main.npc[(int)npc.ai[1]].aiStyle != NPCAIStyleID.SkeletronPrimeHead)
            {
                npc.ai[2] += 10f;
                if (npc.ai[2] > 50f || !Main.dedServ)
                {
                    npc.life = -1;
                    npc.HitEffect(0, 10.0);
                    npc.active = false;
                }
            }

            CalamityGlobalNPC.primeCannon = npc.whoAmI;

            // Check if arms are alive
            bool laserAlive = false;
            bool viceAlive = false;
            bool sawAlive = false;
            if (CalamityGlobalNPC.primeLaser != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeLaser].active)
                    laserAlive = true;
            }
            if (CalamityGlobalNPC.primeVice != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeVice].active)
                    viceAlive = true;
            }
            if (CalamityGlobalNPC.primeSaw != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeSaw].active)
                    sawAlive = true;
            }

            // Inflict 0 damage for 3 seconds after spawning
            float timeToNotAttack = 180f;
            bool dontAttack = npc.Calamity().newAI[2] < timeToNotAttack;
            if (dontAttack)
            {
                npc.Calamity().newAI[2] += 1f;
                if (npc.Calamity().newAI[2] >= timeToNotAttack)
                    npc.SyncExtraAI();
            }

            bool fireSlower = false;
            if (laserAlive)
            {
                // If laser is firing ring of lasers
                if (Main.npc[CalamityGlobalNPC.primeLaser].ai[2] == 1f)
                    fireSlower = true;
            }
            else
            {
                fireSlower = npc.ai[2] == 0f;

                if (fireSlower)
                {
                    // Go to other phase after 13.33 seconds (change this as each arm dies)
                    npc.ai[3] += 1f;
                    if (!laserAlive)
                        npc.ai[3] += 1f;
                    if (!viceAlive)
                        npc.ai[3] += 1f;
                    if (!sawAlive)
                        npc.ai[3] += 1f;

                    if (npc.ai[3] >= (death ? 200f : 800f))
                    {
                        npc.target = Main.npc[(int)npc.ai[1]].target;
                        npc.localAI[0] = 0f;
                        npc.ai[2] = 1f;
                        fireSlower = false;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }
                }
                else
                {
                    // Go to phase 1 after 2 seconds (change this as each arm dies to stay in this phase for longer)
                    npc.ai[3] += 1f;

                    float timeLimit = 120f;
                    float timeMult = 1.882075f;
                    if (!laserAlive)
                        timeLimit *= timeMult;
                    if (!viceAlive)
                        timeLimit *= timeMult;
                    if (!sawAlive)
                        timeLimit *= timeMult;

                    if (npc.ai[3] >= timeLimit)
                    {
                        npc.target = Main.npc[(int)npc.ai[1]].target;
                        npc.localAI[0] = 0f;
                        npc.ai[2] = 0f;
                        fireSlower = true;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }
                }
            }

            // Movement
            float acceleration = death ? 0.375f : 0.25f;
            float accelerationMult = 1f;
            if (!laserAlive)
            {
                acceleration += 0.025f;
                accelerationMult += 0.5f;
            }
            if (!viceAlive)
                acceleration += 0.025f;
            if (!sawAlive)
                acceleration += 0.025f;
            if (death)
                acceleration *= accelerationMult;

            float topVelocity = acceleration * 100f;
            float deceleration = death ? 0.6f : 0.8f;

            if (npc.position.Y > Main.npc[(int)npc.ai[1]].position.Y - 70f)
            {
                if (npc.velocity.Y > 0f)
                    npc.velocity.Y *= deceleration;

                npc.velocity.Y -= acceleration;

                if (npc.velocity.Y > topVelocity)
                    npc.velocity.Y = topVelocity;
            }
            else if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y - 100f)
            {
                if (npc.velocity.Y < 0f)
                    npc.velocity.Y *= deceleration;

                npc.velocity.Y += acceleration;

                if (npc.velocity.Y < -topVelocity)
                    npc.velocity.Y = -topVelocity;
            }

            if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X + 130f)
            {
                if (npc.velocity.X > 0f)
                    npc.velocity.X *= deceleration;

                npc.velocity.X -= acceleration;

                if (npc.velocity.X > topVelocity)
                    npc.velocity.X = topVelocity;
            }
            if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X + 160f)
            {
                if (npc.velocity.X < 0f)
                    npc.velocity.X *= deceleration;

                npc.velocity.X += acceleration;

                if (npc.velocity.X < -topVelocity)
                    npc.velocity.X = -topVelocity;
            }

            if (fireSlower)
            {
                if (Main.npc[(int)npc.ai[1]].ai[1] == 3f && npc.timeLeft > 10)
                    npc.timeLeft = 10;

                Vector2 cannonArmPosition = npc.Center;
                float cannonArmTargetX = Main.player[npc.target].Center.X - cannonArmPosition.X;
                float cannonArmTargetY = Main.player[npc.target].Center.Y - cannonArmPosition.Y;
                float cannonArmTargetDist = (float)Math.Sqrt(cannonArmTargetX * cannonArmTargetX + cannonArmTargetY * cannonArmTargetY);
                npc.rotation = (float)Math.Atan2(cannonArmTargetY, cannonArmTargetX) - MathHelper.PiOver2;

                if (Main.netMode != NetmodeID.MultiplayerClient && !dontAttack)
                {
                    // Fire rocket every 2 seconds (change this as each arm dies to fire more aggressively)
                    npc.localAI[0] += 1f;
                    if (!laserAlive)
                        npc.localAI[0] += 1f;
                    if (!viceAlive)
                        npc.localAI[0] += 1f;
                    if (!sawAlive)
                        npc.localAI[0] += 1f;

                    if (npc.localAI[0] >= 120f)
                    {
                        SoundEngine.PlaySound(SoundID.Item62, npc.Center);
                        npc.localAI[0] = 0f;
                        int type = ProjectileID.RocketSkeleton;

                        float rocketSpeed = 10f;
                        cannonArmTargetDist = rocketSpeed / cannonArmTargetDist;
                        cannonArmTargetX *= cannonArmTargetDist;
                        cannonArmTargetY *= cannonArmTargetDist;

                        Vector2 rocketVelocity = new Vector2(cannonArmTargetX, cannonArmTargetY);
                        int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), cannonArmPosition + rocketVelocity.SafeNormalize(Vector2.UnitY) * 40f, rocketVelocity, type, RocketDamage.CalculateMechDamage(), 0f, Main.myPlayer, npc.target, 2f);
                        Main.projectile[proj].timeLeft = 540;
                    }
                }
            }
            else
            {
                Vector2 cannonSpreadArmPosition = npc.Center;
                float cannonSpreadArmTargetX = Main.player[npc.target].Center.X - cannonSpreadArmPosition.X;
                float cannonSpreadArmTargetY = Main.player[npc.target].Center.Y - cannonSpreadArmPosition.Y;
                npc.rotation = (float)Math.Atan2(cannonSpreadArmTargetY, cannonSpreadArmTargetX) - MathHelper.PiOver2;

                if (Main.netMode != NetmodeID.MultiplayerClient && !dontAttack)
                {
                    // Fire rockets every 2 seconds (change this as each arm dies to fire more aggressively)
                    npc.localAI[0] += 1f;
                    if (!laserAlive)
                        npc.localAI[0] += 0.5f;
                    if (!viceAlive)
                        npc.localAI[0] += 0.5f;
                    if (!sawAlive)
                        npc.localAI[0] += 0.5f;

                    if (npc.localAI[0] >= 180f)
                    {
                        SoundEngine.PlaySound(SoundID.Item62, npc.Center);
                        npc.localAI[0] = 0f;
                        int type = ProjectileID.RocketSkeleton;

                        float rocketSpeed = 10f;
                        Vector2 cannonSpreadTargetDist = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * rocketSpeed;
                        int numProj = 3;
                        float rotation = MathHelper.ToRadians(9);
                        for (int i = 0; i < numProj; i++)
                        {
                            Vector2 perturbedSpeed = cannonSpreadTargetDist.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 40f, perturbedSpeed, type, RocketDamage.CalculateMechDamage(), 0f, Main.myPlayer, npc.target, 2f);
                            Main.projectile[proj].timeLeft = 600;
                        }
                    }
                }
            }

            return false;
        }

        public static bool BuffedPrimeViceAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            // Direction
            npc.spriteDirection = -(int)npc.ai[0];

            // Where the vice should be in relation to the head
            Vector2 viceArmPosition = npc.Center;
            float viceArmIdleXPos = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - viceArmPosition.X;
            float viceArmIdleYPos = Main.npc[(int)npc.ai[1]].Center.Y + 230f - viceArmPosition.Y;
            float viceArmIdleDistance = (float)Math.Sqrt(viceArmIdleXPos * viceArmIdleXPos + viceArmIdleYPos * viceArmIdleYPos);

            // Return the vice to its proper location in relation to the head if it's too far away
            if (npc.ai[2] != 99f)
            {
                if (viceArmIdleDistance > 800f)
                    npc.ai[2] = 99f;
            }
            else if (viceArmIdleDistance < 400f)
                npc.ai[2] = 0f;

            // Despawn if head is gone
            if (!Main.npc[(int)npc.ai[1]].active || Main.npc[(int)npc.ai[1]].aiStyle != NPCAIStyleID.SkeletronPrimeHead)
            {
                npc.ai[2] += 10f;
                if (npc.ai[2] > 50f || !Main.dedServ)
                {
                    npc.life = -1;
                    npc.HitEffect(0, 10.0);
                    npc.active = false;
                }
            }

            CalamityGlobalNPC.primeVice = npc.whoAmI;

            // Check if arms are alive
            bool cannonAlive = false;
            bool laserAlive = false;
            bool sawAlive = false;
            if (CalamityGlobalNPC.primeCannon != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeCannon].active)
                    cannonAlive = true;
            }
            if (CalamityGlobalNPC.primeLaser != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeLaser].active)
                    laserAlive = true;
            }
            if (CalamityGlobalNPC.primeSaw != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeSaw].active)
                    sawAlive = true;
            }

            // Return to the head
            if (npc.ai[2] == 99f)
            {
                float acceleration = death ? 0.375f : 0.25f;
                float accelerationMult = 1f;
                if (!cannonAlive)
                {
                    acceleration += 0.025f;
                    accelerationMult += 0.5f;
                }
                if (!laserAlive)
                {
                    acceleration += 0.025f;
                    accelerationMult += 0.5f;
                }
                if (!sawAlive)
                    acceleration += 0.025f;
                if (death)
                    acceleration *= accelerationMult;

                float topVelocity = acceleration * 100f;
                float deceleration = death ? 0.6f : 0.8f;

                if (npc.position.Y > Main.npc[(int)npc.ai[1]].position.Y + 20f)
                {
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y *= deceleration;

                    npc.velocity.Y -= acceleration;

                    if (npc.velocity.Y > topVelocity)
                        npc.velocity.Y = topVelocity;
                }
                else if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y - 20f)
                {
                    if (npc.velocity.Y < 0f)
                        npc.velocity.Y *= deceleration;

                    npc.velocity.Y += acceleration;

                    if (npc.velocity.Y < -topVelocity)
                        npc.velocity.Y = -topVelocity;
                }

                if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X + 20f)
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X *= deceleration;

                    npc.velocity.X -= acceleration * 2f;

                    if (npc.velocity.X > topVelocity)
                        npc.velocity.X = topVelocity;
                }
                if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 20f)
                {
                    if (npc.velocity.X < 0f)
                        npc.velocity.X *= deceleration;

                    npc.velocity.X += acceleration * 2f;

                    if (npc.velocity.X < -topVelocity)
                        npc.velocity.X = -topVelocity;
                }
            }

            // Other phases
            else
            {
                // Stay near the head
                if (npc.ai[2] == 0f || npc.ai[2] == 3f)
                {
                    // Despawn if head is despawning
                    if (Main.npc[(int)npc.ai[1]].ai[1] == 3f && npc.timeLeft > 10)
                        npc.timeLeft = 10;

                    // Start charging after 10 seconds (change this as each arm dies)
                    npc.ai[3] += 1f;
                    if (!cannonAlive)
                        npc.ai[3] += 1f;
                    if (!laserAlive)
                        npc.ai[3] += 1f;
                    if (!sawAlive)
                        npc.ai[3] += 1f;

                    if (npc.ai[3] >= (death ? 150f : 600f))
                    {
                        npc.target = Main.npc[(int)npc.ai[1]].target;
                        npc.ai[2] += 1f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }

                    float acceleration = death ? 0.375f : 0.25f;
                    float accelerationMult = 1f;
                    if (!cannonAlive)
                    {
                        acceleration += 0.025f;
                        accelerationMult += 0.5f;
                    }
                    if (!laserAlive)
                    {
                        acceleration += 0.025f;
                        accelerationMult += 0.5f;
                    }
                    if (!sawAlive)
                        acceleration += 0.025f;
                    if (death)
                        acceleration *= accelerationMult;

                    float topVelocity = acceleration * 100f;
                    float deceleration = death ? 0.6f : 0.8f;

                    if (npc.position.Y > Main.npc[(int)npc.ai[1]].position.Y + 100f)
                    {
                        if (npc.velocity.Y > 0f)
                            npc.velocity.Y *= deceleration;

                        npc.velocity.Y -= acceleration;

                        if (npc.velocity.Y > topVelocity)
                            npc.velocity.Y = topVelocity;
                    }
                    else if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y + 70f)
                    {
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= deceleration;

                        npc.velocity.Y += acceleration;

                        if (npc.velocity.Y < -topVelocity)
                            npc.velocity.Y = -topVelocity;
                    }

                    if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X + 160f)
                    {
                        if (npc.velocity.X > 0f)
                            npc.velocity.X *= deceleration;

                        npc.velocity.X -= acceleration;

                        if (npc.velocity.X > topVelocity)
                            npc.velocity.X = topVelocity;
                    }
                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X + 130f)
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X *= deceleration;

                        npc.velocity.X += acceleration;

                        if (npc.velocity.X < -topVelocity)
                            npc.velocity.X = -topVelocity;
                    }

                    Vector2 viceArmReelbackCurrentPos = npc.Center;
                    float viceArmReelbackXDest = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - viceArmReelbackCurrentPos.X;
                    float viceArmReelbackYDest = Main.npc[(int)npc.ai[1]].position.Y + 230f - viceArmReelbackCurrentPos.Y;
                    npc.rotation = (float)Math.Atan2(viceArmReelbackYDest, viceArmReelbackXDest) + MathHelper.PiOver2;
                    return false;
                }

                // Charge towards the player
                if (npc.ai[2] == 1f)
                {
                    float deceleration = death ? 0.75f : 0.8f;
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y *= deceleration;

                    Vector2 viceArmChargePosition = npc.Center;
                    float viceArmChargeTargetX = Main.npc[(int)npc.ai[1]].Center.X - 280f * npc.ai[0] - viceArmChargePosition.X;
                    float viceArmChargeTargetY = Main.npc[(int)npc.ai[1]].position.Y + 230f - viceArmChargePosition.Y;
                    npc.rotation = (float)Math.Atan2(viceArmChargeTargetY, viceArmChargeTargetX) + MathHelper.PiOver2;

                    npc.velocity.X = (npc.velocity.X * 5f + Main.npc[(int)npc.ai[1]].velocity.X) / 6f;
                    npc.velocity.X += 0.5f;

                    npc.velocity.Y -= 0.5f;
                    if (npc.velocity.Y < -12f)
                        npc.velocity.Y = -12f;

                    if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y - 280f)
                    {
                        float chargeVelocity = 16f;
                        if (!cannonAlive)
                            chargeVelocity += 1.5f;
                        if (!laserAlive)
                            chargeVelocity += 1.5f;
                        if (!sawAlive)
                            chargeVelocity += 1.5f;

                        npc.ai[2] = 2f;
                        viceArmChargePosition = npc.Center;
                        viceArmChargeTargetX = Main.player[npc.target].Center.X - viceArmChargePosition.X;
                        viceArmChargeTargetY = Main.player[npc.target].Center.Y - viceArmChargePosition.Y;
                        float viceArmChargeTargetDist = (float)Math.Sqrt(viceArmChargeTargetX * viceArmChargeTargetX + viceArmChargeTargetY * viceArmChargeTargetY);
                        viceArmChargeTargetDist = chargeVelocity / viceArmChargeTargetDist;
                        npc.velocity.X = viceArmChargeTargetX * viceArmChargeTargetDist;
                        npc.velocity.Y = viceArmChargeTargetY * viceArmChargeTargetDist;
                        npc.netUpdate = true;
                    }
                }

                // Charge 4 times (more if arms are dead)
                else if (npc.ai[2] == 2f)
                {
                    if (npc.position.Y > Main.player[npc.target].position.Y || npc.velocity.Y < 0f)
                    {
                        float chargeAmt = 4f;
                        if (!cannonAlive)
                            chargeAmt += 1f;
                        if (!laserAlive)
                            chargeAmt += 1f;
                        if (!sawAlive)
                            chargeAmt += 1f;

                        if (npc.ai[3] >= chargeAmt)
                        {
                            // Return to head
                            npc.ai[2] = 3f;
                            npc.ai[3] = 0f;
                            return false;
                        }

                        npc.ai[2] = 1f;
                        npc.ai[3] += 1f;
                    }
                }

                // Different type of charge
                else if (npc.ai[2] == 4f)
                {
                    Vector2 viceArmOtherChargePosition = npc.Center;
                    float viceArmOtherChargeTargetX = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - viceArmOtherChargePosition.X;
                    float viceArmOtherChargeTargetY = Main.npc[(int)npc.ai[1]].position.Y + 230f - viceArmOtherChargePosition.Y;
                    npc.rotation = (float)Math.Atan2(viceArmOtherChargeTargetY, viceArmOtherChargeTargetX) + MathHelper.PiOver2;

                    npc.velocity.Y = (npc.velocity.Y * 5f + Main.npc[(int)npc.ai[1]].velocity.Y) / 6f;

                    npc.velocity.X += 0.5f;
                    if (npc.velocity.X > 12f)
                        npc.velocity.X = 12f;

                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 500f || npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X + 500f)
                    {
                        float chargeVelocity = 14f;
                        if (!cannonAlive)
                            chargeVelocity += 1f;
                        if (!laserAlive)
                            chargeVelocity += 1f;
                        if (!sawAlive)
                            chargeVelocity += 1f;

                        npc.ai[2] = 5f;
                        viceArmOtherChargePosition = npc.Center;
                        viceArmOtherChargeTargetX = Main.player[npc.target].Center.X - viceArmOtherChargePosition.X;
                        viceArmOtherChargeTargetY = Main.player[npc.target].Center.Y - viceArmOtherChargePosition.Y;
                        float viceArmOtherChargeTargetDist = (float)Math.Sqrt(viceArmOtherChargeTargetX * viceArmOtherChargeTargetX + viceArmOtherChargeTargetY * viceArmOtherChargeTargetY);
                        viceArmOtherChargeTargetDist = chargeVelocity / viceArmOtherChargeTargetDist;
                        npc.velocity.X = viceArmOtherChargeTargetX * viceArmOtherChargeTargetDist;
                        npc.velocity.Y = viceArmOtherChargeTargetY * viceArmOtherChargeTargetDist;
                        npc.netUpdate = true;
                    }
                }

                // Charge 4 times (more if arms are dead)
                else if (npc.ai[2] == 5f && npc.Center.X < Main.player[npc.target].Center.X - 100f)
                {
                    float chargeAmt = 4f;
                    if (!cannonAlive)
                        chargeAmt += 1f;
                    if (!laserAlive)
                        chargeAmt += 1f;
                    if (!sawAlive)
                        chargeAmt += 1f;

                    if (npc.ai[3] >= chargeAmt)
                    {
                        // Return to head
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        return false;
                    }

                    npc.ai[2] = 4f;
                    npc.ai[3] += 1f;
                }
            }

            return false;
        }

        public static bool BuffedPrimeSawAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            Vector2 sawArmLocation = npc.Center;
            float sawArmIdleXPos = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - sawArmLocation.X;
            float sawArmIdleYPos = Main.npc[(int)npc.ai[1]].Center.Y + 230f - sawArmLocation.Y;
            float sawArmIdleDistance = (float)Math.Sqrt(sawArmIdleXPos * sawArmIdleXPos + sawArmIdleYPos * sawArmIdleYPos);

            if (npc.ai[2] != 99f)
            {
                if (sawArmIdleDistance > 800f)
                    npc.ai[2] = 99f;
            }
            else if (sawArmIdleDistance < 400f)
                npc.ai[2] = 0f;

            npc.spriteDirection = -(int)npc.ai[0];

            if (!Main.npc[(int)npc.ai[1]].active || Main.npc[(int)npc.ai[1]].aiStyle != NPCAIStyleID.SkeletronPrimeHead)
            {
                npc.ai[2] += 10f;
                if (npc.ai[2] > 50f || !Main.dedServ)
                {
                    npc.life = -1;
                    npc.HitEffect(0, 10.0);
                    npc.active = false;
                }
            }

            CalamityGlobalNPC.primeSaw = npc.whoAmI;

            // Check if arms are alive
            bool cannonAlive = false;
            bool laserAlive = false;
            bool viceAlive = false;
            if (CalamityGlobalNPC.primeCannon != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeCannon].active)
                    cannonAlive = true;
            }
            if (CalamityGlobalNPC.primeLaser != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeLaser].active)
                    laserAlive = true;
            }
            if (CalamityGlobalNPC.primeVice != -1)
            {
                if (Main.npc[CalamityGlobalNPC.primeVice].active)
                    viceAlive = true;
            }

            if (npc.ai[2] == 99f)
            {
                float acceleration = death ? 0.375f : 0.25f;
                float accelerationMult = 1f;
                if (!cannonAlive)
                {
                    acceleration += 0.025f;
                    accelerationMult += 0.5f;
                }
                if (!laserAlive)
                {
                    acceleration += 0.025f;
                    accelerationMult += 0.5f;
                }
                if (!viceAlive)
                    acceleration += 0.025f;
                if (death)
                    acceleration *= accelerationMult;

                float topVelocity = acceleration * 100f;
                float deceleration = death ? 0.6f : 0.8f;

                if (npc.position.Y > Main.npc[(int)npc.ai[1]].position.Y + 20f)
                {
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y *= deceleration;

                    npc.velocity.Y -= acceleration;

                    if (npc.velocity.Y > topVelocity)
                        npc.velocity.Y = topVelocity;
                }
                else if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y - 20f)
                {
                    if (npc.velocity.Y < 0f)
                        npc.velocity.Y *= deceleration;

                    npc.velocity.Y += acceleration;

                    if (npc.velocity.Y < -topVelocity)
                        npc.velocity.Y = -topVelocity;
                }

                if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X + 20f)
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X *= deceleration;

                    npc.velocity.X -= acceleration * 2f;

                    if (npc.velocity.X > topVelocity)
                        npc.velocity.X = topVelocity;
                }
                if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 20f)
                {
                    if (npc.velocity.X < 0f)
                        npc.velocity.X *= deceleration;

                    npc.velocity.X += acceleration * 2f;

                    if (npc.velocity.X < -topVelocity)
                        npc.velocity.X = -topVelocity;
                }
            }
            else
            {
                if (npc.ai[2] == 0f || npc.ai[2] == 3f)
                {
                    if (Main.npc[(int)npc.ai[1]].ai[1] == 3f && npc.timeLeft > 10)
                        npc.timeLeft = 10;

                    // Start charging after 3 seconds (change this as each arm dies)
                    npc.ai[3] += 1f;
                    if (!cannonAlive)
                        npc.ai[3] += 1f;
                    if (!laserAlive)
                        npc.ai[3] += 1f;
                    if (!viceAlive)
                        npc.ai[3] += 1f;

                    if (npc.ai[3] >= (death ? 90f : 180f))
                    {
                        npc.target = Main.npc[(int)npc.ai[1]].target;
                        npc.ai[2] += 1f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }

                    float acceleration = death ? 0.375f : 0.25f;
                    float accelerationMult = 1f;
                    if (!cannonAlive)
                    {
                        acceleration += 0.025f;
                        accelerationMult += 0.5f;
                    }
                    if (!laserAlive)
                    {
                        acceleration += 0.025f;
                        accelerationMult += 0.5f;
                    }
                    if (!viceAlive)
                        acceleration += 0.025f;
                    if (death)
                        acceleration *= accelerationMult;

                    float topVelocity = acceleration * 100f;
                    float deceleration = death ? 0.6f : 0.8f;

                    if (npc.position.Y > Main.npc[(int)npc.ai[1]].position.Y + 100f)
                    {
                        if (npc.velocity.Y > 0f)
                            npc.velocity.Y *= deceleration;

                        npc.velocity.Y -= acceleration;

                        if (npc.velocity.Y > topVelocity)
                            npc.velocity.Y = topVelocity;
                    }
                    else if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y + 70f)
                    {
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= deceleration;

                        npc.velocity.Y += acceleration;

                        if (npc.velocity.Y < -topVelocity)
                            npc.velocity.Y = -topVelocity;
                    }

                    if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X - 130f)
                    {
                        if (npc.velocity.X > 0f)
                            npc.velocity.X *= deceleration;

                        npc.velocity.X -= acceleration * 1.5f;

                        if (npc.velocity.X > topVelocity)
                            npc.velocity.X = topVelocity;
                    }
                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 160f)
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X *= deceleration;

                        npc.velocity.X += acceleration * 1.5f;

                        if (npc.velocity.X < -topVelocity)
                            npc.velocity.X = -topVelocity;
                    }

                    Vector2 sawArmReelbackCurrentPos = npc.Center;
                    float sawArmReelbackXDest = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - sawArmReelbackCurrentPos.X;
                    float sawArmReelbackYDest = Main.npc[(int)npc.ai[1]].position.Y + 230f - sawArmReelbackCurrentPos.Y;
                    npc.rotation = (float)Math.Atan2(sawArmReelbackYDest, sawArmReelbackXDest) + MathHelper.PiOver2;
                    return false;
                }

                if (npc.ai[2] == 1f)
                {
                    Vector2 sawArmChargePos = npc.Center;
                    float sawArmChargeTargetX = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - sawArmChargePos.X;
                    float sawArmChargeTargetY = Main.npc[(int)npc.ai[1]].position.Y + 230f - sawArmChargePos.Y;
                    npc.rotation = (float)Math.Atan2(sawArmChargeTargetY, sawArmChargeTargetX) + MathHelper.PiOver2;

                    float deceleration = death ? 0.875f : 0.9f;
                    npc.velocity.X *= deceleration;
                    npc.velocity.Y -= 0.5f;
                    if (npc.velocity.Y < -12f)
                        npc.velocity.Y = -12f;

                    if (npc.position.Y < Main.npc[(int)npc.ai[1]].position.Y - 200f)
                    {
                        float chargeVelocity = 22f;
                        if (!cannonAlive)
                            chargeVelocity += 1.5f;
                        if (!laserAlive)
                            chargeVelocity += 1.5f;
                        if (!viceAlive)
                            chargeVelocity += 1.5f;

                        npc.ai[2] = 2f;
                        sawArmChargePos = npc.Center;
                        sawArmChargeTargetX = Main.player[npc.target].Center.X - sawArmChargePos.X;
                        sawArmChargeTargetY = Main.player[npc.target].Center.Y - sawArmChargePos.Y;
                        float sawArmChargeTargetDist = (float)Math.Sqrt(sawArmChargeTargetX * sawArmChargeTargetX + sawArmChargeTargetY * sawArmChargeTargetY);
                        sawArmChargeTargetDist = chargeVelocity / sawArmChargeTargetDist;
                        npc.velocity.X = sawArmChargeTargetX * sawArmChargeTargetDist;
                        npc.velocity.Y = sawArmChargeTargetY * sawArmChargeTargetDist;
                        npc.netUpdate = true;
                    }
                }

                else if (npc.ai[2] == 2f)
                {
                    if (npc.position.Y > Main.player[npc.target].position.Y || npc.velocity.Y < 0f)
                        npc.ai[2] = 3f;
                }

                else
                {
                    if (npc.ai[2] == 4f)
                    {
                        float chargeVelocity = 11f;
                        if (!cannonAlive)
                            chargeVelocity += 1.5f;
                        if (!laserAlive)
                            chargeVelocity += 1.5f;
                        if (!viceAlive)
                            chargeVelocity += 1.5f;
                        if (death)
                            chargeVelocity *= 1.25f;

                        Vector2 sawArmOtherChargePos = npc.Center;
                        float sawArmOtherChargeTargetX = Main.player[npc.target].Center.X - sawArmOtherChargePos.X;
                        float sawArmOtherChargeTargetY = Main.player[npc.target].Center.Y - sawArmOtherChargePos.Y;
                        float sawArmOtherChargeTargetDist = (float)Math.Sqrt(sawArmOtherChargeTargetX * sawArmOtherChargeTargetX + sawArmOtherChargeTargetY * sawArmOtherChargeTargetY);
                        sawArmOtherChargeTargetDist = chargeVelocity / sawArmOtherChargeTargetDist;
                        sawArmOtherChargeTargetX *= sawArmOtherChargeTargetDist;
                        sawArmOtherChargeTargetY *= sawArmOtherChargeTargetDist;

                        float acceleration = death ? 0.125f : 0.08f;
                        float deceleration = death ? 0.6f : 0.8f;

                        if (npc.velocity.X > sawArmOtherChargeTargetX)
                        {
                            if (npc.velocity.X > 0f)
                                npc.velocity.X *= deceleration;

                            npc.velocity.X -= acceleration;
                        }
                        if (npc.velocity.X < sawArmOtherChargeTargetX)
                        {
                            if (npc.velocity.X < 0f)
                                npc.velocity.X *= deceleration;

                            npc.velocity.X += acceleration;
                        }
                        if (npc.velocity.Y > sawArmOtherChargeTargetY)
                        {
                            if (npc.velocity.Y > 0f)
                                npc.velocity.Y *= deceleration;

                            npc.velocity.Y -= acceleration;
                        }
                        if (npc.velocity.Y < sawArmOtherChargeTargetY)
                        {
                            if (npc.velocity.Y < 0f)
                                npc.velocity.Y *= deceleration;

                            npc.velocity.Y += acceleration;
                        }

                        npc.ai[3] += 1f;
                        if (npc.justHit)
                            npc.ai[3] += 2f;

                        if (npc.ai[3] >= 600f)
                        {
                            npc.ai[2] = 0f;
                            npc.ai[3] = 0f;
                            npc.netUpdate = true;
                        }

                        sawArmOtherChargePos = npc.Center;
                        sawArmOtherChargeTargetX = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - sawArmOtherChargePos.X;
                        sawArmOtherChargeTargetY = Main.npc[(int)npc.ai[1]].position.Y + 230f - sawArmOtherChargePos.Y;
                        npc.rotation = (float)Math.Atan2(sawArmOtherChargeTargetY, sawArmOtherChargeTargetX) + MathHelper.PiOver2;
                        return false;
                    }

                    if (npc.ai[2] == 5f && ((npc.velocity.X > 0f && npc.Center.X > Main.player[npc.target].Center.X) || (npc.velocity.X < 0f && npc.Center.X < Main.player[npc.target].Center.X)))
                        npc.ai[2] = 0f;
                }
            }

            return false;
        }
    }
}
