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
    public static class BrainOfCthulhuAI
    {
        public const float TimeBeforeCreeperAttack = 600f;
        public const float CreeperTelegraphTime = 180f;
        private const float SpinVelocity = 12f;
        private const int SpinRadius = 45;

        // Rev+ exclusive
        public static float ContactDamageMult = 1.15f; // 62 (buffed from 54)
        public static int BloodShotDamage = 12; // 48

        public static bool BuffedBrainofCthulhuAI(NPC npc, Mod mod)
        {
            // whoAmI variable
            NPC.crimsonBoss = npc.whoAmI;

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                // Ignore tank players, target low HP players, Brain is smart
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(npc, options);
            }

            // Despawn check
            bool despawn = (Main.player[npc.target].dead || !Main.player[npc.target].ZoneCrimson) && !BossRushEvent.BossRushActive;

            // Despawn
            if (despawn)
            {
                if (npc.localAI[3] < 120f)
                    npc.localAI[3] += 1f;

                if (npc.localAI[3] > 60f)
                    npc.velocity.Y += (npc.localAI[3] - 60f) * 0.25f;
            }
            else if (npc.localAI[3] > 0f)
                npc.localAI[3] -= 1f;

            // Spawn Creepers
            if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f)
            {
                npc.localAI[0] = 1f;
                int brainOfCthuluCreepersCount = GetBrainOfCthuluCreepersCountRevDeath();
                float attackTimerIncrement = 15f;
                for (int i = 0; i < brainOfCthuluCreepersCount; i++)
                {
                    float brainX = npc.Center.X;
                    float brainY = npc.Center.Y;
                    brainX += Main.rand.Next(-npc.width, npc.width);
                    brainY += Main.rand.Next(-npc.height, npc.height);

                    int creeperSpawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)brainX, (int)brainY, NPCID.Creeper, 0, 0f, i * attackTimerIncrement);
                    Main.npc[creeperSpawn].velocity = new Vector2(Main.rand.Next(-30, 31) * 0.1f, Main.rand.Next(-30, 31) * 0.1f);
                    Main.npc[creeperSpawn].netUpdate = true;
                }
            }

            // Despawn
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 6000f)
                {
                    npc.active = false;
                    npc.life = 0;

                    if (Main.dedServ)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            // Phase 2
            if (npc.ai[0] < 0f)
            {
                if (Main.getGoodWorld)
                    NPC.brainOfGravity = npc.whoAmI;

                // Spawn gore
                if (npc.localAI[2] == 0f)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);

                    npc.localAI[2] = 1f;

                    if (!Main.dedServ)
                    {
                        Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 392, 1f);
                        Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 393, 1f);
                        Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 394, 1f);
                        Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 395, 1f);
                    }

                    for (int j = 0; j < 20; j++)
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                    SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                }

                // Percent life remaining
                float lifeRatio = npc.life / (float)npc.lifeMax;

                // Phases

                // Start spinning, create additional afterimages, shoot projectiles and charging phase
                bool phase3 = lifeRatio < 0.8f;

                // Fire projectiles from 2 locations phase
                bool phase4 = lifeRatio < 0.6f;

                // Fire projectiles from 4 locations phase
                bool phase5 = lifeRatio < 0.4f;

                // Spin faster, shoot faster projectiles and begin teleporting from cardinal directions phase
                bool phase6 = lifeRatio < 0.25f;

                // Super fast spin and charges phase
                bool phase7 = lifeRatio < 0.1f;

                // Spawn Creepers in Death Mode
                if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 1f && death && phase3)
                {
                    npc.localAI[0] = 2f;
                    SpawnDeathModeCreepers();
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 2f && death && phase5)
                {
                    npc.localAI[0] = 3f;
                    SpawnDeathModeCreepers();
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 3f && death && phase7)
                {
                    npc.localAI[0] = 4f;
                    SpawnDeathModeCreepers();
                }

                void SpawnDeathModeCreepers()
                {
                    int brainOfCthuluCreepersCount = GetBrainOfCthuluCreepersCountRevDeath() / 4;
                    float attackTimerIncrement = 15f * 4;
                    for (int i = 0; i < brainOfCthuluCreepersCount; i++)
                    {
                        float brainX = npc.Center.X;
                        float brainY = npc.Center.Y;
                        brainX += Main.rand.Next(-npc.width, npc.width);
                        brainY += Main.rand.Next(-npc.height, npc.height);

                        int creeperSpawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)brainX, (int)brainY, NPCID.Creeper, 0, 0f, i * attackTimerIncrement);
                        Main.npc[creeperSpawn].velocity = new Vector2(Main.rand.Next(-30, 31) * 0.1f, Main.rand.Next(-30, 31) * 0.1f);
                        Main.npc[creeperSpawn].netUpdate = true;
                    }
                }

                // Whether the fucking thing is spinning or not, dipshit
                bool spinning = npc.ai[0] == -4f;

                // Spin variables
                float spinVelocity = SpinVelocity;
                int spinRadius = SpinRadius;
                if (phase7)
                {
                    spinVelocity *= 2f;
                    spinRadius *= 2;
                }
                else if (phase6)
                {
                    spinVelocity *= 1.5f;
                    spinRadius = (int)(SpinRadius * 1.5f);
                }

                // Charge variables
                float chargeVelocity = death ? 19.5f : 15f;
                if (phase7)
                    chargeVelocity *= 1.2f;

                // KnockBack
                float baseKnockBackResist = death ? 0.1f : 0.3f;
                if (!phase3)
                    npc.knockBackResist = GetCrimsonBossKnockBack(npc, CalamityGlobalNPC.GetActivePlayerCount(), lifeRatio, baseKnockBackResist);
                else
                    npc.knockBackResist = 0f;

                // Gain defense while spinning
                npc.defense = npc.defDefense + (spinning ? 7 : 0);

                // Take damage
                npc.dontTakeDamage = false;

                // Target distance X
                float playerLocation = npc.Center.X - Main.player[npc.target].Center.X;

                // Charge
                if (!spinning)
                {
                    // Not charging
                    if (npc.ai[0] != -5f)
                    {
                        // Rubber band movement
                        if (!despawn)
                        {
                            float velocityScale = death ? 6f : 4.5f;
                            float velocityBoost = velocityScale * (1f - lifeRatio);
                            float nonChargeSpeed = (death ? 25.5f : 18f) + velocityBoost;
                            if (Main.getGoodWorld)
                                nonChargeSpeed *= 1.15f;

                            float minInertia = death ? 50f : 75f;
                            float maxInertia = death ? 70f : 100f;
                            float inertia = MathHelper.Lerp(minInertia, maxInertia, lifeRatio);

                            Vector2 destination = Main.player[npc.target].Center + (death ? Main.player[npc.target].velocity * 10f : Vector2.Zero);
                            Vector2 idealVelocity = (destination - npc.Center).SafeNormalize(Vector2.UnitY) * nonChargeSpeed;
                            npc.velocity = (npc.velocity * (inertia - 1f) + idealVelocity) / inertia;
                        }
                    }

                    // Charge, -5
                    else
                    {
                        if (!despawn)
                            npc.ai[1] += 1f;

                        float chargeDistance = 960f; // 60 tile charge distance
                        float chargeDuration = chargeDistance / chargeVelocity;
                        float chargeGateValue = 10f;

                        if (npc.ai[1] < chargeGateValue)
                        {
                            // Avoid cheap bullshit
                            npc.damage = 0;
                        }
                        else
                        {
                            // Set damage
                            npc.damage = (int)Math.Round(npc.defDamage * ContactDamageMult);
                        }

                        // Teleport
                        float timeGateValue = chargeDuration + chargeGateValue;
                        if (npc.ai[1] >= timeGateValue)
                        {
                            npc.ai[0] = -6f;
                            npc.ai[1] = 0f;
                            float maxTeleportPhaseDurationReduction = 60f;
                            float lerpHPScalar = (0.8f - lifeRatio) / 0.8f;
                            npc.localAI[1] = MathHelper.Lerp(0f, maxTeleportPhaseDurationReduction, lerpHPScalar);
                            npc.netUpdate = true;
                        }

                        // Charge sound and velocity
                        else if (npc.ai[1] == chargeGateValue && !despawn)
                        {
                            // Sound
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.Center);

                            // Velocity
                            npc.velocity = (Main.player[npc.target].Center + (death ? Main.player[npc.target].velocity * 10f : Vector2.Zero) - npc.Center).SafeNormalize(Vector2.UnitY) * chargeVelocity;
                            if (Main.getGoodWorld)
                                npc.velocity *= 1.15f;
                        }
                    }
                }

                // Circle around, -4
                if (spinning)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    // Charge sound
                    if (npc.ai[2] == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

                        if (Main.zenithWorld)
                        {
                            if (!Main.dedServ)
                            {
                                if (!Main.LocalPlayer.dead && Main.LocalPlayer.active && Vector2.Distance(Main.LocalPlayer.Center, npc.Center) < CalamityGlobalNPC.CatchUpDistance350Tiles)
                                    Main.LocalPlayer.AddBuff(BuffID.Confused, 90);
                            }
                        }
                    }

                    // Velocity
                    float velocity = MathHelper.TwoPi / spinRadius;
                    npc.velocity = npc.velocity.RotatedBy(-(double)velocity * npc.ai[1]);

                    npc.ai[2] += 1f;

                    float timer = (death ? 0f : 30f) + npc.ai[3];

                    // Move the brain away from the target in order to ensure fairness
                    if (npc.ai[2] >= timer - 5f)
                    {
                        float minChargeDistance = 640f; // 40 tile distance
                        if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) < minChargeDistance)
                        {
                            npc.ai[2] -= 1f;
                            npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * -chargeVelocity;
                            if (death)
                                npc.velocity *= 1.5f;
                            if (Main.getGoodWorld)
                                npc.velocity *= 1.15f;
                        }
                    }

                    // Charge at target
                    if (npc.ai[2] >= timer)
                    {
                        // Shoot projectiles from 4 directions, alternating between diagonal and cardinal
                        float bloodShotVelocity = death ? 8f : 6f;

                        // Scale projectile velocity
                        float phase7ProjectileVelocityMult = 1.2f;
                        float phase6ProjectileVelocityMult = 1.1f;
                        if (phase7)
                            bloodShotVelocity *= phase7ProjectileVelocityMult;
                        else if (phase6)
                            bloodShotVelocity *= phase6ProjectileVelocityMult;

                        if (phase4)
                        {
                            bool alternativeFire = npc.ai[3] % 2f == 0f;
                            bool diagonalShots = alternativeFire || !phase5;
                            bool otherQuadrants = Main.rand.NextBool();
                            int startingIndex = (otherQuadrants && !phase5) ? 2 : 0;
                            int totalProjectileSpreads = (phase5 || startingIndex == 2) ? 4 : 2;
                            for (int i = startingIndex; i < totalProjectileSpreads; i++)
                            {
                                Vector2 position = npc.Center;
                                float distanceFromTargetX = Math.Abs(npc.Center.X - Main.LocalPlayer.Center.X);
                                float distanceFromTargetY = Math.Abs(npc.Center.Y - Main.LocalPlayer.Center.Y);

                                switch (i)
                                {
                                    case 0:

                                        position.X = Main.LocalPlayer.Center.X - distanceFromTargetX;
                                        if (diagonalShots)
                                            position.Y = Main.LocalPlayer.Center.Y - distanceFromTargetY;
                                        else
                                            position.Y = Main.LocalPlayer.Center.Y;

                                        break;

                                    case 1:

                                        position.Y = Main.LocalPlayer.Center.Y - distanceFromTargetY;
                                        if (diagonalShots)
                                            position.X = Main.LocalPlayer.Center.X + distanceFromTargetX;
                                        else
                                            position.X = Main.LocalPlayer.Center.X;

                                        break;

                                    case 2:

                                        position.X = Main.LocalPlayer.Center.X + distanceFromTargetX;
                                        if (diagonalShots)
                                            position.Y = Main.LocalPlayer.Center.Y + distanceFromTargetY;
                                        else
                                            position.Y = Main.LocalPlayer.Center.Y;

                                        break;

                                    case 3:

                                        position.Y = Main.LocalPlayer.Center.Y + distanceFromTargetY;
                                        if (diagonalShots)
                                            position.X = Main.LocalPlayer.Center.X - distanceFromTargetX;
                                        else
                                            position.X = Main.LocalPlayer.Center.X;

                                        break;

                                    default:
                                        break;
                                }

                                Vector2 projectileVelocity = (Main.player[npc.target].Center - position).SafeNormalize(Vector2.UnitY) * bloodShotVelocity;
                                float minFiringDistance = 560f; // 35 tile distance
                                bool firedFromRealBrain = Vector2.Distance(position, npc.Center) < 8f;
                                if (Vector2.Distance(position, Main.player[npc.target].Center) > minFiringDistance && !firedFromRealBrain) // The projectiles can only be fired if the target is more than 15 tiles away from the firing position
                                {
                                    bool canHit = Collision.CanHitLine(position, 1, 1, Main.player[npc.target].Center, 1, 1);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        int type = ProjectileID.BloodShot;
                                        int numProj = death ? 9 : 4;
                                        int spread = death ? 32 : 15;
                                        if (phase7)
                                        {
                                            numProj = death ? 7 : 2;
                                            spread = death ? 25 : 10;
                                        }
                                        else if (phase5)
                                        {
                                            numProj = death ? 8 : 3;
                                            spread = death ? 40 : 20;
                                        }

                                        float rotation = MathHelper.ToRadians(spread);
                                        for (int j = 0; j < numProj; j++)
                                        {
                                            Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, j / (float)(numProj - 1)));
                                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), position + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 10f, perturbedSpeed, type, BloodShotDamage, 0f, Main.myPlayer);
                                            Main.projectile[proj].timeLeft = 600;
                                            if (!canHit)
                                                Main.projectile[proj].tileCollide = false;
                                        }
                                    }
                                }
                            }
                        }

                        Vector2 projectileVelocity2 = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * bloodShotVelocity;
                        bool canHit2 = Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.BloodNautilusShot;
                            int numProj = death ? 13 : 7;
                            int spread = death ? 60 : 40;
                            if (phase7)
                            {
                                numProj = death ? 7 : 2;
                                spread = death ? 23 : 5;
                            }
                            else if (phase5)
                            {
                                numProj = death ? 8 : 2;
                                spread = death ? 30 : 10;
                            }
                            else if (phase4)
                            {
                                numProj = death ? 9 : 3;
                                spread = death ? 30 : 10;
                            }

                            float rotation = MathHelper.ToRadians(spread);
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity2.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 10f, perturbedSpeed, type, BloodShotDamage, 0f, Main.myPlayer);
                                Main.projectile[proj].timeLeft = 600;
                                if (!canHit2)
                                    Main.projectile[proj].tileCollide = false;
                            }
                        }

                        // Complete stop
                        npc.velocity *= 0f;

                        npc.ai[0] = -5f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }
                }

                // Pick teleport location
                else if (npc.ai[0] == -1f || npc.ai[0] == -6f)
                {
                    // Set damage
                    npc.damage = (int)Math.Round(npc.defDamage * ContactDamageMult);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Go to phase 3 and spin
                        if (phase3 && npc.ai[0] == -1f)
                        {
                            // Avoid cheap bullshit
                            npc.damage = 0;

                            // Velocity
                            npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * spinVelocity;
                            npc.ai[0] = -4f;
                            npc.ai[1] = playerLocation < 0 ? 1f : -1f;
                            npc.ai[2] = 0f;

                            int maxRandomTime = phase7 ? (death ? 10 : 30) : (death ? 20 : 60);
                            npc.ai[3] = Main.rand.Next(maxRandomTime) + 1;
                            npc.localAI[1] = 0f;
                            npc.alpha = 0;
                            npc.netUpdate = true;
                        }

                        if (!despawn)
                            npc.localAI[1] += 1f;

                        float teleportGateValue = npc.ai[0] == -6f ? 270f : 210f;
                        if (npc.localAI[1] >= teleportGateValue)
                        {
                            npc.localAI[1] = 0f;
                            
                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            options.finishThemOff = true;
                            CalamityUtils.CalamityTargeting(npc, options);

                            int numTeleportTries = 0;
                            int maxTeleportTries = 100;
                            int teleportTileX;
                            int teleportTileY;
                            bool cardinal = Main.rand.NextBool();
                            bool aboveOrBelow = Main.rand.NextBool();
                            do
                            {
                                numTeleportTries++;
                                teleportTileX = (int)Main.player[npc.target].Center.X / 16;
                                teleportTileY = (int)Main.player[npc.target].Center.Y / 16;

                                int minX = 14;
                                int maxX = 17;
                                int minY = 14;
                                int maxY = 17;
                                if (cardinal && phase6)
                                {
                                    if (aboveOrBelow)
                                    {
                                        minX = 0;
                                        maxX = 3;
                                        minY = 21;
                                        maxY = 24;
                                    }
                                    else
                                    {
                                        minX = 21;
                                        maxX = 24;
                                        minY = 0;
                                        maxY = 3;
                                    }
                                }

                                int teleportX = Main.rand.Next(minX, maxX + 1);
                                int teleportY = Main.rand.Next(minY, maxY + 1);

                                if (Main.rand.NextBool())
                                    teleportX *= -1;

                                if (Main.rand.NextBool())
                                    teleportY *= -1;

                                teleportTileX += teleportX;
                                teleportTileY += teleportY;

                                if (numTeleportTries > maxTeleportTries || !WorldGen.SolidTile(teleportTileX, teleportTileY))
                                {
                                    // Avoid cheap bullshit
                                    npc.damage = 0;

                                    npc.ai[3] = 0f;
                                    npc.ai[0] = npc.ai[0] == -6f ? -7f : -2f;
                                    npc.ai[1] = teleportTileX;
                                    npc.ai[2] = teleportTileY;
                                    npc.ForceNetUpdate();

                                    break;
                                }
                                else
                                {
                                    cardinal = Main.rand.NextBool();
                                    aboveOrBelow = Main.rand.NextBool();
                                }
                            }
                            while (numTeleportTries <= maxTeleportTries);
                        }
                    }
                }

                // Teleport and turn invisible
                else if (npc.ai[0] == -2f || npc.ai[0] == -7f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.velocity *= 0.9f;

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        npc.ai[3] += 15f;
                    else
                        npc.ai[3] += 25f;

                    if (npc.ai[3] >= 255f)
                    {
                        npc.ai[3] = 255f;
                        npc.position.X = npc.ai[1] * 16f - (npc.width / 2);
                        npc.position.Y = npc.ai[2] * 16f - (npc.height / 2);
                        SoundEngine.PlaySound(SoundID.Item8, npc.Center);
                        npc.ai[0] = npc.ai[0] == -7f ? -8f : -3f;

                        // Move non-charging Creepers to new Brain location
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC creeper = Main.npc[i];
                            if (creeper.active && creeper.type == NPCID.Creeper)
                            {
                                bool creeperCanTeleport = creeper.ai[0] == 0f;
                                if (creeperCanTeleport)
                                {
                                    creeper.position.X = npc.position.X;
                                    creeper.position.Y = npc.position.Y;
                                }
                            }
                        }

                        npc.ForceNetUpdate();
                    }

                    npc.alpha = (int)npc.ai[3];
                }

                // Become visible
                else if (npc.ai[0] == -3f || npc.ai[0] == -8f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        npc.ai[3] -= 15f;
                    else
                        npc.ai[3] -= 25f;

                    if (npc.ai[3] <= 0f)
                    {
                        if (npc.ai[0] == -8f)
                        {
                            npc.velocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * spinVelocity;

                            npc.ai[0] = -4f;
                            npc.ai[1] = playerLocation < 0 ? 1f : -1f;
                            npc.ai[2] = 0f;

                            int maxRandomTime = phase7 ? (death ? 10 : 30) : (death ? 20 : 60);
                            npc.ai[3] = Main.rand.Next(maxRandomTime) + 1;
                        }
                        else
                        {
                            npc.ai[3] = 0f;
                            npc.ai[2] = 0f;
                            npc.ai[1] = 0f;
                            npc.ai[0] = -1f;
                        }
                        npc.ForceNetUpdate();
                    }

                    npc.alpha = (int)npc.ai[3];
                }
            }

            // Phase 1
            else
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                // Creeper count
                int creeperCount = NPC.CountNPCS(NPCID.Creeper);
                if (creeperCount > GetBrainOfCthuluCreepersCountRevDeath())
                    creeperCount = GetBrainOfCthuluCreepersCountRevDeath();

                float creeperRatio = creeperCount / (float)GetBrainOfCthuluCreepersCountRevDeath();
                float velocityScale = MathHelper.Lerp(0f, 2f, 1f - creeperRatio) + (death ? 0.5f : 0f);

                // Check for phase 2
                bool phase2 = creeperCount <= 0;

                // Go to phase 2
                if (phase2)
                {
                    npc.ai[0] = -1f;
                    npc.localAI[1] = 0f;
                    npc.alpha = 0;
                    
                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.aggroRatio = -1f;
                    options.finishThemOff = true;
                    CalamityUtils.CalamityTargeting(npc, options);

                    npc.netUpdate = true;
                    return false;
                }

                // Move towards target
                if (!despawn)
                {
                    Vector2 brainCenterPhase1 = npc.Center;
                    float targetXDistPhase1 = Main.player[npc.target].Center.X - brainCenterPhase1.X;
                    float targetYDistPhase1 = Main.player[npc.target].Center.Y - brainCenterPhase1.Y;
                    float targetDistancePhase1 = (float)Math.Sqrt(targetXDistPhase1 * targetXDistPhase1 + targetYDistPhase1 * targetYDistPhase1);
                    float maxMoveVelocity = (death ? 4f : 1.5f) + velocityScale;
                    if (Main.getGoodWorld)
                        maxMoveVelocity *= 2f;

                    if (targetDistancePhase1 < maxMoveVelocity)
                    {
                        npc.velocity.X = targetXDistPhase1;
                        npc.velocity.Y = targetYDistPhase1;
                    }
                    else
                    {
                        targetDistancePhase1 = maxMoveVelocity / targetDistancePhase1;
                        npc.velocity.X = targetXDistPhase1 * targetDistancePhase1;
                        npc.velocity.Y = targetYDistPhase1 * targetDistancePhase1;
                    }
                }

                // Pick a teleport location
                if (npc.ai[0] == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (!despawn)
                            npc.localAI[1] += (death ? 2f : 1f) + velocityScale;

                        if (npc.localAI[1] >= (death ? 540f : 360f))
                        {
                            // Teleport location
                            npc.localAI[1] = 0f;

                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            options.finishThemOff = true;
                            CalamityUtils.CalamityTargeting(npc, options);
                            
                            int phase1TeleportTries = 0;
                            int maxTeleportTries = 100;
                            int phase1TeleportTileX;
                            int phase1TeleportTileY;
                            do
                            {
                                phase1TeleportTries++;
                                phase1TeleportTileX = (int)Main.player[npc.target].Center.X / 16;
                                phase1TeleportTileY = (int)Main.player[npc.target].Center.Y / 16;

                                int min = 28;
                                int max = 30;

                                if (Main.rand.NextBool())
                                    phase1TeleportTileX += Main.rand.Next(min, max);
                                else
                                    phase1TeleportTileX -= Main.rand.Next(min, max);

                                if (Main.rand.NextBool())
                                    phase1TeleportTileY += Main.rand.Next(min, max);
                                else
                                    phase1TeleportTileY -= Main.rand.Next(min, max);

                                if (phase1TeleportTries > maxTeleportTries || (!WorldGen.SolidTile(phase1TeleportTileX, phase1TeleportTileY) && Collision.CanHit(new Vector2(phase1TeleportTileX * 16, phase1TeleportTileY * 16), 1, 1, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height)))
                                {
                                    npc.ai[0] = 1f;
                                    npc.ai[1] = phase1TeleportTileX;
                                    npc.ai[2] = phase1TeleportTileY;
                                    npc.netUpdate = true;
                                    break;
                                }
                            }
                            while (phase1TeleportTries <= maxTeleportTries);
                        }
                    }
                }

                // Turn invisible and teleport
                else if (npc.ai[0] == 1f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.alpha += 25;
                    if (npc.alpha >= 255)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, npc.Center);
                        npc.alpha = 255;
                        npc.position.X = npc.ai[1] * 16f - (npc.width / 2);
                        npc.position.Y = npc.ai[2] * 16f - (npc.height / 2);
                        npc.ai[0] = 2f;

                        // Move non-charging Creepers to new Brain location
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC creeper = Main.npc[i];
                            if (creeper.active && creeper.type == NPCID.Creeper)
                            {
                                bool creeperCanTeleport = creeper.ai[0] == 0f;
                                if (creeperCanTeleport)
                                {
                                    creeper.position.X = npc.position.X;
                                    creeper.position.Y = npc.position.Y;
                                }
                            }
                        }
                    }
                }

                // Become visible
                else if (npc.ai[0] == 2f)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    npc.alpha -= 25;
                    if (npc.alpha <= 0)
                    {
                        npc.alpha = 0;
                        npc.ai[0] = 0f;
                    }
                }
            }

            return false;
        }

        public static bool BuffedCreeperAI(NPC npc, Mod mod)
        {
            // Despawn if Brain is gone
            if (NPC.crimsonBoss < 0)
            {
                npc.active = false;
                npc.netUpdate = true;
                return false;
            }

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                CalamityUtils.CalamityTargeting(npc, default);

            bool brainIsNotTeleportingOrCharging = Main.npc[NPC.crimsonBoss].ai[0] == 0f || Main.npc[NPC.crimsonBoss].ai[0] == -1f || Main.npc[NPC.crimsonBoss].ai[0] == -6f;
            bool brainIsInPhase2 = Main.npc[NPC.crimsonBoss].ai[0] < 0f;

            // Creeper count
            int creeperCount = NPC.CountNPCS(npc.type);
            if (creeperCount > GetBrainOfCthuluCreepersCountRevDeath())
                creeperCount = GetBrainOfCthuluCreepersCountRevDeath();

            float creeperRatio;
            if (death && brainIsInPhase2)
            {
                bool brainIsInPhase3 = Main.npc[NPC.crimsonBoss].localAI[0] == 2f;
                bool brainIsInPhase5 = Main.npc[NPC.crimsonBoss].localAI[0] == 3f;
                bool brainIsInPhase7 = Main.npc[NPC.crimsonBoss].localAI[0] == 4f;
                float creeperAmountScalar = (float)(GetBrainOfCthuluCreepersCountRevDeath() / 4);
                creeperRatio = creeperCount / (creeperAmountScalar * (Main.npc[NPC.crimsonBoss].localAI[0] - 1f));
            }
            else
                creeperRatio = creeperCount / (float)GetBrainOfCthuluCreepersCountRevDeath();

            // Scale the aggressiveness of the charges with amount of Creepers remaining
            float chargeAggressionScale = creeperRatio <= 0.1f ? 1.75f : creeperRatio <= 0.2f ? 1.25f : creeperRatio <= 0.4f ? 0.875f : creeperRatio <= 0.6f ? 0.5f : creeperRatio <= 0.8f ? 0.25f : 0f;
            if (death)
                chargeAggressionScale *= 1.875f;

            // Give off blood dust before charging
            float beginTelegraphGateValue = TimeBeforeCreeperAttack - CreeperTelegraphTime;
            bool showTelegraph = npc.ai[1] >= beginTelegraphGateValue || npc.ai[0] == 1f;
            if (showTelegraph)
            {
                float dustScalar = npc.ai[0] == 1f ? 1f : MathHelper.Clamp((npc.ai[1] - beginTelegraphGateValue) / CreeperTelegraphTime, 0f, 1f);
                int dustAmt = 1 + (int)Math.Round(4 * dustScalar);
                Color dustColor = new Color(255, 50, 50, 0) * dustScalar;
                for (int i = 0; i < dustAmt; i++)
                {
                    Dust dust = Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, npc.velocity.X, npc.velocity.Y, 100, dustColor)];
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = 1.2f;
                }
            }

            // Stay near Brain
            if (npc.ai[0] == 0f)
            {
                // Avoid cheap bullshit
                npc.damage = 0;

                Vector2 creeperCenter = npc.Center;
                float brainXDist = Main.npc[NPC.crimsonBoss].Center.X - creeperCenter.X;
                float brainYDist = Main.npc[NPC.crimsonBoss].Center.Y - creeperCenter.Y;
                float brainDistance = (float)Math.Sqrt(brainXDist * brainXDist + brainYDist * brainYDist);
                float velocity = (death ? 17f : 8f) + chargeAggressionScale;
                if (brainIsInPhase2)
                {
                    float velocityFloor = Main.npc[NPC.crimsonBoss].velocity.Length() + velocity * 0.5f;
                    if (velocity < velocityFloor)
                        velocity = velocityFloor;
                }

                float maxDistanceFromBrain = 90f;
                if (brainDistance > maxDistanceFromBrain)
                {
                    brainDistance = velocity / brainDistance;
                    brainXDist *= brainDistance;
                    brainYDist *= brainDistance;
                    float inertia = brainIsInPhase2 ? 5f : 15f;
                    npc.velocity.X = (npc.velocity.X * inertia + brainXDist) / (inertia + 1f);
                    npc.velocity.Y = (npc.velocity.Y * inertia + brainYDist) / (inertia + 1f);
                }

                // Increase speed
                if (npc.velocity.Length() < velocity)
                    npc.velocity *= 1.05f;

                // Set alpha to brain alpha
                npc.alpha = Main.npc[NPC.crimsonBoss].alpha;

                // Only increment the attack timer if the brain isn't teleporting
                if (brainIsNotTeleportingOrCharging)
                    npc.ai[1] += (brainIsInPhase2 ? 2f : 1f) + chargeAggressionScale;

                // Charge at target
                if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[1] >= TimeBeforeCreeperAttack)
                {
                    npc.ai[1] = 0f;

                    CalamityUtils.CalamityTargeting(npc, default);
                    
                    creeperCenter = npc.Center;
                    brainXDist = Main.player[npc.target].Center.X - creeperCenter.X;
                    brainYDist = Main.player[npc.target].Center.Y - creeperCenter.Y;
                    brainDistance = (float)Math.Sqrt(brainXDist * brainXDist + brainYDist * brainYDist);
                    brainDistance = velocity / brainDistance;
                    npc.velocity.X = brainXDist * brainDistance;
                    npc.velocity.Y = brainYDist * brainDistance;
                    npc.ai[0] = 1f;
                    npc.netUpdate = true;
                }
            }

            // Charge
            else
            {
                // Always fully visible while charging
                npc.alpha = 0;

                float chargeVelocity = (death ? 13f : 6f) + chargeAggressionScale;
                float returnToBrainGateValue = 1f;
                if (!brainIsInPhase2)
                {
                    // Set damage
                    npc.damage = (int)Math.Round(npc.defDamage * ContactDamageMult);

                    Vector2 destination = Main.player[npc.target].Center + (death ? Main.player[npc.target].velocity * 20f : Vector2.Zero);
                    Vector2 targetDirection = destination - npc.Center;
                    targetDirection = targetDirection.SafeNormalize(Vector2.UnitY);
                    if (Main.getGoodWorld)
                    {
                        targetDirection *= chargeVelocity + 6f;
                        npc.velocity = (npc.velocity * 49f + targetDirection) / 50f;
                    }
                    else
                    {
                        targetDirection *= chargeVelocity;
                        float inertia = death ? 75f : 100f;
                        npc.velocity = (npc.velocity * (inertia - 1f) + targetDirection) / inertia;
                    }

                    // Return to Brain after a set time
                    float chargeDistance = death ? 900f : 600f;
                    returnToBrainGateValue = chargeDistance / chargeVelocity;
                }

                npc.ai[1] += 1f;
                if (npc.ai[1] >= returnToBrainGateValue || brainIsInPhase2)
                {
                    // Avoid cheap bullshit
                    npc.damage = 0;

                    // Shoot blood shots
                    if (Vector2.Distance(npc.Center, Main.player[npc.target].Center) > 160f && npc.ai[2] == 0f)
                    {
                        bool canHit = Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1);
                        Vector2 projectileVelocity = (Main.player[npc.target].Center - npc.Center).SafeNormalize(Vector2.UnitY) * chargeVelocity;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int projectileType = ProjectileID.BloodShot;
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, projectileVelocity, projectileType, BloodShotDamage, 0f, Main.myPlayer);
                            Main.projectile[proj].timeLeft = 600;
                            if (!canHit)
                                Main.projectile[proj].tileCollide = false;
                        }
                        npc.ai[2] = 1f;
                    }

                    if (brainIsNotTeleportingOrCharging)
                    {
                        npc.ai[0] = 0f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.netUpdate = true;
                    }
                }
            }

            return false;
        }

        public static int GetBrainOfCthuluCreepersCountRevDeath()
        {
            return Main.getGoodWorld ? 40 : (CalamityWorld.death || BossRushEvent.BossRushActive) ? 30 : 20;
        }

        private static float GetCrimsonBossKnockBack(NPC npc, int numPlayers, float lifeScale, float baseKnockBackResist)
        {
            float balance = 1f;
            float boost = 0.35f;

            for (int i = 1; i < numPlayers; i++)
            {
                balance += boost;
                boost += (1f - boost) / 3f;
            }

            if (balance > 8f)
                balance = (balance * 2f + 8f) / 3f;
            if (balance > 1000f)
                balance = 1000f;

            float KBResist = baseKnockBackResist * lifeScale;
            float KBResistMultiplier = 1f - baseKnockBackResist * 0.4f;
            for (float num = 1f; num < balance; num += 0.34f)
            {
                if (KBResist < 0.05)
                {
                    KBResist = 0f;
                    break;
                }
                KBResist *= KBResistMultiplier;
            }

            return KBResist;
        }
    }
}
