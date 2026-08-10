using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureMarnite;

public class MarniteChair : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMarnite.MarniteChair>());
        Item.value = Item.sellPrice(copper: 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PolishedMarniteBlock>(4).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
