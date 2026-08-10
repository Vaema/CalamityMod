using CalamityMod.Items.Critters;
using CalamityMod.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

public class BabyGhostBellJar : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BabyGhostBellJarTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<BabyGhostBellItem>().
            AddIngredient(ItemID.BottledWater).
            Register();
    }
}
