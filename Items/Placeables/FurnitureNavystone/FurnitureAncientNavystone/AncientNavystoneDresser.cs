using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureNavystone.FurnitureAncientNavystone
{
    [LegacyName("EutrophicDresser")]
    public class AncientNavystoneDresser : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.FurnitureAncientNavystone.AncientNavystoneDresser>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AncientSmoothNavystone>(16).
                AddTile(TileID.Sawmill).
                Register();
        }
    }
}
