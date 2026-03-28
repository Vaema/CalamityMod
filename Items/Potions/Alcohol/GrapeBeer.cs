using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class GrapeBeer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public static float LongRangeDistance = 1000;
        public static float CloseRangeDamage = 0.8f;
        public static float LongRangeDamage = 0.4f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LongRangeDamage,LongRangeDistance/16f);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(36, 2, 41),
                new Color(56, 0, 64),
                new Color(82, 10, 92)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(12, 28, ModContent.BuffType<GrapeBeerBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.Grapes).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
