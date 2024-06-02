using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.SulphurousSea;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Sounds;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.CalamityAIs.CalamityBossAIs
{
    public static class AstrumDeusAI
    {
        public static void VanillaAstrumDeusAI(NPC npc, Mod mod, bool head)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Difficulty variables
            bool bossRush = BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || bossRush;
            bool revenge = CalamityWorld.revenge || bossRush;
            bool death = CalamityWorld.death || bossRush;

            float enrageScale = bossRush ? 0.5f : 0f;
            if (Main.IsItDay() || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 1.5f;
            }

            // Deus cannot hit for 3 seconds or while invulnerable
            bool doNotDealDamage = calamityGlobalNPC.newAI[1] < 180f || npc.dontTakeDamage;
            if (doNotDealDamage)
                npc.damage = 0;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            Player player = Main.player[npc.target];

            bool increaseSpeed = Vector2.Distance(player.Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles;
            bool increaseSpeedMore = Vector2.Distance(player.Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles;

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (increaseSpeedMore && head)
                npc.TargetClosest();

            // Inflict Extreme Gravity to nearby players
            if (revenge)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    if (!Main.player[Main.myPlayer].dead && Main.player[Main.myPlayer].active && Vector2.Distance(Main.player[Main.myPlayer].Center, npc.Center) < CalamityGlobalNPC.CatchUpDistance350Tiles)
                        Main.player[Main.myPlayer].AddBuff(ModContent.BuffType<DoGExtremeGravity>(), 2);
                }
            }

            // Life
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases based on life percentage
            bool halfHealth = lifeRatio < 0.5f;
            bool doubleWormPhase = calamityGlobalNPC.newAI[0] != 0f;
            bool startFlightPhase = lifeRatio < 0.8f || death || doubleWormPhase;
            bool phase2 = lifeRatio < 0.5f && doubleWormPhase && expertMode;
            bool phase3 = lifeRatio < 0.2f && doubleWormPhase && expertMode;
            bool splittingMines = lifeRatio < 0.7f;
            bool movingMines = lifeRatio < 0.3f && doubleWormPhase && expertMode;
            bool deathModeEnragePhase_Head = calamityGlobalNPC.newAI[0] == 3f;
            bool deathModeEnragePhase_BodyAndTail = false;

            // 5 seconds of resistance in phase 2, 10 seconds in phase 1, to prevent spawn killing
            float resistanceTime = doubleWormPhase ? 300f : 600f;
            if (calamityGlobalNPC.newAI[1] < resistanceTime)
                calamityGlobalNPC.newAI[1] += 1f;

            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = calamityGlobalNPC.newAI[1] < resistanceTime;

            // Flight timer
            float aiSwitchTimer = doubleWormPhase ? (Main.getGoodWorld ? 600f : 1200f) : (Main.getGoodWorld ? 900f : 1800f);

            calamityGlobalNPC.newAI[3] += 1f;
            if (calamityGlobalNPC.newAI[3] >= aiSwitchTimer)
                calamityGlobalNPC.newAI[3] = 0f;

            // Phase for flying at the player
            bool flyAtTarget = calamityGlobalNPC.newAI[3] >= (aiSwitchTimer * 0.5f) && startFlightPhase;

            // Length of worms
            int phase1Length = death ? 80 : revenge ? 70 : expertMode ? 60 : 50;
            int phase2Length = death ? 40 : revenge ? 35 : expertMode ? 30 : 25;
            int gfbLength = death ? 8 : revenge ? 7 : expertMode ? 6 : 5;
            int maxLength = Main.zenithWorld && doubleWormPhase ? gfbLength : doubleWormPhase ? phase2Length : phase1Length;

            // Become gradually more pissed as more worms are killed
            int gfbMaxWormCount = 10;
            int gfbWormCount = 0;
            if (CalamityWorld.LegendaryMode && revenge)
                gfbWormCount = NPC.CountNPCS(ModContent.NPCType<AstrumDeusHead>());
            if (gfbWormCount > gfbMaxWormCount)
                gfbWormCount = gfbMaxWormCount;
            if (gfbWormCount > 0)
                enrageScale += (gfbMaxWormCount - gfbWormCount) * 0.111f;

            // Split into two worms
            if (head)
            {
                float splitAnimationTime = 180f;
                bool oneWormAlive = NPC.CountNPCS(ModContent.NPCType<AstrumDeusHead>()) < 2;
                bool deathModeFinalWormEnrage = death && doubleWormPhase && oneWormAlive && calamityGlobalNPC.newAI[1] >= resistanceTime;
                if (deathModeFinalWormEnrage)
                {
                    if (calamityGlobalNPC.newAI[0] != 3f)
                    {
                        SoundEngine.PlaySound(AstrumDeusHead.SplitSound, player.Center);
                        calamityGlobalNPC.newAI[0] = 3f;
                        npc.defense = 12;
                        calamityGlobalNPC.DR = 0.075f;

                        // Despawns the other deus worm segments
                        int bodyID = ModContent.NPCType<AstrumDeusBody>();
                        int tailID = ModContent.NPCType<AstrumDeusTail>();
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC wormseg = Main.npc[i];
                            if (!wormseg.active)
                                continue;

                            if ((wormseg.type == bodyID || wormseg.type == tailID) && Main.npc[(int)wormseg.ai[2]].Calamity().newAI[0] != 3f)
                            {
                                wormseg.life = 0;
                                wormseg.active = false;
                            }
                        }
                    }

                    calamityGlobalNPC.newAI[2] += 10f;
                    if (calamityGlobalNPC.newAI[2] > 162f)
                        calamityGlobalNPC.newAI[2] = 162f;

                    npc.Opacity = MathHelper.Clamp(1f - (calamityGlobalNPC.newAI[2] / splitAnimationTime), 0f, 1f);
                }

                bool despawnRemainingWorm = doubleWormPhase && oneWormAlive && !death;
                if ((halfHealth && calamityGlobalNPC.newAI[0] == 0f) || despawnRemainingWorm)
                {
                    npc.dontTakeDamage = true;

                    calamityGlobalNPC.newAI[2] += despawnRemainingWorm ? 10f : 1f;
                    npc.Opacity = MathHelper.Clamp(1f - (calamityGlobalNPC.newAI[2] / splitAnimationTime), 0f, 1f);

                    bool despawning = calamityGlobalNPC.newAI[2] == splitAnimationTime;

                    //
                    // CODE TWEAKED BY: OZZATRON
                    // September 20th, 2020
                    // reason: fixing Astrum Deus death mode NPC cap bug
                    //

                    // Despawn the unsplit Astrum Deus and spawn two half-size Astrum Deus worms.
                    if (despawning)
                    {
                        // If this is already a split worm (newAI[0] != 0f) then don't do anything. At all.
                        if (doubleWormPhase)
                            return;

                        // Mark all existing body and tail segments as inactive. This instantly frees up their NPC slots for the freshly spawned worms.
                        int bodyID = ModContent.NPCType<AstrumDeusBody>();
                        int tailID = ModContent.NPCType<AstrumDeusTail>();
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC wormseg = Main.npc[i];
                            if (!wormseg.active)
                                continue;
                            if (wormseg.type == bodyID || wormseg.type == tailID)
                            {
                                wormseg.life = 0;
                                wormseg.active = false;
                            }
                        }

                        // The unsplit Astrum Deus worm head will die next frame.
                        npc.life = 0;

                        // Do not spawn worms client side. The server handles this.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int wormamt = Main.zenithWorld ? 5 : 1;
                            for (int i = 0; i < wormamt; i++)
                            {
                                // Now that the original worm doesn't exist, startCount can be zero.
                                // int startCount = npc.whoAmI + phase1Length + 1;
                                int startIndexHeadOne = 1;
                                int headOneID = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, npc.type, startIndexHeadOne);
                                Main.npc[headOneID].Calamity().newAI[0] = 1f;
                                Main.npc[headOneID].velocity = Vector2.Normalize(player.Center - Main.npc[headOneID].Center) * 16f;
                                Main.npc[headOneID].timeLeft *= 20;
                                Main.npc[headOneID].netSpam = 0;
                                Main.npc[headOneID].netUpdate = true;

                                // On server, immediately send the correct extra AI of this head to clients.
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    var netMessage = mod.GetPacket();
                                    netMessage.Write((byte)CalamityModMessageType.SyncCalamityNPCAIArray);
                                    netMessage.Write((byte)headOneID);
                                    netMessage.Write(Main.npc[headOneID].Calamity().newAI[0]);
                                    netMessage.Write(Main.npc[headOneID].Calamity().newAI[1]);
                                    netMessage.Write(Main.npc[headOneID].Calamity().newAI[2]);
                                    netMessage.Write(Main.npc[headOneID].Calamity().newAI[3]);
                                    netMessage.Send();
                                }

                                // Make sure the second split worm is also contiguous.
                                int startIndexHeadTwo = startIndexHeadOne + phase2Length + 1;
                                int headTwoID = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, npc.type, startIndexHeadTwo);
                                Main.npc[headTwoID].Calamity().newAI[0] = 2f;
                                Main.npc[headTwoID].Calamity().newAI[3] = Main.getGoodWorld ? 300f : 600f;
                                Main.npc[headTwoID].velocity = Vector2.Normalize(player.Center - Main.npc[headTwoID].Center) * 16f;
                                Main.npc[headTwoID].timeLeft *= 20;
                                Main.npc[headTwoID].netSpam = 0;
                                Main.npc[headTwoID].netUpdate = true;

                                // On server, immediately send the correct extra AI of this head to clients.
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    var netMessage = mod.GetPacket();
                                    netMessage.Write((byte)CalamityModMessageType.SyncCalamityNPCAIArray);
                                    netMessage.Write((byte)headTwoID);
                                    netMessage.Write(Main.npc[headTwoID].Calamity().newAI[0]);
                                    netMessage.Write(Main.npc[headTwoID].Calamity().newAI[1]);
                                    netMessage.Write(Main.npc[headTwoID].Calamity().newAI[2]);
                                    netMessage.Write(Main.npc[headTwoID].Calamity().newAI[3]);
                                    netMessage.Send();
                                }
                            }

                            SoundEngine.PlaySound(AstrumDeusHead.SplitSound, player.Center);
                        }
                        return;
                    }
                }

                if (Main.zenithWorld && calamityGlobalNPC.newAI[1] < 10f) // desync the deuses
                {
                    float pushForce = 0.25f;
                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        NPC otherDeus = Main.npc[k];
                        // Short circuits to make the loop as fast as possible
                        if (!otherDeus.active || k == npc.whoAmI)
                            continue;

                        // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                        bool sameProjType = otherDeus.type == npc.type;
                        float taxicabDist = Vector2.Distance(npc.Center, otherDeus.Center);
                        float distancegate = 320f;
                        if (sameProjType && taxicabDist < distancegate)
                        {
                            if (npc.position.X < otherDeus.position.X)
                                npc.velocity.X -= pushForce;
                            else
                                npc.velocity.X += pushForce;

                            if (npc.position.Y < otherDeus.position.Y)
                                npc.velocity.Y -= pushForce;
                            else
                                npc.velocity.Y += pushForce;
                        }
                    }
                }
            }

            // Copy dontTakeDamage and Opacity from head
            else
            {
                npc.dontTakeDamage = Main.npc[(int)npc.ai[2]].dontTakeDamage;
                npc.Opacity = Main.npc[(int)npc.ai[2]].Opacity;
                deathModeEnragePhase_BodyAndTail = Main.npc[(int)npc.ai[2]].Calamity().newAI[0] == 3f;

                if (deathModeEnragePhase_BodyAndTail)
                {
                    npc.defense = 25;
                    calamityGlobalNPC.DR = 0.15f;
                }
            }

            // Set worm variable
            if (npc.ai[2] > 0f)
                npc.realLife = (int)npc.ai[2];

            // Alpha effects
            if ((head || Main.npc[(int)npc.ai[1]].alpha < 128) && !npc.dontTakeDamage)
            {
                // Alpha changes
                npc.alpha -= 42;
                if (npc.alpha < 0)
                    npc.alpha = 0;
            }

            // Check if other segments are still alive, if not, die
            if (!head)
            {
                bool shouldDespawn = true;
                int headType = ModContent.NPCType<AstrumDeusHead>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].type != headType || !Main.npc[i].active)
                        continue;
                    shouldDespawn = false;
                    break;
                }
                if (shouldDespawn)
                {
                    if (Main.npc.IndexInRange((int)npc.ai[1]) && Main.npc[(int)npc.ai[1]].active && Main.npc[(int)npc.ai[1]].life > 0)
                        shouldDespawn = false;
                }
                if (shouldDespawn)
                {
                    npc.life = 0;
                    npc.HitEffect(0, 10.0);
                    npc.checkDead();
                    npc.active = false;

                    npc.netUpdate = true;

                    // Prevent netUpdate from being blocked by the spam counter.
                    if (npc.netSpam >= 10)
                        npc.netSpam = 9;
                }
            }

            // Direction
            if (npc.velocity.X < 0f)
                npc.spriteDirection = -1;
            else if (npc.velocity.X > 0f)
                npc.spriteDirection = 1;

            // Head code
            if (head)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (npc.ai[0] == 0f)
                    {
                        int Previous = npc.whoAmI;
                        int bodyType = ModContent.NPCType<AstrumDeusBody>();
                        int tailType = ModContent.NPCType<AstrumDeusTail>();
                        for (int segments = 0; segments < maxLength; segments++)
                        {
                            int lol;
                            if (segments >= 0 && segments < maxLength - 1)
                                lol = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, bodyType, npc.whoAmI);
                            else
                                lol = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, tailType, npc.whoAmI);

                            if (segments % 2 == 0)
                                Main.npc[lol].localAI[3] = 1f;

                            Main.npc[lol].realLife = npc.whoAmI;
                            Main.npc[lol].Calamity().newAI[0] = Main.npc[Previous].Calamity().newAI[0];
                            Main.npc[lol].Calamity().newAI[3] = Main.npc[Previous].Calamity().newAI[3];

                            if (Main.netMode == NetmodeID.Server)
                            {
                                var netMessage = mod.GetPacket();
                                netMessage.Write((byte)CalamityModMessageType.SyncCalamityNPCAIArray);
                                netMessage.Write((byte)lol);
                                netMessage.Write(Main.npc[lol].Calamity().newAI[0]);
                                netMessage.Write(Main.npc[lol].Calamity().newAI[1]);
                                netMessage.Write(Main.npc[lol].Calamity().newAI[2]);
                                netMessage.Write(Main.npc[lol].Calamity().newAI[3]);
                                netMessage.Send();
                            }

                            Main.npc[lol].ai[2] = npc.whoAmI;
                            Main.npc[lol].ai[1] = Previous;
                            Main.npc[Previous].ai[0] = lol;
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, lol);
                            Previous = lol;
                        }
                    }
                }

                if (npc.life > Main.npc[(int)npc.ai[0]].life)
                    npc.life = Main.npc[(int)npc.ai[0]].life;
            }
            else
            {
                if (npc.life > Main.npc[(int)npc.ai[1]].life)
                    npc.life = Main.npc[(int)npc.ai[1]].life;
            }

            float segmentVelocity = deathModeEnragePhase_Head ? 19f : death ? 17.5f : 16f;

            float segmentVelocityBoost = 5f * (1f - lifeRatio);
            segmentVelocity += segmentVelocityBoost;
            segmentVelocity += 4f * enrageScale;

            if (revenge)
            {
                float revMultiplier = 1.1f;
                segmentVelocity *= revMultiplier;
            }

            if (head)
            {
                int headTilePositionX = (int)(npc.position.X / 16f) - 1;
                int headTileWidthPosX = (int)((npc.position.X + npc.width) / 16f) + 2;
                int headTilePositionY = (int)(npc.position.Y / 16f) - 1;
                int headTileWidthPosY = (int)((npc.position.Y + npc.height) / 16f) + 2;

                if (headTilePositionX < 0)
                    headTilePositionX = 0;
                if (headTileWidthPosX > Main.maxTilesX)
                    headTileWidthPosX = Main.maxTilesX;
                if (headTilePositionY < 0)
                    headTilePositionY = 0;
                if (headTileWidthPosY > Main.maxTilesY)
                    headTileWidthPosY = Main.maxTilesY;

                // Fly or not
                bool shouldFly = flyAtTarget;
                if (!shouldFly)
                {
                    for (int k = headTilePositionX; k < headTileWidthPosX; k++)
                    {
                        for (int l = headTilePositionY; l < headTileWidthPosY; l++)
                        {
                            if (Main.tile[k, l] != null && ((Main.tile[k, l].HasUnactuatedTile && (Main.tileSolid[Main.tile[k, l].TileType] || (Main.tileSolidTop[Main.tile[k, l].TileType] && Main.tile[k, l].TileFrameY == 0))) || Main.tile[k, l].LiquidAmount > 64))
                            {
                                Vector2 vector2;
                                vector2.X = k * 16f;
                                vector2.Y = l * 16f;
                                if (npc.position.X + npc.width > vector2.X && npc.position.X < vector2.X + 16f && npc.position.Y + npc.height > vector2.Y && npc.position.Y < vector2.Y + 16f)
                                {
                                    shouldFly = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Start flying if target is not within a certain distance
                if (!shouldFly)
                {
                    npc.localAI[1] = 1f;

                    Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                    int noFlyZone = 200;
                    int heightReduction = death ? 130 : (int)(130f * (1f - lifeRatio));
                    int height = 400 - heightReduction;
                    bool outsideNoFlyZone = true;

                    if (npc.position.Y > player.position.Y)
                    {
                        for (int m = 0; m < Main.maxPlayers; m++)
                        {
                            if (Main.player[m].active)
                            {
                                Rectangle rectangle2 = new Rectangle((int)Main.player[m].position.X - noFlyZone, (int)Main.player[m].position.Y - noFlyZone, noFlyZone * 2, height);
                                if (rectangle.Intersects(rectangle2))
                                {
                                    outsideNoFlyZone = false;
                                    break;
                                }
                            }
                        }

                        if (outsideNoFlyZone)
                            shouldFly = true;
                    }
                }
                else
                    npc.localAI[1] = 0f;

                // Despawn
                if (player.dead)
                {
                    shouldFly = false;
                    float velocity = 2f;
                    npc.velocity.Y -= velocity;
                    if ((double)npc.position.Y < Main.topWorld + 16f)
                    {
                        segmentVelocity *= 2f;
                        npc.velocity.Y -= velocity;
                    }

                    int headType = ModContent.NPCType<AstrumDeusHead>();
                    int bodyType = ModContent.NPCType<AstrumDeusBody>();
                    int tailType = ModContent.NPCType<AstrumDeusBody>();
                    if ((double)npc.position.Y < Main.topWorld + 16f)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (Main.npc[i].type == headType || Main.npc[i].type == bodyType || Main.npc[i].type == tailType)
                            {
                                Main.npc[i].active = false;

                                Main.npc[i].netUpdate = true;

                                // Prevent netUpdate from being blocked by the spam counter.
                                if (Main.npc[i].netSpam >= 10)
                                    Main.npc[i].netSpam = 9;
                            }
                        }
                    }
                }

                // Speed and movement
                float speedBoost = death ? (0.1f * (1f - lifeRatio)) : (0.13f * (1f - lifeRatio));
                float turnSpeedBoost = death ? (0.18f * (1f - lifeRatio)) : (0.2f * (1f - lifeRatio));
                float speed = (deathModeEnragePhase_Head ? 0.2f : death ? 0.18f : 0.13f) + speedBoost;
                float turnSpeed = (deathModeEnragePhase_Head ? 0.27f : death ? 0.25f : 0.2f) + turnSpeedBoost;
                speed += 0.05f * enrageScale;
                turnSpeed += 0.08f * enrageScale;

                if (flyAtTarget)
                {
                    float speedMultiplier = deathModeEnragePhase_Head ? 1.25f : doubleWormPhase ? (phase3 ? 1.25f : phase2 ? 1.2f : 1.15f) : 1f;
                    speed *= speedMultiplier;
                }

                if (revenge)
                {
                    float revMultiplier = 1.1f;
                    speed *= revMultiplier;
                    turnSpeed *= revMultiplier;
                }

                speed *= increaseSpeedMore ? 2f : increaseSpeed ? 1.5f : 1f;
                turnSpeed *= increaseSpeedMore ? 2f : increaseSpeed ? 1.5f : 1f;

                if (Main.getGoodWorld)
                {
                    speed *= 1.15f;
                    turnSpeed *= 1.15f;
                }

                Vector2 deusCenter = npc.Center;
                float deusTargetX = player.Center.X;
                float deusTargetY = player.Center.Y;
                deusTargetX = (int)(deusTargetX / 16f) * 16;
                deusTargetY = (int)(deusTargetY / 16f) * 16;
                deusCenter.X = (int)(deusCenter.X / 16f) * 16;
                deusCenter.Y = (int)(deusCenter.Y / 16f) * 16;
                deusTargetX -= deusCenter.X;
                deusTargetY -= deusCenter.Y;

                if (!shouldFly)
                {
                    npc.velocity.Y += 0.15f;
                    if (npc.velocity.Y > segmentVelocity)
                        npc.velocity.Y = segmentVelocity;

                    // This bool exists to stop the strange wiggle behavior when worms are falling down
                    bool slowXVelocity = Math.Abs(npc.velocity.X) > speed;
                    if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.4)
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X -= speed * 1.1f;
                        else
                            npc.velocity.X += speed * 1.1f;
                    }
                    else if (npc.velocity.Y == segmentVelocity)
                    {
                        if (slowXVelocity)
                        {
                            if (npc.velocity.X < deusTargetX)
                                npc.velocity.X += speed;
                            else if (npc.velocity.X > deusTargetX)
                                npc.velocity.X -= speed;
                        }
                        else
                            npc.velocity.X = 0f;
                    }
                    else if (npc.velocity.Y > 4f)
                    {
                        if (slowXVelocity)
                        {
                            if (npc.velocity.X < 0f)
                                npc.velocity.X += speed * 0.9f;
                            else
                                npc.velocity.X -= speed * 0.9f;
                        }
                        else
                            npc.velocity.X = 0f;
                    }
                }
                else
                {
                    float deusTargetDist = (float)Math.Sqrt(deusTargetX * deusTargetX + deusTargetY * deusTargetY);
                    float deusAbsoluteTargetX = Math.Abs(deusTargetX);
                    float deusAbsoluteTargetY = Math.Abs(deusTargetY);
                    float deusTimeToReachTarget = segmentVelocity / deusTargetDist;
                    deusTargetX *= deusTimeToReachTarget;
                    deusTargetY *= deusTimeToReachTarget;

                    bool speedUpWhileFlying = false;
                    if (flyAtTarget)
                    {
                        if (((npc.velocity.X > 0f && deusTargetX < 0f) || (npc.velocity.X < 0f && deusTargetX > 0f) || (npc.velocity.Y > 0f && deusTargetY < 0f) || (npc.velocity.Y < 0f && deusTargetY > 0f)) && Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) > speed / 2f && deusTargetDist < 400f)
                        {
                            speedUpWhileFlying = true;

                            if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < segmentVelocity)
                                npc.velocity *= 1.1f;
                        }

                        if (npc.position.Y > player.position.Y)
                        {
                            speedUpWhileFlying = true;

                            if (Math.Abs(npc.velocity.X) < segmentVelocity / 2f)
                            {
                                if (npc.velocity.X == 0f)
                                    npc.velocity.X -= npc.direction;

                                npc.velocity.X *= 1.1f;
                            }
                            else if (npc.velocity.Y > -segmentVelocity)
                                npc.velocity.Y -= speed;
                        }
                    }

                    if (!speedUpWhileFlying)
                    {
                        if (!flyAtTarget)
                        {
                            if (((npc.velocity.X > 0f && deusTargetX > 0f) || (npc.velocity.X < 0f && deusTargetX < 0f)) && ((npc.velocity.Y > 0f && deusTargetY > 0f) || (npc.velocity.Y < 0f && deusTargetY < 0f)))
                            {
                                if (npc.velocity.X < deusTargetX)
                                    npc.velocity.X += turnSpeed;
                                else if (npc.velocity.X > deusTargetX)
                                    npc.velocity.X -= turnSpeed;

                                if (npc.velocity.Y < deusTargetY)
                                    npc.velocity.Y += turnSpeed;
                                else if (npc.velocity.Y > deusTargetY)
                                    npc.velocity.Y -= turnSpeed;
                            }
                        }

                        if ((npc.velocity.X > 0f && deusTargetX > 0f) || (npc.velocity.X < 0f && deusTargetX < 0f) || (npc.velocity.Y > 0f && deusTargetY > 0f) || (npc.velocity.Y < 0f && deusTargetY < 0f))
                        {
                            if (npc.velocity.X < deusTargetX)
                                npc.velocity.X += speed;
                            else if (npc.velocity.X > deusTargetX)
                                npc.velocity.X -= speed;

                            if (npc.velocity.Y < deusTargetY)
                                npc.velocity.Y += speed;
                            else if (npc.velocity.Y > deusTargetY)
                                npc.velocity.Y -= speed;

                            if (Math.Abs(deusTargetY) < segmentVelocity * 0.2 && ((npc.velocity.X > 0f && deusTargetX < 0f) || (npc.velocity.X < 0f && deusTargetX > 0f)))
                            {
                                if (npc.velocity.Y > 0f)
                                    npc.velocity.Y += speed * 2f;
                                else
                                    npc.velocity.Y -= speed * 2f;
                            }

                            if (Math.Abs(deusTargetX) < segmentVelocity * 0.2 && ((npc.velocity.Y > 0f && deusTargetY < 0f) || (npc.velocity.Y < 0f && deusTargetY > 0f)))
                            {
                                if (npc.velocity.X > 0f)
                                    npc.velocity.X += speed * 2f;
                                else
                                    npc.velocity.X -= speed * 2f;
                            }
                        }
                        else if (deusAbsoluteTargetX > deusAbsoluteTargetY)
                        {
                            if (npc.velocity.X < deusTargetX)
                                npc.velocity.X += speed * 1.1f;
                            else if (npc.velocity.X > deusTargetX)
                                npc.velocity.X -= speed * 1.1f;

                            if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.5)
                            {
                                if (npc.velocity.Y > 0f)
                                    npc.velocity.Y += speed;
                                else
                                    npc.velocity.Y -= speed;
                            }
                        }
                        else
                        {
                            if (npc.velocity.Y < deusTargetY)
                                npc.velocity.Y += speed * 1.1f;
                            else if (npc.velocity.Y > deusTargetY)
                                npc.velocity.Y -= speed * 1.1f;

                            if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.5)
                            {
                                if (npc.velocity.X > 0f)
                                    npc.velocity.X += speed;
                                else
                                    npc.velocity.X -= speed;
                            }
                        }
                    }
                }

                if (shouldFly)
                {
                    if (npc.localAI[0] != 1f)
                    {
                        npc.netUpdate = true;

                        // Prevent netUpdate from being blocked by the spam counter.
                        if (npc.netSpam >= 10)
                            npc.netSpam = 9;
                    }

                    npc.localAI[0] = 1f;
                }
                else
                {
                    if (npc.localAI[0] != 0f)
                    {
                        npc.netUpdate = true;

                        // Prevent netUpdate from being blocked by the spam counter.
                        if (npc.netSpam >= 10)
                            npc.netSpam = 9;
                    }

                    npc.localAI[0] = 0f;
                }

                if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
                {
                    npc.netUpdate = true;

                    // Prevent netUpdate from being blocked by the spam counter.
                    if (npc.netSpam >= 10)
                        npc.netSpam = 9;
                }

                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + MathHelper.PiOver2;
            }

            // Only the main worm body and tail use this code
            else
            {
                // Shoot lasers
                if (npc.type == ModContent.NPCType<AstrumDeusBody>())
                {
                    npc.localAI[0] += 1f;

                    int shootTime = (doubleWormPhase && expertMode) ? 2 : 1;
                    float shootProjectile = (doubleWormPhase && expertMode) ? 200 : 400;
                    float timer = npc.ai[0] + 15f;
                    float divisor = timer + shootProjectile;
                    bool shootGodRays = phase2 || deathModeEnragePhase_BodyAndTail;

                    if (!flyAtTarget || deathModeEnragePhase_BodyAndTail)
                    {
                        float laserDivisor = (phase2 && !deathModeEnragePhase_BodyAndTail) ? 4f : 2f;
                        if (npc.localAI[0] % divisor == 0f && npc.ai[0] % laserDivisor == 0f)
                        {
                            npc.TargetClosest();

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (deathModeEnragePhase_BodyAndTail)
                                {
                                    Vector2 velocity = Vector2.Zero;
                                    if (movingMines)
                                    {
                                        Vector2 randomMineMovement = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                                        randomMineMovement.Normalize();
                                        randomMineMovement *= Main.rand.Next(90, 121) * 0.01f;
                                        velocity = randomMineMovement;
                                    }

                                    int type = ModContent.ProjectileType<DeusMine>();
                                    int damage = npc.GetProjectileDamage(type);
                                    float split = (splittingMines && npc.ai[0] % 3f == 0f) ? 1f : 0f;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, type, damage, 0f, Main.myPlayer, split, 0f);
                                }
                            }

                            if (Vector2.Distance(player.Center, npc.Center) > 80f)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    float deusLaserSpeed = (death ? 16f : revenge ? 14f : 13f) + enrageScale * 4f;

                                    Vector2 deusLaserCenter = npc.Center;
                                    float deusLaserTargetX = player.Center.X - deusLaserCenter.X;
                                    float deusLaserTargetY = player.Center.Y - deusLaserCenter.Y;
                                    float deusLaserTargetDist = (float)Math.Sqrt(deusLaserTargetX * deusLaserTargetX + deusLaserTargetY * deusLaserTargetY);
                                    deusLaserTargetDist = deusLaserSpeed / deusLaserTargetDist;
                                    deusLaserTargetX *= deusLaserTargetDist;
                                    deusLaserTargetY *= deusLaserTargetDist;
                                    deusLaserCenter.X += deusLaserTargetX * 5f;
                                    deusLaserCenter.Y += deusLaserTargetY * 5f;

                                    Vector2 shootDirection = new Vector2(deusLaserTargetX, deusLaserTargetY).SafeNormalize(Vector2.UnitY);
                                    Vector2 laserVelocity = shootDirection * deusLaserSpeed;

                                    int type = shootGodRays ? ModContent.ProjectileType<AstralGodRay>() : ModContent.ProjectileType<AstralShot2>();
                                    int damage = npc.GetProjectileDamage(type);
                                    if (shootGodRays)
                                    {
                                        SoundEngine.PlaySound(AstrumDeusHead.GodRaySound, npc.Center);
                                        // Waving beams need to start offset so they cross each other neatly.
                                        float waveSideOffset = Main.rand.NextFloat(9f, 14f);
                                        Vector2 perp = shootDirection.RotatedBy(-MathHelper.PiOver2) * waveSideOffset;

                                        for (int i = -1; i <= 1; i += 2)
                                        {
                                            Vector2 laserStartPos = deusLaserCenter + i * perp + Main.rand.NextVector2CircularEdge(6f, 6f);
                                            Projectile godRay = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), laserStartPos, laserVelocity, type, damage, 0f, Main.myPlayer, player.Center.X, player.Center.Y);

                                            // Tell this Phased God Ray exactly which way it should be waving.
                                            godRay.localAI[1] = i * 0.5f;
                                        }
                                    }
                                    else
                                    {
                                        SoundEngine.PlaySound(AstrumDeusHead.LaserSound, npc.Center);
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), deusLaserCenter, laserVelocity, type, damage, 0f, Main.myPlayer, player.Center.X, player.Center.Y);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (npc.localAI[0] % divisor == 0f && npc.ai[0] % 2f == 0f)
                        {
                            Vector2 velocity = Vector2.Zero;
                            if (movingMines)
                            {
                                Vector2 randomMineMovement = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                                randomMineMovement.Normalize();
                                randomMineMovement *= Main.rand.Next(30, 121) * 0.01f;
                                velocity = randomMineMovement;
                            }

                            int type = ModContent.ProjectileType<DeusMine>();
                            int damage = npc.GetProjectileDamage(type);
                            float split = (splittingMines && npc.ai[0] % 3f == 0f) ? 1f : 0f;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, type, damage, 0f, Main.myPlayer, split, 0f);
                        }
                    }
                }

                // Follow the head
                Vector2 segmentCenter = npc.Center;
                float segmentTargetX = player.Center.X;
                float segmentTargetY = player.Center.Y;
                segmentTargetX = (int)(segmentTargetX / 16f) * 16;
                segmentTargetY = (int)(segmentTargetY / 16f) * 16;
                segmentCenter.X = (int)(segmentCenter.X / 16f) * 16;
                segmentCenter.Y = (int)(segmentCenter.Y / 16f) * 16;
                segmentTargetX -= segmentCenter.X;
                segmentTargetY -= segmentCenter.Y;

                if (npc.ai[1] > 0f && npc.ai[1] < Main.npc.Length)
                {
                    try
                    {
                        segmentCenter = npc.Center;
                        segmentTargetX = Main.npc[(int)npc.ai[1]].Center.X - segmentCenter.X;
                        segmentTargetY = Main.npc[(int)npc.ai[1]].Center.Y - segmentCenter.Y;
                    }
                    catch
                    {
                    }

                    npc.rotation = (float)Math.Atan2(segmentTargetY, segmentTargetX) + MathHelper.PiOver2;
                    float segmentTargetDist = (float)Math.Sqrt(segmentTargetX * segmentTargetX + segmentTargetY * segmentTargetY);
                    int segmentWidth = npc.width;
                    segmentTargetDist = (segmentTargetDist - segmentWidth) / segmentTargetDist;
                    segmentTargetX *= segmentTargetDist;
                    segmentTargetY *= segmentTargetDist;
                    npc.velocity = Vector2.Zero;
                    npc.position.X = npc.position.X + segmentTargetX;
                    npc.position.Y = npc.position.Y + segmentTargetY;

                    if (segmentTargetX < 0f)
                        npc.spriteDirection = -1;
                    else if (segmentTargetX > 0f)
                        npc.spriteDirection = 1;
                }
            }

            // Calculate contact damage based on velocity
            if (!doNotDealDamage)
            {
                float minimalContactDamageVelocity = segmentVelocity * 0.25f;
                float minimalDamageVelocity = segmentVelocity * 0.5f;
                if (head)
                {
                    if (npc.velocity.Length() <= minimalContactDamageVelocity)
                    {
                        npc.damage = (int)Math.Round(npc.defDamage * 0.5);
                    }
                    else
                    {
                        float velocityDamageScalar = MathHelper.Clamp((npc.velocity.Length() - minimalContactDamageVelocity) / minimalDamageVelocity, 0f, 1f);
                        npc.damage = (int)MathHelper.Lerp((float)Math.Round(npc.defDamage * 0.5), npc.defDamage, velocityDamageScalar);
                    }
                }
                else
                {
                    float bodyAndTailVelocity = (npc.position - npc.oldPosition).Length();
                    if (bodyAndTailVelocity <= minimalContactDamageVelocity)
                    {
                        npc.damage = 0;
                    }
                    else
                    {
                        float velocityDamageScalar = MathHelper.Clamp((bodyAndTailVelocity - minimalContactDamageVelocity) / minimalDamageVelocity, 0f, 1f);
                        npc.damage = (int)MathHelper.Lerp(0f, npc.defDamage, velocityDamageScalar);
                    }
                }
            }
        }
    }
}
