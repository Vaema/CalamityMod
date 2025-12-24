using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    [LegacyName("FabsolsVodka")]
    public class PurpleHaze : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float DamageBoost = 0.25f;
        public static float StealthDamageLoss = 0.25f;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(),StealthDamageLoss.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(239, 123, 202),
                new Color(187, 56, 158),
                new Color(165, 47, 255)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(30, 42, ModContent.BuffType<PurpleHazeBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe(6).
                AddIngredient(ItemID.Bottle, 6).
                AddIngredient(ItemID.Plum).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
