using System;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCOverrides.Bosses
{
    public static class SkeletronAI
    {
        public const float HandSlapGateValue = 300f;
        public const float HandSlapTelegraphTime = 120f;
        public const float HandSwipeDistance = 960f; // 60 tiles

        public static bool BuffedSkeletronAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases

            // Begin firing spreads of skulls phase
            bool phase2 = lifeRatio < 0.85f;

            // Begin using a more dangerous charge attack, similar to Dreadnautilus, rather than the regular spin charge and fire a spread of Chaos Balls at the end of it phase
            bool phase3 = lifeRatio < 0.7f;

            // Spawn a new set of hands and fire skulls from hands at the end of each slap phase
            bool respawnHands = lifeRatio < 0.5f;

            // Fire giant cursed skull projectiles during charge attack and spin away while firing skull projectiles before charging phase (yes, these curse you if you get hit)
            bool phase4 = lifeRatio < 0.3f;

            // Rapid teleport and charge, stop using idle phase
            bool phase5 = lifeRatio < 0.1f;

            // Set defense
            npc.defense = npc.defDefense;

            npc.reflectsProjectiles = false;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            // Spawn hands
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (npc.ai[0] == 0f)
                {
                    npc.ai[0] = 1f;
                    SpawnHands();
                    npc.netUpdate = true;
                }

                // Respawn hands
                if (respawnHands && calamityGlobalNPC.newAI[0] == 0f && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f)
                {
                    calamityGlobalNPC.newAI[0] = 1f;
                    SoundEngine.PlaySound(SoundID.ForceRoar with { Pitch = SoundID.ForceRoar.Pitch - 0.25f }, npc.Center);
                    SpawnHands();

                    npc.netUpdate = true;
                    npc.SyncExtraAI();
                }

                void SpawnHands()
                {
                    int skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                    Main.npc[skeletronHand].ai[0] = -1f;
                    Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                    Main.npc[skeletronHand].target = npc.target;
                    Main.npc[skeletronHand].netUpdate = true;

                    skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                    Main.npc[skeletronHand].ai[0] = 1f;
                    Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                    Main.npc[skeletronHand].ai[3] = 150f;
                    Main.npc[skeletronHand].target = npc.target;
                    Main.npc[skeletronHand].netUpdate = true;

                    // Spawn two additional hands with different attack timings
                    if (death)
                    {
                        skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                        Main.npc[skeletronHand].ai[0] = -1f;
                        Main.npc[skeletronHand].Calamity().newAI[0] = -1f;
                        Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                        Main.npc[skeletronHand].ai[3] = respawnHands ? -75f : 0f;
                        Main.npc[skeletronHand].target = npc.target;
                        Main.npc[skeletronHand].netUpdate = true;

                        skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                        Main.npc[skeletronHand].ai[0] = 1f;
                        Main.npc[skeletronHand].Calamity().newAI[0] = -1f;
                        Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                        Main.npc[skeletronHand].ai[3] = respawnHands ? 75f : 150f;
                        Main.npc[skeletronHand].target = npc.target;
                        Main.npc[skeletronHand].netUpdate = true;
                    }
                }
            }

            // Distance from target
            float distance = Vector2.Distance(Main.player[npc.target].Center, npc.Center);

            // Despawn
            if (Main.player[npc.target].dead || distance > (bossRush ? CalamityGlobalNPC.CatchUpDistance350Tiles : CalamityGlobalNPC.CatchUpDistance200Tiles))
            {
                npc.TargetClosest();
                if (Main.player[npc.target].dead || distance > (bossRush ? CalamityGlobalNPC.CatchUpDistance350Tiles : CalamityGlobalNPC.CatchUpDistance200Tiles))
                    npc.ai[1] = 3f;
            }

            // Daytime enrage
            if (Main.dayTime && !bossRush && npc.ai[1] != 3f && npc.ai[1] != 2f)
            {
                npc.ai[1] = 2f;
                SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
            }

            // Hand count
            int numHandsAlive = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPCID.SkeletronHand)
                    numHandsAlive++;
            }

            // Hand variables
            bool handsDead = numHandsAlive == 0;
            int numProj = Main.getGoodWorld ? 22 : death ? 5 : 3;
            float spread = MathHelper.ToRadians(Main.getGoodWorld ? 180 : 60);
            float headSpinVelocityMult = bossRush ? 9f : 4.5f;

            switch (numHandsAlive)
            {
                case 0:
                    numProj = Main.getGoodWorld ? 36 : death ? 12 : 8;
                    spread = MathHelper.ToRadians(Main.getGoodWorld ? 180 : death ? 90 : 82);
                    headSpinVelocityMult = bossRush ? 12f : 6f;
                    break;

                case 1:
                    numProj = Main.getGoodWorld ? 27 : death ? 9 : 6;
                    spread = MathHelper.ToRadians(Main.getGoodWorld ? 150 : death ? 76 : 68);
                    headSpinVelocityMult = bossRush ? 10f : 5f;
                    break;

                case 2:
                    numProj = Main.getGoodWorld ? 18 : death ? 6 : 4;
                    spread = MathHelper.ToRadians(Main.getGoodWorld ? 140 : death ? 70 : 62);
                    headSpinVelocityMult = bossRush ? 9f : 4.5f;
                    break;

                case 3:
                    numProj = Main.getGoodWorld ? 15 : death ? 5 : 3;
                    spread = MathHelper.ToRadians(Main.getGoodWorld ? 130 : death ? 64 : 56);
                    headSpinVelocityMult = bossRush ? 8f : 4f;
                    break;

                case 4:
                    numProj = Main.getGoodWorld ? 12 : death ? 4 : 3;
                    spread = MathHelper.ToRadians(Main.getGoodWorld ? 120 : 56);
                    headSpinVelocityMult = bossRush ? 7f : 3.5f;
                    break;
            }

            if (death)
                headSpinVelocityMult *= 1.25f;

            // Hand DR, scale DR up if the hands are still alive as Skeletron's HP lowers
            npc.chaseable = handsDead;
            float minDR = 0.05f;
            float maxDR = 0.9999f;
            calamityGlobalNPC.DR = !handsDead ? (float)Math.Sqrt(MathHelper.Lerp(minDR, maxDR, respawnHands ? (0.5f - lifeRatio) / 0.5f : 2f - lifeRatio / 0.5f)) : minDR;
            calamityGlobalNPC.unbreakableDR = !handsDead;
            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = !handsDead;

            // Teleport while not despawning
            if (npc.ai[1] != 3f)
            {
                int dustType = 181;

                // Post-teleport
                if (npc.ai[3] == -60f)
                {
                    npc.ai[3] = 0f;

                    SoundEngine.PlaySound(SoundID.Item66, npc.Center);

                    // Fire skulls after teleport
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float skullSpeed = death ? 7.2f : 6f;
                        int type = ProjectileID.Skull;
                        int damage = npc.GetProjectileDamage(type);

                        float boltTargetXDist = Main.player[npc.target].Center.X - npc.Center.X;
                        float boltTargetYDist = Main.player[npc.target].Center.Y - npc.Center.Y;
                        float boltTargetDistance = (float)Math.Sqrt(boltTargetXDist * boltTargetXDist + boltTargetYDist * boltTargetYDist);

                        boltTargetDistance = skullSpeed / boltTargetDistance;
                        boltTargetXDist *= boltTargetDistance;
                        boltTargetYDist *= boltTargetDistance;
                        Vector2 center = npc.Center;
                        center.X += boltTargetXDist * 5f;
                        center.Y += boltTargetYDist * 5f;

                        float baseSpeed = (float)Math.Sqrt(boltTargetXDist * boltTargetXDist + boltTargetYDist * boltTargetYDist);
                        double startAngle = Math.Atan2(boltTargetXDist, boltTargetYDist) - spread / 2;
                        double deltaAngle = spread / numProj;
                        double offsetAngle;

                        for (int i = 0; i < numProj; i++)
                        {
                            offsetAngle = startAngle + deltaAngle * i;
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), center.X, center.Y, baseSpeed * (float)Math.Sin(offsetAngle), baseSpeed * (float)Math.Cos(offsetAngle), type, damage, 0f, Main.myPlayer, -2f);
                            Main.projectile[proj].timeLeft = 300;
                        }

                        npc.netUpdate = true;
                    }

                    // Teleport dust
                    for (int m = 0; m < 30; m++)
                    {
                        int teleportDust = Dust.NewDust(npc.position, npc.width, npc.height, dustType, 0f, 0f, 100, default, 3f);
                        Main.dust[teleportDust].noGravity = true;
                        Main.dust[teleportDust].velocity.X *= 2f;
                    }
                }

                // Teleport after a certain time
                // If hands are dead: 7 seconds
                // If hands are not dead: 14 seconds
                // If hands are dead in phase 2: 4.7 seconds
                npc.ai[3] += 1f + (((phase2 && handsDead) || bossRush || phase4) ? 0.5f : 0f) - ((handsDead || bossRush) ? 0f : 0.5f);

                // Dust to show teleport
                int ai3 = (int)npc.ai[3]; // 0 to 30, and -60
                bool emitDust = false;

                int teleportGateValue = phase5 ? 180 : 300;
                if (ai3 >= teleportGateValue && calamityGlobalNPC.newAI[2] == 0f && calamityGlobalNPC.newAI[3] == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 skullFaceDirection = npc.Center + new Vector2(npc.direction * 20, 6f);
                        Vector2 skullTargetDirection = Main.player[npc.target].Center - skullFaceDirection;
                        Point skullTileCoords = npc.Center.ToTileCoordinates();
                        Point targetTileCoords = Main.player[npc.target].Center.ToTileCoordinates();
                        int randomTeleportOffset = 32 - (int)Math.Ceiling(MathHelper.Lerp(0f, 16f, 1f - lifeRatio));
                        int skullPositionOffset = 4;
                        int targetPositionOffset = randomTeleportOffset - 4;
                        int teleportTries = 0;

                        bool targetTooFar = false;
                        if (skullTargetDirection.Length() > 2000f)
                            targetTooFar = true;

                        while (!targetTooFar && teleportTries < 100)
                        {
                            teleportTries++;
                            int teleportTileX = Main.rand.Next(targetTileCoords.X - randomTeleportOffset, targetTileCoords.X + randomTeleportOffset + 1);
                            int teleportTileY = Main.rand.Next(targetTileCoords.Y - randomTeleportOffset, targetTileCoords.Y + randomTeleportOffset + 1);
                            if ((teleportTileY < targetTileCoords.Y - targetPositionOffset || teleportTileY > targetTileCoords.Y + targetPositionOffset || teleportTileX < targetTileCoords.X - targetPositionOffset || teleportTileX > targetTileCoords.X + targetPositionOffset) && (teleportTileY < skullTileCoords.Y - skullPositionOffset || teleportTileY > skullTileCoords.Y + skullPositionOffset || teleportTileX < skullTileCoords.X - skullPositionOffset || teleportTileX > skullTileCoords.X + skullPositionOffset) && !Main.tile[teleportTileX, teleportTileY].HasUnactuatedTile)
                            {
                                // New location params
                                calamityGlobalNPC.newAI[2] = teleportTileX * 16 - npc.width / 2;
                                calamityGlobalNPC.newAI[3] = teleportTileY * 16 - npc.height;
                                npc.SyncExtraAI();
                                break;
                            }
                        }
                    }
                }

                if (calamityGlobalNPC.newAI[2] != 0f && calamityGlobalNPC.newAI[3] != 0f)
                {
                    for (int m = 0; m < 5; m++)
                    {
                        Vector2 position = new Vector2(calamityGlobalNPC.newAI[2], calamityGlobalNPC.newAI[3]);
                        int teleportDust = Dust.NewDust(position, npc.width, npc.height, dustType, 0f, 0f, 100, default, 2f);
                        Main.dust[teleportDust].noGravity = true;
                    }
                }

                if (ai3 >= teleportGateValue + 90)
                {
                    emitDust = true;
                }
                else if (ai3 >= teleportGateValue + 30)
                {
                    if (Main.rand.Next(teleportGateValue + 10, ai3 + 1) >= teleportGateValue + 25)
                        emitDust = true;
                }

                if (emitDust)
                {
                    int dust = Dust.NewDust(npc.position, npc.width, npc.height, dustType, 0f, 0f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                }

                // Teleport
                if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[3] >= teleportGateValue + 120)
                {
                    // Teleport dust
                    for (int m = 0; m < 30; m++)
                    {
                        int teleportDust = Dust.NewDust(npc.position, npc.width, npc.height, dustType, 0f, 0f, 100, default, 3f);
                        Main.dust[teleportDust].noGravity = true;
                        Main.dust[teleportDust].velocity.X *= 2f;
                    }

                    // New location
                    npc.Center = new Vector2(calamityGlobalNPC.newAI[2], calamityGlobalNPC.newAI[3]);
                    npc.velocity = Vector2.Zero;
                    npc.ai[3] = -60f;
                    calamityGlobalNPC.newAI[2] = calamityGlobalNPC.newAI[3] = 0f;
                    npc.SyncExtraAI();
                    npc.netUpdate = true;
                }
            }

            // Skull shooting
            if (handsDead && npc.ai[1] == 0f && !phase4)
            {
                float skullProjFrequency = bossRush ? 10f : phase2 ? (32f - (death ? 22.5f * (1f - lifeRatio) : 0f)) : 40f;
                if (Main.getGoodWorld)
                    skullProjFrequency *= 0.8f;
                skullProjFrequency = (float)Math.Ceiling(skullProjFrequency);

                if (Main.netMode != NetmodeID.MultiplayerClient && calamityGlobalNPC.newAI[1] % skullProjFrequency == 0f && calamityGlobalNPC.newAI[1] > 45f)
                {
                    Vector2 skullFiringPos = npc.Center;
                    float skullProjTargetX = Main.player[npc.target].Center.X - skullFiringPos.X;
                    float skullProjTargetY = Main.player[npc.target].Center.Y - skullFiringPos.Y;
                    if (Collision.CanHit(skullFiringPos, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                    {
                        float skullProjSpeed = phase2 ? (5f + (death ? 3f * (1f - lifeRatio) : 0f)) : 4f;
                        int spread2 = bossRush ? 100 : 50;
                        Vector2 skullProjDirection = new Vector2(skullProjTargetX + Main.rand.Next(-spread2, spread2 + 1) * 0.01f, skullProjTargetY + Main.rand.Next(-spread2, spread2 + 1) * 0.01f);
                        skullProjDirection.Normalize();
                        skullProjDirection *= skullProjSpeed;
                        skullProjDirection += npc.velocity;
                        skullFiringPos += skullProjDirection * 5f;

                        int type = ProjectileID.Skull;
                        int damage = npc.GetProjectileDamage(type);

                        int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), skullFiringPos, skullProjDirection, type, damage, 0f, Main.myPlayer, -1f);
                        Main.projectile[skullProjectile].timeLeft = 300;

                        npc.netUpdate = true;
                    }
                }
            }

            // Float above target
            if (npc.ai[1] == 0f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                calamityGlobalNPC.newAI[1] += 1f;
                float phaseChangeRateBoost = 3f * (1f - lifeRatio);
                npc.ai[2] += 1f + phaseChangeRateBoost;
                float chargePhaseGateValue = phase5 ? 24f : phase4 ? 120f : 600f;
                float canChargeDistance = phase3 ? 640f : 320f; // 20 tile distance, 40 tile distance in phase 3
                bool canCharge = Vector2.Distance(Main.player[npc.target].Center, npc.Center) >= canChargeDistance;
                bool charge = npc.ai[2] >= chargePhaseGateValue && canCharge;
                if (charge)
                {
                    npc.ai[2] = 0f;
                    npc.ai[1] = 1f;
                    calamityGlobalNPC.newAI[1] = 0f;

                    npc.TargetClosest();
                    npc.SyncExtraAI();
                    npc.netUpdate = true;
                }

                float headYAcceleration = 0.04f + (death ? 0.04f * (1f - lifeRatio) : 0f);
                float headYTopSpeed = 3.5f - (death ? 1.75f - lifeRatio : 0f);
                float headXAcceleration = 0.08f + (death ? 0.08f * (1f - lifeRatio) : 0f);
                float headXTopSpeed = 8.5f - (death ? 4.25f * (1f - lifeRatio) : 0f);
                if (Main.getGoodWorld)
                {
                    headYAcceleration += 0.01f;
                    headYTopSpeed -= 1f;
                    headXAcceleration += 0.05f;
                    headXTopSpeed -= 2f;
                }
                if (bossRush)
                {
                    headYAcceleration *= 1.25f;
                    headYTopSpeed *= 0.75f;
                    headXAcceleration *= 1.25f;
                    headXTopSpeed *= 0.75f;
                }

                float moveAwayGateValue = chargePhaseGateValue - (5f + phaseChangeRateBoost);
                bool moveAwayBeforeCharge = npc.ai[2] >= moveAwayGateValue;
                if (moveAwayBeforeCharge)
                {
                    if (!canCharge)
                    {
                        float maxAcceleration = headXAcceleration + headYAcceleration + (npc.ai[2] - moveAwayGateValue) * (phase5 ? 0.002f : 0.001f);
                        npc.velocity += Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * -maxAcceleration;
                        float maxVelocity = (headXTopSpeed + headYTopSpeed + (npc.ai[2] - moveAwayGateValue) * 0.01f) / 2f;
                        if (npc.velocity.Length() > maxVelocity)
                        {
                            npc.velocity.Normalize();
                            npc.velocity *= maxVelocity;
                        }
                    }

                    // Spin in phase 4 and fire homing skulls outward (skull firing direction varies based on Skeletron's rotation)
                    if (phase4)
                    {
                        npc.rotation += npc.direction * 0.3f;

                        if (npc.localAI[0] == 0f)
                        {
                            npc.localAI[0] = 1f;
                            npc.SyncVanillaLocalAI();

                            SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                        }

                        // Spawn projectiles
                        float skullProjFrequency = death ? 8f : 16f;
                        if (calamityGlobalNPC.newAI[1] % skullProjFrequency == 0f)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 240f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                                {
                                    float skullProjSpeed = headSpinVelocityMult * 0.66f;
                                    Vector2 initialProjectileVelocity = Main.rand.NextVector2CircularEdge(skullProjSpeed, skullProjSpeed);
                                    int type = ProjectileID.Skull;
                                    int damage = npc.GetProjectileDamage(type);
                                    int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, damage, 0f, Main.myPlayer, -1f);
                                    Main.projectile[skullProjectile].timeLeft = 300;
                                }
                            }
                        }
                    }
                    else
                        npc.rotation = npc.velocity.X / 15f;

                    return false;
                }

                npc.rotation = npc.velocity.X / 15f;

                if (npc.Top.Y > Main.player[npc.target].Top.Y - 250f)
                {
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y *= 0.98f;
                    npc.velocity.Y -= headYAcceleration;
                    if (npc.velocity.Y > headYTopSpeed)
                        npc.velocity.Y = headYTopSpeed;
                }
                else if (npc.Top.Y < Main.player[npc.target].Top.Y - 250f)
                {
                    if (npc.velocity.Y < 0f)
                        npc.velocity.Y *= 0.98f;
                    npc.velocity.Y += headYAcceleration;
                    if (npc.velocity.Y < -headYTopSpeed)
                        npc.velocity.Y = -headYTopSpeed;
                }

                if (npc.Center.X > Main.player[npc.target].Center.X)
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X *= 0.98f;
                    npc.velocity.X -= headXAcceleration;
                    if (npc.velocity.X > headXTopSpeed)
                        npc.velocity.X = headXTopSpeed;
                }

                if (npc.Center.X < Main.player[npc.target].Center.X)
                {
                    if (npc.velocity.X < 0f)
                        npc.velocity.X *= 0.98f;
                    npc.velocity.X += headXAcceleration;
                    if (npc.velocity.X < -headXTopSpeed)
                        npc.velocity.X = -headXTopSpeed;
                }
            }

            // Spin charge
            else if (npc.ai[1] == 1f)
            {
                // Set damage
                npc.damage = npc.defDamage;

                if (Main.getGoodWorld)
                {
                    npc.reflectsProjectiles = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[2] == 0f)
                    {
                        if (NPC.CountNPCS(NPCID.DarkCaster) < 6)
                        {
                            for (int i = 0; i < 1000; i++)
                            {
                                int headYAcceleration = (int)(npc.Center.X / 16f) + Main.rand.Next(-50, 51);
                                int headYTopSpeed;
                                for (headYTopSpeed = (int)(npc.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                                {
                                }

                                headYTopSpeed--;
                                if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                                {
                                    int headXAcceleration = NPC.NewNPC(npc.GetSource_FromAI(), headYAcceleration * 16 + 8, headYTopSpeed * 16, NPCID.DarkCaster);
                                    if (Main.netMode == NetmodeID.Server && headXAcceleration < Main.maxNPCs)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, headXAcceleration);

                                    break;
                                }
                            }
                        }

                        if (CalamityWorld.LegendaryMode)
                        {
                            if (!NPC.AnyNPCs(NPCID.DiabolistWhite))
                            {
                                for (int i = 0; i < 1000; i++)
                                {
                                    int headYAcceleration = (int)(npc.Center.X / 16f) + Main.rand.Next(-50, 51);
                                    int headYTopSpeed;
                                    for (headYTopSpeed = (int)(npc.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                                    {
                                    }

                                    headYTopSpeed--;
                                    if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                                    {
                                        int headXAcceleration = NPC.NewNPC(npc.GetSource_FromAI(), headYAcceleration * 16 + 8, headYTopSpeed * 16, NPCID.DiabolistWhite);
                                        if (Main.netMode == NetmodeID.Server && headXAcceleration < Main.maxNPCs)
                                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, headXAcceleration);

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                npc.defense -= 10;

                float phaseChangeRateBoost = phase3 ? 0f : 1f - (lifeRatio - 0.7f) / 0.3f;
                npc.ai[2] += 1f + phaseChangeRateBoost;

                calamityGlobalNPC.newAI[1] += 1f;
                if (calamityGlobalNPC.newAI[1] == 2f)
                    SoundEngine.PlaySound(phase4 ? SoundID.ForceRoarPitched : SoundID.ForceRoar, npc.Center);

                // Shoot shadowflames (giant cursed skull projectiles) while charging in phase 4
                if (phase4)
                {
                    float shadowFlameGateValue = death ? 20f : 30f;
                    int shadowFlameLimit = death ? 3 : 2;
                    if (calamityGlobalNPC.newAI[1] % shadowFlameGateValue == 0f && calamityGlobalNPC.newAI[1] < shadowFlameGateValue * shadowFlameLimit)
                    {
                        // Spawn projectiles
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                            {
                                float shadowFlameProjectileSpeed = death ? 5f : 4f;
                                Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * shadowFlameProjectileSpeed;
                                int type = ProjectileID.Shadowflames;
                                int damage = npc.GetProjectileDamage(type);
                                int shadowFlameProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, damage, 0f, Main.myPlayer);
                                Main.projectile[shadowFlameProjectile].timeLeft = 600;
                            }
                        }
                    }
                }

                float dashPhaseTime = 300f;
                if (npc.ai[2] >= dashPhaseTime)
                {
                    if (Main.getGoodWorld)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.CountNPCS(NPCID.DarkCaster) < 6)
                        {
                            for (int j = 0; j < 1000; j++)
                            {
                                int headYAcceleration = (int)(npc.Center.X / 16f) + Main.rand.Next(-50, 51);
                                int headYTopSpeed;
                                for (headYTopSpeed = (int)(npc.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                                {
                                }

                                headYTopSpeed--;
                                if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                                {
                                    int headXAcceleration = NPC.NewNPC(npc.GetSource_FromAI(), headYAcceleration * 16 + 8, headYTopSpeed * 16, NPCID.DarkCaster);
                                    if (Main.netMode == NetmodeID.Server && headXAcceleration < Main.maxNPCs)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, headXAcceleration);

                                    break;
                                }
                            }
                        }
                    }

                    if (phase3)
                    {
                        // Spawn projectiles
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int chargeSkullAmt = death ? 5 : 3;
                            int chargeSkullSpread = death ? 80 : 60;
                            float rotation = MathHelper.ToRadians(spread);
                            float skullProjSpeed = phase4 ? (6f + (death ? 2f * ((0.35f - lifeRatio) / 0.35f) : 0f)) : 4f;
                            Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                            int type = ProjectileID.Skull;
                            int damage = npc.GetProjectileDamage(type);
                            for (int k = 0; k < chargeSkullAmt + 1; k++)
                            {
                                Vector2 perturbedSpeed = initialProjectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, k / (float)(chargeSkullAmt - 1)));
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center - perturbedSpeed.SafeNormalize(Vector2.UnitY) * 5f, initialProjectileVelocity + perturbedSpeed, type, damage, 0f, Main.myPlayer, -1f);
                                Main.projectile[proj].timeLeft = 300;
                            }
                        }
                    }

                    npc.ai[2] = 0f;
                    npc.ai[1] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;

                    npc.TargetClosest();
                    npc.SyncExtraAI();
                    npc.netUpdate = true;
                }

                npc.rotation += npc.direction * 0.3f;

                Vector2 headSpinPos = npc.Center;
                float headSpinTargetX = Main.player[npc.target].Center.X - headSpinPos.X;
                float headSpinTargetY = Main.player[npc.target].Center.Y - headSpinPos.Y;
                float headSpinTargetDist = (float)Math.Sqrt(headSpinTargetX * headSpinTargetX + headSpinTargetY * headSpinTargetY);

                // Increase speed while charging
                npc.damage = (int)(npc.defDamage * 1.3);
                float velocityBoost = MathHelper.Lerp(0f, 3f, 1f - lifeRatio);
                if (handsDead || bossRush)
                    headSpinVelocityMult += velocityBoost;

                if (!phase3)
                {
                    float headSpeedIncreaseDist = 160f;
                    if (headSpinTargetDist > headSpeedIncreaseDist)
                    {
                        float baseDistanceVelocityMult = 1f + MathHelper.Clamp((headSpinTargetDist - headSpeedIncreaseDist) * 0.0015f, 0.05f, 1.5f);
                        headSpinVelocityMult *= baseDistanceVelocityMult;
                    }
                }

                if (Main.getGoodWorld)
                    headSpinVelocityMult *= 1.3f;

                headSpinTargetDist = headSpinVelocityMult / headSpinTargetDist;
                Vector2 headSpinVelocity = new Vector2(headSpinTargetX, headSpinTargetY) * headSpinTargetDist;

                if (phase3)
                {
                    // Dash directly towards the target until within 15 tiles of the target, and then continue in the same direction
                    float altDashPhaseTime = dashPhaseTime * (death ? 0.9f : 0.85f);
                    if (npc.ai[2] < altDashPhaseTime)
                    {
                        if (npc.Center.Distance(Main.player[npc.target].Center) > 240f || npc.ai[2] == 1f + phaseChangeRateBoost)
                            npc.velocity = headSpinVelocity.SafeNormalize(Vector2.UnitY) * (headSpinVelocityMult * 2f) + npc.Center.DirectionTo(Main.player[npc.target].Center) * 2f;
                        else
                            npc.ai[2] = altDashPhaseTime;
                    }
                }
                else
                    npc.velocity = headSpinVelocity;
            }

            // Daytime enrage
            else if (npc.ai[1] == 2f)
            {
                npc.damage = 1000;
                calamityGlobalNPC.DR = 0.9999f;
                calamityGlobalNPC.unbreakableDR = true;
                npc.rotation += npc.direction * 0.3f;
                Vector2 enrageSpinPos = npc.Center;
                float enrageSpinTargetX = Main.player[npc.target].Center.X - enrageSpinPos.X;
                float enrageSpinTargetY = Main.player[npc.target].Center.Y - enrageSpinPos.Y;
                float enrageSpinTargetDist = (float)Math.Sqrt(enrageSpinTargetX * enrageSpinTargetX + enrageSpinTargetY * enrageSpinTargetY);
                enrageSpinTargetDist = 8f / enrageSpinTargetDist;
                npc.velocity.X = enrageSpinTargetX * enrageSpinTargetDist;
                npc.velocity.Y = enrageSpinTargetY * enrageSpinTargetDist;
            }

            // Despawn
            else if (npc.ai[1] == 3f)
            {
                // Disable teleports
                if (npc.ai[3] != 0f || calamityGlobalNPC.newAI[2] != 0f || calamityGlobalNPC.newAI[3] != 0f)
                {
                    npc.ai[3] = 0f;
                    calamityGlobalNPC.newAI[2] = 0f;
                    calamityGlobalNPC.newAI[3] = 0f;
                    npc.SyncExtraAI();
                    npc.netUpdate = true;
                }

                // Avoid cheap bullshit
                npc.damage = 0;

                npc.velocity.Y += 0.1f;
                if (npc.velocity.Y < 0f)
                    npc.velocity.Y *= 0.95f;
                npc.velocity.X *= 0.95f;
                if (npc.timeLeft > 50)
                    npc.timeLeft = 50;
            }

            // Emit dust
            if (npc.ai[1] != 2f && npc.ai[1] != 3f && numHandsAlive != 0)
            {
                int idleDust = Dust.NewDust(new Vector2(npc.position.X + (npc.width / 2) - 15f - npc.velocity.X * 5f, npc.position.Y + npc.height - 2f), 30, 10, 5, -npc.velocity.X * 0.2f, 3f, 0, default, 2f);
                Main.dust[idleDust].noGravity = true;
                Main.dust[idleDust].velocity.X = Main.dust[idleDust].velocity.X * 1.3f;
                Main.dust[idleDust].velocity.X = Main.dust[idleDust].velocity.X + npc.velocity.X * 0.4f;
                Main.dust[idleDust].velocity.Y = Main.dust[idleDust].velocity.Y + (2f + npc.velocity.Y);
                for (int j = 0; j < 2; j++)
                {
                    idleDust = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + 120f), npc.width, 60, 5, npc.velocity.X, npc.velocity.Y, 0, default, 2f);
                    Main.dust[idleDust].noGravity = true;
                    Main.dust[idleDust].velocity -= npc.velocity;
                    Main.dust[idleDust].velocity.Y = Main.dust[idleDust].velocity.Y + 5f;
                }
            }

            return false;
        }

        public static bool BuffedSkeletronHandAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool bossRush = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRush;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            float yMultiplier = 1f;
            if (calamityGlobalNPC.newAI[0] != 0f)
                yMultiplier = calamityGlobalNPC.newAI[0];

            // Inflict 0 damage for 3 seconds after spawning
            if (calamityGlobalNPC.newAI[1] < 180f)
            {
                calamityGlobalNPC.newAI[1] += 1f;
                if (calamityGlobalNPC.newAI[1] % 15f == 0f)
                    npc.SyncExtraAI();

                npc.damage = 0;
            }
            else
                npc.damage = npc.defDamage;

            npc.spriteDirection = -(int)npc.ai[0];

            if (Main.npc[(int)npc.ai[1]].ai[3] == -60f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Teleport dust
                    for (int m = 0; m < 10; m++)
                    {
                        int teleportDust = Dust.NewDust(npc.position, npc.width, npc.height, 27, 0f, 0f, 200, default, 3f);
                        Main.dust[teleportDust].noGravity = true;
                        Main.dust[teleportDust].velocity.X = Main.dust[teleportDust].velocity.X * 2f;
                    }

                    // New location
                    npc.Center = Main.npc[(int)npc.ai[1]].Center;
                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;
                }
            }

            float skeletronLifeRatio = 1f;
            if (!Main.npc[(int)npc.ai[1]].active || Main.npc[(int)npc.ai[1]].aiStyle != NPCAIStyleID.SkeletronHead)
            {
                npc.ai[2] += 10f;
                if (npc.ai[2] > 50f || Main.netMode != NetmodeID.Server)
                {
                    npc.life = -1;
                    npc.HitEffect(0, 10.0);
                    npc.active = false;
                }
            }
            else
                skeletronLifeRatio = Main.npc[(int)npc.ai[1]].life / (float)Main.npc[(int)npc.ai[1]].lifeMax;

            // Fire skulls from hands at the end of each slap phase
            bool phase2 = skeletronLifeRatio < 0.5f;

            // Fire skulls on a timer
            bool phase3 = skeletronLifeRatio < 0.15f;

            float velocityMultiplier = MathHelper.Lerp(death ? 0.5f : 0.7f, 1f, skeletronLifeRatio);
            float velocityIncrement = MathHelper.Lerp(0.2f, death ? 0.6f : 0.45f, 1f - skeletronLifeRatio);
            float handSwipeVelocity = MathHelper.Lerp(16f, death ? 28f : 22f, 1f - skeletronLifeRatio);
            float handSwipeDuration = HandSwipeDistance / handSwipeVelocity;
            float slapGateValue = HandSlapGateValue;
            float slapTimerIncrement = MathHelper.Lerp(1f, death ? 3f : 2f, 1f - skeletronLifeRatio);

            if (npc.ai[2] == 0f || npc.ai[2] == 3f || phase3)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                if (Main.npc[(int)npc.ai[1]].ai[1] == 3f && npc.timeLeft > 10)
                    npc.timeLeft = 10;

                if (Main.npc[(int)npc.ai[1]].ai[1] != 0f || phase3)
                {
                    // Spawn projectiles
                    if (phase3 && npc.ai[3] > slapGateValue)
                    {
                        npc.ai[3] += 1f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float projectileGateValue = death ? 30f : 60f;
                            if (phase3 && npc.ai[3] % projectileGateValue == 0f && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                            {
                                float skullProjSpeed = handSwipeVelocity * 0.1f;
                                Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                                int type = ProjectileID.Skull;
                                int damage = npc.GetProjectileDamage(type);
                                int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, damage, 0f, Main.myPlayer, -1f);
                                Main.projectile[skullProjectile].timeLeft = 300;
                            }
                        }
                    }

                    float maxX = (bossRush ? 4f : death ? 6f : 7f) * velocityMultiplier;
                    float maxY = (bossRush ? 3f : death ? 4.5f : 5f) * velocityMultiplier;

                    if (npc.Top.Y > Main.npc[(int)npc.ai[1]].Top.Y - 100f * yMultiplier)
                    {
                        if (npc.velocity.Y > 0f)
                            npc.velocity.Y *= 0.94f;
                        npc.velocity.Y -= velocityIncrement;
                        if (npc.velocity.Y > maxY)
                            npc.velocity.Y = maxY;
                    }
                    else if (npc.Top.Y < Main.npc[(int)npc.ai[1]].Top.Y - 100f * yMultiplier)
                    {
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= 0.94f;
                        npc.velocity.Y += velocityIncrement;
                        if (npc.velocity.Y < -maxY)
                            npc.velocity.Y = -maxY;
                    }

                    if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X - 120f * npc.ai[0])
                    {
                        if (npc.velocity.X > 0f)
                            npc.velocity.X *= 0.94f;
                        npc.velocity.X -= velocityIncrement;
                        if (npc.velocity.X > maxX)
                            npc.velocity.X = maxX;
                    }

                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 120f * npc.ai[0])
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X *= 0.94f;
                        npc.velocity.X += velocityIncrement;
                        if (npc.velocity.X < -maxX)
                            npc.velocity.X = -maxX;
                    }
                }
                else
                {
                    if (calamityGlobalNPC.newAI[3] == 1f)
                    {
                        calamityGlobalNPC.newAI[2] += slapTimerIncrement;
                        npc.ai[3] += slapTimerIncrement;
                        if (npc.ai[3] >= slapGateValue)
                        {
                            npc.ai[2] += 1f;
                            npc.ai[3] = calamityGlobalNPC.newAI[2] = slapGateValue;
                            calamityGlobalNPC.newAI[3] = 0f;
                            npc.netUpdate = true;
                            npc.SyncExtraAI();
                        }
                    }
                    else
                    {
                        calamityGlobalNPC.newAI[2] -= slapTimerIncrement * 2f;
                        if (calamityGlobalNPC.newAI[2] <= 0f)
                        {
                            calamityGlobalNPC.newAI[2] = 0f;
                            calamityGlobalNPC.newAI[3] = 1f;
                            npc.SyncExtraAI();
                        }
                    }

                    float maxX = (bossRush ? 4f : death ? 6f : 7f) * velocityMultiplier;
                    float maxY = (bossRush ? 1f : death ? 2f : 2.5f) * velocityMultiplier;

                    if (npc.Top.Y > Main.npc[(int)npc.ai[1]].Top.Y + 230f * yMultiplier)
                    {
                        if (npc.velocity.Y > 0f)
                            npc.velocity.Y *= 0.92f;
                        npc.velocity.Y -= velocityIncrement;
                        if (npc.velocity.Y > maxY)
                            npc.velocity.Y = maxY;
                    }
                    else if (npc.Top.Y < Main.npc[(int)npc.ai[1]].Top.Y + 230f * yMultiplier)
                    {
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= 0.92f;
                        npc.velocity.Y += velocityIncrement;
                        if (npc.velocity.Y < -maxY)
                            npc.velocity.Y = -maxY;
                    }

                    if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0])
                    {
                        if (npc.velocity.X > 0f)
                            npc.velocity.X *= 0.92f;
                        npc.velocity.X -= velocityIncrement;
                        if (npc.velocity.X > maxX)
                            npc.velocity.X = maxX;
                    }

                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0])
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X *= 0.92f;
                        npc.velocity.X += velocityIncrement;
                        if (npc.velocity.X < -maxX)
                            npc.velocity.X = -maxX;
                    }
                }

                Vector2 handCurrentPos = npc.Center;
                float handIdleXPos = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - handCurrentPos.X;
                float handIdleYPos = Main.npc[(int)npc.ai[1]].Top.Y + 230f - handCurrentPos.Y;
                float handIdleDist = (float)Math.Sqrt(handIdleXPos * handIdleXPos + handIdleYPos * handIdleYPos);
                npc.rotation = (float)Math.Atan2(handIdleYPos, handIdleXPos) + MathHelper.PiOver2;

                return false;
            }

            if (npc.ai[2] == 1f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                Vector2 handCurrentPosition = npc.Center;
                float handDrawbackXPos = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - handCurrentPosition.X;
                float handDrawbackYPos = Main.npc[(int)npc.ai[1]].Top.Y + 230f - handCurrentPosition.Y;
                float handDrawbackDist = (float)Math.Sqrt(handDrawbackXPos * handDrawbackXPos + handDrawbackYPos * handDrawbackYPos);
                npc.rotation = (float)Math.Atan2(handDrawbackYPos, handDrawbackXPos) + MathHelper.PiOver2;
                npc.velocity.X *= 0.95f;
                npc.velocity.Y -= velocityIncrement;

                if (npc.velocity.Y < -14f)
                    npc.velocity.Y = -14f;
                else if (npc.velocity.Y > 10f)
                    npc.velocity.Y = 10f;

                if (npc.Top.Y < Main.npc[(int)npc.ai[1]].Top.Y - 200f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.TargetClosest();
                    npc.ai[2] = 2f;
                    npc.ai[3] = 0f;
                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[2] == 2f)
            {
                // Set damage
                npc.damage = npc.defDamage;

                npc.ai[3] += 1f;
                if (npc.ai[3] >= handSwipeDuration || Vector2.Distance(Main.npc[(int)npc.ai[1]].Center, npc.Center) > HandSwipeDistance)
                {
                    npc.ai[2] = 3f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;

                    // Spawn projectiles
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (phase2 && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                        {
                            float skullProjSpeed = handSwipeVelocity * 0.33f;
                            Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                            int type = ProjectileID.Skull;
                            int damage = npc.GetProjectileDamage(type);
                            int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, damage, 0f, Main.myPlayer, -1f);
                            Main.projectile[skullProjectile].timeLeft = 300;
                        }
                    }
                }
            }
            else if (npc.ai[2] == 4f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                Vector2 handStrikeCurrentPos = npc.Center;
                float handStrikeXPos = Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0] - handStrikeCurrentPos.X;
                float handStrikeYPos = Main.npc[(int)npc.ai[1]].Top.Y + 230f - handStrikeCurrentPos.Y;
                float handStrikeDist = (float)Math.Sqrt(handStrikeXPos * handStrikeXPos + handStrikeYPos * handStrikeYPos);
                npc.rotation = (float)Math.Atan2(handStrikeYPos, handStrikeXPos) + MathHelper.PiOver2;
                npc.velocity.Y *= 0.95f;
                npc.velocity.X += velocityIncrement * -npc.ai[0];

                if (npc.velocity.X < -10f)
                    npc.velocity.X = -10f;
                else if (npc.velocity.X > 14f)
                    npc.velocity.X = 14f;

                if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 500f || npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X + 500f)
                {
                    // Set damage
                    npc.damage = npc.defDamage;

                    npc.TargetClosest();
                    npc.ai[2] = 5f;
                    npc.ai[3] = 0f;
                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[2] == 5f)
            {
                // Set damage
                npc.damage = npc.defDamage;

                npc.ai[3] += 1f;
                if (npc.ai[3] >= handSwipeDuration || Vector2.Distance(Main.npc[(int)npc.ai[1]].Center, npc.Center) > HandSwipeDistance)
                {
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;

                    // Spawn projectiles
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (phase2 && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                        {
                            float skullProjSpeed = handSwipeVelocity * 0.33f;
                            Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                            int type = ProjectileID.Skull;
                            int damage = npc.GetProjectileDamage(type);
                            int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, damage, 0f, Main.myPlayer, -1f);
                            Main.projectile[skullProjectile].timeLeft = 300;
                        }
                    }
                }
            }

            return false;
        }

        public static void RevengeanceDungeonGuardianAI(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (npc.ai[1] != 3f)
            {
                Vector2 targetVector = target.Center - npc.Center;
                float targetDist = targetVector.Length();
                targetDist = 12f / targetDist;
                npc.velocity.X = targetVector.X * targetDist;
                npc.velocity.Y = targetVector.Y * targetDist;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (npc.localAI[1]++ % 60f == 59f)
                    {
                        Vector2 source = npc.Center;
                        if (Collision.CanHit(source, 1, 1, target.Center, target.width, target.height))
                        {
                            float speed = 5f;
                            float xDist = target.Center.X - source.X + Main.rand.Next(-20, 21);
                            float yDist = target.Center.Y - source.Y + Main.rand.Next(-20, 21);
                            Vector2 velocity = new Vector2(xDist, yDist);
                            float distTarget = velocity.Length();
                            distTarget = speed / distTarget;
                            velocity.X *= distTarget;
                            velocity.Y *= distTarget;
                            Vector2 offset = new Vector2(velocity.X * 1f + Main.rand.Next(-50, 51) * 0.01f, velocity.Y * 1f + Main.rand.Next(-50, 51) * 0.01f);
                            offset.Normalize();
                            offset *= speed;
                            offset += npc.velocity;
                            velocity.X = offset.X;
                            velocity.Y = offset.Y;
                            int damage = 2500;
                            int projType = ProjectileID.Skull;
                            source += offset * 5f;
                            int skull = Projectile.NewProjectile(npc.GetSource_FromAI(), source, velocity, projType, damage, 0f, Main.myPlayer, -1f);
                            Main.projectile[skull].timeLeft = 300;
                        }
                    }
                }
            }
        }
    }
}
