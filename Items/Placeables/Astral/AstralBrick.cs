using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Astral
{
    public class AstralBrick : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.AstralBrick>());

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient<AstralOre>().
                AddIngredient<AstralStone>().
                AddTile(TileID.Furnaces).
                Register();

            CreateRecipe().
                AddIngredient<AstralBrickWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
        }
    }
}
