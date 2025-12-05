using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SlimeGod
{
    [AutoloadBossHead]
    public class CrimulanPaladin : ModNPC
    {
        private float bossLife;
        private float addedStretch = 0f;
        private float landingRecoil = 0f;
        private const int Width = 206;
        private const int Height = 156;

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
        }

        public static int BigSpikeDamage = 15; // 60

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 42; // 84
            NPC.width = Width;
            NPC.height = Height;
            NPC.defense = 12;
            NPC.LifeMaxNERB(7500, 9000, 160000);
            NPC.BossBar = Main.BigBossProgressBar.NeverValid;
            NPC.knockBackResist = 0f;
            NPC.Opacity = 1f;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            for (int i = 0; i < 4; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void AI()
        {
            Vector3 light = new Vector3(1f, 0.2f, 0.2f) * NPC.scale;
            Lighting.AddLight(NPC.Center, light.X, light.Y, light.Z);

            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            CalamityGlobalNPC.slimeGodRed = NPC.whoAmI;
            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || NPC.localAI[1] == 1f || BossRushEvent.BossRushActive;

            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            // Reset defense and damage
            NPC.defense = NPC.defDefense;
            int setDamage = NPC.defDamage;

            // Boost defense and damage while buffed by the Slime God
            if (NPC.localAI[1] == 1f)
            {
                NPC.defense = NPC.defDefense + 24;
                setDamage += SlimeGodCore.PossessionDamageBoost;
            }

            // Used for landing squash and stretch
            if (landingRecoil > 0f)
            {
                landingRecoil *= 0.965f;
                landingRecoil -= 0.01f;
            }
            else
                landingRecoil = 0f;

            addedStretch = -landingRecoil;

            // Used for teleporting
            float scale = Main.getGoodWorld ? 0.8f : 1f;

            // How fast the slime slams down
            float slamVelocity = death ? 16f : revenge ? 15f : expertMode ? 14f : 12f;

            // Used for how fast the slime animates
            NPC.aiAction = 0;

            // Reset tile collide and gravity
            NPC.noTileCollide = false;
            NPC.noGravity = false;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            // Despawn
            if (NPC.ai[0] != 3f)
            {
                if (player.dead || !player.active)
                {
                    NPC.TargetClosest();
                    player = Main.player[NPC.target];
                    if (player.dead || !player.active)
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        calamityGlobalNPC.newAI[0] = 0f;
                        NPC.netUpdate = true;
                    }
                }
                else if (NPC.timeLeft < 1800)
                    NPC.timeLeft = 1800;
            }

            // Split into smaller slimes and fire spikes at half HP
            if (lifeRatio <= 0.5f && Main.netMode != NetmodeID.MultiplayerClient && expertMode)
            {
                // Spread of random globs in meme mode
                if (Main.zenithWorld)
                {
                    int type = ModContent.ProjectileType<UnstableCrimulanGlob>();
                    for (int i = 0; i < 30; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, (float)Main.rand.Next(-1499, 1500) * 0.01f, (float)Main.rand.Next(-1499, 1500) * 0.01f, type, 35, 0f);
                }

                // Spawn split slimes
                SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
                Vector2 spawnAt = NPC.Center + new Vector2(0f, NPC.height / 2f);
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X - 80, (int)spawnAt.Y, ModContent.NPCType<SplitCrimulanPaladin>(), 0, 0f, 0f, 0f);
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X + 80, (int)spawnAt.Y, ModContent.NPCType<SplitCrimulanPaladin>(), 0, 0f, 0f, 45f);

                // Split into 3 additional slimes if the other large slime hasn't split yet
                if (Main.zenithWorld && NPC.CountNPCS(ModContent.NPCType<SplitEbonianPaladin>()) < 3)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X, (int)spawnAt.Y - 80, ModContent.NPCType<SplitCrimulanPaladin>(), 0, 0f, 0f, 90f);

                // Calculate stretch here so that the spike spread looks more natural
                float stretch = NPC.velocity.Y * 0.03f;
                stretch = Math.Abs(stretch) + addedStretch;

                // Stretch rapidly if about to jump or teleport, stretch normally while idle
                if (NPC.velocity.Y == 0f)
                    stretch += MathHelper.Lerp(0f, 0.16f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * (NPC.aiAction == 1 ? 15f : 2f)) / 2f + 0.5f);

                if (stretch > 0.5f)
                    stretch = 0.5f;

                Vector2 scaleStretch = new Vector2(1f - stretch, 1f + stretch) * NPC.scale;

                // Spread of spikes
                float projectileVelocity = 2f;
                int type2 = ModContent.ProjectileType<CrimulanSpike>();
                Vector2 spawnLocation = new Vector2(NPC.Center.X, NPC.Center.Y + 50f * NPC.scale);
                Vector2 destination = (new Vector2(NPC.Center.X, NPC.Center.Y - 100f) - NPC.Center).SafeNormalize(Vector2.UnitY);
                destination *= projectileVelocity;
                int numProj = 9;
                float rotation = MathHelper.ToRadians(100);
                float maxVelocity = death ? 18f : revenge ? 16f : 14f;
                float acceleration = death ? 1.03f : revenge ? 1.025f : 1.02f;
                int spikeType = 0;
                int dustType = DustID.TintableDust;
                Color dustColor = Color.Crimson;
                dustColor.A = 150;
                Vector2 dustSpawnBox = new Vector2(40f, 40f);
                Vector2 dustSpawnOffset = dustSpawnBox * 0.5f;
                for (int i = 0; i < numProj; i++)
                {
                    spikeType = numProj - 1 - i;

                    Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                    Vector2 projectileLocation = spawnLocation + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 70f * scaleStretch;

                    float dustSpeed = Main.rand.NextFloat(2f, 4f);
                    float angleRandom = 0.1f;
                    Vector2 dustVelocity = new Vector2(dustSpeed, 0f).RotatedBy(perturbedSpeed.ToRotation());
                    dustVelocity = dustVelocity.RotatedBy(-angleRandom);
                    dustVelocity = dustVelocity.RotatedByRandom(2f * angleRandom);

                    for (int j = 0; j < 32; j++)
                    {
                        int slimeDust = Dust.NewDust(projectileLocation - dustSpawnOffset, (int)dustSpawnBox.X, (int)dustSpawnBox.Y, dustType);
                        Main.dust[slimeDust].velocity = dustVelocity;
                        Main.dust[slimeDust].color = dustColor;
                        Main.dust[slimeDust].noGravity = true;
                    }

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), projectileLocation, perturbedSpeed, type2, BigSpikeDamage, 0f, Main.myPlayer, maxVelocity, acceleration, spikeType);
                }

                // Die
                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            bool enraged = true;
            if (CalamityGlobalNPC.slimeGodPurple != -1)
            {
                if (Main.npc[CalamityGlobalNPC.slimeGodPurple].active)
                    enraged = false;
            }

            if (NPC.localAI[1] != 1f)
            {
                if (enraged)
                    NPC.defense = NPC.defDefense * 2;
            }

            // Slow down dramatically while teleporting
            if (NPC.ai[0] == 4f || NPC.ai[0] == 5f)
            {
                if (Math.Abs(NPC.velocity.X) > 0.1f)
                {
                    NPC.velocity.X *= 0.8f;
                    if (Math.Abs(NPC.velocity.X) <= 0.1f)
                        NPC.velocity.X = 0f;
                }
            }

            // Teleport
            float teleportGateValue = 720f;
            if (!player.dead && NPC.timeLeft > 10 && calamityGlobalNPC.newAI[0] >= teleportGateValue && NPC.ai[0] == 0f && NPC.velocity.Y == 0f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.ai[0] = 4f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.netUpdate = true;
                    NPC.TargetClosest(false);
                    player = Main.player[NPC.target];

                    float distanceAhead = 800f;
                    Vector2 randomDefault = Main.rand.NextBool() ? Vector2.UnitX : -Vector2.UnitX;
                    Vector2 vectorAimedAheadOfTarget = player.Center + new Vector2((float)Math.Round(player.velocity.X), 0f).SafeNormalize(randomDefault) * distanceAhead;
                    Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
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
                            if (canTeleportToTile && !Collision.CanHitLine(NPC.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
                                canTeleportToTile = false;

                            if (canTeleportToTile)
                            {
                                NPC.localAI[0] = teleportTileX * 16 + 8;
                                NPC.localAI[3] = teleportTileY * 16 + 16;
                                calamityGlobalNPC.newAI[0] = 0f;
                                break;
                            }
                            else
                                predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                        }
                        else
                            predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                    }

                    // Default teleport if the above conditions aren't met in 100 iterations
                    if (teleportTries >= 100)
                    {
                        Vector2 bottom = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)].Bottom;
                        NPC.localAI[0] = bottom.X;
                        NPC.localAI[3] = bottom.Y;
                        calamityGlobalNPC.newAI[0] = 0f;
                    }
                }
            }

            // Get ready to teleport
            if (calamityGlobalNPC.newAI[0] < teleportGateValue)
            {
                // Teleport very soon if too far away
                float catchUpDistance = 1500f;
                bool fastTeleport = NPC.Distance(player.Center) > catchUpDistance;
                if (fastTeleport)
                {
                    calamityGlobalNPC.newAI[0] += 10f;
                }
                else
                {
                    float teleportFasterDistance = 500f;
                    if (!Collision.CanHitLine(NPC.Center, 0, 0, player.Center, 0, 0) || Math.Abs(NPC.Top.Y - player.Bottom.Y) > teleportFasterDistance)
                        calamityGlobalNPC.newAI[0] += death ? 3f : 2f;
                    else
                        calamityGlobalNPC.newAI[0] += 1f;
                }
            }

            float distanceSpeedBoost = NPC.Distance(player.Center) * 0.005f;

            if (NPC.ai[0] == 0f)
            {
                if (NPC.velocity.Y == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[2] += 1f;
                    if (revenge)
                        NPC.ai[2] += death ? 4f * (1f - lifeRatio) : 2f * (1f - lifeRatio);

                    NPC.TargetClosest();
                    NPC.velocity.X *= 0.8f;
                }

                float landingSquashTime = 10f;
                float telegraphTime = 20f;
                float phaseSwitchGateValue = 240f;
                float jumpTelegraphGateValue = phaseSwitchGateValue - telegraphTime;
                if (NPC.ai[2] < jumpTelegraphGateValue)
                {
                    if (NPC.velocity.Y == 0f)
                    {
                        // Squash when landing
                        if (NPC.ai[1] < 0f)
                            landingRecoil += Math.Abs(NPC.ai[1]) / landingSquashTime;

                        NPC.ai[1] += 1f;

                        float jumpGateValue = 50f;
                        float velocityX = death ? 14f : revenge ? 13f : expertMode ? 12f : 10f;
                        if (revenge)
                        {
                            float moveBoost = death ? 22f * (1f - lifeRatio) : 14f * (1f - lifeRatio);
                            float speedBoost = death ? 4.5f * (1f - lifeRatio) : 3f * (1f - lifeRatio);
                            jumpGateValue -= moveBoost;
                            velocityX += speedBoost;
                        }

                        float distanceBelowTarget = NPC.Top.Y - (player.Top.Y + 80f);
                        float speedMult = 1f;
                        if (distanceBelowTarget > 0f)
                            speedMult += distanceBelowTarget * 0.002f;

                        if (speedMult > 2f)
                            speedMult = 2f;

                        float velocityY = 6f;
                        float jumpTelegraphGateValue2 = jumpGateValue - telegraphTime;
                        if (NPC.ai[1] >= jumpGateValue)
                        {
                            // Set damage
                            NPC.damage = setDamage;

                            NPC.ai[3] += 1f;
                            if (NPC.ai[3] >= 2f)
                            {
                                NPC.ai[3] = 0f;
                                velocityX *= 1.25f;
                            }

                            NPC.ai[1] = -landingSquashTime;
                            NPC.velocity.Y -= velocityY * speedMult;
                            NPC.velocity.X = (velocityX + distanceSpeedBoost) * NPC.direction;
                            NPC.noTileCollide = true;
                            NPC.netUpdate = true;
                        }
                        else if (NPC.ai[1] >= jumpTelegraphGateValue2)
                            landingRecoil += (NPC.ai[1] - jumpTelegraphGateValue2) / telegraphTime;
                    }
                    else
                    {
                        // Set damage
                        NPC.damage = setDamage;

                        NPC.velocity.X *= 0.99f;
                        if (NPC.direction < 0 && NPC.velocity.X > -1f)
                            NPC.velocity.X = -1f;
                        if (NPC.direction > 0 && NPC.velocity.X < 1f)
                            NPC.velocity.X = 1f;

                        if (!player.dead)
                        {
                            if (NPC.velocity.Y > 0f && NPC.Bottom.Y > player.Top.Y)
                                NPC.noTileCollide = false;
                            else if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.Center, 1, 1) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                                NPC.noTileCollide = false;
                            else
                                NPC.noTileCollide = true;
                        }
                    }
                }

                if (NPC.velocity.Y == 0f)
                {
                    bool switchPhase = NPC.ai[2] >= phaseSwitchGateValue;
                    if (switchPhase)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            switch ((int)NPC.localAI[2])
                            {
                                // Jump combo
                                default:
                                case 1:
                                    NPC.ai[0] = 2f;
                                    break;

                                // Slam
                                case 2:
                                    NPC.ai[0] = 1f;
                                    break;
                            }

                            if (NPC.ai[0] == 1f)
                            {
                                // Set damage
                                NPC.damage = setDamage;

                                NPC.noTileCollide = true;
                                NPC.velocity.Y = death ? -12f : revenge ? -11f : expertMode ? -10f : -8f;
                            }

                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[2] >= jumpTelegraphGateValue)
                        landingRecoil += (NPC.ai[2] - jumpTelegraphGateValue) / telegraphTime;
                }
            }

            // Jump above for slam attack
            else if (NPC.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.noTileCollide = true;
                NPC.noGravity = true;

                NPC.direction = NPC.velocity.X < 0f ? -1 : 1;
                NPC.spriteDirection = NPC.direction;

                NPC.TargetClosest();

                float distanceAboveTarget = 350f;
                Vector2 velocity = player.Center - Vector2.UnitY * distanceAboveTarget - NPC.Center;

                if (NPC.ai[2] == 1f)
                {
                    NPC.ai[1] += 1f;

                    float moveSpeed = slamVelocity - 6f;
                    float inertia = 5f;
                    velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * moveSpeed;
                    NPC.velocity = (NPC.velocity * (inertia - 1f) + velocity) / inertia;

                    float delayBeforeSlamming = 12f;
                    if (NPC.ai[1] > delayBeforeSlamming)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[0] = 1.1f;
                        NPC.ai[2] = 0f;
                        NPC.velocity = velocity;
                    }
                }
                else
                {
                    if (Math.Abs(NPC.Center.X - player.Center.X) < 40f && NPC.Center.Y < player.Center.Y - (distanceAboveTarget - 50f))
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 1f;
                        return;
                    }

                    float moveSpeed = (death ? 16f : revenge ? 15f : expertMode ? 14f : 12f) + distanceSpeedBoost;
                    float inertia = 6f;
                    velocity = velocity.SafeNormalize(Vector2.UnitY) * moveSpeed;
                    NPC.velocity = (NPC.velocity * (inertia - 1f) + velocity) / inertia;
                }
            }

            // Slam down
            else if (NPC.ai[0] == 1.1f)
            {
                // Set damage
                NPC.damage = setDamage;

                bool atTargetPosition = NPC.Bottom.Y >= player.Top.Y;
                if (NPC.ai[2] == 0f && (atTargetPosition || NPC.localAI[1] == 0f) && Collision.CanHit(NPC.Center, 1, 1, player.Center, 1, 1) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.ai[2] = 1f;
                    NPC.netUpdate = true;
                }

                if (atTargetPosition || NPC.velocity.Y <= 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] > 10f)
                    {
                        SoundEngine.PlaySound(SlimeGodCore.BigShotSound, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            // Calculate stretch here so that the spike spread looks more natural
                            float stretch = NPC.velocity.Y * 0.03f;
                            stretch = Math.Abs(stretch) + addedStretch;

                            // Stretch rapidly if about to jump or teleport, stretch normally while idle
                            if (NPC.velocity.Y == 0f)
                                stretch += MathHelper.Lerp(0f, 0.16f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * (NPC.aiAction == 1 ? 15f : 2f)) / 2f + 0.5f);

                            if (stretch > 0.5f)
                                stretch = 0.5f;

                            Vector2 scaleStretch = new Vector2(1f - stretch, 1f + stretch) * NPC.scale;

                            // Spread of spikes
                            float projectileVelocity = 2f;
                            int type2 = ModContent.ProjectileType<CrimulanSpike>();
                            Vector2 spawnLocation = new Vector2(NPC.Center.X, NPC.Center.Y + 50f * NPC.scale);
                            Vector2 destination = (new Vector2(NPC.Center.X, NPC.Center.Y - 100f) - NPC.Center).SafeNormalize(Vector2.UnitY);
                            destination *= projectileVelocity;
                            int numProj = 9;
                            float rotation = MathHelper.ToRadians(100);
                            float maxVelocity = death ? 18f : revenge ? 16f : 14f;
                            float acceleration = death ? 1.03f : revenge ? 1.025f : 1.02f;
                            int spikeType = 0;
                            int dustType = DustID.TintableDust;
                            Color dustColor = Color.Crimson;
                            dustColor.A = 150;
                            Vector2 dustSpawnBox = new Vector2(40f, 40f);
                            Vector2 dustSpawnOffset = dustSpawnBox * 0.5f;
                            for (int i = 0; i < numProj; i++)
                            {
                                spikeType = numProj - 1 - i;

                                Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                Vector2 projectileLocation = spawnLocation + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 70f * scaleStretch;

                                float dustSpeed = Main.rand.NextFloat(2f, 4f);
                                float angleRandom = 0.1f;
                                Vector2 dustVelocity = new Vector2(dustSpeed, 0f).RotatedBy(perturbedSpeed.ToRotation());
                                dustVelocity = dustVelocity.RotatedBy(-angleRandom);
                                dustVelocity = dustVelocity.RotatedByRandom(2f * angleRandom);

                                for (int j = 0; j < 32; j++)
                                {
                                    int slimeDust = Dust.NewDust(projectileLocation - dustSpawnOffset, (int)dustSpawnBox.X, (int)dustSpawnBox.Y, dustType);
                                    Main.dust[slimeDust].velocity = dustVelocity;
                                    Main.dust[slimeDust].color = dustColor;
                                    Main.dust[slimeDust].noGravity = true;
                                }

                                Projectile.NewProjectile(NPC.GetSource_FromAI(), projectileLocation, perturbedSpeed, type2, BigSpikeDamage, 0f, Main.myPlayer, maxVelocity, acceleration, spikeType);
                            }
                        }

                        NPC.localAI[2] = NPC.ai[0] - 0.1f;
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                    else
                        landingRecoil += NPC.ai[1] / 10f;
                }
                else if (NPC.ai[2] == 0f)
                    NPC.noTileCollide = true;

                NPC.noGravity = true;

                NPC.velocity.Y += 0.5f;
                if (NPC.velocity.Y > slamVelocity)
                    NPC.velocity.Y = slamVelocity;
            }

            // Jump in quick succession
            else if (NPC.ai[0] == 2f)
            {
                if (NPC.velocity.Y == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.TargetClosest();
                    NPC.velocity.X *= 0.8f;

                    NPC.ai[1] += 1f;
                    float jumpGateValue = 15f;
                    if (NPC.ai[1] > jumpGateValue)
                    {
                        // Set damage
                        NPC.damage = setDamage;

                        NPC.ai[1] = 0f;

                        float jumpSpeedX = ((death ? 14f : revenge ? 13f : expertMode ? 12f : 10f) + distanceSpeedBoost) * NPC.direction;
                        float jumpSpeedY = -4f - (NPC.Top.Y > player.Bottom.Y ? ((NPC.Top.Y - player.Bottom.Y) * 0.05f) : 0f);
                        if (jumpSpeedY < -16f)
                            jumpSpeedY = -16f;

                        NPC.velocity = new Vector2(jumpSpeedX, jumpSpeedY);

                        NPC.ai[2] += 1f;
                    }
                    else
                        landingRecoil += NPC.ai[1] / 10f;
                }
                else
                {
                    // Set damage
                    NPC.damage = setDamage;

                    NPC.velocity.X *= 0.98f;
                    float velocityLimit = (death ? 7f : revenge ? 6.5f : expertMode ? 6f : 5f);
                    if (NPC.direction < 0 && NPC.velocity.X > -velocityLimit)
                        NPC.velocity.X = -velocityLimit;
                    if (NPC.direction > 0 && NPC.velocity.X < velocityLimit)
                        NPC.velocity.X = velocityLimit;
                }

                if (NPC.ai[2] >= 3f && NPC.velocity.Y == 0f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[2] += 1f;
                    if (NPC.ai[2] >= 23f)
                    {
                        NPC.localAI[2] = NPC.ai[0];
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                    else
                        landingRecoil += (NPC.ai[2] - 3f) / 10f;
                }
            }

            // Despawn
            else if (NPC.ai[0] == 3f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.noTileCollide = true;
                NPC.Opacity -= 0.03f;

                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;

                if (NPC.Opacity < 0f)
                    NPC.Opacity = 0f;

                NPC.velocity.X *= 0.98f;
            }

            // Teleport shit
            else if (NPC.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.aiAction = 1;
                NPC.ai[1] += 1f;
                float teleportTime = death ? 30f : 40f;
                scale = MathHelper.Clamp((teleportTime - NPC.ai[1]) / teleportTime, 0f, 1f);
                scale = 0.5f + scale * 0.5f;
                if (NPC.ai[1] >= teleportTime && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.Bottom = new Vector2(NPC.localAI[0], NPC.localAI[3]);
                    NPC.ai[0] = 5f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }

                if (Main.netMode == NetmodeID.MultiplayerClient && NPC.ai[1] >= teleportTime * 2f)
                {
                    NPC.ai[0] = 5f;
                    NPC.ai[1] = 0f;
                }

                // Emit teleport dust
                Color dustColor = Color.Crimson;
                dustColor.A = 150;
                for (int i = 0; i < 10; i++)
                {
                    int crimsonDust = Dust.NewDust(NPC.position + Vector2.UnitX * -20f, NPC.width + 40, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 0, dustColor, 2f);
                    Main.dust[crimsonDust].noGravity = true;
                    Main.dust[crimsonDust].velocity *= 0.5f;
                }
            }
            else if (NPC.ai[0] == 5f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.ai[1] += 1f;
                float teleportEndTime = death ? 15f : 20f;
                scale = MathHelper.Clamp(NPC.ai[1] / teleportEndTime, 0f, 1f);
                scale = 0.5f + scale * 0.5f;
                if (NPC.ai[1] >= teleportEndTime && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = -10f;
                    NPC.netUpdate = true;
                    NPC.TargetClosest();
                }

                if (Main.netMode == NetmodeID.MultiplayerClient && NPC.ai[1] >= teleportEndTime * 2f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = -10f;
                    NPC.TargetClosest();
                }

                // Emit teleport dust
                Color dustColor = Color.Crimson;
                dustColor.A = 150;
                for (int i = 0; i < 10; i++)
                {
                    int crimsonDust = Dust.NewDust(NPC.position + Vector2.UnitX * -20f, NPC.width + 40, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 0, dustColor, 2f);
                    Main.dust[crimsonDust].noGravity = true;
                    Main.dust[crimsonDust].velocity *= 0.5f;
                }
            }

            // Limit the landing recoil
            if (landingRecoil > 0.36f)
                landingRecoil = 0.36f;

            // Slime spawning and scale logic
            if (bossLife == 0f && NPC.life > 0)
                bossLife = NPC.lifeMax;

            if (NPC.life > 0)
            {
                float scaleRatio = lifeRatio;
                scaleRatio = scaleRatio * 0.5f + 0.75f;
                scaleRatio *= scale;

                if (scaleRatio != NPC.scale)
                {
                    NPC.position.X = NPC.Center.X;
                    NPC.position.Y = NPC.position.Y + (float)NPC.height;
                    NPC.scale = scaleRatio;
                    NPC.width = (int)(Width * NPC.scale);
                    NPC.height = (int)(Height * NPC.scale);
                    NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
                    NPC.position.Y = NPC.position.Y - (float)NPC.height;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int slimeSpawnThreshold = (int)((double)NPC.lifeMax * 0.15);
                    if ((float)(NPC.life + slimeSpawnThreshold) < bossLife)
                    {
                        bossLife = (float)NPC.life;

                        int offset = 16;
                        int x = (int)(NPC.position.X + offset + (float)Main.rand.Next(NPC.width - offset * 2));
                        int y = (int)(NPC.position.Y + offset + (float)Main.rand.Next(NPC.height - offset * 2));
                        int slimeType = Main.rand.NextBool(3) ? ModContent.NPCType<CrimsonSlimeSpawn2>() : ModContent.NPCType<CrimsonSlimeSpawn>();
                        int slimeSpawn = NPC.NewNPC(NPC.GetSource_FromAI(), x, y, slimeType);
                        Main.npc[slimeSpawn].SetDefaults(slimeType);
                        Main.npc[slimeSpawn].velocity.X = (float)Main.rand.Next(-15, 16) * 0.1f;
                        Main.npc[slimeSpawn].velocity.Y = (float)Main.rand.Next(-30, 1) * 0.1f;
                        Main.npc[slimeSpawn].ai[0] = (float)(-1000 * Main.rand.Next(3));
                        Main.npc[slimeSpawn].ai[1] = 0f;
                        if (Main.dedServ && slimeSpawn < Main.maxNPCs)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slimeSpawn);
                    }
                }
            }
        }

        // Can only hit the target if within certain distance
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetHitbox = target.Hitbox;

            float hitboxTopLeft = Vector2.Distance(NPC.Center, targetHitbox.TopLeft());
            float hitboxTopRight = Vector2.Distance(NPC.Center, targetHitbox.TopRight());
            float hitboxBotLeft = Vector2.Distance(NPC.Center, targetHitbox.BottomLeft());
            float hitboxBotRight = Vector2.Distance(NPC.Center, targetHitbox.BottomRight());

            float minDist = hitboxTopLeft;
            if (hitboxTopRight < minDist)
                minDist = hitboxTopRight;
            if (hitboxBotLeft < minDist)
                minDist = hitboxBotLeft;
            if (hitboxBotRight < minDist)
                minDist = hitboxBotRight;

            return minDist <= 75f * NPC.scale;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            Color lightColor = new Color(Main.DiscoR, 100, 150, NPC.alpha);
            Color newColor = NPC.localAI[1] == 1f ? lightColor : drawColor;
            return newColor * NPC.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Color drawColorAlpha = NPC.GetAlpha(drawColor);

            // Stretch based on Y velocity.
            float stretch = NPC.velocity.Y * 0.03f;
            stretch = Math.Abs(stretch) + addedStretch;

            // Stretch rapidly if about to jump or teleport, stretch normally while idle.
            if (NPC.velocity.Y == 0f)
                stretch += MathHelper.Lerp(0f, 0.16f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * (NPC.aiAction == 1 ? 15f : 2f)) / 2f + 0.5f);

            if (stretch > 0.5f)
                stretch = 0.5f;

            Vector2 scaleStretch = new Vector2(1f - stretch, 1f + stretch) * NPC.scale;
            float yOffset = stretch * 0.5f * NPC.height;

            spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY - yOffset), NPC.frame, drawColorAlpha, NPC.rotation + SlimeRotationFactor(NPC), NPC.frame.Size() * 0.5f, scaleStretch, spriteEffects, 0f);

            return false;
        }

        public static float SlimeRotationFactor(NPC npc) => MathHelper.ToRadians(npc.velocity.X * (-npc.velocity.Y * 0.1f));

        public override void HitEffect(NPC.HitInfo hit)
        {
            Color dustColor = Color.Crimson;
            dustColor.A = 150;

            for (int k = 0; k < 5; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hit.HitDirection, -1f, 0, dustColor, 1f);

            if (NPC.life <= 0)
            {
                NPC.position = NPC.Center;
                NPC.width = NPC.height = (int)(160f * NPC.scale);
                NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (float)(NPC.height / 2);

                for (int i = 0; i < 40; i++)
                {
                    int crimDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, 0, dustColor, 2f);
                    Main.dust[crimDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[crimDust].scale = 0.5f;
                        Main.dust[crimDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 80; j++)
                {
                    int crimDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, 0, dustColor, 3f);
                    Main.dust[crimDust2].noGravity = true;
                    Main.dust[crimDust2].velocity *= 5f;
                    crimDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, 0, dustColor, 2f);
                    Main.dust[crimDust2].velocity *= 2f;
                }
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        }
    }
}
