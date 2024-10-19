using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Everclear : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            // Everclear is clear so uhm... yeah?
            ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
                new Color(153, 168, 162, 180),
                new Color(198, 205, 207, 180)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 42, ModContent.BuffType<EverclearBuff>(), CalamityUtils.SecondsToFrames(60f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Lime;
        }
    }
}
