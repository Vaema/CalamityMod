using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Fireball : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float HeatDebuffBoost = 0.25f;
        public static int RegenLoss = 1;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((1f + HeatDebuffBoost).ToString(), RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(245, 171, 22),
                new Color(227, 128, 41),
                new Color(237, 82, 31)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(16, 38, ModContent.BuffType<FireballBuff>(), CalamityUtils.MinutesToFrames(8), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
