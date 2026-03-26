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
    public class Moonshine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float MaxLifePercentBoost = 0.33f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxLifePercentBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(237, 237, 218, 128),
                new Color(227, 219, 191, 128),
                new Color(209, 204, 194, 128)
            };
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(30, 48, ModContent.BuffType<MoonshineBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Lime;
        }

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.LifeFruit).
                AddIngredient<AncientBoneDust>(5).
                AddTile(TileID.Kegs).
                Register();
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X += 4 * player.direction;
            player.itemLocation.Y -= 8;
        }
    }
}
