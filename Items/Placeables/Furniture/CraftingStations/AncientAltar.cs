using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Items.Placeables.Ores;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.CraftingStations
{
    public class AncientAltar : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CraftingStations.AncientAltar>());
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BrimstoneSlag>(10).
                AddIngredient<ScorchedBone>(5).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
