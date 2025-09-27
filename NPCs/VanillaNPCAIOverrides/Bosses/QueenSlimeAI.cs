using System;
using System.Collections.Generic;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class QueenSlimeAI
    {
        // Vanilla values
        public static int SmallGelDamage = Main.masterMode ? 20 : 17; // 68
        public static int SpikeDamage = Main.masterMode ? 20 : 17; // 68
        public static int LargeGelDamage = 30; // 120
        public static int SlamDamage = 40; // 160

        public static bool BuffedQueenSlimeAI(NPC npc, Mod mod)
        {
            // Difficulty bools
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            float slimeScale = 1f;
            bool teleported = false;
            bool phase2 = lifeRatio <= 0.5f;
            bool phase3 = lifeRatio <= (death ? 0.5f : 0.4f);
            bool phase4 = lifeRatio <= (death ? 0.5f : 0.2f);
            bool phase5 = lifeRatio <= 0.25f && death;

            // Spawn settings
            if (npc.localAI[0] == 0f)
            {
                npc.ai[1] = -20f;
                npc.localAI[0] = npc.lifeMax;

                CalamityUtils.CalamityTargeting(npc, default);

                npc.netUpdate = true;
            }

            // Emit light
            Lighting.AddLight(npc.Center, 1f, 0.7f, 0.9f);

            // Despawn
            int despawnDistanceInTiles = 500;
            if (Main.player[npc.target].dead || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > despawnDistanceInTiles)
            {
                CalamityUtils.CalamityTargeting(npc, default);

                if (Main.player[npc.target].dead || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > despawnDistanceInTiles)
                {
                    npc.EncourageDespawn(10);
                    if (Main.player[npc.target].Center.X < npc.Center.X)
                        npc.direction = 1;
                    else
                        npc.direction = -1;
                }
            }

            // Slow down dramatically while teleporting
            if (npc.ai[0] == 1f || npc.ai[0] == 2f)
            {
                if (Math.Abs(npc.velocity.X) > 0.1f)
                {
                    npc.velocity.X *= 0.8f;
                    if (Math.Abs(npc.velocity.X) <= 0.1f)
                        npc.velocity.X = 0f;
                }
            }

            // Teleport
            float teleportGateValue = death ? 300f : 600f;
            if (!Main.player[npc.target].dead && npc.timeLeft > 10 && !phase2 && npc.ai[3] >= teleportGateValue && npc.ai[0] == 0f && npc.velocity.Y == 0f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.ai[0] = 2f;
                npc.ai[1] = 0f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.netUpdate = true;
                    
                    CalamityTargetingParameters options = CalamityTargetingParameters.Defaults;
                    options.faceTarget = false;
                    CalamityUtils.CalamityTargeting(npc, options);

                    Vector2 vectorAimedAheadOfTarget = Main.player[npc.target].Center + new Vector2((float)Math.Round(Main.player[npc.target].velocity.X), 0f).SafeNormalize(Vector2.Zero) * 800f;
                    Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
                    int randomTeleportOffset = 5;
                    int teleportTries = 0;
                    while (teleportTries < 100)
                    {
                        teleportTries++;
                        int teleportTileX = Main.rand.Next(predictiveTeleportPoint.X - randomTeleportOffset, predictiveTeleportPoint.X + randomTeleportOffset + 1);
                        int teleportTileY = Main.rand.Next(predictiveTeleportPoint.Y - randomTeleportOffset, predictiveTeleportPoint.Y);
                        if (Main.tile[teleportTileX, teleportTileY].HasUnactuatedTile)
                            continue;

                        bool canTeleportToTile = true;
                        if (canTeleportToTile && Main.tile[teleportTileX, teleportTileY].LiquidType == LiquidID.Lava)
                            canTeleportToTile = false;
                        if (canTeleportToTile && !Collision.CanHitLine(npc.Center, 0, 0, vectorAimedAheadOfTarget, 0, 0))
                            canTeleportToTile = false;

                        if (canTeleportToTile)
                        {
                            npc.localAI[1] = teleportTileX * 16 + 8;
                            npc.localAI[2] = teleportTileY * 16 + 16;
                            npc.ai[3] = 0f;
                            break;
                        }
                    }

                    // Default teleport if the above conditions aren't met in 100 iterations
                    if (teleportTries >= 100)
                    {
                        Vector2 bottom = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].Bottom;
                        npc.localAI[1] = bottom.X;
                        npc.localAI[2] = bottom.Y;
                        npc.ai[3] = 0f;
                    }
                }
            }

            // Get ready to teleport by increasing ai[3]
            if (!phase2)
            {
                if (npc.ai[3] < teleportGateValue)
                {
                    if (!Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0) || Math.Abs(npc.Top.Y - Main.player[npc.target].Bottom.Y) > 320f)
                        npc.ai[3] += death ? 3f : 2f;
                    else
                        npc.ai[3] += 1f;
                }
            }
            else
            {
                float teleportNetUpdate = npc.ai[3];
                npc.ai[3] -= 1f;
                if (npc.ai[3] < 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient && teleportNetUpdate > 0f)
                        npc.netUpdate = true;

                    npc.ai[3] = 0f;
                }
            }

            // Reset variables if despawning
            if (npc.timeLeft <= 10 && ((phase2 && npc.ai[0] != 0f) || (!phase2 && npc.ai[0] != 3f)))
            {
                if (phase2)
                    npc.ai[0] = 0f;
                else
                    npc.ai[0] = 3f;

                npc.ai[1] = 0f;
                npc.ai[2] = 0f;
                npc.ai[3] = 0f;
                npc.netUpdate = true;
            }

            npc.noTileCollide = false;
            npc.noGravity = false;

            // Frame data shit I guess?
            if (phase2)
            {
                npc.localAI[3] += 1f;
                if (npc.localAI[3] >= 24f)
                    npc.localAI[3] = 0f;

                if ((npc.ai[0] == 4f || npc.ai[0] == 6f) && npc.ai[2] == 1f)
                    npc.localAI[3] = 6f;

                if (npc.ai[0] == 5f && npc.ai[2] != 1f)
                    npc.localAI[3] = 7f;
            }

            // Phases
            switch ((int)npc.ai[0])
            {
                // Phase switch phase
                case 0:

                    // Avoid cheap bullshit
                    npc.damage = 0;

                    if (phase2)
                    {
                        QueenSlime_FlyMovement(npc);
                    }
                    else
                    {
                        npc.noTileCollide = false;
                        npc.noGravity = false;
                        if (npc.velocity.Y == 0f)
                        {
                            npc.velocity.X *= 0.8f;
                            if (npc.velocity.X > -0.1 && npc.velocity.X < 0.1)
                                npc.velocity.X = 0f;
                        }
                    }

                    if (npc.timeLeft <= 10 || (!phase2 && npc.velocity.Y != 0f))
                        break;

                    npc.ai[1] += 1f;
                    int idleTime = death ? 30 : 40;
                    if (phase2)
                        idleTime = death ? 60 : 80;
                    if (phase4)
                        idleTime /= 2;

                    if (!(npc.ai[1] > idleTime))
                        break;

                    npc.ai[1] = 0f;
                    if (phase2)
                    {
                        Player player = Main.player[npc.target];

                        switch ((int)npc.Calamity().newAI[0])
                        {
                            default:
                                npc.ai[0] = Main.rand.NextBool() ? 6f : 5f;
                                break;
                            case 5:
                                npc.ai[0] = phase4 ? 6f : Main.rand.NextBool() ? 4f : 6f;
                                break;
                            case 6:
                                npc.ai[0] = phase4 ? 5f : Main.rand.NextBool() ? 5f : 4f;
                                break;
                        }

                        if (npc.ai[0] == 4f || npc.ai[0] == 6f)
                        {
                            npc.ai[2] = 1f;
                            if (player != null && player.active && !player.dead && (player.Bottom.Y < npc.Bottom.Y || Math.Abs(player.Center.X - npc.Center.X) > 450f))
                            {
                                npc.ai[0] = 5f;
                                npc.ai[2] = 0f;
                            }
                        }
                    }
                    else
                    {
                        switch ((int)npc.Calamity().newAI[0])
                        {
                            default:
                                npc.ai[0] = Main.rand.NextBool() ? 5f : 4f;
                                break;
                            case 4:
                                npc.ai[0] = Main.rand.NextBool() ? 3f : 5f;
                                break;
                            case 5:
                                npc.ai[0] = Main.rand.NextBool() ? 4f : 3f;
                                break;
                        }
                    }

                    npc.netUpdate = true;
                    break;

                // Enlarge after teleport
                case 1:

                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.rotation = 0f;
                    npc.ai[1] += 1f;
                    float teleportEndTime = death ? 15f : 20f;
                    slimeScale = MathHelper.Clamp(npc.ai[1] / teleportEndTime, 0f, 1f);
                    slimeScale = 0.5f + slimeScale * 0.5f;
                    if (npc.ai[1] >= teleportEndTime && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.ai[0] = 0f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;

                        CalamityUtils.CalamityTargeting(npc, default);
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient && npc.ai[1] >= teleportEndTime * 2f)
                    {
                        npc.ai[0] = 0f;
                        npc.ai[1] = 0f;

                        CalamityUtils.CalamityTargeting(npc, default);
                    }

                    // Emit teleport dust
                    Color newColor2 = NPC.AI_121_QueenSlime_GetDustColor();
                    newColor2.A = 150;
                    for (int i = 0; i < 10; i++)
                    {
                        int queenSlimeDust = Dust.NewDust(npc.position + Vector2.UnitX * -20f, npc.width + 40, npc.height, DustID.TintableDust, npc.velocity.X, npc.velocity.Y, 50, newColor2, 1.5f);
                        Main.dust[queenSlimeDust].noGravity = true;
                        Main.dust[queenSlimeDust].velocity *= 2f;
                    }

                    break;

                // Shrink and spawn teleport gore and dust
                case 2:

                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.rotation = 0f;
                    npc.ai[1] += 1f;
                    float teleportTime = death ? 30f : 40f;
                    slimeScale = MathHelper.Clamp((teleportTime - npc.ai[1]) / teleportTime, 0f, 1f);
                    slimeScale = 0.5f + slimeScale * 0.5f;

                    if (npc.ai[1] >= teleportTime)
                        teleported = true;

                    // Spawn crown gore
                    if (npc.ai[1] == teleportTime)
                    {
                        Gore.NewGore(npc.GetSource_FromAI(), npc.Center + new Vector2(-40f, -npc.height / 2), npc.velocity, GoreID.QueenSlimeCrown);

                        // Spawn spread of projectiles in Master Mode
                        if (death)
                        {
                            int numGelProjectiles = 12;
                            if (Main.getGoodWorld)
                                numGelProjectiles = 15;

                            float gelVelocity = death ? 20f : 16f;
                            int type = ProjectileID.QueenSlimeGelAttack;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int j = 0; j < numGelProjectiles; j++)
                                {
                                    Vector2 spinningpoint = new Vector2(gelVelocity, 0f);

                                    if (Main.getGoodWorld)
                                        spinningpoint *= Main.rand.NextFloat() + 0.5f;

                                    spinningpoint = spinningpoint.RotatedBy((float)(-j) * MathHelper.TwoPi / (float)numGelProjectiles, Vector2.Zero);
                                    int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, spinningpoint, type, LargeGelDamage, 0f, Main.myPlayer, 0f, -2f);
                                    Main.projectile[proj].timeLeft = 900;
                                }
                            }

                            SoundEngine.PlaySound(SoundID.Item155, npc.Center);
                        }
                    }

                    if (npc.ai[1] >= teleportTime && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.Bottom = new Vector2(npc.localAI[1], npc.localAI[2]);
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient && npc.ai[1] >= teleportTime * 2f)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                    }

                    // Emit teleport dust
                    if (!teleported)
                    {
                        Color newColor = NPC.AI_121_QueenSlime_GetDustColor();
                        newColor.A = 150;
                        for (int n = 0; n < 10; n++)
                        {
                            int queenSlimeDust2 = Dust.NewDust(npc.position + Vector2.UnitX * -20f, npc.width + 40, npc.height, DustID.TintableDust, npc.velocity.X, npc.velocity.Y, 50, newColor, 1.5f);
                            Main.dust[queenSlimeDust2].noGravity = true;
                            Main.dust[queenSlimeDust2].velocity *= 0.5f;
                        }
                    }

                    break;

                // She jump
                case 3:

                    // Faster fall
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y += death ? 0.05f : 0f;

                    npc.rotation = 0f;
                    if (npc.velocity.Y == 0f)
                    {
                        // Avoid cheap bullshit
                        npc.damage = 0;

                        npc.velocity.X *= 0.8f;
                        if (npc.velocity.X > -0.1 && npc.velocity.X < 0.1)
                            npc.velocity.X = 0f;

                        float timerIncrement = death ? 6f : 5f;
                        npc.ai[1] += timerIncrement;
                        if (lifeRatio < 0.85f)
                            npc.ai[1] += timerIncrement;
                        if (lifeRatio < 0.7f)
                            npc.ai[1] += timerIncrement;

                        if (!(npc.ai[1] >= 0f))
                            break;

                        // Set damage
                        npc.damage = npc.defDamage;

                        float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                        float speedMult = 1f;
                        if (distanceBelowTarget > 0f)
                            speedMult += distanceBelowTarget * 0.002f;

                        if (speedMult > 2f)
                            speedMult = 2f;

                        npc.netUpdate = true;

                        CalamityUtils.CalamityTargeting(npc, default);

                        if (npc.ai[2] == 3f)
                        {
                            npc.velocity.Y = -13f * speedMult;
                            npc.velocity.X += (death ? 6f : 5.5f) * npc.direction;
                            npc.ai[1] = 0f;
                            npc.ai[2] = 0f;
                            if (npc.timeLeft > 10)
                            {
                                npc.Calamity().newAI[0] = npc.ai[0];
                                npc.SyncExtraAI();
                                npc.ai[0] = 0f;
                            }
                            else
                                npc.ai[1] = -60f;
                        }
                        else if (npc.ai[2] == 2f)
                        {
                            npc.velocity.Y = -(death ? 8f : 6f) * speedMult;
                            npc.velocity.X += (death ? 7.5f : 7f) * npc.direction;
                            npc.ai[1] = -40f;
                            npc.ai[2] += 1f;
                        }
                        else
                        {
                            npc.velocity.Y = -(death ? 10f : 8f) * speedMult;
                            npc.velocity.X += (death ? 6.5f : 6f) * npc.direction;
                            npc.ai[1] = -40f;
                            npc.ai[2] += 1f;
                        }

                        if (death)
                            npc.velocity.X *= (death ? 1.4f : 1.2f);

                        npc.noTileCollide = true;
                    }
                    else
                    {
                        if (npc.target >= Main.maxPlayers)
                            break;

                        float jumpVelocity = death ? 7f : 4.5f;
                        if (Main.getGoodWorld)
                            jumpVelocity = 12f;

                        if ((npc.direction == 1 && npc.velocity.X < jumpVelocity) || (npc.direction == -1 && npc.velocity.X > 0f - jumpVelocity))
                        {
                            if ((npc.direction == -1 && npc.velocity.X < 0.1) || (npc.direction == 1 && npc.velocity.X > -0.1))
                                npc.velocity.X += (death ? 0.45f : 0.3f) * npc.direction;
                            else
                                npc.velocity.X *= death ? 0.85f : 0.91f;
                        }

                        if (!Main.player[npc.target].dead)
                        {
                            if (npc.velocity.Y > 0f && npc.Bottom.Y > Main.player[npc.target].Top.Y)
                                npc.noTileCollide = false;
                            else if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].Center, 1, 1) && !Collision.SolidCollision(npc.position, npc.width, npc.height))
                                npc.noTileCollide = false;
                            else
                                npc.noTileCollide = true;
                        }
                    }

                    break;

                // Slam down and create shockwave
                // Create a cascade of crystals while falling down in phase 3 and the case is 4
                // Release a massive eruption of crystals in phase 3 and the case is 6
                case 4:
                case 6:

                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.rotation *= 0.9f;
                    npc.noTileCollide = true;
                    npc.noGravity = true;

                    if (npc.ai[2] == 1f)
                    {
                        npc.noTileCollide = false;
                        npc.noGravity = false;

                        int slamDelay = 30;
                        if (phase2)
                            slamDelay = 10;

                        if (Main.getGoodWorld)
                            slamDelay = 0;

                        if (npc.velocity.Y == 0f)
                        {
                            SoundEngine.PlaySound(SoundID.Item167, npc.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int type = ProjectileID.QueenSlimeSmash;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Bottom, Vector2.Zero, type, SlamDamage, 0f, Main.myPlayer);

                                // Line of explosions in Master Mode
                                if (death)
                                {
                                    float expandDelay = 0f;
                                    int maxSmashes = death ? 22 : 16;
                                    float maxSmashOffset = maxSmashes * 100f;
                                    Vector2 extraSmashPosition = npc.Bottom + Vector2.UnitX * maxSmashOffset;
                                    int maxSmashesPerSide = maxSmashes / 2;
                                    float maxExpandDelay = (death ? 10f : 15f) * maxSmashesPerSide;
                                    float smashSpawnDistanceOffset = 200f;
                                    for (int i = 0; i < maxSmashes + 1; i++)
                                    {
                                        expandDelay = MathHelper.Lerp(0f, maxExpandDelay, Math.Abs(i - maxSmashesPerSide) / (float)maxSmashesPerSide);
                                        if (i != maxSmashesPerSide)
                                            Projectile.NewProjectile(npc.GetSource_FromAI(), extraSmashPosition, Vector2.Zero, type, SlamDamage, 0f, Main.myPlayer, -expandDelay);

                                        extraSmashPosition -= Vector2.UnitX * smashSpawnDistanceOffset;
                                    }
                                }

                                // Eruption of crystals in phase 3
                                if (npc.ai[0] == 6f && phase3)
                                {
                                    float projectileVelocity = death ? 18f : 12f;
                                    type = ProjectileID.QueenSlimeMinionBlueSpike;
                                    Vector2 destination = (new Vector2(npc.Center.X, npc.Center.Y - 100f) - npc.Center).SafeNormalize(Vector2.UnitY);
                                    destination *= projectileVelocity;
                                    int numProj = 20;
                                    float rotation = MathHelper.ToRadians(100);
                                    for (int i = 0; i < numProj; i++)
                                    {
                                        Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, perturbedSpeed, type, SpikeDamage, 0f, Main.myPlayer, 0f, -2f);
                                    }

                                    if (phase5)
                                    {
                                        numProj = 15;
                                        destination *= 0.75f;
                                        for (int i = 0; i < numProj; i++)
                                        {
                                            Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, perturbedSpeed, type, SpikeDamage, 0f, Main.myPlayer, 0f, -2f);
                                        }
                                    }
                                }
                            }

                            for (int l = 0; l < 20; l++)
                            {
                                int slamDust = Dust.NewDust(npc.Bottom - new Vector2(npc.width / 2, 30f), npc.width, 30, DustID.Smoke, npc.velocity.X, npc.velocity.Y, 40, NPC.AI_121_QueenSlime_GetDustColor());
                                Main.dust[slamDust].noGravity = true;
                                Main.dust[slamDust].velocity.Y = -5f + Main.rand.NextFloat() * -3f;
                                Main.dust[slamDust].velocity.X *= 7f;
                            }

                            npc.Calamity().newAI[0] = npc.ai[0];
                            npc.SyncExtraAI();
                            npc.ai[0] = 0f;
                            npc.ai[1] = 0f;
                            npc.ai[2] = 0f;
                            npc.netUpdate = true;
                        }
                        else if (npc.ai[1] >= slamDelay)
                        {
                            for (int m = 0; m < 4; m++)
                            {
                                Vector2 position = npc.Bottom - new Vector2(Main.rand.NextFloatDirection() * 16f, Main.rand.Next(8));
                                int slamDust2 = Dust.NewDust(position, 2, 2, DustID.Smoke, npc.velocity.X, npc.velocity.Y, 40, NPC.AI_121_QueenSlime_GetDustColor(), 1.4f);
                                Main.dust[slamDust2].position = position;
                                Main.dust[slamDust2].noGravity = true;
                                Main.dust[slamDust2].velocity.Y = npc.velocity.Y * 0.9f;
                                Main.dust[slamDust2].velocity.X = (Main.rand.NextBool() ? (-10f) : 10f) + Main.rand.NextFloatDirection() * 3f;
                            }
                        }

                        npc.velocity.X *= 0.8f;
                        float slamNetUpdate = npc.ai[1];
                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= slamDelay)
                        {
                            if (slamNetUpdate < slamDelay)
                                npc.netUpdate = true;

                            if (phase2 && npc.ai[1] > (slamDelay + 120))
                            {
                                npc.Calamity().newAI[0] = npc.ai[0];
                                npc.SyncExtraAI();
                                npc.ai[0] = 0f;
                                npc.ai[1] = 0f;
                                npc.ai[2] = 0f;
                                npc.velocity.Y *= 0.8f;
                                npc.netUpdate = true;
                                break;
                            }

                            // Set damage
                            npc.damage = npc.defDamage;

                            npc.velocity.Y += death ? 1.75f : 1.5f;
                            float slamVelocity = death ? 15.5f : 15f;
                            if (Main.getGoodWorld)
                            {
                                npc.velocity.Y += 1f;
                                slamVelocity = 15.99f;
                            }

                            if (npc.velocity.Y == 0f)
                                npc.velocity.Y = 0.01f;

                            if (npc.velocity.Y >= slamVelocity)
                                npc.velocity.Y = slamVelocity;

                            // Cascade of crystals in phase 3 or 4 while falling down
                            if (((npc.ai[0] == 4f && phase3) || phase4) && npc.ai[1] % 12f == 0f)
                            {
                                SoundEngine.PlaySound(SoundID.Item154, npc.Center);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 fireFrom = npc.Center;
                                    int projectileAmt = 2;
                                    int type = ProjectileID.QueenSlimeMinionBlueSpike;
                                    Vector2 velocityIncrease = death ? (Vector2.UnitY * 4f) : Vector2.Zero;
                                    for (int i = 0; i < projectileAmt; i++)
                                    {
                                        int totalProjectiles = 2;
                                        float radians = MathHelper.TwoPi / totalProjectiles;
                                        for (int j = 0; j < totalProjectiles; j++)
                                        {
                                            Vector2 projVelocity = (npc.velocity + velocityIncrease).RotatedBy(radians * j + MathHelper.PiOver2);
                                            Projectile.NewProjectile(npc.GetSource_FromAI(), fireFrom, projVelocity, type, SpikeDamage, 0f, Main.myPlayer, 0f, -1f);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            npc.velocity.Y *= 0.8f;

                        break;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] == 0f)
                    {
                        CalamityUtils.CalamityTargeting(npc, default);

                        npc.netUpdate = true;
                    }

                    npc.ai[1] += 1f;
                    if (!(npc.ai[1] >= 30f))
                        break;

                    if (npc.ai[1] >= 60f)
                    {
                        npc.ai[1] = 60f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[2] = 1f;
                            npc.velocity.Y = -3f;
                            npc.netUpdate = true;
                        }
                    }

                    Player player3 = Main.player[npc.target];
                    Vector2 center = npc.Center;
                    if (!player3.dead && player3.active && Math.Abs(npc.Center.X - player3.Center.X) / 16f <= despawnDistanceInTiles)
                        center = player3.Center;

                    center.Y -= 384f;
                    if (npc.velocity.Y == 0f)
                    {
                        npc.velocity = center - npc.Center;
                        npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero);
                        npc.velocity *= death ? 27.3f : 24f;
                    }
                    else
                        npc.velocity.Y *= 0.95f;

                    break;

                // Fire spread of gel projectiles
                case 5:

                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.rotation *= 0.9f;
                    npc.noTileCollide = true;
                    npc.noGravity = true;

                    if (phase2)
                        npc.ai[3] = 0f;

                    if (npc.ai[2] == 1f)
                    {
                        npc.ai[1] += 1f;
                        if (!(npc.ai[1] >= 10f))
                            break;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int numGelProjectiles = phase4 ? Main.rand.Next(9, 12) : phase2 ? Main.rand.Next(6, 9) : 12;
                            if (phase5)
                                numGelProjectiles += 6;
                            if (Main.getGoodWorld)
                                numGelProjectiles = 15;

                            float projectileVelocity = death ? 12f : 10.5f;
                            int type = ProjectileID.QueenSlimeGelAttack;
                            if (phase2)
                            {
                                Vector2 destination = (new Vector2(npc.Center.X, npc.Center.Y + 100f) - npc.Center).SafeNormalize(Vector2.UnitY);
                                destination *= projectileVelocity;
                                float rotation = MathHelper.ToRadians(120);
                                for (int i = 0; i < numGelProjectiles; i++)
                                {
                                    if (Main.getGoodWorld)
                                        destination *= Main.rand.NextFloat() + 0.5f;

                                    Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numGelProjectiles - 1)));
                                    int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, perturbedSpeed, type, LargeGelDamage, 0f, Main.myPlayer, 0f, -2f);
                                    Main.projectile[proj].timeLeft = 900;
                                }
                            }
                            else
                            {
                                for (int j = 0; j < numGelProjectiles; j++)
                                {
                                    Vector2 spinningpoint = new Vector2(projectileVelocity, 0f);

                                    if (Main.getGoodWorld)
                                        spinningpoint *= Main.rand.NextFloat() + 0.5f;

                                    spinningpoint = spinningpoint.RotatedBy((-j) * MathHelper.TwoPi / numGelProjectiles, Vector2.Zero);
                                    int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, spinningpoint, type, LargeGelDamage, 0f, Main.myPlayer, 0f, -2f);
                                    Main.projectile[proj].timeLeft = 900;
                                }
                            }

                            // Fire gel balls directly at players with a max of 3
                            List<int> targets = new List<int>();
                            for (int p = 0; p < Main.maxPlayers; p++)
                            {
                                if (Main.player[p].active && !Main.player[p].dead)
                                    targets.Add(p);

                                if (targets.Count > 2)
                                    break;
                            }
                            foreach (int t in targets)
                            {
                                Vector2 velocity2 = (Main.player[t].Center - npc.Center).SafeNormalize(Vector2.UnitY) * projectileVelocity;
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity2, type, LargeGelDamage, 0f, Main.myPlayer, 0f, -2f);
                                Main.projectile[proj].timeLeft = 900;
                            }
                        }

                        SoundEngine.PlaySound(SoundID.Item155, npc.Center);
                        npc.Calamity().newAI[0] = npc.ai[0];
                        npc.SyncExtraAI();
                        npc.ai[0] = 0f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.netUpdate = true;
                        break;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] == 0f)
                    {
                        CalamityUtils.CalamityTargeting(npc, default);

                        npc.netUpdate = true;
                    }

                    npc.ai[1] += 1f;
                    if (npc.ai[1] >= 50f)
                    {
                        npc.ai[1] = 50f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[2] = 1f;
                            npc.netUpdate = true;
                        }
                    }

                    float slamDustRadius = 100f;
                    for (int k = 0; k < 4; k++)
                    {
                        Vector2 slamDustArea = npc.Center + Main.rand.NextVector2CircularEdge(slamDustRadius, slamDustRadius);
                        if (!phase2)
                            slamDustArea += new Vector2(0f, 20f);

                        Vector2 v = slamDustArea - npc.Center;
                        v = v.SafeNormalize(Vector2.Zero) * -8f;
                        int superSlamDust = Dust.NewDust(slamDustArea, 2, 2, DustID.Smoke, v.X, v.Y, 40, NPC.AI_121_QueenSlime_GetDustColor(), 1.8f);
                        Main.dust[superSlamDust].position = slamDustArea;
                        Main.dust[superSlamDust].noGravity = true;
                        Main.dust[superSlamDust].alpha = 250;
                        Main.dust[superSlamDust].velocity = v;
                        Main.dust[superSlamDust].customData = npc;
                    }

                    if (phase2)
                        QueenSlime_FlyMovement(npc);

                    break;
            }

            // Don't take damage while teleporting
            npc.dontTakeDamage = npc.hide = teleported;

            // Adjust size with HP
            if (slimeScale != npc.scale)
            {
                npc.position.X += npc.width / 2;
                npc.position.Y += npc.height;
                npc.scale = slimeScale;
                npc.width = (int)(114f * npc.scale);
                npc.height = (int)(100f * npc.scale);
                npc.position.X -= npc.width / 2;
                npc.position.Y -= npc.height;
            }

            // Spawn small slimes
            // Don't spawn any slimes in final phase
            if (npc.life <= 0 || (phase4 && !death))
                return false;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return false;

            // Reset numerous variables when phase 2 begins
            if (npc.localAI[0] >= (npc.lifeMax / 2) && npc.life < npc.lifeMax / 2)
            {
                npc.localAI[0] = npc.life;
                npc.ai[0] = 0f;
                npc.ai[1] = 0f;
                npc.ai[2] = 0f;
                npc.netUpdate = true;
            }

            float slimeSpawnHealthGateValue = Main.zenithWorld ? 0.01f : phase3 ? 0.04f : phase2 ? 0.03f : 0.025f;
            if (death)
                slimeSpawnHealthGateValue *= 0.5f;

            int slimeSpawnThreshold = (int)(npc.lifeMax * slimeSpawnHealthGateValue);

            if (!((npc.life + slimeSpawnThreshold) < npc.localAI[0]))
                return false;

            npc.localAI[0] = npc.life;

            int offset = 16;
            int x = (int)(npc.position.X + offset + Main.rand.Next(npc.width - offset * 2));
            int y = (int)(npc.position.Y + offset + Main.rand.Next(npc.height - offset * 2));

            int random = Main.rand.Next(2);
            if (phase2)
                random += 1;
            if (phase3)
                random = 2;

            int typeToSpawn = NPCID.QueenSlimeMinionBlue;
            switch (random)
            {
                case 0:
                    typeToSpawn = NPCID.QueenSlimeMinionBlue;
                    break;
                case 1:
                    typeToSpawn = NPCID.QueenSlimeMinionPink;
                    break;
                case 2:
                    typeToSpawn = NPCID.QueenSlimeMinionPurple;
                    break;
            }

            int slimeScale2 = NPC.NewNPC(npc.GetSource_FromAI(), x, y, typeToSpawn);
            Main.npc[slimeScale2].SetDefaults(typeToSpawn);
            Main.npc[slimeScale2].velocity.X = Main.rand.Next(-15, 16) * 0.1f;
            Main.npc[slimeScale2].velocity.Y = Main.rand.Next(-30, 1) * 0.1f;
            Main.npc[slimeScale2].ai[0] = -500 * Main.rand.Next(3);
            Main.npc[slimeScale2].ai[1] = 0f;
            if (Main.dedServ && slimeScale2 < Main.maxNPCs)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slimeScale2);

            return false;
        }

        public static void QueenSlime_FlyMovement(NPC npc)
        {
            // Difficulty bools
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            npc.noTileCollide = true;
            npc.noGravity = true;

            float flyVelocity = death ? 21f : 16f;
            float flyAcceleration = death ? 0.18f : 0.12f;
            float flyDistanceY = death ? 350f : 450f;

            Vector2 desiredVelocity = npc.Center;

            if (npc.timeLeft > 10)
            {
                if (!Collision.CanHit(npc, Main.player[npc.target]))
                {
                    bool flyToSolidTilesAboveTarget = false;
                    Vector2 center = Main.player[npc.target].Center;
                    for (int i = 0; i < 16; i++)
                    {
                        float tileDistanceAboveTarget = 16 * i;
                        Point point = (center + new Vector2(0f, 0f - tileDistanceAboveTarget)).ToTileCoordinates();
                        if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
                        {
                            desiredVelocity = center + new Vector2(0f, 0f - tileDistanceAboveTarget + 16f) - npc.Center;
                            flyToSolidTilesAboveTarget = true;
                            break;
                        }
                    }

                    if (!flyToSolidTilesAboveTarget)
                        desiredVelocity = center - npc.Center;
                }
                else
                    desiredVelocity = Main.player[npc.target].Center + new Vector2(0f, -flyDistanceY) - npc.Center;
            }
            else
                desiredVelocity = npc.Center + new Vector2(500f * npc.direction, -flyDistanceY) - npc.Center;

            float distanceFromFlightTarget = desiredVelocity.Length();
            if (Math.Abs(desiredVelocity.X) < 40f)
                desiredVelocity.X = npc.velocity.X;

            if (distanceFromFlightTarget > 100f && ((npc.velocity.X < -12f && desiredVelocity.X > 0f) || (npc.velocity.X > 12f && desiredVelocity.X < 0f)))
                flyAcceleration = 0.2f;

            if (distanceFromFlightTarget < 40f)
            {
                desiredVelocity = npc.velocity;
            }
            else if (distanceFromFlightTarget < 80f)
            {
                desiredVelocity = desiredVelocity.SafeNormalize(Vector2.UnitY);
                desiredVelocity *= flyVelocity * 0.65f;
            }
            else
            {
                desiredVelocity = desiredVelocity.SafeNormalize(Vector2.UnitY);
                desiredVelocity *= flyVelocity;
            }

            npc.SimpleFlyMovement(desiredVelocity, flyAcceleration);
            npc.rotation = npc.velocity.X * 0.1f;
            if (npc.rotation > 0.5f)
                npc.rotation = 0.5f;

            if (npc.rotation < -0.5f)
                npc.rotation = -0.5f;
        }

        public static bool BuffedQueenSlimeCrystalSlimeAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            if (npc.localAI[0] > 0f)
                npc.localAI[0] -= 1f;

            if (!npc.wet && Main.player[npc.target].active && !Main.player[npc.target].dead && !Main.player[npc.target].npcTypeNoAggro[npc.type])
            {
                Player obj = Main.player[npc.target];
                Vector2 center = npc.Center;
                float num19 = obj.Center.X - center.X;
                float num20 = obj.Center.Y - center.Y;
                float num21 = (float)Math.Sqrt(num19 * num19 + num20 * num20);
                int num22 = NPC.CountNPCS(NPCID.QueenSlimeMinionBlue);
                if (Main.expertMode && num22 < 5 && Math.Abs(num19) < 500f && Math.Abs(num20) < 550f && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && npc.velocity.Y == 0f)
                {
                    npc.ai[0] = -40f;
                    if (npc.velocity.Y == 0f)
                        npc.velocity.X *= 0.9f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            Vector2 vector6 = new Vector2(k - 1, -4f);
                            vector6.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.005f;
                            vector6.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.005f;
                            vector6.Normalize();
                            vector6 *= 6f + (float)Main.rand.Next(-50, 51) * 0.01f;
                            if (num21 > 350f)
                                vector6 *= 2f;
                            else if (num21 > 250f)
                                vector6 *= 1.5f;

                            int type = ProjectileID.QueenSlimeMinionBlueSpike;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), center, vector6 * (death ? 0.8f : 0.5f), type, SpikeDamage, 0f, Main.myPlayer);
                            npc.localAI[0] = 25f;
                            if (num22 > 4)
                                break;
                        }
                    }
                }
                else if (Math.Abs(num19) < 500f && Math.Abs(num20) < 550f && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && npc.velocity.Y == 0f)
                {
                    float num23 = num21;
                    npc.ai[0] = -40f;
                    if (npc.velocity.Y == 0f)
                        npc.velocity.X *= 0.9f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f)
                    {
                        num20 = Main.player[npc.target].position.Y - center.Y - (float)Main.rand.Next(0, 200);
                        num21 = (float)Math.Sqrt(num19 * num19 + num20 * num20);
                        num21 = 4.5f / num21;
                        num21 *= 2f;
                        if (num23 > 350f)
                            num21 *= 2f;
                        else if (num23 > 250f)
                            num21 *= 1.5f;

                        num19 *= num21;
                        num20 *= num21;
                        npc.localAI[0] = 50f;
                        int type = ProjectileID.QueenSlimeMinionBlueSpike;
                        Vector2 spikeVelocity = new Vector2(num19, num20) * (death ? 0.8f : 0.5f);
                        Projectile.NewProjectile(npc.GetSource_FromAI(), center, spikeVelocity, type, SpikeDamage, 0f, Main.myPlayer);
                    }
                }
            }

            if (npc.ai[2] > 1f)
                npc.ai[2] -= 1f;

            if (npc.wet)
            {
                if (npc.collideY)
                    npc.velocity.Y = -2f;

                if (npc.velocity.Y < 0f && npc.ai[3] == npc.position.X)
                {
                    npc.direction *= -1;
                    npc.ai[2] = 200f;
                }

                if (npc.velocity.Y > 0f)
                    npc.ai[3] = npc.position.X;

                if (npc.velocity.Y > 2f)
                    npc.velocity.Y *= 0.9f;

                npc.velocity.Y -= 0.5f;
                if (npc.velocity.Y < -4f)
                    npc.velocity.Y = -4f;

                if (npc.ai[2] == 1f)
                    CalamityUtils.CalamityTargeting(npc, default);
            }

            npc.aiAction = 0;
            if (npc.ai[2] == 0f)
            {
                npc.ai[0] = -100f;
                npc.ai[2] = 1f;
                CalamityUtils.CalamityTargeting(npc, default);
            }

            if (npc.velocity.Y == 0f)
            {
                if (npc.collideY && npc.oldVelocity.Y != 0f && Collision.SolidCollision(npc.position, npc.width, npc.height))
                    npc.position.X -= npc.velocity.X + (float)npc.direction;

                if (npc.ai[3] == npc.position.X)
                {
                    npc.direction *= -1;
                    npc.ai[2] = 200f;
                }

                npc.ai[3] = 0f;
                npc.velocity.X *= 0.8f;
                if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                    npc.velocity.X = 0f;

                npc.ai[0] += (death ? 16f : Main.expertMode ? 10f : 7f);

                float num33 = -1000f;

                int num34 = 0;
                if (npc.ai[0] >= 0f)
                    num34 = 1;

                if (npc.ai[0] >= num33 && npc.ai[0] <= num33 * 0.5f)
                    num34 = 2;

                if (npc.ai[0] >= num33 * 2f && npc.ai[0] <= num33 * 1.5f)
                    num34 = 3;

                if (num34 > 0)
                {
                    npc.netUpdate = true;
                    if (npc.ai[2] == 1f)
                        CalamityUtils.CalamityTargeting(npc, default);

                    if (num34 == 3)
                    {
                        npc.velocity.Y = -8f;
                        npc.velocity.X += (death ? 9 : Main.expertMode ? 6 : 3) * npc.direction;
                        npc.ai[0] = -200f;
                        npc.ai[3] = npc.position.X;
                    }
                    else
                    {
                        npc.velocity.Y = -6f;
                        npc.velocity.X += (death ? 6 : Main.expertMode ? 4 : 2) * npc.direction;
                        npc.ai[0] = -120f;
                        if (num34 == 1)
                            npc.ai[0] += num33;
                        else
                            npc.ai[0] += num33 * 2f;
                    }
                }
                else if (npc.ai[0] >= -30f)
                    npc.aiAction = 1;
            }
            else if (npc.target < Main.maxPlayers && ((npc.direction == 1 && npc.velocity.X < 3f) || (npc.direction == -1 && npc.velocity.X > -3f)))
            {
                if (npc.collideX && Math.Abs(npc.velocity.X) == 0.2f)
                    npc.position.X -= 1.4f * (float)npc.direction;

                if (npc.collideY && npc.oldVelocity.Y != 0f && Collision.SolidCollision(npc.position, npc.width, npc.height))
                    npc.position.X -= npc.velocity.X + (float)npc.direction;

                if ((npc.direction == -1 && (double)npc.velocity.X < 0.01) || (npc.direction == 1 && (double)npc.velocity.X > -0.01))
                    npc.velocity.X += 0.2f * (float)npc.direction;
                else
                    npc.velocity.X *= 0.93f;
            }

            return false;
        }

        public static bool BuffedQueenSlimeBouncySlimeAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            if (npc.localAI[0] > 0f)
                npc.localAI[0] -= 1f;

            if (!npc.wet && Main.player[npc.target].active && !Main.player[npc.target].dead && !Main.player[npc.target].npcTypeNoAggro[npc.type])
            {
                Player obj2 = Main.player[npc.target];
                Vector2 center2 = npc.Center;
                float num24 = obj2.Center.X - center2.X;
                float num25 = obj2.Center.Y - center2.Y;
                float num26 = (float)Math.Sqrt(num24 * num24 + num25 * num25);
                float num27 = num26;
                if (Math.Abs(num24) < 500f && Math.Abs(num25) < 550f && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && npc.velocity.Y == 0f)
                {
                    npc.ai[0] = -40f;
                    if (npc.velocity.Y == 0f)
                        npc.velocity.X *= 0.9f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f)
                    {
                        num25 = Main.player[npc.target].position.Y - center2.Y - (float)Main.rand.Next(0, 200);
                        num26 = (float)Math.Sqrt(num24 * num24 + num25 * num25);
                        num26 = 4.5f / num26;
                        num26 *= 2f;
                        if (num27 > 350f)
                            num26 *= 1.75f;
                        else if (num27 > 250f)
                            num26 *= 1.25f;

                        num24 *= num26;
                        num25 *= num26;
                        npc.localAI[0] = 40f;
                        if (Main.expertMode)
                            npc.localAI[0] = 30f;

                        int type = ProjectileID.QueenSlimeMinionPinkBall;
                        Vector2 pinkBallVelocity = new Vector2(num24, num25) * (death ? 0.8f : 0.5f);
                        Projectile.NewProjectile(npc.GetSource_FromAI(), center2, pinkBallVelocity, type, SmallGelDamage, 0f, Main.myPlayer);
                    }
                }
            }

            if (npc.ai[2] > 1f)
                npc.ai[2] -= 1f;

            if (npc.wet)
            {
                if (npc.collideY)
                    npc.velocity.Y = -2f;

                if (npc.velocity.Y < 0f && npc.ai[3] == npc.position.X)
                {
                    npc.direction *= -1;
                    npc.ai[2] = 200f;
                }

                if (npc.velocity.Y > 0f)
                    npc.ai[3] = npc.position.X;

                if (npc.velocity.Y > 2f)
                    npc.velocity.Y *= 0.9f;

                npc.velocity.Y -= 0.5f;
                if (npc.velocity.Y < -4f)
                    npc.velocity.Y = -4f;

                if (npc.ai[2] == 1f)
                    CalamityUtils.CalamityTargeting(npc, default);
            }

            npc.aiAction = 0;
            if (npc.ai[2] == 0f)
            {
                npc.ai[0] = -100f;
                npc.ai[2] = 1f;
                CalamityUtils.CalamityTargeting(npc, default);
            }

            if (npc.velocity.Y == 0f)
            {
                if (npc.collideY && npc.oldVelocity.Y != 0f && Collision.SolidCollision(npc.position, npc.width, npc.height))
                    npc.position.X -= npc.velocity.X + (float)npc.direction;

                if (npc.ai[3] == npc.position.X)
                {
                    npc.direction *= -1;
                    npc.ai[2] = 200f;
                }

                npc.ai[3] = 0f;
                npc.velocity.X *= 0.8f;
                if ((double)npc.velocity.X > -0.1 && (double)npc.velocity.X < 0.1)
                    npc.velocity.X = 0f;

                npc.ai[0] += (death ? 11f : Main.expertMode ? 7f : 5f);

                float num33 = -500f;

                int num34 = 0;
                if (npc.ai[0] >= 0f)
                    num34 = 1;

                if (npc.ai[0] >= num33 && npc.ai[0] <= num33 * 0.5f)
                    num34 = 2;

                if (npc.ai[0] >= num33 * 2f && npc.ai[0] <= num33 * 1.5f)
                    num34 = 3;

                if (num34 > 0)
                {
                    npc.netUpdate = true;
                    if (npc.ai[2] == 1f)
                        CalamityUtils.CalamityTargeting(npc, default);

                    if (num34 == 3)
                    {
                        npc.velocity.Y = -8f;
                        npc.velocity.X += (death ? 9 : Main.expertMode ? 6 : 3) * npc.direction;
                        npc.ai[0] = -200f;
                        npc.ai[3] = npc.position.X;
                    }
                    else
                    {
                        npc.velocity.Y = -6f;
                        npc.velocity.X += (death ? 6 : Main.expertMode ? 4 : 2) * npc.direction;
                        npc.ai[0] = -120f;
                        if (num34 == 1)
                            npc.ai[0] += num33;
                        else
                            npc.ai[0] += num33 * 2f;
                    }

                    npc.velocity.Y *= 1.6f;
                    npc.velocity.X *= 1.2f;
                }
                else if (npc.ai[0] >= -30f)
                    npc.aiAction = 1;
            }
            else if (npc.target < Main.maxPlayers && ((npc.direction == 1 && npc.velocity.X < 3f) || (npc.direction == -1 && npc.velocity.X > -3f)))
            {
                if (npc.collideX && Math.Abs(npc.velocity.X) == 0.2f)
                    npc.position.X -= 1.4f * (float)npc.direction;

                if (npc.collideY && npc.oldVelocity.Y != 0f && Collision.SolidCollision(npc.position, npc.width, npc.height))
                    npc.position.X -= npc.velocity.X + (float)npc.direction;

                if ((npc.direction == -1 && (double)npc.velocity.X < 0.01) || (npc.direction == 1 && (double)npc.velocity.X > -0.01))
                    npc.velocity.X += 0.2f * (float)npc.direction;
                else
                    npc.velocity.X *= 0.93f;
            }

            return false;
        }
    }
}
