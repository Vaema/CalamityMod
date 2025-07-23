using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class QueenBeeAI
    {
        // Vanilla values
        public static int StingerDamage = 11; // 44; Also applies to GFB stinger replacements

        // Death exclusive
        public static int BeenadeDamage = 15; // 90
        public static int BeenadeBeeDamage = 12; // 72

        public static bool BuffedQueenBeeAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            bool enrage = !BossRushEvent.BossRushActive;
            int targetTileX = (int)Main.player[npc.target].Center.X / 16;
            int targetTileY = (int)Main.player[npc.target].Center.Y / 16;

            Tile tile = Framing.GetTileSafely(targetTileX, targetTileY);
            if (tile.WallType == WallID.HiveUnsafe)
                enrage = false;

            float maxEnrageScale = 2f;
            float enrageScale = death ? 0.5f : 0f;
            if ((npc.position.Y / 16f) < Main.worldSurface && enrage)
            {
                calamityGlobalNPC.CurrentlyEnraged = true;
                enrageScale += 0.5f;
            }
            if (!Main.player[npc.target].ZoneJungle && enrage)
            {
                calamityGlobalNPC.CurrentlyEnraged = true;
                enrageScale += 0.5f;
            }

            if (CalamityWorld.LegendaryMode)
                enrageScale += 1f;

            if (enrageScale > maxEnrageScale)
                enrageScale = maxEnrageScale;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Bee spawn limit
            int beeLimit = death ? 9 : 15;

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

            // Hornet spawn limit
            int hornetLimit = 3;
            bool hornetLimitReached = false;

            // Only run this when necessary
            if (death)
            {
                // Queen Bee Hornet count
                int totalHornets = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC hornet = Main.npc[i];
                    bool isQueenBeeHornet = hornet.ai[3] == 1f;
                    if (hornet.active && (hornet.type == NPCID.LittleHornetHoney || hornet.type == NPCID.HornetHoney || hornet.type == NPCID.BigHornetHoney) && isQueenBeeHornet)
                    {
                        int hornetCountIncrement = hornet.type == NPCID.BigHornetHoney ? 3 : hornet.type == NPCID.HornetHoney ? 2 : 1;
                        totalHornets += hornetCountIncrement;
                        if (totalHornets >= hornetLimit)
                        {
                            hornetLimitReached = true;
                            break;
                        }
                    }
                }
            }
            else
                hornetLimitReached = true;

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
            if (npc.ai[0] != 7f)
            {
                if (npc.timeLeft < 60)
                    npc.timeLeft = 60;
                if (distanceFromTarget > 3000f)
                    npc.ai[0] = 4f;
            }
            if (Main.player[npc.target].dead)
                npc.ai[0] = 7f;

            // Adjust slowing debuff immunity
            bool immuneToSlowingDebuffs = npc.ai[0] == 0f;
            npc.buffImmune[ModContent.BuffType<GlacialState>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<TemporalSadness>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<Eutrophication>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<TimeDistortion>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<GalvanicCorrosion>()] = immuneToSlowingDebuffs;
            npc.buffImmune[ModContent.BuffType<Vaporfied>()] = immuneToSlowingDebuffs;
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
            if (npc.ai[0] == 7f)
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
                    int maxRandom = phase4 ? (death ? 6 : 5) : 4;
                    do phase = Main.rand.Next(maxRandom);
                    while (phase == npc.ai[1] || phase == 1 || (phase == 2 && phase4) || (death && phase6 && phase == 3));

                    bool charging = phase == 0;
                    bool stingerArcs = phase == 4;
                    bool beenadeVomit = phase == 5;

                    // 5 is stinger arc and charge
                    if (stingerArcs)
                        phase = 5;

                    // 6 is beenade vomit
                    if (beenadeVomit)
                        phase = 6;

                    CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                    npc.ai[0] = phase;
                    npc.ai[1] = 0f;

                    // Movement direction for the stinger arcs
                    npc.ai[2] = (phase == 5 && phase5) ? (Main.rand.NextBool() ? 1f : -1f) : phase == 5 ? 1f : 0f;

                    // Velocity for the charges
                    if (death)
                        npc.ai[3] = charging ? ((phase6 ? 27f : phase5 ? 16f : phase4 ? 27f : phase2 ? 22f : 17f) + 3f * enrageScale) : 0f;
                    else
                        npc.ai[3] = charging ? ((phase6 ? 25f : phase5 ? 14f : phase4 ? 25f : phase2 ? 20f : 15f) + 3f * enrageScale) : 0f;

                    // Distance for the charges
                    if (death)
                        calamityGlobalNPC.newAI[1] = charging ? ((phase6 ? 700f : phase5 ? 300f : phase4 ? 600f : phase2 ? 500f : 400f) - 50f * enrageScale) : 0f;
                    else
                        calamityGlobalNPC.newAI[1] = charging ? ((phase6 ? 750f : phase5 ? 350f : phase4 ? 650f : phase2 ? 550f : 450f) - 50f * enrageScale) : 0f;

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

                int deathChargeLimit = phase5 ? 3 : 2;
                if (death && chargeAmt > deathChargeLimit)
                    chargeAmt = deathChargeLimit;

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
                    chargeDistanceY += 50f * enrageScale;
                    if (death)
                    {
                        chargeDistanceY += MathHelper.Lerp(0f, 100f, 1f - lifeRatio);
                        chargeDistanceY *= 2f;
                    }

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
                    float chargeAccelerationX = (phase4 ? 0.7f : phase2 ? 0.6f : 0.5f) + 0.25f * enrageScale;
                    float chargeAccelerationY = (phase4 ? 0.35f : phase2 ? 0.3f : 0.25f) + 0.125f * enrageScale;

                    if (death)
                    {
                        chargeVelocityX += 2f;
                        chargeVelocityY += 4f;
                        chargeAccelerationX += 0.1f;
                        chargeAccelerationY += 0.2f;
                    }

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
                    npc.ForceNetUpdate(false);
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
                        npc.velocity *= MathHelper.Lerp(0.5f, death ? 0.9f : 1f, 1f - enrageScale / maxEnrageScale);

                    // Keep moving
                    if (npc.ai[2] != 1f)
                    {
                        // Velocity fix if Queen Bee is slowed
                        if (npc.velocity.Length() < velocity)
                            npc.velocity.X = velocity * npc.direction;

                        float accelerateGateValue = phase6 ? 30f : phase5 ? 10f : 90f;
                        if (death)
                            accelerateGateValue *= 0.75f;
                        if (enrageScale > 0f)
                            accelerateGateValue *= 0.75f;

                        calamityGlobalNPC.newAI[0] += 1f;
                        if (calamityGlobalNPC.newAI[0] > accelerateGateValue)
                        {
                            npc.SyncExtraAI();
                            float velocityXLimit = velocity * 2f;
                            if (Math.Abs(npc.velocity.X) < velocityXLimit)
                                npc.velocity.X *= (death ? 1.02f : 1.01f);
                        }

                        // Spawn bees
                        float beeSpawnGateValue = death ? 16f : 20f;
                        bool spawnBee = phase4 && calamityGlobalNPC.newAI[0] % beeSpawnGateValue == 0f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                        if (spawnBee)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit18, npc.Center);

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
                                else if (death)
                                {
                                    int random = hornetLimitReached ? 0 : beeLimitReached ? Main.rand.Next(6, 12) : Main.rand.Next(12);
                                    switch (random)
                                    {
                                        default:
                                        case 0:
                                        case 1:
                                        case 2:
                                        case 3:
                                        case 4:
                                        case 5:
                                            break;

                                        case 6:
                                        case 7:
                                        case 8:
                                            spawnType = NPCID.LittleHornetHoney;
                                            break;

                                        case 9:
                                        case 10:
                                            spawnType = NPCID.HornetHoney;
                                            break;

                                        case 11:
                                            spawnType = NPCID.BigHornetHoney;
                                            break;
                                    }
                                }

                                if (!beeLimitReached || !hornetLimitReached)
                                {
                                    int spawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, spawnType);
                                    Vector2 beeVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY);
                                    Main.npc[spawn].velocity = beeVelocity;
                                    Main.npc[spawn].velocity *= 5f;
                                    if (!Main.zenithWorld)
                                    {
                                        Main.npc[spawn].ai[2] = enrageScale;
                                        Main.npc[spawn].ai[3] = 1f;
                                    }
                                    Main.npc[spawn].timeLeft = 600;
                                    Main.npc[spawn].netUpdate = true;
                                }
                            }
                        }

                        npc.localAI[0] = 1f;
                        return false;
                    }

                    // Avoid cheap bullshit
                    npc.damage = 0;

                    float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                    npc.direction = playerLocation < 0 ? 1 : -1;
                    npc.spriteDirection = npc.direction;

                    // Slow down
                    npc.localAI[0] = 0f;
                    npc.velocity *= (death ? 0.8f : 0.9f);

                    float chargeDeceleration = death ? 0.2f : 0.1f;
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
                        npc.velocity *= MathHelper.Lerp(0.7f, 1f, 1f - enrageScale / maxEnrageScale);

                    if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < chargeDeceleration)
                    {
                        npc.ai[2] = 0f;
                        npc.ai[1] += 1f;
                        calamityGlobalNPC.newAI[0] = 0f;
                        npc.SyncExtraAI();
                    }

                    npc.ForceNetUpdate(false);
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
                float beeAttackAccel = death ? 0.6f : 0.24f;
                float beeAttackSpeed = 12f + enrageScale * 3f;

                if (death)
                    beeAttackSpeed *= 1.5f;

                bool canHitTarget = Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height);
                float distanceAboveTarget = !canHitTarget ? 0f : 320f;
                Vector2 hoverDestination = Main.player[npc.target].Center - Vector2.UnitY * distanceAboveTarget;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * beeAttackSpeed;

                // Go to bee spawn phase
                calamityGlobalNPC.newAI[0] += 1f;
                if ((Vector2.Distance(npc.Center, hoverDestination) < 400f && canHitTarget) || calamityGlobalNPC.newAI[0] >= (death ? 90f : 180f))
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
                float beeAttackHoverAccel = death ? 0.8f : 0.3f;

                if (death)
                    beeAttackHoverSpeed *= 1.5f;

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
                if (Collision.CanHit(beeSpawnLocation, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && spawnBee && (!beeLimitReached || !hornetLimitReached))
                {
                    SoundEngine.PlaySound(SoundID.NPCHit18, beeSpawnLocation);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (phase3)
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
                            else if (death)
                            {
                                int random = hornetLimitReached ? 0 : beeLimitReached ? Main.rand.Next(6, 12) : Main.rand.Next(12);
                                switch (random)
                                {
                                    default:
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                        break;

                                    case 6:
                                    case 7:
                                    case 8:
                                        spawnType = NPCID.LittleHornetHoney;
                                        break;

                                    case 9:
                                    case 10:
                                        spawnType = NPCID.HornetHoney;
                                        break;

                                    case 11:
                                        spawnType = NPCID.BigHornetHoney;
                                        break;
                                }
                            }

                            int spawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)beeSpawnLocation.X, (int)beeSpawnLocation.Y, spawnType);
                            Vector2 beeVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY);
                            Main.npc[spawn].velocity = beeVelocity;
                            Main.npc[spawn].velocity *= 5f;
                            if (!Main.zenithWorld)
                            {
                                Main.npc[spawn].ai[2] = enrageScale;
                                Main.npc[spawn].ai[3] = 1f;
                            }
                            Main.npc[spawn].timeLeft = 600;
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
                float numSpawns = phase3 ? (death ? 1f : 2f) : (death ? 3f : 5f);
                if (npc.ai[2] > numSpawns || (beeLimitReached && hornetLimitReached))
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
                float stingerAttackAccel = phase6 ? 0.16f : 0.12f;
                if (enrageScale > 0f)
                    stingerAttackAccel = MathHelper.Lerp(phase6 ? 0.3f : 0.24f, phase6 ? 0.6f : 0.48f, enrageScale / maxEnrageScale);

                if (death)
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
                        if (death)
                            stingerSpeed += 1f;

                        float stingerTargetX = Main.player[npc.target].Center.X - stingerSpawnLocation.X;
                        float stingerTargetY = Main.player[npc.target].Center.Y - stingerSpawnLocation.Y;
                        float stingerTargetDist = (float)Math.Sqrt(stingerTargetX * stingerTargetX + stingerTargetY * stingerTargetY);
                        stingerTargetDist = stingerSpeed / stingerTargetDist;
                        stingerTargetX *= stingerTargetDist;
                        stingerTargetY *= stingerTargetDist;
                        Vector2 stingerVelocity = new Vector2(stingerTargetX, stingerTargetY);
                        int type = Main.zenithWorld ? (phase3 ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.FlamingWood) : ProjectileID.QueenBeeStinger;

                        int projectile = Projectile.NewProjectile(npc.GetSource_FromAI(), stingerSpawnLocation, stingerVelocity, type, StingerDamage, 0f, Main.myPlayer, 0f, (Main.zenithWorld && phase3) ? Main.player[npc.target].position.Y : 0f);
                        Main.projectile[projectile].timeLeft = 1200;
                        Main.projectile[projectile].extraUpdates = 1;

                        if (phase2)
                        {
                            int numExtraStingers = death ? (phase6 ? 4 : 2) : (phase6 ? 2 : 1);
                            for (int i = 0; i < numExtraStingers; i++)
                            {
                                projectile = Projectile.NewProjectile(npc.GetSource_FromAI(), stingerSpawnLocation + Main.rand.NextVector2CircularEdge(16f, 16f) * (i + 1), stingerVelocity * MathHelper.Lerp(0.75f, 1f, i / (float)numExtraStingers), type, StingerDamage, 0f, Main.myPlayer, 0f, (Main.zenithWorld && phase3) ? Main.player[npc.target].position.Y : 0f);
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
                if (death)
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

                Vector2 despawnTargetDist = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY);
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

                if (death)
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
                if (canFireStingers && npc.ai[1] < phaseLimit)
                {
                    npc.ai[1] += 1f;
                    if (npc.ai[1] % stingerAttackTimer == 0f && npc.ai[1] != 0f && npc.ai[1] != phaseLimit)
                    {
                        SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float stingerSpeed = (phase6 ? 5f : 4f) + enrageScale;
                            if (death)
                                stingerSpeed += 1f;

                            Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * stingerSpeed;
                            int type = Main.zenithWorld ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.QueenBeeStinger;
                            int numProj = death ? (phase6 ? 7 : phase5 ? 11 : 15) : (phase6 ? 5 : phase5 ? 9 : 13);
                            int spread = phase6 ? 30 : phase5 ? 50 : 60;

                            if (death)
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

                                int projectile = Projectile.NewProjectile(npc.GetSource_FromAI(), stingerSpawnLocation + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 10f, perturbedSpeed, type, StingerDamage, 0f, Main.myPlayer, 0f, Main.player[npc.target].position.Y);
                                Main.projectile[projectile].timeLeft = 1200;
                                Main.projectile[projectile].extraUpdates = 1;

                                if (!Main.zenithWorld)
                                    Main.projectile[projectile].tileCollide = false;
                            }
                        }
                    }
                }

                // Go to a random phase after pausing for a bit
                if (npc.ai[1] >= phaseLimit)
                {
                    npc.ai[1] += 1f;

                    if (npc.Distance(Main.player[npc.target].Center) > 400f || !canHitTarget)
                    {
                        idealVelocity = npc.SafeDirectionTo(Main.player[npc.target].Center) * stingerAttackSpeed;
                        npc.SimpleFlyMovement(idealVelocity * 0.5f, stingerAttackAccel * 0.5f);
                    }
                    else
                        npc.velocity *= 0.8f;

                    float idleTime = death ? 120f : 180f;
                    if (npc.ai[1] >= phaseLimit + idleTime)
                    {
                        npc.ai[0] = -1f;
                        npc.ai[1] = 4f;
                        npc.ai[2] = 0f;
                        npc.netUpdate = true;
                    }
                }
                else
                    npc.SimpleFlyMovement(idealVelocity, stingerAttackAccel);
            }

            // Spit beenades
            else if (npc.ai[0] == 6f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                // Direction
                float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                float beenadeAttackSpeed = 16f + 4f * enrageScale;
                float beenadeAttackAccel = 0.5f;
                Vector2 targetVector = Main.player[npc.target].Center - Vector2.UnitY * 240f;

                if (npc.Distance(targetVector) > 160f)
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(targetVector) * beenadeAttackSpeed, beenadeAttackAccel);
                else
                    npc.velocity *= 0.8f;

                float phaseLimit = 180f;
                npc.ai[1] += 1f;
                if (npc.ai[1] >= phaseLimit)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 5f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                }
                else if (npc.ai[1] % 61f == 0f)
                {
                    Vector2 beenadeSpawnLocation = npc.Center + new Vector2(20f * npc.direction, -40f);
                    SoundEngine.PlaySound(SoundID.Item76, beenadeSpawnLocation);
                    int type = ModContent.ProjectileType<QueenBeenade>();
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(npc.GetSource_FromAI(), beenadeSpawnLocation, Vector2.UnitX * 6f * npc.direction, type, BeenadeDamage, 0f, Main.myPlayer);
                }
            }

            if (Main.dedServ)
                npc.ForceNetUpdate();

            return false;
        }
    }
}
