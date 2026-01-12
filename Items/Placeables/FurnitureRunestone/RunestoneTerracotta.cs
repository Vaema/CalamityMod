using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureRunestone
{
    public class RunestoneTerracotta : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureRunestone.RunestoneTerracotta>());

        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient<RunestoneWall>(4).
        //        AddTile(TileID.WorkBenches).
        //        DisableDecraft().
        //        Register();
        //}

    }
}
