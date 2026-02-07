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

        public static float MagicDmgMult = 0.9f;
        public static int ManaRegenBoost = 20;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MagicDmgMult);

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
            Item.DefaultToFood(20, 34, ModContent.BuffType<StarBeamRyeBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.Starfruit).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
