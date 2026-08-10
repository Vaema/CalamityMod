using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureVoid;

public class VoidLantern : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureVoid.VoidLantern>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothVoidstone>(6).
            AddIngredient(ItemID.Torch).
            AddTile<VoidCondenser>().
            Register();
    }
}
