using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSilva
{
    public class SilvaPiano : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureSilva.SilvaPiano>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SilvaCrystal>(15).
                AddIngredient(ItemID.Bone, 4).
                AddIngredient(ItemID.Book).
                AddTile(TileID.GlassKiln).
                Register();
        }
    }
}
