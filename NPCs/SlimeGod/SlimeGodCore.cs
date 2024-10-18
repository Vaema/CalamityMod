using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Furniture.BossRelics;
using CalamityMod.Items.Placeables.Furniture.DevPaintings;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.Potions;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Projectiles.Boss;
using CalamityMod.UI.VanillaBossBars;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SlimeGod
{
    [AutoloadBossHead]
    public class SlimeGodCore : ModNPC
    {
        private bool slimesSpawned = false;
        private int buffedSlime = 0;

        public static readonly SoundStyle PossessionSound = new("CalamityMod/Sounds/Custom/SlimeGodPossession");
        public static readonly SoundStyle ExitSound = new("CalamityMod/Sounds/Custom/SlimeGodExit");
        public static readonly SoundStyle ShotSound = new("CalamityMod/Sounds/Custom/SlimeGodShot", 2);
        public static readonly SoundStyle BigShotSound = new("CalamityMod/Sounds/Custom/SlimeGodBigShot", 2);

        public static Asset<Texture2D> ZenithSeedEyeTexture;
        public static Asset<Texture2D> EyeTexture;
        public static Asset<Texture2D> OverlayTexture;

        private int eyeFrameDrawn = 0;
        private int eyeFrameX = 0;
        private const int TotalXEyeFrames = 2;
        private const int TotalEyeFrames = 25;
        private const int TotalEyeFrames_FirstColumn = 19;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Scale = 0.5f,
                PortraitScale = 0.6f,
                CustomTexturePath = "CalamityMod/ExtraTextures/Bestiary/SlimeGod_Bestiary",
                PortraitPositionXOverride = 40,
                PortraitPositionYOverride = 40
            };
            value.Position.X += 65;
            value.Position.Y += 35;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            if (!Main.dedServ)
            {
                ZenithSeedEyeTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SlimeGod/SlimeGodEyes", AssetRequestMode.AsyncLoad);
                EyeTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SlimeGod/ExtraTextures/SlimeGodCoreEye", AssetRequestMode.AsyncLoad);
                OverlayTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SlimeGod/ExtraTextures/SlimeGodCoreOverlay", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.GetNPCDamage();
            NPC.npcSlots = 10f;
            NPC.width = 96;
            NPC.height = 98;
            if (CalamityWorld.LegendaryMode && CalamityWorld.revenge)
                NPC.scale = 2f;

            NPC.defense = 6;
            NPC.LifeMaxNERB(420);
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 12, 0, 0);
            NPC.Opacity = 0.8f;
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<SlimeGodBossBar>();
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;

            // Scale HP in Master
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC, true);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.SlimeGodCore")
            });
        }

        public static bool ShouldDespawn(NPC npc)
        {
            return (!EbonianPaladinAlive(npc) && !CrimulanPaladinAlive(npc)) || npc.Calamity().newAI[3] == 1f || npc.ai[3] == 1f;
        }

        public static bool EbonianPaladinAlive(NPC npc)
        {
            if (CalamityGlobalNPC.slimeGodPurple != -1)
            {
                if (Main.npc[CalamityGlobalNPC.slimeGodPurple].active)
                {
                    if ((npc.ModNPC as SlimeGodCore).buffedSlime == 1)
                        Main.npc[CalamityGlobalNPC.slimeGodPurple].localAI[1] = 1f;
                    else
                        Main.npc[CalamityGlobalNPC.slimeGodPurple].localAI[1] = 0f;

                    npc.Calamity().newAI[0] = Main.npc[CalamityGlobalNPC.slimeGodPurple].Center.X;
                    npc.Calamity().newAI[1] = Main.npc[CalamityGlobalNPC.slimeGodPurple].Center.Y;

                    // Despawn check
                    npc.Calamity().newAI[3] = Main.npc[CalamityGlobalNPC.slimeGodPurple].ai[0] == 4f ? 1f : 0f;

                    return true;
                }
            }
            return false;
        }

        public static bool CrimulanPaladinAlive(NPC npc)
        {
            if (CalamityGlobalNPC.slimeGodRed != -1)
            {
                if (Main.npc[CalamityGlobalNPC.slimeGodRed].active)
                {
                    if ((npc.ModNPC as SlimeGodCore).buffedSlime == 2)
                        Main.npc[CalamityGlobalNPC.slimeGodRed].localAI[1] = 1f;
                    else
                        Main.npc[CalamityGlobalNPC.slimeGodRed].localAI[1] = 0f;

                    npc.ai[1] = Main.npc[CalamityGlobalNPC.slimeGodRed].Center.X;
                    npc.ai[2] = Main.npc[CalamityGlobalNPC.slimeGodRed].Center.Y;

                    // Despawn check
                    npc.Calamity().newAI[3] = Main.npc[CalamityGlobalNPC.slimeGodRed].ai[0] == 3f ? 1f : 0f;

                    return true;
                }
            }
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(slimesSpawned);
            writer.Write(buffedSlime);
            writer.Write(NPC.Opacity);
            for (int i = 0; i < 4; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            slimesSpawned = reader.ReadBoolean();
            buffedSlime = reader.ReadInt32();
            NPC.Opacity = reader.ReadSingle();
            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void AI()
        {
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            CalamityGlobalNPC.slimeGod = NPC.whoAmI;

            bool bossRush = BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || bossRush;
            bool revenge = CalamityWorld.revenge || bossRush;
            bool death = CalamityWorld.death || bossRush;

            // For animating the eye
            NPC.localAI[0] += 1f;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (!slimesSpawned)
            {
                slimesSpawned = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<EbonianPaladin>());
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<CrimulanPaladin>());
                }
            }

            // Used to gauge how aggressive the core is, based on Paladins alive
            // Stay near the Paladins and do nothing else at level 0
            // Fire projectiles at level 1
            // Charge at level 2
            // Follow the player around instead of the Paladins at level 3
            // Buff a Paladin far sooner, fire more projectiles, and charge faster at level 4
            int aggressionLevel = 0;
            if (!NPC.AnyNPCs(ModContent.NPCType<EbonianPaladin>()) && !NPC.AnyNPCs(ModContent.NPCType<CrimulanPaladin>()))
            {
                aggressionLevel = 1;
                int splitPaladinCount = NPC.CountNPCS(ModContent.NPCType<SplitEbonianPaladin>()) + NPC.CountNPCS(ModContent.NPCType<SplitCrimulanPaladin>());
                if (splitPaladinCount < 2)
                    aggressionLevel = 4;
                else if (splitPaladinCount < 3)
                    aggressionLevel = 3;
                else if (splitPaladinCount < 4)
                    aggressionLevel = 2;
            }

            // Enrage based on large slimes
            bool purpleSlimeAlive = EbonianPaladinAlive(NPC);
            bool redSlimeAlive = CrimulanPaladinAlive(NPC);

            // Start shooting blobs more often, move faster and buff large slimes more often if one type of large slime is dead
            bool phase2 = !purpleSlimeAlive || !redSlimeAlive;

            // Vanish phase
            if (ShouldDespawn(NPC))
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                // Make sure Opacity is set to 0.8f if it's below that when the vanish phase starts
                if (NPC.ai[3] == 0f)
                {
                    if (!NPC.AnyNPCs(ModContent.NPCType<EbonianPaladin>()) && !NPC.AnyNPCs(ModContent.NPCType<CrimulanPaladin>()))
                    {
                        NPC.ai[3] = 1f;
                        NPC.Opacity = 0.8f;
                    }
                }

                // Emit dust
                if (!Main.zenithWorld) // you must see his glory.
                {
                    for (int k = 0; k < 5; k++)
                    {
                        Color color = Main.rand.NextBool() ? Color.Lavender : Color.Crimson;
                        color.A = 150;
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 1f);
                    }
                }

                // Slow down
                NPC.velocity *= 0.97f;

                // Rotate
                NPC.rotation += NPC.direction * 0.3f;

                // Gradually turn invisible
                NPC.Opacity -= 0.005f;

                // Drop loot, explode into dust and vanish once invisible
                if (NPC.Opacity <= 0f)
                {
                    NPC.Opacity = 0f;
                    SoundEngine.PlaySound(PossessionSound, NPC.Center);
                    NPC.position.X = NPC.position.X + (NPC.width / 2);
                    NPC.position.Y = NPC.position.Y + (NPC.height / 2);
                    NPC.width = 40;
                    NPC.height = 40;
                    NPC.position.X = NPC.position.X - (NPC.width / 2);
                    NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                    for (int i = 0; i < 40; i++)
                    {
                        Color color = Main.rand.NextBool() ? Color.Lavender : Color.Crimson;
                        color.A = 150;
                        int slimyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 2f);
                        Main.dust[slimyDust].velocity *= 3f;
                        if (Main.rand.NextBool())
                        {
                            Main.dust[slimyDust].scale = 0.5f;
                            Main.dust[slimyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                        }
                    }
                    for (int j = 0; j < 70; j++)
                    {
                        Color color = Main.rand.NextBool() ? Color.Lavender : Color.Crimson;
                        color.A = 150;
                        int slimyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 3f);
                        Main.dust[slimyDust2].noGravity = true;
                        Main.dust[slimyDust2].velocity *= 5f;
                        slimyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 2f);
                        Main.dust[slimyDust2].velocity *= 2f;
                    }

                    if (calamityGlobalNPC.newAI[3] != 1f)
                    {
                        // Let the player know that the Slime God isn't dead fr
                        if (!DownedBossSystem.downedSlimeGod)
                        {
                            string key = "Mods.CalamityMod.Status.Boss.SlimeGodRun";
                            Color messageColor = Color.Magenta;

                            CalamityUtils.DisplayLocalizedText(key, messageColor);
                        }

                        // Set Slime God to have interacted with all players
                        for (int i = Main.maxPlayers - 1; i >= 0; i--)
                            NPC.ApplyInteraction(i);

                        NPC.active = false;
                        NPC.HitEffect();
                        NPC.NPCLoot();
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        for (int x = 0; x < Main.maxNPCs; x++)
                        {
                            if (Main.npc[x].type == ModContent.NPCType<EbonianPaladin>() || Main.npc[x].type == ModContent.NPCType<SplitEbonianPaladin>() ||
                                Main.npc[x].type == ModContent.NPCType<CrimulanPaladin>() || Main.npc[x].type == ModContent.NPCType<SplitCrimulanPaladin>())
                            {
                                Main.npc[x].active = false;
                                Main.npc[x].netUpdate = true;
                            }
                        }

                        NPC.active = false;
                        NPC.HitEffect();
                        NPC.netUpdate = true;
                    }
                }

                return;
            }

            // Despawn
            if (!player.active || player.dead || Vector2.Distance(player.Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead || Vector2.Distance(player.Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles)
                {
                    if (NPC.velocity.Y < -3f)
                        NPC.velocity.Y = -3f;
                    NPC.velocity.Y += 0.2f;
                    if (NPC.velocity.Y > 16f)
                        NPC.velocity.Y = 16f;

                    if (NPC.position.Y > Main.worldSurface * 16D)
                    {
                        for (int x = 0; x < Main.maxNPCs; x++)
                        {
                            if (Main.npc[x].type == ModContent.NPCType<EbonianPaladin>() || Main.npc[x].type == ModContent.NPCType<SplitEbonianPaladin>() ||
                                Main.npc[x].type == ModContent.NPCType<CrimulanPaladin>() || Main.npc[x].type == ModContent.NPCType<SplitCrimulanPaladin>())
                            {
                                Main.npc[x].active = false;
                                Main.npc[x].netUpdate = true;
                            }
                        }
                        NPC.active = false;
                        NPC.netUpdate = true;
                    }

                    NPC.Opacity = 0.8f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    calamityGlobalNPC.newAI[0] = 0f;
                    calamityGlobalNPC.newAI[1] = 0f;
                    calamityGlobalNPC.newAI[2] = 0f;
                    NPC.netUpdate = true;
                    return;
                }
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;

            // Hide inside large slime
            float hideInsideLargeSlimePhaseGateValue = aggressionLevel == 4 ? 120f : phase2 ? 300f : 900f;
            float hideInsideLargeSlimePhaseDuration = 600f;
            float exitLargeSlimeGateValue = hideInsideLargeSlimePhaseGateValue + hideInsideLargeSlimePhaseDuration;
            calamityGlobalNPC.newAI[2] += 1f;
            if (calamityGlobalNPC.newAI[2] >= hideInsideLargeSlimePhaseGateValue)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.rotation += NPC.direction * 0.3f;

                if (buffedSlime == 0)
                {
                    SoundEngine.PlaySound(PossessionSound, NPC.Center);

                    if (purpleSlimeAlive && redSlimeAlive)
                        buffedSlime = Main.rand.Next(2) + 1;
                    else if (purpleSlimeAlive)
                        buffedSlime = 1;
                    else if (redSlimeAlive)
                        buffedSlime = 2;
                }

                Vector2 purpleSlimeVector = new Vector2(calamityGlobalNPC.newAI[0], calamityGlobalNPC.newAI[1]);
                Vector2 redSlimeVector = new Vector2(NPC.ai[1], NPC.ai[2]);
                Vector2 goToVector = buffedSlime == 1 ? purpleSlimeVector : redSlimeVector;

                Vector2 goToPosition = goToVector - NPC.Center;
                NPC.velocity = Vector2.Normalize(goToPosition) * 24f;

                // Reduce velocity to 0 to avoid spastic movement when inside big slime.
                float stickPositionAdjustment = buffedSlime == 1 ? 16f : 32f;
                if (Vector2.Distance(NPC.Center, goToVector) < 80f + stickPositionAdjustment)
                {
                    NPC.velocity = Vector2.Zero;
                    NPC.Center = goToVector + Vector2.UnitY * stickPositionAdjustment;

                    NPC.Opacity -= 0.2f;
                    if (NPC.Opacity < 0f)
                        NPC.Opacity = 0f;
                }

                bool slimeDead;
                if (goToVector == purpleSlimeVector)
                    slimeDead = CalamityGlobalNPC.slimeGodPurple < 0 || !Main.npc[CalamityGlobalNPC.slimeGodPurple].active;
                else
                    slimeDead = CalamityGlobalNPC.slimeGodRed < 0 || !Main.npc[CalamityGlobalNPC.slimeGodRed].active;

                if (calamityGlobalNPC.newAI[2] >= exitLargeSlimeGateValue || slimeDead)
                {
                    NPC.TargetClosest();
                    calamityGlobalNPC.newAI[2] = 0f;
                    NPC.velocity = Vector2.UnitY * -12f;
                    SoundEngine.PlaySound(ExitSound, NPC.Center);
                    for (int i = 0; i < 20; i++)
                    {
                        Color color = Main.rand.NextBool() ? Color.Lavender : Color.Crimson;
                        color.A = 150;
                        int dust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 2f);
                        Main.dust[dust2].velocity *= 3f;
                        if (Main.rand.NextBool())
                        {
                            Main.dust[dust2].scale = 0.5f;
                            Main.dust[dust2].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                        }
                    }
                    for (int j = 0; j < 30; j++)
                    {
                        Color color = Main.rand.NextBool() ? Color.Lavender : Color.Crimson;
                        color.A = 150;
                        int dust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 3f);
                        Main.dust[dust2].noGravity = true;
                        Main.dust[dust2].velocity *= 5f;
                        dust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, NPC.alpha, color, 2f);
                        Main.dust[dust2].velocity *= 2f;
                    }
                }

                return;
            }

            // Avoid cheap bullshit
            NPC.damage = 0;

            if (expertMode && aggressionLevel >= 1)
            {
                float divisor = bossRush ? 50f : death ? 90f : revenge ? 120f : 150f;
                divisor -= (aggressionLevel - 1) * 10f;
                if (aggressionLevel == 4)
                    divisor *= 0.5f;

                if (calamityGlobalNPC.newAI[2] % divisor == 0f)
                {
                    SoundEngine.PlaySound(ShotSound, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (Main.rand.NextBool())
                        {
                            float projectileVelocity = 4f + (aggressionLevel - 1) * 2f;
                            int type = ModContent.ProjectileType<UnstableEbonianGlob>();
                            int damage = NPC.GetProjectileDamage(type);
                            Vector2 velocity = Vector2.Normalize(player.Center - NPC.Center) * projectileVelocity;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, damage, 0f, Main.myPlayer);
                        }
                        else
                        {
                            float projectileVelocity = 8f + (aggressionLevel - 1) * 2f;
                            int type = ModContent.ProjectileType<UnstableCrimulanGlob>();
                            int damage = NPC.GetProjectileDamage(type);
                            Vector2 velocity = Vector2.Normalize(player.Center - NPC.Center) * projectileVelocity;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, type, damage, 0f, Main.myPlayer);
                        }
                    }
                }
            }

            NPC.Opacity += 0.2f;
            if (NPC.Opacity > 0.8f)
                NPC.Opacity = 0.8f;

            buffedSlime = 0;

            float flySpeed = death ? 15f : revenge ? 13.5f : expertMode ? 12f : 9f;
            flySpeed += aggressionLevel;
            if (aggressionLevel == 4)
                flySpeed += 3f;
            if (phase2)
                flySpeed *= 1.1f;
            if (bossRush)
                flySpeed *= 1.2f;
            if (Main.getGoodWorld)
                flySpeed *= 1.3f;

            Vector2 flyDirection = new Vector2(NPC.Center.X + (NPC.direction * 20), NPC.Center.Y + 6f);
            Vector2 flyDestination = aggressionLevel >= 3 ? player.Center : GetFlyDestination(player);
            Vector2 idealVelocity = (flyDestination - flyDirection).SafeNormalize(Vector2.UnitY) * flySpeed;

            float distanceFromFlyDestination = NPC.Distance(flyDestination);

            if (aggressionLevel >= 2)
            {
                NPC.ai[0] -= 1f;
                if (distanceFromFlyDestination < 200f || NPC.ai[0] > 0f)
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    if (distanceFromFlyDestination < 200f)
                        NPC.ai[0] = 20f;

                    if (NPC.velocity.X < 0f)
                        NPC.direction = -1;
                    else
                        NPC.direction = 1;

                    NPC.rotation += NPC.direction * 0.3f;

                    return;
                }
            }

            if (distanceFromFlyDestination < 150f && aggressionLevel < 2)
            {
                if (NPC.velocity.Length() > flySpeed * 0.2f)
                    NPC.velocity *= 0.9f;
            }
            else
            {
                float inertia = 50f;
                inertia -= aggressionLevel * 3f;
                if (Main.getGoodWorld)
                    inertia *= 0.8f;
                if (CalamityWorld.LegendaryMode && CalamityWorld.revenge)
                    inertia *= Main.rand.NextFloat(0.2f, 1f);

                NPC.velocity = (NPC.velocity * inertia + idealVelocity) / (inertia + 1f);
                if (distanceFromFlyDestination < 350f)
                    NPC.velocity = (NPC.velocity * 10f + idealVelocity) / 11f;
                if (distanceFromFlyDestination < 300f)
                    NPC.velocity = (NPC.velocity * 7f + idealVelocity) / 8f;
            }

            NPC.rotation = NPC.velocity.X * 0.05f;
        }

        public Vector2 GetFlyDestination(Player target)
        {
            // Find all large slimes in the world.
            // If multiple slimes are present, and they are all relatively close together, try to stay in their general area.
            // If they are far apart, try to stay towards the closest slime.
            // If no slimes exist, or they are all extremely far away, try to stay near the target player instead.

            int largeCrimulanPaladin = ModContent.NPCType<CrimulanPaladin>();
            int splitCrimulanPaladin = ModContent.NPCType<SplitCrimulanPaladin>();
            int largeEbonianPaladin = ModContent.NPCType<EbonianPaladin>();
            int splitEbonianPaladin = ModContent.NPCType<SplitEbonianPaladin>();
            List<NPC> largeSlimes = new();

            float ignoreGeneralAreaDistanceThreshold = 750f;
            float ignoreAllSlimesDistanceThreshold = 3200f;

            // Find all slimes within a generous area.
            foreach (NPC n in Main.ActiveNPCs)
            {
                int npcType = n.type;
                if (npcType != largeCrimulanPaladin && npcType != splitCrimulanPaladin && npcType != largeEbonianPaladin && npcType != splitEbonianPaladin)
                    continue;

                if (!NPC.WithinRange(n.Center, ignoreAllSlimesDistanceThreshold))
                    continue;

                largeSlimes.Add(n);
            }

            // If no slimes were found, don't bother doing any more calculations. Just use the player's center.
            if (largeSlimes.Count <= 0)
                return target.Center;

            // Find the closest slime.
            NPC closestSlime = largeSlimes.OrderBy(n => n.Distance(NPC.Center)).First();

            // Get the general area of all the slimes by averaging together their positions.
            Vector2 generalSlimeArea = Vector2.Zero;
            for (int i = 0; i < largeSlimes.Count; i++)
                generalSlimeArea += largeSlimes[i].Center;
            generalSlimeArea /= largeSlimes.Count;

            // Determine the average deviation of all slimes from the general area.
            // This provides a general idea of how far apart all the slimes are from each-other.
            float averageGeneralAreaDistanceDeviation = largeSlimes.Average(s => s.Distance(generalSlimeArea));

            // The slimes are too far apart. Simply go with the closest slime.
            if (averageGeneralAreaDistanceDeviation > ignoreGeneralAreaDistanceThreshold)
                return closestSlime.Center;

            // Otherwise, use the average general position as a place to hover.
            return generalSlimeArea;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            Color newColor = new Color(255, 255, 255, 255);
            return newColor * NPC.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float sc = 1f;
            if (!ShouldDespawn(NPC))
            {
                if (buffedSlime != 0f)
                {
                    NPC.scale = MathHelper.Lerp(NPC.scale, 0f, 0.2f);
                    NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.7f, 0.4f);
                }
                else
                {
                    NPC.scale = MathHelper.Lerp(NPC.scale, sc, 0.2f);
                    NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1f, 0.4f);
                }
            }

            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color drawColorAlpha = (buffedSlime != 0f && !ShouldDespawn(NPC)) ? new Color(200, 150, Main.DiscoB, NPC.alpha) * NPC.Opacity : NPC.GetAlpha(drawColor);
            Vector2 origin = NPC.frame.Size() * 0.5f;
            Vector2 halfSize = NPC.Size * 0.5f;

            Texture2D texture = TextureAssets.Npc[Type].Value;
            Texture2D eyeTexture = EyeTexture.Value;
            Texture2D overlayTexture = OverlayTexture.Value;
            Texture2D pog = ZenithSeedEyeTexture.Value;

            // Used for animating the eye
            if (NPC.localAI[0] % 6f == 0f)
            {
                eyeFrameDrawn++;
                if (eyeFrameX == 0)
                {
                    if (eyeFrameDrawn >= TotalEyeFrames_FirstColumn)
                    {
                        eyeFrameDrawn = 0;
                        eyeFrameX = 1;
                    }
                }
                else
                {
                    if (eyeFrameDrawn >= TotalEyeFrames)
                    {
                        eyeFrameDrawn = 0;
                        eyeFrameX = 0;
                    }
                }
            }
            Rectangle eyeFrame = new Rectangle(eyeTexture.Width / TotalXEyeFrames * eyeFrameX, eyeTexture.Height / TotalEyeFrames * eyeFrameDrawn, eyeTexture.Width / TotalXEyeFrames, eyeTexture.Height / TotalEyeFrames);
            Vector2 eyeOrigin = new Vector2(eyeTexture.Width / TotalXEyeFrames, eyeTexture.Height / TotalEyeFrames) * 0.5f;

            if (!Main.zenithWorld)
            {
                Vector2 drawPositionAdjustment = halfSize - screenPos + new Vector2(0f, NPC.gfxOffY);
                float colorAlphaDivisor = NPCID.Sets.TrailCacheLength[Type] * 1.5f;
                int twoConst = 2;
                int coreID = 1;

                while (((twoConst > 0 && coreID < 8) || (twoConst < 0 && coreID > 8)) && CalamityClientConfig.Instance.Afterimages)
                {
                    float trailLengthMult = (float)(8 - coreID);
                    if (twoConst < 0)
                        trailLengthMult = (float)(1 - coreID);

                    drawColorAlpha *= trailLengthMult / colorAlphaDivisor;
                    Vector2 drawPosition = NPC.oldPos[coreID] + drawPositionAdjustment;

                    // Draw the base texture afterimages
                    spriteBatch.Draw(texture, drawPosition, NPC.frame, drawColorAlpha, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                    
                    // Draw the eye texture afterimages
                    spriteBatch.Draw(eyeTexture, drawPosition, eyeFrame, drawColorAlpha, 0f, eyeOrigin, 1f, spriteEffects, 0f);
                    
                    // Draw the overlay texture afterimages
                    spriteBatch.Draw(overlayTexture, drawPosition, NPC.frame, drawColorAlpha, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                    
                    coreID += twoConst;
                }
            }

            // Reset the color
            drawColorAlpha = (buffedSlime != 0f && !ShouldDespawn(NPC)) ? new Color(200, 150, Main.DiscoB, NPC.alpha) * NPC.Opacity : NPC.GetAlpha(drawColor);

            // Draw the base texture
            spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, drawColorAlpha, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            // Draw the eye
            spriteBatch.Draw(eyeTexture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), eyeFrame, drawColorAlpha, 0f, eyeOrigin, 1f, spriteEffects, 0f);

            // Draw the overlay
            spriteBatch.Draw(overlayTexture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, drawColorAlpha, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            if (Main.zenithWorld)
                spriteBatch.Draw(pog, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, drawColorAlpha, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
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

            return minDist <= 40f * NPC.scale;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void OnKill()
        {
            // Don't bother running any of this in Boss Rush.
            if (BossRushEvent.BossRushActive)
                return;

            CalamityGlobalNPC.SetNewBossJustDowned(NPC);

            CalamityGlobalNPC.SetNewShopVariable(new int[] { ModContent.NPCType<THIEF>() }, DownedBossSystem.downedSlimeGod);

            // Mark the Slime God as dead
            DownedBossSystem.downedSlimeGod = true;
            CalamityNetcode.SyncWorld();
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Every Slime God piece drops Gel, even if it's not the last one.
            npcLoot.Add(ItemID.Gel, 1, 32, 48);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<SlimeGodBag>()));

            // Normal drops: Everything that would otherwise be in the bag
            LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
            npcLoot.Add(normalOnly);
            {
                // Weapons
                int[] weapons = new int[]
                {
                    ModContent.ItemType<OverloadedBlaster>(),
                    ModContent.ItemType<AbyssalTome>(),
                    ModContent.ItemType<EldritchTome>(),
                    ModContent.ItemType<CorroslimeStaff>(),
                    ModContent.ItemType<CrimslimeStaff>()
                };
                normalOnly.Add(DropHelper.CalamityStyle(DropHelper.NormalWeaponDropRateFraction, weapons));

                // Materials
                normalOnly.Add(DropHelper.PerPlayer(ModContent.ItemType<PurifiedGel>(), 1, 30, 45));

                // Vanity
                normalOnly.Add(ModContent.ItemType<SlimeGodMask>(), 7);
                normalOnly.Add(ModContent.ItemType<SlimeGodMask2>(), 7);
                normalOnly.Add(ModContent.ItemType<ThankYouPainting>(), ThankYouPainting.DropInt);

                // Equipment
                normalOnly.Add(ModContent.ItemType<ManaPolarizer>());
            }

            npcLoot.Add(ModContent.ItemType<SlimeGodTrophy>(), 10);

            // Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ModContent.ItemType<SlimeGodRelic>());

            // GFB Gelatin Crystal drop
            npcLoot.DefineConditionalDropSet(DropHelper.GFB).Add(ItemID.QueenSlimeCrystal, hideLootReport: true);

            // Lore
            npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedSlimeGod, ModContent.ItemType<LoreSlimeGod>(), desc: DropHelper.FirstKillText);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Color color = Main.rand.NextBool() ? Color.Lavender : Color.Crimson;
                color.A = 150;
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hit.HitDirection, -1f, NPC.alpha, color, 1f);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                int debufftype = Main.zenithWorld ? BuffID.VortexDebuff : BuffID.Slow;
                target.AddBuff(debufftype, 180, true);
                target.AddBuff(BuffID.Weak, 180, true);
                target.AddBuff(BuffID.Darkness, 180, true);
            }
        }
    }
}
