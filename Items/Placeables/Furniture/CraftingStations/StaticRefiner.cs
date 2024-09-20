using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.CraftingStations
{
    public class StaticRefiner : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CraftingStations.StaticRefiner>());
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Solidifier).
                AddIngredient<PurifiedGel>(5).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
