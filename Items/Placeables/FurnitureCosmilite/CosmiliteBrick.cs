using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureCosmilite;

public class CosmiliteBrick : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureCosmilite.CosmiliteBrick>());

    public override void AddRecipes()
    {
        CreateRecipe(200).
            AddRecipeGroup("AnyStoneBlock", 200).
            AddIngredient<CosmiliteBar>().
            AddTile<CosmicAnvil>().
            Register();
        CreateRecipe().
            AddIngredient<CosmiliteBrickWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<CosmilitePlatform>(2).
            DisableDecraft().
            Register();
    }
}
