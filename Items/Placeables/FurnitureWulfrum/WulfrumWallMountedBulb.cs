using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureWulfrum
{
    public class WulfrumWallMountedBulb : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.WulfrumWallMountedBulb> ());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient<AnodizedWulfrumMetal>().
                AddIngredient<WulfrumMetalScrap>().
                AddIngredient<EnergyCore>().
                AddTile(TileID.HeavyWorkBench).
                Register();
        }
    }
}
