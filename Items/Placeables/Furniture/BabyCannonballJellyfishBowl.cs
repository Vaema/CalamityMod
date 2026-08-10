using CalamityMod.Items.Critters;
using CalamityMod.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture;

public class BabyCannonballJellyfishBowl : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BabyCannonballJellyfishBowlTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<BabyCannonballJellyfishItem>().
            AddIngredient(ItemID.BottledWater).
            Register();
    }
}
