using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class EvergreenGin : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float SicknessWaterDebuffBoost = 0.25f;
        public static int RegenLoss = 1;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((1f + SicknessWaterDebuffBoost).ToString(), RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            // Pale green-blue
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(201, 248, 248),
                new Color(201, 248, 221),
                new Color(177, 240, 184)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(18, 32, ModContent.BuffType<EvergreenGinBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Lime;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<LivingShard>().
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<LivingShard>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
