using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class MoscowMule : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float KnockbackBoost = 0.5f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(KnockbackBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(194, 252, 192),
                new Color(226, 252, 192),
                new Color(250, 225, 222)
            };
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(46, 46, ModContent.BuffType<MoscowMuleBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient<TitanHeart>().
                AddTile(TileID.Kegs).
                Register();
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X += 8 * player.direction;
            player.itemLocation.Y -= 12;
        }
    }
}
