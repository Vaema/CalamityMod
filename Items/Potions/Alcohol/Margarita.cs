using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Margarita : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public static int BuffType = ModContent.BuffType<MargaritaBuff>();
        public static int BuffDuration = 10800;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(219, 227, 191),
                new Color(186, 189, 147),
                new Color(142, 161, 125)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(28, 40, 200);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Lime;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(BuffType, BuffDuration);
        }
    }
}
