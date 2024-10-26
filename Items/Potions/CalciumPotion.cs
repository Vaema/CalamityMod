using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class CalciumPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float KnockbackResistance = 0.5f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(KnockbackResistance.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
                new Color(194, 202, 134),
                new Color(149, 144, 86)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(18, 30, ModContent.BuffType<CalciumBuff>(), CalamityUtils.MinutesToFrames(20), true);
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<AncientBoneDust>().
                AddTile(TileID.Bottles).
                Register()
                .DisableDecraft();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
