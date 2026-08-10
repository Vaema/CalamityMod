using System;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;

public class KingSlimeAI : VanillaAIOverride
{
    public static readonly SoundStyle SpawnCrystalSound = new SoundStyle("CalamityMod/Sounds/Custom/KingSlimeJewelSpawn");
    public static readonly SoundStyle ShootSound = new SoundStyle("CalamityMod/Sounds/Custom/RedJewelFire");

    public override bool AI(Mod mod)
    {
        float lifeRatio = NPC.life / (float)NPC.lifeMax;
        float lifeRatio2 = lifeRatio;

        float teleportScale = 1f;
        bool teleporting = false;
        bool teleported = false;
        NPC.aiAction = 0;
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
        bool rubySpawnedForCurrentPhase = NPC.Calamity().newAI[0] == 1f;

        int setDamage = NPC.defDamage; // Defined externally
        NPC.defense = NPC.defDefense;

        if (rubySpawnPhaseActive && !redCrystalAlive && !rubySpawnedForCurrentPhase)
        {
            NPC.Calamity().newAI[0] = 1f;
            NPC.SyncExtraAI();

            Vector2 vector = NPC.Center + new Vector2(-40f, -(float)NPC.height / 2) * NPC.scale;
            int totalDustPerCrystalSpawn = 20;
            for (int i = 0; i < totalDustPerCrystalSpawn; i++)
            {
                int rubyDust = Dust.NewDust(vector, NPC.width / 2, NPC.height / 2, DustID.GemRuby, 0f, 0f, 100, default, 2f);
                Main.dust[rubyDust].velocity *= 2f;
                Main.dust[rubyDust].noGravity = true;
                if (Main.rand.NextBool())
                {
                    Main.dust[rubyDust].scale = 0.5f;
                    Main.dust[rubyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }

            SoundEngine.PlaySound(SpawnCrystalSound with { Volume = 2f }, NPC.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int jewel = NPC.NewNPC(NPC.GetSource_FromAI(), (int)vector.X, (int)vector.Y, ModContent.NPCType<KingSlimeJewelRuby>());
                Main.npc[jewel].localAI[2] = NPC.whoAmI;
                Main.npc[jewel].velocity.Y = -6;
            }
        }
        else if (rubySpawnedForCurrentPhase && !rubySpawnPhaseActive)
        {
            NPC.Calamity().newAI[0] = 0f;
            NPC.SyncExtraAI();
        }

        // Set up health value for spawning slimes
        if (NPC.ai[3] == 0f && NPC.life > 0)
            NPC.ai[3] = NPC.lifeMax;

        // Spawn with attack delay
        if (NPC.localAI[3] == 0f)
        {
            NPC.localAI[3] = 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[0] = -100f;

                CalamityUtils.CalamityTargeting(NPC, default);

                NPC.netUpdate = true;
            }
        }

        // Despawn
        int despawnDistance = 60;
        int forceDespawnDistance = 300;
        if ((Main.player[NPC.target].dead && Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) / 16f > despawnDistance) || Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) / 16f > forceDespawnDistance)
        {
            CalamityUtils.CalamityTargeting(NPC, default);

            if ((Main.player[NPC.target].dead && Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) / 16f > despawnDistance) || Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) / 16f > forceDespawnDistance)
            {
                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;

                if (Main.player[NPC.target].Center.X < NPC.Center.X)
                    NPC.direction = 1;
                else
                    NPC.direction = -1;
            }
        }

        // Faster fall
        if (NPC.velocity.Y > 0f)
        {
            float fallSpeedBonus = (death ? 0.1f : 0f) + (!redCrystalAlive ? 0.1f : 0f);
            NPC.velocity.Y += fallSpeedBonus;
        }

        // Activate teleport
        float teleportGateValue = 480f;
        if (!Main.player[NPC.target].dead && NPC.ai[2] >= teleportGateValue && NPC.ai[1] < 5f && NPC.velocity.Y == 0f)
        {
            NPC.damage = 0;

            NPC.ai[2] = 0f;
            NPC.ai[0] = 0f;
            NPC.ai[1] = 5f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
                GetPlaceToTeleportTo(NPC);
        }

        if (!Collision.CanHitLine(NPC.Center, 0, 0, Main.player[NPC.target].Center, 0, 0) || Math.Abs(NPC.Top.Y - Main.player[NPC.target].Bottom.Y) > 160f)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.localAI[0] += 1f;
        }
        else if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.localAI[0] -= 1f;

            if (NPC.localAI[0] < 0f)
                NPC.localAI[0] = 0f;
        }

        if (NPC.timeLeft < 10 && (NPC.ai[0] != 0f || NPC.ai[1] != 0f))
        {
            NPC.ai[0] = 0f;
            NPC.ai[1] = 0f;
            NPC.netUpdate = true;
            teleporting = false;
        }

        // Closer to activating teleport
        if (NPC.ai[2] < teleportGateValue)
        {
            if (!Collision.CanHitLine(NPC.Center, 0, 0, Main.player[NPC.target].Center, 0, 0) || Math.Abs(NPC.Top.Y - Main.player[NPC.target].Bottom.Y) > (death ? 160f : 320f))
                NPC.ai[2] += death ? 3f : 2f;
            else
                NPC.ai[2] += 1f;
        }

        // Slow down while teleporting
        if (NPC.ai[1] == 5f || NPC.ai[1] == 6f)
        {
            if (Math.Abs(NPC.velocity.X) > 0.1f)
            {
                NPC.velocity.X *= 0.8f;
                if (Math.Abs(NPC.velocity.X) <= 0.1f)
                    NPC.velocity.X = 0f;
            }
        }

        // Teleport
        if (NPC.ai[1] == 5f)
        {
            NPC.damage = 0;

            teleporting = true;
            NPC.aiAction = 1;

            float teleportRate = redCrystalAlive ? 1f : 2f;
            if (death)
                teleportRate *= 2f;

            NPC.ai[0] += teleportRate;
            teleportScale = MathHelper.Clamp((60f - NPC.ai[0]) / 60f, 0f, 1f);
            teleportScale = 0.5f + teleportScale * 0.5f;
            if (Main.getGoodWorld)
                teleportScale *= teleportScaleSpeed;

            if (NPC.ai[0] >= 60f)
                teleported = true;

            if (NPC.ai[0] == 60f && !Main.dedServ)
                Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-40f, -(float)NPC.height / 2), NPC.velocity, 734, 1f);

            if (NPC.ai[0] >= 60f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.Bottom = new Vector2(NPC.localAI[1], NPC.localAI[2]);
                NPC.ai[1] = 6f;
                NPC.ai[0] = 0f;
                NPC.netUpdate = true;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient && NPC.ai[0] >= 120f)
            {
                NPC.ai[1] = 6f;
                NPC.ai[0] = 0f;
            }

            if (!teleported)
            {
                for (int i = 0; i < 10; i++)
                {
                    int slimeDust = Dust.NewDust(NPC.position + Vector2.UnitX * -20f, NPC.width + 40, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
                    Main.dust[slimeDust].noGravity = true;
                    Main.dust[slimeDust].velocity *= 0.5f;
                }
            }
        }

        // Post-teleport
        else if (NPC.ai[1] == 6f)
        {
            NPC.damage = 0;

            teleporting = true;
            NPC.aiAction = 0;

            float teleportRate = redCrystalAlive ? 1f : 2f;
            if (death)
                teleportRate *= 2f;


            NPC.ai[0] += teleportRate;
            teleportScale = MathHelper.Clamp(NPC.ai[0] / 30f, 0f, 1f);
            teleportScale = 0.5f + teleportScale * 0.5f;
            if (Main.getGoodWorld)
                teleportScale *= teleportScaleSpeed;

            if (NPC.ai[0] >= 30f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[1] = 0f;
                NPC.ai[0] = -15f;
                NPC.netUpdate = true;

                CalamityUtils.CalamityTargeting(NPC, default);
            }

            if (Main.netMode == NetmodeID.MultiplayerClient && NPC.ai[0] >= 60f)
            {
                NPC.ai[1] = 0f;
                NPC.ai[0] = -15f;

                CalamityUtils.CalamityTargeting(NPC, default);
            }

            for (int j = 0; j < 10; j++)
            {
                int slimyDust = Dust.NewDust(NPC.position + Vector2.UnitX * -20f, NPC.width + 40, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
                Main.dust[slimyDust].noGravity = true;
                Main.dust[slimyDust].velocity *= 2f;
            }
        }

        NPC.noTileCollide = false;

        // Jump
        if (NPC.velocity.Y == 0f)
        {
            NPC.damage = 0;

            NPC.velocity.X *= 0.8f;
            if (NPC.velocity.X > -0.1f && NPC.velocity.X < 0.1f)
                NPC.velocity.X = 0f;

            if (!teleporting)
            {
                NPC.ai[0] += MathHelper.Lerp(1f, 8f, 1f - lifeRatio);
                if (NPC.ai[0] >= 0f)
                {
                    NPC.damage = setDamage;

                    NPC.netUpdate = true;

                    CalamityUtils.CalamityTargeting(NPC, default);

                    float distanceBelowTarget = NPC.position.Y - (Main.player[NPC.target].position.Y + 80f);
                    float speedMult = 1f;
                    if (distanceBelowTarget > 0f)
                        speedMult += distanceBelowTarget * 0.002f;

                    if (speedMult > 2f)
                        speedMult = 2f;

                    bool deathModeRapidHops = death && lifeRatio < 0.2f;
                    if (deathModeRapidHops)
                        NPC.ai[1] = 2f;

                    if (NPC.ai[1] == 3f)
                    {
                        NPC.velocity.Y = -10f * speedMult;
                        NPC.velocity.X += (phase2 ? (death ? 5.35f : 4.5f) : 3.5f) * NPC.direction;
                        NPC.ai[0] = -100f;
                        NPC.ai[1] = 0f;
                    }
                    else if (NPC.ai[1] == 2f)
                    {
                        NPC.velocity.Y = -6f * speedMult;
                        NPC.velocity.X += (phase2 ? (deathModeRapidHops ? 7.65f : death ? 6.25f : 5.5f) : 4.5f) * NPC.direction;
                        NPC.ai[0] = -60f;

                        if (!deathModeRapidHops)
                            NPC.ai[1] += 1f;
                    }
                    else
                    {
                        NPC.velocity.Y = -8f * speedMult;
                        NPC.velocity.X += (phase2 ? (death ? 5.75f : 5f) : 4f) * NPC.direction;
                        NPC.ai[0] = -60f;
                        NPC.ai[1] += 1f;
                    }

                    if (death)
                        NPC.velocity.X *= 1.2f;

                    NPC.noTileCollide = true;
                }
                else if (NPC.ai[0] >= -30f)
                    NPC.aiAction = 1;
            }
        }

        else if (NPC.target < Main.maxPlayers) // Velocity factoring
        {
            float jumpVelocityLimit = redCrystalAlive ? 3f : 4.5f;
            if (death)
                jumpVelocityLimit += 2.25f;
            if (Main.getGoodWorld)
                jumpVelocityLimit = 8f;

            if ((NPC.direction == 1 && NPC.velocity.X < jumpVelocityLimit) || (NPC.direction == -1 && NPC.velocity.X > -jumpVelocityLimit))
            {
                if ((NPC.direction == -1 && NPC.velocity.X < 0.1) || (NPC.direction == 1 && NPC.velocity.X > -0.1))
                {
                    NPC.velocity.X += (death ? 0.25f : 0.2f) * NPC.direction;
                    if (death)
                        NPC.velocity.X += 0.25f * NPC.direction;
                }
                else
                {
                    NPC.velocity.X *= death ? 0.92f : 0.93f;
                    if (death)
                        NPC.velocity.X *= 0.9f;
                }
            }

            if (!Main.player[NPC.target].dead)
            {
                if (NPC.velocity.Y > 0f && NPC.Bottom.Y > Main.player[NPC.target].Top.Y)
                    NPC.noTileCollide = false;
                else if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].Center, 1, 1) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.noTileCollide = false;
                else
                    NPC.noTileCollide = true;
            }
        }

        int idleSlimeDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 255, new Color(0, 80, 255, 80), NPC.scale * 1.2f);
        Main.dust[idleSlimeDust].noGravity = true;
        Main.dust[idleSlimeDust].velocity *= 0.5f;

        if (NPC.life <= 0)
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
        if (lifeRatio != NPC.scale)
        {
            NPC.position.X += NPC.width / 2;
            NPC.position.Y += NPC.height;
            NPC.scale = lifeRatio;
            NPC.width = (int)(98f * NPC.scale);
            NPC.height = (int)(92f * NPC.scale);
            NPC.position.X -= NPC.width / 2;
            NPC.position.Y -= NPC.height;
        }

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int slimeSpawnThreshold = (int)(NPC.lifeMax * 0.03);
            if (NPC.life + slimeSpawnThreshold < NPC.ai[3])
            {
                NPC.ai[3] = NPC.life;
                int slimeAmt = death ? 1 : Main.rand.Next(1, 3);
                for (int i = 0; i < slimeAmt; i++)
                {
                    int minTypeChoice = (int)MathHelper.Lerp(0f, 5f, 1f - lifeRatio2);
                    int maxTypeChoice = (int)MathHelper.Lerp(2f, 7f, 1f - lifeRatio2);

                    int npcType;
                    switch (Main.rand.Next(minTypeChoice, maxTypeChoice + 1))
                    {
                        case 0:
                            npcType = NPCID.GreenSlime;
                            break;
                        case 1:
                            npcType = NPCID.BlueSlime;
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
                        default:
                            npcType = NPCID.SlimeSpiked;
                            break;
                    }

                    if ((Main.raining && Main.hardMode) && Main.rand.NextBool(50))
                        npcType = NPCID.RainbowSlime;

                    if (death && Main.rand.NextBool()) // 50% chance to spawn a spiked slime instead of the above npcType value
                        npcType = NPCID.SlimeSpiked;

                    if (Main.rand.NextBool(100))
                        npcType = NPCID.Pinky;

                    if (Main.zenithWorld)
                        npcType = NPCID.RainbowSlime;

                    int offset = 16;
                    int spawnZoneWidth = NPC.width - offset * 2;
                    int spawnZoneHeight = NPC.height - offset * 2;
                    int x = (int)(NPC.position.X + offset + Main.rand.Next(spawnZoneWidth));
                    int y = (int)(NPC.position.Y + offset + Main.rand.Next(spawnZoneHeight));
                    int slimeSpawns = NPC.NewNPC(NPC.GetSource_FromAI(), x, y, npcType);
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
