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
    public class CinnamonRoll : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(245, 223, 181),
                new Color(222, 186, 147),
                new Color(176, 129, 106)
            };
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(28, 48, ModContent.BuffType<CinnamonRollBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Lime;
        }
        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.TurtleShell).
                AddTile(TileID.Kegs).
                Register();
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X += 5 * player.direction;
            player.itemLocation.Y -= 9;
        }
    }
}
