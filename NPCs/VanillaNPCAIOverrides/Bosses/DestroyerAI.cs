using System;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class DestroyerAI
    {
        public const float DRIncreaseTime = 600f;
        public const float DeathModeLaserBreathGateValue = 600f;
        public const float LaserTelegraphTime = 120f;
        public const float SparkTelegraphTime = 30f;
        public const float FlightPhaseGateValue = 900f;
        public const float FlightPhaseResetGateValue = FlightPhaseGateValue * 2f;
        private const float Phase4FlightPhaseTimerSetValue = FlightPhaseGateValue * 0.5f;
        private const float Phase5FlightPhaseTimerSetValue = FlightPhaseGateValue;
        public const float PhaseTransitionTelegraphTime = 180f;
        public const float GroundTelegraphStartGateValue = FlightPhaseResetGateValue - PhaseTransitionTelegraphTime;
        public const float FlightTelegraphStartGateValue = FlightPhaseGateValue - PhaseTransitionTelegraphTime;

        public const float ProbeLaserGateValue_Mechdusa = 360f;
        public const float ProbeLaserGateValue_Rev = 240f;
        public const float ProbeLaserGateValue = 120f;
        public const float ProbeLaserTelegraphTime = 60f;

        // Vanilla values
        public static int ProbeLaserDamage = 22; // 88

        // Rev+ exclusive
        public static int LaserDamage = 25; // 100 (buffed); Applies to all Rev+ lasers

        public static bool BuffedDestroyerAI(NPC npc, Mod mod)
        {
            int mechdusaCurvedSpineSegmentIndex = 0;
            int mechdusaCurvedSpineSegments = 10;
            if (NPC.IsMechQueenUp && npc.type != NPCID.TheDestroyer)
            {
                int mechdusaIndex = (int)npc.ai[1];
                while (mechdusaIndex > 0 && mechdusaIndex < Main.maxNPCs)
                {
                    if (Main.npc[mechdusaIndex].active && Main.npc[mechdusaIndex].type >= NPCID.TheDestroyer && Main.npc[mechdusaIndex].type <= NPCID.TheDestroyerTail)
                    {
                        mechdusaCurvedSpineSegmentIndex++;
                        if (Main.npc[mechdusaIndex].type == NPCID.TheDestroyer)
                            break;

                        if (mechdusaCurvedSpineSegmentIndex >= mechdusaCurvedSpineSegments)
                        {
                            mechdusaCurvedSpineSegmentIndex = 0;
                            break;
                        }

                        mechdusaIndex = (int)Main.npc[mechdusaIndex].ai[1];
                        continue;
                    }

                    mechdusaCurvedSpineSegmentIndex = 0;
                    break;
                }
            }

            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = calamityGlobalNPC.newAI[1] < DRIncreaseTime;

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Phases based on life percentage
            bool phase2 = lifeRatio < 0.85f || death;
            bool phase3 = lifeRatio < 0.7f || death;
            bool startFlightPhase = lifeRatio < 0.5f;
            bool phase4 = lifeRatio < (death ? 0.4f : 0.25f);
            bool phase5 = lifeRatio < (death ? 0.2f : 0.1f);

            // Flight timer
            if (startFlightPhase)
                calamityGlobalNPC.newAI[3] += 1f;

            // Force the timer to be at a certain value in later phases
            float flightPhaseTimerSetValue = phase5 ? Phase5FlightPhaseTimerSetValue : phase4 ? Phase4FlightPhaseTimerSetValue : 0f;
            if (calamityGlobalNPC.newAI[3] < flightPhaseTimerSetValue)
                calamityGlobalNPC.newAI[3] = flightPhaseTimerSetValue;

            // Return to ground phase, with less time spent in later phases
            if (calamityGlobalNPC.newAI[3] >= FlightPhaseResetGateValue)
                calamityGlobalNPC.newAI[3] = flightPhaseTimerSetValue;

            // Spawn DR check
            bool hasSpawnDR = calamityGlobalNPC.newAI[1] < DRIncreaseTime && calamityGlobalNPC.newAI[1] > 60f;

            // Gradual color transition from ground to flight and vice versa
            // 0f = Red, 1f = Purple
            float phaseTransitionColorAmount = (hasSpawnDR || phase5) ? 1f : 0f;
            if (!hasSpawnDR && !phase5)
            {
                if (calamityGlobalNPC.newAI[3] >= GroundTelegraphStartGateValue)
                    phaseTransitionColorAmount = MathHelper.Clamp(1f - (calamityGlobalNPC.newAI[3] - GroundTelegraphStartGateValue) / PhaseTransitionTelegraphTime, 0f, 1f);
                else if (calamityGlobalNPC.newAI[3] >= FlightTelegraphStartGateValue)
                    phaseTransitionColorAmount = MathHelper.Clamp((calamityGlobalNPC.newAI[3] - FlightTelegraphStartGateValue) / PhaseTransitionTelegraphTime, 0f, 1f);
            }

            // Set worm variable for worms
            if (npc.ai[3] > 0f)
                npc.realLife = (int)npc.ai[3];

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

            Player player = Main.player[npc.target];

            bool increaseSpeed = Vector2.Distance(player.Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles;
            bool increaseSpeedMore = Vector2.Distance(player.Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles;

            // Phase for flying at the player
            bool flyAtTarget = (calamityGlobalNPC.newAI[3] >= FlightPhaseGateValue && startFlightPhase) || hasSpawnDR;

            // Dust on spawn and alpha effects
            if (npc.type == NPCID.TheDestroyer || (npc.type != NPCID.TheDestroyer && Main.npc[(int)npc.ai[1]].alpha < 128))
            {
                if (npc.alpha != 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int spawnDust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.TheDestroyer, 0f, 0f, 100, default, 2f);
                        Main.dust[spawnDust].noGravity = true;
                        Main.dust[spawnDust].noLight = true;
                    }
                }

                npc.alpha -= 42;
                if (npc.alpha < 0)
                    npc.alpha = 0;
            }

            // Check if other segments are still alive, if not, die
            if (npc.type > NPCID.TheDestroyer)
            {
                bool shouldDespawn = true;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.TheDestroyer)
                    {
                        shouldDespawn = false;
                        break;
                    }
                }
                if (!shouldDespawn)
                {
                    if (npc.ai[1] <= 0f)
                        shouldDespawn = true;
                    else if (Main.npc[(int)npc.ai[1]].life <= 0)
                        shouldDespawn = true;
                }
                if (shouldDespawn)
                {
                    npc.life = 0;
                    npc.HitEffect(0, 10.0);
                    npc.checkDead();
                    npc.active = false;
                }
            }

            // Total segment variable
            int totalSegments = Main.getGoodWorld ? 100 : 80;

            // Calculate aggression based on how many broken segments there are
            float brokenSegmentAggressionMultiplier = 1f;
            if (npc.type == NPCID.TheDestroyer)
            {
                int numProbeSegments = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.TheDestroyerBody && Main.npc[i].ai[2] == 0f)
                        numProbeSegments++;
                }
                brokenSegmentAggressionMultiplier += (1f - MathHelper.Clamp(numProbeSegments / (float)totalSegments, 0f, 1f)) * 0.25f;
            }

            // Death Mode laser spit bool
            bool spitLaserSpreads = death;

            // Height of the box used to calculate whether The Destroyer should fly at its target or not
            int noFlyZoneBoxHeight = death ? 1500 : 1800;

            // Speed and movement variables
            float speed = death ? 0.2f : 0.1f;
            float turnSpeed = death ? 0.3f : 0.15f;

            // Max velocity
            float segmentVelocity = flyAtTarget ? (death ? 22.5f : 15f) : (death ? 30f : 20f);

            // Increase velocity based on distance
            float velocityMultiplier = increaseSpeedMore ? 2f : increaseSpeed ? 1.5f : 1f;
            noFlyZoneBoxHeight -= death ? 400 : (int)(400f * (1f - lifeRatio));

            float segmentVelocityBoost = death ? (flyAtTarget ? 4.5f : 6f) * (1f - lifeRatio) : (flyAtTarget ? 3f : 4f) * (1f - lifeRatio);
            float speedBoost = death ? (flyAtTarget ? 0.1125f : 0.15f) * (1f - lifeRatio) : (flyAtTarget ? 0.075f : 0.1f) * (1f - lifeRatio);
            float turnSpeedBoost = death ? 0.18f * (1f - lifeRatio) : 0.12f * (1f - lifeRatio);

            segmentVelocity += segmentVelocityBoost;
            speed += speedBoost;
            turnSpeed += turnSpeedBoost;

            if (flyAtTarget)
            {
                float speedMultiplier = phase5 ? 1.8f : phase4 ? 1.65f : 1.5f;
                speed *= speedMultiplier;
            }

            segmentVelocity *= velocityMultiplier;
            speed *= velocityMultiplier;
            turnSpeed *= velocityMultiplier;

            segmentVelocity *= brokenSegmentAggressionMultiplier;
            speed *= brokenSegmentAggressionMultiplier;
            turnSpeed *= brokenSegmentAggressionMultiplier;

            if (Main.getGoodWorld)
            {
                segmentVelocity *= 1.2f;
                speed *= 1.2f;
                turnSpeed *= 1.2f;
            }

            bool probeLaunched = npc.ai[2] == 1f;
            if (npc.type == NPCID.TheDestroyerBody)
            {
                // Regenerate Probes in Death Mode if the number of Probes is less than 40 and the number of living NPCs is less than the segment count + 40 (this limit is here just in case)
                if (death && probeLaunched)
                {
                    npc.localAI[2] += 1f;
                    if (npc.localAI[2] >= 600f)
                    {
                        int maxProbes = 40;
                        bool regenerateProbeSegment = NPC.CountNPCS(NPCID.Probe) < maxProbes;
                        if (regenerateProbeSegment)
                        {
                            int maxNPCs = totalSegments + maxProbes;
                            int numNPCs = 0;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                if (Main.npc[i].active)
                                {
                                    numNPCs++;
                                    if (numNPCs >= maxNPCs)
                                    {
                                        regenerateProbeSegment = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (regenerateProbeSegment)
                        {
                            npc.ai[2] = 0f;
                            npc.netUpdate = true;
                        }

                        npc.localAI[2] = 0f;
                        npc.SyncVanillaLocalAI();
                    }
                }
            }

            if (npc.type == NPCID.TheDestroyer)
            {
                // Spawn segments from head
                if (npc.ai[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.ai[3] = npc.whoAmI;
                    npc.realLife = npc.whoAmI;
                    int index = npc.whoAmI;
                    for (int j = 0; j <= totalSegments; j++)
                    {
                        int type = NPCID.TheDestroyerBody;
                        if (j == totalSegments)
                            type = NPCID.TheDestroyerTail;

                        int segment = NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.Center.X), (int)(npc.position.Y + npc.height), type, npc.whoAmI);
                        Main.npc[segment].ai[3] = npc.whoAmI;
                        Main.npc[segment].realLife = npc.whoAmI;
                        Main.npc[segment].ai[1] = index;
                        Main.npc[index].ai[0] = segment;
                        Main.npc[index].Calamity().newAI[0] = -90f - Main.npc[index].ai[0] * (death ? 8f : 3f); // This controls the delay between laser shots
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, segment);
                        index = segment;
                    }
                }

                // Laser breath in Death Mode
                if (spitLaserSpreads)
                {
                    // Set laser color and type
                    if (calamityGlobalNPC.destroyerLaserColor == -1)
                    {
                        calamityGlobalNPC.destroyerLaserColor = phase3 ? 3 : phase2 ? 2 : 1;
                        npc.SyncDestroyerLaserColor();
                    }

                    float laserBreathGateValue = DeathModeLaserBreathGateValue;
                    if (calamityGlobalNPC.newAI[0] < laserBreathGateValue)
                        calamityGlobalNPC.newAI[0] += 1f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Sync newAI every 20 frames for the new telegraph
                        if (calamityGlobalNPC.newAI[0] % 20f == 10f)
                            npc.SyncExtraAI();
                    }

                    if ((player.Center - npc.Center).SafeNormalize(Vector2.UnitY).ToRotation().AngleTowards(npc.velocity.ToRotation(), MathHelper.PiOver4) == npc.velocity.ToRotation() &&
                        calamityGlobalNPC.newAI[0] >= laserBreathGateValue && Vector2.Distance(npc.Center, player.Center) > 480f &&
                        Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height))
                    {
                        if (calamityGlobalNPC.newAI[0] % 30f == 0f)
                        {
                            float velocity = death ? 5.333f : 5f;
                            int type = ProjectileID.DeathLaser;
                            switch (calamityGlobalNPC.destroyerLaserColor)
                            {
                                default:
                                case 0:
                                    break;

                                case 1:
                                    type = ModContent.ProjectileType<DestroyerCursedLaser>();
                                    break;

                                case 2:
                                    type = ModContent.ProjectileType<DestroyerElectricLaser>();
                                    break;
                            }

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 projectileVelocity = (player.Center - npc.Center).SafeNormalize(Vector2.UnitY) * velocity;
                                int numProj = calamityGlobalNPC.newAI[0] % 60f == 0f ? (death ? 9 : 7) : (death ? 6 : 4);
                                int spread = death ? 38 : 26;
                                float rotation = MathHelper.ToRadians(spread);
                                for (int i = 0; i < numProj; i++)
                                {
                                    Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                    int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 100f, perturbedSpeed, type, LaserDamage.CalculateMechDamage(), 0f, Main.myPlayer, 1f, 0f);
                                    Main.projectile[proj].timeLeft = 1200;
                                }
                            }
                        }

                        calamityGlobalNPC.newAI[0] += 1f;
                        if (calamityGlobalNPC.newAI[0] > laserBreathGateValue + 60f)
                        {
                            calamityGlobalNPC.newAI[0] = 0f;
                            calamityGlobalNPC.destroyerLaserColor = -1;
                            npc.SyncDestroyerLaserColor();
                            npc.SyncExtraAI();
                        }
                    }
                }
            }

            // Fire lasers
            if (npc.type == NPCID.TheDestroyerBody)
            {
                bool ableToFireLaser = calamityGlobalNPC.destroyerLaserColor != -1;

                // Set laser color and type
                if (calamityGlobalNPC.destroyerLaserColor == -1 && !probeLaunched)
                {
                    int random = phase3 ? 4 : phase2 ? 3 : 2;
                    switch (Main.rand.Next(random))
                    {
                        case 0:
                        case 1:
                            calamityGlobalNPC.destroyerLaserColor = 0;
                            break;
                        case 2:
                            calamityGlobalNPC.destroyerLaserColor = 1;
                            break;
                        case 3:
                            calamityGlobalNPC.destroyerLaserColor = 2;
                            break;
                    }

                    npc.SyncDestroyerLaserColor();
                }

                if (probeLaunched && ableToFireLaser)
                {
                    calamityGlobalNPC.destroyerLaserColor = -1;
                    npc.SyncDestroyerLaserColor();
                }

                // Laser rate of fire
                float shootProjectileTime = death ? (phase5 ? 180f : phase4 ? 270f : 360f) : 450f;
                if (ableToFireLaser)
                    calamityGlobalNPC.newAI[0] += 1f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Sync newAI every 20 frames for the new telegraph
                    if (calamityGlobalNPC.newAI[0] % 20f == 10f && ableToFireLaser)
                        npc.SyncExtraAI();
                }

                Color telegraphColor = Color.Transparent;
                switch (calamityGlobalNPC.destroyerLaserColor)
                {
                    case 0:
                        telegraphColor = Color.Red;
                        break;
                    case 1:
                        telegraphColor = Color.Green;
                        break;
                    case 2:
                        telegraphColor = Color.Cyan;
                        break;
                }

                if (calamityGlobalNPC.newAI[0] == shootProjectileTime - LaserTelegraphTime)
                {
                    Particle telegraph = new DestroyerReticleTelegraph(
                        npc,
                        telegraphColor,
                        1.5f,
                        0.15f,
                        (int)LaserTelegraphTime);
                    GeneralParticleHandler.SpawnParticle(telegraph);
                }

                if (calamityGlobalNPC.newAI[0] == shootProjectileTime - SparkTelegraphTime)
                {
                    Particle spark = new DestroyerSparkTelegraph(
                        npc,
                        telegraphColor * 2f,
                        Color.White,
                        3f,
                        30,
                        Main.rand.NextFloat(MathHelper.ToRadians(3f)) * Main.rand.NextBool().ToDirectionInt());
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // Shoot lasers
                // Shoot nothing if probe has been launched
                if (calamityGlobalNPC.newAI[0] >= shootProjectileTime && ableToFireLaser)
                {
                    int numProbeSegments = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == npc.type && Main.npc[i].ai[2] == 0f)
                            numProbeSegments++;
                    }
                    float lerpAmount = MathHelper.Clamp(numProbeSegments / (float)totalSegments, 0f, 1f);
                    float laserShootTimeBonus = (int)MathHelper.Lerp(0f, shootProjectileTime - LaserTelegraphTime, 1f - lerpAmount);
                    // Controls the fire rate getting slower as health lowers
                    if (!death)
                    {
                        if (npc.localAI[3] == 0f && phase3)
                        {
                            npc.localAI[3] = 1f;
                            npc.SyncVanillaLocalAI();
                            laserShootTimeBonus -= npc.ai[0] * 2f;
                        }
                        else if (npc.localAI[3] == 1f && startFlightPhase)
                        {
                            npc.localAI[3] = 2f;
                            npc.SyncVanillaLocalAI();
                            laserShootTimeBonus -= npc.ai[0] * 2f;
                        }
                    }
                    calamityGlobalNPC.newAI[0] = laserShootTimeBonus;
                    npc.SyncExtraAI();
                    CalamityUtils.CalamityTargeting(npc, CalamityTargetingParameters.BossDefaults);

                    if (Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height))
                    {
                        // Laser speed
                        float projectileSpeed = death ? 5f : 4f;

                        // Set projectile damage and type
                        int projectileType = ProjectileID.DeathLaser;
                        switch (calamityGlobalNPC.destroyerLaserColor)
                        {
                            default:
                            case 0:
                                break;

                            case 1:
                                projectileType = ModContent.ProjectileType<DestroyerCursedLaser>();
                                break;

                            case 2:
                                projectileType = ModContent.ProjectileType<DestroyerElectricLaser>();
                                break;
                        }

                        // Get target vector
                        Vector2 projectileVelocity = (player.Center - npc.Center).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                        Vector2 projectileSpawn = npc.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 100f;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), projectileSpawn, projectileVelocity, projectileType, LaserDamage.CalculateMechDamage(), 0f, Main.myPlayer, 1f, 0f);
                            Main.projectile[proj].timeLeft = 1200;
                        }

                        npc.netUpdate = true;

                        if (death)
                        {
                            calamityGlobalNPC.destroyerLaserColor = -1;
                            npc.SyncDestroyerLaserColor();
                        }
                    }

                    if (!death)
                    {
                        calamityGlobalNPC.destroyerLaserColor = -1;
                        npc.SyncDestroyerLaserColor();
                    }
                }
            }

            if (npc.type == NPCID.TheDestroyer)
            {
                if (npc.life > Main.npc[(int)npc.ai[0]].life)
                    npc.life = Main.npc[(int)npc.ai[0]].life;
            }
            else
            {
                if (npc.life > Main.npc[(int)npc.ai[1]].life)
                    npc.life = Main.npc[(int)npc.ai[1]].life;
            }

            int tilePosX = (int)(npc.position.X / 16f) - 1;
            int tileWidthPosX = (int)((npc.position.X + npc.width) / 16f) + 2;
            int tilePosY = (int)(npc.position.Y / 16f) - 1;
            int tileWidthPosY = (int)((npc.position.Y + npc.height) / 16f) + 2;

            if (tilePosX < 0)
                tilePosX = 0;
            if (tileWidthPosX > Main.maxTilesX)
                tileWidthPosX = Main.maxTilesX;
            if (tilePosY < 0)
                tilePosY = 0;
            if (tileWidthPosY > Main.maxTilesY)
                tileWidthPosY = Main.maxTilesY;

            // Fly or not
            bool shouldFly = flyAtTarget;
            if (!shouldFly)
            {
                for (int k = tilePosX; k < tileWidthPosX; k++)
                {
                    for (int l = tilePosY; l < tileWidthPosY; l++)
                    {
                        if (Main.tile[k, l] != null && ((Main.tile[k, l].HasUnactuatedTile && (Main.tileSolid[Main.tile[k, l].TileType] || (Main.tileSolidTop[Main.tile[k, l].TileType] && Main.tile[k, l].TileFrameY == 0))) || Main.tile[k, l].LiquidAmount > 64))
                        {
                            Vector2 tileConvertedPosition;
                            tileConvertedPosition.X = k * 16;
                            tileConvertedPosition.Y = l * 16;
                            if (npc.position.X + npc.width > tileConvertedPosition.X && npc.position.X < tileConvertedPosition.X + 16f && npc.position.Y + npc.height > tileConvertedPosition.Y && npc.position.Y < tileConvertedPosition.Y + 16f)
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

                if (npc.type == NPCID.TheDestroyer)
                {
                    Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                    int noFlyZone = 1000;
                    bool outsideNoFlyZone = true;

                    if (npc.position.Y > player.position.Y)
                    {
                        for (int m = 0; m < Main.maxPlayers; m++)
                        {
                            if (Main.player[m].active)
                            {
                                Rectangle noFlyRectangle = new Rectangle((int)Main.player[m].position.X - noFlyZone, (int)Main.player[m].position.Y - noFlyZone, noFlyZone * 2, noFlyZoneBoxHeight);
                                if (rectangle.Intersects(noFlyRectangle))
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
            }
            else
                npc.localAI[1] = 0f;

            if (npc.type != NPCID.TheDestroyerBody || !probeLaunched)
            {
                Vector3 lightColor = Color.Red.ToVector3();

                // Light colors
                Vector3 groundColor = new Vector3(0.3f, 0.1f, 0.05f);
                Vector3 flightColor = new Vector3(0.05f, 0.1f, 0.3f);
                Vector3 segmentColor = Vector3.Lerp(groundColor, flightColor, phaseTransitionColorAmount);
                Vector3 telegraphColor = groundColor;

                // Telegraph for the laser breath and body lasers
                float telegraphProgress = 0f;
                if (calamityGlobalNPC.destroyerLaserColor != -1)
                {
                    if (npc.type == NPCID.TheDestroyer && spitLaserSpreads)
                    {
                        float telegraphGateValue = DeathModeLaserBreathGateValue - LaserTelegraphTime;
                        if (calamityGlobalNPC.newAI[0] > telegraphGateValue)
                        {
                            switch (calamityGlobalNPC.destroyerLaserColor)
                            {
                                default:
                                case 0:
                                    break;

                                case 1:
                                    telegraphColor = new Vector3(0.1f, 0.3f, 0.05f);
                                    break;

                                case 2:
                                    telegraphColor = new Vector3(0.05f, 0.2f, 0.2f);
                                    break;
                            }
                            telegraphProgress = MathHelper.Clamp((calamityGlobalNPC.newAI[0] - telegraphGateValue) / LaserTelegraphTime, 0f, 1f);
                        }
                    }
                    else if (npc.type == NPCID.TheDestroyerBody)
                    {
                        float shootProjectileTime = (CalamityWorld.death || BossRushEvent.BossRushActive) ? 270f : 450f;
                        float telegraphGateValue = shootProjectileTime - LaserTelegraphTime;
                        if (calamityGlobalNPC.newAI[0] > telegraphGateValue)
                        {
                            switch (calamityGlobalNPC.destroyerLaserColor)
                            {
                                default:
                                case 0:
                                    break;

                                case 1:
                                    telegraphColor = new Vector3(0.1f, 0.3f, 0.05f);
                                    break;

                                case 2:
                                    telegraphColor = new Vector3(0.05f, 0.2f, 0.2f);
                                    break;
                            }
                            telegraphProgress = MathHelper.Clamp((calamityGlobalNPC.newAI[0] - telegraphGateValue) / LaserTelegraphTime, 0f, 1f);
                        }
                    }
                }

                Lighting.AddLight(npc.Center, Vector3.Lerp(segmentColor, telegraphColor * 2f, telegraphProgress));
            }

            // Despawn
            if ((player.dead || Main.IsItDay()) && !BossRushEvent.BossRushActive)
            {
                shouldFly = false;
                npc.velocity.Y += 2f;

                if (npc.position.Y > Main.worldSurface * 16D)
                {
                    npc.velocity.Y += 2f;
                    segmentVelocity *= 2f;
                }

                if (npc.position.Y > Main.rockLayer * 16D)
                {
                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        if (Main.npc[n].aiStyle == npc.aiStyle)
                            Main.npc[n].active = false;
                    }
                }
            }

            Vector2 npcCenter = npc.Center;
            float targetTilePosX = player.Center.X;
            float targetTilePosY = player.Center.Y;
            targetTilePosX = (int)(targetTilePosX / 16f) * 16;
            targetTilePosY = (int)(targetTilePosY / 16f) * 16;
            npcCenter.X = (int)(npcCenter.X / 16f) * 16;
            npcCenter.Y = (int)(npcCenter.Y / 16f) * 16;
            targetTilePosX -= npcCenter.X;
            targetTilePosY -= npcCenter.Y;
            float targetTileDist = (float)Math.Sqrt(targetTilePosX * targetTilePosX + targetTilePosY * targetTilePosY);

            if (npc.ai[1] > 0f && npc.ai[1] < Main.npc.Length)
            {
                int mechdusaSegmentScale = (int)(44f * npc.scale);
                try
                {
                    npcCenter = npc.Center;
                    targetTilePosX = Main.npc[(int)npc.ai[1]].Center.X - npcCenter.X;
                    targetTilePosY = Main.npc[(int)npc.ai[1]].Center.Y - npcCenter.Y;
                }
                catch
                {
                }

                if (mechdusaCurvedSpineSegmentIndex > 0)
                {
                    float absoluteTilePosX = (float)mechdusaSegmentScale - (float)mechdusaSegmentScale * (((float)mechdusaCurvedSpineSegmentIndex - 1f) * 0.1f);
                    if (absoluteTilePosX < 0f)
                        absoluteTilePosX = 0f;

                    if (absoluteTilePosX > (float)mechdusaSegmentScale)
                        absoluteTilePosX = mechdusaSegmentScale;

                    targetTilePosY = Main.npc[(int)npc.ai[1]].Center.Y + absoluteTilePosX - npcCenter.Y;
                }

                npc.rotation = (float)Math.Atan2(targetTilePosY, targetTilePosX) + MathHelper.PiOver2;
                targetTileDist = (float)Math.Sqrt(targetTilePosX * targetTilePosX + targetTilePosY * targetTilePosY);
                if (mechdusaCurvedSpineSegmentIndex > 0)
                    mechdusaSegmentScale = mechdusaSegmentScale / mechdusaCurvedSpineSegments * mechdusaCurvedSpineSegmentIndex;

                targetTileDist = (targetTileDist - mechdusaSegmentScale) / targetTileDist;
                targetTilePosX *= targetTileDist;
                targetTilePosY *= targetTileDist;
                npc.velocity = Vector2.Zero;
                npc.position.X += targetTilePosX;
                npc.position.Y += targetTilePosY;
            }
            else
            {
                if (!shouldFly)
                {
                    npc.velocity.Y += 0.15f;
                    if (death && npc.velocity.Y > 0f && Math.Abs(npc.Center.Y - player.Center.Y) > 360f)
                        npc.velocity.Y += 0.05f;

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
                            if (npc.velocity.X < targetTilePosX)
                                npc.velocity.X += speed;
                            else if (npc.velocity.X > targetTilePosX)
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
                    if (npc.soundDelay == 0)
                    {
                        float soundDelay = targetTileDist / 40f;
                        if (soundDelay < 10f)
                            soundDelay = 10f;
                        if (soundDelay > 20f)
                            soundDelay = 20f;

                        npc.soundDelay = (int)soundDelay;
                        SoundEngine.PlaySound(SoundID.WormDig, npc.Center);
                    }

                    targetTileDist = (float)Math.Sqrt(targetTilePosX * targetTilePosX + targetTilePosY * targetTilePosY);
                    float absoluteTilePosX = Math.Abs(targetTilePosX);
                    float absoluteTilePosY = Math.Abs(targetTilePosY);
                    float tileToReachTarget = segmentVelocity / targetTileDist;
                    targetTilePosX *= tileToReachTarget;
                    targetTilePosY *= tileToReachTarget;

                    bool flyWyvernMovement = false;
                    if (flyAtTarget)
                    {
                        float chargeDistance = 600f;
                        if (((npc.velocity.X > 0f && targetTilePosX < 0f) || (npc.velocity.X < 0f && targetTilePosX > 0f) || (npc.velocity.Y > 0f && targetTilePosY < 0f) || (npc.velocity.Y < 0f && targetTilePosY > 0f)) && Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) > speed / 2f && targetTileDist < chargeDistance)
                        {
                            flyWyvernMovement = true;

                            if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < segmentVelocity)
                                npc.velocity *= 1.1f;
                        }

                        if (npc.position.Y > player.position.Y)
                        {
                            flyWyvernMovement = true;

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

                    if (!flyWyvernMovement)
                    {
                        if (!flyAtTarget)
                        {
                            if (((npc.velocity.X > 0f && targetTilePosX > 0f) || (npc.velocity.X < 0f && targetTilePosX < 0f)) && ((npc.velocity.Y > 0f && targetTilePosY > 0f) || (npc.velocity.Y < 0f && targetTilePosY < 0f)))
                            {
                                if (npc.velocity.X < targetTilePosX)
                                    npc.velocity.X += turnSpeed;
                                else if (npc.velocity.X > targetTilePosX)
                                    npc.velocity.X -= turnSpeed;
                                if (npc.velocity.Y < targetTilePosY)
                                    npc.velocity.Y += turnSpeed;
                                else if (npc.velocity.Y > targetTilePosY)
                                    npc.velocity.Y -= turnSpeed;
                            }
                        }

                        if ((npc.velocity.X > 0f && targetTilePosX > 0f) || (npc.velocity.X < 0f && targetTilePosX < 0f) || (npc.velocity.Y > 0f && targetTilePosY > 0f) || (npc.velocity.Y < 0f && targetTilePosY < 0f))
                        {
                            if (npc.velocity.X < targetTilePosX)
                                npc.velocity.X += speed;
                            else if (npc.velocity.X > targetTilePosX)
                                npc.velocity.X -= speed;
                            if (npc.velocity.Y < targetTilePosY)
                                npc.velocity.Y += speed;
                            else if (npc.velocity.Y > targetTilePosY)
                                npc.velocity.Y -= speed;

                            if (Math.Abs(targetTilePosY) < segmentVelocity * 0.2 && ((npc.velocity.X > 0f && targetTilePosX < 0f) || (npc.velocity.X < 0f && targetTilePosX > 0f)))
                            {
                                if (npc.velocity.Y > 0f)
                                    npc.velocity.Y += speed * 2f;
                                else
                                    npc.velocity.Y -= speed * 2f;
                            }
                            if (Math.Abs(targetTilePosX) < segmentVelocity * 0.2 && ((npc.velocity.Y > 0f && targetTilePosY < 0f) || (npc.velocity.Y < 0f && targetTilePosY > 0f)))
                            {
                                if (npc.velocity.X > 0f)
                                    npc.velocity.X += speed * 2f;
                                else
                                    npc.velocity.X -= speed * 2f;
                            }
                        }
                        else if (absoluteTilePosX > absoluteTilePosY)
                        {
                            if (npc.velocity.X < targetTilePosX)
                                npc.velocity.X += speed * 1.1f;
                            else if (npc.velocity.X > targetTilePosX)
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
                            if (npc.velocity.Y < targetTilePosY)
                                npc.velocity.Y += speed * 1.1f;
                            else if (npc.velocity.Y > targetTilePosY)
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

                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + MathHelper.PiOver2;

                if (npc.type == NPCID.TheDestroyer)
                {
                    if (shouldFly)
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

            // Force the fucker to turn around in ground phase in Death
            if (npc.type == NPCID.TheDestroyer && death && !flyAtTarget)
            {
                if (npc.Distance(player.Center) > 2000f)
                    npc.velocity += (player.Center - npc.Center).SafeNormalize(Vector2.UnitY) * turnSpeed;
            }

            if (NPC.IsMechQueenUp && npc.type == NPCID.TheDestroyer)
            {
                NPC nPC = Main.npc[NPC.mechQueen];
                Vector2 mechQueenCenter = nPC.GetMechQueenCenter();
                Vector2 mechdusaSpinningVector = new Vector2(0f, 100f);
                Vector2 spinningpoint = mechQueenCenter + mechdusaSpinningVector;
                float mechdusaRotation = nPC.velocity.X * 0.025f;
                spinningpoint = spinningpoint.RotatedBy(mechdusaRotation, mechQueenCenter);
                npc.position = spinningpoint - npc.Size / 2f + nPC.velocity;
                npc.velocity.X = 0f;
                npc.velocity.Y = 0f;
                npc.rotation = mechdusaRotation * 0.75f + (float)Math.PI;
            }

            // 10 seconds of resistance to prevent spawn killing
            if (calamityGlobalNPC.newAI[1] < DRIncreaseTime && ((npc.position - npc.oldPosition).Length() > 2f || calamityGlobalNPC.newAI[1] > 0f))
                calamityGlobalNPC.newAI[1] += 1f;

            return false;
        }

        public static bool BuffedProbeAI(NPC npc, Mod mod)
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, default);

            NPCAimedTarget targetData = npc.GetTargetData();
            bool targetDead = false;
            if (targetData.Type == NPCTargetType.Player)
                targetDead = Main.player[npc.target].dead;

            float velocity = death ? 8.4f : 7.2f;
            float acceleration = death ? 0.07f : 0.06f;
            float deceleration = 1f - acceleration;

            if (targetDead)
            {
                Vector2 destination = npc.Center - Vector2.UnitY;
                Vector2 idealVelocity = npc.SafeDirectionTo(destination) * velocity * 0.5f;
                idealVelocity.X *= npc.direction;
                idealVelocity.Y *= 2.5f;
                npc.SimpleFlyMovement(idealVelocity, acceleration);
                npc.EncourageDespawn(10);
            }
            else if (npc.Distance(targetData.Center) > 400f)
            {
                Vector2 idealVelocity = npc.SafeDirectionTo(targetData.Center) * velocity;
                npc.SimpleFlyMovement(idealVelocity, acceleration);
            }
            else
            {
                if (npc.Distance(targetData.Center) < 160f)
                {
                    Vector2 idealVelocity = npc.SafeDirectionTo(targetData.Center) * velocity;
                    npc.SimpleFlyMovement(-idealVelocity, acceleration);
                }
                else
                    npc.velocity *= deceleration;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (i != npc.whoAmI && Main.npc[i].active && Main.npc[i].type == npc.type)
                {
                    Vector2 otherProbeDist = Main.npc[i].Center - npc.Center;
                    if (otherProbeDist.Length() < (npc.width + npc.height))
                    {
                        otherProbeDist = otherProbeDist.SafeNormalize(Vector2.UnitY);
                        otherProbeDist *= -0.1f;
                        npc.velocity += otherProbeDist;
                        Main.npc[i].velocity -= otherProbeDist;
                    }
                }
            }

            if (npc.ai[3] != 0f)
            {
                if (NPC.IsMechQueenUp)
                {
                    NPC nPC = Main.npc[NPC.mechQueen];
                    Vector2 tileConvertedPosition = new Vector2(26f * npc.ai[3], 0f);
                    int mechdusaProbe = (int)npc.ai[2];
                    if (mechdusaProbe < 0 || mechdusaProbe >= Main.maxNPCs)
                    {
                        mechdusaProbe = NPC.FindFirstNPC(NPCID.TheDestroyer);
                        npc.ai[2] = mechdusaProbe;
                        npc.netUpdate = true;
                    }

                    if (mechdusaProbe > -1)
                    {
                        NPC nPC2 = Main.npc[mechdusaProbe];
                        if (!nPC2.active || nPC2.type != NPCID.TheDestroyer)
                        {
                            npc.dontTakeDamage = false;
                            if (npc.ai[3] > 0f)
                                npc.netUpdate = true;

                            npc.ai[3] = 0f;
                        }
                        else
                        {
                            Vector2 spinningpoint = nPC2.Center + tileConvertedPosition;
                            spinningpoint = spinningpoint.RotatedBy(nPC2.rotation, nPC2.Center);
                            npc.Center = spinningpoint;
                            npc.velocity = nPC.velocity;
                            npc.dontTakeDamage = true;
                        }
                    }
                    else
                    {
                        npc.dontTakeDamage = false;
                        if (npc.ai[3] > 0f)
                            npc.netUpdate = true;

                        npc.ai[3] = 0f;
                    }
                }
                else
                {
                    npc.dontTakeDamage = false;
                    if (npc.ai[3] > 0f)
                        npc.netUpdate = true;

                    npc.ai[3] = 0f;
                }
            }
            else
                npc.dontTakeDamage = false;

            npc.localAI[0] += 1f;
            if ((npc.justHit && !death) || targetDead)
                npc.localAI[0] = 0f;

            float laserGateValue = NPC.IsMechQueenUp ? ProbeLaserGateValue_Mechdusa : ProbeLaserGateValue_Rev;
            if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] >= laserGateValue)
            {
                npc.localAI[0] = 0f;
                if (targetData.Type != 0 && Collision.CanHit(npc.position, npc.width, npc.height, targetData.Position, targetData.Width, targetData.Height))
                {
                    int type = ProjectileID.PinkLaser;
                    int totalProjectiles = death ? 3 : 1;
                    Vector2 projectileVelocity = (targetData.Center - npc.Center).SafeNormalize(Vector2.UnitY) * velocity;
                    if (NPC.IsMechQueenUp)
                    {
                        Vector2 v = targetData.Center - npc.Center - targetData.Velocity * 20f;
                        projectileVelocity = v.SafeNormalize(Vector2.UnitY) * 8f;
                    }
                    for (int i = 0; i < totalProjectiles; i++)
                    {
                        float velocityMultiplier = 1f;
                        switch (i)
                        {
                            case 0:
                                break;
                            case 1:
                                velocityMultiplier = 0.95f;
                                break;
                            case 2:
                                velocityMultiplier = 0.9f;
                                break;
                        }
                        Vector2 laserVelocity = projectileVelocity * velocityMultiplier;
                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + laserVelocity.SafeNormalize(Vector2.UnitY) * 50f, laserVelocity, type, ProbeLaserDamage.CalculateMechDamage(), 0f, Main.myPlayer);
                    }

                    npc.netUpdate = true;
                }
            }

            int x = (int)npc.Center.X / 16;
            int y = (int)npc.Center.Y / 16;
            if (WorldGen.InWorld(x, y) && !WorldGen.SolidTile(x, y))
                Lighting.AddLight((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f), 0.3f, 0.1f, 0.05f);

            if (targetData.Center.X - npc.Center.X > 0f)
            {
                npc.spriteDirection = 1;
                npc.rotation = (float)Math.Atan2(targetData.Center.Y - npc.Center.Y, targetData.Center.X - npc.Center.X);
            }
            else
            {
                npc.spriteDirection = -1;
                npc.rotation = (float)Math.Atan2(targetData.Center.Y - npc.Center.Y, targetData.Center.X - npc.Center.X) + MathHelper.Pi;
            }

            if (NPC.IsMechQueenUp && npc.ai[2] == 0f)
            {
                Vector2 center = npc.GetTargetData().Center;
                Vector2 v2 = center - npc.Center;
                if (v2.Length() < 120f)
                    npc.Center = center - v2.SafeNormalize(Vector2.UnitY) * 120;
            }

            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
                npc.netUpdate = true;

            return false;
        }
    }
}
