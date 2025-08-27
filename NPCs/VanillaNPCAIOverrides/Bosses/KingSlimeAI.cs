using System;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.UI;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public static class KingSlimeAI
    {
        public static readonly SoundStyle SpawnCrystalSound = new SoundStyle("CalamityMod/Sounds/Custom/KingSlimeJewelSpawn");
        public static readonly SoundStyle ShootSound = new SoundStyle("CalamityMod/Sounds/Custom/RedJewelFire");

        public static bool BuffedKingSlimeAI(NPC npc, Mod mod)
        {
            float lifeRatio = npc.life / (float)npc.lifeMax;
            float lifeRatio2 = lifeRatio;

            float teleportScale = 1f;
            bool teleporting = false;
            bool teleported = false;
            npc.aiAction = 0;
            float teleportScaleSpeed = 2f;
            if (Main.getGoodWorld)
            {
                teleportScaleSpeed -= 1f - lifeRatio;
                teleportScale *= teleportScaleSpeed;
            }

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Higher velocity jumps phase
            bool phase2 = lifeRatio < 0.75f;

            // In death: 75-55 = ruby, 50-35 = emerald, 35-0 = switches back and forth
            bool rubySpawnPhaseActive = death ? (lifeRatio < 0.75f) : (lifeRatio < 0.5f);

            bool redCrystalAlive = NPC.AnyNPCs(ModContent.NPCType<KingSlimeJewelRuby>());

            // npc.Calamity().newAI[0] as a flag for Ruby spawn state: 0f = can spawn, 1f = spawned
            bool rubySpawnedForCurrentPhase = npc.Calamity().newAI[0] == 1f;

            int setDamage = npc.defDamage; // Defined externally
            npc.defense = npc.defDefense;

            if (rubySpawnPhaseActive && !redCrystalAlive && !rubySpawnedForCurrentPhase)
            {
                npc.Calamity().newAI[0] = 1f;
                npc.SyncExtraAI();

                Vector2 vector = npc.Center + new Vector2(-40f, -(float)npc.height / 2) * npc.scale;
                int totalDustPerCrystalSpawn = 20;
                for (int i = 0; i < totalDustPerCrystalSpawn; i++)
                {
                    int rubyDust = Dust.NewDust(vector, npc.width / 2, npc.height / 2, DustID.GemRuby, 0f, 0f, 100, default, 2f);
                    Main.dust[rubyDust].velocity *= 2f;
                    Main.dust[rubyDust].noGravity = true;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[rubyDust].scale = 0.5f;
                        Main.dust[rubyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                SoundEngine.PlaySound(SpawnCrystalSound with { Volume = 2f }, npc.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int jewel = NPC.NewNPC(npc.GetSource_FromAI(), (int)vector.X, (int)vector.Y, ModContent.NPCType<KingSlimeJewelRuby>());
                    Main.npc[jewel].localAI[2] = npc.whoAmI;
                    Main.npc[jewel].velocity.Y = -6;
                }
            }
            else if (rubySpawnedForCurrentPhase && !rubySpawnPhaseActive)
            {
                npc.Calamity().newAI[0] = 0f;
                npc.SyncExtraAI();
            }

            // Set up health value for spawning slimes
            if (npc.ai[3] == 0f && npc.life > 0)
                npc.ai[3] = npc.lifeMax;

            // Spawn with attack delay
            if (npc.localAI[3] == 0f)
            {
                npc.localAI[3] = 1f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.ai[0] = -100f;

                    CalamityUtils.CalamityTargeting(npc, default);

                    npc.netUpdate = true;
                }
            }

            // Despawn
            int despawnDistance = 60;
            int forceDespawnDistance = 300;
            if ((Main.player[npc.target].dead && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > despawnDistance) || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > forceDespawnDistance)
            {
                CalamityUtils.CalamityTargeting(npc, default);

                if ((Main.player[npc.target].dead && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > despawnDistance) || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > forceDespawnDistance)
                {
                    if (npc.timeLeft > 10)
                        npc.timeLeft = 10;

                    if (Main.player[npc.target].Center.X < npc.Center.X)
                        npc.direction = 1;
                    else
                        npc.direction = -1;
                }
            }

            // Faster fall
            if (npc.velocity.Y > 0f)
            {
                float fallSpeedBonus = (death ? 0.1f : 0f) + (!redCrystalAlive ? 0.1f : 0f);
                npc.velocity.Y += fallSpeedBonus;
            }

            // Activate teleport
            float teleportGateValue = 480f;
            if (!Main.player[npc.target].dead && npc.ai[2] >= teleportGateValue && npc.ai[1] < 5f && npc.velocity.Y == 0f)
            {
                npc.damage = 0;

                npc.ai[2] = 0f;
                npc.ai[0] = 0f;
                npc.ai[1] = 5f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    GetPlaceToTeleportTo(npc);
            }

            if (!Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0) || Math.Abs(npc.Top.Y - Main.player[npc.target].Bottom.Y) > 160f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.localAI[0] += 1f;
            }
            else if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.localAI[0] -= 1f;

                if (npc.localAI[0] < 0f)
                    npc.localAI[0] = 0f;
            }

            if (npc.timeLeft < 10 && (npc.ai[0] != 0f || npc.ai[1] != 0f))
            {
                npc.ai[0] = 0f;
                npc.ai[1] = 0f;
                npc.netUpdate = true;
                teleporting = false;
            }

            // Closer to activating teleport
            if (npc.ai[2] < teleportGateValue)
            {
                if (!Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0) || Math.Abs(npc.Top.Y - Main.player[npc.target].Bottom.Y) > (death ? 160f : 320f))
                    npc.ai[2] += death ? 3f : 2f;
                else
                    npc.ai[2] += 1f;
            }

            // Slow down while teleporting
            if (npc.ai[1] == 5f || npc.ai[1] == 6f)
            {
                if (Math.Abs(npc.velocity.X) > 0.1f)
                {
                    npc.velocity.X *= 0.8f;
                    if (Math.Abs(npc.velocity.X) <= 0.1f)
                        npc.velocity.X = 0f;
                }
            }

            // Teleport
            if (npc.ai[1] == 5f)
            {
                npc.damage = 0;

                teleporting = true;
                npc.aiAction = 1;

                float teleportRate = redCrystalAlive ? 1f : 2f;
                if (death)
                    teleportRate *= 2f;

                npc.ai[0] += teleportRate;
                teleportScale = MathHelper.Clamp((60f - npc.ai[0]) / 60f, 0f, 1f);
                teleportScale = 0.5f + teleportScale * 0.5f;
                if (Main.getGoodWorld)
                    teleportScale *= teleportScaleSpeed;

                if (npc.ai[0] >= 60f)
                    teleported = true;

                if (npc.ai[0] == 60f && !Main.dedServ) 
                    Gore.NewGore(npc.GetSource_FromAI(), npc.Center + new Vector2(-40f, -(float)npc.height / 2), npc.velocity, 734, 1f);

                if (npc.ai[0] >= 60f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.Bottom = new Vector2(npc.localAI[1], npc.localAI[2]);
                    npc.ai[1] = 6f;
                    npc.ai[0] = 0f;
                    npc.netUpdate = true;
                }

                if (Main.netMode == NetmodeID.MultiplayerClient && npc.ai[0] >= 120f)
                {
                    npc.ai[1] = 6f;
                    npc.ai[0] = 0f;
                }

                if (!teleported)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int slimeDust = Dust.NewDust(npc.position + Vector2.UnitX * -20f, npc.width + 40, npc.height, DustID.TintableDust, npc.velocity.X, npc.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
                        Main.dust[slimeDust].noGravity = true;
                        Main.dust[slimeDust].velocity *= 0.5f;
                    }
                }
            }

            // Post-teleport
            else if (npc.ai[1] == 6f)
            {
                npc.damage = 0;

                teleporting = true;
                npc.aiAction = 0;

                float teleportRate = redCrystalAlive ? 1f : 2f;
                if (death)
                    teleportRate *= 2f;


                npc.ai[0] += teleportRate;
                teleportScale = MathHelper.Clamp(npc.ai[0] / 30f, 0f, 1f);
                teleportScale = 0.5f + teleportScale * 0.5f;
                if (Main.getGoodWorld)
                    teleportScale *= teleportScaleSpeed;

                if (npc.ai[0] >= 30f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.ai[1] = 0f;
                    npc.ai[0] = -15f;
                    npc.netUpdate = true;

                    CalamityUtils.CalamityTargeting(npc, default);
                }

                if (Main.netMode == NetmodeID.MultiplayerClient && npc.ai[0] >= 60f)
                {
                    npc.ai[1] = 0f;
                    npc.ai[0] = -15f;

                    CalamityUtils.CalamityTargeting(npc, default);
                }

                for (int j = 0; j < 10; j++)
                {
                    int slimyDust = Dust.NewDust(npc.position + Vector2.UnitX * -20f, npc.width + 40, npc.height, DustID.TintableDust, npc.velocity.X, npc.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
                    Main.dust[slimyDust].noGravity = true;
                    Main.dust[slimyDust].velocity *= 2f;
                }
            }

            npc.noTileCollide = false;

            // Jump
            if (npc.velocity.Y == 0f)
            {
                npc.damage = 0;

                npc.velocity.X *= 0.8f;
                if (npc.velocity.X > -0.1f && npc.velocity.X < 0.1f)
                    npc.velocity.X = 0f;

                if (!teleporting)
                {
                    npc.ai[0] += MathHelper.Lerp(1f, 8f, 1f - lifeRatio);
                    if (npc.ai[0] >= 0f)
                    {
                        npc.damage = setDamage;

                        npc.netUpdate = true;

                        CalamityUtils.CalamityTargeting(npc, default);

                        float distanceBelowTarget = npc.position.Y - (Main.player[npc.target].position.Y + 80f);
                        float speedMult = 1f;
                        if (distanceBelowTarget > 0f)
                            speedMult += distanceBelowTarget * 0.002f;

                        if (speedMult > 2f)
                            speedMult = 2f;

                        bool deathModeRapidHops = death && lifeRatio < 0.2f;
                        if (deathModeRapidHops)
                            npc.ai[1] = 2f;

                        if (npc.ai[1] == 3f)
                        {
                            npc.velocity.Y = -10f * speedMult;
                            npc.velocity.X += (phase2 ? (death ? 5.35f : 4.5f) : 3.5f) * npc.direction;
                            npc.ai[0] = -100f;
                            npc.ai[1] = 0f;
                        }
                        else if (npc.ai[1] == 2f)
                        {
                            npc.velocity.Y = -6f * speedMult;
                            npc.velocity.X += (phase2 ? (deathModeRapidHops ? 7.65f : death ? 6.25f : 5.5f) : 4.5f) * npc.direction;
                            npc.ai[0] = -60f;

                            if (!deathModeRapidHops)
                                npc.ai[1] += 1f;
                        }
                        else
                        {
                            npc.velocity.Y = -8f * speedMult;
                            npc.velocity.X += (phase2 ? (death ? 5.75f : 5f) : 4f) * npc.direction;
                            npc.ai[0] = -60f;
                            npc.ai[1] += 1f;
                        }

                        if (death)
                            npc.velocity.X *= 1.2f;

                        npc.noTileCollide = true;
                    }
                    else if (npc.ai[0] >= -30f)
                        npc.aiAction = 1;
                }
            }

            else if (npc.target < Main.maxPlayers) // Velocity factoring
            {
                float jumpVelocityLimit = redCrystalAlive ? 3f : 4.5f;
                if (death)
                    jumpVelocityLimit += 2.25f;
                if (Main.getGoodWorld)
                    jumpVelocityLimit = 8f;

                if ((npc.direction == 1 && npc.velocity.X < jumpVelocityLimit) || (npc.direction == -1 && npc.velocity.X > -jumpVelocityLimit))
                {
                    if ((npc.direction == -1 && npc.velocity.X < 0.1) || (npc.direction == 1 && npc.velocity.X > -0.1))
                    {
                        npc.velocity.X += (death ? 0.25f : 0.2f) * npc.direction;
                        if (death)
                            npc.velocity.X += 0.25f * npc.direction;
                    }
                    else
                    {
                        npc.velocity.X *= death ? 0.92f : 0.93f;
                        if (death)
                            npc.velocity.X *= 0.9f;
                    }
                }

                if (!Main.player[npc.target].dead)
                {
                    if (npc.velocity.Y > 0f && npc.Bottom.Y > Main.player[npc.target].Top.Y)
                        npc.noTileCollide = false;
                    else if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].Center, 1, 1) && !Collision.SolidCollision(npc.position, npc.width, npc.height))
                        npc.noTileCollide = false;
                    else
                        npc.noTileCollide = true;
                }
            }

            int idleSlimeDust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.TintableDust, npc.velocity.X, npc.velocity.Y, 255, new Color(0, 80, 255, 80), npc.scale * 1.2f);
            Main.dust[idleSlimeDust].noGravity = true;
            Main.dust[idleSlimeDust].velocity *= 0.5f;

            if (npc.life <= 0)
                return false;

            // Adjust size based on max health
            float maxScale = Main.getGoodWorld ? 3f : death ? 2.5f : 1.5f;
            float minScale = 0.75f;
            float maxScaledValue = maxScale - minScale;

            // Inverse scaling in FTW
            if (Main.getGoodWorld)
                lifeRatio = (maxScaledValue - lifeRatio * maxScaledValue) + minScale;
            else
                lifeRatio = lifeRatio * maxScaledValue + minScale;

            lifeRatio *= teleportScale;
            if (lifeRatio != npc.scale)
            {
                npc.position.X += npc.width / 2;
                npc.position.Y += npc.height;
                npc.scale = lifeRatio;
                npc.width = (int)(98f * npc.scale);
                npc.height = (int)(92f * npc.scale);
                npc.position.X -= npc.width / 2;
                npc.position.Y -= npc.height;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int slimeSpawnThreshold = (int)(npc.lifeMax * 0.03);
                if (npc.life + slimeSpawnThreshold < npc.ai[3])
                {
                    npc.ai[3] = npc.life;
                    int slimeAmt = death ? 1 : Main.rand.Next(1, 3);
                    for (int i = 0; i < slimeAmt; i++)
                    {
                        float minLowerLimit = 0f;
                        float maxLowerLimit = 2f;
                        int minTypeChoice = (int)MathHelper.Lerp(minLowerLimit, 5f, 1f - lifeRatio2);
                        int maxTypeChoice = (int)MathHelper.Lerp(maxLowerLimit, 7f, 1f - lifeRatio2);

                        int npcType;
                        switch (Main.rand.Next(minTypeChoice, maxTypeChoice + 1))
                        {
                            default:
                                npcType = NPCID.SlimeSpiked;
                                break;
                            case 0:
                                npcType = NPCID.GreenSlime;
                                break;
                            case 1:
                                npcType = Main.player[npc.target].ZoneJungle ? NPCID.JungleSlime : Main.player[npc.target].ZoneSnow ? NPCID.IceSlime : NPCID.BlueSlime;
                                break;
                            case 2:
                                npcType = Main.raining ? NPCID.UmbrellaSlime : NPCID.BlueSlime;
                                break;
                            case 3:
                                npcType = NPCID.RedSlime;
                                break;
                            case 4:
                                npcType = NPCID.PurpleSlime;
                                break;
                            case 5:
                                npcType = NPCID.YellowSlime;
                                break;
                            case 6:
                                npcType = Main.player[npc.target].ZoneJungle ? NPCID.SpikedJungleSlime : Main.player[npc.target].ZoneSnow ? NPCID.SpikedIceSlime : NPCID.SlimeSpiked;
                                break;
                        }

                        if ((Main.raining && Main.hardMode) && Main.rand.NextBool(50))
                            npcType = NPCID.RainbowSlime;

                        if (death) // 50% chance to spawn a spiked slime instead of the above npcType value
                        {
                            if (Main.rand.NextBool())
                            {
                                npcType = Main.player[npc.target].ZoneJungle ? NPCID.SpikedJungleSlime : Main.player[npc.target].ZoneSnow ? NPCID.SpikedIceSlime : NPCID.SlimeSpiked;
                            }
                        }

                        if (Main.rand.NextBool(100))
                            npcType = NPCID.Pinky;

                        if (Main.zenithWorld)
                            npcType = NPCID.RainbowSlime;

                        int offset = 16;
                        int spawnZoneWidth = npc.width - offset * 2;
                        int spawnZoneHeight = npc.height - offset * 2;
                        int x = (int)(npc.position.X + offset + Main.rand.Next(spawnZoneWidth));
                        int y = (int)(npc.position.Y + offset + Main.rand.Next(spawnZoneHeight));
                        int slimeSpawns = NPC.NewNPC(npc.GetSource_FromAI(), x, y, npcType);
                        Main.npc[slimeSpawns].SetDefaults(npcType);
                        Main.npc[slimeSpawns].velocity.X = Main.rand.Next(-15, 16) * 0.1f;
                        Main.npc[slimeSpawns].velocity.Y = Main.rand.Next(-30, 31) * 0.1f;
                        Main.npc[slimeSpawns].ai[0] = -1000 * Main.rand.Next(3);
                        Main.npc[slimeSpawns].ai[1] = 0f;

                        if (Main.dedServ && slimeSpawns < Main.maxNPCs)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slimeSpawns);
                    }
                }
            }
            return false;
        }

        public static void GetPlaceToTeleportTo(NPC npc)
        {
            CalamityTargetingParameters options = CalamityTargetingParameters.Defaults;
            options.faceTarget = false;
            CalamityUtils.CalamityTargeting(npc, options);

            float distanceAhead = 800f;
            Vector2 randomDefault = Main.rand.NextBool() ? Vector2.UnitX : -Vector2.UnitX;
            Vector2 vectorAimedAheadOfTarget = Main.player[npc.target].Center + new Vector2((float)Math.Round(Main.player[npc.target].velocity.X), 0f).SafeNormalize(randomDefault) * distanceAhead;
            Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
            if (predictiveTeleportPoint.X < 10)
                predictiveTeleportPoint.X = 10;
            if (predictiveTeleportPoint.X > Main.maxTilesX - 10)
                predictiveTeleportPoint.X = Main.maxTilesX - 10;
            if (predictiveTeleportPoint.Y < 10)
                predictiveTeleportPoint.Y = 10;
            if (predictiveTeleportPoint.Y > Main.maxTilesY - 10)
                predictiveTeleportPoint.Y = Main.maxTilesY - 10;

            int randomPredictiveTeleportOffset = 5;
            int teleportTries = 0;
            while (teleportTries < 100)
            {
                teleportTries++;
                int teleportTileX = Main.rand.Next(predictiveTeleportPoint.X - randomPredictiveTeleportOffset, predictiveTeleportPoint.X + randomPredictiveTeleportOffset + 1);
                int teleportTileY = Main.rand.Next(predictiveTeleportPoint.Y - randomPredictiveTeleportOffset, predictiveTeleportPoint.Y);

                if (!Main.tile[teleportTileX, teleportTileY].HasUnactuatedTile)
                {
                    bool canTeleportToTile = true;
                    if (canTeleportToTile && Main.tile[teleportTileX, teleportTileY].LiquidType == LiquidID.Lava)
                        canTeleportToTile = false;
                    if (canTeleportToTile && !Collision.CanHitLine(npc.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
                        canTeleportToTile = false;

                    if (canTeleportToTile)
                    {
                        npc.localAI[1] = teleportTileX * 16 + 8;
                        npc.localAI[2] = teleportTileY * 16 + 16;
                        break;
                    }
                    else
                    {
                        predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                        if (predictiveTeleportPoint.X < 10)
                            predictiveTeleportPoint.X = 10;
                        if (predictiveTeleportPoint.X > Main.maxTilesX - 10)
                            predictiveTeleportPoint.X = Main.maxTilesX - 10;
                    }
                }
                else
                {
                    predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                    if (predictiveTeleportPoint.X < 10)
                        predictiveTeleportPoint.X = 10;
                    if (predictiveTeleportPoint.X > Main.maxTilesX - 10)
                        predictiveTeleportPoint.X = Main.maxTilesX - 10;
                }
            }

            if (teleportTries >= 100)
            {
                Vector2 bottom = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].Bottom;
                npc.localAI[1] = bottom.X;
                npc.localAI[2] = bottom.Y;
            }
        }
    }
}
