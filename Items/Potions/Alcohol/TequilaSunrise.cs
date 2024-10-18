using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class TequilaSunrise : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        internal static readonly int CritBoost = 8;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(220, 75, 59),
                new Color(229, 147, 47),
                new Color(255, 190, 76)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(22, 28, ModContent.BuffType<TequilaSunriseBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
