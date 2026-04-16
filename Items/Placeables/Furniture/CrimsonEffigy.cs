using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class CrimsonEffigy : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public static float DamageBoost = 0.15f;
        public static int DefenseBoost = 6;
        public static float MaxHealthLossPercent = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), DefenseBoost, MaxHealthLossPercent.ToPercent());

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CrimsonEffigy>());
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CorruptionEffigy>().
                AddTile(TileID.TinkerersWorkbench).
                AddCondition(Condition.InGraveyard).
                Register()
                .DisableDecraft();
        }
    }
}
