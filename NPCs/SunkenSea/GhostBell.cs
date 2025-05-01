using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class GhostBell : SunkenSeaNPC
    {
        public enum JellyColor
        {
            Blue = 0,
            Pink = 1,
            Green = 2
        }

        public static Asset<Texture2D> PinkTexture;
        public static Asset<Texture2D> GreenTexture;
        public static Asset<Texture2D> GlowTexture;
        public static Asset<Texture2D> PinkGlowTexture;
        public static Asset<Texture2D> GreenGlowTexture;

        public ref float Variant => ref NPC.ai[1];

        public ref float Role => ref NPC.ai[2];

        public ref float Aggro => ref NPC.ai[3];

        protected override List<int> PreyIDs => new List<int>()
        {
            ModContent.NPCType<PolypPanasea>(),
            ModContent.NPCType<PrismaticGuppy>(),
            ModContent.NPCType<SeaMinnow>(),
            ModContent.NPCType<SeaMinnowGold>(),
            ModContent.NPCType<AlphaSeaMinnow>(),
            ModContent.NPCType<AlphaSeaMinnowGold>(),
        };

        protected override List<int> PredatorIDs => new List<int>();

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.GleamingBurrows | SunkenSeaBiomeFlags.PolypForest;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            if (!Main.dedServ)
            {
                PinkTexture = ModContent.Request<Texture2D>(Texture + "Pink");
                GreenTexture = ModContent.Request<Texture2D>(Texture + "Green");
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow");
                PinkGlowTexture = ModContent.Request<Texture2D>(Texture + "PinkGlow");
                GreenGlowTexture = ModContent.Request<Texture2D>(Texture + "GreenGlow");
            }
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = Main.hardMode ? 75 : 25;
            NPC.width = 54;
            NPC.height = 76;
            NPC.defense = Main.hardMode ? 10 : 0;
            NPC.lifeMax = Main.hardMode ? 400 : 120;
            NPC.knockBackResist = 0f;
            NPC.alpha = 100;
            NPC.value = Main.hardMode ? Item.buyPrice(0, 0, 5, 0) : Item.buyPrice(0, 0, 1, 0);
            NPC.HitSound = SoundID.NPCHit25;
            NPC.DeathSound = SoundID.NPCDeath28;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<GhostBellBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = false;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };

            // Scale stats in Expert and Master
            CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.GhostBell")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void OnSpawn(IEntitySource source)
        {
            Variant = Main.rand.Next(0, 3);
            pathfinding = new PathfindingManager(NPC)
            {
                Acceleration = 0.1f,
                MaxSpeed = 3f,
            };
            if (Role == 0)
            {
                NPC.TargetClosest();
                int jellyAmt;
                // Large swarm in polyp forest ft babies, otherwise just a few
                if (Main.player[NPC.target].Calamity().ZonePolypForest)
                {
                    jellyAmt = Main.rand.Next(3, 6);
                    // Spawn a bunch of babies too
                    for (int i = 0; i < Main.rand.Next(4, 8); i++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BabyGhostBell>());
                        }
                    }
                }
                else
                {
                    jellyAmt = Main.rand.Next(1, 3);
                }
                // Spawn more Ghost Bells
                // Newly spawned Ghost Bells have their Role set to 1 as to not spawn further Ghost Bells
                for (int i = 0; i < jellyAmt; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, Type, ai2: 1);
                    }
                }
            }
        }

        public override void AI()
        {
            // Lite
            Color lightColor = Color.LightBlue;
            switch (Variant)
            {
                case (int)JellyColor.Pink:
                    lightColor = Color.Pink;
                    break;
                case (int)JellyColor.Green:
                    lightColor = Color.LightGreen;
                    break;
            }
            Lighting.AddLight(NPC.Center, (lightColor.R - NPC.alpha) * 1f / 255f, (lightColor.G - NPC.alpha) * 1f / 255f, (lightColor.B - NPC.alpha) * 1f / 255f);
            NPC.chaseable = Aggro == 1;
            // De-aggro
            if (Aggro == 1)
            {
                if (Main.player[NPC.target] != null)
                {
                    if (Main.player[NPC.target].dead)
                    {
                        Aggro = 0;
                    }
                }
            }
            if (NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                NPC.velocity.Y = -6f;
                NPC.netUpdate = true;
            }
            if (NPC.wet)
            {
                NPC.noGravity = true;
                bool hasTarget = CurrentPrey != null || (Aggro == 1 && Main.player[NPC.target] != null);
                if (hasTarget)
                {
                    Entity target = CurrentPrey != null ? CurrentPrey : Main.player[NPC.target];
                    pathfinding.DoPathfinding(new PathfindingParameters(NPC.Center, target.Center, SunkenSeaTileValidity));
                }
                else
                {
                    if (NPC.localAI[2] > 0f)
                    {
                        NPC.localAI[2] -= 1f;
                    }
                    if (NPC.localAI[2] <= 0f)
                    {
                        if (NPC.velocity.Y == 0f)
                        {
                            NPC.localAI[1] += 1f;
                        }
                        else
                        {
                            NPC.localAI[1] = 0f;
                        }
                        NPC.velocity.Y += 0.1f;
                        if (NPC.velocity.Y > 3f || NPC.localAI[1] >= 6f)
                        {
                            NPC.velocity.Y = -3f;
                        }
                    }
                    NPC.velocity.X *= 0.95f;
                }
            }
            else
            {
                NPC.noGravity = false;
                NPC.velocity.Y = 2f;
                NPC.localAI[2] = 75f;
                NPC.netUpdate = true;
            }

            float SAImovement = 0.05f;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC otherJelly = Main.npc[k];
                // Short circuits to make the loop as fast as possible
                if (!otherJelly.active || k == NPC.whoAmI || (otherJelly.type != ModContent.NPCType<GhostBell>()))
                    continue;

                float taxicabDist = Math.Abs(NPC.position.X - otherJelly.position.X) + Math.Abs(NPC.position.Y - otherJelly.position.Y);
                if (taxicabDist < NPC.width * 2f)
                {
                    if (NPC.position.X < otherJelly.position.X)
                        NPC.velocity.X -= SAImovement;
                    else
                        NPC.velocity.X += SAImovement;

                    if (NPC.position.Y < otherJelly.position.Y)
                        NPC.velocity.Y -= SAImovement;
                    else
                        NPC.velocity.Y += SAImovement;
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.1f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                // Drastically smaller in the Polyp Forest due to a bunch spawning at once
                float multiplier = spawnInfo.Player.Calamity().ZonePolypForest ? 0.1f : 0.9f;
                return SpawnCondition.CaveJellyfish.Chance * multiplier;
            }
            return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Vector2 center = new Vector2(NPC.Center.X, NPC.Center.Y);
            Vector2 halfSizeTexture = new Vector2((float)(TextureAssets.Npc[Type].Value.Width / 2), (float)(TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type] / 2));
            Vector2 vector = center - screenPos;
            vector -= new Vector2((float)GlowTexture.Value.Width, (float)(GlowTexture.Value.Height / Main.npcFrameCount[Type])) / 2f;
            vector += halfSizeTexture * 1f + new Vector2(0f, 4f + NPC.gfxOffY);

            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Texture2D glowTex = GlowTexture.Value;
            Color color = Color.LightBlue;

            switch (Variant)
            {
                    case (int)JellyColor.Pink:
                        tex = PinkTexture.Value;
                        glowTex = PinkGlowTexture.Value;
                        color = Color.LightPink;
                    break;
                    case (int)JellyColor.Green:
                        tex = GreenTexture.Value;
                        glowTex = GreenGlowTexture.Value;
                        color = Color.SeaGreen;
                    break;
            }

            color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(color);

            Main.spriteBatch.Draw(tex, vector, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            Main.spriteBatch.Draw(glowTex, vector, NPC.frame, color, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            return false;
        }

        // Can only hit the target if they're touching the tentacles
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Vector2 npcCenter = NPC.Center;
            Rectangle tentacleHitbox = new Rectangle((int)(npcCenter.X - (NPC.width / 4f)), (int)npcCenter.Y, NPC.width / 2, NPC.height / 2);

            Rectangle targetHitbox = target.Hitbox;
            bool insideTentacleHitbox = targetHitbox.Intersects(tentacleHitbox);

            return insideTentacleHitbox;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<StaticDischarge>(), 120);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.JellyfishNecklace, 25);
            LeadingConditionRule postDS = npcLoot.DefineConditionalDropSet(DropHelper.PostDS());
            postDS.Add(ModContent.ItemType<VoltaicJelly>(), 5);
        }
        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.friendly)
            {
                if (projectile.owner != -1)
                {
                    BecomeHostile(projectile.owner);
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            BecomeHostile(player.whoAmI);
        }

        public void BecomeHostile(int player)
        {
            NPC.target = player;
            Aggro = 1;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.type == Type && n.whoAmI != NPC.whoAmI)
                {
                    n.ai[3] = 1;
                    n.target = player;
                }
            }
        }
    }
}
