using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum.FurnitureAnodizedWulfrum;

public class AnodizedWulfrumDeskLight : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.FurnitureAnodizedWulfrum.AnodizedWulfrumDeskLight>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<RoundedAnodizedWulfrumPanels>(4).
            AddIngredient(ItemID.Torch).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}
