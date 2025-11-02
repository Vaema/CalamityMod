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
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using CalamityMod.Enums;
using System.Collections.Generic;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Slugbun : SunkenSeaNPC
    {
        public enum SlugSkin
        {
            Reef = 0,
            Burrows = 1,
            Polyp = 2,
            Radiant = 3
        }
        public ref float CurrentSkin => ref NPC.Calamity().newAI[2];

        public static Asset<Texture2D> burrowsTex;
        public static Asset<Texture2D> polypTex;
        public static Asset<Texture2D> radiantTex;

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs | SunkenSeaBiomeFlags.PolypForest | SunkenSeaBiomeFlags.GleamingBurrows;

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<Probesnout>(),
            ModContent.NPCType<SandProwler>(),
            ModContent.NPCType<SandProwlerNested>(),
            ModContent.NPCType<PrismaticGuppy>(),
            ModContent.NPCType<KelpieSeadragon>(),
        };

        protected override List<int> PreyIDs => new List<int>();

        public override void Load()
        {
            burrowsTex = ModContent.Request<Texture2D>(Texture + "Gleaming");
            polypTex = ModContent.Request<Texture2D>(Texture + "Polyp");
            radiantTex = ModContent.Request<Texture2D>(Texture + "Radiant");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.aiStyle = NPCAIStyleID.Snail;
            NPC.damage = 0;
            NPC.width = 38;
            NPC.height = 28;
            NPC.defense = 0;
            NPC.lifeMax = 20;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.GravityIgnoresLiquid = true;
            NPC.chaseable = false;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<SlugbunBanner>();
            NPC.catchItem = ModContent.ItemType<SlugbunItem>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < 3; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < 3; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Slugbun")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Guppies released by the player do not randomize when spawned
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                CurrentSkin = NPC.ai[1];
                return;
            }
            // Red/Green in reefs, Green/Blue elsewhere
            if (Main.player[NPC.target].Calamity().ZoneGleamingBurrows)
            {
                CurrentSkin = (int)SlugSkin.Burrows;
            }
            else if (Main.player[NPC.target].Calamity().ZoneRadiantReefs)
            {
                CurrentSkin = (int)SlugSkin.Reef;
            }
            else
            {
                CurrentSkin = (int)SlugSkin.Polyp;
            }
            NPC.TargetClosest();
            // 1 in 30 chance for a rare fish variant (rfv)
            if (Main.rand.NextBool(30))
            {
                CurrentSkin = (int)SlugSkin.Radiant;
                NPC.rarity = 3;
            }
            // Decide item..........................
            switch (CurrentSkin)
            {
                case (int)SlugSkin.Polyp:
                    NPC.catchItem = ModContent.ItemType<SlugbunPolypItem>();
                    break;
                case (int)SlugSkin.Burrows:
                    NPC.catchItem = ModContent.ItemType<SlugbunBurrowsItem>();
                    break;
                case (int)SlugSkin.Reef:
                    NPC.catchItem = ModContent.ItemType<SlugbunItem>();
                    break;
                case (int)SlugSkin.Radiant:
                    NPC.catchItem = ModContent.ItemType<SlugbunRadiantItem>();
                    break;
            }
        }

        public override void AI()
        {
            Tile vine = CalamityUtils.ParanoidTileRetrieval((int)NPC.position.X / 16, (int)NPC.position.Y / 16);
            // Set newAI[0] to 1 if the Slug is inside of a vine
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
                // After munching 10 times, the vine is broken and the Slugbun continues about its day
                if (NPC.Calamity().newAI[1] == 10)
                {
                    if (vine.TileType == ModContent.TileType<DepthVines>())
                    {
                        WorldGen.KillTile((int)NPC.position.X / 16, (int)NPC.position.Y / 16);
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > frameHeight * 3)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void ModifyTypeName(ref string typeName)
        {
            if (CurrentSkin == (int)SlugSkin.Radiant)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.RadiantSlugbun");
            }
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneGleamingBurrows && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 1f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenBlood, hit.HitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            switch (CurrentSkin)
            {
                case (int)SlugSkin.Burrows:
                    tex = burrowsTex.Value;
                    break;
                case (int)SlugSkin.Polyp:
                    tex = polypTex.Value;
                    break;
                case (int)SlugSkin.Radiant:
                    tex = radiantTex.Value;
                    break;
            }
            Vector2 pos = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY - (Vector2.UnitY * 6).RotatedBy(NPC.rotation);
            spriteBatch.Draw(tex, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);

            return false;
        }
    }
}
