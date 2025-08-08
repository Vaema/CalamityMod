using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class OddMushroom : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DamageBoost = 0.5f;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
                new Color(232, 100, 90),
                new Color(230, 215, 117),
                new Color(143, 83, 64)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(38, 50, ModContent.BuffType<Trippy>(), CalamityUtils.MinutesToFrames(60));

            Item.value = Item.buyPrice(gold: 5); // Sold by Truffle
            Item.rare = ItemRarityID.LightRed;
        }

        public override void OnConsumeItem(Player player)
        {
            if (player.Calamity().trippyLevel < 3)
                player.Calamity().trippyLevel++;
        }
    }
}
