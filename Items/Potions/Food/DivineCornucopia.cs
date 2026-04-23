using CalamityMod.Items.Tools;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food
{
    public class DivineCornucopia : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

            ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
                new Color(241, 4, 8),
                new Color(142, 183, 57),
                new Color(245, 222, 73),
            };
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 28;
            Item.useAnimation = Item.useTime = 17;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
            Item.maxStack = 1;
            Item.UseSound = SoundID.Item2;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.buffType = BuffID.WellFed;
            Item.buffTime = CalamityUtils.MinutesToFrames(30);
            Item.useTurn = true;
            Item.consumable = false;
        }
    }
}
