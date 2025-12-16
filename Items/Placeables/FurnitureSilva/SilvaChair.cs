using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSilva
{
    public class SilvaChair : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureSilva.SilvaChair>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SilvaCrystal>(4).
                AddTile(TileID.GlassKiln).
                Register();
        }
    }
}
