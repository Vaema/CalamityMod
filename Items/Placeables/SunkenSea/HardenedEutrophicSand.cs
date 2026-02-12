using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea
{
    public class HardenedEutrophicSand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<EutrophicSand>();
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.HardenedEutrophicSand>());

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
