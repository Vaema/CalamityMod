using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Systems;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureMarnite;

public class PolishedMarniteBlock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMarnite.PolishedMarniteBlock>());

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient(ItemID.Marble, 2).
            AddIngredient(ItemID.Granite, 2).
            AddRecipeGroup(RecipeSystem.AnyGoldOre, 1).
            AddTile(TileID.WorkBenches).
            Register();
        CreateRecipe().
            AddIngredient<PolishedMarniteWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
        CreateRecipe().
            AddIngredient<PolishedMarnitePlatform>(2).
            DisableDecraft().
            Register();
    }
}
