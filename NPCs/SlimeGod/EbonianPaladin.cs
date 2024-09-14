using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SlimeGod
{
    [AutoloadBossHead]
    public class EbonianPaladin : ModNPC
    {
        private float bossLife;
        private float addedStretch = 0f;
        private int wingFrameDrawn = 0;
        private const int TotalWingFrames = 8;

        public static Asset<Texture2D> WingTexture;

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();

            if (!Main.dedServ)
                WingTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SlimeGod/ExtraTextures/EbonianPaladinWings", AssetRequestMode.AsyncLoad);
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.GetNPCDamage();
            NPC.width = 134;
            NPC.height = 150;
            NPC.scale = 1.1f;
            NPC.defense = 10;
            NPC.LifeMaxNERB(8000, 9600, 220000);
            NPC.BossBar = Main.BigBossProgressBar.NeverValid;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = 0f;
            NPC.Opacity = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;

            // Scale HP in Master
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC, true);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            for (int i = 0; i < 4; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void AI()
        {
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            CalamityGlobalNPC.slimeGodPurple = NPC.whoAmI;
            bool bossRush = BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || bossRush;
            bool revenge = CalamityWorld.revenge || bossRush;
            bool death = CalamityWorld.death || NPC.localAI[1] == 1f || bossRush;

            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            NPC.defense = NPC.defDefense;
            int setDamage = NPC.defDamage;
            if (NPC.localAI[1] == 1f)
            {
                NPC.defense = NPC.defDefense + 20;
                setDamage += 22;
            }

            float scale = (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 0.6f : Main.getGoodWorld ? 0.8f : 1f;
            NPC.aiAction = 0;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (NPC.ai[0] != 4f)
            {
                if (player.dead || !player.active)
                {
                    NPC.TargetClosest();
                    player = Main.player[NPC.target];
                    if (player.dead || !player.active)
                    {
                        NPC.ai[0] = 4f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                }
                else if (NPC.timeLeft < 1800)
                    NPC.timeLeft = 1800;
            }

            if (lifeRatio <= 0.5f && Main.netMode != NetmodeID.MultiplayerClient && expertMode)
            {
                if (CalamityWorld.LegendaryMode && CalamityWorld.revenge)
                {
                    int type = ModContent.ProjectileType<UnstableEbonianGlob>();
                    for (int i = 0; i < 30; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, (float)Main.rand.Next(-1199, 1200) * 0.01f, (float)Main.rand.Next(-1199, 1200) * 0.01f, type, 35, 0f);
                }

                SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
                Vector2 spawnAt = NPC.Center + new Vector2(0f, NPC.height / 2f);
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X - 30, (int)spawnAt.Y, ModContent.NPCType<SplitEbonianPaladin>());
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X + 30, (int)spawnAt.Y, ModContent.NPCType<SplitEbonianPaladin>());
                if (Main.zenithWorld && NPC.CountNPCS(ModContent.NPCType<SplitCrimulanPaladin>()) < 3) // Split into 3 slimes if the other large slime hasn't split yet
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnAt.X, (int)spawnAt.Y - 30, ModContent.NPCType<SplitEbonianPaladin>());

                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            bool enraged = true;
            bool hyperMode = NPC.localAI[1] == 1f;
            if (CalamityGlobalNPC.slimeGodRed != -1)
            {
                if (Main.npc[CalamityGlobalNPC.slimeGodRed].active)
                    enraged = false;
            }

            if (bossRush)
            {
                enraged = true;
                hyperMode = true;
            }

            // For animating the wings
            NPC.localAI[0] += 1f;

            if (NPC.ai[0] == 0f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.TargetClosest();

                FlyMovement(NPC);

                if (NPC.timeLeft > 10)
                {
                    NPC.ai[1] += 1f;
                    int idleTime = bossRush ? 30 : death ? 75 : revenge ? 90 : 120;
                    if (NPC.ai[1] >= idleTime)
                    {
                        NPC.ai[1] = 0f;
                        switch ((int)NPC.Calamity().newAI[0])
                        {
                            default:
                                NPC.ai[0] = Main.rand.NextBool() ? 1f : 2f;
                                break;
                            case 1:
                                NPC.ai[0] = Main.rand.NextBool() ? 2f : 3f;
                                break;
                            case 2:
                                NPC.ai[0] = Main.rand.NextBool() ? 3f : 1f;
                                break;
                        }

                        // Pick directional movements for slime bombardment
                        if (NPC.ai[0] == 2f)
                            NPC.ai[2] = NPC.Center.X - player.Center.X > 0f ? 1f : -1f;

                        NPC.netUpdate = true;
                    }
                }
            }

            // Spread of slime projectiles
            else if (NPC.ai[0] == 1f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.rotation *= 0.9f;

                if (NPC.ai[2] == 1f)
                {
                    // Set animation timer to one in order to prevent weird quick animation here
                    NPC.localAI[0] = 1f;

                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] >= 10f)
                    {
                        float minFiringDistance = 160f; // 10 tile distance
                        if (Vector2.Distance(NPC.Center, player.Center) > minFiringDistance)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int numProjectiles = Main.getGoodWorld ? 12 : death ? 6 : 4;
                                float projectileVelocity = death ? 8f : 6f;
                                int type = ModContent.ProjectileType<UnstableEbonianGlob>();
                                int damage = NPC.GetProjectileDamage(type);
                                Vector2 destination = (new Vector2(NPC.Center.X, NPC.Center.Y + 100f) - NPC.Center).SafeNormalize(Vector2.UnitY);
                                destination *= projectileVelocity;
                                float rotation = MathHelper.ToRadians(80);
                                for (int i = 0; i < numProjectiles; i++)
                                {
                                    if (CalamityWorld.LegendaryMode)
                                        destination *= Main.rand.NextFloat() + 0.5f;

                                    Vector2 perturbedSpeed = destination.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProjectiles - 1)));
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + perturbedSpeed.SafeNormalize(Vector2.UnitY) * 30f * NPC.scale, perturbedSpeed, type, damage, 0f, Main.myPlayer);
                                }
                            }

                            SoundEngine.PlaySound(SlimeGodCore.BigShotSound, NPC.Center);
                        }

                        NPC.Calamity().newAI[0] = NPC.ai[0];
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.netUpdate = true;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] == 0f)
                {
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 50f)
                {
                    NPC.ai[1] = 50f;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 1f;
                        NPC.netUpdate = true;
                    }
                }

                FlyMovement(NPC);
            }

            // Fly overhead in a horizontal line and drop slime
            else if (NPC.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                FlyMovement(NPC, true);

                // Drop slime while flying horizontally
                if (NPC.ai[1] == 1f)
                {
                    NPC.ai[3] += 1f;
                    float slimeDropGateValue = bossRush ? 10f : death ? 20f : revenge ? 25f : expertMode ? 30f : 40f;
                    if (NPC.ai[3] % slimeDropGateValue == 0f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float projectileVelocity = 3f;
                            int type = ModContent.ProjectileType<UnstableEbonianGlob>();
                            int damage = NPC.GetProjectileDamage(type);
                            Vector2 destination = (new Vector2(NPC.Center.X, NPC.Center.Y + 100f) - NPC.Center).SafeNormalize(Vector2.UnitY);
                            destination *= projectileVelocity;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + destination.SafeNormalize(Vector2.UnitY) * 30f * NPC.scale, destination, type, damage, 0f, Main.myPlayer, 1f);
                        }

                        SoundEngine.PlaySound(SlimeGodCore.ShotSound, NPC.Center);
                    }

                    NPC.aiAction = 1;
                }

                if (NPC.ai[1] == 2f)
                {
                    NPC.Calamity().newAI[0] = NPC.ai[0];
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }

            // Charge by swooping down
            else if (NPC.ai[0] == 3f)
            {
                // Charge variables
                float chargeVelocityMult = 0.125f;
                float maxChargeVelocity = (bossRush || enraged) ? 24f : death ? 21f : revenge ? 19.5f : expertMode ? 18f : 15f;
                if (Main.getGoodWorld)
                    maxChargeVelocity *= 1.15f;
                if (CalamityWorld.LegendaryMode && revenge)
                    maxChargeVelocity *= 2f;

                float inertia = (bossRush || enraged) ? 50f : death ? 55f : revenge ? 57f : expertMode ? 60f : 65f;
                if (lifeRatio < 0.75f)
                    inertia *= 0.8f;
                if (Main.getGoodWorld)
                    inertia *= 0.8f;

                // Start charge
                if (NPC.ai[1] == 0f)
                {
                    // Set damage
                    NPC.damage = setDamage;

                    Vector2 velocity = (player.Center - NPC.Center).SafeNormalize(new Vector2(NPC.direction, 0f)) * maxChargeVelocity;
                    NPC.velocity = velocity * chargeVelocityMult;

                    NPC.ai[1] = 1f;
                    NPC.netUpdate = true;

                    SoundEngine.PlaySound(SlimeGodCore.BigShotSound, NPC.Center);

                    NPC.aiAction = 1;
                }
                else if (NPC.ai[1] == 1f)
                {
                    // Set damage
                    NPC.damage = setDamage;

                    NPC.ai[2] += 1f;
                    float phaseGateValue = (bossRush || enraged) ? 120f : death ? 140f : revenge ? 150f : expertMode ? 160f : 180f;
                    if (NPC.ai[2] >= phaseGateValue)
                    {
                        NPC.ai[1] = 2f;
                        float slowDownDurationAfterCharge = revenge ? 90f : 120f;
                        NPC.ai[2] = slowDownDurationAfterCharge;
                        NPC.localAI[2] = 0f;
                        NPC.velocity /= 2f;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        Vector2 targetVector = (player.Center - NPC.Center).SafeNormalize(new Vector2(NPC.direction, 0f));

                        if (NPC.localAI[2] == 0f)
                        {
                            // Accelerate
                            if (NPC.velocity.Length() < maxChargeVelocity)
                            {
                                float velocityMult = (bossRush || enraged) ? 1.045f : death ? 1.039f : revenge ? 1.036f : expertMode ? 1.033f : 1.03f;
                                NPC.velocity = targetVector * (NPC.velocity.Length() * velocityMult);
                                if (NPC.velocity.Length() > maxChargeVelocity)
                                {
                                    NPC.localAI[2] = 1f;
                                    NPC.velocity = NPC.velocity.SafeNormalize(new Vector2(NPC.direction, 0f)) * maxChargeVelocity;
                                }
                            }
                        }
                        else if (NPC.localAI[2] == 1f)
                        {
                            inertia *= 2f;
                            NPC.velocity = (NPC.velocity * (inertia - 1f) + targetVector * (NPC.velocity.Length() + (0.111111117f * inertia))) / inertia;

                            // Stop charging towards the player when within a certain distance
                            if (NPC.Distance(player.Center) < 130f * NPC.scale)
                                NPC.localAI[2] = 2f;
                        }
                        else
                        {
                            // Slow down
                            if (NPC.Distance(player.Center) >= 210f * NPC.scale || NPC.localAI[2] == 3f)
                            {
                                if (NPC.localAI[2] != 3f)
                                    NPC.localAI[2] = 3f;

                                NPC.velocity *= 0.97f;
                            }
                        }
                    }

                    NPC.aiAction = 1;
                }
                else
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[2] -= 1f;
                    if (NPC.ai[2] <= 0f)
                    {
                        NPC.Calamity().newAI[0] = NPC.ai[0];
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.localAI[2] = 0f;

                        NPC.TargetClosest();
                        NPC.netUpdate = true;
                    }

                    NPC.velocity *= 0.95f;
                }
            }

            // Despawn
            else if (NPC.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.Opacity -= 0.03f;

                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;

                if (NPC.Opacity < 0f)
                    NPC.Opacity = 0f;

                NPC.velocity.X *= 0.98f;
            }

            if (bossLife == 0f && NPC.life > 0)
                bossLife = NPC.lifeMax;

            if (NPC.life > 0)
            {
                float scaleRatio = lifeRatio;
                scaleRatio = scaleRatio * 0.5f + 0.75f;
                scaleRatio *= scale;

                if (scaleRatio != NPC.scale)
                {
                    NPC.position.X = NPC.position.X + (float)(NPC.width / 2);
                    NPC.position.Y = NPC.position.Y + (float)NPC.height;
                    NPC.scale = scaleRatio;
                    NPC.width = (int)(134f * NPC.scale);
                    NPC.height = (int)(150f * NPC.scale);
                    NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
                    NPC.position.Y = NPC.position.Y - (float)NPC.height;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int slimeSpawnThreshold = (int)((double)NPC.lifeMax * 0.2);
                    if ((float)(NPC.life + slimeSpawnThreshold) < bossLife)
                    {
                        bossLife = (float)NPC.life;
                        int randSlimeAmt = Main.rand.Next(1, 3);
                        for (int j = 0; j < randSlimeAmt; j++)
                        {
                            int x = (int)(NPC.position.X + (float)Main.rand.Next(NPC.width - 32));
                            int y = (int)(NPC.position.Y + (float)Main.rand.Next(NPC.height - 32));
                            int slimeType = ModContent.NPCType<CorruptSlimeSpawn>();
                            int slimeSpawn = NPC.NewNPC(NPC.GetSource_FromAI(), x, y, slimeType);
                            Main.npc[slimeSpawn].SetDefaults(slimeType);
                            Main.npc[slimeSpawn].velocity.X = (float)Main.rand.Next(-15, 16) * 0.1f;
                            Main.npc[slimeSpawn].velocity.Y = (float)Main.rand.Next(-30, 1) * 0.1f;
                            Main.npc[slimeSpawn].ai[0] = (float)(-1000 * Main.rand.Next(3));
                            Main.npc[slimeSpawn].ai[1] = 0f;
                            if (Main.netMode == NetmodeID.Server && slimeSpawn < Main.maxNPCs)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slimeSpawn);
                        }
                    }
                }
            }
        }

        public static void FlyMovement(NPC npc, bool slimeBombardment = false)
        {
            // Difficulty bools
            bool bossRush = BossRushEvent.BossRushActive;
            bool masterMode = Main.masterMode;
            bool death = CalamityWorld.death;
            bool revenge = CalamityWorld.revenge;
            bool ableToDropSlime = npc.ai[1] == 1f;

            float flyVelocity = bossRush ? 20f : death ? 15f : revenge ? 12.5f : 10f;
            float flyAcceleration = bossRush ? 0.24f : death ? 0.18f : revenge ? 0.15f : 0.12f;

            if (slimeBombardment)
            {
                // Increase max velocity while attempting to line up the bombardment
                if (!ableToDropSlime)
                    flyVelocity *= 1.5f;

                // Keep acceleration high to maintain the bombardment path
                flyAcceleration *= 2f;
            }

            float slimeBombardmentDistance = 720f;
            float flyDistanceY = slimeBombardment ? -450f : masterMode ? -360f : -405f;
            float flyDistanceX = slimeBombardment ? (ableToDropSlime ? (slimeBombardmentDistance * -npc.ai[2]) : (slimeBombardmentDistance * npc.ai[2])) : 0f;
            Vector2 destination = Main.player[npc.target].Center + new Vector2(flyDistanceX, flyDistanceY);

            if (slimeBombardment)
            {
                if (!ableToDropSlime)
                {
                    if (npc.Distance(destination) < 80f)
                        npc.ai[1] = 1f;
                }
                else
                {
                    if (npc.ai[3] >= 180f || npc.Distance(destination) < 80f)
                        npc.ai[1] = 2f;
                }
            }

            Vector2 desiredVelocity = npc.Center;

            if (npc.timeLeft > 10)
            {
                if (!Collision.CanHit(npc, Main.player[npc.target]) && !slimeBombardment)
                {
                    bool flyToSolidTilesAboveTarget = false;
                    Vector2 center = Main.player[npc.target].Center;
                    for (int i = 0; i < 16; i++)
                    {
                        float tileDistanceAboveTarget = 16 * i;
                        Point point = (center + new Vector2(0f, -tileDistanceAboveTarget)).ToTileCoordinates();
                        if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
                        {
                            desiredVelocity = center + new Vector2(0f, -tileDistanceAboveTarget + 16f) - npc.Center;
                            flyToSolidTilesAboveTarget = true;
                            break;
                        }
                    }

                    if (!flyToSolidTilesAboveTarget)
                        desiredVelocity = center - npc.Center;
                }
                else
                    desiredVelocity = destination - npc.Center;
            }
            else
                desiredVelocity = npc.Center + new Vector2(500f * npc.direction, flyDistanceY) - npc.Center;

            float distanceFromFlightTarget = desiredVelocity.Length();
            if (Math.Abs(desiredVelocity.X) < 40f)
                desiredVelocity.X = npc.velocity.X;

            if (distanceFromFlightTarget > 100f && ((npc.velocity.X < -12f && desiredVelocity.X > 0f) || (npc.velocity.X > 12f && desiredVelocity.X < 0f)))
                flyAcceleration *= 1.5f;

            if (distanceFromFlightTarget < 40f)
            {
                desiredVelocity = npc.velocity;
            }
            else if (distanceFromFlightTarget < 80f)
            {
                desiredVelocity = desiredVelocity.SafeNormalize(Vector2.UnitY);
                desiredVelocity *= flyVelocity * 0.65f;
            }
            else
            {
                desiredVelocity = desiredVelocity.SafeNormalize(Vector2.UnitY);
                desiredVelocity *= flyVelocity;
            }

            npc.SimpleFlyMovement(desiredVelocity, flyAcceleration);
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

            return minDist <= 65f * NPC.scale;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            Color lightColor = new Color(200, 150, Main.DiscoB, NPC.alpha);
            Color newColor = NPC.localAI[1] == 1f ? lightColor : drawColor;
            return newColor * NPC.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Texture2D wingTexture = WingTexture.Value;
            Color drawColorAlpha = NPC.GetAlpha(drawColor);

            // Stretch based on Y velocity.
            float vel = NPC.velocity.Length();

            float stretch = addedStretch;

            float rot = MathHelper.Lerp(0f, MathHelper.WrapAngle(Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.ToRadians(90f)), Math.Clamp(vel / 30f, 0f, 1f));

            NPC.rotation = MathHelper.Lerp(NPC.rotation, rot, 0.1f);

            // Stretch rapidly if about to jump or teleport.
            if (NPC.aiAction == 1)
                stretch += MathHelper.Lerp(-0.24f, 0.24f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) / 2f + 0.5f) * (vel / 30);

            // Stretch while idle.
            else
                stretch += MathHelper.Lerp(-0.1f, 0.1f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) / 2f + 0.5f) * (vel / 30);

            if (stretch > 0.5f)
                stretch = 0.5f;

            Vector2 scaleStretch = new Vector2(1f - stretch, 1f + stretch) * NPC.scale;

            // Used for animating the wings.
            bool blobSpreadPhase = NPC.ai[0] == 1f;
            bool useFurledWingFrame = blobSpreadPhase && NPC.ai[1] >= 20f;
            bool useSpreadWingFrame = blobSpreadPhase && NPC.ai[2] == 1f;
            bool slimeBombardmentPhase = NPC.ai[0] == 2f;
            bool slightlyIncreaseWingAnimationSpeed = slimeBombardmentPhase && NPC.ai[1] == 1f;
            bool swoopingPhase = NPC.ai[0] == 3f;
            bool increaseWingAnimationSpeed = swoopingPhase && NPC.ai[1] < 2f;
            bool decreaseWingAnimationSpeed = swoopingPhase && NPC.ai[1] == 2f;
            if (useFurledWingFrame)
            {
                wingFrameDrawn = 4;
            }
            else if (useSpreadWingFrame)
            {
                wingFrameDrawn = 0;
            }
            else if (NPC.localAI[0] % (decreaseWingAnimationSpeed ? 9f : increaseWingAnimationSpeed ? 4f : slightlyIncreaseWingAnimationSpeed ? 5f : 6f) == 0f)
            {
                wingFrameDrawn++;
                if (wingFrameDrawn >= TotalWingFrames)
                    wingFrameDrawn = 0;
            }
            Rectangle wingFrame1 = new Rectangle(0, wingTexture.Height / TotalWingFrames * wingFrameDrawn, wingTexture.Width / 2, wingTexture.Height / TotalWingFrames);
            Rectangle wingFrame2 = new Rectangle(wingTexture.Width / 2, wingTexture.Height / TotalWingFrames * wingFrameDrawn, wingTexture.Width / 2, wingTexture.Height / TotalWingFrames);
            
            Vector2 wingOrigin1 = new Vector2(wingTexture.Width, wingTexture.Height / TotalWingFrames) * 0.5f;
            Vector2 wingOrigin2 = new Vector2(0, wingTexture.Height / TotalWingFrames) * 0.5f;

            Vector2 wingOffset1 = Vector2.Zero;
            Vector2 wingOffset2 = Vector2.Zero;

            // Draw the wings.
            spriteBatch.Draw(wingTexture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY) + wingOffset1, wingFrame1, drawColorAlpha, 0f, wingOrigin1, NPC.scale, spriteEffects, 0f);
            spriteBatch.Draw(wingTexture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY) + wingOffset2, wingFrame2, drawColorAlpha, 0f, wingOrigin2, NPC.scale, spriteEffects, 0f);

            // Draw the actual paladin.
            spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY) + new Vector2(0, 16), NPC.frame, drawColorAlpha, NPC.rotation, NPC.frame.Size() * 0.5f + new Vector2(0, 8), scaleStretch, spriteEffects, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            Color dustColor = Color.Lavender;
            dustColor.A = 150;
            for (int k = 0; k < 5; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hit.HitDirection, -1f, NPC.alpha, dustColor, 1f);
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(BuffID.Weak, 300, true);
        }
    }
}
