using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Whiskey : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        internal static readonly int CritBoost = 2;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(227, 148, 11),
                new Color(235, 177, 5),
                new Color(250, 190, 12)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(18, 32, ModContent.BuffType<WhiskeyBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
