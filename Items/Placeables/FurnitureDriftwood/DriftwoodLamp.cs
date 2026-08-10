using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureDriftwood;

public class DriftwoodLamp : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureDriftwood.DriftwoodLamp>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Torch).
            AddIngredient<Driftwood>(3).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
