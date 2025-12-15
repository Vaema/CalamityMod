using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class ZenPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(216, 230, 236),
                new Color(137, 149, 173),
                new Color(102, 85, 128)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(24, 28, ModContent.BuffType<Zen>(), CalamityUtils.MinutesToFrames(12), true);
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<PurifiedGel>(2).
                AddIngredient(ItemID.Daybloom, 3).
                AddTile(TileID.AlchemyTable).
                AddConsumeIngredientCallback(Recipe.IngredientQuantityRules.Alchemy).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<PurifiedGel>(2).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
