using CalamityMod.BiomeManagers;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Scavenger : ModNPC
    {
        public static List<(int, int)> itemValues = new List<(int, int)>()
        {
            (ItemID.CopperCoin, 1),
            (ItemID.SilverCoin, 5),
            (ItemID.GoldCoin, 10),
            (ItemID.PlatinumCoin, 20),
            (ItemID.WhitePearl, 5),
            (ItemID.BlackPearl, 10),
            (ItemID.PinkPearl, 20),
        };

        public static List<(int, float)> rewards = new List<(int, float)>();

        public int HeldItemIndex
        {
            get => (int)NPC.ai[1] - 1;
            set => NPC.ai[1] = value + 1;
        }

        public bool AcceptableItem(int type)
        {
            if (type <= 0)
                return false;
            foreach (var v in itemValues)
            {
                if (v.Item1 == type)
                    return true;
            }
            return false;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;

            rewards.Add((ModContent.ItemType<Baroclaw>(), 1 / 10f));
            rewards.Add((ModContent.ItemType<PearlShard>(), 1 / 2f));
            rewards.Add((ModContent.ItemType<HalibutCannon>(), 1 / 20f));
            rewards.Add((ModContent.ItemType<PrismShard>(), 1f));
            rewards.Add((ModContent.ItemType<Navystone>(), 1f));
            rewards.Add((ModContent.ItemType<AmidiasSpark>(), 1 / 5f));
            rewards.Add((ModContent.ItemType<Earth>(), 1 / 100f));
            rewards.Add((ModContent.ItemType<AbandonedWulfrumHelmet>(), 1 / 10f));
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 20;
            NPC.width = 48;
            NPC.height = 46;
            NPC.defense = 5;
            NPC.lifeMax = 350;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.15f;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<ScavengerBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Scavenger")
            });
        }

        public override void AI()
        {
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            if (HeldItemIndex > 0)
            {
                Item targetItem = Main.item[HeldItemIndex];
                if (targetItem == null || !targetItem.active)
                {
                    HeldItemIndex = -1;
                }
            }

            switch (NPC.ai[0])
            {
                case 0:
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                    if (NPC.ai[3] >= 0 && HeldItemIndex <= -1)
                    {
                        bool foundItem = false;
                        foreach (Item i in Main.ActiveItems)
                        {
                            if (foundItem)
                                break;
                            if (!i.active)
                                continue;
                            if (i.beingGrabbed)
                                continue;
                            if (i.Distance(NPC.Center) > 460)
                                continue;

                            if (AcceptableItem(i.type))
                            {
                                HeldItemIndex = i.whoAmI;
                                NPC.ai[0] = 1;
                                break;
                            }
                        }
                    }
                    if (NPC.ai[3] < 0)
                        NPC.ai[3]++;
                    break;
                case 1:
                    {
                        if (HeldItemIndex <= -1)
                        {
                            NPC.ai[0] = 0;
                            return;
                        }
                        Item targetItem = Main.item[HeldItemIndex];
                        if (!AcceptableItem(targetItem.type))
                        {
                            HeldItemIndex = -1;
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[0] = 0;
                            break;
                        }
                        NPC.noGravity = true;
                        NPC.noTileCollide = true;
                        NPC.velocity = NPC.DirectionTo(targetItem.Center) * 10;

                        if (targetItem.Distance(NPC.Center) < 20)
                        {
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[0] = 2;
                            if (targetItem.stack == 1)
                                targetItem.active = false;
                            else
                                targetItem.stack--;
                            NPC.ai[2] = targetItem.type;
                        }
                    }
                    break;
                case 2:
                    {
                        NPC.noGravity = false;
                        NPC.noTileCollide = false;
                        NPC.ai[3]++;
                        if (NPC.ai[3] % 40 == 0 && NPC.ai[3] < (50 * 2))
                        {
                            NPC.velocity.Y = -4;
                        }

                        if (NPC.ai[3] > (50 * 2) + 100)
                        {
                            HeldItemIndex = -1;
                            NPC.ai[0] = 0;
                            NPC.ai[3] = -CalamityUtils.SecondsToFrames(8);
                            int itemResult = CalculateGivenItem();
                            NPC.ai[2] = 0;
                            if (itemResult > 0)
                            {
                                int i = Item.NewItem(NPC.GetSource_FromThis(), NPC.getRect(), itemResult);
                                Main.item[i].velocity = new Vector2(Main.rand.NextFloat(-4f, 4f), -4);
                            }
                        }
                    }
                    break;
            }
        }
        
        public int CalculateGivenItem()
        {
            int type = (int)NPC.ai[2];
            int value = 0;
            foreach (var v in itemValues)
            {
                if (type == v.Item1)
                {
                    value = v.Item2;
                    break;
                }
            }
            if (value == 0)
                return 0;

            int currentItem = 0;
            float currentValue = 0;

            WeightedRandom<int> rewardsRoll = new WeightedRandom<int>();
            foreach (var v in rewards)
            {
                rewardsRoll.Add(v.Item1, v.Item2);
            }
            for (int i = 0; i < value; i++)
            {
                int newItem = rewardsRoll.Get();
                float newValue = 0;
                foreach (var v in rewards)
                {
                    if (v.Item1 == newItem)
                    {
                        newValue = v.Item2;
                        break;
                    }
                }
                if (newValue < currentValue || currentValue == 0)
                {
                    currentValue = newValue;
                    currentItem = newItem;
                }
            }
            return currentItem;
        }



        public override void FindFrame(int frameHeight)
        {
            /*NPC.frameCounter += 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;*/
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);

            return !spawnInfo.Player.Calamity().clamity && tile.WallType == WallID.CrimstoneUnsafe ? 0.05f : 0f;

            //fuck this
            //if (spawnInfo.Player.Calamity().ZoneSunkenSeaShores && !spawnInfo.Player.Calamity().clamity && tile.WallType == ModContent.WallType<RunestoneWall>())
            //{
            //    return 0.05f;
            //}
            //return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, Color.DarkGray * 0.2f, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, Color.DarkGray * 0.2f, 1f);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return true;
        }
    }
}
