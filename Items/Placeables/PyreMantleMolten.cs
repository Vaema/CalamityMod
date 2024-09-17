using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader; // If you are using c# 6, you can use: "using static Terraria.Localization.GameCulture;" which would mean you could just write "DisplayName.AddTranslation(German, "");"

namespace CalamityMod.Items.Placeables
{
    public class PyreMantleMolten : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Abyss.PyreMantleMolten>());

        public override void AddRecipes()
        {
            CreateRecipe(25).
                AddIngredient(ItemID.LavaBucket).
                AddIngredient<PyreMantle>(25).
                AddTile(TileID.Furnaces).
                Register();
        }
    }
}
