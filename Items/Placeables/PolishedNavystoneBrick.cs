using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    public class PolishedNavystoneBrick : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.PolishedNavystoneBrick>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(2).
                AddTile(TileID.Furnaces).
                Register();
        }
    }
}
