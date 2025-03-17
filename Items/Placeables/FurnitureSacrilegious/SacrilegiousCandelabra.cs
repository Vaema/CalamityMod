using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSacrilegious
{
    public class SacrilegiousCandelabra : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SacrilegiousCandelabraTile>());
            Item.value = Item.sellPrice(silver: 3);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OccultBrickItem>(5).
                AddIngredient(ItemID.Torch, 3).
                AddTile<SCalAltar>().
                Register();
        }
    }
}
