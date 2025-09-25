using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Vodka : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DamageBoost = 0.06f;
        public static int CritBoost = 2;
        public static int RegenLoss = 1;
        public static float DefenseLossPercent = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost, RegenLoss.ToRegenPerSecond(), DefenseLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            // Another clear drink
            ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
                new Color(219, 219, 208, 180),
                new Color(181, 181, 176, 180)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 36, ModContent.BuffType<VodkaBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightPurple;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<HallowedOre>(3).
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<HallowedOre>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
