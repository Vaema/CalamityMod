using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;
using CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs;

namespace CalamityMod.NPCs.SunkenSea
{
    public class AlphaSeaMinnow : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 40;
            NPC.height = 32;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<AlphaSeaMinnowBanner>();
            NPC.chaseable = false;
            NPC.catchItem = (short)ModContent.ItemType<AlphaSeaMinnowItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.AlphaSeaMinnow")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Alphas released by the player do not spawn a shoal of minnows
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
            // Spawn a shoal of minnows
            int fishCount = Main.rand.Next(5, 10);
            for (int i = 0; i < fishCount; i++)
            {
                int goldchance = NPC.type == ModContent.NPCType<AlphaSeaMinnowGold>() ? 20 : 50;
                int minnowtype = Main.rand.NextBool(goldchance) ? ModContent.NPCType<SeaMinnowGold>() : ModContent.NPCType<SeaMinnow>();
                int n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, minnowtype);
                Main.npc[n].ai[2] = NPC.whoAmI; // makes the spawned minnow recognize this one as the alpha
            }
        }

        public override void AI()
        {
            CalamityRegularEnemyAI.PassiveSwimmingAI(NPC, Mod, 2, 150f, 0.25f, 0.15f, 6f, 6f, 0.05f);
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.noGravity = true;
            if (NPC.direction == 0)
            {
                NPC.TargetClosest(true);
            }
            if (NPC.wet)
            {
                NPC.TargetClosest(false);
                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * 0.1f;
                if (NPC.velocity.X < -2.5f || NPC.velocity.X > 2.5f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.95f;
                }
                if (NPC.ai[0] == -1f)
                {
                    NPC.velocity.Y = NPC.velocity.Y - 0.01f;
                    if ((double)NPC.velocity.Y < -0.3)
                    {
                        NPC.ai[0] = 1f;
                    }
                }
                else
                {
                    NPC.velocity.Y = NPC.velocity.Y + 0.01f;
                    if ((double)NPC.velocity.Y > 0.3)
                    {
                        NPC.ai[0] = -1f;
                    }
                }
                int npcTileX = (int)(NPC.position.X + (float)(NPC.width / 2)) / 16;
                int npcTileY = (int)(NPC.position.Y + (float)(NPC.height / 2)) / 16;
                if (Main.tile[npcTileX, npcTileY - 1].LiquidAmount > 128)
                {
                    if (Main.tile[npcTileX, npcTileY + 1].HasTile)
                    {
                        NPC.ai[0] = -1f;
                    }
                    else if (Main.tile[npcTileX, npcTileY + 2].HasTile)
                    {
                        NPC.ai[0] = -1f;
                    }
                }
                if ((double)NPC.velocity.Y > 0.4 || (double)NPC.velocity.Y < -0.4)
                {
                    NPC.velocity.Y = NPC.velocity.Y * 0.95f;
                }
            }
            else
            {
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.94f;
                    if ((double)NPC.velocity.X > -0.2 && (double)NPC.velocity.X < 0.2)
                    {
                        NPC.velocity.X = 0f;
                    }
                }
                NPC.velocity.Y = NPC.velocity.Y + 0.3f;
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
                NPC.ai[0] = 1f;
            }
            NPC.rotation = NPC.velocity.X * 0.05f;
            if ((double)NPC.rotation < -0.1)
            {
                NPC.rotation = -0.1f;
            }
            if ((double)NPC.rotation > 0.1)
            {
                NPC.rotation = 0.1f;
            }
            if (NPC.type == ModContent.NPCType<AlphaSeaMinnowGold>())
            {
                NPC.ProduceGoldCritterDust();
            }
        }

        public override void ModifyTypeName(ref string typeName)
        {
            // Holy mackerel is that an Ultrakill reference!?!?
            // (I have barely played the game as of writing this _ YuH)
            if (Main.zenithWorld)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.MinnowPrime");
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter += 0.075f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 68, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
    public class AlphaSeaMinnowGold : AlphaSeaMinnow
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
            this.HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 40;
            NPC.height = 32;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<AlphaSeaMinnowBanner>();
            NPC.chaseable = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
            NPC.rarity = 3;
            NPC.catchItem = ModContent.ItemType<AlphaSeaMinnowGoldItem>();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.03f;
            }
            return 0f;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
