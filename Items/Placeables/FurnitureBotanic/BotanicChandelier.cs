using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureBotanic;

public class BotanicChandelier : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureBotanic.BotanicChandelier>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<UelibloomBrick>(4).
            AddIngredient(ItemID.Torch, 4).
            AddIngredient(ItemID.Chain).
            AddTile(TileID.LivingLoom).
            Register();
    }
}
