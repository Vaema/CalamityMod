using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureBotanic;

public class BotanicClock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureBotanic.BotanicClock>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<UelibloomBrick>(10).
            AddRecipeGroup("IronBar", 3).
            AddIngredient(ItemID.Glass, 6).
            AddTile(TileID.LivingLoom).
            Register();
    }
}
