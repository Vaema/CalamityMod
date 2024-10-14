using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Vodka : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        internal static readonly int CritBoost = 2;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            // Another clear drink
            ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
                new Color(219, 219, 208, 180),
                new Color(181, 181, 176, 180)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 36, ModContent.BuffType<VodkaBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightPurple;
        }
    }
}
