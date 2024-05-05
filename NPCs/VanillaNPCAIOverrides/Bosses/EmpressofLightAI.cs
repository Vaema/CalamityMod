using System;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class EmpressofLightAI
    {
        public static bool BuffedEmpressofLightAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Difficulty bools.
            bool bossRush = BossRushEvent.BossRushActive;
            bool masterMode = Main.masterMode || bossRush;
            bool death = CalamityWorld.death || bossRush;

            // Rotation
            npc.rotation = npc.velocity.X * 0.005f;

            // Reset DR every frame.
            calamityGlobalNPC.DR = 0.15f;

            // Percent life remaining.
            float lifeRatio = npc.life / (float)npc.lifeMax;

            float phase2LifeRatio = masterMode ? 0.7f : 0.6f;
            float phase3LifeRatio = masterMode ? 0.3f : 0.15f;
            bool phase2 = npc.AI_120_HallowBoss_IsInPhase2();
            bool phase3 = lifeRatio <= phase3LifeRatio;

            bool shouldBeInPhase2ButIsStillInPhase1 = lifeRatio <= phase2LifeRatio && !phase2;
            if (shouldBeInPhase2ButIsStillInPhase1)
                calamityGlobalNPC.DR = 0.99f;

            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = shouldBeInPhase2ButIsStillInPhase1 || npc.ai[0] == 6f;

            bool dayTimeEnrage = NPC.ShouldEmpressBeEnraged();
            if (npc.life == npc.lifeMax && dayTimeEnrage && !npc.AI_120_HallowBoss_IsGenuinelyEnraged())
                npc.ai[3] += 2f;

            npc.Calamity().CurrentlyEnraged = !bossRush && dayTimeEnrage;

            int projectileDamageMultiplier = dayTimeEnrage ? 2 : 1;

            Vector2 rainbowStreakDistance = new Vector2(-250f, -350f);
            Vector2 everlastingRainbowDistance = new Vector2(0f, -450f);
            Vector2 etherealLanceDistance = new Vector2(0f, -450f);
            Vector2 sunDanceDistance = new Vector2(-80f, -500f);

            float acceleration = death ? 0.66f : 0.6f;
            float velocity = death ? 16.5f : 15f;
            float movementDistanceGateValue = 40f;
            float despawnDistanceGateValue = 6400f;

            if (dayTimeEnrage)
            {
                float enragedDistanceMultiplier = 1.1f;
                rainbowStreakDistance *= enragedDistanceMultiplier;
                everlastingRainbowDistance *= enragedDistanceMultiplier;
                etherealLanceDistance *= enragedDistanceMultiplier;

                float enragedVelocityMultiplier = 1.2f;
                acceleration *= enragedVelocityMultiplier;
                velocity *= enragedVelocityMultiplier;
            }

            bool visible = true;
            bool takeDamage = true;
            float lessTimeSpentPerPhaseMultiplier = phase2 ? (death ? 0.375f : 0.5f) : (death ? 0.75f : 1f);
            if (Main.getGoodWorld)
                lessTimeSpentPerPhaseMultiplier *= 0.2f;

            float extraPhaseTime;
            Vector2 destination;

            // Variables for dust visuals on spawn and in phase 3
            float playSpawnSoundTime = 10f;
            float stopSpawningDustTime = 150f;
            float spawnTime = 180f;

            // Do visual stuff in phase 3
            float maxOpacity = phase3 ? 0.7f : 1f;
            int minAlpha = 255 - (int)(255 * maxOpacity);
            if (phase3)
            {
                if (calamityGlobalNPC.newAI[0] == playSpawnSoundTime)
                    SoundEngine.PlaySound(SoundID.Item161, npc.Center);

                if (calamityGlobalNPC.newAI[0] > playSpawnSoundTime && calamityGlobalNPC.newAI[0] < stopSpawningDustTime)
                    CreateSpawnDust(npc, false);

                calamityGlobalNPC.newAI[0] += 1f;
                if (calamityGlobalNPC.newAI[0] >= stopSpawningDustTime)
                {
                    calamityGlobalNPC.newAI[0] = playSpawnSoundTime + 1f;
                    npc.SyncExtraAI();
                }
            }

            switch ((int)npc.ai[0])
            {
                // Spawn animation.
                case 0:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                    {
                        npc.velocity = new Vector2(0f, 5f);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + new Vector2(0f, -80f), Vector2.Zero, ProjectileID.HallowBossDeathAurora, 0, 0f, Main.myPlayer);
                    }

                    if (npc.ai[1] == playSpawnSoundTime)
                        SoundEngine.PlaySound(SoundID.Item161, npc.Center);

                    npc.velocity *= 0.95f;

                    if (npc.ai[1] > playSpawnSoundTime && npc.ai[1] < stopSpawningDustTime)
                        CreateSpawnDust(npc);

                    npc.ai[1] += 1f;
                    visible = false;
                    takeDamage = false;
                    npc.Opacity = MathHelper.Clamp(npc.ai[1] / spawnTime, 0f, 1f);

                    if (npc.ai[1] >= spawnTime)
                    {
                        if (dayTimeEnrage && !npc.AI_120_HallowBoss_IsGenuinelyEnraged())
                            npc.ai[3] += 2f;

                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                        npc.TargetClosest();
                    }

                    break;

                // Phase switch.
                case 1:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    float idleTimer = phase2 ? (death ? 10f : 15f) : (death ? 20f : 30f);
                    if (Main.getGoodWorld)
                        idleTimer *= 0.5f;
                    if (idleTimer < 10f)
                        idleTimer = 10f;

                    if (npc.ai[1] <= 10f)
                    {
                        if (npc.ai[1] == 0f)
                            npc.TargetClosest();

                        // Despawn.
                        NPCAimedTarget targetData4 = npc.GetTargetData();
                        if (targetData4.Invalid)
                        {
                            npc.ai[0] = 13f;
                            npc.ai[1] = 0f;
                            npc.ai[2] += 1f;
                            npc.velocity /= 4f;
                            npc.netUpdate = true;
                            break;
                        }

                        Vector2 center = targetData4.Center;
                        center += new Vector2(0f, -400f);
                        if (npc.Distance(center) > 200f)
                            center -= npc.DirectionTo(center) * 100f;

                        Vector2 targetDirection = center - npc.Center;
                        float lerpValue = Utils.GetLerpValue(100f, 600f, targetDirection.Length());
                        float targetDistance = targetDirection.Length();

                        float maxVelocity = death ? 24f : 21f;
                        if (targetDistance > maxVelocity)
                            targetDistance = maxVelocity;

                        npc.velocity = Vector2.Lerp(targetDirection.SafeNormalize(Vector2.Zero) * targetDistance, targetDirection / 6f, lerpValue);
                        npc.netUpdate = true;
                    }

                    npc.velocity *= 0.92f;
                    npc.ai[1] += 1f;
                    if (!(npc.ai[1] >= idleTimer))
                        break;

                    int attackPatternLength = (int)npc.ai[2];
                    int attackType = 2;
                    int attackIncrement = 0;

                    if (!phase2)
                    {
                        int phase1Attack1 = attackIncrement++;
                        int phase1Attack2 = attackIncrement++;
                        int phase1Attack3 = attackIncrement++;
                        int phase1Attack4 = attackIncrement++;
                        int phase1Attack5 = attackIncrement++;
                        int phase1Attack6 = attackIncrement++;
                        int phase1Attack7 = attackIncrement++;
                        int phase1Attack8 = attackIncrement++;
                        int phase1Attack9 = attackIncrement++;
                        int phase1Attack10 = attackIncrement++;

                        if (attackPatternLength % attackIncrement == phase1Attack1)
                            attackType = 2;

                        if (attackPatternLength % attackIncrement == phase1Attack2)
                            attackType = 6;

                        if (attackPatternLength % attackIncrement == phase1Attack3)
                            attackType = 8;

                        if (attackPatternLength % attackIncrement == phase1Attack4)
                        {
                            attackType = 4;

                            // Adjust the upcoming Ethereal Lance attack depending on what random variable is chosen here.
                            calamityGlobalNPC.newAI[3] = Main.rand.Next(2);

                            // Sync the Calamity AI variables.
                            npc.SyncExtraAI();
                        }

                        if (attackPatternLength % attackIncrement == phase1Attack5)
                            attackType = 5;

                        if (attackPatternLength % attackIncrement == phase1Attack6)
                            attackType = 8;

                        if (attackPatternLength % attackIncrement == phase1Attack7)
                            attackType = 2;

                        if (attackPatternLength % attackIncrement == phase1Attack8)
                        {
                            attackType = 4;

                            // Adjust the upcoming Ethereal Lance attack depending on what random variable is chosen here.
                            calamityGlobalNPC.newAI[3] = Main.rand.Next(2);

                            // Sync the Calamity AI variables.
                            npc.SyncExtraAI();
                        }

                        if (attackPatternLength % attackIncrement == phase1Attack9)
                            attackType = 8;

                        if (attackPatternLength % attackIncrement == phase1Attack10)
                            attackType = 5;

                        if (lifeRatio <= phase2LifeRatio)
                            attackType = 10;
                    }

                    if (phase2)
                    {
                        int phase2Attack1 = attackIncrement++;
                        int phase2Attack2 = attackIncrement++;
                        int phase2Attack3 = attackIncrement++;
                        int phase2Attack4 = attackIncrement++;
                        int phase2Attack5 = attackIncrement++;
                        int phase2Attack6 = attackIncrement++;
                        int phase2Attack7 = attackIncrement++;
                        int phase2Attack8 = attackIncrement++;
                        int phase2Attack9 = attackIncrement++;
                        int phase2Attack10 = attackIncrement++;

                        if (attackPatternLength % attackIncrement == phase2Attack1)
                        {
                            attackType = 7;

                            // Adjust the upcoming Ethereal Lance attack depending on what random variable is chosen here.
                            calamityGlobalNPC.newAI[2] = Main.rand.Next(2);

                            // Sync the Calamity AI variables.
                            npc.SyncExtraAI();
                        }

                        if (attackPatternLength % attackIncrement == phase2Attack2)
                            attackType = phase3 ? 8 : 2;

                        if (attackPatternLength % attackIncrement == phase2Attack3)
                            attackType = 8;

                        if (attackPatternLength % attackIncrement == phase2Attack5)
                            attackType = 5;

                        if (attackPatternLength % attackIncrement == phase2Attack6)
                            attackType = 2;

                        if (attackPatternLength % attackIncrement == phase2Attack7)
                        {
                            if (phase3)
                            {
                                attackType = 7;

                                // Adjust the upcoming Ethereal Lance attack depending on what random variable is chosen here.
                                calamityGlobalNPC.newAI[2] = Main.rand.Next(2);

                                // Sync the Calamity AI variables.
                                npc.SyncExtraAI();
                            }
                            else
                                attackType = 6;
                        }

                        if (attackPatternLength % attackIncrement == phase2Attack7)
                        {
                            if (phase3)
                            {
                                attackType = 4;

                                // Adjust the upcoming Ethereal Lance attack depending on what random variable is chosen here.
                                calamityGlobalNPC.newAI[3] = Main.rand.Next(2);

                                // Sync the Calamity AI variables.
                                npc.SyncExtraAI();
                            }
                            else
                                attackType = 6;
                        }

                        if (attackPatternLength % attackIncrement == phase2Attack8)
                        {
                            attackType = 4;

                            // Adjust the upcoming Ethereal Lance attack depending on what random variable is chosen here.
                            calamityGlobalNPC.newAI[3] = Main.rand.Next(2);

                            // Sync the Calamity AI variables.
                            npc.SyncExtraAI();
                        }

                        if (attackPatternLength % attackIncrement == phase2Attack9)
                            attackType = 8;

                        if (attackPatternLength % attackIncrement == phase2Attack4)
                            attackType = 11;

                        if (attackPatternLength % attackIncrement == phase2Attack10)
                            attackType = 12;
                    }

                    npc.TargetClosest();
                    NPCAimedTarget targetData5 = npc.GetTargetData();
                    bool despawnFlag = false;
                    if (npc.AI_120_HallowBoss_IsGenuinelyEnraged() && !bossRush)
                    {
                        if (!Main.dayTime)
                            despawnFlag = true;

                        if (Main.dayTime && Main.time >= 53400D)
                            despawnFlag = true;
                    }

                    // Despawn.
                    if (targetData5.Invalid || npc.Distance(targetData5.Center) > despawnDistanceGateValue || despawnFlag)
                        attackType = 13;

                    // Set charge direction.
                    if (attackType == 8 && targetData5.Center.X > npc.Center.X)
                        attackType = 9;

                    if (attackType != 5 && attackType != 12)
                        npc.velocity = npc.DirectionFrom(targetData5.Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * (targetData5.Center.X > npc.Center.X).ToDirectionInt()) * 24f;

                    npc.ai[0] = attackType;
                    npc.ai[1] = 0f;
                    npc.ai[2] += Main.rand.Next(2) + 1f;
                    npc.netUpdate = true;

                    break;

                // Spawn homing Rainbow Streaks.
                case 2:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                        SoundEngine.PlaySound(SoundID.Item164, npc.Center);

                    Vector2 randomStreakOffset = new Vector2(-55f, -30f);
                    NPCAimedTarget targetData11 = npc.GetTargetData();
                    Vector2 targetCenter = targetData11.Invalid ? npc.Center : targetData11.Center;
                    if (npc.Distance(targetCenter + rainbowStreakDistance) > movementDistanceGateValue)
                        npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + rainbowStreakDistance).SafeNormalize(Vector2.Zero) * velocity, acceleration);

                    if (npc.ai[1] < 60f)
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + randomStreakOffset, 1, Utils.GetLerpValue(0f, 60f, npc.ai[1], clamped: true), npc);

                    int streakSpawnFrequency = CalamityWorld.LegendaryMode ? 1 : 2;
                    if (phase3)
                        streakSpawnFrequency *= 2;

                    if ((int)npc.ai[1] % streakSpawnFrequency == 0 && npc.ai[1] < 60f)
                    {
                        int projectileType = ProjectileID.HallowBossRainbowStreak;
                        int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                        float ai3 = npc.ai[1] / 60f;
                        Vector2 rainbowStreakVelocity = new Vector2(0f, death ? -10f : -8f).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
                        if (phase2)
                            rainbowStreakVelocity = new Vector2(0f, death ? -12f : -10f).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());

                        if (dayTimeEnrage)
                            rainbowStreakVelocity *= MathHelper.Lerp(0.8f, 1.6f, ai3);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + randomStreakOffset, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, npc.target, ai3);
                            if (phase3)
                            {
                                int proj2 = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + randomStreakOffset, -rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, npc.target, 1f - ai3);
                                if (Main.rand.NextBool(60) && CalamityWorld.LegendaryMode)
                                {
                                    Main.projectile[proj2].extraUpdates += 1;
                                    Main.projectile[proj2].netUpdate = true;
                                }
                            }

                            if (Main.rand.NextBool(60) && CalamityWorld.LegendaryMode)
                            {
                                Main.projectile[proj].extraUpdates += 1;
                                Main.projectile[proj].netUpdate = true;
                            }
                        }

                        // Spawn extra homing Rainbow Streaks per player.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int multiplayerStreakSpawnFrequency = (int)(npc.ai[1] / streakSpawnFrequency);
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                if (npc.Boss_CanShootExtraAt(i, multiplayerStreakSpawnFrequency % 3, 3, 2400f))
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + randomStreakOffset, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, i, ai3);
                            }
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? (masterMode ? 30f : 36f) : (masterMode ? 60f : 72f)) + 30f * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= 60f + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // This is never fucking used.
                /*case 3:
                    {
                        npc.ai[1] += 1f;
                        NPCAimedTarget targetData8 = npc.GetTargetData();
                        Vector2 targetCenter = targetData8.Invalid ? npc.Center : targetData8.Center;
                        if (npc.Distance(targetCenter + phase2AnimationDistance) > 0.5f)
                            npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + phase2AnimationDistance).SafeNormalize(Vector2.Zero) * scaleFactor, 4f);

                        if ((int)npc.ai[1] % 180 == 0)
                        {
                            Vector2 auroraVector = new Vector2(0f, -100f);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), targetData8.Center + auroraVector, Vector2.Zero, ProjectileID.HallowBossDeathAurora, magicAmt, 0f, Main.myPlayer);
                        }

                        if (npc.ai[1] >= 120f)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }*/

                // Spawn Ethereal Lances around the target in seemingly random places (they will be made slower to make this easier to deal with).
                case 4:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                        SoundEngine.PlaySound(SoundID.Item162, npc.Center);

                    float lanceGateValue = masterMode ? 75f : 100f;

                    if (npc.ai[1] >= 6f && npc.ai[1] < 54f)
                    {
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(-55f, -20f), 2, Utils.GetLerpValue(0f, lanceGateValue, npc.ai[1], clamped: true), npc);
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(55f, -20f), 4, Utils.GetLerpValue(0f, lanceGateValue, npc.ai[1], clamped: true), npc);
                    }

                    NPCAimedTarget targetData10 = npc.GetTargetData();
                    targetCenter = targetData10.Invalid ? npc.Center : targetData10.Center;
                    if (npc.Distance(targetCenter + etherealLanceDistance) > movementDistanceGateValue)
                        npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + etherealLanceDistance).SafeNormalize(Vector2.Zero) * velocity, acceleration);

                    int lanceRotation = death ? 10 : 8;
                    if (npc.ai[1] % (dayTimeEnrage ? 2f : 3f) == 0f && npc.ai[1] < lanceGateValue)
                    {
                        int lanceAmount = phase3 ? 2 : 1;
                        for (int i = 0; i < lanceAmount; i++)
                        {
                            int lanceFrequency = (int)(npc.ai[1] / (dayTimeEnrage ? 2f : 3f));
                            lanceRotation += (masterMode ? 5 : 4) * i;
                            Vector2 lanceDirection = Vector2.UnitX.RotatedBy((float)Math.PI / (lanceRotation * 2) + lanceFrequency * ((float)Math.PI / lanceRotation));
                            if (calamityGlobalNPC.newAI[3] == 0f)
                                lanceDirection.X += (lanceDirection.X > 0f) ? 0.5f : -0.5f;

                            lanceDirection = lanceDirection.SafeNormalize(Vector2.UnitY);
                            float spawnDistance = 600f;

                            Vector2 playerCenter = targetData10.Center;
                            if (npc.Distance(playerCenter) > 2400f)
                                continue;

                            if (Vector2.Dot(targetData10.Velocity.SafeNormalize(Vector2.UnitY), lanceDirection) > 0f)
                                lanceDirection *= -1f;

                            Vector2 targetHoverPos = playerCenter + targetData10.Velocity * 90;
                            Vector2 spawnLocation = playerCenter + lanceDirection * spawnDistance - targetData10.Velocity * 30f;
                            if (spawnLocation.Distance(playerCenter) < spawnDistance)
                            {
                                Vector2 lanceSpawnDirection = playerCenter - spawnLocation;
                                if (lanceSpawnDirection == Vector2.Zero)
                                    lanceSpawnDirection = lanceDirection;

                                spawnLocation = playerCenter - lanceSpawnDirection.SafeNormalize(Vector2.UnitY) * spawnDistance;
                            }

                            int projectileType = ProjectileID.FairyQueenLance;
                            int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                            Vector2 v3 = targetHoverPos - spawnLocation;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(npc.GetSource_FromAI(), spawnLocation, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v3.ToRotation(), npc.ai[1] / lanceGateValue);

                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                continue;

                            // Spawn extra Ethereal Lances per player.
                            for (int j = 0; j < Main.maxPlayers; j++)
                            {
                                if (!npc.Boss_CanShootExtraAt(j, lanceFrequency % 3, 3, 2400f))
                                    continue;

                                Player extraPlayer = Main.player[j];
                                playerCenter = extraPlayer.Center;
                                if (Vector2.Dot(extraPlayer.velocity.SafeNormalize(Vector2.UnitY), lanceDirection) > 0f)
                                    lanceDirection *= -1f;

                                Vector2 extraPlayerSpawnLocation = playerCenter + extraPlayer.velocity * 90;
                                spawnLocation = playerCenter + lanceDirection * spawnDistance - extraPlayer.velocity * 30f;
                                if (spawnLocation.Distance(playerCenter) < spawnDistance)
                                {
                                    Vector2 extraPlayerSpawnDirection = playerCenter - spawnLocation;
                                    if (extraPlayerSpawnDirection == Vector2.Zero)
                                        extraPlayerSpawnDirection = lanceDirection;

                                    spawnLocation = playerCenter - extraPlayerSpawnDirection.SafeNormalize(Vector2.UnitY) * spawnDistance;
                                }

                                v3 = extraPlayerSpawnLocation - spawnLocation;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), spawnLocation, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v3.ToRotation(), npc.ai[1] / lanceGateValue);
                            }
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? 24f : 48f) + 20f * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= lanceGateValue + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        calamityGlobalNPC.newAI[3] = 0f;
                        npc.netUpdate = true;

                        // Sync the Calamity AI variables.
                        npc.SyncExtraAI();
                    }

                    break;

                // Spawn Everlasting Rainbow spiral.
                case 5:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                        SoundEngine.PlaySound(SoundID.Item163, npc.Center);

                    Vector2 magicSpawnOffset = new Vector2(55f, -30f);
                    Vector2 everlastingRainbowSpawn = npc.Center + magicSpawnOffset;
                    if (npc.ai[1] < 42f)
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + magicSpawnOffset, 3, Utils.GetLerpValue(0f, 42f, npc.ai[1], clamped: true), npc);

                    NPCAimedTarget targetData7 = npc.GetTargetData();
                    targetCenter = targetData7.Invalid ? npc.Center : targetData7.Center;
                    if (npc.Distance(targetCenter + everlastingRainbowDistance) > movementDistanceGateValue)
                        npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + everlastingRainbowDistance).SafeNormalize(Vector2.Zero) * velocity, acceleration);

                    if (npc.ai[1] % 42f == 0f && npc.ai[1] < 42f)
                    {
                        float projRotation = MathHelper.TwoPi * Main.rand.NextFloat();
                        float totalProjectiles = CalamityWorld.LegendaryMode ? 30f : death ? (dayTimeEnrage ? 22f : 15f) : (dayTimeEnrage ? 18f : 13f);
                        int projIndex = 0;
                        bool inversePhase2SpreadPattern = Main.rand.NextBool();
                        for (float i = 0f; i < 1f; i += 1f / totalProjectiles)
                        {
                            int projectileType = ProjectileID.HallowBossLastingRainbow;
                            int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;
                            int projectileType2 = ProjectileID.HallowBossRainbowStreak;
                            int projectileDamage2 = npc.GetProjectileDamage(projectileType2) * projectileDamageMultiplier;

                            float projRotationMultiplier = i;
                            Vector2 spinningpoint = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 + MathHelper.TwoPi * projRotationMultiplier + projRotation);

                            float initialVelocity = death ? 2f : 1.75f;
                            if (dayTimeEnrage && projIndex % 2 == 0)
                                initialVelocity *= 2f;
                            if (CalamityWorld.LegendaryMode)
                                initialVelocity *= 1.5f;

                            // Given that maxAddedVelocity = 2
                            // Before inverse: index 0 = 2, index 0.25 = 0, index 0.5 = 2, index 0.75 = 0, index 1 = 2
                            // After inverse: index 0 = 0, index 0.25 = 2, index 0.5 = 0, index 0.75 = 2, index 1 = 0
                            if (phase2)
                            {
                                float maxAddedVelocity = initialVelocity;
                                float addedVelocity = inversePhase2SpreadPattern ? Math.Abs(maxAddedVelocity - Math.Abs(MathHelper.Lerp(-maxAddedVelocity, maxAddedVelocity, Math.Abs(i - 0.5f) * 2f))) : Math.Abs(MathHelper.Lerp(-maxAddedVelocity, maxAddedVelocity, Math.Abs(i - 0.5f) * 2f));
                                initialVelocity += addedVelocity;
                            }

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromAI(), everlastingRainbowSpawn + spinningpoint.RotatedBy(-MathHelper.PiOver2) * 30f, spinningpoint * initialVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, 0f, projRotationMultiplier);

                                if (phase3)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), everlastingRainbowSpawn + spinningpoint.RotatedBy(-MathHelper.PiOver2) * 30f, spinningpoint * (masterMode ? 3f : 2f) * initialVelocity, projectileType2, projectileDamage2, 0f, Main.myPlayer, npc.target, projRotationMultiplier);
                                }
                            }

                            projIndex++;
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? 36f : 72f) + 30f * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= 72f + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // Use Sun Dance.
                case 6:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    // Increase durability.
                    calamityGlobalNPC.DR = shouldBeInPhase2ButIsStillInPhase1 ? 0.99f : (bossRush ? 0.99f : 0.575f);

                    int totalSunDances = phase2 ? 2 : 3;
                    float sunDanceGateValue = dayTimeEnrage ? 35f : death ? 40f : 50f;
                    float totalSunDancePhaseTime = totalSunDances * sunDanceGateValue;

                    Vector2 sunDanceHoverOffset = new Vector2(0f, -100f);
                    Vector2 position = npc.Center + sunDanceHoverOffset;

                    NPCAimedTarget targetData2 = npc.GetTargetData();
                    targetCenter = targetData2.Invalid ? npc.Center : targetData2.Center;
                    if (npc.Distance(targetCenter + sunDanceDistance) > movementDistanceGateValue)
                        npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + sunDanceDistance).SafeNormalize(Vector2.Zero) * velocity * 0.3f, acceleration * 0.7f);

                    if (npc.ai[1] % sunDanceGateValue == 0f && npc.ai[1] < totalSunDancePhaseTime)
                    {
                        int projectileType = ProjectileID.FairyQueenSunDance;
                        int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                        int sunDanceExtension = (int)(npc.ai[1] / sunDanceGateValue);
                        int targetFloatDirection = (targetData2.Center.X > npc.Center.X) ? 1 : 0;
                        float projAmount = phase2 ? 8f : 6f;
                        float projRotation = 1f / projAmount;
                        for (float j = 0f; j < 1f; j += projRotation)
                        {
                            float projDirection = (j + projRotation * 0.5f + sunDanceExtension * projRotation * 0.5f) % 1f;
                            float ai = MathHelper.TwoPi * (projDirection + targetFloatDirection);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(npc.GetSource_FromAI(), position, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, ai, npc.whoAmI);
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? (masterMode ? 105f : 110f) : (masterMode ? 140f : 150f)) + 30f * lessTimeSpentPerPhaseMultiplier; // 112.5 is too little
                    if (npc.ai[1] >= totalSunDancePhaseTime + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // Spawn rows of Ethereal Lances.
                case 7:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    // Expert attack or not.
                    bool expertAttack = calamityGlobalNPC.newAI[2] == 0f;

                    int numLanceWalls = expertAttack ? 6 : 4;
                    float lanceWallSpawnGateValue = expertAttack ? 36f : 54f;
                    if (dayTimeEnrage)
                        lanceWallSpawnGateValue -= expertAttack ? 4f : 6f;

                    float lanceWallPhaseTime = lanceWallSpawnGateValue * numLanceWalls;

                    NPCAimedTarget targetData9 = npc.GetTargetData();
                    destination = targetData9.Invalid ? npc.Center : targetData9.Center;
                    if (npc.Distance(destination + etherealLanceDistance) > movementDistanceGateValue)
                        npc.SimpleFlyMovement(npc.DirectionTo(destination + etherealLanceDistance).SafeNormalize(Vector2.Zero) * velocity * 0.4f, acceleration);

                    if ((int)npc.ai[1] % lanceWallSpawnGateValue == 0f && npc.ai[1] < lanceWallPhaseTime)
                    {
                        SoundEngine.PlaySound(SoundID.Item162, npc.Center);

                        float totalProjectiles = masterMode ? 18f : 15f;
                        float lanceSpacing = masterMode ? 150f : 175f;
                        float lanceWallSize = totalProjectiles * lanceSpacing;

                        Vector2 lanceSpawnOffset = targetData9.Center;
                        if (npc.Distance(lanceSpawnOffset) <= 3200f)
                        {
                            Vector2 lanceWallStartingPosition = Vector2.Zero;
                            Vector2 lanceWallDirection = Vector2.UnitY;
                            float lanceWallConvergence = 0.4f;
                            float lanceWallSizeMult = 1.4f;
                            totalProjectiles += 5f;
                            lanceSpacing += 50f;
                            lanceWallSize *= (masterMode ? 0.75f : 0.5f);
                            float direction = 1f;

                            int randomLanceWallType;
                            do randomLanceWallType = Main.rand.Next(numLanceWalls);
                            while (randomLanceWallType == calamityGlobalNPC.newAI[3]);

                            // This is set so that Empress doesn't use the same wall type twice in a row.
                            calamityGlobalNPC.newAI[3] = randomLanceWallType;

                            // Keeps track of the total number of lance walls used.
                            calamityGlobalNPC.newAI[1] += 1f;

                            // Sync the Calamity AI variables.
                            npc.SyncExtraAI();

                            switch (randomLanceWallType)
                            {
                                case 0:
                                    lanceSpawnOffset += new Vector2((0f - lanceWallSize) / 2f, 0f) * direction;
                                    lanceWallStartingPosition = new Vector2(0f, lanceWallSize);
                                    lanceWallDirection = Vector2.UnitX;
                                    break;

                                case 1:
                                    lanceSpawnOffset += new Vector2(lanceWallSize / 2f, lanceSpacing / 2f) * direction;
                                    lanceWallStartingPosition = new Vector2(0f, lanceWallSize);
                                    lanceWallDirection = -Vector2.UnitX;
                                    break;

                                case 2:
                                    lanceSpawnOffset += new Vector2(0f - lanceWallSize, 0f - lanceWallSize) * lanceWallConvergence * direction;
                                    lanceWallStartingPosition = new Vector2(lanceWallSize * lanceWallSizeMult, 0f);
                                    lanceWallDirection = new Vector2(1f, 1f);
                                    break;

                                case 3:
                                    lanceSpawnOffset += new Vector2(lanceWallSize * lanceWallConvergence + lanceSpacing / 2f, (0f - lanceWallSize) * lanceWallConvergence) * direction;
                                    lanceWallStartingPosition = new Vector2((0f - lanceWallSize) * lanceWallSizeMult, 0f);
                                    lanceWallDirection = new Vector2(-1f, 1f);
                                    break;

                                case 4:
                                    lanceSpawnOffset += new Vector2(0f - lanceWallSize, lanceWallSize) * lanceWallConvergence * direction;
                                    lanceWallStartingPosition = new Vector2(lanceWallSize * lanceWallSizeMult, 0f);
                                    lanceWallDirection = lanceSpawnOffset.DirectionTo(targetData9.Center);
                                    break;

                                case 5:
                                    lanceSpawnOffset += new Vector2(lanceWallSize * lanceWallConvergence + lanceSpacing / 2f, lanceWallSize * lanceWallConvergence) * direction;
                                    lanceWallStartingPosition = new Vector2((0f - lanceWallSize) * lanceWallSizeMult, 0f);
                                    lanceWallDirection = lanceSpawnOffset.DirectionTo(targetData9.Center);
                                    break;
                            }

                            int projectileType = ProjectileID.FairyQueenLance;
                            int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                            for (float i = 0f; i <= 1f; i += 1f / totalProjectiles)
                            {
                                Vector2 spawnLocation = lanceSpawnOffset + lanceWallStartingPosition * (i - 0.5f) * (expertAttack ? 1f : 2f);
                                Vector2 v2 = lanceWallDirection;
                                if (expertAttack)
                                {
                                    Vector2 lanceWallSpawnPredictiveness = targetData9.Velocity * 20f * i;
                                    Vector2 lanceWallSpawnLocation = spawnLocation.DirectionTo(targetData9.Center + lanceWallSpawnPredictiveness);
                                    v2 = Vector2.Lerp(lanceWallDirection, lanceWallSpawnLocation, 0.75f).SafeNormalize(Vector2.UnitY);
                                }

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), spawnLocation, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v2.ToRotation(), i);
                            }
                        }

                        // Chance to stop using the lance walls and switch to a different attack after 3 lance walls are used.
                        if (Main.rand.NextBool(5 - ((int)calamityGlobalNPC.newAI[1] - 2)) && calamityGlobalNPC.newAI[1] >= 2f)
                        {
                            npc.ai[1] = lanceWallPhaseTime;
                            npc.netUpdate = true;
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? 24f : 48f) + 20f * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= lanceWallPhaseTime + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        calamityGlobalNPC.newAI[3] = 0f;
                        calamityGlobalNPC.newAI[1] = 0f;
                        calamityGlobalNPC.newAI[2] = 0f;
                        npc.SyncExtraAI();
                        npc.netUpdate = true;
                    }

                    break;

                // Charge either left or right.
                case 8:
                case 9:

                    int chargeDirection = (npc.ai[0] != 8f) ? 1 : (-1);

                    AI_120_HallowBoss_DoMagicEffect(npc.Center, 5, Utils.GetLerpValue(40f, 90f, npc.ai[1], clamped: true), npc);

                    float chargeGateValue = 40f;
                    float playChargeSoundTime = 20f;
                    float chargeDuration = phase3 ? 40f : 50f;
                    float slowDownTime = 30f;
                    float totalPhaseTime = chargeGateValue + chargeDuration + slowDownTime;
                    float chargeStartDistance = phase3 ? 1000f : 800f;
                    float chargeVelocity = phase3 ? 100f : 70f;
                    float chargeAcceleration = phase3 ? 0.1f : 0.07f;

                    if (npc.ai[1] <= chargeGateValue)
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        if (npc.ai[1] == playChargeSoundTime)
                            SoundEngine.PlaySound(SoundID.Item160, npc.Center);

                        NPCAimedTarget targetData3 = npc.GetTargetData();
                        destination = (targetData3.Invalid ? npc.Center : targetData3.Center) + new Vector2(chargeDirection * -chargeStartDistance, 0f);
                        npc.SimpleFlyMovement(npc.DirectionTo(destination).SafeNormalize(Vector2.Zero) * velocity, acceleration * 2f);

                        if (npc.ai[1] == chargeGateValue)
                            npc.velocity *= 0.3f;
                    }
                    else if (npc.ai[1] <= chargeGateValue + chargeDuration)
                    {
                        // Spawn Rainbow Streaks during charge.
                        if (npc.ai[1] == chargeGateValue + 1f)
                            SoundEngine.PlaySound(SoundID.Item164, npc.Center);

                        float rainbowStreakGateValue = 2f;
                        if ((npc.ai[1] - 1f) % rainbowStreakGateValue == 0f)
                        {
                            int projectileType = ProjectileID.HallowBossRainbowStreak;
                            int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                            float ai3 = (npc.ai[1] - chargeGateValue - 1f) / chargeDuration;
                            Vector2 rainbowStreakVelocity = new Vector2(0f, death ? -5f : -4f).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
                            if (phase2)
                                rainbowStreakVelocity = new Vector2(0f, death ? -6f : -5f).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());

                            rainbowStreakVelocity.X *= 2f;
                            if (!phase2)
                                rainbowStreakVelocity.Y *= 0.5f;

                            if (dayTimeEnrage)
                                rainbowStreakVelocity *= MathHelper.Lerp(0.8f, 1.6f, ai3);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, npc.target, ai3);
                                if (Main.rand.NextBool(30) && CalamityWorld.LegendaryMode)
                                {
                                    Main.projectile[proj].extraUpdates += 1;
                                    Main.projectile[proj].netUpdate = true;
                                }
                            }

                            // Spawn extra homing Rainbow Streaks per player.
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int multiplayerStreakSpawnFrequency = (int)((npc.ai[1] - chargeGateValue - 1f) / rainbowStreakGateValue);
                                for (int i = 0; i < Main.maxPlayers; i++)
                                {
                                    if (npc.Boss_CanShootExtraAt(i, multiplayerStreakSpawnFrequency % 3, 3, 2400f))
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, i, ai3);
                                }
                            }
                        }

                        npc.velocity = Vector2.Lerp(value2: new Vector2(chargeDirection * chargeVelocity, 0f), value1: npc.velocity, amount: chargeAcceleration);

                        if (npc.ai[1] == chargeGateValue + chargeDuration)
                            npc.velocity *= 0.45f;

                        npc.damage = (int)Math.Round(npc.defDamage * (dayTimeEnrage ? 3D : 1.5));
                    }
                    else
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        npc.velocity *= 0.92f;
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? 24f : 48f) * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= totalPhaseTime + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // Phase 2 animation.
                case 10:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                        SoundEngine.PlaySound(SoundID.Item161, npc.Center);

                    takeDamage = !(npc.ai[1] >= 30f) || !(npc.ai[1] <= 170f);

                    npc.velocity *= 0.95f;

                    if (npc.ai[1] == 90f)
                    {
                        if (npc.ai[3] == 0f)
                            npc.ai[3] = 1f;

                        if (npc.ai[3] == 2f)
                            npc.ai[3] = 3f;

                        npc.Center = npc.GetTargetData().Center + new Vector2(0f, -250f);
                        npc.netUpdate = true;
                    }

                    npc.ai[1] += 1f;
                    if (npc.ai[1] >= 180f)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // Spawn Ethereal Lances ahead of the target.
                case 11:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                        SoundEngine.PlaySound(SoundID.Item162, npc.Center);

                    float lanceGateValue2 = masterMode ? 75f : 100f;

                    if (npc.ai[1] >= 6f && npc.ai[1] < 54f)
                    {
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(-55f, -20f), 2, Utils.GetLerpValue(0f, lanceGateValue2, npc.ai[1], clamped: true), npc);
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(55f, -20f), 4, Utils.GetLerpValue(0f, lanceGateValue2, npc.ai[1], clamped: true), npc);
                    }

                    NPCAimedTarget targetData6 = npc.GetTargetData();
                    targetCenter = targetData6.Invalid ? npc.Center : targetData6.Center;
                    if (npc.Distance(targetCenter + etherealLanceDistance) > movementDistanceGateValue)
                        npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + etherealLanceDistance).SafeNormalize(Vector2.Zero) * velocity, acceleration);

                    float etherealLanceGateValue = death ? 5f : 6f;
                    if (dayTimeEnrage)
                        etherealLanceGateValue -= 1f;

                    if (npc.ai[1] % etherealLanceGateValue == 0f && npc.ai[1] < lanceGateValue2)
                    {
                        int numLances = phase3 ? 4 : 3;
                        for (int i = 0; i < numLances; i++)
                        {
                            // Spawn another lance in the opposite location
                            bool oppositeLance = i % 2 == 0;

                            Vector2 inverseTargetVel = oppositeLance ? targetData6.Velocity : -targetData6.Velocity;
                            inverseTargetVel.SafeNormalize(-Vector2.UnitY);
                            float spawnDistance = 100f + (i * 100f);

                            targetCenter = targetData6.Center;
                            if (npc.Distance(targetCenter) > 2400f)
                                continue;

                            Vector2 straightLanceSpawnPredict = targetCenter + (oppositeLance ? -targetData6.Velocity : targetData6.Velocity) * 90;
                            Vector2 straightLanceSpawnDirection = targetCenter + inverseTargetVel * spawnDistance;
                            if (straightLanceSpawnDirection.Distance(targetCenter) < spawnDistance)
                            {
                                Vector2 straightLanceSpawnLocation = targetCenter - straightLanceSpawnDirection;
                                if (straightLanceSpawnLocation == Vector2.Zero)
                                    straightLanceSpawnLocation = inverseTargetVel;

                                straightLanceSpawnDirection = targetCenter - straightLanceSpawnLocation.SafeNormalize(Vector2.UnitY) * spawnDistance;
                            }

                            int projectileType = ProjectileID.FairyQueenLance;
                            int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                            Vector2 v = straightLanceSpawnPredict - straightLanceSpawnDirection;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(npc.GetSource_FromAI(), straightLanceSpawnDirection, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v.ToRotation(), npc.ai[1] / lanceGateValue2);

                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                continue;

                            int multiplayerExtraStraightLances = (int)(npc.ai[1] / etherealLanceGateValue);
                            for (int l = 0; l < Main.maxPlayers; l++)
                            {
                                if (!npc.Boss_CanShootExtraAt(l, multiplayerExtraStraightLances % 3, 3, 2400f))
                                    continue;

                                Player player = Main.player[l];
                                inverseTargetVel = oppositeLance ? player.velocity : -player.velocity;
                                inverseTargetVel.SafeNormalize(-Vector2.UnitY);
                                targetCenter = player.Center;
                                Vector2 extraPlayerLancePredict = targetCenter + (oppositeLance ? -player.velocity : player.velocity) * 90;
                                straightLanceSpawnDirection = targetCenter + inverseTargetVel * spawnDistance;
                                if (straightLanceSpawnDirection.Distance(targetCenter) < spawnDistance)
                                {
                                    Vector2 extraPlayerLanceSpawnLocation = targetCenter - straightLanceSpawnDirection;
                                    if (extraPlayerLanceSpawnLocation == Vector2.Zero)
                                        extraPlayerLanceSpawnLocation = inverseTargetVel;

                                    straightLanceSpawnDirection = targetCenter - extraPlayerLanceSpawnLocation.SafeNormalize(Vector2.UnitY) * spawnDistance;
                                }

                                v = extraPlayerLancePredict - straightLanceSpawnDirection;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), straightLanceSpawnDirection, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v.ToRotation(), npc.ai[1] / lanceGateValue2);
                            }
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? 24f : 48f) * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= lanceGateValue2 + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // Spawn homing Rainbow Streaks.
                case 12:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    Vector2 projRandomOffset = new Vector2(-55f, -30f);

                    if (npc.ai[1] == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item165, npc.Center);
                        npc.velocity = new Vector2(0f, -12f);
                    }

                    npc.velocity *= 0.95f;

                    bool shouldSpawnStreaks = npc.ai[1] < 60f && npc.ai[1] >= 10f;
                    if (shouldSpawnStreaks)
                        AI_120_HallowBoss_DoMagicEffect(npc.Center + projRandomOffset, 1, Utils.GetLerpValue(0f, 60f, npc.ai[1], clamped: true), npc);

                    int stationaryStreakSpawnFrequency = 4;
                    if (dayTimeEnrage)
                        stationaryStreakSpawnFrequency -= 1;
                    if (phase3)
                        stationaryStreakSpawnFrequency *= 2;

                    float streakHomeTime = (npc.ai[1] - 10f) / 50f;
                    if ((int)npc.ai[1] % stationaryStreakSpawnFrequency == 0 && shouldSpawnStreaks)
                    {
                        int projectileType = ProjectileID.HallowBossRainbowStreak;
                        int projectileDamage = npc.GetProjectileDamage(projectileType) * projectileDamageMultiplier;

                        Vector2 vector = new Vector2(0f, (death ? -24f : -22f) - (phase3 ? ((masterMode ? 6f : 4f) * streakHomeTime) : 0f)).RotatedBy(MathHelper.TwoPi * streakHomeTime);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projRandomOffset, vector, projectileType, projectileDamage, 0f, Main.myPlayer, npc.target, streakHomeTime);
                            if (phase3)
                            {
                                int proj2 = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projRandomOffset, -vector, projectileType, projectileDamage, 0f, Main.myPlayer, npc.target, 1f - streakHomeTime);
                                if (Main.rand.NextBool(15) && CalamityWorld.LegendaryMode)
                                {
                                    Main.projectile[proj2].extraUpdates += 1;
                                    Main.projectile[proj2].netUpdate = true;
                                }
                            }

                            if (Main.rand.NextBool(15) && CalamityWorld.LegendaryMode)
                            {
                                Main.projectile[proj].extraUpdates += 1;
                                Main.projectile[proj].netUpdate = true;
                            }
                        }

                        // Spawn extra homing Rainbow Streaks per player.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int extraStationaryStreakSpawnFrequency = (int)(npc.ai[1] % stationaryStreakSpawnFrequency);
                            for (int j = 0; j < Main.maxPlayers; j++)
                            {
                                if (npc.Boss_CanShootExtraAt(j, extraStationaryStreakSpawnFrequency % 3, 3, 2400f))
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + projRandomOffset, vector, projectileType, projectileDamage, 0f, Main.myPlayer, j, streakHomeTime);
                            }
                        }
                    }

                    npc.ai[1] += 1f;
                    extraPhaseTime = (dayTimeEnrage ? 36f : 72f) + 30f * lessTimeSpentPerPhaseMultiplier;
                    if (npc.ai[1] >= (masterMode ? 105f : 120f) + extraPhaseTime)
                    {
                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                    }

                    break;

                // Despawn.
                case 13:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item165, npc.Center);
                        npc.velocity = new Vector2(0f, -7f);
                    }

                    npc.velocity *= 0.95f;

                    npc.TargetClosest();
                    NPCAimedTarget targetData = npc.GetTargetData();

                    visible = false;

                    bool trueDespawnFlag = false;
                    bool shouldDespawn = false;
                    if (!trueDespawnFlag)
                    {
                        if (npc.AI_120_HallowBoss_IsGenuinelyEnraged() && !bossRush)
                        {
                            if (!Main.dayTime)
                                shouldDespawn = true;

                            if (Main.dayTime && Main.time >= 53400.0)
                                shouldDespawn = true;
                        }

                        trueDespawnFlag = trueDespawnFlag || shouldDespawn;
                    }

                    if (!trueDespawnFlag)
                    {
                        bool hasNoTarget = targetData.Invalid || npc.Distance(targetData.Center) > despawnDistanceGateValue;
                        trueDespawnFlag = trueDespawnFlag || hasNoTarget;
                    }

                    npc.alpha = Utils.Clamp(npc.alpha + trueDespawnFlag.ToDirectionInt() * 5, 0, 255);
                    bool alphaExtreme = npc.alpha == 0 || npc.alpha == 255;

                    int despawnDustAmt = 5;
                    for (int i = 0; i < despawnDustAmt; i++)
                    {
                        float despawnDustOpacity = MathHelper.Lerp(1.3f, 0.7f, npc.Opacity);
                        Color newColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                        int despawnRainbowDust = Dust.NewDust(npc.position - npc.Size * 0.5f, npc.width * 2, npc.height * 2, DustID.RainbowMk2, 0f, 0f, 0, newColor);
                        Main.dust[despawnRainbowDust].position = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height);
                        Main.dust[despawnRainbowDust].velocity *= Main.rand.NextFloat() * 0.8f;
                        Main.dust[despawnRainbowDust].noGravity = true;
                        Main.dust[despawnRainbowDust].scale = 0.9f + Main.rand.NextFloat() * 1.2f;
                        Main.dust[despawnRainbowDust].fadeIn = 0.4f + Main.rand.NextFloat() * 1.2f * despawnDustOpacity;
                        Main.dust[despawnRainbowDust].velocity += Vector2.UnitY * -2f;
                        Main.dust[despawnRainbowDust].scale = 0.35f;
                        if (despawnRainbowDust != 6000)
                        {
                            Dust dust = Dust.CloneDust(despawnRainbowDust);
                            dust.scale /= 2f;
                            dust.fadeIn *= 0.85f;
                            dust.color = new Color(255, 255, 255, 255);
                        }
                    }

                    npc.ai[1] += 1f;
                    if (!(npc.ai[1] >= 20f && alphaExtreme))
                        break;

                    if (npc.alpha == 255)
                    {
                        npc.active = false;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);

                        return false;
                    }

                    npc.ai[0] = 1f;
                    npc.ai[1] = 0f;
                    npc.netUpdate = true;
                    break;
            }

            npc.dontTakeDamage = !takeDamage;

            if (phase3)
                npc.defense = (int)Math.Round(npc.defDefense * 0.8);
            else if (phase2)
                npc.defense = (int)Math.Round(npc.defDefense * 1.2);
            else
                npc.defense = npc.defDefense;

            if ((npc.localAI[0] += 1f) >= 44f)
                npc.localAI[0] = 0f;

            if (visible)
                npc.alpha = Utils.Clamp(npc.alpha - 5, 0, 255);

            Lighting.AddLight(npc.Center, Vector3.One * npc.Opacity);

            return false;
        }

        public static bool VanillaEmpressofLightAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            Vector2 stopMovingLocation = new Vector2(-250f, -350f);
            Vector2 stopMovingLocation3 = new Vector2(0f, -450f);
            Vector2 stopMovingLocation4 = new Vector2(0f, -450f);
            Vector2 stopMovingLocation5 = new Vector2(-80f, -500f);

            // Rotation
            npc.rotation = npc.velocity.X * 0.005f;

            // Reset DR every frame
            calamityGlobalNPC.DR = 0.15f;

            bool phase2 = npc.AI_120_HallowBoss_IsInPhase2();
            bool expertMode = Main.expertMode;
            bool masterMode = Main.masterMode;
            bool expertModePhase2 = phase2 && expertMode;
            bool masterModePhase2 = phase2 && masterMode;
            float phase2LifeRatio = masterMode ? 0.7f : expertMode ? 0.6f : 0.5f;
            float phase3LifeRatio = masterMode ? 0.3f : 0.15f;
            bool genuinePhase2 = npc.life / (float)npc.lifeMax <= phase2LifeRatio;
            bool phase3 = npc.life / (float)npc.lifeMax <= phase3LifeRatio && expertMode;

            bool shouldBeInPhase2ButIsStillInPhase1 = npc.life / (float)npc.lifeMax <= phase2LifeRatio && !phase2;
            if (shouldBeInPhase2ButIsStillInPhase1)
                calamityGlobalNPC.DR = 0.99f;

            calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = shouldBeInPhase2ButIsStillInPhase1 || npc.ai[0] == 6f;

            bool enraged = NPC.ShouldEmpressBeEnraged();
            if (npc.life == npc.lifeMax && enraged && !npc.AI_120_HallowBoss_IsGenuinelyEnraged())
                npc.ai[3] += 2f;

            calamityGlobalNPC.CurrentlyEnraged = !BossRushEvent.BossRushActive && enraged;

            int projectileDamageMultiplier = enraged ? 2 : 1;

            bool becomeVisible = true;

            float moveSpeed = masterMode ? 0.6f : expertMode ? 0.55f : 0.5f;
            float desiredVelocity = masterMode ? 14.4f : expertMode ? 13.2f : 12f;
            float stopMovingDistance = 40f;
            float despawnDistance = 6400f;
            int lanceDamage = npc.GetProjectileDamage(ProjectileID.FairyQueenLance);
            int rainbowStreakDamage = npc.GetProjectileDamage(ProjectileID.HallowBossRainbowStreak);
            int lastingRainbowDamage = npc.GetProjectileDamage(ProjectileID.HallowBossLastingRainbow);
            int sunDanceDamage = npc.GetProjectileDamage(ProjectileID.FairyQueenSunDance);

            lanceDamage *= projectileDamageMultiplier;
            rainbowStreakDamage *= projectileDamageMultiplier;
            lastingRainbowDamage *= projectileDamageMultiplier;
            sunDanceDamage *= projectileDamageMultiplier;

            if (enraged)
                expertMode = true;

            bool takeDamage = true;
            int reducedAttackCooldown = 0;
            if (phase2)
                reducedAttackCooldown += 15;
            if (expertMode)
                reducedAttackCooldown += 5;

            // Variables for dust visuals on spawn and in phase 3
            float playSpawnSoundTime = 10f;
            float stopSpawningDustTime = 150f;
            float spawnTime = 180f;

            // Do visual stuff in phase 3
            float maxOpacity = phase3 ? 0.7f : 1f;
            int minAlpha = 255 - (int)(255 * maxOpacity);
            if (phase3)
            {
                if (calamityGlobalNPC.newAI[0] == playSpawnSoundTime)
                    SoundEngine.PlaySound(SoundID.Item161, npc.Center);

                if (calamityGlobalNPC.newAI[0] > playSpawnSoundTime && calamityGlobalNPC.newAI[0] < stopSpawningDustTime)
                    CreateSpawnDust(npc, false);

                calamityGlobalNPC.newAI[0] += 1f;
                if (calamityGlobalNPC.newAI[0] >= stopSpawningDustTime)
                {
                    calamityGlobalNPC.newAI[0] = playSpawnSoundTime + 1f;
                    npc.SyncExtraAI();
                }
            }

            switch ((int)npc.ai[0])
            {
                case 0:

                    // Avoid cheap bullshit.
                    npc.damage = 0;

                    if (npc.ai[1] == 0f)
                    {
                        npc.velocity = new Vector2(0f, 5f);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + new Vector2(0f, -80f), Vector2.Zero, ProjectileID.HallowBossDeathAurora, 0, 0f, Main.myPlayer);
                    }

                    if (npc.ai[1] == playSpawnSoundTime)
                        SoundEngine.PlaySound(SoundID.Item161, npc.Center);

                    npc.velocity *= 0.95f;
                    if (npc.ai[1] > playSpawnSoundTime && npc.ai[1] < stopSpawningDustTime)
                        CreateSpawnDust(npc);

                    npc.ai[1] += 1f;
                    becomeVisible = false;
                    takeDamage = false;
                    npc.Opacity = MathHelper.Clamp(npc.ai[1] / spawnTime, 0f, 1f);
                    if (npc.ai[1] >= spawnTime)
                    {
                        if (enraged && !npc.AI_120_HallowBoss_IsGenuinelyEnraged())
                            npc.ai[3] += 2f;

                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                        npc.TargetClosest();
                    }

                    break;

                case 1:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        float phaseTime = (phase2 ? 20f : 45f);
                        if (masterMode)
                            phaseTime /= 2f;
                        if (Main.getGoodWorld)
                            phaseTime /= 2f;
                        if (phaseTime < 10f)
                            phaseTime = 10f;

                        if (npc.ai[1] <= 10f)
                        {
                            if (npc.ai[1] == 0f)
                                npc.TargetClosest();

                            NPCAimedTarget targetData = npc.GetTargetData();
                            if (targetData.Invalid)
                            {
                                npc.ai[0] = 13f;
                                npc.ai[1] = 0f;
                                npc.ai[2] += 1f;
                                npc.velocity /= 4f;
                                npc.netUpdate = true;
                                break;
                            }

                            Vector2 center = targetData.Center;
                            npc.DirectionTo(center);
                            center += new Vector2(0f, -300f);
                            if (npc.Distance(center) > 200f)
                                center -= npc.DirectionTo(center) * 100f;

                            Vector2 distanceFromTarget = center - npc.Center;
                            float lerpValue = Utils.GetLerpValue(100f, 600f, distanceFromTarget.Length(), clamped: true);
                            float movementVelocity = distanceFromTarget.Length();
                            if (movementVelocity > 18f)
                                movementVelocity = 18f;

                            npc.velocity = Vector2.Lerp(distanceFromTarget.SafeNormalize(Vector2.Zero) * movementVelocity, distanceFromTarget / 6f, lerpValue);

                            npc.netUpdate = true;
                        }

                        if (npc.velocity.Length() > 16f && npc.ai[1] > 10f)
                            npc.velocity /= 2f;

                        npc.velocity *= 0.92f;
                        npc.ai[1] += 1f;
                        if (!(npc.ai[1] >= phaseTime))
                            break;

                        int phaseIncrement = (int)npc.ai[2];
                        int phase = 2;
                        int phaseDivisor = 0;
                        if (!phase2)
                        {
                            int num38 = phaseDivisor++;
                            int num39 = phaseDivisor++;
                            int num40 = phaseDivisor++;
                            int num41 = phaseDivisor++;
                            int num42 = phaseDivisor++;
                            int num43 = phaseDivisor++;
                            int num44 = phaseDivisor++;
                            int num45 = phaseDivisor++;
                            int num46 = phaseDivisor++;
                            int num47 = phaseDivisor++;
                            if (phaseIncrement % phaseDivisor == num38)
                                phase = 2;

                            if (phaseIncrement % phaseDivisor == num39)
                                phase = 8;

                            if (phaseIncrement % phaseDivisor == num40)
                                phase = 6;

                            if (phaseIncrement % phaseDivisor == num41)
                                phase = 8;

                            if (phaseIncrement % phaseDivisor == num42)
                                phase = 5;

                            if (phaseIncrement % phaseDivisor == num43)
                                phase = 2;

                            if (phaseIncrement % phaseDivisor == num44)
                                phase = 8;

                            if (phaseIncrement % phaseDivisor == num45)
                                phase = 4;

                            if (phaseIncrement % phaseDivisor == num46)
                                phase = 8;

                            if (phaseIncrement % phaseDivisor == num47)
                                phase = 5;

                            if (genuinePhase2)
                                phase = 10;
                        }

                        if (phase2)
                        {
                            int num48 = phaseDivisor++;
                            int num49 = phaseDivisor++;
                            int num50 = phaseDivisor++;
                            int num51 = -1;
                            if (expertMode)
                                num51 = phaseDivisor++;

                            int num52 = phaseDivisor++;
                            int num53 = phaseDivisor++;
                            int num54 = phaseDivisor++;
                            int num55 = phaseDivisor++;
                            int num56 = phaseDivisor++;
                            int num57 = phaseDivisor++;
                            if (phaseIncrement % phaseDivisor == num48)
                                phase = 7;

                            if (phaseIncrement % phaseDivisor == num49)
                                phase = phase3 ? 8 : 2;

                            if (phaseIncrement % phaseDivisor == num50)
                                phase = 8;

                            if (phaseIncrement % phaseDivisor == num52)
                                phase = 5;

                            if (phaseIncrement % phaseDivisor == num53)
                                phase = 2;

                            if (phaseIncrement % phaseDivisor == num54)
                                phase = phase3 ? 4 : 6;

                            if (phaseIncrement % phaseDivisor == num54)
                                phase = phase3 ? 7 : 6;

                            if (phaseIncrement % phaseDivisor == num55)
                                phase = 4;

                            if (phaseIncrement % phaseDivisor == num56)
                                phase = 8;

                            if (phaseIncrement % phaseDivisor == num51)
                                phase = 11;

                            if (phaseIncrement % phaseDivisor == num57)
                                phase = 12;
                        }

                        npc.TargetClosest();
                        NPCAimedTarget targetData2 = npc.GetTargetData();
                        bool transitionToEnrage = false;
                        if (npc.AI_120_HallowBoss_IsGenuinelyEnraged())
                        {
                            if (!Main.dayTime)
                                transitionToEnrage = true;

                            if (Main.dayTime && Main.time >= 53400D)
                                transitionToEnrage = true;
                        }

                        if (targetData2.Invalid || npc.Distance(targetData2.Center) > despawnDistance || transitionToEnrage)
                            phase = 13;

                        if (phase == 8 && targetData2.Center.X > npc.Center.X)
                            phase = 9;

                        if (expertMode && phase != 5 && phase != 12)
                            npc.velocity = npc.DirectionFrom(targetData2.Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * (float)(targetData2.Center.X > npc.Center.X).ToDirectionInt()) * 20f;

                        npc.ai[0] = phase;
                        npc.ai[1] = 0f;

                        int oneInXChance = masterMode ? 3 : 4;
                        if (phase3)
                            oneInXChance--;

                        float numPhaseIncrements = expertMode ? (Main.rand.NextBool(oneInXChance) ? 2f : 1f) : 1f;
                        npc.ai[2] += numPhaseIncrements;
                        npc.netUpdate = true;

                        break;
                    }

                case 2:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        if (npc.ai[1] == 0f)
                            SoundEngine.PlaySound(SoundID.Item164, npc.Center);

                        float extraPhaseTime = (masterMode ? 30f : expertMode ? 60f : 90f) - (float)reducedAttackCooldown;
                        Vector2 offset = new Vector2(-55f, -30f);
                        NPCAimedTarget targetData11 = npc.GetTargetData();
                        Vector2 targetCenter = (targetData11.Invalid ? npc.Center : targetData11.Center);
                        if (npc.Distance(targetCenter + stopMovingLocation) > stopMovingDistance)
                            npc.SimpleFlyMovement(npc.DirectionTo(targetCenter + stopMovingLocation).SafeNormalize(Vector2.Zero) * desiredVelocity, moveSpeed);

                        if (npc.ai[1] < 60f)
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + offset, 1, Utils.GetLerpValue(0f, 60f, npc.ai[1], clamped: true), npc);

                        int rainbowStreakGateValue = 3;
                        if (expertMode)
                            rainbowStreakGateValue = 2;
                        if (phase3)
                            rainbowStreakGateValue *= 2;

                        if ((int)npc.ai[1] % rainbowStreakGateValue == 0 && npc.ai[1] < 60f)
                        {
                            float ai3 = npc.ai[1] / 60f;
                            Vector2 rainbowStreakVelocity = new Vector2(0f, -6f - (masterMode ? (ai3 * 4f) : expertMode ? (ai3 * 2f) : 0f)).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
                            if (expertModePhase2)
                                rainbowStreakVelocity = new Vector2(0f, -10f - (masterMode ? (ai3 * 5f) : expertMode ? (ai3 * 2.5f) : 0f)).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, rainbowStreakVelocity, ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, npc.target, ai3);
                                if (phase3)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, -rainbowStreakVelocity, ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, npc.target, 1f - ai3);
                            }

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int num92 = (int)(npc.ai[1] / (float)rainbowStreakGateValue);
                                for (int num93 = 0; num93 < Main.maxPlayers; num93++)
                                {
                                    if (npc.Boss_CanShootExtraAt(num93, num92 % 3, 3, 2400f))
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, rainbowStreakVelocity, ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, num93, ai3);
                                }
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= 60f + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                /*case 3:
                    {
                        npc.ai[1] += 1f;
                        NPCAimedTarget targetData8 = npc.GetTargetData();
                        Vector2 vector23 = (targetData8.Invalid ? npc.Center : targetData8.Center);
                        if (npc.Distance(vector23 + vector2) > num3)
                            npc.SimpleFlyMovement(npc.DirectionTo(vector23 + vector2).SafeNormalize(Vector2.Zero) * num2, num);

                        if ((int)npc.ai[1] % 180 == 0)
                        {
                            Vector2 vector24 = new Vector2(0f, -100f);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), targetData8.Center + vector24, Vector2.Zero, ProjectileID.HallowBossDeathAurora, num5, 0f, Main.myPlayer);
                        }

                        if (npc.ai[1] >= 120f)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }*/

                case 4:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        float extraPhaseTime = 20 - reducedAttackCooldown;

                        if (npc.ai[1] == 0f)
                            SoundEngine.PlaySound(SoundID.Item162, npc.Center);

                        float lanceGateValue = masterMode ? 75f : 100f;
                        if (npc.ai[1] >= 6f && npc.ai[1] < 54f)
                        {
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(-55f, -20f), 2, Utils.GetLerpValue(0f, lanceGateValue, npc.ai[1], clamped: true), npc);
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(55f, -20f), 4, Utils.GetLerpValue(0f, lanceGateValue, npc.ai[1], clamped: true), npc);
                        }

                        NPCAimedTarget targetData = npc.GetTargetData();
                        Vector2 targetLocation = (targetData.Invalid ? npc.Center : targetData.Center);
                        if (npc.Distance(targetLocation + stopMovingLocation3) > stopMovingDistance)
                            npc.SimpleFlyMovement(npc.DirectionTo(targetLocation + stopMovingLocation3).SafeNormalize(Vector2.Zero) * desiredVelocity, moveSpeed);

                        int radialSpawnOffset = masterMode ? 6 : expertMode ? 5 : 4;
                        int lanceSpawnGateValue = masterMode ? 3 : 4;
                        if ((int)npc.ai[1] % lanceSpawnGateValue == 0 && npc.ai[1] < lanceGateValue)
                        {
                            int lanceAmount = phase3 ? 2 : 1;
                            for (int i = 0; i < lanceAmount; i++)
                            {
                                int num85 = (int)npc.ai[1] / lanceSpawnGateValue;
                                radialSpawnOffset += (masterMode ? 3 : 2) * i;
                                Vector2 radialOffset = Vector2.UnitX.RotatedBy((float)Math.PI / (float)(radialSpawnOffset * 2) + (float)num85 * ((float)Math.PI / (float)radialSpawnOffset) + 0f);
                                if (masterMode)
                                    radialOffset.X += (radialOffset.X > 0f ? -0.5f : 0.5f);
                                else if (!expertMode)
                                    radialOffset.X += (radialOffset.X > 0f ? 0.5f : -0.5f);

                                radialOffset = radialOffset.SafeNormalize(Vector2.UnitY);
                                float lanceSpawnDistance = 300f;
                                if (expertMode)
                                    lanceSpawnDistance = 450f;

                                Vector2 targetCenter = targetData.Center;
                                if (npc.Distance(targetCenter) > 2400f)
                                    continue;

                                if (Vector2.Dot(targetData.Velocity.SafeNormalize(Vector2.UnitY), radialOffset) > 0f)
                                    radialOffset *= -1f;

                                int targetVelocityOffset = 90;
                                Vector2 vector31 = targetCenter + targetData.Velocity * targetVelocityOffset;
                                Vector2 vector32 = targetCenter + radialOffset * lanceSpawnDistance - targetData.Velocity * 30f;
                                if (vector32.Distance(targetCenter) < lanceSpawnDistance)
                                {
                                    Vector2 vector33 = targetCenter - vector32;
                                    if (vector33 == Vector2.Zero)
                                        vector33 = radialOffset;

                                    vector32 = targetCenter - vector33.SafeNormalize(Vector2.UnitY) * lanceSpawnDistance;
                                }

                                Vector2 v3 = vector31 - vector32;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), vector32, Vector2.Zero, ProjectileID.FairyQueenLance, lanceDamage, 0f, Main.myPlayer, v3.ToRotation(), npc.ai[1] / lanceGateValue);

                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    continue;

                                int num88 = (int)(npc.ai[1] / (float)lanceSpawnGateValue);
                                for (int num89 = 0; num89 < Main.maxPlayers; num89++)
                                {
                                    if (!npc.Boss_CanShootExtraAt(num89, num88 % 3, 3, 2400f))
                                        continue;

                                    Player player2 = Main.player[num89];
                                    targetCenter = player2.Center;
                                    if (Vector2.Dot(player2.velocity.SafeNormalize(Vector2.UnitY), radialOffset) > 0f)
                                        radialOffset *= -1f;

                                    Vector2 vector34 = targetCenter + player2.velocity * targetVelocityOffset;
                                    vector32 = targetCenter + radialOffset * lanceSpawnDistance - player2.velocity * 30f;
                                    if (vector32.Distance(targetCenter) < lanceSpawnDistance)
                                    {
                                        Vector2 vector35 = targetCenter - vector32;
                                        if (vector35 == Vector2.Zero)
                                            vector35 = radialOffset;

                                        vector32 = targetCenter - vector35.SafeNormalize(Vector2.UnitY) * lanceSpawnDistance;
                                    }

                                    v3 = vector34 - vector32;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), vector32, Vector2.Zero, ProjectileID.FairyQueenLance, lanceDamage, 0f, Main.myPlayer, v3.ToRotation(), npc.ai[1] / lanceGateValue);
                                }
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= lanceGateValue + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 5:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        if (npc.ai[1] == 0f)
                            SoundEngine.PlaySound(SoundID.Item163, npc.Center);

                        float extraPhaseTime = masterMode ? 22f : 30f;
                        extraPhaseTime -= (float)reducedAttackCooldown;

                        Vector2 offset = new Vector2(55f, -30f);
                        Vector2 lastingRainbowInitialSpawnLocation = npc.Center + offset;

                        float lastingRainbowGateValue = 42f;
                        if (npc.ai[1] < lastingRainbowGateValue)
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + offset, 3, Utils.GetLerpValue(0f, lastingRainbowGateValue, npc.ai[1], clamped: true), npc);

                        NPCAimedTarget targetData = npc.GetTargetData();
                        Vector2 targetLocation = (targetData.Invalid ? npc.Center : targetData.Center);
                        if (npc.Distance(targetLocation + stopMovingLocation4) > stopMovingDistance)
                            npc.SimpleFlyMovement(npc.DirectionTo(targetLocation + stopMovingLocation4).SafeNormalize(Vector2.Zero) * desiredVelocity, moveSpeed);

                        if ((int)npc.ai[1] % (int)lastingRainbowGateValue == 0 && npc.ai[1] < lastingRainbowGateValue)
                        {
                            float lastingRainbowRandomRotation = MathHelper.TwoPi * Main.rand.NextFloat();
                            float numLastingRainbows = 13f;
                            for (float i = 0f; i < 1f; i += 1f / numLastingRainbows)
                            {
                                float ai1 = i;
                                Vector2 lastingRainbowVelocity = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 + MathHelper.TwoPi * ai1 + lastingRainbowRandomRotation);
                                Vector2 lastingRainbowSpawnLocation = lastingRainbowInitialSpawnLocation + lastingRainbowVelocity.RotatedBy(-MathHelper.PiOver2) * 30f;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), lastingRainbowSpawnLocation, lastingRainbowVelocity * (masterMode ? 10f : 8f), ProjectileID.HallowBossLastingRainbow, lastingRainbowDamage, 0f, Main.myPlayer, 0f, ai1);

                                if (phase3)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), lastingRainbowSpawnLocation, lastingRainbowVelocity * (masterMode ? 12f : 10f), ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, npc.target, ai1);
                                }
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= lastingRainbowGateValue + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 6:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        // Increase durability.
                        calamityGlobalNPC.DR = shouldBeInPhase2ButIsStillInPhase1 ? 0.99f : (BossRushEvent.BossRushActive ? 0.99f : 0.575f);

                        float extraPhaseTime = (masterMode ? 105 : expertMode ? 120 : 135) - reducedAttackCooldown;
                        Vector2 offset = new Vector2(0f, -100f);
                        Vector2 sunDanceSpawnLocation = npc.Center + offset;
                        NPCAimedTarget targetData = npc.GetTargetData();
                        Vector2 targetLocation = (targetData.Invalid ? npc.Center : targetData.Center);
                        if (npc.Distance(targetLocation + stopMovingLocation5) > stopMovingDistance)
                            npc.SimpleFlyMovement(npc.DirectionTo(targetLocation + stopMovingLocation5).SafeNormalize(Vector2.Zero) * desiredVelocity * 0.3f, moveSpeed * 0.7f);

                        float phaseGateValue = masterMode ? 120f : 180f;
                        int sunDanceGateValue = 60;
                        if ((int)npc.ai[1] % sunDanceGateValue == 0 && npc.ai[1] < phaseGateValue)
                        {
                            int sunDanceSpawnOffset = (int)npc.ai[1] / sunDanceGateValue;
                            int targetLocationX = ((targetData.Center.X > npc.Center.X) ? 1 : 0);
                            float numSunDancePetals = masterMode ? 9f : expertMode ? 8f : 6f;
                            float sunDanceIncrement = 1f / numSunDancePetals;
                            for (float i = 0f; i < 1f; i += sunDanceIncrement)
                            {
                                float radialOffset = (i + sunDanceIncrement * 0.5f + (float)sunDanceSpawnOffset * sunDanceIncrement * 0.5f) % 1f;
                                float ai = MathHelper.TwoPi * (radialOffset + (float)targetLocationX);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), sunDanceSpawnLocation, Vector2.Zero, ProjectileID.FairyQueenSunDance, sunDanceDamage, 0f, Main.myPlayer, ai, npc.whoAmI);
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= phaseGateValue + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 7:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        float extraPhaseTime = masterMode ? 34f : expertMode ? 40f : 20f;
                        float lanceGateValue = masterMode ? 34f : expertMode ? 40f : 60f;
                        float totalLanceWalls = expertMode ? 6f : 4f;
                        float lancePhaseTime = lanceGateValue * totalLanceWalls;

                        extraPhaseTime -= (float)reducedAttackCooldown;
                        NPCAimedTarget targetData = npc.GetTargetData();
                        Vector2 vector25 = (targetData.Invalid ? npc.Center : targetData.Center);
                        if (npc.Distance(vector25 + stopMovingLocation4) > stopMovingDistance)
                            npc.SimpleFlyMovement(npc.DirectionTo(vector25 + stopMovingLocation4).SafeNormalize(Vector2.Zero) * desiredVelocity * 0.4f, moveSpeed);

                        if ((float)(int)npc.ai[1] % lanceGateValue == 0f && npc.ai[1] < lancePhaseTime)
                        {
                            SoundEngine.PlaySound(SoundID.Item162, npc.Center);
                            Main.rand.NextFloat();
                            int lanceWallType = (int)npc.ai[1] / (int)lanceGateValue;
                            float numLances = phase3 ? (masterMode ? 18f : 15f) : 13f;
                            float distanceBetweenLances = phase3 ? (masterMode ? 125f : 135f) : 150f;
                            float lanceWallLength = numLances * distanceBetweenLances;
                            Vector2 lanceSpawnOffset = targetData.Center;
                            if (npc.Distance(lanceSpawnOffset) <= 3200f)
                            {
                                Vector2 vector26 = Vector2.Zero;
                                Vector2 lanceDirection = Vector2.UnitY;
                                float num77 = 0.4f;
                                float num78 = 1.4f;
                                float num79 = 1f;
                                if (expertMode)
                                {
                                    numLances += 5f;
                                    distanceBetweenLances += 50f;
                                    lanceWallLength *= (masterMode ? 0.75f : 0.5f);
                                }

                                switch (lanceWallType)
                                {
                                    case 0:
                                        lanceSpawnOffset += new Vector2((0f - lanceWallLength) / 2f, 0f) * num79;
                                        vector26 = new Vector2(0f, lanceWallLength);
                                        lanceDirection = Vector2.UnitX;
                                        break;
                                    case 1:
                                        lanceSpawnOffset += new Vector2(lanceWallLength / 2f, distanceBetweenLances / 2f) * num79;
                                        vector26 = new Vector2(0f, lanceWallLength);
                                        lanceDirection = -Vector2.UnitX;
                                        break;
                                    case 2:
                                        lanceSpawnOffset += new Vector2(0f - lanceWallLength, 0f - lanceWallLength) * num77 * num79;
                                        vector26 = new Vector2(lanceWallLength * num78, 0f);
                                        lanceDirection = new Vector2(1f, 1f);
                                        break;
                                    case 3:
                                        lanceSpawnOffset += new Vector2(lanceWallLength * num77 + distanceBetweenLances / 2f, (0f - lanceWallLength) * num77) * num79;
                                        vector26 = new Vector2((0f - lanceWallLength) * num78, 0f);
                                        lanceDirection = new Vector2(-1f, 1f);
                                        break;
                                    case 4:
                                        lanceSpawnOffset += new Vector2(0f - lanceWallLength, lanceWallLength) * num77 * num79;
                                        vector26 = new Vector2(lanceWallLength * num78, 0f);
                                        lanceDirection = lanceSpawnOffset.DirectionTo(targetData.Center);
                                        break;
                                    case 5:
                                        lanceSpawnOffset += new Vector2(lanceWallLength * num77 + distanceBetweenLances / 2f, lanceWallLength * num77) * num79;
                                        vector26 = new Vector2((0f - lanceWallLength) * num78, 0f);
                                        lanceDirection = lanceSpawnOffset.DirectionTo(targetData.Center);
                                        break;
                                }

                                for (float lance = 0f; lance <= 1f; lance += 1f / numLances)
                                {
                                    Vector2 origin = lanceSpawnOffset + vector26 * (lance - 0.5f);
                                    Vector2 lanceRotation = lanceDirection;
                                    if (expertMode)
                                    {
                                        Vector2 predictionVector = targetData.Velocity * 20f * lance;
                                        Vector2 value2 = origin.DirectionTo(targetData.Center + predictionVector);
                                        lanceRotation = Vector2.Lerp(lanceDirection, value2, 0.75f).SafeNormalize(Vector2.UnitY);
                                    }

                                    float ai2 = lance;
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), origin, Vector2.Zero, ProjectileID.FairyQueenLance, lanceDamage, 0f, Main.myPlayer, lanceRotation.ToRotation(), ai2);
                                }
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= lancePhaseTime + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 8:
                case 9:
                    {
                        float extraPhaseTime = 20 - reducedAttackCooldown;
                        float startDashTime = 40f;
                        float playDashSoundTime = 20f;
                        float endDashTime = phase3 ? 75f : 90f;
                        int dashStartDistance = phase3 ? 750 : 550;
                        int dashVelocity = phase3 ? 80 : 50;
                        float dashAcceleration = phase3 ? 0.08f : 0.05f;

                        int dashDirection = (npc.ai[0] != 8f ? 1 : -1);
                        AI_120_HallowBoss_DoMagicEffect(npc.Center, 5, Utils.GetLerpValue(startDashTime, endDashTime, npc.ai[1], clamped: true), npc);

                        if (npc.ai[1] <= startDashTime)
                        {
                            // Avoid cheap bullshit.
                            npc.damage = 0;

                            if (npc.ai[1] == playDashSoundTime)
                                SoundEngine.PlaySound(SoundID.Item160, npc.Center);

                            NPCAimedTarget targetData = npc.GetTargetData();
                            Vector2 destination = (targetData.Invalid ? npc.Center : targetData.Center) + new Vector2(dashDirection * -dashStartDistance, 0f);
                            npc.SimpleFlyMovement(npc.DirectionTo(destination).SafeNormalize(Vector2.Zero) * desiredVelocity, moveSpeed * 2f);
                            if (npc.ai[1] == startDashTime)
                                npc.velocity *= 0.3f;
                        }
                        else if (npc.ai[1] <= endDashTime)
                        {
                            npc.velocity = Vector2.Lerp(value2: new Vector2(dashDirection * dashVelocity, 0f), value1: npc.velocity, amount: dashAcceleration);
                            if (npc.ai[1] == endDashTime)
                                npc.velocity *= 0.7f;

                            npc.damage = (int)Math.Round(npc.defDamage * (enraged ? 3D : 1.5));
                        }
                        else
                        {
                            // Avoid cheap bullshit.
                            npc.damage = 0;

                            npc.velocity *= 0.92f;
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= endDashTime + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 10:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        float extraPhaseTime = 20 - reducedAttackCooldown;
                        if (npc.ai[1] == 0f)
                            SoundEngine.PlaySound(SoundID.Item161, npc.Center);

                        takeDamage = !(npc.ai[1] >= 30f) || !(npc.ai[1] <= 170f);
                        npc.velocity *= 0.95f;
                        if (npc.ai[1] == 90f)
                        {
                            if (npc.ai[3] == 0f)
                                npc.ai[3] = 1f;

                            if (npc.ai[3] == 2f)
                                npc.ai[3] = 3f;

                            npc.Center = npc.GetTargetData().Center + new Vector2(0f, -250f);
                            npc.netUpdate = true;
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= 180f + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.ai[2] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 11:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        if (npc.ai[1] == 0f)
                            SoundEngine.PlaySound(SoundID.Item162, npc.Center);

                        float extraPhaseTime = 20 - reducedAttackCooldown;
                        float lanceGateValue = masterMode ? 75f : 100f;
                        if (npc.ai[1] >= 6f && npc.ai[1] < 54f)
                        {
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(-55f, -20f), 2, Utils.GetLerpValue(0f, lanceGateValue, npc.ai[1], clamped: true), npc);
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + new Vector2(55f, -20f), 4, Utils.GetLerpValue(0f, lanceGateValue, npc.ai[1], clamped: true), npc);
                        }

                        NPCAimedTarget targetData = npc.GetTargetData();
                        Vector2 targetLocation = (targetData.Invalid ? npc.Center : targetData.Center);
                        if (npc.Distance(targetLocation + stopMovingLocation3) > stopMovingDistance)
                            npc.SimpleFlyMovement(npc.DirectionTo(targetLocation + stopMovingLocation3).SafeNormalize(Vector2.Zero) * desiredVelocity, moveSpeed);

                        if ((int)npc.ai[1] % 3 == 0 && npc.ai[1] < lanceGateValue)
                        {
                            int numLances = phase3 ? 2 : 1;
                            for (int i = 0; i < numLances; i++)
                            {
                                // Spawn another lance in the opposite location
                                bool oppositeLance = i == 1;

                                Vector2 vector13 = oppositeLance ? targetData.Velocity : -targetData.Velocity;
                                vector13.SafeNormalize(-Vector2.UnitY);
                                float lanceSpawnOffset = 100f;
                                Vector2 targetCenter = targetData.Center;
                                if (npc.Distance(targetCenter) > 2400f)
                                    continue;

                                int targetVelocityOffset = 90;
                                Vector2 vector14 = targetCenter + (oppositeLance ? -targetData.Velocity : targetData.Velocity) * targetVelocityOffset;
                                Vector2 lanceSpawnLocation = targetCenter + vector13 * lanceSpawnOffset;
                                if (lanceSpawnLocation.Distance(targetCenter) < lanceSpawnOffset)
                                {
                                    Vector2 vector16 = targetCenter - lanceSpawnLocation;
                                    if (vector16 == Vector2.Zero)
                                        vector16 = vector13;

                                    lanceSpawnLocation = targetCenter - vector16.SafeNormalize(Vector2.UnitY) * lanceSpawnOffset;
                                }

                                Vector2 rotationVector = vector14 - lanceSpawnLocation;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), lanceSpawnLocation, Vector2.Zero, ProjectileID.FairyQueenLance, lanceDamage, 0f, Main.myPlayer, rotationVector.ToRotation(), npc.ai[1] / lanceGateValue);

                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    continue;

                                int rotationIndex = (int)(npc.ai[1] / 3f);
                                for (int l = 0; l < Main.maxPlayers; l++)
                                {
                                    if (!npc.Boss_CanShootExtraAt(l, rotationIndex % 3, 3, 2400f))
                                        continue;

                                    Player player = Main.player[l];
                                    vector13 = oppositeLance ? player.velocity : -player.velocity;
                                    vector13.SafeNormalize(-Vector2.UnitY);
                                    lanceSpawnOffset = 100f;
                                    targetCenter = player.Center;
                                    targetVelocityOffset = 90;
                                    Vector2 lanceDestination = targetCenter + (oppositeLance ? -player.velocity : player.velocity) * targetVelocityOffset;
                                    lanceSpawnLocation = targetCenter + vector13 * lanceSpawnOffset;
                                    if (lanceSpawnLocation.Distance(targetCenter) < lanceSpawnOffset)
                                    {
                                        Vector2 vector18 = targetCenter - lanceSpawnLocation;
                                        if (vector18 == Vector2.Zero)
                                            vector18 = vector13;

                                        lanceSpawnLocation = targetCenter - vector18.SafeNormalize(Vector2.UnitY) * lanceSpawnOffset;
                                    }

                                    rotationVector = lanceDestination - lanceSpawnLocation;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), lanceSpawnLocation, Vector2.Zero, ProjectileID.FairyQueenLance, lanceDamage, 0f, Main.myPlayer, rotationVector.ToRotation(), npc.ai[1] / lanceGateValue);
                                }
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= lanceGateValue + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 12:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        float extraPhaseTime = (masterMode ? 30f : expertMode ? 60f : 90f) - (float)reducedAttackCooldown;
                        Vector2 offset = new Vector2(-55f, -30f);
                        if (npc.ai[1] == 0f)
                        {
                            SoundEngine.PlaySound(SoundID.Item165, npc.Center);
                            npc.velocity = new Vector2(0f, -12f);
                        }

                        npc.velocity *= 0.95f;
                        bool shootRainbowStreaks = npc.ai[1] < 60f && npc.ai[1] >= 10f;
                        if (shootRainbowStreaks)
                            AI_120_HallowBoss_DoMagicEffect(npc.Center + offset, 1, Utils.GetLerpValue(0f, 60f, npc.ai[1], clamped: true), npc);

                        int rainbowStreakGateValue = 6;
                        if (expertMode)
                            rainbowStreakGateValue = 4;
                        if (phase3)
                            rainbowStreakGateValue *= 2;

                        float radialOffset = (npc.ai[1] - 10f) / 50f;
                        if ((int)npc.ai[1] % rainbowStreakGateValue == 0 && shootRainbowStreaks)
                        {
                            Vector2 rainbowStreakVelocity = (rainbowStreakVelocity = new Vector2(0f, -20f - (phase3 ? ((masterMode ? 10f : 5f) * radialOffset) : 0f)).RotatedBy(MathHelper.TwoPi * radialOffset));
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, rainbowStreakVelocity, ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, npc.target, radialOffset);
                                if (phase3)
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, -rainbowStreakVelocity, ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, npc.target, 1f - radialOffset);
                            }

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int num24 = (int)(npc.ai[1] % (float)rainbowStreakGateValue);
                                for (int j = 0; j < Main.maxPlayers; j++)
                                {
                                    if (npc.Boss_CanShootExtraAt(j, num24 % 3, 3, 2400f))
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + offset, rainbowStreakVelocity, ProjectileID.HallowBossRainbowStreak, rainbowStreakDamage, 0f, Main.myPlayer, j, radialOffset);
                                }
                            }
                        }

                        npc.ai[1] += 1f;
                        if (npc.ai[1] >= 60f + extraPhaseTime)
                        {
                            npc.ai[0] = 1f;
                            npc.ai[1] = 0f;
                            npc.netUpdate = true;
                        }

                        break;
                    }

                case 13:
                    {
                        // Avoid cheap bullshit.
                        npc.damage = 0;

                        if (npc.ai[1] == 0f)
                        {
                            SoundEngine.PlaySound(SoundID.Item165, npc.Center);
                            npc.velocity = new Vector2(0f, -7f);
                        }

                        npc.velocity *= 0.95f;
                        npc.TargetClosest();
                        NPCAimedTarget targetData = npc.GetTargetData();
                        becomeVisible = false;
                        bool turnInvisible = false;
                        bool despawn = false;
                        if (!turnInvisible)
                        {
                            if (npc.AI_120_HallowBoss_IsGenuinelyEnraged())
                            {
                                if (!Main.dayTime)
                                    despawn = true;

                                if (Main.dayTime && Main.time >= 53400D)
                                    despawn = true;
                            }

                            turnInvisible = turnInvisible || despawn;
                        }

                        if (!turnInvisible)
                        {
                            bool noValidTarget = targetData.Invalid || npc.Distance(targetData.Center) > despawnDistance;
                            turnInvisible = turnInvisible || noValidTarget;
                        }

                        npc.alpha = Utils.Clamp(npc.alpha + turnInvisible.ToDirectionInt() * 5, 0, 255);
                        bool flag10 = npc.alpha == 0 || npc.alpha == 255;
                        int totalDust = 5;
                        for (int i = 0; i < totalDust; i++)
                        {
                            float num19 = MathHelper.Lerp(1.3f, 0.7f, npc.Opacity);
                            Color newColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                            int num20 = Dust.NewDust(npc.position - npc.Size * 0.5f, npc.width * 2, npc.height * 2, DustID.RainbowMk2, 0f, 0f, 0, newColor);
                            Main.dust[num20].position = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height);
                            Main.dust[num20].velocity *= Main.rand.NextFloat() * 0.8f;
                            Main.dust[num20].noGravity = true;
                            Main.dust[num20].scale = 0.9f + Main.rand.NextFloat() * 1.2f;
                            Main.dust[num20].fadeIn = 0.4f + Main.rand.NextFloat() * 1.2f * num19;
                            Main.dust[num20].velocity += Vector2.UnitY * -2f;
                            Main.dust[num20].scale = 0.35f;
                            if (num20 != Main.maxDust)
                            {
                                Dust dust = Dust.CloneDust(num20);
                                dust.scale /= 2f;
                                dust.fadeIn *= 0.85f;
                                dust.color = new Color(255, 255, 255, 255);
                            }
                        }

                        npc.ai[1] += 1f;
                        if (!(npc.ai[1] >= 20f && flag10))
                            break;

                        if (npc.alpha == 255)
                        {
                            npc.active = false;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);

                            return false;
                        }

                        npc.ai[0] = 1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                        break;
                    }
            }

            npc.dontTakeDamage = !takeDamage;

            if (phase3)
                npc.defense = (int)Math.Round(npc.defDefense * 0.8);
            else if (phase2)
                npc.defense = (int)Math.Round(npc.defDefense * 1.2);
            else
                npc.defense = npc.defDefense;

            if ((npc.localAI[0] += 1f) >= 44f)
                npc.localAI[0] = 0f;

            if (becomeVisible)
                npc.alpha = Utils.Clamp(npc.alpha - 5, minAlpha, 255);

            Lighting.AddLight(npc.Center, Vector3.One * npc.Opacity);

            return false;
        }

        private static void CreateSpawnDust(NPC npc, bool useAI = true)
        {
            int spawnDustAmount = 2;
            float timer = useAI ? npc.ai[1] : npc.Calamity().newAI[0];
            float spawnTime = 180f;
            for (int i = 0; i < spawnDustAmount; i++)
            {
                float fadeInScalar = MathHelper.Lerp(1.3f, 0.7f, npc.Opacity) * Utils.GetLerpValue(0f, 120f, timer, clamped: true);
                Color newColor = Main.hslToRgb(timer / spawnTime, 1f, 0.5f);
                int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.RainbowMk2, 0f, 0f, 0, newColor);
                Main.dust[dust].position = npc.Center + Main.rand.NextVector2Circular((float)npc.width * 3f, (float)npc.height * 3f) + new Vector2(0f, -150f);
                Main.dust[dust].velocity *= Main.rand.NextFloat() * 0.8f;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = 0.6f + Main.rand.NextFloat() * 0.7f * fadeInScalar;
                Main.dust[dust].velocity += Vector2.UnitY * 3f;
                Main.dust[dust].scale = 0.35f;
                if (dust != Main.maxDust)
                {
                    Dust dust2 = Dust.CloneDust(dust);
                    dust2.scale /= 2f;
                    dust2.fadeIn *= 0.85f;
                    dust2.color = new Color(255, 255, 255, 255);
                }
            }
        }

        private static void AI_120_HallowBoss_DoMagicEffect(Vector2 spot, int effectType, float progress, NPC npc)
        {
            float magicDustSpawnArea = 4f;
            float magicDustColorMult = 1f;
            float fadeIn = 0f;
            float magicDustPosChange = 0.5f;
            int magicAmt = 2;
            int magicDustType = 267;
            switch (effectType)
            {
                case 1:
                    magicDustColorMult = 0.5f;
                    fadeIn = 2f;
                    magicDustPosChange = 0f;
                    break;
                case 2:
                case 4:
                    magicDustSpawnArea = 50f;
                    magicDustColorMult = 0.5f;
                    fadeIn = 0f;
                    magicDustPosChange = 0f;
                    magicAmt = 4;
                    break;
                case 3:
                    magicDustSpawnArea = 30f;
                    magicDustColorMult = 0.1f;
                    fadeIn = 2.5f;
                    magicDustPosChange = 0f;
                    break;
                case 5:
                    if (progress == 0f)
                    {
                        magicAmt = 0;
                    }
                    else
                    {
                        magicAmt = 5;
                        magicDustType = Main.rand.Next(86, 92);
                    }
                    if (progress >= 1f)
                        magicAmt = 0;
                    break;
            }

            for (int i = 0; i < magicAmt; i++)
            {
                Dust dust = Dust.NewDustPerfect(spot, magicDustType, Main.rand.NextVector2CircularEdge(magicDustSpawnArea, magicDustSpawnArea) * (Main.rand.NextFloat() * (1f - magicDustPosChange) + magicDustPosChange), 0, Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f), (Main.rand.NextFloat() * 2f + 2f) * magicDustColorMult);
                dust.fadeIn = fadeIn;
                dust.noGravity = true;
                switch (effectType)
                {
                    case 2:
                    case 4:
                        {
                            dust.velocity *= 0.005f;
                            dust.scale = 3f * Utils.GetLerpValue(0.7f, 0f, progress, clamped: true) * Utils.GetLerpValue(0f, 0.3f, progress, clamped: true);
                            dust.velocity = (MathHelper.TwoPi * (i / 4f) + MathHelper.PiOver4).ToRotationVector2() * 8f * Utils.GetLerpValue(1f, 0f, progress, clamped: true);
                            dust.velocity += npc.velocity * 0.3f;
                            float magicDustColorChange = 0f;
                            if (effectType == 4)
                                magicDustColorChange = 0.5f;

                            dust.color = Main.hslToRgb((i / 5f + magicDustColorChange + progress * 0.5f) % 1f, 1f, 0.5f);
                            dust.color.A /= 2;
                            dust.alpha = 127;
                            break;
                        }
                    case 5:
                        if (progress == 0f)
                        {
                            dust.customData = npc;
                            dust.scale = 1.5f;
                            dust.fadeIn = 0f;
                            dust.velocity = new Vector2(0f, -1f) + Main.rand.NextVector2Circular(1f, 1f);
                            dust.color = new Color(255, 255, 255, 80) * 0.3f;
                        }
                        else
                        {
                            dust.color = Main.hslToRgb(progress * 2f % 1f, 1f, 0.5f);
                            dust.alpha = 0;
                            dust.scale = 1f;
                            dust.fadeIn = 1.3f;
                            dust.velocity *= 3f;
                            dust.velocity.X *= 0.1f;
                            dust.velocity += npc.velocity * 1f;
                        }
                        break;
                }
            }
        }
    }
}
