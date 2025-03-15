using CalamityMod.BiomeManagers;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using rail;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;
using CalamityMod.Graphics.Metaballs;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Searslug : ModNPC
    {
        public bool Skinwalker => NPC.Calamity()?.newAI[0] == 1;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 7;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Snail;
            AIType = NPCID.Snail;
            NPC.damage = 0;
            NPC.width = 56;
            NPC.height = 42;
            NPC.defense = 0;
            NPC.lifeMax = 20;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 10, 0);
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.GravityIgnoresLiquid = true;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<SearslugBanner>();
            NPC.catchItem = ModContent.ItemType<SearslugItem>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToWater = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < 2; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < 2; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Searslug")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 1 in 10 chance to be a skinwalker
            NPC.Calamity().newAI[0] = Main.rand.NextBool(10).ToInt();
        }

        public override void AI()
        {
            NPC.Calamity().newAI[1]++;
            NPC.TargetClosest(false);
            Lighting.AddLight(NPC.Center, 1, 0, 0);
            if (Skinwalker)
            {
                // Randomly stutter
                if (Main.rand.NextBool(300) && NPC.velocity.Y == 0)
                    NPC.velocity.X *= 0.7f;

                // If the player gets close enough, burst out and KILL the host
                if (NPC.HasPlayerTarget && NPC.Calamity().newAI[1] > 120)
                {
                    Player p = Main.player[NPC.target];
                    if (p != null && p.active && p.Distance(NPC.Center) < 120 && Collision.CanHitLine(NPC.Center, 1, 1, p.Center, 1, 1))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.8f, Volume = 0.6f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = 0.4f }, NPC.Center);
                        for (int i = 0; i < 10; i++)
                        {
                            int size = 12;
                            Vector2 position = NPC.Center;
                            Vector2 velocity = Main.rand.NextVector2Circular(size, size);
                            SquishyLightParticle energy = new(position, velocity, Main.rand.NextFloat(0.2f, 0.3f), Color.Orange, Main.rand.Next(5, 8), 1, 1.5f);
                            GeneralParticleHandler.SpawnParticle(energy);
                            Dust dust = Dust.NewDustPerfect(position, DustID.Torch, velocity, 0, default, Main.rand.NextFloat(1f, 1.6f));
                            dust.noGravity = true;
                        }
                        NPC.HitEffect();
                        NPC.active = false;
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            int frameGate = Skinwalker ? 8 : 6;
            if (NPC.frameCounter > frameGate)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > frameHeight * (Main.npcFrameCount[Type] - 1))
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneBasaltGully && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.Cavern.Chance * 0.2f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LavaMoss, hit.HitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 pos = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY - (Vector2.UnitY * 10).RotatedBy(NPC.rotation);
            // Shake if a skinwalker
            if (Skinwalker)
            {
                pos += Main.rand.NextVector2Unit() * Main.rand.NextFloat(-1f, 1f);
            }
            spriteBatch.Draw(tex, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);
            return false;
        }

        public override bool? CanBeCaughtBy(Item item, Player player)
        {
            return ItemID.Sets.LavaproofCatchingTool[item.type];
        }

        public override void OnCaughtBy(Player player, Item item, bool failed)
        {
            // Wear protection!
            if (failed)
            {
                player.AddBuff(BuffID.OnFire3, CalamityUtils.SecondsToFrames(5));
            }
        }
    }
}
