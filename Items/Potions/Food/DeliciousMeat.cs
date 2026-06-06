using CalamityMod.Items.Tools;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food
{
    public class DeliciousMeat : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
            ItemID.Sets.FoodParticleColors[Type] = new Color[2] {
                new Color(147, 197, 206),
                new Color(94, 131, 168),
            };
            ItemID.Sets.IsFood[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.DefaultToFood(32, 30, BuffID.WellFed2, CalamityUtils.MinutesToFrames(30));
            Item.value = Item.buyPrice(silver: 50); // Sold by Archmage
            Item.rare = ItemRarityID.Pink;
        }
    }
}
