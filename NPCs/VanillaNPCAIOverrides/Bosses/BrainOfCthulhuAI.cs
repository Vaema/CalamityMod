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
    public class BrainOfCthulhuAI : VanillaAIOverride
    {
        public const float TimeBeforeCreeperAttack = 600f;
        public const float CreeperTelegraphTime = 180f;
        private const float SpinVelocity = 12f;
        private const int SpinRadius = 45;

        // Rev+ exclusive
        public static float ContactDamageMult = 1.15f; // 62 (buffed from 54)
        public static int BloodShotDamage = 12; // 48

        public override bool AI(Mod mod)
        {
            // whoAmI variable
            NPC.crimsonBoss = NPC.whoAmI;

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                // Ignore tank players, target low HP players, Brain is smart
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                CalamityUtils.CalamityTargeting(NPC, options);
            }

            // Despawn check
            bool despawn = (Main.player[NPC.target].dead || !Main.player[NPC.target].ZoneCrimson) && !BossRushEvent.BossRushActive;

            // Despawn
            if (despawn)
            {
                if (NPC.localAI[3] < 120f)
                    NPC.localAI[3] += 1f;

                if (NPC.localAI[3] > 60f)
                    NPC.velocity.Y += (NPC.localAI[3] - 60f) * 0.25f;
            }
            else if (NPC.localAI[3] > 0f)
                NPC.localAI[3] -= 1f;

            // Spawn Creepers
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                int brainOfCthuluCreepersCount = GetBrainOfCthuluCreepersCountRevDeath();
                float attackTimerIncrement = 15f;
                for (int i = 0; i < brainOfCthuluCreepersCount; i++)
                {
                    float brainX = NPC.Center.X;
                    float brainY = NPC.Center.Y;
                    brainX += Main.rand.Next(-NPC.width, NPC.width);
                    brainY += Main.rand.Next(-NPC.height, NPC.height);

                    int creeperSpawn = NPC.NewNPC(NPC.GetSource_FromAI(), (int)brainX, (int)brainY, NPCID.Creeper, 0, 0f, i * attackTimerIncrement);
                    Main.npc[creeperSpawn].velocity = new Vector2(Main.rand.Next(-30, 31) * 0.1f, Main.rand.Next(-30, 31) * 0.1f);
                    Main.npc[creeperSpawn].netUpdate = true;
                }
            }

            // Despawn
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 6000f)
                {
                    NPC.active = false;
                    NPC.life = 0;

                    if (Main.dedServ)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            // Phase 2
            if (NPC.ai[0] < 0f)
            {
                if (Main.getGoodWorld)
                    NPC.brainOfGravity = NPC.whoAmI;

                // Spawn gore
                if (NPC.localAI[2] == 0f)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);

                    NPC.localAI[2] = 1f;

                    if (!Main.dedServ)
                    {
                        Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 392, 1f);
                        Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 393, 1f);
                        Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 394, 1f);
                        Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f), 395, 1f);
                    }

                    for (int j = 0; j < 20; j++)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f, 0, default, 1f);

                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                }

                // Percent life remaining
                float lifeRatio = NPC.life / (float)NPC.lifeMax;

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

                // Whether the fucking thing is spinning or not, dipshit
                bool spinning = NPC.ai[0] == -4f;

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
                    NPC.knockBackResist = GetCrimsonBossKnockBack(NPC, CalamityGlobalNPC.GetActivePlayerCount(), lifeRatio, baseKnockBackResist);
                else
                    NPC.knockBackResist = 0f;

                // Gain defense while spinning
                NPC.defense = NPC.defDefense + (spinning ? 7 : 0);

                // Take damage
                NPC.dontTakeDamage = false;

                // Target distance X
                float playerLocation = NPC.Center.X - Main.player[NPC.target].Center.X;

                // Charge
                if (!spinning)
                {
                    // Not charging
                    if (NPC.ai[0] != -5f)
                    {
                        // Rubber band movement
                        if (!despawn)
                        {
                            float velocityScale = death ? 5.5f : 4.5f;
                            float velocityBoost = velocityScale * (1f - lifeRatio);
                            float nonChargeSpeed = (death ? 22f : 18f) + velocityBoost;
                            if (Main.getGoodWorld)
                                nonChargeSpeed *= 1.15f;

                            float minInertia = death ? 50f : 75f;
                            float maxInertia = death ? 70f : 100f;
                            float inertia = MathHelper.Lerp(minInertia, maxInertia, lifeRatio);

                            Vector2 destination = Main.player[NPC.target].Center + (death ? Main.player[NPC.target].velocity * 10f : Vector2.Zero);
                            Vector2 idealVelocity = (destination - NPC.Center).SafeNormalize(Vector2.UnitY) * nonChargeSpeed;
                            NPC.velocity = (NPC.velocity * (inertia - 1f) + idealVelocity) / inertia;
                        }
                    }

                    // Charge, -5
                    else
                    {
                        if (!despawn)
                            NPC.ai[1] += 1f;

                        float chargeDistance = 960f; // 60 tile charge distance
                        float chargeDuration = chargeDistance / chargeVelocity;
                        float chargeGateValue = 10f;

                        if (NPC.ai[1] < chargeGateValue)
                        {
                            // Avoid cheap bullshit
                            NPC.damage = 0;
                        }
                        else
                        {
                            // Set damage
                            NPC.damage = (int)Math.Round(NPC.defDamage * ContactDamageMult);
                        }

                        // Teleport
                        float timeGateValue = chargeDuration + chargeGateValue;
                        if (NPC.ai[1] >= timeGateValue)
                        {
                            NPC.ai[0] = -6f;
                            NPC.ai[1] = 0f;
                            float maxTeleportPhaseDurationReduction = 60f;
                            float lerpHPScalar = (0.8f - lifeRatio) / 0.8f;
                            NPC.localAI[1] = MathHelper.Lerp(0f, maxTeleportPhaseDurationReduction, lerpHPScalar);
                            NPC.netUpdate = true;
                        }

                        // Charge sound and velocity
                        else if (NPC.ai[1] == chargeGateValue && !despawn)
                        {
                            // Sound
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);

                            // Velocity
                            NPC.velocity = (Main.player[NPC.target].Center + (death ? Main.player[NPC.target].velocity * 10f : Vector2.Zero) - NPC.Center).SafeNormalize(Vector2.UnitY) * chargeVelocity;
                            if (Main.getGoodWorld)
                                NPC.velocity *= 1.15f;
                        }
                    }
                }

                // Circle around, -4
                if (spinning)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    // Charge sound
                    if (NPC.ai[2] == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);

                        if (Main.zenithWorld)
                        {
                            if (!Main.dedServ)
                            {
                                if (!Main.LocalPlayer.dead && Main.LocalPlayer.active && Vector2.Distance(Main.LocalPlayer.Center, NPC.Center) < CalamityGlobalNPC.CatchUpDistance350Tiles)
                                    Main.LocalPlayer.AddBuff(BuffID.Confused, 90);
                            }
                        }
                    }

                    // Velocity
                    float velocity = MathHelper.TwoPi / spinRadius;
                    NPC.velocity = NPC.velocity.RotatedBy(-(double)velocity * NPC.ai[1]);

                    NPC.ai[2] += 1f;

                    float timer = (death ? 20f : 30f) + NPC.ai[3];

                    // Move the brain away from the target in order to ensure fairness
                    if (NPC.ai[2] >= timer - 5f)
                    {
                        float minChargeDistance = 640f; // 40 tile distance
                        if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) < minChargeDistance)
                        {
                            NPC.ai[2] -= 1f;
                            NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -chargeVelocity;
                            if (death)
                                NPC.velocity *= 1.2f;
                            if (Main.getGoodWorld)
                                NPC.velocity *= 1.15f;
                        }
                    }

                    // Charge at target
                    if (NPC.ai[2] >= timer)
                    {
                        // Shoot projectiles from 4 directions, alternating between diagonal and cardinal
                        float bloodShotVelocity = death ? 7f : 6f;

                        // Scale projectile velocity
                        float phase7ProjectileVelocityMult = 1.2f;
                        float phase6ProjectileVelocityMult = 1.1f;
                        if (phase7)
                            bloodShotVelocity *= phase7ProjectileVelocityMult;
                        else if (phase6)
                            bloodShotVelocity *= phase6ProjectileVelocityMult;

                        if (phase4)
                        {
                            bool alternativeFire = NPC.ai[3] % 2f == 0f;
                            bool diagonalShots = alternativeFire || !phase5;
                            bool otherQuadrants = Main.rand.NextBool();
                            int startingIndex = (otherQuadrants && !phase5) ? 2 : 0;
                            int totalProjectileSpreads = (phase5 || startingIndex == 2) ? 4 : 2;
                            for (int i = startingIndex; i < totalProjectileSpreads; i++)
                            {
                                Vector2 position = NPC.Center;
                                float distanceFromTargetX = Math.Abs(NPC.Center.X - Main.LocalPlayer.Center.X);
                                float distanceFromTargetY = Math.Abs(NPC.Center.Y - Main.LocalPlayer.Center.Y);

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
                            }
                        }

                        Vector2 projectileVelocity2 = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * bloodShotVelocity;
                        bool canHit2 = Collision.CanHitLine(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.BloodNautilusShot;
                            int numProj = death ? 9 : 7;
                            int spread = death ? 55 : 40;
                            if (phase7)
                            {
                                numProj = death ? 3 : 2;
                                spread = death ? 10 : 5;
                            }
                            else if (phase5)
                            {
                                numProj = death ? 3 : 2;
                                spread = death ? 15 : 10;
                            }
                            else if (phase4)
                            {
                                numProj = death ? 6 : 3;
                                spread = death ? 25 : 10;
                            }

                            float rotation = MathHelper.ToRadians(spread);
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity2.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 10f, perturbedSpeed, type, BloodShotDamage, 0f, Main.myPlayer);
                                Main.projectile[proj].timeLeft = 600;
                                if (!canHit2)
                                    Main.projectile[proj].tileCollide = false;
                            }
                        }

                        // Complete stop
                        NPC.velocity *= 0f;

                        NPC.ai[0] = -5f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                }

                // Pick teleport location
                else if (NPC.ai[0] == -1f || NPC.ai[0] == -6f)
                {
                    // Set damage
                    NPC.damage = (int)Math.Round(NPC.defDamage * ContactDamageMult);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Go to phase 3 and spin
                        if (phase3 && NPC.ai[0] == -1f)
                        {
                            // Avoid cheap bullshit
                            NPC.damage = 0;

                            // Velocity
                            NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * spinVelocity;
                            NPC.ai[0] = -4f;
                            NPC.ai[1] = playerLocation < 0 ? 1f : -1f;
                            NPC.ai[2] = 0f;

                            int maxRandomTime = phase7 ? (death ? 22 : 30) : (death ? 45 : 60);
                            NPC.ai[3] = Main.rand.Next(maxRandomTime) + 1;
                            NPC.localAI[1] = 0f;
                            NPC.alpha = 0;
                            NPC.netUpdate = true;
                        }

                        if (!despawn)
                            NPC.localAI[1] += 1f;

                        float teleportGateValue = NPC.ai[0] == -6f ? 270f : 210f;
                        if (NPC.localAI[1] >= teleportGateValue)
                        {
                            NPC.localAI[1] = 0f;

                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            options.finishThemOff = true;
                            CalamityUtils.CalamityTargeting(NPC, options);

                            int numTeleportTries = 0;
                            int maxTeleportTries = 100;
                            int teleportTileX;
                            int teleportTileY;
                            bool cardinal = Main.rand.NextBool();
                            bool aboveOrBelow = Main.rand.NextBool();
                            do
                            {
                                numTeleportTries++;
                                teleportTileX = (int)Main.player[NPC.target].Center.X / 16;
                                teleportTileY = (int)Main.player[NPC.target].Center.Y / 16;

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
                                    NPC.damage = 0;

                                    NPC.ai[3] = 0f;
                                    NPC.ai[0] = NPC.ai[0] == -6f ? -7f : -2f;
                                    NPC.ai[1] = teleportTileX;
                                    NPC.ai[2] = teleportTileY;
                                    NPC.ForceNetUpdate();

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
                else if (NPC.ai[0] == -2f || NPC.ai[0] == -7f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.velocity *= 0.9f;

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NPC.ai[3] += 15f;
                    else
                        NPC.ai[3] += 25f;

                    if (NPC.ai[3] >= 255f)
                    {
                        NPC.ai[3] = 255f;
                        NPC.position.X = NPC.ai[1] * 16f - (NPC.width / 2);
                        NPC.position.Y = NPC.ai[2] * 16f - (NPC.height / 2);
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        NPC.ai[0] = NPC.ai[0] == -7f ? -8f : -3f;

                        // Move non-charging Creepers to new Brain location
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC creeper = Main.npc[i];
                            if (creeper.active && creeper.type == NPCID.Creeper)
                            {
                                bool creeperCanTeleport = creeper.ai[0] == 0f;
                                if (creeperCanTeleport)
                                {
                                    creeper.position.X = NPC.position.X;
                                    creeper.position.Y = NPC.position.Y;
                                }
                            }
                        }

                        NPC.ForceNetUpdate();
                    }

                    NPC.alpha = (int)NPC.ai[3];
                }

                // Become visible
                else if (NPC.ai[0] == -3f || NPC.ai[0] == -8f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NPC.ai[3] -= 15f;
                    else
                        NPC.ai[3] -= 25f;

                    if (NPC.ai[3] <= 0f)
                    {
                        if (NPC.ai[0] == -8f)
                        {
                            NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * spinVelocity;

                            NPC.ai[0] = -4f;
                            NPC.ai[1] = playerLocation < 0 ? 1f : -1f;
                            NPC.ai[2] = 0f;

                            int maxRandomTime = phase7 ? (death ? 22 : 30) : (death ? 45 : 60);
                            NPC.ai[3] = Main.rand.Next(maxRandomTime) + 1;
                        }
                        else
                        {
                            NPC.ai[3] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.ai[0] = -1f;
                        }
                        NPC.ForceNetUpdate();
                    }

                    NPC.alpha = (int)NPC.ai[3];
                }
            }

            // Phase 1
            else
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

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
                    NPC.ai[0] = -1f;
                    NPC.localAI[1] = 0f;
                    NPC.alpha = 0;

                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.aggroRatio = -1f;
                    options.finishThemOff = true;
                    CalamityUtils.CalamityTargeting(NPC, options);

                    NPC.netUpdate = true;
                    return false;
                }

                // Move towards target
                if (!despawn)
                {
                    Vector2 brainCenterPhase1 = NPC.Center;
                    float targetXDistPhase1 = Main.player[NPC.target].Center.X - brainCenterPhase1.X;
                    float targetYDistPhase1 = Main.player[NPC.target].Center.Y - brainCenterPhase1.Y;
                    float targetDistancePhase1 = (float)Math.Sqrt(targetXDistPhase1 * targetXDistPhase1 + targetYDistPhase1 * targetYDistPhase1);
                    float maxMoveVelocity = (death ? 1.9f : 1.5f) + velocityScale; // This used to be 4f in death. Yeah
                    if (Main.getGoodWorld)
                        maxMoveVelocity *= 2f;

                    if (targetDistancePhase1 < maxMoveVelocity)
                    {
                        NPC.velocity.X = targetXDistPhase1;
                        NPC.velocity.Y = targetYDistPhase1;
                    }
                    else
                    {
                        targetDistancePhase1 = maxMoveVelocity / targetDistancePhase1;
                        NPC.velocity.X = targetXDistPhase1 * targetDistancePhase1;
                        NPC.velocity.Y = targetYDistPhase1 * targetDistancePhase1;
                    }
                }

                // Pick a teleport location
                if (NPC.ai[0] == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (!despawn)
                            NPC.localAI[1] += (death ? 2f : 1f) + velocityScale;

                        if (NPC.localAI[1] >= (death ? 570f : 360f))
                        {
                            // Teleport location
                            NPC.localAI[1] = 0f;

                            CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                            options.aggroRatio = -1f;
                            options.finishThemOff = true;
                            CalamityUtils.CalamityTargeting(NPC, options);

                            int phase1TeleportTries = 0;
                            int maxTeleportTries = 100;
                            int phase1TeleportTileX;
                            int phase1TeleportTileY;
                            do
                            {
                                phase1TeleportTries++;
                                phase1TeleportTileX = (int)Main.player[NPC.target].Center.X / 16;
                                phase1TeleportTileY = (int)Main.player[NPC.target].Center.Y / 16;

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

                                if (phase1TeleportTries > maxTeleportTries || (!WorldGen.SolidTile(phase1TeleportTileX, phase1TeleportTileY) && Collision.CanHit(new Vector2(phase1TeleportTileX * 16, phase1TeleportTileY * 16), 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height)))
                                {
                                    NPC.ai[0] = 1f;
                                    NPC.ai[1] = phase1TeleportTileX;
                                    NPC.ai[2] = phase1TeleportTileY;
                                    NPC.netUpdate = true;
                                    break;
                                }
                            }
                            while (phase1TeleportTries <= maxTeleportTries);
                        }
                    }
                }

                // Turn invisible and teleport
                else if (NPC.ai[0] == 1f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.alpha += 25;
                    if (NPC.alpha >= 255)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        NPC.alpha = 255;
                        NPC.position.X = NPC.ai[1] * 16f - (NPC.width / 2);
                        NPC.position.Y = NPC.ai[2] * 16f - (NPC.height / 2);
                        NPC.ai[0] = 2f;

                        // Move non-charging Creepers to new Brain location
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC creeper = Main.npc[i];
                            if (creeper.active && creeper.type == NPCID.Creeper)
                            {
                                bool creeperCanTeleport = creeper.ai[0] == 0f;
                                if (creeperCanTeleport)
                                {
                                    creeper.position.X = NPC.position.X;
                                    creeper.position.Y = NPC.position.Y;
                                }
                            }
                        }
                    }
                }

                // Become visible
                else if (NPC.ai[0] == 2f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.alpha -= 25;
                    if (NPC.alpha <= 0)
                    {
                        NPC.alpha = 0;
                        NPC.ai[0] = 0f;
                    }
                }
            }

            return false;
        }

        public class CreeperAI : VanillaAIOverride
        {
            public override bool AI(Mod mod)
            {
                // Despawn if Brain is gone
                if (NPC.crimsonBoss < 0)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                    return false;
                }

                bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

                // Get a target
                if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                    CalamityUtils.CalamityTargeting(NPC, default);

                bool brainIsNotTeleportingOrCharging = Main.npc[NPC.crimsonBoss].ai[0] == 0f || Main.npc[NPC.crimsonBoss].ai[0] == -1f || Main.npc[NPC.crimsonBoss].ai[0] == -6f;
                bool brainIsInPhase2 = Main.npc[NPC.crimsonBoss].ai[0] < 0f;

                // Creeper count
                int creeperCount = NPC.CountNPCS(NPC.type);
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

                // Give off blood dust before charging
                float beginTelegraphGateValue = TimeBeforeCreeperAttack - CreeperTelegraphTime;
                bool showTelegraph = NPC.ai[1] >= beginTelegraphGateValue || NPC.ai[0] == 1f;
                if (showTelegraph)
                {
                    float dustScalar = NPC.ai[0] == 1f ? 1f : MathHelper.Clamp((NPC.ai[1] - beginTelegraphGateValue) / CreeperTelegraphTime, 0f, 1f);
                    int dustAmt = 1 + (int)Math.Round(4 * dustScalar);
                    Color dustColor = new Color(255, 50, 50, 0) * dustScalar;
                    for (int i = 0; i < dustAmt; i++)
                    {
                        Dust dust = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, NPC.velocity.X, NPC.velocity.Y, 100, dustColor)];
                        dust.noGravity = true;
                        dust.velocity = Vector2.Zero;
                        dust.scale = 1.2f;
                    }
                }

                // Stay near Brain
                if (NPC.ai[0] == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    Vector2 creeperCenter = NPC.Center;
                    float brainXDist = Main.npc[NPC.crimsonBoss].Center.X - creeperCenter.X;
                    float brainYDist = Main.npc[NPC.crimsonBoss].Center.Y - creeperCenter.Y;
                    float brainDistance = (float)Math.Sqrt(brainXDist * brainXDist + brainYDist * brainYDist);
                    float velocity = (death ? 10f : 8f) + chargeAggressionScale;
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
                        NPC.velocity.X = (NPC.velocity.X * inertia + brainXDist) / (inertia + 1f);
                        NPC.velocity.Y = (NPC.velocity.Y * inertia + brainYDist) / (inertia + 1f);
                    }

                    // Increase speed
                    if (NPC.velocity.Length() < velocity)
                        NPC.velocity *= 1.05f;

                    // Set alpha to brain alpha
                    NPC.alpha = Main.npc[NPC.crimsonBoss].alpha;

                    // Only increment the attack timer if the brain isn't teleporting
                    if (brainIsNotTeleportingOrCharging)
                        NPC.ai[1] += (brainIsInPhase2 ? 2f : 1f) + chargeAggressionScale;

                    // Charge at target
                    if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] >= TimeBeforeCreeperAttack)
                    {
                        NPC.ai[1] = 0f;

                        CalamityUtils.CalamityTargeting(NPC, default);

                        creeperCenter = NPC.Center;
                        brainXDist = Main.player[NPC.target].Center.X - creeperCenter.X;
                        brainYDist = Main.player[NPC.target].Center.Y - creeperCenter.Y;
                        brainDistance = (float)Math.Sqrt(brainXDist * brainXDist + brainYDist * brainYDist);
                        brainDistance = velocity / brainDistance;
                        NPC.velocity.X = brainXDist * brainDistance;
                        NPC.velocity.Y = brainYDist * brainDistance;
                        NPC.ai[0] = 1f;
                        NPC.netUpdate = true;
                    }
                }

                // Charge
                else
                {
                    // Always fully visible while charging
                    NPC.alpha = 0;

                    float chargeVelocity = (death ? 8f : 6f) + chargeAggressionScale;
                    float returnToBrainGateValue = 1f;
                    if (!brainIsInPhase2)
                    {
                        // Set damage
                        NPC.damage = (int)Math.Round(NPC.defDamage * ContactDamageMult);

                        Vector2 destination = Main.player[NPC.target].Center + (death ? Main.player[NPC.target].velocity * 20f : Vector2.Zero);
                        Vector2 targetDirection = destination - NPC.Center;
                        targetDirection = targetDirection.SafeNormalize(Vector2.UnitY);
                        if (Main.getGoodWorld)
                        {
                            targetDirection *= chargeVelocity + 6f;
                            NPC.velocity = (NPC.velocity * 49f + targetDirection) / 50f;
                        }
                        else
                        {
                            targetDirection *= chargeVelocity;
                            float inertia = death ? 75f : 100f;
                            NPC.velocity = (NPC.velocity * (inertia - 1f) + targetDirection) / inertia;
                        }

                        // Return to Brain after a set time
                        float chargeDistance = death ? 800f : 600f;
                        returnToBrainGateValue = chargeDistance / chargeVelocity;
                    }

                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] >= returnToBrainGateValue || brainIsInPhase2)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        // Shoot blood shots
                        if (Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) > 160f && NPC.ai[2] == 0f)
                        {
                            bool canHit = Collision.CanHitLine(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1);
                            Vector2 projectileVelocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * chargeVelocity;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int projectileType = ProjectileID.BloodShot;
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelocity, projectileType, BloodShotDamage, 0f, Main.myPlayer);
                                Main.projectile[proj].timeLeft = 600;
                                if (!canHit)
                                    Main.projectile[proj].tileCollide = false;
                            }
                            NPC.ai[2] = 1f;
                        }

                        if (brainIsNotTeleportingOrCharging)
                        {
                            NPC.ai[0] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                }

                return false;
            }
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
