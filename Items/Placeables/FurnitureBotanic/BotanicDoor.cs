using CalamityMod.Tiles.FurnitureBotanic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureBotanic;

public class BotanicDoor : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BotanicDoorClosed>());
        Item.value = Item.sellPrice(copper: 40);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<UelibloomBrick>(6).
            AddTile(TileID.LivingLoom).
            Register();
    }
}
