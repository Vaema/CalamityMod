using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.FurnitureProfaned;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.CraftingStations;

[LegacyName("ProfanedBasin")]
public class ProfanedCrucible : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CraftingStations.ProfanedCrucible>());
        Item.value = Item.sellPrice(gold: 2);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ProfanedRock>(10).
            AddIngredient<UnholyEssence>(5).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
