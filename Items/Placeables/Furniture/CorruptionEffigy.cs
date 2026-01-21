using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class CorruptionEffigy : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public static float MoveSpeedBoost = 0.1f;
        public static int CritBoost = 10; // Both 10% so we only need just one in the tooltip
        public static float DamageReductionLoss = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CritBoost, DamageReductionLoss.ToPercent());

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CorruptionEffigy>());
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
        }

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
