using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureProfaned;

public class RunicProfanedBrick : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureProfaned.RunicProfanedBrick>());

    public override void AddRecipes()
    {
        CreateRecipe(5).
            AddIngredient<ProfanedRock>(4).
            AddIngredient<ProfanedCrystal>().
            AddTile(TileID.AdamantiteForge).
            Register();
        CreateRecipe().
            AddIngredient<RunicProfanedBrickWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
