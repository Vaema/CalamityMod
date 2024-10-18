using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class GrapeBeer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(36, 2, 41),
                new Color(56, 0, 64),
                new Color(82, 10, 92)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(12, 28, 100);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 3);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<GrapeBeerBuff>(), 900);
        }
    }
}
