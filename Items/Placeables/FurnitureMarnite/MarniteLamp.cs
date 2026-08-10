using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureMarnite;

public class MarniteLamp : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMarnite.MarniteLamp>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PolishedMarniteBlock>(4).
            AddIngredient(ItemID.Torch).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
