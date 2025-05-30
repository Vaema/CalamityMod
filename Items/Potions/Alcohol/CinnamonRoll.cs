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
    public class CinnamonRoll : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        // Mana has a stupid formula so it won't go into the tooltip
        public static float ManaRegenDelayBonus = 1f;
        public static int ManaRegenBonus = 10;
        public static float HeatDebuffBoost = 0.5f;
        public static float DefenseLossPercent = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((1f + HeatDebuffBoost).ToString(), DefenseLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(245, 223, 181),
                new Color(222, 186, 147),
                new Color(176, 129, 106)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(18, 32, ModContent.BuffType<CinnamonRollBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<ScoriaOre>(3).
                AddIngredient(ItemID.LivingFireBlock).
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
