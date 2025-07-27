using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureNavystone
{
    public class NavystoneCandelabra : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.NavystoneCandelabra>());
            Item.value = Item.sellPrice(silver: 3);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SmoothNavystone>(5).
                AddIngredient(ItemID.Torch, 3).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
