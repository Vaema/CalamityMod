using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class CaribbeanRum : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static int RegenBoost = 2;
        public static float MoveSpeedBoost = 0.1f;
        public static float DefenseLossPercent = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), MoveSpeedBoost.ToPercent(), DefenseLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            // Based on the drink itself
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(105, 29, 14),
                new Color(128, 39, 22),
                new Color(138, 28, 7)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 32, ModContent.BuffType<CaribbeanRumBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Lime;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<LivingShard>().
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<LivingShard>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
