using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum;

public class WulfrumBed : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.WulfrumBed>());
        Item.value = Item.sellPrice(silver: 4);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<WulfrumPlating>(15).
            AddIngredient(ItemID.Silk, 5).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}
