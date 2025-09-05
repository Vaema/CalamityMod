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
    public static class SkeletronAI
    {
        public const float ChargeGateValue = 600f;
        public const float ChargeTelegraphTime = 120f;
        public const float HandSlapGateValue = 300f;
        public const float HandSlapTelegraphTime = 120f;
        public const float HandSwipeDistance = 960f; // 60 tiles
        public const float HandSwipeDistance_Master = 1280f; // 80 tiles

        // Vanilla values
        public static float SpinDamageMult = 1.3f; // 91
        public static int SkullDamage = 17; // 68; Also applies to crossbones

        // Rev+ exclusive
        public static int ShadowflameDamage = 20; // 80

        public static bool BuffedSkeletronAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases
            float phase2LifeRatio = death ? 1f : 0.85f;
            float phase3LifeRatio = death ? 0.9f : 0.7f;
            float respawnHandsLifeRatio = 0.5f;
            float phase4LifeRatio = death ? 0.4f : 0.3f;
            float useSkullSpreadsAfterChargeLifeRatio = death ? 0.3f : 0.2f;
            float phase5LifeRatio = death ? 0.2f : 0.1f;

            // Begin firing spreads of skulls phase
            bool phase2 = lifeRatio < phase2LifeRatio;

            // Begin using a more dangerous charge attack phase
            bool phase3 = lifeRatio < phase3LifeRatio;

            // Spawn a new set of hands, fire skulls at the end of each charge and fire skulls from hands at the end of each slap phase
            bool respawnHands = lifeRatio < respawnHandsLifeRatio;

            // Fire giant cursed skull projectiles (yes, these curse you if you get hit) during charge attack and hands fire skulls phase
            bool phase4 = lifeRatio < phase4LifeRatio;

            // Self-explanatory
            bool useSkullSpreadsAfterCharge = lifeRatio < useSkullSpreadsAfterChargeLifeRatio;

            // Rapid teleport and charge, stop using idle phase
            bool phase5 = lifeRatio < phase5LifeRatio;

            // Set defense
            npc.defense = npc.defDefense;
            npc.damage = npc.defDamage;

            npc.reflectsProjectiles = false;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

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
                    Main.npc[skeletronHand].ai[0] = death ? -1.3f : -1f;
                    Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                    Main.npc[skeletronHand].target = npc.target;
                    Main.npc[skeletronHand].netUpdate = true;

                    skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                    Main.npc[skeletronHand].ai[0] = death ? 1.3f : 1f;
                    Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                    Main.npc[skeletronHand].ai[3] = 150f;
                    Main.npc[skeletronHand].Calamity().newAI[2] = 150f;
                    Main.npc[skeletronHand].target = npc.target;
                    Main.npc[skeletronHand].netUpdate = true;

                    // Spawn two additional hands with different attack timings
                    if (death)
                    {
                        skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                        Main.npc[skeletronHand].ai[0] = -1.3f;
                        Main.npc[skeletronHand].Calamity().newAI[0] = -1f;
                        Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                        Main.npc[skeletronHand].ai[3] = respawnHands ? -75f : 0f;
                        Main.npc[skeletronHand].Calamity().newAI[2] = respawnHands ? -75f : 0f;
                        Main.npc[skeletronHand].target = npc.target;
                        Main.npc[skeletronHand].netUpdate = true;

                        skeletronHand = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI);
                        Main.npc[skeletronHand].ai[0] = 1.3f;
                        Main.npc[skeletronHand].Calamity().newAI[0] = -1f;
                        Main.npc[skeletronHand].ai[1] = npc.whoAmI;
                        Main.npc[skeletronHand].ai[3] = respawnHands ? 75f : 150f;
                        Main.npc[skeletronHand].Calamity().newAI[2] = respawnHands ? 75f : 150f;
                        Main.npc[skeletronHand].target = npc.target;
                        Main.npc[skeletronHand].netUpdate = true;
                    }
                }
            }

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

            // Daytime enrage
            if (Main.IsItDay() && !BossRushEvent.BossRushActive && npc.ai[1] != 3f && npc.ai[1] != 2f)
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
            float spread = Main.getGoodWorld ? 180 : 60;
            float headSpinVelocityMult = phase3 ? 12f : 4.5f;

            switch (numHandsAlive)
            {
                case 0:
                    numProj = Main.getGoodWorld ? 36 : death ? 9 : 7;
                    spread = Main.getGoodWorld ? 180 : death ? 90 : 82;
                    headSpinVelocityMult = phase3 ? 12f : 6f;
                    break;

                case 1:
                    numProj = Main.getGoodWorld ? 27 : death ? 7 : 5;
                    spread = Main.getGoodWorld ? 150 : death ? 76 : 68;
                    headSpinVelocityMult = phase3 ? 11.5f : 5f;
                    break;

                case 2:
                    numProj = Main.getGoodWorld ? 18 : death ? 6 : 4;
                    spread = Main.getGoodWorld ? 140 : death ? 70 : 62;
                    headSpinVelocityMult = phase3 ? 11f : 4.5f;
                    break;

                case 3:
                    numProj = Main.getGoodWorld ? 15 : death ? 5 : 3;
                    spread = Main.getGoodWorld ? 130 : death ? 64 : 56;
                    headSpinVelocityMult = phase3 ? 10.5f : 4f;
                    break;

                case 4:
                    numProj = Main.getGoodWorld ? 12 : death ? 4 : 3;
                    spread = Main.getGoodWorld ? 120 : 56;
                    headSpinVelocityMult = phase3 ? 10f : 3.5f;
                    break;
            }

            // Reduce the amount of skulls per spread in later phases due to near-constant teleporting
            if (!death && numProj > 3)
            {
                if (phase4)
                    numProj--;
                if (phase5 && numProj > 3)
                    numProj--;
            }

            if (death)
                headSpinVelocityMult *= 1.2f;

            // Velocity used to move Skeletron away from the target before charging
            float moveAwayVelocity = headSpinVelocityMult;
            if (!phase3)
                moveAwayVelocity *= 2f;

            // Hand DR, scale DR up if the hands are still alive as Skeletron's HP lowers
            npc.chaseable = handsDead;
            float minDR = 0f;
            float maxDR = 0.9999f;
            calamityGlobalNPC.DR = !handsDead ? (float)Math.Sqrt(MathHelper.Lerp(minDR, maxDR, respawnHands ? (respawnHandsLifeRatio - lifeRatio) / respawnHandsLifeRatio : 2f - lifeRatio / respawnHandsLifeRatio)) : minDR;
            calamityGlobalNPC.unbreakableDR = !handsDead;
            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = !handsDead;

            // Value to start teleport dust
            int teleportGateValue = phase5 ? 180 : 300;

            // Bool to disable skull firing after charging if teleport was recent or is about to happen
            bool disableSkullsAfterCharge = npc.ai[3] <= 60f || npc.ai[3] > teleportGateValue + 60f;

            // Teleport while not despawning
            if (npc.ai[1] != 3f)
            {
                int dustType = DustID.GemDiamond;

                // Post-teleport
                if (npc.ai[3] == -60f)
                {
                    npc.ai[3] = 0f;

                    SoundEngine.PlaySound(SoundID.Item66, npc.Center);

                    // Fire skulls after teleport
                    if (Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.Skull;

                            // Inverse parabolic projectile spreads
                            Vector2 baseVel = npc.SafeDirectionTo(Main.player[npc.target].Center) * (death ? 6f : 5f);
                            Vector2 firingPos = npc.Center + baseVel * 5f;
                            float centralCount = 0.5f * (numProj - 1f);
                            for (int i = 0; i < numProj; i++)
                            {
                                float offset = MathHelper.ToRadians(MathHelper.Lerp(-spread * 0.5f, spread * 0.5f, i / (numProj - 1f)));
                                float velocityMult = MathHelper.Lerp(0.5f, 1.5f, MathF.Abs(centralCount - i) / centralCount);
                                Projectile shot = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), firingPos, baseVel.RotatedBy(offset) * velocityMult, type, SkullDamage, 0f, Main.myPlayer, -2f);
                                shot.timeLeft = 600;
                            }

                            npc.netUpdate = true;
                        }
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
                npc.ai[3] += 1f + (((phase2 && handsDead) || phase4) ? 0.5f : 0f) - (handsDead ? 0f : 0.5f);

                // Dust to show teleport
                int ai3 = (int)npc.ai[3]; // 0 to 30, and -60

                if (npc.localAI[0] == 1f && calamityGlobalNPC.newAI[2] == 0f && calamityGlobalNPC.newAI[3] == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 skullFaceDirection = npc.Center + new Vector2(npc.direction * 20, 6f);
                        Vector2 skullTargetDirection = Main.player[npc.target].Center - skullFaceDirection;
                        Point skullTileCoords = npc.Center.ToTileCoordinates();
                        Point targetTileCoords = Main.player[npc.target].Center.ToTileCoordinates();
                        int randomTeleportOffset = 20 - (int)Math.Ceiling(MathHelper.Lerp(0f, 10f, 1f - lifeRatio));
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

                // Teleport location telegraph
                if (calamityGlobalNPC.newAI[2] != 0f && calamityGlobalNPC.newAI[3] != 0f)
                {
                    for (int m = 0; m < 5; m++)
                    {
                        Vector2 position = new Vector2(calamityGlobalNPC.newAI[2], calamityGlobalNPC.newAI[3]);
                        int teleportDust = Dust.NewDust(position, npc.width, npc.height, dustType, 0f, 0f, 100, default, 2f);
                        Main.dust[teleportDust].noGravity = true;
                    }
                }

                // Teleport
                if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f && npc.ai[1] != 1f && calamityGlobalNPC.newAI[2] != 0f && calamityGlobalNPC.newAI[3] != 0f)
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
            if ((handsDead || death) && npc.ai[1] == 0f && !phase4)
            {
                float skullProjFrequency = phase2 ? (48f - (death ? 17.5f * (1f - lifeRatio) : 0f)) : 60f;
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
                        int spread2 = 50;
                        Vector2 skullProjDirection = new Vector2(skullProjTargetX + Main.rand.Next(-spread2, spread2 + 1) * 0.01f, skullProjTargetY + Main.rand.Next(-spread2, spread2 + 1) * 0.01f).SafeNormalize(Vector2.UnitY);
                        skullProjDirection *= skullProjSpeed;
                        skullProjDirection += npc.velocity;
                        skullFiringPos += skullProjDirection * 5f;

                        int type = ProjectileID.Skull;

                        int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), skullFiringPos, skullProjDirection, type, SkullDamage, 0f, Main.myPlayer, -1f);
                        Main.projectile[skullProjectile].timeLeft = 600;
                        if (death && handsDead)
                        {
                            skullProjDirection = new Vector2(skullProjTargetX, skullProjTargetY).SafeNormalize(Vector2.UnitY);
                            skullProjDirection *= skullProjSpeed * 2f;
                            int skullProjectile2 = Projectile.NewProjectile(npc.GetSource_FromAI(), skullFiringPos, skullProjDirection, type, SkullDamage, 0f, Main.myPlayer, -2f);
                            Main.projectile[skullProjectile2].timeLeft = 600;
                        }

                        npc.netUpdate = true;
                    }
                }
            }

            // Float above target
            if (npc.ai[1] == 0f)
            {
                calamityGlobalNPC.newAI[1] += 1f;
                float chargePhaseChangeRateBoost = phase5 ? (death ? 24f : 8f) : phase4 ? (death ? 6f : 4f) : ((death ? 4.5f : 3f) * ((1f - lifeRatio) / (1f - phase4LifeRatio)));
                if (!handsDead)
                    chargePhaseChangeRateBoost *= 0.25f;

                float chargePhaseChangeRate = chargePhaseChangeRateBoost + 1f;
                npc.ai[2] += chargePhaseChangeRate;
                npc.localAI[1] += chargePhaseChangeRate;
                float chargePhaseGateValue = ChargeGateValue;
                if (npc.localAI[1] > chargePhaseGateValue)
                    npc.localAI[1] = chargePhaseGateValue;

                float forcedMoveAwayTime = death ? 15f : 45f;
                float canChargeDistance = 320f; // 20 tile distance
                bool hasMovedForcedDistance = npc.localAI[2] >= forcedMoveAwayTime;
                bool canCharge = Vector2.Distance(Main.player[npc.target].Center, npc.Center) >= canChargeDistance;
                bool charge = npc.ai[2] >= chargePhaseGateValue && canCharge;
                bool forceCharge = npc.ai[2] > chargePhaseGateValue + 120f;
                if (charge || forceCharge)
                {
                    npc.localAI[2] += 1f;
                    if (hasMovedForcedDistance || !phase3)
                    {
                        npc.ai[2] = 0f;
                        npc.ai[1] = 1f;
                        npc.localAI[0] = 1f;
                        npc.localAI[1] = chargePhaseGateValue;
                        npc.localAI[2] = 0f;
                        calamityGlobalNPC.newAI[1] = 0f;

                        npc.SyncExtraAI();
                        npc.SyncVanillaLocalAI();
                        npc.netUpdate = true;
                    }
                }

                float headYAcceleration = (Main.getGoodWorld ? 0.07f : death ? (0.06f + 0.04f * (1f - lifeRatio)) : 0.04f);
                float headYTopSpeed = headYAcceleration * 100f;
                float headXAcceleration = (Main.getGoodWorld ? 0.21f : death ? (0.16f + 0.08f * (1f - lifeRatio)) : 0.08f);
                float headXTopSpeed = headXAcceleration * 100f;
                float deceleration = Main.getGoodWorld ? 0.83f : death ? 0.86f : 0.89f;

                float moveAwayGateValue = chargePhaseGateValue - (5f + chargePhaseChangeRate);
                bool moveAwayBeforeCharge = npc.ai[2] >= moveAwayGateValue;
                if (moveAwayBeforeCharge)
                {
                    if (!canCharge || !hasMovedForcedDistance)
                    {
                        float phase5Multiplier = 1.2f;
                        float maxVelocity = (npc.ai[2] - moveAwayGateValue) * (moveAwayVelocity * 0.002f);
                        if (phase5)
                            maxVelocity *= phase5Multiplier;

                        float maxVelocityCap = moveAwayVelocity;
                        if (phase5)
                            maxVelocityCap *= phase5Multiplier;
                        if (maxVelocity > maxVelocityCap)
                            maxVelocity = maxVelocityCap;

                        npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * -maxVelocity;
                        npc.SyncMotionToServer();
                    }

                    // New charge attack
                    if (phase3)
                    {
                        npc.rotation += npc.direction * 0.3f;

                        if (npc.localAI[0] == 0f)
                        {
                            npc.localAI[0] = 1f;
                            npc.SyncVanillaLocalAI();

                            SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                        }
                    }
                    else
                        npc.rotation = npc.velocity.X / 15f;

                    // Force net updates every frame during this movement to avoid despawning in multiplayer
                    // I'm doing this because npc.ai[2] changes every frame and that's used to calculate Skeletron's velocity here
                    npc.ForceNetUpdate();
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
                                    if (Main.dedServ && headXAcceleration < Main.maxNPCs)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, headXAcceleration);

                                    break;
                                }
                            }
                        }

                        if (Main.zenithWorld)
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
                                        if (Main.dedServ && headXAcceleration < Main.maxNPCs)
                                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, headXAcceleration);

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                npc.defense = npc.defDefense - 10;
                npc.damage = (int)Math.Round(npc.defDamage * SpinDamageMult);

                float phaseChangeRateBoost = phase3 ? 0f : 1f - (lifeRatio - phase3LifeRatio) / (1f - phase3LifeRatio);
                npc.ai[2] += 1f + phaseChangeRateBoost;

                calamityGlobalNPC.newAI[1] += 1f;
                if (calamityGlobalNPC.newAI[1] == 2f)
                    SoundEngine.PlaySound(phase3 ? SoundID.ForceRoarPitched : SoundID.ForceRoar, npc.Center);

                // Shoot shadowflames (giant cursed skull projectiles) while charging in phase 4
                if (phase4 && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    float shadowFlameGateValue = 20f;
                    int shadowFlameLimit = death ? 3 : 2;
                    if (calamityGlobalNPC.newAI[1] % shadowFlameGateValue == 0f && calamityGlobalNPC.newAI[1] < shadowFlameGateValue * shadowFlameLimit)
                    {
                        // Spawn projectiles
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 240f)
                            {
                                float shadowFlameProjectileSpeed = death ? 6f : 4f;
                                Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * shadowFlameProjectileSpeed;
                                int type = ProjectileID.Shadowflames;
                                int shadowFlameProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, ShadowflameDamage, 0f, Main.myPlayer, 0f, 1f);
                                Main.projectile[shadowFlameProjectile].timeLeft = 600;
                            }
                        }
                    }
                }

                // Reset telegraph timer to create color fade
                if (npc.localAI[1] > 0f)
                {
                    npc.localAI[1] -= 2f;
                    if (npc.localAI[1] <= 0f)
                    {
                        npc.localAI[1] = 0f;
                        npc.SyncVanillaLocalAI();
                    }
                }

                bool dontGoMach10 = false;
                float dashPhaseTime = death ? 210f : 300f;
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
                                    if (Main.dedServ && headXAcceleration < Main.maxNPCs)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, headXAcceleration);

                                    break;
                                }
                            }
                        }
                    }

                    if (useSkullSpreadsAfterCharge && !disableSkullsAfterCharge && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                    {
                        // Spawn projectiles
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int chargeSkullAmt = death ? 5 : 3;
                            int chargeSkullSpread = death ? 80 : 60;
                            float rotation = MathHelper.ToRadians(chargeSkullSpread);
                            float skullProjSpeed = phase5 ? (6f + (death ? 2f * ((phase5LifeRatio - lifeRatio) / phase5LifeRatio) : 0f)) : 4f;
                            Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                            int type = ProjectileID.Skull;
                            for (int k = 0; k < chargeSkullAmt + 1; k++)
                            {
                                Vector2 perturbedSpeed = initialProjectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, k / (float)(chargeSkullAmt - 1)));
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center - perturbedSpeed.SafeNormalize(Vector2.UnitY) * 5f, perturbedSpeed, type, SkullDamage, 0f, Main.myPlayer, -1f);
                                Main.projectile[proj].timeLeft = 600;
                                if (death)
                                {
                                    int proj2 = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center - perturbedSpeed.SafeNormalize(Vector2.UnitY) * 5f, perturbedSpeed, type, SkullDamage, 0f, Main.myPlayer, -2f);
                                    Main.projectile[proj2].timeLeft = 600;
                                }
                            }
                        }
                    }

                    npc.ai[2] = 0f;
                    npc.ai[1] = 0f;
                    npc.localAI[0] = 0f;
                    npc.localAI[1] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;

                    CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);
                    npc.SyncVanillaLocalAI();
                    npc.SyncExtraAI();
                    npc.netUpdate = true;

                    dontGoMach10 = true;
                }

                npc.rotation += npc.direction * 0.3f;

                Vector2 headSpinPos = npc.Center;
                float headSpinTargetX = Main.player[npc.target].Center.X - headSpinPos.X;
                float headSpinTargetY = Main.player[npc.target].Center.Y - headSpinPos.Y;
                float headSpinTargetDist = (float)Math.Sqrt(headSpinTargetX * headSpinTargetX + headSpinTargetY * headSpinTargetY);

                // Increase speed while charging
                if (!phase3)
                {
                    float velocityBoost = MathHelper.Lerp(0f, 3f, (1f - lifeRatio) / (1f - phase3LifeRatio));
                    if (handsDead)
                        headSpinVelocityMult += velocityBoost;
                }

                float altDashStopDistance = death ? 320f : 400f;
                float headSpeedIncreaseDist = phase3 ? altDashStopDistance : 160f;
                if (headSpinTargetDist > headSpeedIncreaseDist)
                {
                    float velocityMult = phase3 ? 0.00075f : 0.0015f;
                    float baseDistanceVelocityMult = 1f + MathHelper.Clamp((headSpinTargetDist - headSpeedIncreaseDist) * 0.0015f, 0.05f, death ? 2f : 1.5f);
                    headSpinVelocityMult *= baseDistanceVelocityMult;
                }

                if (Main.getGoodWorld)
                    headSpinVelocityMult *= 1.3f;

                headSpinTargetDist = headSpinVelocityMult / headSpinTargetDist;
                Vector2 headSpinVelocity = new Vector2(headSpinTargetX, headSpinTargetY) * headSpinTargetDist;

                if (!dontGoMach10)
                {
                    if (phase3)
                    {
                        // Dash directly towards the target until within 15 tiles of the target, and then continue in the same direction
                        float altDashPhaseTime = dashPhaseTime * (death ? 0.9f : 0.85f);
                        if (npc.ai[2] < altDashPhaseTime)
                        {
                            if (npc.Center.Distance(Main.player[npc.target].Center) > altDashStopDistance || npc.ai[2] == 1f + phaseChangeRateBoost)
                                npc.velocity = headSpinVelocity.SafeNormalize(Vector2.UnitY) * headSpinVelocityMult + npc.Center.DirectionTo(Main.player[npc.target].Center) * 2f;
                            else
                                npc.ai[2] = altDashPhaseTime;
                        }
                    }
                    else
                        npc.velocity = headSpinVelocity;
                }
            }

            // Daytime enrage
            else if (npc.ai[1] == 2f)
            {
                npc.damage = 1000;
                calamityGlobalNPC.DR = 0.9999f;
                calamityGlobalNPC.unbreakableDR = true;

                calamityGlobalNPC.CurrentlyEnraged = true;
                calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = true;

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
                int idleDust = Dust.NewDust(new Vector2(npc.Center.X - 15f - npc.velocity.X * 5f, npc.position.Y + npc.height - 2f), 30, 10, DustID.Blood, -npc.velocity.X * 0.2f, 3f, 0, default, 2f);
                Main.dust[idleDust].noGravity = true;
                Main.dust[idleDust].velocity.X = Main.dust[idleDust].velocity.X * 1.3f;
                Main.dust[idleDust].velocity.X = Main.dust[idleDust].velocity.X + npc.velocity.X * 0.4f;
                Main.dust[idleDust].velocity.Y = Main.dust[idleDust].velocity.Y + (2f + npc.velocity.Y);
                for (int j = 0; j < 2; j++)
                {
                    idleDust = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + 120f), npc.width, 60, DustID.Blood, npc.velocity.X, npc.velocity.Y, 0, default, 2f);
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

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            float yMultiplier = 1f;
            if (calamityGlobalNPC.newAI[0] != 0f)
                yMultiplier = calamityGlobalNPC.newAI[0];
            if (death)
                yMultiplier *= 1.3f;

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
                        int teleportDust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GemDiamond, 0f, 0f, 200, default, 3f);
                        Main.dust[teleportDust].noGravity = true;
                        Main.dust[teleportDust].velocity.X *= 2f;
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
                if (npc.ai[2] > 50f || !Main.dedServ)
                {
                    npc.life = -1;
                    npc.HitEffect(0, 10.0);
                    npc.active = false;
                }
            }
            else
                skeletronLifeRatio = Main.npc[(int)npc.ai[1]].life / (float)Main.npc[(int)npc.ai[1]].lifeMax;

            // This bool exists for fairness so the hands don't slap when Skeletron is in phase 3 and getting ready to do the new charge
            bool cancelSlap = Main.npc[(int)npc.ai[1]].ai[2] >= ChargeGateValue;

            // Fire skulls from hands at the end of each slap phase (master mode only)
            bool phase2 = skeletronLifeRatio < 0.5f;

            // Attack far more often if still alive
            bool phase3 = skeletronLifeRatio < 0.3f;

            float velocityMultiplier = MathHelper.Lerp(death ? 0.6f : 0.7f, 1f, skeletronLifeRatio);
            float velocityIncrement = MathHelper.Lerp(0.2f, death ? 0.4f : 0.3f, 1f - skeletronLifeRatio);
            float handSwipeVelocity = MathHelper.Lerp(16f, death ? 24f : 20f, 1f - skeletronLifeRatio);
            float deceleration = Main.getGoodWorld ? 0.78f : death ? 0.82f : 0.86f;

            if (death)
            {
                velocityMultiplier *= 0.75f;
                velocityIncrement *= 1.5f;
                handSwipeVelocity *= 1.35f;
                deceleration *= 0.75f;
            }

            float handSwipeDistance = death ? HandSwipeDistance_Master : HandSwipeDistance;
            float handSwipeDuration = handSwipeDistance / handSwipeVelocity;
            float slapGateValue = HandSlapGateValue;

            float slapTimerIncrement = MathHelper.Lerp(death ? 1.5f : 1f, death ? 3f : 2f, 1f - skeletronLifeRatio);
            if (phase3)
                slapTimerIncrement *= (death ? 2.5f : 2f);
            else if (phase2)
                slapTimerIncrement *= (death ? 2f : 1.5f);

            if (npc.ai[2] == 0f || npc.ai[2] == 3f)
            {
                if (Main.npc[(int)npc.ai[1]].ai[1] == 3f && npc.timeLeft > 10)
                    npc.timeLeft = 10;

                if (Main.npc[(int)npc.ai[1]].ai[1] != 0f || cancelSlap)
                {
                    deceleration *= 0.75f;
                    velocityIncrement *= 1.5f;

                    float maxX = velocityIncrement * 100f * velocityMultiplier;
                    float maxY = velocityIncrement * 100f * velocityMultiplier;

                    if (npc.Top.Y > Main.npc[(int)npc.ai[1]].Top.Y - 100f * yMultiplier)
                    {
                        if (npc.velocity.Y > 0f)
                            npc.velocity.Y *= deceleration;
                        npc.velocity.Y -= velocityIncrement;
                        if (npc.velocity.Y > maxY)
                            npc.velocity.Y = maxY;
                    }
                    else if (npc.Top.Y < Main.npc[(int)npc.ai[1]].Top.Y - 100f * yMultiplier)
                    {
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= deceleration;
                        npc.velocity.Y += velocityIncrement;
                        if (npc.velocity.Y < -maxY)
                            npc.velocity.Y = -maxY;
                    }

                    if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X - 120f * npc.ai[0])
                    {
                        if (npc.velocity.X > 0f)
                            npc.velocity.X *= deceleration;
                        npc.velocity.X -= velocityIncrement;
                        if (npc.velocity.X > maxX)
                            npc.velocity.X = maxX;
                    }

                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 120f * npc.ai[0])
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X *= deceleration;
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
                            npc.target = Main.npc[(int)npc.ai[1]].target;
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

                    float maxX = velocityIncrement * 100f * velocityMultiplier;
                    float maxY = velocityIncrement * 100f * velocityMultiplier;

                    if (npc.Top.Y > Main.npc[(int)npc.ai[1]].Top.Y + 230f * yMultiplier)
                    {
                        if (npc.velocity.Y > 0f)
                            npc.velocity.Y *= deceleration;
                        npc.velocity.Y -= velocityIncrement;
                        if (npc.velocity.Y > maxY)
                            npc.velocity.Y = maxY;
                    }
                    else if (npc.Top.Y < Main.npc[(int)npc.ai[1]].Top.Y + 230f * yMultiplier)
                    {
                        if (npc.velocity.Y < 0f)
                            npc.velocity.Y *= deceleration;
                        npc.velocity.Y += velocityIncrement;
                        if (npc.velocity.Y < -maxY)
                            npc.velocity.Y = -maxY;
                    }

                    if (npc.Center.X > Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0])
                    {
                        if (npc.velocity.X > 0f)
                            npc.velocity.X *= deceleration;
                        npc.velocity.X -= velocityIncrement;
                        if (npc.velocity.X > maxX)
                            npc.velocity.X = maxX;
                    }

                    if (npc.Center.X < Main.npc[(int)npc.ai[1]].Center.X - 200f * npc.ai[0])
                    {
                        if (npc.velocity.X < 0f)
                            npc.velocity.X *= deceleration;
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
                    npc.ai[2] = 2f;
                    npc.ai[3] = 0f;
                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[2] == 2f)
            {
                npc.ai[3] += 1f;
                if (npc.ai[3] >= handSwipeDuration || Vector2.Distance(Main.npc[(int)npc.ai[1]].Center, npc.Center) > handSwipeDistance || cancelSlap)
                {
                    npc.ai[2] = 3f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;

                    // Spawn projectiles
                    if (death && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && !cancelSlap)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (phase2 && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f)
                            {
                                float skullProjSpeed = handSwipeVelocity * (phase3 ? 0.6f : 0.2f);
                                Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                                int type = ProjectileID.Skull;
                                int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, SkullDamage, 0f, Main.myPlayer, -(phase3 ? 2f : 1f));
                                Main.projectile[skullProjectile].timeLeft = 600;
                            }
                        }
                    }
                }
            }
            else if (npc.ai[2] == 4f)
            {
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
                    npc.ai[2] = 5f;
                    npc.ai[3] = 0f;
                    npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[2] == 5f)
            {
                npc.ai[3] += 1f;
                if (npc.ai[3] >= handSwipeDuration || Vector2.Distance(Main.npc[(int)npc.ai[1]].Center, npc.Center) > handSwipeDistance || cancelSlap)
                {
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;

                    // Spawn projectiles
                    if (death && Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height) && !cancelSlap)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (phase2 && Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 160f)
                            {
                                float skullProjSpeed = handSwipeVelocity * (phase3 ? 0.6f : 0.2f);
                                Vector2 initialProjectileVelocity = npc.Center.DirectionTo(Main.player[npc.target].Center) * skullProjSpeed;
                                int type = ProjectileID.Skull;
                                int skullProjectile = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, initialProjectileVelocity, type, SkullDamage, 0f, Main.myPlayer, -(phase3 ? 2f : 1f));
                                Main.projectile[skullProjectile].timeLeft = 600;
                            }
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
                            Vector2 offset = new Vector2(velocity.X * 1f + Main.rand.Next(-50, 51) * 0.01f, velocity.Y * 1f + Main.rand.Next(-50, 51) * 0.01f).SafeNormalize(Vector2.UnitY);
                            offset *= speed;
                            offset += npc.velocity;
                            velocity.X = offset.X;
                            velocity.Y = offset.Y;
                            int damage = 2500;
                            int projType = ProjectileID.Skull;
                            source += offset * 5f;
                            int skull = Projectile.NewProjectile(npc.GetSource_FromAI(), source, velocity, projType, damage, 0f, Main.myPlayer, -1f);
                            Main.projectile[skull].timeLeft = 600;
                            Main.projectile[skull].tileCollide = false;
                        }
                    }
                }
            }
        }
    }
}
