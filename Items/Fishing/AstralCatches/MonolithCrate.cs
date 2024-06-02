using CalamityMod.Items.Critters;
using CalamityMod.Tiles.Astral;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.AstralCatches
{
    public class MonolithCrate : ModItem, ILocalizedModType
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
            Item.createTile = ModContent.TileType<MonolithCrateTile>();
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
            // Materials
            itemLoot.Add(ItemID.FallenStar, 1, 5, 10);
            itemLoot.Add(ItemID.Meteorite, 5, 10, 20);
            itemLoot.Add(ItemID.MeteoriteBar, 10, 1, 3);

            // Pet
            itemLoot.Add(ModContent.ItemType<AstrophageItem>(), 10);

            // Bait
            itemLoot.Add(ModContent.ItemType<TwinklerItem>(), 5, 1, 3);
            itemLoot.Add(ItemID.EnchantedNightcrawler, 5, 1, 3);
            itemLoot.Add(ModContent.ItemType<ArcturusAstroidean>(), 5, 1, 3);

            itemLoot.Add(ItemID.Firefly, 3, 1, 3);

            // Potions
            itemLoot.AddCratePotionRules(false);

            // Money
            itemLoot.Add(ItemID.SilverCoin, 1, 10, 90);
            itemLoot.Add(ItemID.GoldCoin, 2, 1, 5);
        }
    }
}
