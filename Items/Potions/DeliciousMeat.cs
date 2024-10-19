using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class DeliciousMeat : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.FoodParticleColors[Type] = new Color[2] {
                new Color(147, 197, 206),
                new Color(94, 131, 168),
            };
        }
        public override void SetDefaults()
        {
            Item.DefaultToFood(32, 30, BuffID.WellFed2, CalamityUtils.SecondsToFrames(1800f));
            Item.value = Item.buyPrice(silver: 50); // Sold by Archmage
            Item.rare = ItemRarityID.Pink;
        }
    }
}
