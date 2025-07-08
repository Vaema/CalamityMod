using System;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class GolemAI
    {
        public static bool BuffedGolemAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // whoAmI variable
            NPC.golemBoss = npc.whoAmI;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases
            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;
            bool phase2 = lifeRatio < 0.75f;
            bool phase3 = lifeRatio < 0.5f;
            bool phase4 = lifeRatio < 0.25f;

            // Spawn parts
            if (npc.localAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.localAI[0] = 1f;
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X - 84, (int)npc.Center.Y - 9, NPCID.GolemFistLeft, 0);
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X + 78, (int)npc.Center.Y - 9, NPCID.GolemFistRight, 0);
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X - 3, (int)npc.Center.Y - 57, NPCID.GolemHead);
            }

            // Despawn
            if (npc.target >= 0 && Main.player[npc.target].dead)
            {
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                if (Main.player[npc.target].dead)
                    npc.noTileCollide = true;
            }

            // Enrage if the target isn't inside the temple
            // Turbo enrage if target isn't inside the temple and it's Boss Rush or For the Worthy
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int targetTilePosX = (int)Main.player[npc.target].Center.X / 16;
                int targetTilePosY = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || CalamityWorld.LegendaryMode;
            }
            else
                turboEnrage = bossRush || CalamityWorld.LegendaryMode;

            if (bossRush || CalamityWorld.LegendaryMode)
                enrage = true;

            npc.Calamity().CurrentlyEnraged = !bossRush && (enrage || turboEnrage);

            bool reduceFallSpeed = npc.velocity.Y > 0f && Collision.SolidCollision(npc.position + Vector2.UnitY * 1.1f * npc.velocity.Y, npc.width, npc.height);

            // Alpha
            if (npc.alpha > 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                npc.ai[1] = 0f;
            }

            // Check for body parts
            bool headAlive = NPC.AnyNPCs(NPCID.GolemHead);
            bool leftFistAlive = NPC.AnyNPCs(NPCID.GolemFistLeft);
            bool rightFistAlive = NPC.AnyNPCs(NPCID.GolemFistRight);
            npc.dontTakeDamage = (headAlive || leftFistAlive || rightFistAlive) && !CalamityWorld.LegendaryMode;

            // Phase 2, check for free head
            bool freeHeadAlive = NPC.AnyNPCs(NPCID.GolemHeadFree);

            // Distance required for despawning
            int despawnDistance = turboEnrage ? 7500 : enrage ? 6000 : 4500;

            // Deactivate torches
            if (Main.netMode != NetmodeID.MultiplayerClient && CalamityWorld.LegendaryMode && npc.velocity.Y > 0f)
            {
                for (int j = (int)(npc.position.X / 16f); (float)j < (npc.position.X + (float)npc.width) / 16f; j++)
                {
                    for (int k = (int)(npc.position.Y / 16f); (float)k < (npc.position.Y + (float)npc.width) / 16f; k++)
                    {
                        if (Main.tile[j, k].TileType == TileID.Torches)
                        {
                            Main.tile[j, k].Get<TileWallWireStateData>().HasTile = false;
                            if (Main.dedServ)
                                NetMessage.SendTileSquare(-1, j, k);
                        }
                    }
                }
            }

            // Spawn arm dust
            if (!CalamityWorld.LegendaryMode)
            {
                if (!leftFistAlive)
                {
                    int lostLeftFistDust = Dust.NewDust(new Vector2(npc.Center.X - 80f * npc.scale, npc.Center.Y - 9f), 8, 8, DustID.Smoke, 0f, 0f, 100, default, 1f);
                    Dust dust = Main.dust[lostLeftFistDust];
                    dust.alpha += Main.rand.Next(100);
                    dust.velocity *= 0.2f;
                    dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                    dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;

                    if (Main.rand.NextBool(10))
                    {
                        lostLeftFistDust = Dust.NewDust(new Vector2(npc.Center.X - 80f * npc.scale, npc.Center.Y - 9f), 8, 8, DustID.Torch, 0f, 0f, 0, default, 1f);
                        if (!Main.rand.NextBool(20))
                        {
                            Main.dust[lostLeftFistDust].noGravity = true;
                            dust = Main.dust[lostLeftFistDust];
                            dust.scale *= 1f + Main.rand.Next(10) * 0.1f;
                            dust.velocity.Y -= 1f;
                        }
                    }
                }
                if (!rightFistAlive)
                {
                    int lostRightFistDust = Dust.NewDust(new Vector2(npc.Center.X + 62f * npc.scale, npc.Center.Y - 9f), 8, 8, DustID.Smoke, 0f, 0f, 100, default, 1f);
                    Dust dust = Main.dust[lostRightFistDust];
                    dust.alpha += Main.rand.Next(100);
                    dust.velocity *= 0.2f;
                    dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                    dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;

                    if (Main.rand.NextBool(10))
                    {
                        lostRightFistDust = Dust.NewDust(new Vector2(npc.Center.X + 62f * npc.scale, npc.Center.Y - 9f), 8, 8, DustID.Torch, 0f, 0f, 0, default, 1f);
                        if (!Main.rand.NextBool(20))
                        {
                            Main.dust[lostRightFistDust].noGravity = true;
                            dust = Main.dust[lostRightFistDust];
                            dust.scale *= 1f + Main.rand.Next(10) * 0.1f;
                            dust.velocity.Y -= 1f;
                        }
                    }
                }
            }

            if (npc.noTileCollide && !Main.player[npc.target].dead)
            {
                if (npc.velocity.Y > 0f && npc.Bottom.Y > Main.player[npc.target].Top.Y)
                    npc.noTileCollide = false;
                else if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].Center, 1, 1) && !Collision.SolidCollision(npc.position, npc.width, npc.height))
                    npc.noTileCollide = false;
            }

            // Jump
            if (npc.ai[0] == 0f)
            {
                if (npc.velocity.Y == 0f || npc.ai[2] > 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    // Laser fire when head is dead
                    if (Main.netMode != NetmodeID.MultiplayerClient && (!headAlive || turboEnrage || CalamityWorld.LegendaryMode) && npc.ai[2] == 0f)
                    {
                        npc.localAI[1] += 1f;

                        float divisor = 15f -
                            (phase2 ? 4f : 0f) -
                            (phase3 ? 3f : 0f) -
                            (phase4 ? 2f : 0f);

                        if (enrage)
                            divisor = 5f;

                        if (turboEnrage && CalamityWorld.LegendaryMode)
                            divisor = 2f;

                        Vector2 projectileFirePos = new Vector2(npc.Center.X, npc.Center.Y - 60f);
                        if (npc.localAI[1] % divisor == 0f && (Vector2.Distance(Main.player[npc.target].Center, projectileFirePos) > 160f || !freeHeadAlive))
                        {
                            float laserSpeed = turboEnrage ? 16f : enrage ? 12f : 6f;
                            float laserTargetXDist = Main.player[npc.target].Center.X - projectileFirePos.X;
                            float laserTargetYDist = Main.player[npc.target].Center.Y - projectileFirePos.Y;
                            float laserTargetDistance = (float)Math.Sqrt(laserTargetXDist * laserTargetXDist + laserTargetYDist * laserTargetYDist);

                            laserTargetDistance = laserSpeed / laserTargetDistance;
                            laserTargetXDist *= laserTargetDistance;
                            laserTargetYDist *= laserTargetDistance;

                            Vector2 laserVelocity = new Vector2(laserTargetXDist, laserTargetYDist);
                            int type = ProjectileID.EyeBeam;
                            int damage = npc.GetProjectileDamage(type);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int bodyLaser = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileFirePos + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, type, damage, 0f, Main.myPlayer);
                                Main.projectile[bodyLaser].timeLeft = enrage ? 720 : 360;
                                if (turboEnrage && CalamityWorld.LegendaryMode)
                                    Main.projectile[bodyLaser].extraUpdates += 1;
                            }
                        }

                        if (npc.localAI[1] >= 15f)
                            npc.localAI[1] = 0f;
                    }

                    // Delay before jumping
                    if (npc.ai[2] == 0f)
                    {
                        npc.velocity.X *= 0.8f;
                        npc.ai[1] += 1f;
                    }

                    if (npc.ai[1] > 0f)
                    {
                        npc.ai[1] += (death ? 1.5f : 1f);
                        if (CalamityWorld.LegendaryMode)
                            npc.ai[1] += 100f;

                        if (enrage || death)
                        {
                            npc.ai[1] += 18f;
                        }
                        else
                        {
                            if (!leftFistAlive)
                                npc.ai[1] += 6f;
                            if (!rightFistAlive)
                                npc.ai[1] += 6f;
                        }
                    }
                    if (npc.ai[1] >= 300f)
                    {
                        npc.ai[1] = -20f;
                        npc.frameCounter = 0D;
                    }
                    else if (npc.ai[1] == -1f)
                    {
                        // Set jump velocity
                        if (!headAlive)
                            CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

                        // Set damage
                        npc.damage = npc.defDamage;

                        if (death)
                        {
                            float straightUpJumpHeight = 640f;
                            if (npc.ai[3] == 0f)
                                npc.ai[3] = (!headAlive && npc.Bottom.Y - straightUpJumpHeight > Main.player[npc.target].Top.Y) ? Main.rand.Next(3) + 1f : (!leftFistAlive && !rightFistAlive) ? Main.rand.Next(2) + 1f : 1f;

                            switch ((int)npc.ai[3])
                            {
                                default:
                                case 0:
                                case 1:
                                    NormalJump();
                                    break;

                                // Jump directly above the target's head and slam down
                                case 2:

                                    npc.noTileCollide = true;

                                    npc.ai[2] += 1f;
                                    float jumpVelocity = 26f;
                                    if (enrage)
                                        jumpVelocity *= 1.25f;
                                    if (turboEnrage)
                                        jumpVelocity *= 1.25f;

                                    float minJumpTime = 15f;
                                    float maxJumpTime = 45f;
                                    if ((npc.ai[2] >= minJumpTime && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) <= jumpVelocity) || npc.ai[2] >= maxJumpTime)
                                    {
                                        npc.ai[0] = 1f;
                                        npc.ai[1] = 0f;
                                        npc.ai[2] = 1f;
                                        npc.velocity.Y = -3f;
                                        npc.netUpdate = true;
                                    }

                                    Vector2 center = npc.Center;
                                    if (!Main.player[npc.target].dead && Main.player[npc.target].active && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) <= despawnDistance)
                                        center = Main.player[npc.target].Center;

                                    center.Y -= 480f;
                                    if (npc.velocity.Y == 0f)
                                    {
                                        npc.velocity = center - npc.Center;
                                        npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero);
                                        npc.velocity *= jumpVelocity;

                                        float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                                        float speedMult = 1f;

                                        float multiplier = turboEnrage ? 0.0025f : enrage ? 0.002f : 0.0015f;
                                        if (distanceBelowTarget > 0f && ((!leftFistAlive && !rightFistAlive) || turboEnrage || CalamityWorld.LegendaryMode))
                                            speedMult += distanceBelowTarget * multiplier;

                                        float speedMultLimit = turboEnrage ? 3.25f : enrage ? 3f : 2.5f;
                                        if (speedMult > speedMultLimit)
                                            speedMult = speedMultLimit;

                                        if (Main.player[npc.target].position.Y < npc.Bottom.Y)
                                            npc.velocity.Y *= speedMult;
                                    }
                                    else
                                        npc.velocity.Y *= 0.95f;

                                    break;

                                // Jump straight up and create a wall of lasers on both sides
                                case 3:

                                    npc.velocity.Y = (((!freeHeadAlive && !headAlive) || turboEnrage || CalamityWorld.LegendaryMode) ? -15.1f : -12.1f) + (enrage ? -4f : 0f);

                                    npc.noTileCollide = true;

                                    npc.ai[0] = 1f;
                                    npc.ai[1] = 0f;
                                    npc.ai[2] = 2f;

                                    float jumpDuration = (float)Math.Floor(straightUpJumpHeight / Math.Abs(npc.velocity.Y));
                                    npc.ai[3] = jumpDuration;

                                    npc.netUpdate = true;

                                    break;
                            }
                        }
                        else
                        {
                            if (npc.ai[3] == 0f)
                                npc.ai[3] = !headAlive ? 2f : 1f;

                            switch ((int)npc.ai[3])
                            {
                                default:
                                case 0:
                                case 1:
                                    NormalJump();
                                    break;

                                // Jump directly above the target's head and slam down
                                case 2:

                                    npc.noTileCollide = true;

                                    npc.ai[2] += 1f;
                                    float jumpVelocity = 21f;
                                    if (enrage)
                                        jumpVelocity *= 1.25f;
                                    if (turboEnrage)
                                        jumpVelocity *= 1.25f;

                                    float minJumpTime = 15f;
                                    float maxJumpTime = 45f;
                                    if ((npc.ai[2] >= minJumpTime && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) <= jumpVelocity) || npc.ai[2] >= maxJumpTime)
                                    {
                                        npc.ai[0] = 1f;
                                        npc.ai[1] = 0f;
                                        npc.ai[2] = 1f;
                                        npc.velocity.Y = -3f;
                                        npc.netUpdate = true;
                                    }

                                    Vector2 center = npc.Center;
                                    if (!Main.player[npc.target].dead && Main.player[npc.target].active && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) <= despawnDistance)
                                        center = Main.player[npc.target].Center;

                                    center.Y -= 480f;
                                    if (npc.velocity.Y == 0f)
                                    {
                                        npc.velocity = center - npc.Center;
                                        npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero);
                                        npc.velocity *= jumpVelocity;

                                        float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                                        float speedMult = 1f;

                                        float multiplier = turboEnrage ? 0.0025f : enrage ? 0.002f : 0.0015f;
                                        if (distanceBelowTarget > 0f && ((!leftFistAlive && !rightFistAlive) || turboEnrage || CalamityWorld.LegendaryMode))
                                            speedMult += distanceBelowTarget * multiplier;

                                        float speedMultLimit = turboEnrage ? 3.25f : enrage ? 3f : 2.5f;
                                        if (speedMult > speedMultLimit)
                                            speedMult = speedMultLimit;

                                        if (Main.player[npc.target].position.Y < npc.Bottom.Y)
                                            npc.velocity.Y *= speedMult;
                                    }
                                    else
                                        npc.velocity.Y *= 0.95f;

                                    break;
                            }
                        }

                        void NormalJump()
                        {
                            float velocityBoost = death ? 5f * (1f - (lifeRatio / 2)) : 3.8f * (1f - (lifeRatio / 2));
                            float velocityX = (death ? 8.75f : 6.25f) + velocityBoost;
                            if (enrage)
                                velocityX *= 1.5f;

                            float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                            npc.direction = playerLocation < 0 ? 1 : -1;
                            calamityGlobalNPC.newAI[1] = npc.direction;

                            npc.velocity.X = velocityX * npc.direction;

                            float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                            float speedMult = 1f;

                            float multiplier = turboEnrage ? 0.0025f : enrage ? 0.002f : 0.0015f;
                            if (distanceBelowTarget > 0f && ((!leftFistAlive && !rightFistAlive) || turboEnrage || CalamityWorld.LegendaryMode))
                                speedMult += distanceBelowTarget * multiplier;

                            float speedMultLimit = turboEnrage ? 3.25f : enrage ? 3f : 2.5f;
                            if (speedMult > speedMultLimit)
                                speedMult = speedMultLimit;

                            if (Main.player[npc.target].position.Y < npc.Bottom.Y)
                                npc.velocity.Y = ((((!freeHeadAlive && !headAlive) || turboEnrage || CalamityWorld.LegendaryMode) ? -15.1f : -12.1f) + (enrage ? -4f : 0f)) * speedMult;
                            else
                                npc.velocity.Y = 1f;

                            npc.noTileCollide = true;

                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;

                            npc.netUpdate = true;
                            npc.SyncExtraAI();
                        }
                    }
                }

                // Don't run custom gravity when starting a jump
                if (npc.ai[0] != 1f && npc.ai[2] == 0f)
                    CustomGravity(false);
            }

            // Fall down
            else if (npc.ai[0] == 1f)
            {
                if (npc.velocity.Y == 0f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    // Play sound
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);

                    npc.ai[0] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;
                    npc.SyncExtraAI();

                    // Dust and gore
                    for (int i = (int)npc.position.X - 20; i < (int)npc.position.X + npc.width + 40; i += 20)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int fallDust = Dust.NewDust(new Vector2(npc.position.X - 20f, npc.position.Y + npc.height), npc.width + 20, 4, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                            Dust dust = Main.dust[fallDust];
                            dust.velocity *= 0.2f;
                        }
                        if (!Main.dedServ)
                        {
                            int fallGore = Gore.NewGore(npc.GetSource_FromAI(), new Vector2(i - 20, npc.position.Y + npc.height - 8f), default, Main.rand.Next(61, 64), 1f);
                            Gore gore = Main.gore[fallGore];
                            gore.velocity *= 0.4f;
                        }
                    }

                    // Fireball explosion when head is detached
                    if (Main.netMode != NetmodeID.MultiplayerClient && (!headAlive || turboEnrage || CalamityWorld.LegendaryMode))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int fiery = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                            Main.dust[fiery].velocity.Y *= 6f;
                            Main.dust[fiery].velocity.X *= 3f;
                            if (Main.rand.NextBool())
                            {
                                Main.dust[fiery].scale = 0.5f;
                                Main.dust[fiery].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                            }
                        }
                        for (int j = 0; j < 20; j++)
                        {
                            int fiery2 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                            Main.dust[fiery2].noGravity = true;
                            Main.dust[fiery2].velocity.Y *= 10f;
                            fiery2 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                            Main.dust[fiery2].velocity.X *= 2f;
                        }

                        float projectileVelocity = death ? 10.5f : 7.25f;
                        if (enrage)
                            projectileVelocity *= 1.5f;
                        if (turboEnrage)
                            projectileVelocity *= 1.25f;

                        int type = ProjectileID.Fireball;
                        int damage = npc.GetProjectileDamage(type);
                        Vector2 destination = new Vector2(npc.Center.X, npc.Center.Y - 100f) - npc.Center;
                        destination.Normalize();
                        destination *= projectileVelocity;
                        int totalFireballsPerSide = death ? 3 : 2;
                        int totalIterations = (turboEnrage && CalamityWorld.LegendaryMode) ? 11 : death ? 25 : 35;
                        float rotation = MathHelper.ToRadians(90);
                        for (int i = 0; i < totalIterations; i++)
                        {
                            // Spawn projectiles 0, 1, 2, 22, 23, and 24 (in non-master)
                            if (i < totalFireballsPerSide || i >= totalIterations - totalFireballsPerSide)
                            {
                                Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(totalIterations - 1)));
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + Vector2.UnitY * (npc.height / 2 * 0.8f) * npc.scale + Vector2.Normalize(perturbedSpeed) * (npc.width / 3) * npc.scale, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                                Main.projectile[proj].timeLeft = enrage ? 480 : 150; // The difference is meant to be this stark.
                                if (turboEnrage && CalamityWorld.LegendaryMode)
                                    Main.projectile[proj].extraUpdates += 1;
                            }
                        }

                        npc.netUpdate = true;
                    }
                }
                else
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    // Velocity when falling
                    if (npc.ai[2] == 2f)
                    {
                        // Do not collide with tiles while doing this crazy shit
                        npc.noTileCollide = true;

                        float laserShootGateValue = death ? 8f : 10.5f;
                        if (npc.ai[3] % laserShootGateValue == 0f)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 fireFrom = new Vector2(npc.Center.X, npc.Center.Y - 60f);
                                int projectileAmt = 2;
                                int type = ProjectileID.EyeBeam;
                                int damage = npc.GetProjectileDamage(type);
                                Vector2 laserVelocity = Vector2.UnitY * npc.velocity.Y * (turboEnrage ? 2f : enrage ? 1f : 0.5f);
                                for (int i = 0; i < projectileAmt; i++)
                                {
                                    int totalProjectiles = 2;
                                    float radians = MathHelper.TwoPi / totalProjectiles;
                                    for (int j = 0; j < totalProjectiles; j++)
                                    {
                                        Vector2 projVelocity = laserVelocity.RotatedBy(radians * j + MathHelper.PiOver2);
                                        int bodyLaser = Projectile.NewProjectile(npc.GetSource_FromAI(), fireFrom, projVelocity, type, damage, 0f, Main.myPlayer);
                                        Main.projectile[bodyLaser].timeLeft = enrage ? 720 : 360;
                                        if (turboEnrage && CalamityWorld.LegendaryMode)
                                            Main.projectile[bodyLaser].extraUpdates += 1;
                                    }
                                }
                            }
                        }

                        npc.ai[3] -= 1f;
                        if (npc.ai[3] <= 0f)
                        {
                            npc.ai[2] = 0f;
                            npc.ai[3] = 0f;
                            npc.ForceNetUpdate();
                        }
                    }
                    else
                    {
                        if ((npc.position.X < Main.player[npc.target].position.X && npc.position.X + npc.width > Main.player[npc.target].position.X + Main.player[npc.target].width) || npc.ai[2] == 1f)
                        {
                            npc.velocity.X *= npc.ai[2] == 1f ? 0.5f : 0.8f;

                            if (npc.Bottom.Y < Main.player[npc.target].position.Y || npc.ai[2] == 1f)
                            {
                                float fallSpeedBoost = death ? 0.9f * (1f - (lifeRatio / 2)) : 0.75f * (1f - (lifeRatio / 2));
                                float fallSpeed = (death ? 0.3f : 0.2f) + fallSpeedBoost;
                                if (enrage)
                                    fallSpeed *= 2f;

                                npc.velocity.Y += fallSpeed;
                            }
                        }
                        else
                        {
                            float velocityChangeBoost = death ? 0.16f * (1f - (lifeRatio / 2)) : 0.12f * (1f - (lifeRatio / 2));
                            float velocityXChange = (death ? 0.285f : 0.2f) + velocityChangeBoost;
                            if (npc.direction < 0)
                                npc.velocity.X -= velocityXChange;
                            else if (npc.direction > 0)
                                npc.velocity.X += velocityXChange;

                            float velocityBoost = death ? 5.75f * (1f - (lifeRatio / 2)) : 4f * (1f - (lifeRatio / 2));
                            float velocityXCap = (death ? 8.75f : 6f) + velocityBoost;
                            if (enrage)
                                velocityXCap *= 3f;

                            float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                            int directionRelativeToTarget = playerLocation < 0 ? 1 : -1;
                            bool slowDown = directionRelativeToTarget != calamityGlobalNPC.newAI[1];

                            if (slowDown)
                                velocityXCap *= (enrage ? 0.2f : 0.5f);

                            if (npc.velocity.X < -velocityXCap)
                                npc.velocity.X = -velocityXCap;
                            if (npc.velocity.X > velocityXCap)
                                npc.velocity.X = velocityXCap;
                        }

                        CustomGravity(npc.ai[2] == 1f);
                    }
                }
            }

            void CustomGravity(bool isSlamming)
            {
                float gravity = turboEnrage ? (CalamityWorld.LegendaryMode ? 1.15f : 0.85f) : enrage ? 0.75f : (!leftFistAlive && !rightFistAlive) ? 0.45f : 0.3f;
                float maxFallSpeed = reduceFallSpeed ? 12f : turboEnrage ? (CalamityWorld.LegendaryMode ? 40f : 30f) : enrage ? 25f : (!leftFistAlive && !rightFistAlive) ? 15f : 10f;
                if (isSlamming && !reduceFallSpeed)
                {
                    gravity *= 4f;
                    maxFallSpeed *= 2f;
                }

                npc.velocity.Y += gravity;
                if (npc.velocity.Y > maxFallSpeed)
                    npc.velocity.Y = maxFallSpeed;
            }

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            // Despawn
            if (Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) + Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) > despawnDistance)
            {
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

                if (Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) + Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) > despawnDistance)
                {
                    npc.active = false;
                    npc.netUpdate = true;
                }
            }

            return false;
        }

        public static bool BuffedGolemFistAI(NPC npc, Mod mod)
        {
            if (NPC.golemBoss < 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            if (npc.alpha > 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;
            }

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            NPC golem = Main.npc[NPC.golemBoss];
            Player player = Main.player[npc.target];

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Enrage if the target isn't inside the temple
            // Turbo enrage if target isn't inside the temple and it's Boss Rush or For the Worthy
            bool enrage = true;
            bool turboEnrage = false;
            if (player.Center.Y > Main.worldSurface * 16.0)
            {
                int targetTilePosX = (int)player.Center.X / 16;
                int targetTilePosY = (int)player.Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || CalamityWorld.LegendaryMode;
            }
            else
                turboEnrage = bossRush || CalamityWorld.LegendaryMode;

            if (bossRush || CalamityWorld.LegendaryMode)
                enrage = true;

            float aggression = turboEnrage ? (CalamityWorld.LegendaryMode ? 4f : 3f) : enrage ? 2f : death ? 1.7f : 1f;

            Vector2 fistCenter = golem.Center + golem.velocity + new Vector2(0f, -9f * npc.scale);
            fistCenter.X += (float)((npc.type == NPCID.GolemFistLeft) ? -84 : 78) * npc.scale;
            Vector2 distanceFromFistCenter = fistCenter - npc.Center;
            float distanceFromRestPosition = distanceFromFistCenter.Length();
            if (npc.ai[0] == 0f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.noTileCollide = true;

                float fistSpeed = 28f;
                fistSpeed *= (aggression + 3f) / 4f;
                if (fistSpeed > 48f)
                    fistSpeed = 48f;

                float fistRestDistance = distanceFromRestPosition;
                if (fistRestDistance < 12f + fistSpeed)
                {
                    npc.rotation = 0f;
                    npc.velocity.X = distanceFromFistCenter.X;
                    npc.velocity.Y = distanceFromFistCenter.Y;

                    bool canPunch = npc.alpha == 0 && (npc.type == NPCID.GolemFistLeft && npc.Center.X + 100f > player.Center.X) || (npc.type == NPCID.GolemFistRight && npc.Center.X - 100f < player.Center.X);
                    if (canPunch)
                    {
                        float fistShootSpeed = death ? Main.rand.NextFloat(aggression * 0.5f, aggression * 2f) : aggression;
                        npc.ai[1] += fistShootSpeed;
                        if (npc.life < npc.lifeMax / 2)
                            npc.ai[1] += fistShootSpeed;
                        if (npc.life < npc.lifeMax / 4)
                            npc.ai[1] += fistShootSpeed;
                    }

                    float fistPunchGateValue = death ? 120f : 40f;
                    if (npc.ai[1] >= fistPunchGateValue)
                    {
                        if (canPunch)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[0] = 1f;
                        }
                        else
                            npc.ai[1] = 0f;

                        // Net update in Master due to rng
                        if (death)
                            npc.ForceNetUpdate();
                    }
                }
                else
                {
                    fistRestDistance = fistSpeed / fistRestDistance;
                    npc.velocity.X = distanceFromFistCenter.X * fistRestDistance;
                    npc.velocity.Y = distanceFromFistCenter.Y * fistRestDistance;

                    npc.rotation = (float)Math.Atan2(-npc.velocity.Y, -npc.velocity.X);
                    if (npc.type == NPCID.GolemFistLeft)
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);
                }
            }
            else if (npc.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                npc.ai[1] += 1f;
                npc.Center = fistCenter;
                npc.rotation = 0f;
                npc.velocity = Vector2.Zero;
                if (npc.ai[1] <= 15f)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        Vector2 largeRandDustRadius = Main.rand.NextVector2Circular(80f, 80f);
                        Vector2 largeRandDustRecoil = largeRandDustRadius * -1f * 0.05f;
                        Vector2 smallRandDustRadius = Main.rand.NextVector2Circular(20f, 20f);
                        Dust dust = Dust.NewDustPerfect(npc.Center + largeRandDustRecoil + largeRandDustRadius + smallRandDustRadius, 228, largeRandDustRecoil);
                        dust.fadeIn = 1.5f;
                        dust.scale = 0.5f;
                        if (CalamityWorld.LegendaryMode)
                            dust.noLight = true;

                        dust.noGravity = true;
                    }
                }

                if (npc.ai[1] >= 30f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.noTileCollide = true;
                    npc.collideX = false;
                    npc.collideY = false;

                    float fistReturnSpeed = 24f;
                    fistReturnSpeed *= (aggression + 3f) / 4f;
                    if (fistReturnSpeed > 48f)
                        fistReturnSpeed = 48f;

                    Vector2 fistCent = npc.Center;
                    float fistTargetXDist = player.Center.X - fistCent.X;
                    float fistTargetYDist = player.Center.Y - fistCent.Y;
                    float fistTargetDistance = (float)Math.Sqrt(fistTargetXDist * fistTargetXDist + fistTargetYDist * fistTargetYDist);
                    fistTargetDistance = fistReturnSpeed / fistTargetDistance;
                    npc.velocity.X = fistTargetXDist * fistTargetDistance;
                    npc.velocity.Y = fistTargetYDist * fistTargetDistance;
                    npc.ai[0] = 2f;
                    npc.ai[1] = 0f;

                    npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);
                    if (npc.type == NPCID.GolemFistLeft)
                        npc.rotation = (float)Math.Atan2(-npc.velocity.Y, -npc.velocity.X);
                }
            }
            else if (npc.ai[0] == 2f)
            {
                // Set damage
                npc.damage = npc.defDamage;

                if (Main.netMode != NetmodeID.MultiplayerClient && CalamityWorld.LegendaryMode)
                {
                    for (int j = (int)(npc.position.X / 16f) - 1; (float)j < (npc.position.X + (float)npc.width) / 16f + 1f; j++)
                    {
                        for (int k = (int)(npc.position.Y / 16f) - 1; (float)k < (npc.position.Y + (float)npc.width) / 16f + 1f; k++)
                        {
                            if (Main.tile[j, k].TileType == TileID.Torches)
                            {
                                Main.tile[j, k].Get<TileWallWireStateData>().HasTile = false;
                                if (Main.dedServ)
                                    NetMessage.SendTileSquare(-1, j, k);
                            }
                        }
                    }
                }

                npc.ai[1] += 1f;
                if (npc.ai[1] == 1f)
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);

                if (Main.rand.NextBool())
                {
                    Vector2 halfVelocityDust = npc.velocity * 0.5f;
                    Vector2 randDustRadius = Main.rand.NextVector2Circular(20f, 20f);
                    Dust.NewDustPerfect(npc.Center + halfVelocityDust + randDustRadius, 306, halfVelocityDust, 0, Main.OurFavoriteColor).scale = 2f;
                }

                if (Math.Abs(npc.velocity.X) > Math.Abs(npc.velocity.Y))
                {
                    if (npc.velocity.X > 0f && npc.Center.X > player.Center.X)
                        npc.noTileCollide = false;

                    if (npc.velocity.X < 0f && npc.Center.X < player.Center.X)
                        npc.noTileCollide = false;
                }
                else
                {
                    if (npc.velocity.Y > 0f && npc.Center.Y > player.Center.Y)
                        npc.noTileCollide = false;

                    if (npc.velocity.Y < 0f && npc.Center.Y < player.Center.Y)
                        npc.noTileCollide = false;
                }

                float maxPunchDistance = 700f;
                if (death)
                {
                    if (npc.life < npc.lifeMax / 2)
                        maxPunchDistance += MathHelper.Lerp(-175f, 75f, Main.rand.NextFloat());
                    if (npc.life < npc.lifeMax / 4)
                        maxPunchDistance += MathHelper.Lerp(-175f, 75f, Main.rand.NextFloat());
                }

                if (distanceFromRestPosition > maxPunchDistance || npc.collideX || npc.collideY)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;
                    npc.noTileCollide = true;
                    npc.ai[0] = 0f;
                }
            }
            else
            {
                if (npc.ai[0] != 3f)
                    return false;

                // Set damage
                npc.damage = npc.defDamage;

                npc.noTileCollide = true;
                float fistAcceleration = 0.4f;
                Vector2 returningFistCenter = npc.Center;
                float returningTargetX = player.Center.X - returningFistCenter.X;
                float returningTargetY = player.Center.Y - returningFistCenter.Y;
                float returningTargetDist = (float)Math.Sqrt(returningTargetX * returningTargetX + returningTargetY * returningTargetY);
                returningTargetDist = 12f / returningTargetDist;
                returningTargetX *= returningTargetDist;
                returningTargetY *= returningTargetDist;

                if (npc.velocity.X < returningTargetX)
                {
                    npc.velocity.X += fistAcceleration;
                    if (npc.velocity.X < 0f && returningTargetX > 0f)
                        npc.velocity.X += fistAcceleration * 2f;
                }
                else if (npc.velocity.X > returningTargetX)
                {
                    npc.velocity.X -= fistAcceleration;
                    if (npc.velocity.X > 0f && returningTargetX < 0f)
                        npc.velocity.X -= fistAcceleration * 2f;
                }

                if (npc.velocity.Y < returningTargetY)
                {
                    npc.velocity.Y += fistAcceleration;
                    if (npc.velocity.Y < 0f && returningTargetY > 0f)
                        npc.velocity.Y += fistAcceleration * 2f;
                }
                else if (npc.velocity.Y > returningTargetY)
                {
                    npc.velocity.Y -= fistAcceleration;
                    if (npc.velocity.Y > 0f && returningTargetY < 0f)
                        npc.velocity.Y -= fistAcceleration * 2f;
                }

                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);
                if (npc.type == NPCID.GolemFistLeft)
                    npc.rotation = (float)Math.Atan2(-npc.velocity.Y, -npc.velocity.X);
            }

            return false;
        }

        public static bool BuffedGolemHeadAI(NPC npc, Mod mod)
        {
            // Don't collide
            npc.noTileCollide = true;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            // Die if body is gone
            if (NPC.golemBoss < 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Count body parts
            bool leftFistAlive = NPC.AnyNPCs(NPCID.GolemFistLeft);
            bool rightFistAlive = NPC.AnyNPCs(NPCID.GolemFistRight);
            npc.dontTakeDamage = (leftFistAlive || rightFistAlive) && !CalamityWorld.LegendaryMode;

            // Stay in position on top of body
            npc.Center = Main.npc[NPC.golemBoss].Center - new Vector2(3f, 57f) * npc.scale;

            // Enrage if the target isn't inside the temple
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int targetTilePosX = (int)Main.player[npc.target].Center.X / 16;
                int targetTilePosY = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || CalamityWorld.LegendaryMode;
            }
            else
                turboEnrage = bossRush || CalamityWorld.LegendaryMode;

            if (bossRush || CalamityWorld.LegendaryMode)
                enrage = true;

            // Alpha
            if (npc.alpha > 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                npc.ai[1] = 30f;
            }

            // Spit fireballs if arms are alive
            if (npc.ai[0] == 0f)
            {
                npc.ai[1] += 1f;
                float openMouthGateValue = (!rightFistAlive || !leftFistAlive) ? 10f : 20f;
                float shootFireballGateValue = (!rightFistAlive || !leftFistAlive) ? 60f : 120f;
                if (npc.ai[1] < openMouthGateValue || npc.ai[1] > shootFireballGateValue - openMouthGateValue)
                    npc.localAI[0] = 1f;
                else
                    npc.localAI[0] = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= shootFireballGateValue)
                {
                    npc.ai[1] = 0f;

                    Vector2 headCent = new Vector2(npc.Center.X, npc.Center.Y + 10f * npc.scale);
                    float headFireballSpeed = turboEnrage ? 24f : enrage ? 18f : 9f;
                    float headFireballTargetX = Main.player[npc.target].Center.X - headCent.X;
                    float headFireballTargetY = Main.player[npc.target].Center.Y - headCent.Y;
                    float headFireballTargetDist = (float)Math.Sqrt(headFireballTargetX * headFireballTargetX + headFireballTargetY * headFireballTargetY);

                    headFireballTargetDist = headFireballSpeed / headFireballTargetDist;
                    headFireballTargetX *= headFireballTargetDist;
                    headFireballTargetY *= headFireballTargetDist;

                    int type = ProjectileID.Fireball;
                    int damage = npc.GetProjectileDamage(type);

                    int fireballAmount = death ? 2 : 1;
                    Vector2 fireballVelocity = new Vector2(headFireballTargetX, headFireballTargetY);
                    for (int i = 0; i < fireballAmount; i++)
                        Projectile.NewProjectile(npc.GetSource_FromAI(), headCent, fireballVelocity * (1f / (i + 1)), type, damage, 0f, Main.myPlayer);

                    npc.netUpdate = true;
                }
            }

            // Shoot lasers and fireballs if arms are dead
            else if (npc.ai[0] == 1f)
            {
                // Fire projectiles from eye positions
                Vector2 projectileFirePos = new Vector2(npc.Center.X, npc.Center.Y + 10f * npc.scale);
                if (Main.player[npc.target].Center.X < npc.Center.X - npc.width)
                {
                    npc.localAI[1] = -1f;
                    projectileFirePos.X -= 40f * npc.scale;
                }
                else if (Main.player[npc.target].Center.X > npc.Center.X + npc.width)
                {
                    npc.localAI[1] = 1f;
                    projectileFirePos.X += 40f * npc.scale;
                }
                else
                    npc.localAI[1] = 0f;

                // Fireballs
                npc.ai[1] += 1f;
                float openMouthGateValue = 20f - (death ? 15f * (1f - (lifeRatio / 2)) : 10f * (1f - (lifeRatio / 2)));
                float shootFireballGateValue = 120f - (death ? 75f * (1f - (lifeRatio / 2)) : 50f * (1f - (lifeRatio / 2)));
                if (npc.ai[1] < openMouthGateValue || npc.ai[1] > shootFireballGateValue - openMouthGateValue)
                    npc.localAI[0] = 1f;
                else
                    npc.localAI[0] = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= shootFireballGateValue)
                {
                    npc.ai[1] = 0f;

                    float fireballSpeedFistsDed = turboEnrage ? 28f : enrage ? 21f : 10.5f;
                    float fireballFistsDedTargetX = Main.player[npc.target].Center.X - projectileFirePos.X;
                    float fireballFistsDedTargetY = Main.player[npc.target].Center.Y - projectileFirePos.Y;
                    float fireballFistsDedTargetDist = (float)Math.Sqrt(fireballFistsDedTargetX * fireballFistsDedTargetX + fireballFistsDedTargetY * fireballFistsDedTargetY);

                    fireballFistsDedTargetDist = fireballSpeedFistsDed / fireballFistsDedTargetDist;
                    fireballFistsDedTargetX *= fireballFistsDedTargetDist;
                    fireballFistsDedTargetY *= fireballFistsDedTargetDist;

                    int type = ProjectileID.Fireball;
                    int damage = npc.GetProjectileDamage(type);

                    int fireballAmount = death ? 2 : 1;
                    Vector2 fireballVelocity = new Vector2(fireballFistsDedTargetX, fireballFistsDedTargetY);
                    for (int i = 0; i < fireballAmount; i++)
                    {
                        int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileFirePos, fireballVelocity * (1f / (i + 1)), type, damage, 0f, Main.myPlayer);
                        Main.projectile[proj].timeLeft = 255;
                    }

                    npc.netUpdate = true;
                }

                // Lasers
                float shootBoost2 = death ? 4.5f * (1f - (lifeRatio / 2)) : 2.75f * (1f - (lifeRatio / 2));
                npc.ai[2] += 1f + shootBoost2;
                if (enrage)
                    npc.ai[2] += 4f;

                if (npc.ai[2] >= 300f)
                {
                    npc.ai[2] = 0f;

                    int projType = ProjectileID.EyeBeam;
                    int dmg = npc.GetProjectileDamage(projType);

                    if (npc.localAI[1] == 0f)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            projectileFirePos = new Vector2(npc.Center.X, npc.Center.Y - 22f * npc.scale);
                            if (i == 0)
                                projectileFirePos.X -= 18f * npc.scale;
                            else
                                projectileFirePos.X += 18f * npc.scale;

                            float laserSpeed = death ? 15f : 12f;
                            float laserTargetXDist = Main.player[npc.target].Center.X - projectileFirePos.X;
                            float laserTargetYDist = Main.player[npc.target].Center.Y - projectileFirePos.Y;
                            float laserTargetDistance = (float)Math.Sqrt(laserTargetXDist * laserTargetXDist + laserTargetYDist * laserTargetYDist);

                            laserTargetDistance = laserSpeed / laserTargetDistance;
                            laserTargetXDist *= laserTargetDistance;
                            laserTargetYDist *= laserTargetDistance;

                            Vector2 laserVelocity = new Vector2(laserTargetXDist, laserTargetYDist);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int bodyLaser = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileFirePos + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, projType, dmg, 0f, Main.myPlayer);
                                Main.projectile[bodyLaser].timeLeft = enrage ? 600 : 300;
                                if (turboEnrage && CalamityWorld.LegendaryMode)
                                    Main.projectile[bodyLaser].extraUpdates += 1;

                                npc.netUpdate = true;
                            }
                        }
                    }
                    else if (npc.localAI[1] != 0f)
                    {
                        projectileFirePos = new Vector2(npc.Center.X, npc.Center.Y - 22f * npc.scale);
                        if (npc.localAI[1] == -1f)
                            projectileFirePos.X -= 30f * npc.scale;
                        else if (npc.localAI[1] == 1f)
                            projectileFirePos.X += 30f * npc.scale;

                        float extraLaserSpeed = death ? 15f : 12f;
                        float extraLaserTargetX = Main.player[npc.target].Center.X - projectileFirePos.X;
                        float extraLaserTargetY = Main.player[npc.target].Center.Y - projectileFirePos.Y;
                        float extraLaserTargetDist = (float)Math.Sqrt(extraLaserTargetX * extraLaserTargetX + extraLaserTargetY * extraLaserTargetY);

                        extraLaserTargetDist = extraLaserSpeed / extraLaserTargetDist;
                        extraLaserTargetX *= extraLaserTargetDist;
                        extraLaserTargetY *= extraLaserTargetDist;

                        Vector2 laserVelocity = new Vector2(extraLaserTargetX, extraLaserTargetY);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int extraLasers = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileFirePos + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, projType, dmg, 0f, Main.myPlayer);
                            Main.projectile[extraLasers].timeLeft = enrage ? 600 : 300;
                            if (turboEnrage && CalamityWorld.LegendaryMode)
                                Main.projectile[extraLasers].extraUpdates += 1;

                            npc.netUpdate = true;
                        }
                    }
                }
            }

            // Laser fire if arms are dead
            if ((!leftFistAlive && !rightFistAlive) || death || CalamityWorld.LegendaryMode)
            {
                npc.ai[0] = 1f;
                return false;
            }
            npc.ai[0] = 0f;

            return false;
        }

        public static bool BuffedGolemHeadFreeAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            // Die if body is gone
            if (NPC.golemBoss < 0)
            {
                calamityGlobalNPC.DR = 0.25f;
                calamityGlobalNPC.unbreakableDR = false;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = false;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;
            float golemLifeRatio = Main.npc[NPC.golemBoss].life / (float)Main.npc[NPC.golemBoss].lifeMax;

            // Phases
            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;
            bool phase2 = lifeRatio < 0.7f || golemLifeRatio < 0.85f || death;
            bool phase3 = lifeRatio < 0.55f || golemLifeRatio < 0.7f || death;
            bool phase4 = lifeRatio < 0.4f || golemLifeRatio < 0.55f || death;

            // Enrage if the target isn't inside the temple
            bool enrage = true;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int targetTilePosX = (int)Main.player[npc.target].Center.X / 16;
                int targetTilePosY = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = bossRush || CalamityWorld.LegendaryMode;
            }
            else
                turboEnrage = bossRush || CalamityWorld.LegendaryMode;

            if (bossRush || CalamityWorld.LegendaryMode)
                enrage = true;

            if (turboEnrage)
            {
                calamityGlobalNPC.DR = 0.9999f;
                calamityGlobalNPC.unbreakableDR = true;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = true;
            }

            // Float through tiles or not
            bool canPassThroughTiles = false;
            if (!Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) || phase3 || turboEnrage)
            {
                npc.noTileCollide = true;
                canPassThroughTiles = true;
            }
            else
                npc.noTileCollide = false;

            // Move to new location
            if (npc.ai[3] <= 0f)
            {
                npc.ai[3] = 400f;

                float maxDistance = 400f;

                // Four corners around target
                if (phase3 || turboEnrage)
                {
                    if (calamityGlobalNPC.newAI[1] == -maxDistance)
                    {
                        switch ((int)calamityGlobalNPC.newAI[0])
                        {
                            case 0:
                            case 400:
                                calamityGlobalNPC.newAI[0] = -maxDistance;
                                break;
                            case -400:
                                calamityGlobalNPC.newAI[1] = maxDistance;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch ((int)calamityGlobalNPC.newAI[0])
                        {
                            case 0:
                            case -400:
                                calamityGlobalNPC.newAI[0] = maxDistance;
                                break;
                            case 400:
                                calamityGlobalNPC.newAI[1] = -maxDistance;
                                break;
                            default:
                                break;
                        }
                    }
                }

                // Above target
                else if (phase2)
                {
                    switch ((int)calamityGlobalNPC.newAI[0])
                    {
                        case 0:
                            calamityGlobalNPC.newAI[0] = maxDistance;
                            break;
                        case 400:
                            calamityGlobalNPC.newAI[0] = -maxDistance;
                            break;
                        case -400:
                            calamityGlobalNPC.newAI[0] = 0f;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    calamityGlobalNPC.newAI[0] = 0f;
                    calamityGlobalNPC.newAI[1] = -maxDistance;
                }

                npc.netSpam = 5;
                npc.SyncExtraAI();
                npc.ForceNetUpdate();
            }

            npc.ai[3] -= 1f +
                (turboEnrage ? 1f : phase2 ? 0.5f : 0f) +
                (turboEnrage ? 1f : phase3 ? 0.5f : 0f) +
                (turboEnrage ? 2f : phase4 ? 1f : 0f);

            float offsetX = calamityGlobalNPC.newAI[0];
            float offsetY = calamityGlobalNPC.newAI[1];
            Vector2 destination = Main.player[npc.target].Center + new Vector2(offsetX, offsetY);

            // Velocity and acceleration
            float velocity = (turboEnrage ? 15f : 10f) +
                (turboEnrage ? 7.5f : phase2 ? 5f : 0f) +
                (turboEnrage ? 7.5f : phase3 ? 5f : 0f);

            if (enrage)
                velocity = (phase3 || turboEnrage) ? 35f : 25f;

            float acceleration = phase3 ? 0f : turboEnrage ? 6f : enrage ? 4.8f : phase2 ? 1.2f : 0.8f;

            // How far Golem's Head is from where it's supposed to be
            Vector2 distanceFromDestination = destination - npc.Center;

            // Whether Golem can fire projectiles
            bool canFireProjectiles = distanceFromDestination.Length() < 120f || enrage;

            CalamityUtils.SmoothMovement(npc, 80f, distanceFromDestination, velocity, acceleration, !phase3);

            if (death && calamityGlobalNPC.newAI[2] < 120f)
            {
                calamityGlobalNPC.newAI[2] += 1f;

                if (calamityGlobalNPC.newAI[2] % 15f == 0f)
                {
                    npc.netUpdate = true;
                    npc.SyncExtraAI();
                }

                return false;
            }

            // Fireballs
            if (canFireProjectiles)
                npc.ai[1] += 1f;

            float combinedLifeRatio = (lifeRatio + golemLifeRatio) * 0.5f;
            float openMouthGateValue = 20f - (death ? 13f * (1f - combinedLifeRatio) : 8.65f * (1f - combinedLifeRatio));
            float shootFireballGateValue = 240f - (death ? 150f * (1f - combinedLifeRatio) : 100f * (1f - combinedLifeRatio));
            if (npc.ai[1] < openMouthGateValue || npc.ai[1] > shootFireballGateValue - openMouthGateValue)
                npc.localAI[0] = 1f;
            else
                npc.localAI[0] = 0f;

            if (canPassThroughTiles && !phase3)
                npc.ai[1] = openMouthGateValue;

            if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= shootFireballGateValue)
            {
                npc.ai[1] = 0f;

                Vector2 freeHeadCenter = new Vector2(npc.Center.X, npc.Center.Y + 20f * npc.scale);
                float freeHeadSpeed = turboEnrage ? 24f : enrage ? 18f : 6f;
                if (death)
                    freeHeadSpeed *= 1.25f;

                float freeHeadTargetX = Main.player[npc.target].Center.X - freeHeadCenter.X;
                float freeHeadTargetY = Main.player[npc.target].Center.Y - freeHeadCenter.Y;
                float freeHeadTargetDist = (float)Math.Sqrt(freeHeadTargetX * freeHeadTargetX + freeHeadTargetY * freeHeadTargetY);

                freeHeadTargetDist = freeHeadSpeed / freeHeadTargetDist;
                freeHeadTargetX *= freeHeadTargetDist;
                freeHeadTargetY *= freeHeadTargetDist;

                int projectileType = (phase3 || death) ? ProjectileID.InfernoHostileBolt : ProjectileID.Fireball;
                int damage = npc.GetProjectileDamage(projectileType);
                float ai0 = projectileType == ProjectileID.InfernoHostileBolt ? Main.player[npc.target].Center.X : 0f;
                float ai1 = projectileType == ProjectileID.InfernoHostileBolt ? Main.player[npc.target].Center.Y : 0f;
                float ai2 = projectileType == ProjectileID.InfernoHostileBolt ? 1f : 0f;
                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), freeHeadCenter.X, freeHeadCenter.Y, freeHeadTargetX, freeHeadTargetY, projectileType, damage, 0f, Main.myPlayer, ai0, ai1, ai2);
                if (projectileType == ProjectileID.InfernoHostileBolt)
                {
                    Main.projectile[proj].timeLeft = 300;
                    Main.projectile[proj].netUpdate = true;
                }
                else
                    Main.projectile[proj].timeLeft = 360;

                npc.netUpdate = true;
            }

            // Lasers
            if (canFireProjectiles)
                npc.ai[2] += 1f;

            float laserGateValue = 300f - (death ? 195f * (1f - combinedLifeRatio) : 160f * (1f - combinedLifeRatio));
            if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[2] >= laserGateValue)
            {
                npc.ai[2] = 0f;

                int numLasers = 2;
                bool leftLaserIsFast = Main.rand.NextBool();
                for (int i = 0; i < numLasers; i++)
                {
                    Vector2 freeHeadProjSpawn = new Vector2(npc.Center.X, npc.Center.Y - 20f * npc.scale);
                    if (i == 0)
                        freeHeadProjSpawn.X -= 14f * npc.scale;
                    else if (i == 1)
                        freeHeadProjSpawn.X += 14f * npc.scale;

                    float freeHeadProjSpeed = 5f + (death ? 5f * (1f - combinedLifeRatio) : 3f * (1f - combinedLifeRatio));
                    if (death)
                    {
                        if (i == 0)
                        {
                            if (leftLaserIsFast)
                                freeHeadProjSpeed *= 1.25f;
                            else
                                freeHeadProjSpeed *= 0.75f;
                        }
                        else
                        {
                            if (!leftLaserIsFast)
                                freeHeadProjSpeed *= 1.25f;
                            else
                                freeHeadProjSpeed *= 0.75f;
                        }
                    }

                    float freeHeadProjTargetX = Main.player[npc.target].Center.X - freeHeadProjSpawn.X;
                    float freeHeadProjTargetY = Main.player[npc.target].Center.Y - freeHeadProjSpawn.Y;
                    float freeHeadProjTargetDist = (float)Math.Sqrt(freeHeadProjTargetX * freeHeadProjTargetX + freeHeadProjTargetY * freeHeadProjTargetY);

                    freeHeadProjTargetDist = freeHeadProjSpeed / freeHeadProjTargetDist;
                    freeHeadProjTargetX *= freeHeadProjTargetDist;
                    freeHeadProjTargetY *= freeHeadProjTargetDist;

                    Vector2 laserVelocity = new Vector2(freeHeadProjTargetX, freeHeadProjTargetY);
                    int type = ProjectileID.EyeBeam;
                    int damage = npc.GetProjectileDamage(type);
                    int freeHeadLaser = Projectile.NewProjectile(npc.GetSource_FromAI(), freeHeadProjSpawn + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, type, damage, 0f, Main.myPlayer);
                    Main.projectile[freeHeadLaser].timeLeft = enrage ? 600 : 300;
                    if (turboEnrage && CalamityWorld.LegendaryMode)
                        Main.projectile[freeHeadLaser].extraUpdates += 1;
                }
            }

            if (!CalamityWorld.LegendaryMode)
            {
                npc.position += npc.netOffset;
                int randDustOffset = Main.rand.Next(2) * 2 - 1;
                Vector2 randDustPos = npc.Bottom + new Vector2((float)(randDustOffset * 22) * npc.scale, -22f * npc.scale);
                Dust getGoodDust = Dust.NewDustPerfect(randDustPos, 228, (MathHelper.PiOver2 + -MathHelper.PiOver2 * (float)randDustOffset + Main.rand.NextFloatDirection() * MathHelper.PiOver4).ToRotationVector2() * (2f + Main.rand.NextFloat()));
                Dust dust = getGoodDust;
                dust.velocity += npc.velocity;
                getGoodDust.noGravity = true;
                getGoodDust = Dust.NewDustPerfect(npc.Bottom + new Vector2(Main.rand.NextFloatDirection() * 6f * npc.scale, (Main.rand.NextFloat() * -4f - 8f) * npc.scale), 228, Vector2.UnitY * (2f + Main.rand.NextFloat()));
                getGoodDust.fadeIn = 0f;
                getGoodDust.scale = 0.7f + Main.rand.NextFloat() * 0.5f;
                getGoodDust.noGravity = true;
                dust = getGoodDust;
                dust.velocity += npc.velocity;
                npc.position -= npc.netOffset;
            }

            return false;
        }
    }
}
