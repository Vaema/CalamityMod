using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PrismBack : SunkenSeaNPC
    {
        public static Asset<Texture2D> GlowTexture;
        public ref float BiteCount => ref NPC.ai[0];
        public ref float ShardCooldown => ref NPC.ai[1];

        /// <summary>
        /// The horizontal coordinate of a located crystal
        /// </summary>
        public ref float TileX => ref NPC.ai[2];

        /// <summary>
        /// The vertical coordinate of a located crystal
        /// </summary>
        public ref float TileY => ref NPC.ai[3];

        public Vector2 tilePosition => new Vector2(TileX, TileY);
        protected override List<int> PreyIDs => [];
        protected override List<int> PredatorIDs => [];
        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs | SunkenSeaBiomeFlags.GleamingBurrows;

        public override void Load() => GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow");

        public override void SetStaticDefaults()
        {
            //Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitPositionXOverride = 0
            };
            value.Position.X += 15;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.damage = 20;
            NPC.width = 88;
            NPC.height = 66;
            NPC.defense = 15;
            NPC.lifeMax = 500;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 2);
            NPC.HitSound = SoundID.NPCHit24;
            NPC.DeathSound = SoundID.NPCDeath27;
            NPC.knockBackResist = 0.15f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<PrismBackBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PrismBack")
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

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(NPC)
                {
                    MaxSpeed = 1.8f
                };
            }

            NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);
            Lighting.AddLight(NPC.Center, (255 - NPC.alpha) * 0f / 255f, (255 - NPC.alpha) * 0.75f / 255f, (255 - NPC.alpha) * 0.75f / 255f);

            if (ShardCooldown > 0)
                ShardCooldown--;

            Tile t = CalamityUtils.ParanoidTileRetrieval((int)(TileX / 16), (int)(TileY / 16));

            // TODO
            // Change this to kelp when it's added
            int tileType = ModContent.TileType<DepthVines>();

            // Assure the kelp still exists, if it's gone, clear the tile
            if ((TileX != 0 || TileY != 0) && t.TileType != tileType)
            {
                TileX = 0;
                TileY = 0;
            }

            if (NPC.wet)
            {
                NPC.noGravity = true;
                Vector2? tilePos = tilePosition;
                // Find kelp
                if (Main.rand.NextBool(300))
                {
                    if (tilePos == null || tilePos == Vector2.Zero)
                    {
                        tilePos = CalamityUtils.NPCTileDetection(NPC, tileType, 300, true);
                    }
                }

                bool eatBehaviour = false;
                // Go to the kelp if one exists nearby
                if (tilePos != null && tilePos != Vector2.Zero)
                {
                    TileX = tilePos.Value.X;
                    TileY = tilePos.Value.Y;
                    t = CalamityUtils.ParanoidTileRetrieval((int)(TileX / 16), (int)(TileY / 16));
                    if (t.TileType == tileType)
                    {
                        eatBehaviour = true;
                        // Go to the vine if not far enough
                        if (tilePos.Value.Distance(NPC.Center) > 40 && BiteCount == 0)
                        {
                            pathfinding.DoPathfinding(new(NPC.Center, tilePos.Value, SunkenSeaTileValidity));
                        }
                        // Slow down and eat the kelp
                        else
                        {
                            pathfinding.ClearResults();
                            NPC.velocity *= 0.97f;
                            // Play a crunch sound and spawn some grass dust randomly 
                            if (Main.rand.NextBool(50))
                            {
                                SoundEngine.PlaySound(SoundID.Item2 with { Volume = 0.4f, Pitch = -0.4f }, NPC.Center);
                                for (int i = 0; i < 4; i++)
                                {
                                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1), 40);
                                }
                                BiteCount++;
                            }
                            // After munching 5 times, the vine is broken and the turtle continues about its day
                            if (BiteCount == 5)
                            {
                                WorldGen.KillTile((int)TileX / 16, (int)TileY / 16);
                                TileX = 0;
                                TileY = 0;
                                BiteCount = 0;
                                NPC.netUpdate = true;
                            }
                        }
                    }
                }
                // Just wander about if it's not trying to eat 
                else if (!eatBehaviour)
                {
                    TileX = 0;
                    TileY = 0;
                    BiteCount = 0;
                    pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(200f, 800f), SunkenSeaTileValidity));
                }
            }
            else
            {
                NPC.noGravity = false;
            }
        }

        public override bool CanBeHitByNPC(NPC attacker) => false;
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return NPC.chaseable;
            }
            return null;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if ((NPC.Center.Y + 10f) > target.Center.Y)
                modifiers.SourceDamage *= CalamityWorld.death ? 3f : CalamityWorld.revenge ? 2.75f : Main.expertMode ? 2.5f : 1.25f;
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) => SpawnShards(player);
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) => SpawnShards(projectile);
        private void SpawnShards(Entity e)
        {
            if (ShardCooldown > 0)
                return;

            ShardCooldown = 20;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 shardVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(1.25f) * -1);
                    Projectile.NewProjectile(NPC.GetSource_OnHurt(e), NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-20f, 20f), shardVel, ModContent.ProjectileType<PrismBackCrystal>(), 10, 0f, Main.myPlayer, Main.rand.Next(3), NPC.whoAmI);
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if ((spawnInfo.Player.Calamity().ZoneRadiantReefs || spawnInfo.Player.Calamity().ZoneGleamingBurrows) && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.7f;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule postDS = npcLoot.DefineConditionalDropSet(DropHelper.PostDS());
            postDS.Add(ModContent.ItemType<PrismShard>(), 1, 1, 3);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismTurtleGore1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismTurtleGore2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismTurtleGore3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismTurtleGore4").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PrismTurtleGore5").Type, 1f);
                }
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        /*public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += (NPC.wet || NPC.IsABestiaryIconDummy) ? 0.1f : 0f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }*/

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Rectangle frame = GlowTexture.Value.Frame(1, Main.npcFrameCount[Type], 0, NPC.frame.Y);
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(Color.Blue);
            Main.spriteBatch.Draw(GlowTexture.Value, NPC.Center - screenPos, null, color, NPC.rotation, frame.Size() / 2f, 1f, spriteEffects, 0f);
        }
    }
}
