using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class BoundingPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float JumpSpeedBoost = 0.25f;
        public static float JumpHeightPercentBoost = 0.2f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(JumpSpeedBoost.ToJumpSpeedPercent(), JumpHeightPercentBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
                new Color(213, 255, 226),
                new Color(141, 220, 166)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(26, 38, ModContent.BuffType<BoundingBuff>(), CalamityUtils.MinutesToFrames(8), true);
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient(ItemID.PinkGel).
                AddIngredient(ItemID.Moonglow).
                AddTile(TileID.Bottles).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(10).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
