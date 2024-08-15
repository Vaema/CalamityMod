using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class CorruptionEffigy : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults() => ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ModContent.ItemType<CrimsonEffigy>(), 1);

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CorruptionEffigy>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CrimsonEffigy>().
                AddTile(TileID.TinkerersWorkbench).
                AddCondition(Condition.InGraveyard).
                Register()
                .DisableDecraft();
        }
    }
}
