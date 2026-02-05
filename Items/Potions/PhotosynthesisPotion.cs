using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class PhotosynthesisPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static int IncreasedHeartHeal = 5;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(IncreasedHeartHeal);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(246, 235, 143),
                new Color(230, 204, 121),
                new Color(214, 173, 78)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(30, 34, ModContent.BuffType<PhotosynthesisBuff>(), CalamityUtils.MinutesToFrames(8), true);
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient(ItemID.Daybloom, 3).
                AddIngredient<EssenceofSunlight>().
                AddTile(TileID.AlchemyTable).
                AddConsumeIngredientCallback(Recipe.IngredientQuantityRules.Alchemy).
                AddDecraftCondition(Condition.Hardmode).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<EssenceofSunlight>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
