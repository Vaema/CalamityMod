using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureMonolith;

public class MonolithCandelabra : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMonolith.MonolithCandelabra>());
        Item.value = Item.sellPrice(silver: 3);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralMonolith>(5).
            AddIngredient(ItemID.Torch, 3).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
