using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum;

public class WulfrumBookcase : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.WulfrumBookcase>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<WulfrumPlating>(20).
            AddIngredient(ItemID.Book, 10).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}
