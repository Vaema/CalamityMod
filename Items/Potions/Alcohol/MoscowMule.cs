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
    public class MoscowMule : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DamageBoost = 0.09f;
        public static float KnockbackBoost = 0.5f;
        public static int CritBoost = 3;
        public static int RegenLoss = 4;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), KnockbackBoost.ToPercent(), CritBoost, RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            // Pale green with a hint of red
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(194, 252, 192),
                new Color(226, 252, 192),
                new Color(250, 225, 222)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(32, 32, ModContent.BuffType<MoscowMuleBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<ScoriaOre>(3).
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<ScoriaOre>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
