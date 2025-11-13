using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Fishing.SunkenSeaCatches;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Items.Placeables.FurnitureDriftwood;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Utilities;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Potions.Alcohol;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Scavenger : SunkenSeaNPC
    {
        public record ScavengerItem(int itemID, int minimum, int maximum, Func<bool> condition);

        public static Asset<Texture2D> walkTexture;

        public static Asset<Texture2D> inspectTexture;

        public static Asset<Texture2D> giveTexture;

        public static Dictionary<int, WeightedRandom<ScavengerItem>> ScavengerLoot = new Dictionary<int, WeightedRandom<ScavengerItem>>()
        {
            { ItemID.WhitePearl, new WeightedRandom<ScavengerItem>() },
            { ItemID.BlackPearl, new WeightedRandom<ScavengerItem>() },
            { ItemID.PinkPearl, new WeightedRandom<ScavengerItem>() },
            { ModContent.ItemType<PearlpodItem>(), new WeightedRandom<ScavengerItem>() },
            { ModContent.ItemType<PearlpodBlackItem>(), new WeightedRandom<ScavengerItem>() },
            { ModContent.ItemType<PearlpodPinkItem>(), new WeightedRandom<ScavengerItem>() },
            { ItemID.GalaxyPearl, new WeightedRandom<ScavengerItem>() },
            { ModContent.ItemType<GiantPearl>(), new WeightedRandom<ScavengerItem>() },
        };

        public enum PhaseType
        {
            Idle = 0,
            FoundItem = 1,
            Bartering = 2,
            Burrow = 3
        }

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

        public ref float WalkTimer => ref NPC.Calamity().newAI[0];

        public ref float TurnTimer => ref NPC.Calamity().newAI[1];

        public ref float WalkOrStand => ref NPC.Calamity().newAI[2];

        public ref float StackMin => ref NPC.localAI[0];

        public ref float StackMax => ref NPC.localAI[1];

        public ref float OldItemType => ref NPC.localAI[2];

        public bool ShouldUseWalkingFrames => NPC.velocity.X != 0 && ((WalkOrStand == 1 && Phase == (int)PhaseType.Idle) || (Phase == (int)PhaseType.FoundItem));

        public bool ShouldUseInspectionFrames => Phase == (int)PhaseType.Bartering && TradeTimer < 80;

        public bool ShouldUseGivingFrames => Phase == (int)PhaseType.Bartering && TradeTimer >= 80;

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.TimelessShores;

        protected override List<int> PreyIDs => new List<int>
        {

        };

        protected override List<int> PredatorIDs => new List<int>
        {
            ModContent.NPCType<Stormlion>(),
            ModContent.NPCType<StormlionSentry>()
        };

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            Main.npcFrameCount[Type] = 7;

            InitializeScavengerLoot();

            if (!Main.dedServ)
            {
                walkTexture = ModContent.Request<Texture2D>(Texture + "Walking");
                inspectTexture = ModContent.Request<Texture2D>(Texture + "Inspecting");
                giveTexture = ModContent.Request<Texture2D>(Texture + "Giving");
            }
        }

        #region Trades

        public static void InitializeScavengerLoot()
        {
            int white = ItemID.WhitePearl;
            int black = ItemID.BlackPearl;
            int pink = ItemID.PinkPearl;
            int minBlock = 6;
            int maxBlock = 17;
            float coralBlockChance = 0.1f;

            AddScavengerItem(white, ModContent.ItemType<Driftwood>(), minBlock, maxBlock);
            AddScavengerItem(white, ModContent.ItemType<Navystone>(), minBlock, maxBlock);
            AddScavengerItem(white, ModContent.ItemType<EutrophicSand>(), minBlock, maxBlock);
            AddScavengerItem(white, ModContent.ItemType<MagentaCoral>(), minBlock, maxBlock, coralBlockChance);
            AddScavengerItem(white, ModContent.ItemType<CyanCoral>(), minBlock, maxBlock, coralBlockChance);
            AddScavengerItem(white, ModContent.ItemType<LimeCoral>(), minBlock, maxBlock, coralBlockChance);
            AddScavengerItem(white, ModContent.ItemType<YellowCoral>(), minBlock, maxBlock, coralBlockChance);
            AddScavengerItem(white, ModContent.ItemType<OrangeCoral>(), minBlock, maxBlock, coralBlockChance);
            AddScavengerItem(white, ItemID.WoodenCrate, 1, () => !Main.hardMode, 0.1f);
            AddScavengerItem(white, ItemID.WoodenCrateHard, 1, () => Main.hardMode, 0.1f);
            AddScavengerItem(white, ModContent.ItemType<StormlionMandible>(), 1, 0.1f);
            AddScavengerItem(white, ModContent.ItemType<PrismShard>(), 1, 0.1f);
            AddScavengerItem(white, ItemID.ScarabBomb, 3, 10, 0.1f);
            AddScavengerItem(white, ItemID.Coral, 3, 10, 0.2f);
            AddScavengerItem(white, ItemID.Starfish, 3, 10, 0.2f);
            AddScavengerItem(white, ItemID.Seashell, 3, 10, 0.2f);
            AddScavengerItem(white, ModContent.ItemType<VictideCoralTurban>(), 1, 0.05f);
            AddScavengerItem(white, ModContent.ItemType<VictideShellmet>(), 1, 0.05f);
            // Critters. Golds, Radiants, Pearlpods and any Basalt Gully/Timeless Shore critters are to be excluded
            AddScavengerItem(white, ModContent.ItemType<PrismaticGuppyPinkItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<PrismaticGuppyGreenItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<PrismaticGuppyBlueItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<SeaMinnowItem>(), 1, 0.05f);
            AddScavengerItem(white, ModContent.ItemType<AlphaSeaMinnowItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<PolypPanaseaItem>(), 1, 0.01f);
            AddScavengerItem(white, ModContent.ItemType<PolypPanaseaGreenItem>(), 1, 0.01f);
            AddScavengerItem(white, ModContent.ItemType<PolypPanaseaPurpleItem>(), 1, 0.01f);
            AddScavengerItem(white, ModContent.ItemType<PolypPanaseaTurquoiseItem>(), 1, 0.01f);
            AddScavengerItem(white, ModContent.ItemType<BabyGhostBellItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<BabyGhostBellPinkItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<BabyGhostBellGreenItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<SlugbunItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<SlugbunBurrowsItem>(), 1, 0.02f);
            AddScavengerItem(white, ModContent.ItemType<SlugbunPolypItem>(), 1, 0.02f);

            AddScavengerItem(black, ModContent.ItemType<EutrophicCrate>(), 1, () => !Main.hardMode, 0.1f);
            AddScavengerItem(black, ModContent.ItemType<PrismCrate>(), 1, () => Main.hardMode, 0.1f);
            AddScavengerItem(black, ItemID.PirateMap, 1, () => Main.hardMode, 0.05f);
            AddScavengerItem(black, ItemID.IronCrate, 1, () => !Main.hardMode, 0.1f);
            AddScavengerItem(black, ItemID.IronCrateHard, 1, () => Main.hardMode, 0.1f);
            AddScavengerItem(black, ItemID.GoldenCrate, 1, () => !Main.hardMode, 0.02f);
            AddScavengerItem(black, ItemID.GoldenCrateHard, 1, () => Main.hardMode, 0.02f);
            AddScavengerItem(black, ItemID.WaterWalkingBoots, 1, 0.05f);
            AddScavengerItem(black, ItemID.JellyfishNecklace, 1, 0.05f);
            AddScavengerItem(white, ModContent.ItemType<SerpentsBite>(), 1, 0.05f);
            //AddScavengerItem(black, ItemID.Nachos, 1, 0.05f); insert food item
            AddScavengerItem(black, ModContent.ItemType<SeaRemains>(), 1, () => DownedBossSystem.downedDesertScourge, 0.3f);
            AddScavengerItem(black, ModContent.ItemType<SeaPrism>(), 1, () => DownedBossSystem.downedDesertScourge, 0.3f);
            AddScavengerItem(black, ModContent.ItemType<SuspiciousScrap>(), 1, () => Main.hardMode, 0.1f);
            AddScavengerItem(black, ModContent.ItemType<InkBomb>(), 1, () => Main.hardMode, 0.1f);
            AddScavengerItem(black, ModContent.ItemType<SeaSpiritAmulet>(), 1, () => Main.hardMode, 0.1f);
            AddScavengerItem(black, ItemID.FinWings, 1, () => Main.hardMode, 0.1f);

            AddScavengerItem(pink, ModContent.ItemType<BurntSienna>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<BurntBow>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<BurntBook>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<BurntStaff>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<BurntDagger>(), 1);
            AddScavengerItem(pink, ModContent.ItemType<EnchantedPearl>(), 1);
            AddScavengerItem(pink, ModContent.ItemType<SpiritGlyph>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<ScavengerHelmet>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<ScavengerChestplate>(), 1);
            //AddScavengerItem(pink, ModContent.ItemType<ScavengerBoots>(), 1);
            AddScavengerItem(white, ModContent.ItemType<DeepDiver>(), 1, () => Main.hardMode);
            AddScavengerItem(white, ModContent.ItemType<Poseidon>(), 1, () => Main.hardMode);

            // Pearlpods can be used as a substitute for pearls
            ScavengerLoot[ModContent.ItemType<PearlpodItem>()] = ScavengerLoot[white];
            ScavengerLoot[ModContent.ItemType<PearlpodBlackItem>()] = ScavengerLoot[black];
            ScavengerLoot[ModContent.ItemType<PearlpodPinkItem>()] = ScavengerLoot[pink];

            ScavengerLoot[ItemID.GalaxyPearl] = ScavengerLoot[white]; // Gives 1-2x the amount of white items
            ScavengerLoot[ModContent.ItemType<GiantPearl>()] = ScavengerLoot[pink]; // Gives 2 pink items

        }

        #region Loot adding methods
        /// <summary>
        /// Adds an item to the Scavenger's loot pool
        /// </summary>
        /// <param name="currency">The item type required to be traded</param>
        /// <param name="itemID">The loot item</param>
        /// <param name="min">The minimum amount to drop</param>
        /// <param name="max">The maximum amount to drop</param>
        /// <param name="weight">The weight of the item drop. Lower numbers means lower chances</param>
        public static void AddScavengerItem(int currency, int itemID, int min, int max, float weight = 1)
        {
            AddScavengerItem(currency, itemID, min, max, () => true, weight);
        }

        /// <summary>
        /// Adds an item to the Scavenger's loot pool
        /// </summary>
        /// <param name="currency">The item type required to be traded</param>
        /// <param name="itemID">The loot item</param>
        /// <param name="amount">How much should drop</param>
        /// <param name="weight">The weight of the item drop. Lower numbers means lower chances</param>
        public static void AddScavengerItem(int currency, int itemID, int amount, float weight = 1)
        {
            AddScavengerItem(currency, itemID, amount, amount, () => true, weight);
        }

        /// <summary>
        /// Adds an item to the Scavenger's loot pool
        /// </summary>
        /// <param name="currency">The item type required to be traded</param>
        /// <param name="itemID">The loot item</param>
        /// <param name="amount">How much should drop</param>
        /// <param name="condition">A condition</param>
        /// <param name="weight">The weight of the item drop. Lower numbers means lower chances</param>
        public static void AddScavengerItem(int currency, int itemID, int amount, Func<bool> condition, float weight = 1)
        {
            AddScavengerItem(currency, itemID, amount, amount, condition, weight);
        }

        /// <summary>
        /// Adds an item to the Scavenger's loot pool
        /// </summary>
        /// <param name="currency">The item type required to be traded</param>
        /// <param name="itemID">The loot item</param>
        /// <param name="min">The minimum amount to drop</param>
        /// <param name="max">The maximum amount to drop</param>
        /// <param name="condition">A condition</param>
        /// <param name="weight">The weight of the item drop. Lower numbers means lower chances</param>
        public static void AddScavengerItem(int currency, int itemID, int min, int max, Func<bool> condition, float weight = 1)
        {
            if (ScavengerLoot.ContainsKey(currency))
            {
                ScavengerLoot[currency].Add(new ScavengerItem(itemID, min, max, condition), weight);
            }
            else
            {
                ScavengerLoot.Add(currency, new WeightedRandom<ScavengerItem>());
                ScavengerLoot[currency].Add(new ScavengerItem(itemID, min, max, condition), weight);
            }
        }
        #endregion
        #endregion

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = 20;
            NPC.width = 48;
            NPC.height = 48;
            NPC.defense = 5;
            NPC.lifeMax = 350;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 5);
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.15f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ScavengerBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Scavenger")
            });
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.Calamity().newAI[0] = reader.ReadSingle();
            NPC.Calamity().newAI[1] = reader.ReadSingle();
            NPC.Calamity().newAI[2] = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.Calamity().newAI[0]);
            writer.Write(NPC.Calamity().newAI[1]);
            writer.Write(NPC.Calamity().newAI[2]);
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

            // Initialize its direction
            if (NPC.direction == 0)
                NPC.direction = Main.rand.NextBool() ? -1 : 1;

            switch (Phase)
            {
                // Do idle stuff, actual behavior not determined rn
                case (int)PhaseType.Idle:
                    {
                        // Decide if it should walk or sit
                        float movementSpeed = CurrentPrey != null || CurrentPredator != null ? 2 : 1;
                        bool startMovement = false;
                        if (WalkTimer <= 0)
                        {
                            WalkTimer = Main.rand.Next(180, 340);
                            WalkOrStand = WalkOrStand <= 0 ? 1 : -1;
                            if (WalkOrStand == 1 && Main.rand.NextBool() && CurrentPrey == null)
                            {
                                NPC.direction *= -1;
                            }
                            if (CurrentPrey != null)
                            {
                                NPC.direction = (int)(CurrentPrey.Center.X - NPC.Center.X);
                            }
                            startMovement = true;
                        }
                        bool lookingAtPredator = CurrentPredator != null && NPC.direction == Math.Sign(CurrentPredator.Center.X - NPC.Center.X);
                        bool waterCheck = WaterCheck(16);
                        // If it bumps into something, jump or turn around
                        if ((!startMovement && TurnTimer <= 0 && (NPC.velocity.X == 0 || waterCheck) && WalkOrStand == 1) || lookingAtPredator)
                        {
                            // Jump if there are a couple tiles and a space above
                            if (!waterCheck && JumpCheck() && NPC.velocity.Y == 0 && !lookingAtPredator)
                            {
                                NPC.velocity.Y -= 7;
                                NPC.velocity.X = movementSpeed * NPC.direction;
                                WalkOrStand = 1;
                                TurnTimer = 10;
                            }
                            // Turn around if it can't jump up or a pit/water is in the way
                            else
                            {
                                NPC.direction *= -1;
                                TurnTimer = 30;
                            }
                        }
                        // Move
                        if (!NPC.justHit)
                        {
                            if (WalkOrStand == 1)
                                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * movementSpeed, 0.05f);
                            else if (NPC.velocity.Y == 0)
                                NPC.velocity.X *= 0.95f;
                        }
                        else if (WalkOrStand == -1)
                        {
                            WalkTimer = 0;
                            if (CurrentPredator != null)
                            {
                                WalkOrStand = 1;
                                WalkTimer = Main.rand.Next(180, 340);
                            }
                        }
                        CalamityUtils.StepUpBlocks(NPC);

                        // If the trade timer is 0 and the crab isn't looking ofr an item, look for an item
                        if (TradeTimer >= 0 && HeldItemIndex <= -1 && CurrentPredator == null)
                        {
                            float curDist = 0;
                            bool superRare = false;
                            foreach (Item i in Main.ActiveItems)
                            {
                                if (!i.active)
                                    continue;
                                if (i.beingGrabbed)
                                    continue;
                                float distance = i.Distance(NPC.Center);
                                if (distance > 460)
                                    continue;
                                if (!NPC.HasSight(i.Center))
                                    continue;
                                if (Math.Abs(NPC.Center.Y - i.Center.Y) > 100)
                                    continue;

                                // Check if the item is a valid currency and is the closest possible currency
                                if (ScavengerLoot.ContainsKey(i.type))
                                {
                                    if (distance < curDist || curDist == 0)
                                    {
                                        HeldItemIndex = i.whoAmI;
                                        superRare = i.type > ItemID.PinkPearl ? true : false;
                                    }
                                    curDist = distance;
                                }
                            }
                            // If an item was found, go after it
                            if (curDist != 0)
                            {
                                NPC.netUpdate = true;
                                Phase = (int)PhaseType.FoundItem;
                                TurnTimer = 0;
                                WalkTimer = 0;
                                if (NPC.velocity.Y == 0)
                                    NPC.velocity.Y = -5;
                                NPC.direction = NPC.DirectionTo(Main.item[HeldItemIndex].Center).X.DirectionalSign();

                                SoundEngine.PlaySound(SoundID.NPCHit51 with { Pitch = -0.4f }, NPC.Center);

                                EmoteExpressionParticle.EmoteType eType = superRare ? EmoteExpressionParticle.EmoteType.DoubleExclamation : EmoteExpressionParticle.EmoteType.Exclamation;

                                var emoteDirection = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 3f);
                                Particle emote = new EmoteExpressionParticle(
                                    NPC.Center + emoteDirection * 2f,
                                    emoteDirection,
                                    2.2f,
                                    Color.YellowGreen,
                                    Main.rand.Next(30, 46),
                                    eType);
                                GeneralParticleHandler.SpawnParticle(emote);
                            }
                        }
                        // Increment the trade timer back to zero if it is below zero
                        if (TradeTimer < 0)
                            TradeTimer++;
                    }
                    break;
                // Go to the item
                case (int)PhaseType.FoundItem:
                    {
                        // If the item is no longer valid, go back to idle behaviour
                        if (HeldItemIndex <= -1 || CurrentPredator != null)
                        {
                            NPC.netUpdate = true;
                            Phase = (int)PhaseType.Idle;
                            HeldItemIndex = -1;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            return;
                        }
                        
                        // If the item is suddenly no longer valid for some reason, go back to idle behaviour
                        Item targetItem = Main.item[HeldItemIndex];
                        if (!targetItem.active || !ScavengerLoot.ContainsKey(targetItem.type))
                        {
                            NPC.netUpdate = true;
                            targetItem.noGrabDelay = 0;
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetItem.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                            }
                            HeldItemIndex = -1;
                            NPC.velocity = Vector2.Zero;
                            Phase = (int)PhaseType.Idle;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            break;
                        }

                        // Once the jump anim is done, go to the pearl
                        if (WalkTimer > 0)
                        {
                            float movementSpeed = 3;
                            bool waterCheck = WaterCheck(16);
                            // If it bumps into something, jump or turn around
                            if (TurnTimer <= 0 && (NPC.velocity.X == 0 || waterCheck) && NPC.velocity.Y == 0)
                            {
                                // Jump if there are a couple tiles and a space above
                                if (!waterCheck && JumpCheck())
                                {
                                    NPC.velocity.Y -= 8;
                                    NPC.velocity.X = movementSpeed * NPC.direction;
                                    TurnTimer = 10;
                                }
                                // Turn around if it can't jump up or a pit/water is in the way
                                else
                                {
                                    NPC.direction *= -1;
                                    TurnTimer = 30;
                                    NPC.netUpdate = true;
                                    targetItem.noGrabDelay = 0;
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                    {
                                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetItem.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                                    }
                                    HeldItemIndex = -1;
                                    Phase = (int)PhaseType.Idle;
                                    TurnTimer = 0;
                                    WalkTimer = 0;
                                }
                            }
                            // Change direction if the item position changed
                            bool itemOnRight = targetItem.Center.X > NPC.Center.X;
                            if (itemOnRight && NPC.direction == -1)
                            {
                                NPC.direction *= -1;
                            }
                            else if (!itemOnRight && NPC.direction == 0)
                            {
                                NPC.direction *= -1;
                            }
                            TurnTimer--;
                            
                            NPC.velocity.X = NPC.direction * movementSpeed;
                            CalamityUtils.StepUpBlocks(NPC);
                        }
                        // While the jump animation is occurring, slow down horizontally
                        else
                        {
                            NPC.velocity.X *= 0.95f;
                        }
                        // Marks if the jump animation is finished
                        if (NPC.velocity.Y == 0)
                        {
                            WalkTimer++;
                        }

                        // Pull the item if close enough
                        if (targetItem.Distance(NPC.Center) < 120)
                        {
                            targetItem.noGrabDelay = 100;
                            targetItem.velocity = targetItem.DirectionTo(NPC.Center) * 5;
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetItem.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                            }
                        }
                        // Grab the item if close enough
                        if (targetItem.Distance(NPC.Center) < 10)
                        {
                            Phase = (int)PhaseType.Bartering;
                            // If the item's stack is 1, despawn the item. Otherwise decrement its stack by 1.
                            if (targetItem.stack == 1)
                                targetItem.active = false;
                            else
                                targetItem.stack--;
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetItem.whoAmI, 1, 0f, 0f, 0, 0, 0);
                            }
                            // Set the crab's held item type
                            HeldItemType = targetItem.type;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            NPC.netUpdate = true;
                        }
                    }
                    break;
                // Ponder the held item then give a reward back
                case (int)PhaseType.Bartering:
                    {
                        if (CurrentPredator != null)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Item.NewItem(NPC.GetSource_FromThis(), NPC.getRect(), HeldItemType);
                            }

                            NPC.netUpdate = true;
                            HeldItemIndex = -1;
                            Phase = (int)PhaseType.Idle;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            OldItemType = 0;
                            StackMin = 0;
                            StackMax = 0;

                            SoundEngine.PlaySound(SoundID.NPCHit51 with { Pitch = -0.3f }, NPC.Center);

                            EmoteExpressionParticle.EmoteType eType = EmoteExpressionParticle.EmoteType.Exclamation;

                            var emoteDirection = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 3f);
                            Particle emote = new EmoteExpressionParticle(
                                NPC.Center + emoteDirection * 2f,
                                emoteDirection,
                                2.2f,
                                Color.Red,
                                Main.rand.Next(30, 46),
                                eType);
                            GeneralParticleHandler.SpawnParticle(emote);
                            return;
                        }
                        NPC.velocity.X *= NPC.velocity.Y != 0 ? 0.96f : 0.9f;
                        TradeTimer++;
                        // Calculate the reward
                        if (TradeTimer == 132)
                        {
                            NPC.netUpdate = true;
                            OldItemType = HeldItemType;
                            HeldItemType = CalculateReward(out int min, out int max);
                            StackMin = min;
                            StackMax = max;
                        }

                        // After some time, spit out a reward and go back to idle with a cooldown
                        if (TradeTimer > 160)
                        {
                            NPC.netUpdate = true;
                            HeldItemIndex = -1;
                            Phase = (int)PhaseType.Idle;
                            // This timer increments during its idle phase so it's set to a negative value
                            // Once the value hits zero, the crab will be able to trade again
                            TradeTimer = -CalamityUtils.SecondsToFrames(8);

                            // Spawn the reward
                            if (HeldItemType > 0)
                            {
                                float stackMult = 1;
                                int dropAmt = 1;
                                if (ContentSamples.ItemsByType[HeldItemType].maxStack > 1)
                                {
                                    if (OldItemType == ModContent.ItemType<GiantPearl>())
                                        stackMult *= 2;
                                    if (OldItemType == ItemID.GalaxyPearl)
                                        stackMult *= Main.rand.NextFloat(1.2f, 2f);
                                }
                                else
                                {
                                    if (OldItemType == ModContent.ItemType<GiantPearl>())
                                        dropAmt *= 2;
                                    if (OldItemType == ItemID.GalaxyPearl)
                                        dropAmt *= 2;
                                }
                                for (int i = 0; i < dropAmt; i++)
                                {
                                    SpawnItem(HeldItemType, stackMult);
                                }
                            }
                            HeldItemType = 0;
                            OldItemType = 0;
                            StackMin = 0;
                            StackMax = 0;
                        }
                    }
                    break;
                // Burrow away and despawn 
                case (int)PhaseType.Burrow:
                    {
                        NPC.velocity.X *= 0.95f;
                        if (NPC.velocity.Y == 0)
                        {
                            WalkTimer++;
                            GeneralParticleHandler.SpawnParticle(new Particles.StoneDebrisParticle(Main.rand.NextVector2FromRectangle(NPC.getRect() with { Y = (int)NPC.Center.Y }), new Vector2(Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, -2)), Lighting.GetColor(NPC.Bottom.ToTileCoordinates()), Main.rand.NextFloat(0.5f, 1.2f), 10));
                        }
                        if (WalkTimer % 10 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Dig, NPC.Center);
                        }
                        int startFall = 40;
                        if (WalkTimer >= startFall)
                        {
                            NPC.alpha += 5;
                            if (NPC.alpha > 255)
                            {
                                NPC.active = false;
                            }
                        }
                    }
                    break;
            }

            if ((Phase != (int)PhaseType.FoundItem && Phase != (int)PhaseType.Burrow) && WalkTimer > 0)
                WalkTimer--;

            if (TurnTimer > 0)
                TurnTimer--;

            NPC.spriteDirection = NPC.direction;
        }

        public void SpawnItem(int ID, float stackMult = 1f)
        {
            int stack = 1;
            if (NPC.localAI[0] != NPC.localAI[1])
            {
                stack = Main.rand.Next((int)NPC.localAI[0], (int)NPC.localAI[1] + 1);
            }
            stack = (int)(stack * stackMult);
            int i = Item.NewItem(NPC.GetSource_FromThis(), new Rectangle((int)NPC.Center.X + NPC.direction * 20, (int)NPC.Center.Y - 20, NPC.width, NPC.height), ID, Stack: stack, noBroadcast: false);
            Main.item[i].velocity = new Vector2(NPC.direction * Main.rand.NextFloat(3.7f, 4.3f), -Main.rand.NextFloat(-1.2f, 0.8f));
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
            }
        }

        // Checks if the horizontal position in front of it has water or is a pit so that the crab can avoid it
        public bool WaterCheck(int height)
        {
            Point startPos = NPC.direction == 1 ? NPC.Right.ToTileCoordinates() : NPC.Left.ToTileCoordinates();
            for (int i = 1; i < height; i++)
            {
                Tile t = CalamityUtils.ParanoidTileRetrieval(startPos.X + NPC.direction, startPos.Y + i);
                Tile above = CalamityUtils.ParanoidTileRetrieval(startPos.X + NPC.direction, startPos.Y + i - 1);
                // If there's a tile, wegud
                if (t.HasTile)
                    return false;
                // If there's liquid or a pit, we not gud
                if (t.LiquidAmount > 0 && !above.HasTile)
                    return true;

            }
            return true;
        }

        // Checks if the horizontal position in front of the crab can be jumped up
        public bool JumpCheck()
        {
            Point startPos = NPC.direction == 1 ? NPC.BottomRight.ToTileCoordinates() : NPC.BottomLeft.ToTileCoordinates();
            bool canJump = false;
            int add = NPC.direction == 1 ? 0 : -1;
            int jumpHeight = 6;
            for (int i = jumpHeight; i > 0; i--)
            {
                Tile t = CalamityUtils.ParanoidTileRetrieval(startPos.X + add, startPos.Y - i);
                // If there are tiles obfuscating the jump position, return false
                if (i > (jumpHeight - 2) && t.HasTile)
                    return false;
                // If there is a tile to jump on, return true
                if (t.HasTile)
                {
                    canJump = true;
                    break;
                }
            }
            return canJump;
        }

        public int CalculateReward(out int min, out int max)
        {
            min = 0;
            max = 0;
            // Make sure the held item type is valid
            if (ScavengerLoot.TryGetValue(HeldItemType, out WeightedRandom<ScavengerItem> value))
            {
                // If the held item has no roll value, immediately return
                if (value == null)
                    return 0;

                int tries = 100;
                for (int i = 0; i < tries; i++)
                {
                    var v = value.Get();
                    if (v.condition.Invoke())
                    {
                        min = v.minimum;
                        max = v.maximum;
                        return v.itemID;
                    }
                }

                return 0;
            }
            else
            {
                return 0;
            }
        }

        public static bool IsPassableTile(int x, int y)
        {
            return (!Main.tile[x, y].HasUnactuatedTile ||
                !Main.tileSolid[(int)Main.tile[x, y].TileType] || Main.tileSolidTop[(int)Main.tile[x, y].TileType]);
        }

        public void StepUp()
        {
            Vector2 position = NPC.position;
            position.X += NPC.velocity.X;
            int x = (int)((position.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1)) * NPC.direction) / 16f);
            int y = (int)((position.Y + (float)NPC.height - 1f) / 16f);

            if ((float)(x * 16) >= position.X + (float)NPC.width || (float)(x * 16 + 16) <= position.X)
                return;

            bool nextTileValid = Main.tile[x, y].HasUnactuatedTile && !Main.tile[x, y].TopSlope && !Main.tile[x, y - 1].TopSlope && Main.tileSolid[(int)Main.tile[x, y].TileType] && !Main.tileSolidTop[(int)Main.tile[x, y].TileType];
            bool aboveTileHalfBlock = Main.tile[x, y - 1].IsHalfBlock && Main.tile[x, y - 1].HasUnactuatedTile;
            bool aboveTileHasRoom = Main.tile[x, y - 1].IsHalfBlock && IsPassableTile(x, y - 4);
            bool aboveTileEmpty = !Main.tile[x, y - 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 1].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 1].TileType] || aboveTileHasRoom;
            bool tile3AbovePassable = !Main.tile[x - NPC.direction, y - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x - NPC.direction, y - 3].TileType];

            if ((nextTileValid || aboveTileHalfBlock) && aboveTileEmpty && IsPassableTile(x, y - 2) && IsPassableTile(x, y - 3) && tile3AbovePassable)
            {
                float npcBottom = (float)(y * 16);
                if (Main.tile[x, y].IsHalfBlock)
                {
                    npcBottom += 8f;
                }
                if (Main.tile[x, y - 1].IsHalfBlock)
                {
                    npcBottom -= 8f;
                }
                if (npcBottom < position.Y + (float)NPC.height)
                {
                    float percentageTileRisen = position.Y + (float)NPC.height - npcBottom;
                    if (percentageTileRisen <= 16.1f)
                    {
                        NPC.gfxOffY += NPC.position.Y + (float)NPC.height - npcBottom;
                        NPC.position.Y = npcBottom - (float)NPC.height;
                        if (percentageTileRisen < 9f)
                        {
                            NPC.stepSpeed = 1f;
                        }
                        else
                        {
                            NPC.stepSpeed = 2f;
                        }
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            float frameCount = Main.npcFrameCount[Type] - 1;

            if (ShouldUseWalkingFrames)
                frameCount = 5;
            else if (ShouldUseInspectionFrames)
                frameCount = 23;
            else if (ShouldUseGivingFrames)
                frameCount = 12;

            NPC.frameCounter++;
            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y++;
                NPC.frameCounter = 0;
            }

            // Reset frame when transitioning to giving animation
            if (NPC.frame.Y >= frameCount || (Phase == (int)PhaseType.Bartering && TradeTimer == 80))
            {
                NPC.frame.Y = 0;
                NPC.frameCounter = 0;
            }
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneTimelessShores && !spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.Cavern.Chance * 0.4f;
            }
            return 0f;

            //fuck this
            //if (spawnInfo.Player.Calamity().ZoneSunkenSeaShores && !spawnInfo.Player.Calamity().clamity && tile.WallType == ModContent.WallType<RunestoneWall>())
            //{
            //    return 0.05f;
            //}
            //return 0f;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            PlayerHurt();
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            PlayerHurt();
        }

        public void PlayerHurt()
        {
            if (Phase != (int)PhaseType.Burrow)
            {
                WalkTimer = 0;
                Phase = (int)PhaseType.Burrow;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n == NPC)
                        continue;
                    if (n.type != NPC.type)
                        continue;
                    if (n.Distance(NPC.Center) > 600)
                        continue;
                    Scavenger scav = n.ModNPC<Scavenger>();
                    scav.PlayerHurt();
                    scav.WalkTimer += Main.rand.Next(-20, 10);
                }
            }
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
            CalamityUtils.SpawnGores(NPC, "Scavenger", 3);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            int frameCount = Main.npcFrameCount[Type];
            float extraPosOffset = 0;
            if (ShouldUseWalkingFrames)
            {
                texture = walkTexture.Value;
                frameCount = 6;
            }
            else if (ShouldUseInspectionFrames)
            {
                texture = inspectTexture.Value;
                frameCount = 24;
                extraPosOffset = 6;
            }
            else if (ShouldUseGivingFrames)
            {
                texture = giveTexture.Value;
                frameCount = 13;
            }
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frameCount / 2);
            Vector2 npcOffset = NPC.Center - screenPos + Vector2.UnitY * extraPosOffset;
            npcOffset -= new Vector2(texture.Width, texture.Height / frameCount) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            int burrowHide = 0;
            if (Phase == (int)PhaseType.Burrow)
            { 
                float comp = Utils.GetLerpValue(40, 120, WalkTimer, true);
                burrowHide = (int)MathHelper.Lerp(0, texture.Height / frameCount + 1, comp);
                npcOffset.Y += MathHelper.Lerp(0, NPC.height, comp);
            }
            Rectangle frame = texture.Frame(1, frameCount, 0, NPC.frame.Y);
            spriteBatch.Draw(texture, npcOffset, frame with { Height = frame.Height - burrowHide }, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            // my dreams devoured
            // legacy code for visually holding an item
            /*Texture2D item = TextureAssets.Item[HeldItemType].Value;
            Vector2 itemOffset = new Vector2(NPC.direction == 1 ? 8 : -8, 20);
            if (TradeTimer < 80)
                spriteBatch.Draw(item, npcOffset + itemOffset, null, drawColor, 0, new Vector2(item.Width / 2, item.Height), 1f, spriteEffects, 0);
            */
            return false;
        }
    }
}
