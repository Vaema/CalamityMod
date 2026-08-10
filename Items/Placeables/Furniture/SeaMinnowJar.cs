using CalamityMod.Items.Critters;
using CalamityMod.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

public class SeaMinnowJar : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<SeaMinnowJarTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SeaMinnowItem>().
            AddIngredient(ItemID.BottledWater).
            Register();
    }
}
