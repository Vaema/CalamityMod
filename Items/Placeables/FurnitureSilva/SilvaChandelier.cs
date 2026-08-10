using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureSilva;

public class SilvaChandelier : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureSilva.SilvaChandelier>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SilvaCrystal>(4).
            AddIngredient(ItemID.Torch, 4).
            AddIngredient(ItemID.Chain).
            AddTile(TileID.GlassKiln).
            Register();
    }
}
