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
    public class Manhattan : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DebuffBoost = 0.5f;
        public static float DebuffLoss = 0.5f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((DebuffBoost).ToPercent(), DebuffLoss.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(225, 84, 33),
                new Color(244, 176, 77),
                new Color(255, 218, 102)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 28, ModContent.BuffType<ManhattanBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightPurple;
        }
        public override void AddRecipes()
        {
            CreateRecipe(6).
                AddIngredient(ItemID.Bottle, 6).
                AddIngredient(ItemID.FrostCore).
                AddIngredient<StarblightSoot>(10).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
