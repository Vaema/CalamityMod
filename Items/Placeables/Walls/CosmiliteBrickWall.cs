using CalamityMod.Items.Placeables.FurnitureCosmilite;
using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls;

public class CosmiliteBrickWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.CosmiliteBrickWall>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient<CosmiliteBrick>().
            AddTile(TileID.WorkBenches).
            Register();
    }
}
