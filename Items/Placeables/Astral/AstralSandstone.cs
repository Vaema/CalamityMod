using CalamityMod.Items.Placeables.Walls;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Astral;

public class AstralSandstone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.Sandstone, 1);
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<AstralSand>();
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.AstralDesert.AstralSandstone>());

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralSandstoneWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
