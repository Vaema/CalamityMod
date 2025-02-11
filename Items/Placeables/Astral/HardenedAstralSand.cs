using CalamityMod.Items.Placeables.Walls;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Astral
{
    public class HardenedAstralSand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.HardenedSand, 1);
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<AstralSand>();
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.AstralDesert.HardenedAstralSand>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<HardenedAstralSandWall>(4).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
        }
    }
}
