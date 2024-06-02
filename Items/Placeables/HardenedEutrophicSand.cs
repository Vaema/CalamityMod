using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    public class HardenedEutrophicSand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<EutrophicSand>();
        }

        public override void SetDefaults()
        {
            Item.width = 13;
            Item.height = 10;
            Item.createTile = ModContent.TileType<Tiles.SunkenSea.HardenedEutrophicSand>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.maxStack = 9999;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<EutrophicSand>().
                AddIngredient(ItemID.DirtBlock).
                AddTile(TileID.Solidifier).
                Register();
        }
    }
}
