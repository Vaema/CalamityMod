using System;
using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Fireball : ModItem, ILocalizedModType, IAlcoholItem
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DebuffBoost = 0.5f;
        public static float DebuffLoss = 0.5f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((DebuffBoost).ToPercent(), DebuffLoss.ToPercent());
        public AlcoholType AlcoholVariant => AlcoholType.Fireball;

        public Action<Player, float> AlcoholEffect => ApplyFireballEffect;

        private static void ApplyFireballEffect(Player player, float intensity)
        {
            // out of order 20 defense
        }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(245, 171, 22),
                new Color(227, 128, 41),
                new Color(237, 82, 31)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(16, 38, ModContent.BuffType<FireballBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {

            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.LivingFireBlock, 50).
                AddIngredient<StarblightSoot>(10).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
