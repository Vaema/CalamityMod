using System;
using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
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
        // Rev+ exclusive
        public static int FireballDamage = 24; // 96 (modified to be always at maximum Expert damage and does not scale)
        public static int LaserDamage = 29; // 116 (modified to be always at maximum Expert damage and does not scale)
        public static int InfernoBoltDamage = 35; // 140

        public static bool BuffedGolemAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // whoAmI variable
            NPC.golemBoss = npc.whoAmI;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
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
            // Turbo enrage if target isn't inside the temple and it's For the Worthy
            bool enrage = !BossRushEvent.BossRushActive;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0)
            {
                int targetTilePosX = (int)Main.player[npc.target].Center.X / 16;
                int targetTilePosY = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = Main.getGoodWorld;
            }
            else
                turboEnrage = Main.getGoodWorld;

            if (Main.getGoodWorld)
                enrage = true;

            npc.Calamity().CurrentlyEnraged = !BossRushEvent.BossRushActive && (enrage || turboEnrage);

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
            npc.dontTakeDamage = headAlive || leftFistAlive || rightFistAlive;

            // Distance required for despawning
            int despawnDistance = turboEnrage ? 7500 : enrage ? 6000 : 4500;

            // Deactivate torches
            if (Main.netMode != NetmodeID.MultiplayerClient && Main.getGoodWorld && npc.velocity.Y > 0f)
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
            if (!Main.getGoodWorld)
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

                    // Delay before jumping
                    if (npc.ai[2] == 0f)
                    {
                        npc.velocity.X *= 0.8f;
                        npc.ai[1] += 1f;
                    }

                    if (npc.ai[1] > 0f)
                    {
                        npc.ai[1] += death ? 1.5f : 1f;
                        if (Main.getGoodWorld)
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
                    bool canJump = (!headAlive || Main.npc[NPC.FindFirstNPC(NPCID.GolemHead)].ai[0] <= 1f) && (!NPC.AnyNPCs(NPCID.GolemHeadFree) || Main.npc[NPC.FindFirstNPC(NPCID.GolemHeadFree)].ai[0] != 3);
                    if (npc.ai[1] >= 300f && canJump)
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

                        if (npc.ai[3] == 0f)
                            npc.ai[3] = (death ? !leftFistAlive && !rightFistAlive : !headAlive) ? Main.rand.Next(1, 2+1) : 1f;

                        switch ((int)npc.ai[3])
                        {
                            default:
                            case 0:
                            case 1:
                                NormalJump(canJump);
                                break;

                            // Jump directly above the target's head and slam down
                            case 2:
                                SlamJump(canJump);
                                break;
                        }

                        void NormalJump(bool jump)
                        {
                            if (!jump)
                                return;

                            float velocityBoost = (death ? 5f : 3.8f) * (1f - (lifeRatio / 2));
                            float velocityX = (death ? 6f : 4f) + velocityBoost;
                            if (enrage)
                                velocityX *= 1.5f;

                            float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;
                            npc.direction = playerLocation < 0 ? 1 : -1;
                            calamityGlobalNPC.newAI[1] = npc.direction;

                            npc.velocity.X = velocityX * npc.direction;

                            float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                            float speedMult = 1f;

                            float multiplier = turboEnrage ? 0.00275f : enrage ? 0.0025f : 0.00175f;
                            if (distanceBelowTarget > 0f && ((!leftFistAlive && !rightFistAlive) || turboEnrage))
                                speedMult += distanceBelowTarget * multiplier;

                            float speedMultLimit = turboEnrage ? 3.25f : enrage ? 3f : 2.5f;
                            if (speedMult > speedMultLimit)
                                speedMult = speedMultLimit;

                            if (Main.player[npc.target].position.Y < npc.Bottom.Y)
                                npc.velocity.Y = ((turboEnrage ? -15.5f : -11.75f) + (enrage ? -4f : 0f)) * speedMult;
                            else
                                npc.velocity.Y = 1f;

                            npc.noTileCollide = true;

                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;

                            npc.netUpdate = true;
                            npc.SyncExtraAI();
                        }

                        void SlamJump(bool jump)
                        {
                            npc.noTileCollide = true;

                            npc.ai[2] += 1f;
                            float jumpVelocity = death ? 26f : 21f;
                            if (enrage)
                                jumpVelocity *= 1.25f;
                            if (turboEnrage)
                                jumpVelocity *= 1.25f;

                            float minJumpTime = 15f;
                            float maxJumpTime = 45f;
                            if ((npc.ai[2] >= minJumpTime && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) <= jumpVelocity) || npc.ai[2] >= maxJumpTime || !jump)
                            {
                                npc.ai[0] = 1f;
                                npc.ai[1] = 0f;
                                npc.ai[2] = 1f;
                                npc.velocity.Y = -3f;
                                npc.netUpdate = true;
                            }

                            if (!jump)
                                return;

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
                                if (distanceBelowTarget > 0f && ((!leftFistAlive && !rightFistAlive) || turboEnrage))
                                    speedMult += distanceBelowTarget * multiplier;

                                float speedMultLimit = turboEnrage ? 3.25f : enrage ? 3f : 2.5f;
                                if (speedMult > speedMultLimit)
                                    speedMult = speedMultLimit;

                                if (Main.player[npc.target].position.Y < npc.Bottom.Y)
                                    npc.velocity.Y *= speedMult;
                            }
                            else
                                npc.velocity.Y *= 0.95f;
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
                    if (Main.netMode != NetmodeID.MultiplayerClient && (!headAlive || turboEnrage))
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

                        float projectileVelocity = death ? 7.5f : 4.75f;
                        if (enrage)
                            projectileVelocity *= 1.5f;
                        if (turboEnrage)
                            projectileVelocity *= 1.25f;

                        int type = ProjectileID.Fireball;
                        int damage = FireballDamage;
                        Vector2 destination = new Vector2(npc.Center.X, npc.Center.Y - 100f) - npc.Center;
                        destination.Normalize();
                        destination *= projectileVelocity;
                        int totalFireballsPerSide = 3;
                        int totalIterations = turboEnrage ? 11 : death ? 40 : 60;
                        float rotation = MathHelper.ToRadians(90);
                        for (int i = 0; i < totalIterations; i++)
                        {
                            // Spawn projectiles 0, 1, 2, 22, 23, and 24
                            if (i < totalFireballsPerSide || i >= totalIterations - totalFireballsPerSide)
                            {
                                Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(totalIterations - 1)));
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + Vector2.UnitY * (npc.height / 2 * 0.8f) * npc.scale + Vector2.Normalize(perturbedSpeed) * (npc.width / 3) * npc.scale, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                                Main.projectile[proj].timeLeft = enrage ? 480 : 150; // The difference is meant to be this stark.
                                if (turboEnrage)
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
                        float velocityXCap = (death ? 6f : 4f) + velocityBoost;
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

            void CustomGravity(bool isSlamming)
            {
                float gravity = turboEnrage ? 0.85f : enrage ? 0.75f : (!leftFistAlive && !rightFistAlive) ? 0.45f : 0.3f;
                float maxFallSpeed = reduceFallSpeed ? 12f : turboEnrage ? 30f : enrage ? 25f : (!leftFistAlive && !rightFistAlive) ? 15f : 10f;
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

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Enrage if the target isn't inside the temple
            // Turbo enrage if target isn't inside the temple and it's For the Worthy
            bool enrage = !BossRushEvent.BossRushActive;
            bool turboEnrage = false;
            if (player.Center.Y > Main.worldSurface * 16.0 && !BossRushEvent.BossRushActive)
            {
                int targetTilePosX = (int)player.Center.X / 16;
                int targetTilePosY = (int)player.Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = Main.getGoodWorld;
            }
            else
                turboEnrage = Main.getGoodWorld;

            if (Main.getGoodWorld)
                enrage = true;

            float aggression = turboEnrage ? 3f : enrage ? 2f : death ? 1.7f : 1f;

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
                        if (Main.getGoodWorld)
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

                if (Main.netMode != NetmodeID.MultiplayerClient && Main.getGoodWorld)
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

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Count body parts
            bool leftFistAlive = NPC.AnyNPCs(NPCID.GolemFistLeft);
            bool rightFistAlive = NPC.AnyNPCs(NPCID.GolemFistRight);
            npc.dontTakeDamage = leftFistAlive || rightFistAlive;

            // Stay in position on top of body
            npc.Center = Main.npc[NPC.golemBoss].Center - new Vector2(3f, 57f) * npc.scale;
            npc.velocity = Main.npc[NPC.golemBoss].velocity;

            // Enrage if the target isn't inside the temple
            bool enrage = !BossRushEvent.BossRushActive;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0 && !BossRushEvent.BossRushActive)
            {
                int targetTilePosX = (int)Main.player[npc.target].Center.X / 16;
                int targetTilePosY = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = Main.getGoodWorld;
            }
            else
                turboEnrage = Main.getGoodWorld;

            if (Main.getGoodWorld)
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
                    int damage = FireballDamage;

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

                // Timer for special laser attack
                npc.ai[3]++;
                if (npc.ai[3] >= 600f && Main.npc[NPC.golemBoss].velocity.Y == 0f && MathF.Abs(Main.npc[NPC.golemBoss].velocity.X) < 0.5f)
                {
                    npc.ai[0] = 2f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.localAI[1] = (Main.player[npc.target].Center.X > npc.Center.X).ToDirectionInt();
                    npc.netUpdate = true;
                }

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
                    int damage = FireballDamage;

                    int fireballAmount = death ? 2 : 1;
                    Vector2 fireballVelocity = new Vector2(fireballFistsDedTargetX, fireballFistsDedTargetY);
                    for (int i = 0; i < fireballAmount; i++)
                    {
                        int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileFirePos, fireballVelocity * (1f / (i + 1)), type, damage, 0f, Main.myPlayer);
                        Main.projectile[proj].timeLeft = 225;
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
                    int dmg = LaserDamage;

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
                                if (turboEnrage)
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
                            if (turboEnrage)
                                Main.projectile[extraLasers].extraUpdates += 1;

                            npc.netUpdate = true;
                        }
                    }
                }
            }

            // Special laser spread attack
            else if (npc.ai[0] == 2f || npc.ai[0] == 3f)
            {
                int telegraphTime = 60;
                int endTime = 120;
                Vector2 spawnLocation = new Vector2(npc.Center.X + (30f * npc.scale * npc.localAI[1]), npc.Center.Y - 22f * npc.scale);
                if (npc.ai[1] == 1f)
                {
                    SparkleParticle eyeTele = new(spawnLocation, Vector2.Zero, Color.Yellow, Color.White, 1.25f * npc.scale, telegraphTime, MathHelper.Pi * 0.02f, needed: true);
                    GeneralParticleHandler.SpawnParticle(eyeTele);
                }

                npc.ai[1]++;
                if (npc.ai[1] >= telegraphTime && npc.ai[1] < endTime && npc.ai[1] % 2f == 0f)
                {
                    // Manually plays the sound at a slower rate (the sound from the lasers is disabled by setting ai[1])
                    if (npc.ai[1] % 10f == 0f)
                        SoundEngine.PlaySound(SoundID.Item33, spawnLocation);

                    float laserFireAngle = MathHelper.ToRadians((npc.ai[1] - telegraphTime + 20) * (death ? 2.45f : 2f));
                    Vector2 laserVelocity = Vector2.UnitY.RotatedBy(laserFireAngle * -npc.localAI[1]) * (death ? 15f : 12f);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int extraLasers = Projectile.NewProjectile(npc.GetSource_FromAI(), spawnLocation + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, ProjectileID.EyeBeam, LaserDamage, 0f, Main.myPlayer, 0f, 1f);
                        Main.projectile[extraLasers].timeLeft = enrage ? 600 : 300;
                        if (turboEnrage)
                            Main.projectile[extraLasers].extraUpdates += 1;

                        npc.netUpdate = true;
                    }
                }

                // Do another attack in the opposite direction in Death Mode
                if (death && npc.ai[0] == 2f && npc.ai[1] >= endTime)
                {
                    npc.ai[0] = 3f;
                    npc.ai[1] = 0f;
                    npc.localAI[1] = -npc.localAI[1];
                    npc.netUpdate = true;
                }

                if (npc.ai[1] >= endTime + 30)
                {
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.netUpdate = true;
                }
            }

            // Laser fire if arms are dead
            if ((!leftFistAlive && !rightFistAlive) || death || Main.getGoodWorld)
            {
                if (npc.ai[0] <= 1f)
                    npc.ai[0] = 1f;
            }
            else
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
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.StrikeInstantKill();

                return false;
            }

            // Percent life remaining
            float golemLifeRatio = Main.npc[NPC.golemBoss].life / (float)Main.npc[NPC.golemBoss].lifeMax;
            float PosDelay = 270f;

            // Phases
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
            bool phase2 = golemLifeRatio < 0.66f;
            bool phase3 = golemLifeRatio < 0.33f;

            // Enrage if the target isn't inside the temple
            bool enrage = !BossRushEvent.BossRushActive;
            bool turboEnrage = false;
            if (Main.player[npc.target].Center.Y > Main.worldSurface * 16.0 && !BossRushEvent.BossRushActive)
            {
                int targetTilePosX = (int)Main.player[npc.target].Center.X / 16;
                int targetTilePosY = (int)Main.player[npc.target].Center.Y / 16;

                Tile tile = Framing.GetTileSafely(targetTilePosX, targetTilePosY);
                if (tile.WallType == WallID.LihzahrdBrickUnsafe)
                    enrage = false;
                else
                    turboEnrage = Main.getGoodWorld;
            }
            else
                turboEnrage = Main.getGoodWorld;

            if (Main.getGoodWorld)
                enrage = true;

            // Float through tiles or not
            npc.noTileCollide = !Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) || phase2 || turboEnrage;

            // Immediately trigger the laser spread attack at phase transition thresholds
            if (phase2 && npc.ai[0] == 0f || phase3 && npc.ai[0] == 1f)
            {
                npc.ai[0] = 3f;
                npc.ai[1] = 0f;
                npc.ai[2] = 0f;
                npc.ai[3] = PosDelay;
                calamityGlobalNPC.newAI[3] = 0;
                npc.netUpdate = true;
                npc.SyncExtraAI();
            }

            // Move to new location
            float maxDistanceDiagonal = 360f;
            float maxDistanceStraight = 480f;
            if (npc.ai[3] <= 0f)
            {
                npc.ai[3] = PosDelay;
                calamityGlobalNPC.newAI[3] += phase2 ? 1 : 0;
                if (calamityGlobalNPC.newAI[3] >= 7)
                {
                    calamityGlobalNPC.newAI[3] = 0;
                    npc.ai[0] = 3f;
                    npc.ai[1] = 0f;
                    npc.ai[3] = PosDelay;
                }

                // Four positions around target
                if (!(phase3 || turboEnrage))
                {
                    if (phase2)
                    {
                        // 0 is top left, then goes counter-clockwise
                        switch ((int)(calamityGlobalNPC.newAI[3] % 4))
                        {
                            case 0:
                                calamityGlobalNPC.newAI[0] = -maxDistanceDiagonal;
                                calamityGlobalNPC.newAI[1] = -maxDistanceDiagonal;
                                break;
                            case 1:
                                calamityGlobalNPC.newAI[0] = -maxDistanceDiagonal;
                                calamityGlobalNPC.newAI[1] = maxDistanceDiagonal;
                                break;
                            case 2:
                                calamityGlobalNPC.newAI[0] = maxDistanceDiagonal;
                                calamityGlobalNPC.newAI[1] = maxDistanceDiagonal;
                                break;
                            case 3:
                                calamityGlobalNPC.newAI[0] = maxDistanceDiagonal;
                                calamityGlobalNPC.newAI[1] = -maxDistanceDiagonal;
                                break;
                        }
                    }

                    // Above target
                    else
                    {
                        calamityGlobalNPC.newAI[0] = Main.rand.NextFloat(-150f, 150f);
                        calamityGlobalNPC.newAI[1] = -maxDistanceStraight * 0.85f;
                    }
                }

                npc.netSpam = 5;
                npc.SyncExtraAI();
                npc.ForceNetUpdate();
            }

            float positioningInc = enrage ? 6f : phase3 ? 2.5f : phase2 ? 1.8f : 1f;
            npc.ai[3] -= positioningInc;

            // Move in a circle around the player in final phase
            if (phase3 || turboEnrage)
            {
                float spinSpeedMult = enrage ? 2.5f : death ? 1.35f : 1.2f;
                calamityGlobalNPC.newAI[0] = maxDistanceStraight * MathF.Sin(MathHelper.ToRadians(npc.ai[2] * spinSpeedMult));
                calamityGlobalNPC.newAI[1] = maxDistanceStraight * MathF.Cos(MathHelper.ToRadians(npc.ai[2] * spinSpeedMult));
            }
            // Always move above the player during laser spread attack
            if (npc.ai[0] == 3f)
            {
                calamityGlobalNPC.newAI[0] = 0f;
                calamityGlobalNPC.newAI[1] = -maxDistanceStraight * 0.8f;
            }

            float offsetX = calamityGlobalNPC.newAI[0];
            float offsetY = calamityGlobalNPC.newAI[1];
            Vector2 destination = Main.player[npc.target].Center + new Vector2(offsetX, offsetY);

            // Velocity and acceleration
            float velocity = (turboEnrage ? 15f : 10f) +
                (turboEnrage ? 7.5f : phase2 ? 10f : 0f) +
                (turboEnrage ? 7.5f : phase3 ? 15f : 0f);

            if (enrage)
                velocity = (phase3 || turboEnrage) ? 35f : 25f;

            float acceleration = npc.ai[0] == 3f ? 1.5f : phase2 ? 0f : turboEnrage ? 5f : enrage ? 3f : 0.3f;

            // How far Golem's Head is from where it's supposed to be
            Vector2 distanceFromDestination = destination - npc.Center;
            // Whether Golem can fire projectiles
            bool canFireProjectiles = (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f && npc.ai[0] != 3f) || enrage;

            // Ensure the free head stays still during the attack
            if (npc.ai[0] == 3f && npc.ai[1] >= 60f)
                npc.velocity = Vector2.Zero;
            else
                CalamityUtils.SmoothMovement(npc, 80f, distanceFromDestination, velocity, acceleration, !phase2 || npc.ai[0] == 3f);

            // Laser spread attack, followed by lingering flame bolts
            if (npc.ai[0] == 3f)
            {
                int telegraphTime = 60;
                int laserEndTime = 120;
                int fireballStartTime = 160;

                npc.ai[1]++;
                // Grrrrr stop incrementing the damn timer
                npc.ai[3] = PosDelay;
                // Laser spread
                if (npc.ai[1] >= telegraphTime && npc.ai[1] < laserEndTime && npc.ai[1] % 2f == 0f)
                {
                    // Manually plays the sound at a slower rate (the sound from the lasers is disabled by setting ai[1])
                    if (npc.ai[1] % 10f == 0f)
                        SoundEngine.PlaySound(SoundID.Item33, npc.Center);

                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 spawnLocation = new Vector2(npc.Center.X + 14f * npc.scale * i, npc.Center.Y - 20f * npc.scale);
                        float laserFireAngle = MathHelper.ToRadians((npc.ai[1] - telegraphTime + 20) * (death ? 2.1f : 2f));
                        Vector2 laserVelocity = -Vector2.UnitY.RotatedBy(laserFireAngle * i) * (death ? 15f : 12f);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int spreadLasers = Projectile.NewProjectile(npc.GetSource_FromAI(), spawnLocation + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, ProjectileID.EyeBeam, LaserDamage, 0f, Main.myPlayer, 0f, 1f);
                            Main.projectile[spreadLasers].timeLeft = enrage ? 600 : 300;
                            if (turboEnrage)
                                Main.projectile[spreadLasers].extraUpdates += 1;

                            npc.netUpdate = true;
                        }
                    }
                }
                // Sets the mouth to be open for the upcoming flame bolts
                if (npc.ai[1] > laserEndTime)
                    npc.localAI[0] = 1f;

                // Flame bolts
                if (npc.ai[1] >= fireballStartTime && npc.ai[1] % 30 == 10f)
                {
                    Vector2 spawnLocation = npc.Center + Vector2.UnitY * 20f * npc.scale;
                    Vector2 fireBoltVelocity = Utils.DirectionTo(spawnLocation, Main.player[npc.target].Center) * (enrage ? 30f : death ? 16f : 12f);

                    int type = ModContent.ProjectileType<GolemInfernoBolt>();
                    int damage = InfernoBoltDamage;
                    Projectile.NewProjectile(npc.GetSource_FromAI(), spawnLocation, fireBoltVelocity, type, damage, 0f, Main.myPlayer, Main.player[npc.target].Center.X, Main.player[npc.target].Center.Y);
                }

                // Death fires 2 bolts, Rev fires just 1
                if (npc.ai[1] >= fireballStartTime + (death ? 40 : 10))
                {
                    npc.ai[0] = phase3 ? 2f : 1f;
                    npc.ai[1] = 0f;
                    if (phase3)
                        calamityGlobalNPC.newAI[2] = Main.rand.Next(120); // Used as a random start point for phase 3 circling
                    npc.ai[2] = calamityGlobalNPC.newAI[2];
                    npc.localAI[0] = 0f;
                    // Needs to be done to properly put it in the top left corner afterwards in phase 2
                    calamityGlobalNPC.newAI[0] = -maxDistanceDiagonal;
                    npc.netUpdate = true;
                    npc.SyncExtraAI();
                }
            }

            // Lasers
            npc.ai[2] += 1f;

            int laserGateValue = (int)(PosDelay / positioningInc);
            if (canFireProjectiles && Main.netMode != NetmodeID.MultiplayerClient && ((npc.ai[2] - calamityGlobalNPC.newAI[2]) % (laserGateValue / 2) == 0f))
            {
                int numLasers = 2;
                for (int i = 0; i < numLasers; i++)
                {
                    Vector2 freeHeadProjSpawn = new Vector2(npc.Center.X, npc.Center.Y - 20f * npc.scale);
                    freeHeadProjSpawn.X += 14f * npc.scale * (i == 1).ToDirectionInt();

                    float freeHeadProjSpeed = 7f + (5f * (1f - golemLifeRatio));
                    Vector2 laserVelocity = Main.player[npc.target].Center - freeHeadProjSpawn;
                    laserVelocity = laserVelocity.SafeNormalize(Vector2.UnitY) * freeHeadProjSpeed;

                    int type = ProjectileID.EyeBeam;
                    int damage = LaserDamage;
                    int freeHeadLaser = Projectile.NewProjectile(npc.GetSource_FromAI(), freeHeadProjSpawn + laserVelocity.SafeNormalize(Vector2.UnitY) * 40f, laserVelocity, type, damage, 0f, Main.myPlayer);
                    Main.projectile[freeHeadLaser].timeLeft = enrage ? 600 : 300;
                    if (turboEnrage)
                        Main.projectile[freeHeadLaser].extraUpdates += 1;
                }
            }

            if (!Main.getGoodWorld)
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
