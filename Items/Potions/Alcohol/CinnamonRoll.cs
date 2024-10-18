using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class CinnamonRoll : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(245, 223, 181),
                new Color(222, 186, 147),
                new Color(176, 129, 106)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(18, 32, ModContent.BuffType<CinnamonRollBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
