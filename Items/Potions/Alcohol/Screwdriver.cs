using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Screwdriver : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float PiercingDamageBuff = 0.05f;
        public static int RegenLoss = 1;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((1f + PiercingDamageBuff).ToString(), RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(225, 84, 33),
                new Color(244, 176, 77),
                new Color(255, 218, 102)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 28, ModContent.BuffType<ScrewdriverBuff>(), CalamityUtils.MinutesToFrames(8), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightPurple;
        }
    }
}
