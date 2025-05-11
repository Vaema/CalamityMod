using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class GhostBell : ModNPC
    {
        public bool hasBeenHit = false;

        public static Asset<Texture2D> PinkTexture;
        public static Asset<Texture2D> GreenTexture;

        public Color TentacleColor = new Color(58, 49, 89);

        public ref float Phase => ref NPC.ai[0];

        public ref float Variant => ref NPC.ai[1];

        public enum JellyColor
        {
            Blue = 0,
            Green = 1,
            Pink = 2
        }

        /// <summary>
        /// The squish of this NPC while drawing.
        /// </summary>
        public Vector2 ScaleSquish;

        public List<List<VerletSimulatedSegment>> tentacles = new List<List<VerletSimulatedSegment>>();

        public override void Load()
        {
            GreenTexture = ModContent.Request<Texture2D>(Texture + "Green");
            PinkTexture = ModContent.Request<Texture2D>(Texture + "Pink");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = Main.hardMode ? 75 : 25;
            NPC.width = 74;
            NPC.height = 58;
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
            writer.Write(hasBeenHit);
            writer.WriteVector2(ScaleSquish);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
            hasBeenHit = reader.ReadBoolean();
            ScaleSquish = reader.ReadVector2();
        }

        public override void OnSpawn(IEntitySource source)
        {
            Variant = Main.rand.Next(0, 3);
        }

        public override void AI()
        {
            CreateTentacles();

            NPC.ai[2]++;
            Lighting.AddLight(NPC.Center, 0f, (255 - NPC.alpha) * 1.5f / 255f, (255 - NPC.alpha) * 1.5f / 255f);
            if (NPC.justHit)
            {
                hasBeenHit = true;
            }
            NPC.chaseable = hasBeenHit;
            if (NPC.wet)
            {
                NPC.noGravity = true;
            }
            else
            {
                NPC.noGravity = false;
            }

            NPC.netUpdate = true;
            NPC.netSpam = 0;
        }

        public void CreateTentacles()
        {
            if (tentacles == null || tentacles.Count < 6)
            {
                int segmentCount = Main.rand.Next(8, 13);
                List<VerletSimulatedSegment> segments = new List<VerletSimulatedSegment>(segmentCount);
                for (int i = 0; i < segmentCount; i++)
                {
                    VerletSimulatedSegment segment = new VerletSimulatedSegment(NPC.Center - Vector2.UnitY * i * 5);
                    segments.Add(segment);
                }
                segments[0].locked = true;
                tentacles.Add(segments);
            }        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.9f;
            }
            return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Color is based on parent color
            Color col = Variant switch
            {
                (int)JellyColor.Green => Color.MintCream,
                (int)JellyColor.Pink => Color.Pink,
                _ => Color.Cyan
            };

            Vector2 drawOffset = Vector2.Zero; // An added offset that gives the jellyfish a visual bobbing movement
            int fullTime = 180; // How long the bob animation lasts
            int localTimer = (int)(NPC.ai[2] % fullTime);
            float goUp = (int)(fullTime * 0.4f); // When should the jellyfish jet upwards
            int height = 30; // The vertical range the jellyfish moves
            Vector2 squash = new Vector2(1.1f, 0.9f);
            Vector2 stretch = new Vector2(0.8f, 1.2f);
            Vector2 finalScale;

            if (localTimer < goUp)
            {
                drawOffset.Y += MathHelper.Lerp(height, 0, CalamityUtils.CircOutEasing(Utils.GetLerpValue(0, goUp - 1, localTimer, true), 1));
                finalScale = Vector2.Lerp(stretch, squash, CalamityUtils.CircOutEasing(Utils.GetLerpValue(0, goUp - 1, localTimer, true), 1));
            }
            else
            {
                drawOffset.Y += MathHelper.Lerp(0, height, CalamityUtils.SineInEasing(Utils.GetLerpValue(goUp, fullTime - 1, localTimer, true), 1));
                finalScale = Vector2.Lerp(squash, stretch, CalamityUtils.SineInEasing(Utils.GetLerpValue(goUp, fullTime - 1, localTimer, true), 1));
            }
            // Keeps the hitbox centered
            drawOffset.Y -= height / 2;

            CreateTentacles();

            // Update the chains
            for (int i = 0; i < tentacles.Count; i++)
            {
                List<VerletSimulatedSegment> segments = tentacles[i];

                segments[0].oldPosition = segments[0].position;
                segments[0].position = NPC.Center + new Vector2(MathHelper.Lerp(-20, 20, (i + 1) / (float)tentacles.Count), 10) + drawOffset;

                tentacles[i] = VerletSimulatedSegment.SimpleSimulation(segments, 5, loops: 1, gravity: 0.6f);
            }

            // Draw tentacle chains
            for (int t = 0; t < tentacles.Count; t++)
            {
                List<VerletSimulatedSegment> segments = tentacles[t];
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    VerletSimulatedSegment seg = segments[i];
                    float dist = i > 0 ? Vector2.Distance(seg.position, segments[i - 1].position) : 0;
                    if (dist <= 2)
                        dist = 2;
                    dist += 4;
                    if (i == segments.Count - 1)
                    {
                        dist = Vector2.Distance(seg.position, NPC.Center) + 2;
                    }
                    float rot = 0f;
                    if (i > 0)
                        rot = seg.position.DirectionTo(segments[i - 1].position).ToRotation();
                    Color finalColor = TentacleColor;
                    if (i > segments.Count - 4)
                    {
                        finalColor = Color.Lerp(finalColor, col, Utils.GetLerpValue(segments.Count - 4, segments.Count, i, true));
                    }
                    SpriteEffects dir = NPC.Center.X > NPC.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, seg.position - screenPos, new Rectangle(0, 0, 4, 8), finalColor, rot + MathHelper.PiOver2, new Vector2(4, 4), new Vector2(1, dist / 8), dir, 0);
                }
            }

            Texture2D tex = Variant switch
            {
                (int)JellyColor.Pink => PinkTexture.Value,
                (int)JellyColor.Green => GreenTexture.Value,
                _ => TextureAssets.Npc[Type].Value
            };

            Main.EntitySpriteDraw(tex, NPC.Center - screenPos + drawOffset, null, Color.White, NPC.rotation, tex.Size() / 2, finalScale * NPC.scale, 0);
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
    }
}
