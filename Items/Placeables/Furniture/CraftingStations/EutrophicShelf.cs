using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.CraftingStations
{
    [LegacyName("EutrophicCrafting")]
    public class EutrophicShelf : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CraftingStations.EutrophicShelf>());
            Item.value = Item.sellPrice(silver: 50); // This is too easy to craft to sell for 2 gold
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Navystone>(10).
                AddIngredient<SeaPrism>(5).
                AddIngredient<PrismShard>(5).
                AddIngredient<PearlShard>(3).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
