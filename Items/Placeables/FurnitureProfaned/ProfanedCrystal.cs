using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureProfaned;

public class ProfanedCrystal : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureProfaned.ProfanedCrystal>());

    public override void AddRecipes()
    {
        CreateRecipe(50).
            AddIngredient(ItemID.Glass, 50).
            AddIngredient<UnholyEssence>().
            AddTile(TileID.AdamantiteForge).
            Register();
        CreateRecipe().
            AddIngredient<ProfanedCrystalWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
