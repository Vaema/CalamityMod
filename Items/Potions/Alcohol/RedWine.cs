using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class RedWine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float VerticalSpeedBoost = 0.1f;
        public static float FlightTimeLoss = 0.25f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(VerticalSpeedBoost.ToPercent(), FlightTimeLoss.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(54, 5, 21),
                new Color(82, 9, 36),
                new Color(105, 4, 29)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 48, ModContent.BuffType<RedWineBuff>(), CalamityUtils.MinutesToFrames(5));

            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.Grapes).
                AddIngredient<StarblightSoot>(5).
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<StarblightSoot>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
