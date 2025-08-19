using System;
using CalamityMod.Events;
using CalamityMod.Packets;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class EaterOfWorldsAI
    {
        private const float ProjectileTelegraphDuration = 30f;
        private const int TotalDeathModeWorms = 4;
        public const float DRIncreaseTime = 600f;

        // Rev+ exclusive
        public static int FireballDamage = 12; // 48; Applies to both Cursed Flames and (Death) Shadowflame fireballs

        public static bool BuffedEaterofWorldsAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Causes it to split far more in death mode
            if ((((npc.ai[2] % 2f == 0f && npc.type == NPCID.EaterofWorldsBody) || npc.type == NPCID.EaterofWorldsHead) && death) || Main.getGoodWorld)
            {
                calamityGlobalNPC.DR = 0.5f;
                npc.defense = npc.defDefense * 2;
            }

            if (Main.getGoodWorld && npc.type == NPCID.EaterofWorldsHead)
                npc.reflectsProjectiles = true;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            // Total body segments
            float totalSegments = GetEaterOfWorldsSegmentsCountRevDeath();

            // Count body segments remaining
            float segmentCount = NPC.CountNPCS(NPCID.EaterofWorldsBody);

            // Percent body segments remaining
            float lifeRatio = MathHelper.Clamp(segmentCount / totalSegments, 0f, 1f);

            // Phases

            // Cursed Flame phase
            bool phase2 = lifeRatio < 0.8f || death;

            // Boost velocity by 20% phase
            bool phase3 = lifeRatio < 0.4f || death;

            // Boost velocity by 50% phase
            bool phase4 = lifeRatio < (death ? 0.5f : 0.2f);

            // Go fucking crazy in Death Mode
            bool phase5 = lifeRatio < 0.1f && death;
            bool phase6 = lifeRatio < 0.05f && death;

            // Fire projectiles
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Vile spit
                if (npc.type == NPCID.EaterofWorldsBody)
                {
                    if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                        npc.localAI[1] += 1f;
                    else
                        npc.localAI[1] -= 1f;

                    int vileSpitGateValue = (int)MathHelper.Lerp(death ? 45f : 90f, 900f, lifeRatio);
                    if (Main.getGoodWorld)
                        vileSpitGateValue = (int)(vileSpitGateValue * 0.5f);

                    Vector2 vileSpitShootLocation = npc.Center + npc.velocity;
                    if (npc.localAI[1] >= vileSpitGateValue)
                    {
                        CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                        
                        if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                            NPC.NewNPC(npc.GetSource_FromAI(), (int)vileSpitShootLocation.X, (int)vileSpitShootLocation.Y, NPCID.VileSpitEaterOfWorlds, 0, 0f, 1f);

                        npc.localAI[1] = 0f;
                    }

                    if (npc.localAI[1] > vileSpitGateValue - ProjectileTelegraphDuration)
                    {
                        Vector2 dustCenter = vileSpitShootLocation + Main.rand.NextVector2CircularEdge(5f, 5f);
                        Dust dust = Dust.NewDustDirect(dustCenter, 1, 1, DustID.CorruptGibs, npc.velocity.X * 0.1f, npc.velocity.Y * 0.1f, 80, default, 2f);
                        dust.noGravity = true;
                        dust.velocity *= 0.3f;
                    }
                }

                // Cursed flames (shadowflames in death mode)
                else if (npc.type == NPCID.EaterofWorldsHead)
                {
                    if (phase2)
                    {
                        float timer = 120f;
                        float shootBoost = lifeRatio * 90f;
                        timer += shootBoost;

                        float showTelegraphGateValue = timer - ProjectileTelegraphDuration;

                        if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                        {
                            if (npc.justHit && death && calamityGlobalNPC.newAI[0] < showTelegraphGateValue)
                            {
                                calamityGlobalNPC.newAI[0] += 10f;
                                if (calamityGlobalNPC.newAI[0] > showTelegraphGateValue)
                                    calamityGlobalNPC.newAI[0] = showTelegraphGateValue;
                            }
                            else
                                calamityGlobalNPC.newAI[0] += 1f;
                        }
                        else
                            calamityGlobalNPC.newAI[0] -= 1f;

                        if (calamityGlobalNPC.newAI[0] >= timer)
                        {
                            if (Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) &&
                                (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY).ToRotation().AngleTowards(npc.velocity.ToRotation(), MathHelper.PiOver4) == npc.velocity.ToRotation())
                            {
                                calamityGlobalNPC.newAI[0] = 0f;
                                Vector2 cursedFlameDirection = Utils.DirectionTo(npc.Center, Main.player[npc.target].Center) * 7f + (npc.velocity * 0.5f);
                                int type = (death && phase3) ? ModContent.ProjectileType<ShadowflameFireball>() : ProjectileID.CursedFlameHostile;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + npc.velocity, cursedFlameDirection, type, FireballDamage, 0f, Main.myPlayer);
                            }
                        }

                        if (calamityGlobalNPC.newAI[0] > showTelegraphGateValue)
                        {
                            Vector2 dustCenter = npc.Center + Main.rand.NextVector2CircularEdge(10f, 10f);
                            int dustType = (death && phase3) ? DustID.Shadowflame : DustID.CursedTorch;
                            Dust dust = Dust.NewDustDirect(dustCenter, 1, 1, dustType, 0f, 0f, 0, default, 3f);
                            dust.noGravity = true;
                            dust.velocity *= 0f;
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
                        // Amount of segments to spawn.
                        int segmentSpawnAmount = (int)(death ? (totalSegments / TotalDeathModeWorms) : totalSegments);

                        // Spawn additional worms of reduced length in Master Mode.
                        if (death)
                        {
                            Vector2 additionalWormSpawnLocation = new Vector2(spawnX, spawnY);
                            int randomXLimit = 80;
                            int randomYLimit = 80;
                            for (int i = 1; i < TotalDeathModeWorms; i++)
                            {
                                additionalWormSpawnLocation += new Vector2((Main.rand.Next(randomXLimit + 1) + randomXLimit) * (Main.rand.NextBool() ? -1f : 1f), Main.rand.Next(randomYLimit + 1) + randomYLimit);
                                int wormHead = NPC.NewNPC(npc.GetSource_FromAI(), (int)additionalWormSpawnLocation.X, (int)additionalWormSpawnLocation.Y, NPCID.EaterofWorldsHead, npc.whoAmI + segmentSpawnAmount * i + 1);
                                Main.npc[wormHead].ai[2] = segmentSpawnAmount;
                                Main.npc[wormHead].ai[0] = NPC.NewNPC(Main.npc[wormHead].GetSource_FromAI(), (int)additionalWormSpawnLocation.X, (int)additionalWormSpawnLocation.Y, NPCID.EaterofWorldsBody, Main.npc[wormHead].whoAmI);
                                Main.npc[(int)Main.npc[wormHead].ai[0]].ai[1] = Main.npc[wormHead].whoAmI;
                                Main.npc[(int)Main.npc[wormHead].ai[0]].ai[2] = Main.npc[wormHead].ai[2] - 1f;
                                Main.npc[wormHead].netUpdate = true;
                            }
                        }

                        // Set head's "length beyond this point" to be the total length of the worm.
                        npc.ai[2] = segmentSpawnAmount;

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

                    CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                    
                    npc.ForceNetUpdate();
                    npc.alpha = 0;
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

                    CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                    
                    npc.ForceNetUpdate();
                    npc.alpha = 0;
                }

                // If for any reason this segment was deleted, send info to clients so they also see it die.
                if (!npc.active && Main.dedServ)
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
                                    WorldGen.KillTile(i, j, true, true, false);
                            }
                        }
                    }
                }
            }

            if (!inTiles && npc.type == NPCID.EaterofWorldsHead)
            {
                Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                int noFlyZone = death ? (phase5 ? 400 : 600) : 900;

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
            float velocityScale = death ? 4.8f : 2.4f;
            float velocityBoost = velocityScale * (1f - lifeRatio);
            float accelerationScale = death ? 0.06f : 0.03f;
            float accelerationBoost = accelerationScale * (1f - lifeRatio);
            float segmentVelocity = 12f + velocityBoost;
            float segmentAcceleration = 0.15f + accelerationBoost;

            if (phase6)
            {
                segmentVelocity += 2.4f;
                segmentAcceleration += 0.12f;
            }
            else if (phase5)
            {
                segmentVelocity += 1.8f;
                segmentAcceleration += 0.09f;
            }
            else if (phase4)
            {
                segmentVelocity += 1.2f;
                segmentAcceleration += 0.06f;
            }
            else if (phase3)
            {
                segmentVelocity += 0.6f;
                segmentAcceleration += 0.03f;
            }

            if (death)
            {
                segmentVelocity += (npc.justHit ? 8f : 2f);
                segmentAcceleration += (npc.justHit ? 0.16f : 0.04f);
            }

            if (Main.getGoodWorld)
            {
                segmentVelocity += 4f;
                segmentAcceleration += 0.05f;
            }

            Vector2 segmentDirection = npc.Center;
            Vector2 destination = Main.player[npc.target].Center + (phase6 ? Main.player[npc.target].velocity * 20f : Vector2.Zero);
            float targetPosX = destination.X;
            float targetPosY = destination.Y;

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
                    segmentDirection = npc.Center;
                    targetPosX = Main.npc[(int)npc.ai[1]].Center.X - segmentDirection.X;
                    targetPosY = Main.npc[(int)npc.ai[1]].Center.Y - segmentDirection.Y;
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
                if (calamityGlobalNPC.newAI[2] < 3f)
                {
                    calamityGlobalNPC.newAI[2] += 1f;

                    // Set velocity for when a new head spawns
                    // Only set this if the head is far enough away from the player, to avoid unfair hits
                    if (npc.Distance(Main.player[npc.target].Center) > segmentVelocity * 20f)
                        npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * (segmentVelocity * (death ? 0.75f : 0.5f));
                }

                if (!inTiles)
                {
                    npc.velocity.Y += death ? 0.1375f : 0.11f;
                    if (death && npc.velocity.Y > 0f)
                        npc.velocity.Y += 0.07f;

                    if (npc.velocity.Y > segmentVelocity)
                        npc.velocity.Y = segmentVelocity;

                    // This bool exists to stop the strange wiggle behavior when worms are falling down
                    bool slowXVelocity = Math.Abs(npc.velocity.X) > segmentAcceleration;
                    if ((Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) < segmentVelocity * 0.4)
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X -= segmentAcceleration * 1.1f;
                        else
                            npc.velocity.X += segmentAcceleration * 1.1f;
                    }
                    else if (npc.velocity.Y == segmentVelocity)
                    {
                        if (slowXVelocity)
                        {
                            if (npc.velocity.X < targetPosX)
                                npc.velocity.X += segmentAcceleration;
                            else if (npc.velocity.X > targetPosX)
                                npc.velocity.X -= segmentAcceleration;
                        }
                        else
                            npc.velocity.X = 0f;
                    }
                    else if (npc.velocity.Y > (death ? 5f : 4f))
                    {
                        if (slowXVelocity)
                        {
                            if (npc.velocity.X < 0f)
                                npc.velocity.X += segmentAcceleration * 0.9f;
                            else
                                npc.velocity.X -= segmentAcceleration * 0.9f;
                        }
                        else
                            npc.velocity.X = 0f;
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
                        SoundEngine.PlaySound(SoundID.WormDig, npc.Center);
                    }

                    targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);
                    float absoluteTargetX = Math.Abs(targetPosX);
                    float absoluteTargetY = Math.Abs(targetPosY);
                    float timeToReachTarget = segmentVelocity / targetDistance;
                    targetPosX *= timeToReachTarget;
                    targetPosY *= timeToReachTarget;

                    // Despawn
                    bool shouldDespawn = npc.type == NPCID.EaterofWorldsHead && (Main.player[npc.target].dead || !Main.player[npc.target].ZoneCorrupt || !Main.player[npc.target].ZoneCrimson) && !BossRushEvent.BossRushActive;
                    if (shouldDespawn)
                    {
                        bool everyoneDead = true;
                        foreach (Player p in Main.ActivePlayers)
                        {
                            if (!p.dead && p.ZoneCorrupt)
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
                                    int attachedSegments = (int)Main.npc[segmentAmt].ai[0];
                                    Main.npc[segmentAmt].active = false;
                                    npc.life = 0;

                                    if (Main.dedServ)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, segmentAmt);

                                    segmentAmt = attachedSegments;
                                }

                                if (Main.dedServ)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
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
                        if (numHeads > 7)
                            numHeads = 7;

                        float pushDistanceLowerLimit = 14f - numHeads;
                        float pushDistanceUpperLimit = 140f - numHeads * 10f;
                        float pushDistance = MathHelper.Lerp(pushDistanceLowerLimit, pushDistanceUpperLimit, 1f - lifeRatio) * npc.scale;
                        float pushVelocity = 0.25f;
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

            // 10 seconds of resistance to prevent spawn killing
            if (calamityGlobalNPC.newAI[1] < DRIncreaseTime && ((npc.position - npc.oldPosition).Length() > 2f || calamityGlobalNPC.newAI[1] > 0f))
                calamityGlobalNPC.newAI[1] += 1f;

            if (npc.type == NPCID.EaterofWorldsHead || (npc.type != NPCID.EaterofWorldsHead && Main.npc[(int)npc.ai[1]].alpha >= 85))
            {
                if (npc.alpha > 0 && npc.life > 0)
                {
                    for (int dustIndex = 0; dustIndex < 2; dustIndex++)
                    {
                        int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Demonite, 0f, 0f, 100, default, 2f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                    }
                }

                if ((npc.position - npc.oldPosition).Length() > 2f)
                {
                    npc.alpha -= 42;
                    if (npc.alpha < 0)
                        npc.alpha = 0;
                }
            }
            else if (npc.type > NPCID.EaterofWorldsHead && npc.alpha > 0)
            {
                npc.alpha -= 42;
                if (npc.alpha < 0)
                    npc.alpha = 0;
            }

            // Manually sync newAI because there is no GlobalNPC.SendExtraAI
            if (npc.active && npc.netUpdate && Main.dedServ)
            {
                SyncCalamityNPCAIArrayPacket.Send(npc);
            }

            return false;
        }

        public static int GetEaterOfWorldsSegmentsCountRevDeath()
        {
            return Main.getGoodWorld ? 100 : (CalamityWorld.death || BossRushEvent.BossRushActive) ? 57 : 62;
        }
    }
}
