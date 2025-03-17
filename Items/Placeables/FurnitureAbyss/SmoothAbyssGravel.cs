using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAbyss
{
    public class SmoothAbyssGravel : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAbyss.SmoothAbyssGravel>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AbyssGravel>().
                AddTile(TileID.WorkBenches).
                Register();
            CreateRecipe().
                AddIngredient<SmoothAbyssGravelWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
            CreateRecipe().
                AddIngredient<SmoothAbyssGravelPlatform>(2).
                DisableDecraft().
                Register();
        }
    }
}
