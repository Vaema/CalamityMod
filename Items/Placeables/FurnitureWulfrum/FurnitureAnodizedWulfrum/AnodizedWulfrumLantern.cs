using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum.FurnitureAnodizedWulfrum;

public class AnodizedWulfrumLantern : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.FurnitureAnodizedWulfrum.AnodizedWulfrumLantern>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<RoundedAnodizedWulfrumPanels>(6).
            AddIngredient(ItemID.Torch).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}
