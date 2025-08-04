using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Placeables.SunkenSea;

namespace CalamityMod.Items.Placeables.FurnitureNavystone
{
    public class PolishedNavystoneBrick : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.PolishedNavystoneBrick>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(2).
                AddTile(TileID.Furnaces).
                Register();
        }
    }
}
