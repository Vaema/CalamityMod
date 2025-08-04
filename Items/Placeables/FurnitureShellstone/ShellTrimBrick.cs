using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Placeables.SunkenSea;
namespace CalamityMod.Items.Placeables.FurnitureShellstone
{
    public class ShellTrimBrick : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureShellstone.ShellTrimBrick>());

        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient<Shellstone>().
        //        AddTile(TileID.HeavyWorkBench).
        //        Register();
        //    CreateRecipe().
        //        AddIngredient<ShellstoneSlabWall>(4).
        //        AddTile(TileID.WorkBenches).
        //        DisableDecraft().
        //        Register();
        //}
    }
}
