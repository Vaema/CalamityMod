using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Tequila : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float KnockbackBoost = 0.1f;
        public static int CritBoost = 4;
        public static int DefenseBoost = 5;
        public static int RegenLoss = 1;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(KnockbackBoost.ToPercent(), CritBoost, DefenseBoost, RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(163, 110, 10),
                new Color(176, 135, 0),
                new Color(194, 132, 25)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(16, 34, ModContent.BuffType<TequilaBuff>(), CalamityUtils.MinutesToFrames(8), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
