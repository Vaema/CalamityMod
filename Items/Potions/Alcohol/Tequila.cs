using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Tequila : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        internal static readonly int CritBoost = 4;

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
            Item.DefaultToFood(16, 34, ModContent.BuffType<TequilaBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
