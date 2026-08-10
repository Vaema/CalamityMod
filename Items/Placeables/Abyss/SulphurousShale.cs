using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Abyss;

public class SulphurousShale : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SulphurousSand>();
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Abyss.SulphurousShale>());

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Walls.SulphurousShaleWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
