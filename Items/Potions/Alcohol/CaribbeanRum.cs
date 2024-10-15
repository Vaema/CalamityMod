using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class CaribbeanRum : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            // Based on the drink itself
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(105, 29, 14),
                new Color(128, 39, 22),
                new Color(138, 28, 7)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 32, ModContent.BuffType<CaribbeanRumBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Lime;
        }
    }
}
