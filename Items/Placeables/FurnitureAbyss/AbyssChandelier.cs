using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAbyss;

public class AbyssChandelier : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAbyss.AbyssChandelier>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothAbyssGravel>(4).
            AddIngredient(ItemID.Torch, 4).
            AddIngredient(ItemID.Chain).
            AddTile<VoidCondenser>().
            Register();
    }
}
