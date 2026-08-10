using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Crags;

public class ScorchedBone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Crags.ScorchedBone>());

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Wood;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
        AddIngredient<ScorchedBoneWall>(4).
        AddTile(TileID.WorkBenches).
        DisableDecraft().
        Register();
    }
}
