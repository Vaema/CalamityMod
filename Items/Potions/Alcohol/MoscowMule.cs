using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class MoscowMule : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        internal static readonly int CritBoost = 3;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            // Pale green with a hint of red
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(194, 252, 192),
                new Color(226, 252, 192),
                new Color(250, 225, 222)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(32, 32, ModContent.BuffType<MoscowMuleBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
