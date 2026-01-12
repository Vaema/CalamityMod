using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureNavystone.FurnitureAncientNavystone
{
    public class AncientSmoothNavystone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.FurnitureAncientNavystone.AncientSmoothNavystone>());

        /*public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AncientSmoothNavystoneWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
        }*/
    }
}
