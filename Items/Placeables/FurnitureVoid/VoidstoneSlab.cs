using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureVoid;

public class VoidstoneSlab : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureVoid.VoidstoneSlab>());

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothVoidstone>().
            AddTile<VoidCondenser>().
            Register();
        CreateRecipe().
            AddIngredient<VoidstoneSlabWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
