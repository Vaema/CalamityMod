using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class StarBeamRye : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            // Dark red-orange
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(89, 18, 10),
                new Color(102, 39, 14),
                new Color(128, 31, 9)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(20, 34, ModContent.BuffType<StarBeamRyeBuff>(), CalamityUtils.SecondsToFrames(480f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Lime;
        }
    }
}
