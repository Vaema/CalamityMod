using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class OldFashioned : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static readonly float DamageBoostMultiplier = 1.25f;
        public static readonly float DamageReductionMultiplier = 0.75f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((DamageBoostMultiplier).ToString("N2"), (DamageReductionMultiplier).ToString("N2"));

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(255, 118, 3),
                new Color(255, 200, 82),
                new Color(255, 228, 122)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(30, 38, ModContent.BuffType<OldFashionedBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Lime;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient(ItemID.Ectoplasm).
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient(ItemID.Ectoplasm).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
