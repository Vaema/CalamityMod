using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class BloodyMary : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DamageBoost = 0.1f;
        public static float MoveSpeedBoost = 0.1f; // Both 10% so we only need just one in the tooltip
        public static int RegenLoss = 4;
        public static float DefenseLossPercent = 0.04f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), RegenLoss.ToRegenPerSecond(), DefenseLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(204, 90, 73),
                new Color(168, 37, 37),
                new Color(120, 28, 28)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(20, 32, ModContent.BuffType<BloodyMaryBuff>(), CalamityUtils.MinutesToFrames(8), true);
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Lime;
        }

         public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<BloodOrb>(5).
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
