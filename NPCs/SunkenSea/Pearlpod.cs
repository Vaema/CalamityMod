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

namespace CalamityMod.NPCs.SunkenSea
{
    public abstract class Pearlpod : ModNPC
    {
        public abstract int PearlType { get; }
        public abstract float SpawnRate { get; }
        public abstract int ItemType { get; }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Snail;
            NPC.damage = 0;
            NPC.width = 24;
            NPC.height = 24;
            NPC.defense = 0;
            NPC.lifeMax = 20;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false; 
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.GravityIgnoresLiquid = true;
            AIType = NPCID.Snail;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<PearlpodBanner>();
            NPC.catchItem = ItemType;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
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
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Pearlpod")
            });
        }

        public override void AI()
        {
            Tile vine = Main.tile[(int)NPC.position.X / 16, (int)NPC.position.Y / 16];
            // Set newAI[0] to 1 if the Pearlpod is inside of a vine
            if (vine.TileType == ModContent.TileType<DepthVines>() && !NPC.justHit)
            {
                NPC.Calamity().newAI[0] = 1;
            }
            // Otherwise reset eating-related variables
            else
            {
                NPC.Calamity().newAI[1] = 0;
                NPC.Calamity().newAI[0] = 0;
            }
            // Eating behavior
            if (NPC.Calamity().newAI[0] == 1)
            {
                NPC.velocity.X *= 0.1f;
                // Play a crunch sound and spawn some grass dust randomly 
                if (Main.rand.NextBool(20))
                {
                    SoundEngine.PlaySound(SoundID.Item2 with { Volume = 0.4f, Pitch = 1.2f }, NPC.Center);
                    for (int i = 0; i < 4; i++)
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1), 40);
                    }
                    NPC.Calamity().newAI[1]++;
                }
                // After munching 10 times, the vine is broken and the Pearlpod continues about its day
                if (NPC.Calamity().newAI[1] == 10)
                {
                    if (vine.TileType == ModContent.TileType<DepthVines>())
                    {
                        WorldGen.KillTile((int)NPC.position.X / 16, (int)NPC.position.Y / 16);
                    }
                }
            }
            if (NPC.type == ModContent.NPCType<PearlpodGold>())
            {
                NPC.ProduceGoldCritterDust();
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > frameHeight * 5)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenBurrows && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * SpawnRate;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(PearlType);
        }
    }

    public class PearlpodWhite : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
        }
        public override int PearlType => ItemID.WhitePearl;
        public override float SpawnRate => 0.6f;
        public override int ItemType => ModContent.ItemType<PearlpodItem>();
    }
    public class PearlpodPink : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[NPC.type] = 6;
        }

        public override int PearlType => ItemID.PinkPearl;
        public override float SpawnRate => 0.2f;
        public override int ItemType => ModContent.ItemType<PearlpodPinkItem>();
    }
    public class PearlpodBlack : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[NPC.type] = 6;
        }
        public override int PearlType => ItemID.BlackPearl;
        public override float SpawnRate => 0.05f;
        public override int ItemType => ModContent.ItemType<PearlpodBlackItem>();
    }
    public class PearlpodGold : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[NPC.type] = 6;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Snail;
            NPC.damage = 0;
            NPC.width = 24;
            NPC.height = 24;
            NPC.defense = 0;
            NPC.lifeMax = 20;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.GravityIgnoresLiquid = true;
            AIType = NPCID.Snail;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<PearlpodBanner>();
            NPC.catchItem = ItemType;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
            NPC.rarity = 3;
        }
        public override int PearlType => ItemID.GoldCoin;
        public override float SpawnRate => 0.0005f;
        public override int ItemType => ModContent.ItemType<PearlpodGoldItem>();
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
