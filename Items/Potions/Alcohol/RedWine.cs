using System;
using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions.Food;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class RedWine : ModItem, ILocalizedModType, IAlcoholItem
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float VerticalSpeedBoost = 0.1f;
        public static float FlightTimeLoss = 0.25f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(VerticalSpeedBoost.ToPercent(), FlightTimeLoss.ToPercent());
        public AlcoholType AlcoholVariant => AlcoholType.RedWine;

        public Action<Player, float> AlcoholEffect => ApplyRedWineEffect;

        private static void ApplyRedWineEffect(Player player, float intensity)
        {
            // out of order 20 defense
        }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(54, 5, 21),
                new Color(82, 9, 36),
                new Color(105, 4, 29)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 48, ModContent.BuffType<RedWineBuff>(), CalamityUtils.MinutesToFrames(6));

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Pink;
        }
        public override void AddRecipes()
        {
            CreateRecipe(20).
                AddIngredient(ItemID.Bottle, 20).
                AddIngredient(ItemID.FireFeather).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
