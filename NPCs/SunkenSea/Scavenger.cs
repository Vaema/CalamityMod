using CalamityMod.BiomeManagers;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Scavenger : ModNPC
    {
        // Items that can be given to the Scavenger
        public static Dictionary<int, int> currencies = new Dictionary<int, int>();

        // Items that can be received form the Scavenger
        public static Dictionary<int, float> rewards = new Dictionary<int, float>();

        // Ditto the above but in a different format that allows for clean rng rolls
        public static WeightedRandom<int> rewardsRoll = new WeightedRandom<int>();

        // The in world index of the item the crab is going after
        public int HeldItemIndex
        {
            get => (int)NPC.ai[1] - 1;
            set => NPC.ai[1] = value + 1;
        }

        // The type of currency the crab is currently holding
        public int HeldItemType
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        // The crab's current behaviour
        public ref float Phase => ref NPC.ai[0];

        // Used during trading and as a cooldown
        public ref float TradeTimer => ref NPC.ai[3];

        public enum PhaseType
        {
            Idle = 0,
            FoundItem = 1,
            Bartering = 2
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;

            // Fill the currency list
            // The key is the item type, the value is how many rolls the item provides
            currencies.Add((int)ItemID.CopperCoin, 1);
            currencies.Add((int)ItemID.SilverCoin, 5);
            currencies.Add((int)ItemID.GoldCoin, 10);
            currencies.Add((int)ItemID.PlatinumCoin, 20);
            currencies.Add((int)ItemID.WhitePearl, 5);
            currencies.Add((int)ItemID.BlackPearl, 10);
            currencies.Add((int)ItemID.PinkPearl, 20);

            // Fill the rewards list
            // The key is the item type, the value is the item's rarity
            rewards.Add(ModContent.ItemType<Baroclaw>(), 10f);
            rewards.Add(ModContent.ItemType<PearlShard>(), 2f);
            rewards.Add(ModContent.ItemType<HalibutCannon>(), 20f);
            rewards.Add(ModContent.ItemType<PrismShard>(), 1f);
            rewards.Add(ModContent.ItemType<Navystone>(), 1f);
            rewards.Add(ModContent.ItemType<AmidiasSpark>(), 5f);
            rewards.Add(ModContent.ItemType<Earth>(), 100f);
            rewards.Add(ModContent.ItemType<AbandonedWulfrumHelmet>(), 10f);

            // Fill the drop pool
            foreach (var v in rewards)
            {
                // Have the value act as a divisor since WeightedRandom prioritizes higher values
                rewardsRoll.Add(v.Key, 1 / v.Value);
            }
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
            // Check to make sure the target item still exists
            if (HeldItemIndex > 0)
            {
                Item targetItem = Main.item[HeldItemIndex];
                // If the item doesn't exist, reset its held item
                if (targetItem == null || !targetItem.active)
                {
                    HeldItemIndex = -1;
                }
            }

            switch (Phase)
            {
                // Do idle stuff, actual behavior not determined rn
                case (int)PhaseType.Idle:
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                    // If the trade timer is 0 and the crab isn't looking ofr an item, look for an item
                    if (TradeTimer >= 0 && HeldItemIndex <= -1)
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

                            // If a valid item is found, go after it
                            if (currencies.ContainsKey(i.type))
                            {
                                HeldItemIndex = i.whoAmI;
                                Phase = (int)PhaseType.FoundItem;
                                break;
                            }
                        }
                    }
                    // Increment the trade timer back to zero if it is below zero
                    if (TradeTimer < 0)
                        TradeTimer++;
                    break;
                // Go to the item
                case (int)PhaseType.FoundItem:
                    {
                        // If the item is no longer valid, go back to idle behaviour
                        if (HeldItemIndex <= -1)
                        {
                            Phase = (int)PhaseType.Idle;
                            return;
                        }
                        
                        // If the item is suddenly no longer valid for some reason, go back to idle behaviur
                        Item targetItem = Main.item[HeldItemIndex];
                        if (!currencies.ContainsKey(targetItem.type))
                        {
                            HeldItemIndex = -1;
                            NPC.velocity = Vector2.Zero;
                            Phase = (int)PhaseType.Idle;
                            break;
                        }

                        // Movement goes here
                        NPC.noGravity = true;
                        NPC.noTileCollide = true;
                        NPC.velocity = NPC.DirectionTo(targetItem.Center) * 10;

                        // Grab the item if close enough
                        if (targetItem.Distance(NPC.Center) < 20)
                        {
                            NPC.velocity = Vector2.Zero;
                            Phase = (int)PhaseType.Bartering;
                            // If the item's stack is 1, despawn the item. Otherwise decrement its stack by 1.
                            if (targetItem.stack == 1)
                                targetItem.active = false;
                            else
                                targetItem.stack--;
                            // Set the crab's held item type
                            HeldItemType = targetItem.type;
                        }
                    }
                    break;
                // Ponder the held item then give a reward back
                case (int)PhaseType.Bartering:
                    {
                        NPC.noGravity = false;
                        NPC.noTileCollide = false;
                        TradeTimer++;
                        // Do little hops (placeholder?)
                        if (TradeTimer % 40 == 0 && TradeTimer < (50 * 2))
                        {
                            NPC.velocity.Y = -4;
                        }

                        // After some time, spit out a reward and go back to idle with a cooldown
                        if (TradeTimer > (50 * 2) + 100)
                        {
                            HeldItemIndex = -1;
                            Phase = (int)PhaseType.Idle;
                            // This timer increments during its idle phase so it's set to a negative value
                            // Once the value hits zero, the crab will be able to trade again
                            TradeTimer = -CalamityUtils.SecondsToFrames(8);

                            // Calculate the reward
                            int itemResult = CalculateReward();
                            HeldItemType = 0;
                            // Spawn the reward
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
        
        public int CalculateReward()
        {
            // Make sure the held item type is valid
            if (currencies.TryGetValue(HeldItemType, out int value))
            {
                // If the held item has no roll value, immediately return
                if (value == 0)
                    return 0;

                // The reward's item type
                int currentItem = 0;
                // The rarity value of the reward
                float currentValue = 0;

                // Roll based on the held item's roll value
                for (int i = 0; i < value; i++)
                {
                    // Grab an item from the pool and its value
                    int newItem = rewardsRoll.Get();
                    float newValue = rewards[newItem];
                    // If the rarity of the reward is larger than the current reward, have it take priority
                    // This also applies if currentValue is at its default
                    if (newValue > currentValue || currentValue == 0)
                    {
                        currentValue = newValue;
                        currentItem = newItem;
                    }
                }
                return currentItem;
            }
            else
            {
                return 0;
            }
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
