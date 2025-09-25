using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class StarBeamRye : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static int MaxManaBoost = 50;
        public static float MagicDamageBoost = 0.08f;
        public static float ManaCostReduction = 0.1f;
        public static float DefenseLossPercent = 0.06f;
        public static int RegenLoss = 2;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaBoost, MagicDamageBoost.ToPercent(), ManaCostReduction.ToPercent(), DefenseLossPercent.ToPercent(), RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            // Dark red-orange
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(89, 18, 10),
                new Color(102, 39, 14),
                new Color(128, 31, 9)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(20, 34, ModContent.BuffType<StarBeamRyeBuff>(), CalamityUtils.MinutesToFrames(8), true);

            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Lime;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<StarblightSoot>(5).
                AddIngredient<AureusCell>().
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<AureusCell>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
