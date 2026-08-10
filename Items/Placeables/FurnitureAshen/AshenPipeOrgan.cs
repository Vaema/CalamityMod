using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAshen;

[LegacyName("AshenPiano")]
public class AshenPipeOrgan : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAshen.AshenPipeOrgan>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothBrimstoneSlag>(15).
            AddIngredient(ItemID.Bone, 4).
            AddIngredient(ItemID.Book).
            AddTile<AshenAltar>().
            Register();
    }
}
