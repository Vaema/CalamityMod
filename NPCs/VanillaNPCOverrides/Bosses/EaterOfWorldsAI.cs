using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityMod.Projectiles.Boss;

namespace CalamityMod.NPCs.VanillaNPCOverrides.Bosses
{
    public static class EaterOfWorldsAI
    {
        public static bool BuffedEaterofWorldsAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Causes it to split far more in death mode
            if ((((npc.ai[2] % 2f == 0f && npc.type == NPCID.EaterofWorldsBody) || npc.type == NPCID.EaterofWorldsHead) && death) || CalamityWorld.LegendaryMode)
            {
                calamityGlobalNPC.DR = 0.5f;
                npc.defense = npc.defDefense * 2;
            }

            if (CalamityWorld.LegendaryMode && npc.type == NPCID.EaterofWorldsHead)
                npc.reflectsProjectiles = true;

            // Calculate contact damage based on velocity
            float minimalContactDamageVelocity = 4f;
            float minimalDamageVelocity = 8f;
            if (npc.velocity.Length() <= minimalContactDamageVelocity)
            {
                if (npc.type == NPCID.EaterofWorldsHead)
                    npc.damage = (int)(npc.defDamage * 0.5f);
                else
                    npc.damage = 0;
            }
            else
            {
                float velocityDamageScalar = MathHelper.Clamp((npc.velocity.Length() - minimalContactDamageVelocity) / minimalDamageVelocity, 0f, 1f);
                if (npc.type == NPCID.EaterofWorldsHead)
                    npc.damage = (int)MathHelper.Lerp(npc.defDamage * 0.5f, npc.defDamage, velocityDamageScalar);
                else
                    npc.damage = (int)MathHelper.Lerp(0f, npc.defDamage, velocityDamageScalar);
            }

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles && npc.type == NPCID.EaterofWorldsHead)
                npc.TargetClosest();

            // Fade in.
            npc.Opacity = MathHelper.Clamp(npc.Opacity + 0.08f, 0f, 1f);

            float enrageScale = bossRush ? 1.5f : 0f;
            if ((npc.position.Y / 16f) < Main.worldSurface || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 0.5f;
            }
            if (!Main.player[npc.target].ZoneCorrupt || bossRush)
            {
                npc.Calamity().CurrentlyEnraged = !bossRush;
                enrageScale += 2f;
            }

            // Total body segments
            float totalSegments = GetEaterOfWorldsSegmentsCountRevDeath();

            // Count body segments remaining
            float segmentCount = NPC.CountNPCS(NPCID.EaterofWorldsBody);

            // Percent body segments remaining
            float lifeRatio = segmentCount / totalSegments;

            // 10 seconds of resistance to prevent spawn killing
            if (calamityGlobalNPC.newAI[1] < 600f && bossRush)
                calamityGlobalNPC.newAI[1] += 1f;

            // Phases

            // Cursed Flame phase
            bool phase2 = lifeRatio < 0.8f;

            // Boost velocity by 20% phase
            bool phase3 = lifeRatio < 0.4f;

            // Boost velocity by 50% phase
            bool phase4 = lifeRatio < 0.2f;

            // Fire projectiles
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Vile spit
                if (npc.type == NPCID.EaterofWorldsBody)
                {
                    int randomChanceLimit = (int)MathHelper.Lerp(30f, 900f, lifeRatio);
                    if (Main.getGoodWorld)
                        randomChanceLimit = (int)(randomChanceLimit * 0.5f);

                    if (Main.rand.NextBool(randomChanceLimit))
                    {
                        npc.TargetClosest();
                        if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) && Vector2.Distance(npc.Center, Main.player[npc.target].Center) > 320f)
                            NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + (npc.width / 2) + npc.velocity.X), (int)(npc.position.Y + (npc.height / 2) + npc.velocity.Y), NPCID.VileSpitEaterOfWorlds, 0, 0f, 1f, 0f, 0f, 255);
                    }
                }

                // Cursed flames (shadowflames in death mode)
                else if (npc.type == NPCID.EaterofWorldsHead)
                {
                    if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                        calamityGlobalNPC.newAI[0] += 1f;

                    float timer = enrageScale > 0f ? 120f : 180f;
                    float shootBoost = (0.8f - lifeRatio) * 120f;
                    timer += shootBoost;

                    if (enrageScale >= 2f)
                        timer = 60f;

                    if (calamityGlobalNPC.newAI[0] >= timer && phase2)
                    {
                        if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) &&
                            npc.SafeDirectionTo(Main.player[npc.target].Center).AngleBetween((npc.rotation - MathHelper.PiOver2).ToRotationVector2()) < MathHelper.ToRadians(18f) &&
                            Vector2.Distance(npc.Center, Main.player[npc.target].Center) > 320f)
                        {
                            calamityGlobalNPC.newAI[0] = 0f;
                            Vector2 cursedFlameDirection = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
                            float targetXDirection = Main.player[npc.target].position.X + (Main.player[npc.target].width / 2) - cursedFlameDirection.X;
                            float targetYDirection = Main.player[npc.target].position.Y + (Main.player[npc.target].height / 2) - cursedFlameDirection.Y;
                            float projSpeed = 7f + enrageScale * 2f;
                            float flameTargetDistance = (float)Math.Sqrt(targetXDirection * targetXDirection + targetYDirection * targetYDirection);
                            flameTargetDistance = projSpeed / flameTargetDistance;
                            targetXDirection *= flameTargetDistance;
                            targetYDirection *= flameTargetDistance;
                            targetYDirection += npc.velocity.Y * 0.5f;
                            targetXDirection += npc.velocity.X * 0.5f;
                            cursedFlameDirection.X -= targetXDirection;
                            cursedFlameDirection.Y -= targetYDirection;

                            int type = (death && phase3) ? ModContent.ProjectileType<ShadowflameFireball>() : ProjectileID.CursedFlameHostile;
                            float homeIn = death ? (phase4 ? 2f : 0f) : 0f;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), cursedFlameDirection.X, cursedFlameDirection.Y, targetXDirection, targetYDirection, type, npc.GetProjectileDamage(type), 0f, Main.myPlayer, 0f, homeIn);
                        }
                    }
                }
            }

            // Despawn
            if (Main.player[npc.target].dead)
            {
                if (npc.timeLeft > 300)
                    npc.timeLeft = 300;
            }

            // All functions that modify the active worm segments are here. This includes spawning the worm originally and splitting effects.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // If this segment is a head or a body without a next-segment defined, then it needs to spawn its own next segment.
                if ((npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody) && npc.ai[0] == 0f)
                {
                    int spawnX = (int)npc.position.X;
                    int spawnY = (int)npc.position.Y;

                    // A head sets the length variable (npc.ai[2]) and then sets its next segment to a freshly spawned body.
                    if (npc.type == NPCID.EaterofWorldsHead)
                    {
                        // Set head's "length beyond this point" to be the total length of the worm.
                        npc.ai[2] = totalSegments;

                        // Body spawn
                        npc.ai[0] = NPC.NewNPC(npc.GetSource_FromAI(), spawnX, spawnY, NPCID.EaterofWorldsBody, npc.whoAmI);
                    }

                    // A body with a "length beyond this point" greater than zero just sets its next spawned segment to a freshly spawned body.
                    else if (npc.type == NPCID.EaterofWorldsBody && npc.ai[2] > 0f)
                        npc.ai[0] = NPC.NewNPC(npc.GetSource_FromAI(), spawnX, spawnY, NPCID.EaterofWorldsBody, npc.whoAmI);

                    // If the worm stops here ("length beyond this point" is zero), then spawn a tail instead.
                    else
                        npc.ai[0] = NPC.NewNPC(npc.GetSource_FromAI(), spawnX, spawnY, NPCID.EaterofWorldsTail, npc.whoAmI);

                    // Maintain the linked list of worm segments, and correctly set the "length beyond this point" of this segment.
                    Main.npc[(int)npc.ai[0]].ai[1] = npc.whoAmI;
                    Main.npc[(int)npc.ai[0]].ai[2] = npc.ai[2] - 1f;
                    npc.netUpdate = true;
                }

                // Helper function to destroy this Eater of Worlds worm segment.
                void DestroyThisSegment()
                {
                    npc.life = 0;
                    npc.HitEffect(0, 10.0);
                    npc.checkDead();
                }

                // If this segment's previous and next segments are both dead, make it explode instantly. Single segments cannot live.
                if (!Main.npc[(int)npc.ai[1]].active && !Main.npc[(int)npc.ai[0]].active)
                    DestroyThisSegment();

                // If this segment is a head and its next segment is dead, make it explode instantly. It's been decapitated.
                if (npc.type == NPCID.EaterofWorldsHead && !Main.npc[(int)npc.ai[0]].active)
                    DestroyThisSegment();

                // If this segment is a tail and its previous segment is dead, make it explode instantly. It's been chopped off.
                if (npc.type == NPCID.EaterofWorldsTail && !Main.npc[(int)npc.ai[1]].active)
                    DestroyThisSegment();

                // If this segment is a body and its previous segment is dead (or was rendered into a tail), transform into a head.
                if (npc.type == NPCID.EaterofWorldsBody && (!Main.npc[(int)npc.ai[1]].active || Main.npc[(int)npc.ai[1]].aiStyle != npc.aiStyle))
                {
                    npc.type = NPCID.EaterofWorldsHead;
                    float segmentLifeRatio = MathHelper.Lerp(0.5f, 1f, npc.life / (float)npc.lifeMax);
                    int whoAmI = npc.whoAmI;
                    float ai0Holdover = npc.ai[0];
                    float newAI1Holdover = calamityGlobalNPC.newAI[1];
                    int slowingDebuffResistTimer = calamityGlobalNPC.debuffResistanceTimer;

                    // Actually transform the body segment into a head segment.
                    npc.SetDefaultsKeepPlayerInteraction(npc.type);
                    npc.life = (int)(npc.lifeMax * segmentLifeRatio);
                    npc.whoAmI = whoAmI;
                    npc.ai[0] = ai0Holdover;
                    // Heads spawned mid fight by splitting do not get reset spawn invincibility.
                    CalamityGlobalNPC newCGN = npc.Calamity();
                    newCGN.newAI[1] = newAI1Holdover;
                    newCGN.debuffResistanceTimer = slowingDebuffResistTimer;
                    npc.TargetClosest();
                    npc.netUpdate = true;
                    npc.netSpam = 0;
                }

                // If this segment is a body and its next segment is dead (or was rendered into a head), transform into a tail.
                if (npc.type == NPCID.EaterofWorldsBody && (!Main.npc[(int)npc.ai[0]].active || Main.npc[(int)npc.ai[0]].aiStyle != npc.aiStyle))
                {
                    npc.type = NPCID.EaterofWorldsTail;
                    float segmentLifeRatio = MathHelper.Lerp(0.5f, 1f, npc.life / (float)npc.lifeMax);
                    int whoAmI = npc.whoAmI;
                    float ai1Holdover = npc.ai[1];
                    int slowingDebuffResistTimer = calamityGlobalNPC.debuffResistanceTimer;

                    // Actually transform the body segment into a tail segment.
                    npc.SetDefaultsKeepPlayerInteraction(npc.type);
                    npc.life = (int)(npc.lifeMax * segmentLifeRatio);
                    npc.whoAmI = whoAmI;
                    npc.ai[1] = ai1Holdover;
                    npc.Calamity().debuffResistanceTimer = slowingDebuffResistTimer;
                    npc.TargetClosest();
                    npc.netUpdate = true;
                    npc.netSpam = 0;
                }

                // If for any reason this segment was deleted, send info to clients so they also see it die.
                if (!npc.active && Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, npc.whoAmI, -1f);
            }

            // Movement
            int tilePositionX = (int)(npc.position.X / 16f) - 1;
            int tileWidthPosX = (int)((npc.position.X + npc.width) / 16f) + 2;
            int tilePositionY = (int)(npc.position.Y / 16f) - 1;
            int tileWidthPosY = (int)((npc.position.Y + npc.height) / 16f) + 2;
            if (tilePositionX < 0)
                tilePositionX = 0;
            if (tileWidthPosX > Main.maxTilesX)
                tileWidthPosX = Main.maxTilesX;
            if (tilePositionY < 0)
                tilePositionY = 0;
            if (tileWidthPosY > Main.maxTilesY)
                tileWidthPosY = Main.maxTilesY;

            // Fly or not
            bool inTiles = false;
            if (!inTiles)
            {
                for (int i = tilePositionX; i < tileWidthPosX; i++)
                {
                    for (int j = tilePositionY; j < tileWidthPosY; j++)
                    {
                        if (Main.tile[i, j] != null && ((Main.tile[i, j].HasUnactuatedTile && (Main.tileSolid[Main.tile[i, j].TileType] || (Main.tileSolidTop[Main.tile[i, j].TileType] && Main.tile[i, j].TileFrameY == 0))) || Main.tile[i, j].LiquidAmount > 64))
                        {
                            Vector2 vector;
                            vector.X = i * 16;
                            vector.Y = j * 16;
                            if (npc.position.X + npc.width > vector.X && npc.position.X < vector.X + 16f && npc.position.Y + npc.height > vector.Y && npc.position.Y < vector.Y + 16f)
                            {
                                inTiles = true;
                                if (Main.rand.NextBool(100) && Main.tile[i, j].HasUnactuatedTile)
                                {
                                    WorldGen.KillTile(i, j, true, true, false);
                                }
                            }
                        }
                    }
                }
            }

            if (!inTiles && npc.type == NPCID.EaterofWorldsHead)
            {
                Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                int noFlyZone = death ? 800 : 900;
                noFlyZone -= (int)(enrageScale * 300f);

                if (noFlyZone < 100)
                    noFlyZone = 100;

                bool freeMoveAnyway = true;
                for (int k = 0; k < Main.maxPlayers; k++)
                {
                    if (Main.player[k].active)
                    {
                        Rectangle rectangle2 = new Rectangle((int)Main.player[k].position.X - noFlyZone, (int)Main.player[k].position.Y - noFlyZone, noFlyZone * 2, noFlyZone * 2);
                        if (rectangle.Intersects(rectangle2))
                        {
                            freeMoveAnyway = false;
                            break;
                        }
                    }
                }
                if (freeMoveAnyway)
                    inTiles = true;
            }

            // Velocity and acceleration
            float velocityScale = (death ? 4.8f : 2.4f) * enrageScale;
            float velocityBoost = velocityScale * (1f - lifeRatio);
            float accelerationScale = (death ? 0.06f : 0.03f) * enrageScale;
            float accelerationBoost = accelerationScale * (1f - lifeRatio);
            float segmentVelocity = 12f + velocityBoost;
            float segmentAcceleration = 0.15f + accelerationBoost;

            if (phase4)
            {
                segmentVelocity += 2f * (enrageScale + 1f);
                segmentAcceleration += 0.1f * (enrageScale + 1f);
            }
            else if (phase3)
            {
                segmentVelocity += 0.8f * (enrageScale + 1f);
                segmentAcceleration += 0.04f * (enrageScale + 1f);
            }

            if (Main.getGoodWorld)
            {
                segmentVelocity += 4f;
                segmentAcceleration += 0.05f;
            }

            Vector2 segmentDirection = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
            float targetPosX = Main.player[npc.target].position.X + (Main.player[npc.target].width / 2);
            float targetPosY = Main.player[npc.target].position.Y + (Main.player[npc.target].height / 2);

            targetPosX = (int)(targetPosX / 16f) * 16;
            targetPosY = (int)(targetPosY / 16f) * 16;
            segmentDirection.X = (int)(segmentDirection.X / 16f) * 16;
            segmentDirection.Y = (int)(segmentDirection.Y / 16f) * 16;
            targetPosX -= segmentDirection.X;
            targetPosY -= segmentDirection.Y;
            float targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);

            // Does this worm segment have a "previous segment" defined?
            if (npc.ai[1] > 0f && npc.ai[1] < Main.npc.Length)
            {
                try
                {
                    segmentDirection = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
                    targetPosX = Main.npc[(int)npc.ai[1]].position.X + (Main.npc[(int)npc.ai[1]].width / 2) - segmentDirection.X;
                    targetPosY = Main.npc[(int)npc.ai[1]].position.Y + (Main.npc[(int)npc.ai[1]].height / 2) - segmentDirection.Y;
                }
                catch
                {
                }

                npc.rotation = (float)Math.Atan2(targetPosY, targetPosX) + MathHelper.PiOver2;
                targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);
                int npcWidth = npc.width;
                npcWidth = (int)(npcWidth * npc.scale);

                if (Main.getGoodWorld)
                    npcWidth = 62;

                targetDistance = (targetDistance - npcWidth) / targetDistance;
                targetPosX *= targetDistance;
                targetPosY *= targetDistance;
                npc.velocity = Vector2.Zero;
                npc.position.X += targetPosX;
                npc.position.Y += targetPosY;
            }

            // Otherwise this is a head. (Why does this not just check for head NPC type?)
            else
            {
                // Prevent new heads from being slowed when they spawn
                if (calamityGlobalNPC.newAI[1] < 3f)
                {
                    calamityGlobalNPC.newAI[1] += 1f;

                    // Set velocity for when a new head spawns
                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * (segmentVelocity * (death ? 0.75f : 0.5f));
                }

                if (!inTiles)
                {
                    npc.velocity.Y += death ? 0.1375f : 0.11f;
                    if (npc.velocity.Y > segmentVelocity)
                        npc.velocity.Y = segmentVelocity;

                    if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.4)
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X -= segmentAcceleration * 1.1f;
                        else
                            npc.velocity.X += segmentAcceleration * 1.1f;
                    }
                    else if (npc.velocity.Y == segmentVelocity)
                    {
                        if (npc.velocity.X < targetPosX)
                            npc.velocity.X += segmentAcceleration;
                        else if (npc.velocity.X > targetPosX)
                            npc.velocity.X -= segmentAcceleration;
                    }
                    else if (npc.velocity.Y > (death ? 5f : 4f))
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X += segmentAcceleration * 0.9f;
                        else
                            npc.velocity.X -= segmentAcceleration * 0.9f;
                    }
                }
                else
                {
                    // Sound
                    if (npc.soundDelay == 0)
                    {
                        float soundDelay = targetDistance / 40f;
                        if (soundDelay < 10f)
                            soundDelay = 10f;
                        if (soundDelay > 20f)
                            soundDelay = 20f;

                        npc.soundDelay = (int)soundDelay;
                        SoundEngine.PlaySound(SoundID.WormDig, npc.position);
                    }

                    targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);
                    float absoluteTargetX = Math.Abs(targetPosX);
                    float absoluteTargetY = Math.Abs(targetPosY);
                    float timeToReachTarget = segmentVelocity / targetDistance;
                    targetPosX *= timeToReachTarget;
                    targetPosY *= timeToReachTarget;

                    // Despawn
                    bool shouldDespawn = npc.type == NPCID.EaterofWorldsHead && Main.player[npc.target].dead;
                    if (shouldDespawn && !bossRush)
                    {
                        bool everyoneDead = true;
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Main.player[i].active && !Main.player[i].dead)
                            {
                                everyoneDead = false;
                                break;
                            }
                        }

                        if (everyoneDead)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient && (npc.position.Y / 16f) > (Main.rockLayer + Main.maxTilesY) / 2.0)
                            {
                                npc.active = false;
                                int segmentAmt = (int)npc.ai[0];

                                while (segmentAmt > 0 && segmentAmt < Main.maxNPCs && Main.npc[segmentAmt].active && Main.npc[segmentAmt].aiStyle == npc.aiStyle)
                                {
                                    int arg_2853_0 = (int)Main.npc[segmentAmt].ai[0];
                                    Main.npc[segmentAmt].active = false;
                                    npc.life = 0;

                                    if (Main.netMode == NetmodeID.Server)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, segmentAmt, 0f, 0f, 0f, 0, 0, 0);

                                    segmentAmt = arg_2853_0;
                                }

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                            }
                            targetPosX = 0f;
                            targetPosY = segmentVelocity;
                        }
                    }

                    if ((npc.velocity.X > 0f && targetPosX > 0f) || (npc.velocity.X < 0f && targetPosX < 0f) || (npc.velocity.Y > 0f && targetPosY > 0f) || (npc.velocity.Y < 0f && targetPosY < 0f))
                    {
                        if (npc.velocity.X < targetPosX)
                            npc.velocity.X += segmentAcceleration;
                        else if (npc.velocity.X > targetPosX)
                            npc.velocity.X -= segmentAcceleration;
                        if (npc.velocity.Y < targetPosY)
                            npc.velocity.Y += segmentAcceleration;
                        else if (npc.velocity.Y > targetPosY)
                            npc.velocity.Y -= segmentAcceleration;

                        if (Math.Abs(targetPosY) < segmentVelocity * 0.2 && ((npc.velocity.X > 0f && targetPosX < 0f) || (npc.velocity.X < 0f && targetPosX > 0f)))
                        {
                            if (npc.velocity.Y > 0f)
                                npc.velocity.Y += segmentAcceleration * 2f;
                            else
                                npc.velocity.Y -= segmentAcceleration * 2f;
                        }

                        if (Math.Abs(targetPosX) < segmentVelocity * 0.2 && ((npc.velocity.Y > 0f && targetPosY < 0f) || (npc.velocity.Y < 0f && targetPosY > 0f)))
                        {
                            if (npc.velocity.X > 0f)
                                npc.velocity.X += segmentAcceleration * 2f;
                            else
                                npc.velocity.X -= segmentAcceleration * 2f;
                        }
                    }
                    else if (absoluteTargetX > absoluteTargetY)
                    {
                        if (npc.velocity.X < targetPosX)
                            npc.velocity.X += segmentAcceleration * 1.1f;
                        else if (npc.velocity.X > targetPosX)
                            npc.velocity.X -= segmentAcceleration * 1.1f;

                        if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.5)
                        {
                            if (npc.velocity.Y > 0f)
                                npc.velocity.Y += segmentAcceleration;
                            else
                                npc.velocity.Y -= segmentAcceleration;
                        }
                    }
                    else
                    {
                        if (npc.velocity.Y < targetPosY)
                            npc.velocity.Y += segmentAcceleration * 1.1f;
                        else if (npc.velocity.Y > targetPosY)
                            npc.velocity.Y -= segmentAcceleration * 1.1f;

                        if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.5)
                        {
                            if (npc.velocity.X > 0f)
                                npc.velocity.X += segmentAcceleration;
                            else
                                npc.velocity.X -= segmentAcceleration;
                        }
                    }
                }

                if (death)
                {
                    int numHeads = NPC.CountNPCS(npc.type);
                    if (numHeads > 0)
                    {
                        // Limit this variable so that the following calculation never goes too low
                        numHeads--;
                        if (numHeads > 8)
                            numHeads = 8;

                        float pushDistanceLowerLimit = 16f - numHeads;
                        float pushDistanceUpperLimit = 160f - numHeads * 10f;
                        float pushDistance = MathHelper.Lerp(pushDistanceLowerLimit, pushDistanceUpperLimit, 1f - lifeRatio) * npc.scale;
                        float pushVelocity = 0.5f + enrageScale * 0.25f;
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (Main.npc[i].active)
                            {
                                if (i != npc.whoAmI && Main.npc[i].type == npc.type)
                                {
                                    if (Vector2.Distance(npc.Center, Main.npc[i].Center) < pushDistance)
                                    {
                                        if (npc.position.X < Main.npc[i].position.X)
                                            npc.velocity.X -= pushVelocity;
                                        else
                                            npc.velocity.X += pushVelocity;

                                        if (npc.position.Y < Main.npc[i].position.Y)
                                            npc.velocity.Y -= pushVelocity;
                                        else
                                            npc.velocity.Y += pushVelocity;
                                    }
                                }
                            }
                        }
                    }
                }

                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + MathHelper.PiOver2;

                if (npc.type == NPCID.EaterofWorldsHead)
                {
                    if (inTiles)
                    {
                        if (npc.localAI[0] != 1f)
                            npc.netUpdate = true;

                        npc.localAI[0] = 1f;
                    }
                    else
                    {
                        if (npc.localAI[0] != 0f)
                            npc.netUpdate = true;

                        npc.localAI[0] = 0f;
                    }
                    if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
                        npc.netUpdate = true;
                }
            }

            // Manually sync newAI because there is no GlobalNPC.SendExtraAI
            if (npc.active && npc.netUpdate && Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)CalamityModMessageType.SyncCalamityNPCAIArray);
                packet.Write((byte)npc.whoAmI);
                packet.Write(calamityGlobalNPC.newAI[0]);
                packet.Write(calamityGlobalNPC.newAI[1]);
                packet.Write(calamityGlobalNPC.newAI[2]);
                packet.Write(calamityGlobalNPC.newAI[3]);
                packet.Send(-1, -1);
            }
            return false;
        }

        public static int GetEaterOfWorldsSegmentsCountRevDeath()
        {
            return CalamityWorld.LegendaryMode ? 100 : (CalamityWorld.death || BossRushEvent.BossRushActive) ? 57 : 62;
        }
    }
}
