using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Moonshine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static int DefenseBoost = 10;
        public static float DamageReductionBoost = 0.03f;
        public static int RegenLoss = 1;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DefenseBoost, DamageReductionBoost.ToPercent(), RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            // Another clear drink
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(237, 237, 218, 128),
                new Color(227, 219, 191, 128),
                new Color(209, 204, 194, 128)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(24, 28, ModContent.BuffType<MoonshineBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
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
