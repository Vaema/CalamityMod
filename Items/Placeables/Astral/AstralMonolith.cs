using CalamityMod.Items.Placeables.FurnitureMonolith;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Astral
{
    public class AstralMonolith : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Astral.AstralMonolith>());

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            // It's not really wood... but it comes from trees!
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Wood;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AstralMonolithWall>(4).
                AddTile(TileID.WorkBenches).
                Register();

            CreateRecipe().
                AddIngredient<MonolithPlatform>(2).
                Register();
        }
    }
}
