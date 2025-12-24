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

        public static float KnockbackBoost = 0.5f;
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
            Item.DefaultToFood(32, 32, ModContent.BuffType<MoscowMuleBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void AddRecipes()
        {
            CreateRecipe(6).
                AddIngredient(ItemID.Bottle, 6).
                AddIngredient<TitanHeart>().
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
