using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class GrapeBeer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float CritLoss = 75;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CritLoss);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(36, 2, 41),
                new Color(56, 0, 64),
                new Color(82, 10, 92)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(12, 28, ModContent.BuffType<GrapeBeerBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 3);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.Grapes).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
