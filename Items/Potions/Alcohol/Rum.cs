using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Rum : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static int RegenBoost = 2;
        public static float MoveSpeedBoost = 0.1f;
        public static float DefenseLossPercent = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), MoveSpeedBoost.ToPercent(), DefenseLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(237, 165, 9),
                new Color(247, 219, 54),
                new Color(255, 195, 31)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(24, 26, ModContent.BuffType<RumBuff>(), CalamityUtils.MinutesToFrames(8), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
