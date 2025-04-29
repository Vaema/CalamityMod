using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Fishing.AstralCatches;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class GravityNormalizerPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(255, 164, 94),
                new Color(109, 242, 196),
                new Color(255, 255, 191)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(24, 36, ModContent.BuffType<GravityNormalizerBuff>(), CalamityUtils.MinutesToFrames(8), true);
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Lime;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<AldebaranAlewife>().
                AddIngredient<AureusCell>().
                AddTile(TileID.AlchemyTable).
                AddConsumeIngredientCallback(Recipe.IngredientQuantityRules.Alchemy).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(10).
                AddIngredient<AureusCell>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
