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
    public class TequilaSunrise : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DoTMultiplier = 1.5f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((DoTMultiplier-1).ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(220, 75, 59),
                new Color(229, 147, 47),
                new Color(255, 190, 76)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(22, 28, ModContent.BuffType<TequilaSunriseBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.LightShard).
                AddIngredient<StarblightSoot>(10).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
