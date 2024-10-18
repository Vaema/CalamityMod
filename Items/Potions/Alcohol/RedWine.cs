using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class RedWine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(54, 5, 21),
                new Color(82, 9, 36),
                new Color(105, 4, 29)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(14, 48, 200);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void GetHealLife(Player player, bool quickHeal, ref int healValue)
        {
            healValue = player.Calamity().baguette ? 250 : 200;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<RedWineBuff>(), 900);
        }
    }
}
