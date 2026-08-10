using Terraria.ID;
using Terraria.ModLoader;
using TileItems = CalamityMod.Items.Placeables.DraedonStructures;
using WallTiles = CalamityMod.Walls.DraedonStructures;

namespace CalamityMod.Items.Placeables.Walls.DraedonStructures;

public class RustedPlatingWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.RustedPlatingWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<TileItems.RustedPlating>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
