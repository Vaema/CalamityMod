using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureStatigel
{
    public class StatigelBlock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStatigel.StatigelBlock>());

        public override void AddRecipes()
        {
            CreateRecipe(25).
                AddIngredient<PurifiedGel>().
                AddTile(TileID.Solidifier).
                Register();
            CreateRecipe().
                AddIngredient<StatigelPlatform>(2).
                DisableDecraft().
                Register();
            CreateRecipe().
                AddIngredient<StatigelWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
        }
    }
}
