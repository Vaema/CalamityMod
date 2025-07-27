using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureNavystone.FurnitureAncientNavystone
{
    [LegacyName("EutrophicClock")]
    public class AncientNavystoneClock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.FurnitureAncientNavystone.AncientNavystoneClock>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AncientSmoothNavystone>(10).
                AddRecipeGroup("IronBar", 3).
                AddIngredient(ItemID.Glass, 6).
                AddTile(TileID.Sawmill).
                Register();
        }
    }
}
