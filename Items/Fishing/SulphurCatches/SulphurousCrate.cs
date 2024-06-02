using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Tools.ClimateChange;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using CrateTile = CalamityMod.Tiles.Abyss.SulphurousCrateTile;

namespace CalamityMod.Items.Fishing.SulphurCatches
{
    [LegacyName("AbyssalCrate")]
    public class SulphurousCrate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.IsFishingCrate[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
            Item.createTile = ModContent.TileType<CrateTile>();
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
        }

        public override bool CanRightClick() => true;
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            var tier1AbyssAvailable = itemLoot.DefineConditionalDropSet(() => DownedBossSystem.downedSlimeGod || Main.hardMode);
            var tier1AcidRain = itemLoot.DefineConditionalDropSet(() => DownedBossSystem.downedEoCAcidRain);

            // Materials
            itemLoot.Add(ModContent.ItemType<SulphurousSand>(), 1, 5, 10);
            itemLoot.Add(ModContent.ItemType<SulphurousSandstone>(), 1, 5, 10);
            itemLoot.Add(ModContent.ItemType<HardenedSulphurousSandstone>(), 1, 5, 10);
            itemLoot.Add(ModContent.ItemType<Acidwood>(), 1, 5, 10);

            tier1AcidRain.Add(ModContent.ItemType<SulphuricScale>(), 10, 1, 3);

            // Pre-HM Abyss Weapons
            tier1AbyssAvailable.Add(new OneFromOptionsDropRule(10, 1,
                ModContent.ItemType<BallOFugu>(),
                ModContent.ItemType<Archerfish>(),
                ModContent.ItemType<BlackAnurian>(),
                ModContent.ItemType<HerringStaff>(),
                ModContent.ItemType<Lionfish>()
            ));

            // Pre-HM Abyss Equipment (and Torrential Tear)
            tier1AbyssAvailable.Add(new OneFromOptionsDropRule(4, 1,
                ModContent.ItemType<AnechoicPlating>(),
                ModContent.ItemType<DepthCharm>(),
                ModContent.ItemType<IronBoots>(),
                ModContent.ItemType<StrangeOrb>(),
                ModContent.ItemType<TorrentialTear>()
            ));

            // Bait
            itemLoot.Add(ItemID.MasterBait, 10, 1, 2);
            itemLoot.Add(ItemID.JourneymanBait, 5, 1, 3);
            itemLoot.Add(ItemID.ApprenticeBait, 3, 2, 3);

            // Potions
            itemLoot.Add(ModContent.ItemType<AnechoicCoating>(), 10, 1, 3);
            itemLoot.AddCratePotionRules(false);

            // Money
            itemLoot.Add(ItemID.SilverCoin, 1, 10, 90);
            itemLoot.Add(ItemID.GoldCoin, 2, 1, 5);
        }
    }
}
