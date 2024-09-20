using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureCosmilite
{
    public class CosmiliteLantern : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureCosmilite.CosmiliteLantern>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CosmiliteBrick>(6).
                AddIngredient(ItemID.Torch).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
