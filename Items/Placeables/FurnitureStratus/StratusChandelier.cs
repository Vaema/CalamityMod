using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureStratus;

public class StratusChandelier : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureStratus.StratusChandelier>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<StratusBricks>(4).
            AddIngredient(ItemID.Torch, 4).
            AddIngredient(ItemID.Chain).
            AddTile<Tiles.Furniture.CraftingStations.VoidCondenser>().
            Register();
    }
}
