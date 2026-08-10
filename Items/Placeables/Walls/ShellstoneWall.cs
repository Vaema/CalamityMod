using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;
using Terraria.ID;
using CalamityMod.Items.Placeables.SunkenSea;

namespace CalamityMod.Items.Placeables.Walls;

public class ShellstoneWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.ShellstoneWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<Shellstone>().
            AddTile(TileID.WorkBenches).
            Register();
    }

}
