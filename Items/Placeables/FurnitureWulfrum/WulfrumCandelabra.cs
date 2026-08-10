using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureWulfrum;

public class WulfrumCandelabra : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.WulfrumCandelabra>());
        Item.value = Item.sellPrice(silver: 3);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<WulfrumPlating>(5).
            AddIngredient(ItemID.Torch, 3).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}
