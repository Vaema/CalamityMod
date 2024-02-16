using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.Projectiles.Boss;

namespace CalamityMod.NPCs.VanillaNPCOverrides.Bosses
{
    public static class QueenBeeAI
    {
        public static bool BuffedQueenBeeAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            bool bossRush = BossRushEvent.BossRushActive;
            bool masterMode = Main.masterMode || bossRush;
            bool death = CalamityWorld.death || bossRush;

            bool enrage = true;
            int targetTileX = (int)Main.player[npc.target].Center.X / 16;
            int targetTileY = (int)Main.player[npc.target].Center.Y / 16;

            Tile tile = Framing.GetTileSafely(targetTileX, targetTileY);
            if (tile.WallType == WallID.HiveUnsafe)
                enrage = false;

            float maxEnrageScale = 2f;
            float enrageScale = death ? 0.5f : 0f;
            if (((npc.position.Y / 16f) < Main.worldSurface && enrage) || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 0.5f;
            }
            if (!Main.player[npc.target].ZoneJungle || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 0.5f;
            }

            if (Main.getGoodWorld)
                enrageScale += ((CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 1f : 0.5f);

            if (bossRush)
                enrageScale = 2f;

            if (enrageScale > maxEnrageScale)
                enrageScale = maxEnrageScale;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Bee spawn limit
            int beeLimit = 15;

            // Queen Bee Bee count
            int totalBees = 0;
            bool beeLimitReached = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC bee = Main.npc[i];
                bool isQueenBeeBee = bee.ai[3] == 1f;
                if (bee.active && (bee.type == NPCID.Bee || bee.type == NPCID.BeeSmall) && isQueenBeeBee)
                {
                    totalBees++;
                    if (totalBees >= beeLimit)
                    {
                        beeLimitReached = true;
                        break;
                    }
                }
            }

            // Phases

            // Become more aggressive and start firing double stingers (triple in death mode) phase
            bool phase2 = lifeRatio < 0.85f;

            // Begin launching beehives instead of bees phase
            bool phase3 = lifeRatio < 0.7f;

            // Stop spawning bees from ass and start performing stinger arcs + diagonal dashes, spawn bees while charging and become more aggressive phase
            bool phase4 = lifeRatio < 0.5f;

            // Perform many shorter-range dashes and use stinger arc in two possible directions phase
            bool phase5 = lifeRatio < 0.3f;

            // Triple stinger (quintuple in death mode) bombardment phase
            bool phase6 = lifeRatio < 0.1f;

            // Despawn
            float distanceFromTarget = Vector2.Distance(npc.Center, Main.player[npc.target].Center);
            if (npc.ai[0] != 6f)
            {
                if (npc.timeLeft < 60)
                    npc.timeLeft = 60;
                if (distanceFromTarget > 3000f)
                    npc.ai[0] = 4f;
            }
            if (Main.player[npc.target].dead)
                npc.ai[0] = 6f;

            // Adjust slowing debuff immunity
            bool immuneToSlowingDebuffs = npc.ai[0] == 0f;
            npc.buffImmune[ModContent.BuffType<GlacialState>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<TemporalSadness>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<KamiFlu>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<Eutrophication>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<TimeDistortion>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<GalvanicCorrosion>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<Vaporfied>()] = immuneToSlowingDebuffs;
            npc.buffImmune[BuffID.Slow] = immuneToSlowingDebuffs;
            npc.buffImmune[BuffID.Webbed] = immuneToSlowingDebuffs;

            // Always start in enemy spawning phase
            if (calamityGlobalNPC.newAI[3] == 0f)
            {
                calamityGlobalNPC.newAI[3] = 1f;
                npc.ai[0] = 2f;
                npc.netUpdate = true;
                npc.SyncExtraAI();
            }

            // Despawn phase
            if (npc.ai[0] == 6f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.velocity.Y *= 0.98f;

                if (npc.velocity.X < 0f)
                    npc.direction = -1;
                else
                    npc.direction = 1;

                npc.spriteDirection = npc.direction;

                if (npc.position.X < (Main.maxTilesX * 8))
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X *= 0.98f;
                    else
                        npc.localAI[0] = 1f;

                    npc.velocity.X -= 0.08f;
                }
                else
                {
                    if (npc.velocity.X < 0f)
                        npc.velocity.X *= 0.98f;
                    else
                        npc.localAI[0] = 1f;

                    npc.velocity.X += 0.08f;
                }

                if (npc.timeLeft > 10)
                    npc.timeLeft = 10;
            }

            // Pick a random phase
            else if (npc.ai[0] == -1f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int phase;
                    int maxRandom = phase4 ? 5 : 4;
                    do phase = Main.rand.Next(maxRandom);
                    while (phase == npc.ai[1] || phase == 1 || (phase == 2 && phase4));

                    bool charging = phase == 0;

                    // 5 is stinger arc and charge
                    if (phase == 4)
                        phase = 5;

                    npc.TargetClosest();
                    npc.ai[0] = phase;
                    npc.ai[1] = 0f;

                    // Movement direction for the stinger arcs
                    npc.ai[2] = (phase == 5 && phase5) ? (Main.rand.NextBool() ? 1f : -1f) : phase == 5 ? 1f : 0f;

                    // Velocity for the charges
                    if (masterMode)
                        npc.ai[3] = charging ? ((phase6 ? 27f : phase5 ? 16f : phase4 ? 27f : phase2 ? 22f : 17f) + 6f * enrageScale) : 0f;
                    else
                        npc.ai[3] = charging ? ((phase6 ? 25f : phase5 ? 14f : phase4 ? 25f : phase2 ? 20f : 15f) + 6f * enrageScale) : 0f;

                    // Distance for the charges
                    if (masterMode)
                        calamityGlobalNPC.newAI[1] = charging ? ((phase6 ? 700f : phase5 ? 300f : phase4 ? 600f : phase2 ? 500f : 400f) - 100f * enrageScale) : 0f;
                    else
                        calamityGlobalNPC.newAI[1] = charging ? ((phase6 ? 750f : phase5 ? 350f : phase4 ? 650f : phase2 ? 550f : 450f) - 100f * enrageScale) : 0f;

                    npc.SyncExtraAI();
                }
            }

            // Charging phase
            else if (npc.ai[0] == 0f)
            {
                // Charging distance from player
                int chargeDistanceX = (int)calamityGlobalNPC.newAI[1];

                // Number of charges
                int chargeAmt = (int)Math.Ceiling((phase6 ? 2f : phase5 ? 4f : phase4 ? 3f : 2f) + enrageScale);
                if (death)
                    chargeAmt = phase6 ? 1 : phase5 ? 3 : phase4 ? 2 : 1;

                int masterModeChargeLimit = death ? (phase5 ? 2 : 1) : (phase5 ? 3 : 2);
                if (masterMode && chargeAmt > masterModeChargeLimit)
                    chargeAmt = masterModeChargeLimit;

                // Switch to a random phase if chargeAmt has been exceeded
                if (npc.ai[1] > (2 * chargeAmt) && npc.ai[1] % 2f == 0f)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                    return false;
                }

                // Charge velocity
                float velocity = npc.ai[3];

                // Line up and initiate charge
                if (npc.ai[1] % 2f == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    // Initiate charge
                    float chargeDistanceY = phase6 ? 100f : phase4 ? 50f : 20f;
                    chargeDistanceY += 100f * enrageScale;
                    if (death)
                        chargeDistanceY += MathHelper.Lerp(0f, 100f, 1f - lifeRatio);
                    if (masterMode)
                        chargeDistanceY *= 1.5f;

                    float distanceFromTargetX = Math.Abs(npc.Center.X - Main.player[npc.target].Center.X);
                    float distanceFromTargetY = Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y);
                    if (distanceFromTargetY < chargeDistanceY && distanceFromTargetX >= chargeDistanceX)
                    {
                        // Set damage
                        npc.damage = npc.defDamage;

                        // Set AI variables and speed
                        npc.localAI[0] = 1f;
                        npc.ai[1] += 1f;
                        npc.ai[2] = 0f;

                        // Get target location
                        Vector2 beeLocation = npc.Center;
                        float targetXDist = Main.player[npc.target].Center.X - beeLocation.X;
                        float targetYDist = Main.player[npc.target].Center.Y - beeLocation.Y;
                        float targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);
                        targetDistance = velocity / targetDistance;
                        npc.velocity.X = targetXDist * targetDistance;
                        npc.velocity.Y = targetYDist * targetDistance;

                        // Face the correct direction and play charge sound
                        float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                        npc.direction = playerLocation < 0 ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        
                        SoundEngine.PlaySound(SoundID.Zombie125, npc.Center);

                        return false;
                    }

                    // Velocity variables
                    npc.localAI[0] = 0f;
                    float chargeVelocityX = (phase4 ? 24f : phase2 ? 20f : 16f) + 8f * enrageScale;
                    float chargeVelocityY = (phase4 ? 18f : phase2 ? 15f : 12f) + 6f * enrageScale;
                    float chargeAccelerationX = (phase4 ? 0.9f : phase2 ? 0.7f : 0.5f) + 0.5f * enrageScale;
                    float chargeAccelerationY = (phase4 ? 0.45f : phase2 ? 0.35f : 0.25f) + 0.25f * enrageScale;

                    // Velocity calculations
                    if (npc.Center.Y < Main.player[npc.target].Center.Y - chargeDistanceY)
                        npc.velocity.Y += chargeAccelerationY;
                    else if (npc.Center.Y > Main.player[npc.target].Center.Y + chargeDistanceY)
                        npc.velocity.Y -= chargeAccelerationY;
                    else
                        npc.velocity.Y *= 0.7f;

                    if (npc.velocity.Y < -chargeVelocityY)
                        npc.velocity.Y = -chargeVelocityY;
                    if (npc.velocity.Y > chargeVelocityY)
                        npc.velocity.Y = chargeVelocityY;

                    float distanceXMax = 100f;
                    float distanceXMin = 20f;
                    if (distanceFromTargetX > chargeDistanceX + distanceXMax)
                        npc.velocity.X += chargeAccelerationX * npc.direction;
                    else if (distanceFromTargetX < chargeDistanceX + distanceXMin)
                        npc.velocity.X -= chargeAccelerationX * npc.direction;
                    else
                        npc.velocity.X *= 0.7f;

                    // Limit velocity
                    if (npc.velocity.X < -chargeVelocityX)
                        npc.velocity.X = -chargeVelocityX;
                    if (npc.velocity.X > chargeVelocityX)
                        npc.velocity.X = chargeVelocityX;

                    // Face the correct direction
                    float playerLocation2 = npc.Center.X - Main.player[npc.target].Center.X;
                    npc.direction = playerLocation2 < 0 ? 1 : -1;
                    npc.spriteDirection = npc.direction;

                    npc.netUpdate = true;

                    if (npc.netSpam > 10)
                        npc.netSpam = 10;
                }
                else
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    // Face the correct direction
                    if (npc.velocity.X < 0f)
                        npc.direction = -1;
                    else
                        npc.direction = 1;

                    npc.spriteDirection = npc.direction;

                    // Get which side of the player the boss is on
                    int chargeDirection = 1;
                    if (npc.Center.X < Main.player[npc.target].Center.X)
                        chargeDirection = -1;

                    // If boss is in correct position, slow down, if not, reset
                    bool shouldCharge = false;
                    if (npc.direction == chargeDirection && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) > chargeDistanceX)
                    {
                        npc.ai[2] = 1f;
                        shouldCharge = true;
                    }
                    if (Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) > chargeDistanceX * 1.5f)
                    {
                        npc.ai[2] = 1f;
                        shouldCharge = true;
                    }
                    if (enrageScale > 0f && shouldCharge)
                        npc.velocity *= MathHelper.Lerp(0.3f, 1f, 1f - enrageScale / maxEnrageScale);

                    // Keep moving
                    if (npc.ai[2] != 1f)
                    {
                        // Velocity fix if Queen Bee is slowed
                        if (npc.velocity.Length() < velocity)
                            npc.velocity.X = velocity * npc.direction;

                        float accelerateGateValue = phase6 ? 30f : phase5 ? 10f : 90f;
                        if (masterMode)
                            accelerateGateValue *= 0.5f;
                        if (enrageScale > 0f)
                            accelerateGateValue *= 0.5f;

                        calamityGlobalNPC.newAI[0] += 1f;
                        if (calamityGlobalNPC.newAI[0] > accelerateGateValue)
                        {
                            npc.SyncExtraAI();
                            float velocityXLimit = velocity * 2f;
                            if (Math.Abs(npc.velocity.X) < velocityXLimit)
                                npc.velocity.X *= (masterMode ? 1.02f : 1.01f);
                        }

                        // Spawn bees
                        float beeSpawnGateValue = masterMode ? 16f : 20f;
                        bool spawnBee = phase4 && calamityGlobalNPC.newAI[0] % beeSpawnGateValue == 0f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                        if (spawnBee)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int spawnType = Main.rand.Next(NPCID.Bee, NPCID.BeeSmall + 1);
                                if (Main.zenithWorld)
                                {
                                    if (phase3)
                                        spawnType = Main.rand.NextBool(3) ? ModContent.NPCType<PlagueChargerLarge>() : ModContent.NPCType<PlagueCharger>();
                                    else
                                        spawnType = NPCID.Hellbat;
                                }

                                if (!beeLimitReached)
                                {
                                    int spawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, spawnType);
                                    Main.npc[spawn].velocity = Main.player[npc.target].Center - npc.Center;
                                    Main.npc[spawn].velocity.Normalize();
                                    Main.npc[spawn].velocity *= 5f;
                                    if (!Main.zenithWorld)
                                    {
                                        Main.npc[spawn].ai[2] = enrageScale;
                                        Main.npc[spawn].ai[3] = 1f;
                                    }
                                    Main.npc[spawn].netUpdate = true;
                                }
                            }
                        }

                        npc.localAI[0] = 1f;
                        return false;
                    }

                    float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                    npc.direction = playerLocation < 0 ? 1 : -1;
                    npc.spriteDirection = npc.direction;

                    // Slow down
                    npc.localAI[0] = 0f;
                    npc.velocity *= (masterMode ? 0.8f : 0.9f);

                    float chargeDeceleration = masterMode ? 0.2f : 0.1f;
                    if (phase2)
                    {
                        npc.velocity *= 0.9f;
                        chargeDeceleration += 0.05f;
                    }
                    if (phase4)
                    {
                        npc.velocity *= 0.8f;
                        chargeDeceleration += 0.1f;
                    }
                    if (enrageScale > 0f)
                        npc.velocity *= MathHelper.Lerp(0.5f, 1f, 1f - enrageScale / maxEnrageScale);

                    if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < chargeDeceleration)
                    {
                        npc.ai[2] = 0f;
                        npc.ai[1] += 1f;
                        calamityGlobalNPC.newAI[0] = 0f;
                        npc.SyncExtraAI();
                    }

                    npc.netUpdate = true;

                    if (npc.netSpam > 10)
                        npc.netSpam = 10;
                }
            }

            // Fly above target before bee spawning phase
            else if (npc.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                // Direction
                float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                // Get target location
                float beeAttackAccel = death ? 0.3f : 0.24f;
                float beeAttackSpeed = 12f + enrageScale * 3f;

                if (masterMode)
                {
                    beeAttackAccel *= 1.4f;
                    beeAttackSpeed *= 1.2f;
                }

                bool canHitTarget = Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                float distanceAboveTarget = !canHitTarget ? 0f : 320f;
                Vector2 hoverDestination = Main.player[npc.target].Center - Vector2.UnitY * distanceAboveTarget;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * beeAttackSpeed;

                // Go to bee spawn phase
                calamityGlobalNPC.newAI[0] += 1f;
                if ((Vector2.Distance(npc.Center, hoverDestination) < 400f && canHitTarget) || calamityGlobalNPC.newAI[0] >= 180f)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    calamityGlobalNPC.newAI[0] = 0f;
                    npc.netUpdate = true;
                    npc.SyncExtraAI();
                    return false;
                }

                npc.SimpleFlyMovement(idealVelocity, beeAttackAccel);
            }

            // Bee spawn phase
            else if (npc.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.localAI[0] = 0f;

                // Get target location and spawn bees from ass
                float beeAttackHoverSpeed = 16f + enrageScale * 4f;
                float beeAttackHoverAccel = death ? 0.4f : 0.3f;

                if (masterMode)
                {
                    beeAttackHoverAccel *= 1.4f;
                    beeAttackHoverSpeed *= 1.2f;
                }

                Vector2 beeSpawnLocation = new Vector2(npc.Center.X + (Main.rand.Next(20) * npc.direction), npc.position.Y + npc.height * 0.8f);
                Vector2 beeSpawnCollisionLocation = new Vector2(beeSpawnLocation.X, beeSpawnLocation.Y - 30f);
                bool canHitTarget = Collision.CanHit(beeSpawnCollisionLocation, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                Vector2 hoverDestination = Main.player[npc.target].Center - Vector2.UnitY * (!canHitTarget ? 0f : 320f);
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * beeAttackHoverSpeed;

                // Bee spawn timer
                npc.ai[1] += 1f;
                int beeSpawnTimer = 0;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && (npc.Center - Main.player[i].Center).Length() < 1000f)
                        beeSpawnTimer++;
                }
                npc.ai[1] += beeSpawnTimer / 2;
                if (phase2)
                    npc.ai[1] += 1f;

                bool spawnBee = false;
                float beeSpawnCheck = (phase3 ? 45f : 15f) - (phase3 ? 18f : 6f) * enrageScale;
                if (npc.ai[1] > beeSpawnCheck)
                {
                    npc.ai[1] = 0f;
                    npc.ai[2] += 1f;
                    spawnBee = true;
                }

                // Spawn bees
                if (Collision.CanHit(beeSpawnLocation, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && spawnBee && !beeLimitReached)
                {
                    if (!phase3 || Main.zenithWorld)
                        SoundEngine.PlaySound(SoundID.NPCHit1, beeSpawnLocation);
                    else
                        SoundEngine.PlaySound(SoundID.NPCHit18, beeSpawnLocation);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (phase3 && !Main.zenithWorld)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromAI(), beeSpawnLocation, (Main.player[npc.target].Center - beeSpawnLocation).SafeNormalize(Vector2.UnitY), ProjectileID.BeeHive, 0, 0f, Main.myPlayer, 0f, 0f, 1f);
                        }
                        else
                        {
                            int spawnType = Main.rand.Next(NPCID.Bee, NPCID.BeeSmall + 1);
                            if (Main.zenithWorld)
                            {
                                if (phase3)
                                    spawnType = Main.rand.NextBool(3) ? ModContent.NPCType<PlagueChargerLarge>() : ModContent.NPCType<PlagueCharger>();
                                else
                                    spawnType = NPCID.Hellbat;
                            }

                            int spawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)beeSpawnLocation.X, (int)beeSpawnLocation.Y, spawnType);
                            Main.npc[spawn].velocity = Main.player[npc.target].Center - npc.Center;
                            Main.npc[spawn].velocity.Normalize();
                            Main.npc[spawn].velocity *= 5f;
                            if (!Main.zenithWorld)
                            {
                                Main.npc[spawn].ai[2] = enrageScale;
                                Main.npc[spawn].ai[3] = 1f;
                            }
                            Main.npc[spawn].netUpdate = true;
                        }
                    }
                }

                // Velocity calculations if target is too far away
                if (Vector2.Distance(beeSpawnLocation, hoverDestination) > 400f || !canHitTarget)
                    npc.SimpleFlyMovement(idealVelocity, beeAttackHoverAccel);
                else
                    npc.velocity *= (death ? 0.8f : 0.85f);

                // Face the correct direction
                float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                // Go to a random phase
                float numSpawns = phase3 ? (masterMode ? 1f : 2f) : (masterMode ? 3f : 5f);
                if (npc.ai[2] > numSpawns || beeLimitReached)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 2f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                }
            }

            // Stinger phase
            else if (npc.ai[0] == 3f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                // Direction
                float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                // Get target location and shoot from ass
                float stingerAttackSpeed = 16f + enrageScale * 4f;
                float stingerAttackAccel = phase6 ? 0.32f : 0.24f;
                if (enrageScale > 0f)
                    stingerAttackAccel = MathHelper.Lerp(phase6 ? 0.4f : 0.32f, phase6 ? 0.8f : 0.64f, enrageScale / maxEnrageScale);

                if (masterMode)
                {
                    stingerAttackSpeed *= 1.1f;
                    stingerAttackAccel *= 1.2f;
                }

                Vector2 stingerSpawnLocation = new Vector2(npc.Center.X + (Main.rand.Next(20) * npc.direction), npc.position.Y + npc.height * 0.8f);
                bool canHitTarget = Collision.CanHit(new Vector2(stingerSpawnLocation.X, stingerSpawnLocation.Y - 30f), 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                Vector2 hoverDestination = Main.player[npc.target].Center - Vector2.UnitY * (!canHitTarget ? 0f : phase4 ? 400f : phase2 ? 360f : 320f);
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * stingerAttackSpeed;

                npc.ai[1] += 1f;
                int stingerAttackTimer = phase6 ? 40 : phase2 ? 30 : 20;
                stingerAttackTimer -= (int)Math.Ceiling((phase6 ? 16f : phase2 ? 12f : 8f) * enrageScale);
                if (stingerAttackTimer < 5)
                    stingerAttackTimer = 5;

                // Fire stingers
                if (npc.ai[1] % stingerAttackTimer == (stingerAttackTimer - 1) && npc.Bottom.Y < Main.player[npc.target].Top.Y && Collision.CanHit(stingerSpawnLocation, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float stingerSpeed = (phase3 ? 6f : 5f) + enrageScale;
                        if (masterMode)
                            stingerSpeed += 1f;

                        float stingerTargetX = Main.player[npc.target].Center.X - stingerSpawnLocation.X;
                        float stingerTargetY = Main.player[npc.target].Center.Y - stingerSpawnLocation.Y;
                        float stingerTargetDist = (float)Math.Sqrt(stingerTargetX * stingerTargetX + stingerTargetY * stingerTargetY);
                        stingerTargetDist = stingerSpeed / stingerTargetDist;
                        stingerTargetX *= stingerTargetDist;
                        stingerTargetY *= stingerTargetDist;
                        Vector2 stingerVelocity = new Vector2(stingerTargetX, stingerTargetY);
                        int type = Main.zenithWorld ? (phase3 ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.FlamingWood) : ProjectileID.QueenBeeStinger;

                        int projectile = Projectile.NewProjectile(npc.GetSource_FromAI(), stingerSpawnLocation, stingerVelocity, type, Main.zenithWorld ? 25 : npc.GetProjectileDamage(type), 0f, Main.myPlayer, 0f, (Main.zenithWorld && phase3) ? Main.player[npc.target].position.Y : 0f);
                        Main.projectile[projectile].timeLeft = 1200;
                        Main.projectile[projectile].extraUpdates = 1;

                        if (phase2)
                        {
                            int numExtraStingers = death ? (phase6 ? 4 : 2) : (phase6 ? 2 : 1);
                            for (int i = 0; i < numExtraStingers; i++)
                            {
                                projectile = Projectile.NewProjectile(npc.GetSource_FromAI(), stingerSpawnLocation + Main.rand.NextVector2CircularEdge(16f, 16f) * (i + 1), stingerVelocity * MathHelper.Lerp(0.75f, 1f, i / (float)numExtraStingers), type, Main.zenithWorld ? 25 : npc.GetProjectileDamage(type), 0f, Main.myPlayer, 0f, (Main.zenithWorld && phase3) ? Main.player[npc.target].position.Y : 0f);
                                Main.projectile[projectile].timeLeft = 1200;
                                Main.projectile[projectile].extraUpdates = 1;
                            }
                        }
                    }
                }

                // Movement calculations
                if (Vector2.Distance(stingerSpawnLocation, hoverDestination) > 40f || !canHitTarget)
                    npc.SimpleFlyMovement(idealVelocity, stingerAttackAccel);

                // Go to a random phase
                float numStingerShots = phase6 ? 5f : phase2 ? 8f : 15f;
                if (masterMode)
                    numStingerShots = (float)Math.Round(numStingerShots * 0.5f);

                if (npc.ai[1] > stingerAttackTimer * numStingerShots)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 3f;
                    npc.netUpdate = true;
                }
            }

            else if (npc.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.localAI[0] = 1f;
                float despawnVelMult = 14f;

                Vector2 despawnTargetDist = Main.player[npc.target].Center - npc.Center;
                despawnTargetDist.Normalize();
                despawnTargetDist *= 14f;

                npc.velocity = (npc.velocity * despawnVelMult + despawnTargetDist) / (despawnVelMult + 1f);
                if (npc.velocity.X < 0f)
                    npc.direction = -1;
                else
                    npc.direction = 1;

                npc.spriteDirection = npc.direction;

                if (distanceFromTarget < 2000f)
                {
                    npc.ai[0] = -1f;
                    npc.localAI[0] = 0f;
                }
            }

            // Stinger arcs above the player
            else if (npc.ai[0] == 5f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                // Direction
                float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                // Get target location and shoot spreads from ass
                float stingerAttackSpeed = 20f + enrageScale * 4f;
                float stingerAttackAccel = phase6 ? 0.7f : 0.5f;
                if (enrageScale > 0f)
                    stingerAttackAccel = MathHelper.Lerp(phase6 ? 0.9f : 0.7f, phase6 ? 2.4f : 1.8f, enrageScale / maxEnrageScale);

                if (masterMode)
                {
                    stingerAttackSpeed *= 1.1f;
                    stingerAttackAccel *= 1.2f;
                }

                int numStingerArcs = phase6 ? 3 : phase5 ? 2 : 1;
                if (death)
                    numStingerArcs++;

                float phaseLimit = phase6 ? 180f : phase5 ? 150f : 120f;
                if (death)
                    phaseLimit *= 1.5f;

                float stingerAttackTimer = (float)Math.Ceiling(phaseLimit / (numStingerArcs + 1));

                float maxDistance = 480f;
                float xLocationScale = MathHelper.Lerp(-maxDistance, maxDistance, npc.ai[1] / phaseLimit) * npc.ai[2];
                Vector2 stingerSpawnLocation = new Vector2(npc.Center.X + (Main.rand.Next(20) * npc.direction), npc.position.Y + npc.height * 0.8f);
                bool canHitTarget = Collision.CanHit(new Vector2(stingerSpawnLocation.X, stingerSpawnLocation.Y - 30f), 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                Vector2 hoverDestination = Main.player[npc.target].Center + Vector2.UnitX * xLocationScale * (death ? 1.5f : 1.25f) - Vector2.UnitY * maxDistance;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * stingerAttackSpeed;

                // Fire stingers
                bool canFireStingers = stingerSpawnLocation.Y < Main.player[npc.target].Top.Y - maxDistance * 0.8f || !canHitTarget;
                if (canFireStingers)
                {
                    npc.ai[1] += 1f;
                    if (npc.ai[1] % stingerAttackTimer == 0f && npc.ai[1] != 0f && npc.ai[1] != phaseLimit)
                    {
                        SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float stingerSpeed = (phase6 ? 5f : 4f) + enrageScale;
                            if (masterMode)
                                stingerSpeed += 1f;

                            Vector2 projectileVelocity = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * stingerSpeed;
                            int type = Main.zenithWorld ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.QueenBeeStinger;
                            int numProj = death ? (phase6 ? 7 : phase5 ? 11 : 15) : (phase6 ? 5 : phase5 ? 9 : 13);
                            int spread = phase6 ? 30 : phase5 ? 50 : 60;

                            if (masterMode)
                            {
                                numProj += (phase6 ? 2 : phase5 ? 4 : 6);
                                spread += (phase6 ? 10 : phase5 ? 15 : 20);
                            }

                            float rotation = MathHelper.ToRadians(spread);
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                if (i % 2f != 0f)
                                    perturbedSpeed *= 0.8f;

                                int projectile = Projectile.NewProjectile(npc.GetSource_FromAI(), stingerSpawnLocation + Vector2.Normalize(perturbedSpeed) * 10f, perturbedSpeed, type, Main.zenithWorld ? 25 : npc.GetProjectileDamage(type), 0f, Main.myPlayer, 0f, Main.player[npc.target].position.Y);
                                Main.projectile[projectile].timeLeft = 1200;
                                Main.projectile[projectile].extraUpdates = 1;

                                if (!Main.zenithWorld)
                                    Main.projectile[projectile].tileCollide = false;
                            }
                        }
                    }
                }

                // Movement calculations
                npc.SimpleFlyMovement(idealVelocity, stingerAttackAccel);

                // Go to a random phase
                if (npc.ai[1] >= phaseLimit)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 4f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                }
            }

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);

            npc.netSpam = 5;

            return false;
        }

        public static bool VanillaQueenBeeAI(NPC npc, Mod mod)
        {
            if (Main.expertMode)
            {
                int num643 = (int)(20f * (1f - (float)npc.life / (float)npc.lifeMax));
                npc.defense = npc.defDefense + num643;
            }

            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            bool dead4 = Main.player[npc.target].dead;
            float num644 = 0f;
            if ((double)(npc.position.Y / 16f) < Main.worldSurface)
                num644 += 1f;

            if (!Main.player[npc.target].ZoneJungle)
                num644 += 1f;

            if (Main.getGoodWorld)
                num644 += 0.5f;

            float num645 = Vector2.Distance(npc.Center, Main.player[npc.target].Center);
            if (npc.ai[0] != 5f)
            {
                if (npc.timeLeft < 60)
                    npc.timeLeft = 60;

                if (num645 > 3000f)
                {
                    npc.ai[0] = 4f;
                    npc.netUpdate = true;
                }
            }

            if (dead4)
            {
                npc.ai[0] = 5f;
                npc.netUpdate = true;
            }

            if (npc.ai[0] == 5f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.velocity.Y *= 0.98f;
                if (npc.velocity.X < 0f)
                    npc.direction = -1;
                else
                    npc.direction = 1;

                npc.spriteDirection = npc.direction;
                if (npc.position.X < (float)(Main.maxTilesX * 8))
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X *= 0.98f;
                    else
                        npc.localAI[0] = 1f;

                    npc.velocity.X -= 0.08f;
                }
                else
                {
                    if (npc.velocity.X < 0f)
                        npc.velocity.X *= 0.98f;
                    else
                        npc.localAI[0] = 1f;

                    npc.velocity.X += 0.08f;
                }

                npc.EncourageDespawn(10);
            }
            else if (npc.ai[0] == -1f)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return false;

                float num646 = npc.ai[1];
                int num647;
                do
                {
                    num647 = Main.rand.Next(3);
                    switch (num647)
                    {
                        case 1:
                            num647 = 2;
                            break;
                        case 2:
                            num647 = 3;
                            break;
                    }
                }
                while ((float)num647 == num646);

                npc.ai[0] = num647;
                npc.ai[1] = 0f;
                npc.ai[2] = 0f;
                npc.netUpdate = true;
            }
            else if (npc.ai[0] == 0f)
            {
                int num648 = Main.masterMode ? 1 : 2;
                if (Main.expertMode)
                {
                    if (npc.life < npc.lifeMax / 2 && !Main.masterMode)
                        num648++;

                    if (npc.life < npc.lifeMax / 3)
                        num648++;

                    if (npc.life < npc.lifeMax / 5)
                        num648++;
                }

                num648 += (int)(1f * num644);
                if (npc.ai[1] > (float)(2 * num648) && npc.ai[1] % 2f == 0f)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                    return false;
                }

                if (npc.ai[1] % 2f == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.TargetClosest();
                    float num649 = 20f;
                    num649 += 20f * num644;
                    if (Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) < num649)
                    {
                        // Set damage
                        npc.damage = npc.defDamage;

                        npc.localAI[0] = 1f;
                        npc.ai[1] += 1f;
                        npc.ai[2] = 0f;
                        npc.netUpdate = true;
                        float num650 = 12f;
                        if (Main.expertMode)
                        {
                            num650 = Main.masterMode ? 20f : 16f;
                            float chargeVelocityIncrease = Main.masterMode ? 1.5f : 2f;
                            if ((double)npc.life < (double)npc.lifeMax * 0.75)
                                num650 += chargeVelocityIncrease;

                            if ((double)npc.life < (double)npc.lifeMax * 0.5)
                                num650 += chargeVelocityIncrease;

                            if ((double)npc.life < (double)npc.lifeMax * 0.25)
                                num650 += chargeVelocityIncrease;

                            if ((double)npc.life < (double)npc.lifeMax * 0.1)
                                num650 += chargeVelocityIncrease;
                        }

                        num650 += 7f * num644;
                        Vector2 vector82 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                        float num651 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector82.X;
                        float num652 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - vector82.Y;
                        float num653 = (float)Math.Sqrt(num651 * num651 + num652 * num652);
                        num653 = num650 / num653;
                        npc.velocity.X = num651 * num653;
                        npc.velocity.Y = num652 * num653;
                        npc.spriteDirection = npc.direction;
                        SoundEngine.PlaySound(SoundID.Zombie125, npc.Center);
                        return false;
                    }

                    npc.localAI[0] = 0f;
                    float num654 = Main.masterMode ? 16f : 12f;
                    float num655 = Main.masterMode ? 0.3f : 0.15f;
                    if (Main.expertMode)
                    {
                        float chargeLineUpVelocityIncrease = Main.masterMode ? 0.5f : 1f;
                        float chargeLineUpAccelerationIncrease = Main.masterMode ? 0.025f : 0.05f;
                        if ((double)npc.life < (double)npc.lifeMax * 0.75)
                        {
                            num654 += chargeLineUpVelocityIncrease;
                            num655 += chargeLineUpAccelerationIncrease;
                        }

                        if ((double)npc.life < (double)npc.lifeMax * 0.5)
                        {
                            num654 += chargeLineUpVelocityIncrease;
                            num655 += chargeLineUpAccelerationIncrease;
                        }

                        if ((double)npc.life < (double)npc.lifeMax * 0.25)
                        {
                            num654 += chargeLineUpVelocityIncrease * 2f;
                            num655 += chargeLineUpAccelerationIncrease;
                        }

                        if ((double)npc.life < (double)npc.lifeMax * 0.1)
                        {
                            num654 += chargeLineUpVelocityIncrease * 2f;
                            num655 += chargeLineUpAccelerationIncrease * 2f;
                        }
                    }

                    num654 += 3f * num644;
                    num655 += 0.5f * num644;
                    if (npc.position.Y + (float)(npc.height / 2) < Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2))
                        npc.velocity.Y += num655;
                    else
                        npc.velocity.Y -= num655;

                    if (npc.velocity.Y < 0f - num654)
                        npc.velocity.Y = 0f - num654;

                    if (npc.velocity.Y > num654)
                        npc.velocity.Y = num654;

                    if (Math.Abs(npc.position.X + (float)(npc.width / 2) - (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2))) > 600f)
                        npc.velocity.X += 0.15f * (float)npc.direction;
                    else if (Math.Abs(npc.position.X + (float)(npc.width / 2) - (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2))) < 300f)
                        npc.velocity.X -= 0.15f * (float)npc.direction;
                    else
                        npc.velocity.X *= 0.8f;

                    if (npc.velocity.X < -16f)
                        npc.velocity.X = -16f;

                    if (npc.velocity.X > 16f)
                        npc.velocity.X = 16f;

                    npc.spriteDirection = npc.direction;
                    return false;
                }

                // Set damage
                npc.damage = npc.defDamage;

                if (npc.velocity.X < 0f)
                    npc.direction = -1;
                else
                    npc.direction = 1;

                npc.spriteDirection = npc.direction;
                int num656 = Main.masterMode ? 425 : 600;
                if (Main.expertMode)
                {
                    int chargeDistanceReduction = Main.masterMode ? 25 : 50;
                    if ((double)npc.life < (double)npc.lifeMax * 0.1)
                        num656 -= chargeDistanceReduction * 6;
                    else if ((double)npc.life < (double)npc.lifeMax * 0.25)
                        num656 -= chargeDistanceReduction * 3;
                    else if ((double)npc.life < (double)npc.lifeMax * 0.5)
                        num656 -= chargeDistanceReduction * 2;
                    else if ((double)npc.life < (double)npc.lifeMax * 0.75)
                        num656 -= chargeDistanceReduction;
                }

                int num657 = 1;
                if (npc.position.X + (float)(npc.width / 2) < Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2))
                    num657 = -1;

                num656 -= (int)(100f * num644);
                bool flag34 = false;
                if (npc.direction == num657 && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) > (float)num656)
                {
                    npc.ai[2] = 1f;
                    flag34 = true;
                }

                if (Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) > (float)num656 * 1.5f)
                {
                    npc.ai[2] = 1f;
                    flag34 = true;
                }

                if (num644 > 0f && flag34)
                    npc.velocity *= 0.5f;

                if (npc.ai[2] == 1f)
                {
                    npc.TargetClosest();
                    npc.spriteDirection = npc.direction;
                    npc.localAI[0] = 0f;
                    npc.velocity *= (Main.masterMode ? 0.8f : 0.9f);
                    float num658 = Main.masterMode ? 0.2f : 0.1f;
                    if (Main.expertMode)
                    {
                        float velocityMultiplier = Main.masterMode ? 0.925f : 0.9f;
                        float velocityGateValueIncrease = Main.masterMode ? 0.025f : 0.05f;
                        if (npc.life < npc.lifeMax / 2)
                        {
                            npc.velocity *= velocityMultiplier;
                            num658 += velocityGateValueIncrease;
                        }

                        if (npc.life < npc.lifeMax / 3)
                        {
                            npc.velocity *= velocityMultiplier;
                            num658 += velocityGateValueIncrease;
                        }

                        if (npc.life < npc.lifeMax / 5)
                        {
                            npc.velocity *= velocityMultiplier;
                            num658 += velocityGateValueIncrease;
                        }
                    }

                    if (num644 > 0f)
                        npc.velocity *= 0.7f;

                    if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num658)
                    {
                        npc.ai[2] = 0f;
                        npc.ai[1] += 1f;
                        npc.netUpdate = true;
                    }
                }
                else
                    npc.localAI[0] = 1f;
            }
            else if (npc.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.TargetClosest();
                npc.spriteDirection = npc.direction;
                float num659 = Main.masterMode ? 16f : 12f;
                float num660 = 0.07f;
                if (Main.expertMode)
                    num660 = Main.masterMode ? 0.15f : 0.1f;

                Vector2 vector83 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                float num661 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector83.X;
                float num662 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - 200f - vector83.Y;
                float num663 = (float)Math.Sqrt(num661 * num661 + num662 * num662);
                if (num663 < 200f)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.netUpdate = true;
                    return false;
                }

                num663 = num659 / num663;
                if (npc.velocity.X < num661)
                {
                    npc.velocity.X += num660;
                    if (npc.velocity.X < 0f && num661 > 0f)
                        npc.velocity.X += num660;
                }
                else if (npc.velocity.X > num661)
                {
                    npc.velocity.X -= num660;
                    if (npc.velocity.X > 0f && num661 < 0f)
                        npc.velocity.X -= num660;
                }

                if (npc.velocity.Y < num662)
                {
                    npc.velocity.Y += num660;
                    if (npc.velocity.Y < 0f && num662 > 0f)
                        npc.velocity.Y += num660;
                }
                else if (npc.velocity.Y > num662)
                {
                    npc.velocity.Y -= num660;
                    if (npc.velocity.Y > 0f && num662 < 0f)
                        npc.velocity.Y -= num660;
                }
            }
            else if (npc.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.localAI[0] = 0f;
                npc.TargetClosest();
                Vector2 vector84 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.8f);
                Vector2 vector85 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                float num664 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector85.X;
                float num665 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - vector85.Y;
                float num666 = (float)Math.Sqrt(num664 * num664 + num665 * num665);
                npc.ai[1] += (Main.masterMode ? 2f : 1f);
                if (Main.expertMode)
                {
                    int num667 = 0;
                    for (int num668 = 0; num668 < Main.maxPlayers; num668++)
                    {
                        if (Main.player[num668].active && !Main.player[num668].dead && (npc.Center - Main.player[num668].Center).Length() < 1000f)
                            num667++;
                    }

                    npc.ai[1] += num667 / 2;
                    float beeTimerIncrease = Main.masterMode ? 0.125f : 0.25f;
                    if ((double)npc.life < (double)npc.lifeMax * 0.75)
                        npc.ai[1] += beeTimerIncrease;

                    if ((double)npc.life < (double)npc.lifeMax * 0.5)
                        npc.ai[1] += beeTimerIncrease;

                    if ((double)npc.life < (double)npc.lifeMax * 0.25)
                        npc.ai[1] += beeTimerIncrease;

                    if ((double)npc.life < (double)npc.lifeMax * 0.1)
                        npc.ai[1] += beeTimerIncrease;
                }

                bool flag35 = false;
                int num669 = (int)(40f - 18f * num644);
                if (npc.ai[1] > (float)num669)
                {
                    npc.ai[1] = 0f;
                    npc.ai[2]++;
                    flag35 = true;
                }

                if (Collision.CanHit(vector84, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && flag35)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int num670 = Main.rand.Next(NPCID.Bee, NPCID.BeeSmall + 1);
                        int num671 = NPC.NewNPC(npc.GetSource_FromAI(), (int)vector84.X, (int)vector84.Y, num670);
                        Main.npc[num671].velocity = Main.player[npc.target].Center - npc.Center;
                        Main.npc[num671].velocity.Normalize();
                        NPC nPC3 = Main.npc[num671];
                        nPC3.velocity *= 5f;
                        Main.npc[num671].CanBeReplacedByOtherNPCs = true;
                        Main.npc[num671].localAI[0] = 60f;
                        Main.npc[num671].netUpdate = true;
                    }
                }

                if (num666 > 400f || !Collision.CanHit(new Vector2(vector84.X, vector84.Y - 30f), 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    float num672 = Main.masterMode ? 16f : 14f;
                    float num673 = Main.masterMode ? 0.15f : 0.1f;
                    vector85 = vector84;
                    num664 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector85.X;
                    num665 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - vector85.Y;
                    num666 = (float)Math.Sqrt(num664 * num664 + num665 * num665);
                    num666 = num672 / num666;
                    if (npc.velocity.X < num664)
                    {
                        npc.velocity.X += num673;
                        if (npc.velocity.X < 0f && num664 > 0f)
                            npc.velocity.X += num673;
                    }
                    else if (npc.velocity.X > num664)
                    {
                        npc.velocity.X -= num673;
                        if (npc.velocity.X > 0f && num664 < 0f)
                            npc.velocity.X -= num673;
                    }

                    if (npc.velocity.Y < num665)
                    {
                        npc.velocity.Y += num673;
                        if (npc.velocity.Y < 0f && num665 > 0f)
                            npc.velocity.Y += num673;
                    }
                    else if (npc.velocity.Y > num665)
                    {
                        npc.velocity.Y -= num673;
                        if (npc.velocity.Y > 0f && num665 < 0f)
                            npc.velocity.Y -= num673;
                    }
                }
                else
                    npc.velocity *= (Main.masterMode ? 0.8f : 0.9f);

                npc.spriteDirection = npc.direction;
                float maxBees = Main.masterMode ? 8f : 5f;
                if (npc.ai[2] > maxBees)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 1f;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[0] == 3f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                float num674 = 4f;
                float num675 = 0.05f;
                if (Main.expertMode)
                {
                    num675 = Main.masterMode ? 0.1f : 0.075f;
                    num674 = Main.masterMode ? 8f : 6f;
                }

                num675 += 0.2f * num644;
                num674 += 6f * num644;
                Vector2 vector86 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.8f);
                Vector2 vector87 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                float num676 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector87.X;
                float num677 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - 300f - vector87.Y;
                float num678 = (float)Math.Sqrt(num676 * num676 + num677 * num677);
                npc.ai[1] += 1f;
                int num679 = 40;
                if (Main.masterMode)
                    num679 = (((double)npc.life < (double)npc.lifeMax * 0.1) ? 10 : ((npc.life < npc.lifeMax / 3) ? 15 : ((npc.life >= npc.lifeMax / 2) ? 25 : 20)));
                else if (Main.expertMode)
                    num679 = (((double)npc.life < (double)npc.lifeMax * 0.1) ? 15 : ((npc.life < npc.lifeMax / 3) ? 25 : ((npc.life >= npc.lifeMax / 2) ? 35 : 30)));

                num679 -= (int)((Main.masterMode ? 3f : 5f) * num644);
                if (npc.ai[1] % (float)num679 == (float)(num679 - 1) && npc.position.Y + (float)npc.height < Main.player[npc.target].position.Y && Collision.CanHit(vector86, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    SoundEngine.PlaySound(SoundID.Item17, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float num680 = Main.masterMode ? 11f : 8f;
                        float stingerVelocityIncrease = Main.masterMode ? 1f : 2f;
                        if (Main.expertMode)
                            num680 += stingerVelocityIncrease;

                        if (Main.expertMode && (double)npc.life < (double)npc.lifeMax * 0.1)
                            num680 += stingerVelocityIncrease * 1.5f;

                        num680 += 7f * num644;
                        int num681 = (int)(80f - 39f * num644);
                        int num682 = (int)(40f - 19f * num644);
                        if (num681 < 1)
                            num681 = 1;

                        if (num682 < 1)
                            num682 = 1;

                        float num683 = Main.player[npc.target].position.X + (float)Main.player[npc.target].width * 0.5f - vector86.X + (float)Main.rand.Next(-num681, num681 + 1);
                        float num684 = Main.player[npc.target].position.Y + (float)Main.player[npc.target].height * 0.5f - vector86.Y + (float)Main.rand.Next(-num682, num682 + 1);
                        float num685 = (float)Math.Sqrt(num683 * num683 + num684 * num684);
                        num685 = num680 / num685;
                        num683 *= num685;
                        num684 *= num685;
                        int num686 = 11;
                        int num687 = ProjectileID.QueenBeeStinger;
                        int num688 = Projectile.NewProjectile(npc.GetSource_FromAI(), vector86.X, vector86.Y, num683, num684, num687, num686, 0f, Main.myPlayer);
                        Main.projectile[num688].timeLeft = 300;
                    }
                }

                if (!Collision.CanHit(new Vector2(vector86.X, vector86.Y - 30f), 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    num674 = Main.masterMode ? 16f : 14f;
                    num675 = Main.masterMode ? 0.15f : 0.1f;
                    if (num644 > 0f)
                        num675 = 0.5f;

                    vector87 = vector86;
                    num676 = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - vector87.X;
                    num677 = Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2) - vector87.Y;
                    num678 = (float)Math.Sqrt(num676 * num676 + num677 * num677);
                    num678 = num674 / num678;
                    if (npc.velocity.X < num676)
                    {
                        npc.velocity.X += num675;
                        if (npc.velocity.X < 0f && num676 > 0f)
                            npc.velocity.X += num675;
                    }
                    else if (npc.velocity.X > num676)
                    {
                        npc.velocity.X -= num675;
                        if (npc.velocity.X > 0f && num676 < 0f)
                            npc.velocity.X -= num675;
                    }

                    if (npc.velocity.Y < num677)
                    {
                        npc.velocity.Y += num675;
                        if (npc.velocity.Y < 0f && num677 > 0f)
                            npc.velocity.Y += num675;
                    }
                    else if (npc.velocity.Y > num677)
                    {
                        npc.velocity.Y -= num675;
                        if (npc.velocity.Y > 0f && num677 < 0f)
                            npc.velocity.Y -= num675;
                    }
                }
                else if (num678 > 100f)
                {
                    npc.TargetClosest();
                    npc.spriteDirection = npc.direction;
                    num678 = num674 / num678;
                    if (npc.velocity.X < num676)
                    {
                        npc.velocity.X += num675;
                        if (npc.velocity.X < 0f && num676 > 0f)
                            npc.velocity.X += num675 * 2f;
                    }
                    else if (npc.velocity.X > num676)
                    {
                        npc.velocity.X -= num675;
                        if (npc.velocity.X > 0f && num676 < 0f)
                            npc.velocity.X -= num675 * 2f;
                    }

                    if (npc.velocity.Y < num677)
                    {
                        npc.velocity.Y += num675;
                        if (npc.velocity.Y < 0f && num677 > 0f)
                            npc.velocity.Y += num675 * 2f;
                    }
                    else if (npc.velocity.Y > num677)
                    {
                        npc.velocity.Y -= num675;
                        if (npc.velocity.Y > 0f && num677 < 0f)
                            npc.velocity.Y -= num675 * 2f;
                    }
                }

                float num689 = Main.masterMode ? 16f : 20f;
                num689 -= 5f * num644;
                if (npc.ai[1] > (float)num679 * num689)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 3f;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.localAI[0] = 1f;
                float num690 = 14f;
                float num691 = 14f;
                Vector2 vector88 = Main.player[npc.target].Center - npc.Center;
                vector88.Normalize();
                vector88 *= num690;
                npc.velocity = (npc.velocity * num691 + vector88) / (num691 + 1f);
                if (npc.velocity.X < 0f)
                    npc.direction = -1;
                else
                    npc.direction = 1;

                npc.spriteDirection = npc.direction;
                if (num645 < 2000f)
                {
                    npc.ai[0] = -1f;
                    npc.localAI[0] = 0f;
                }
            }

            return false;
        }
    }
}
