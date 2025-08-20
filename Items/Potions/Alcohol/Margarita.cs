using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Margarita : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static int BuffType = ModContent.BuffType<MargaritaBuff>();
        public static float DebuffLoss = 0.5f;
        public static int MinuteDuration = 3; 
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DebuffLoss.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(219, 227, 191),
                new Color(186, 189, 147),
                new Color(142, 161, 125)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(28, 40, ModContent.BuffType<MargaritaBuff>(), CalamityUtils.MinutesToFrames(5), true);

            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Lime;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<LivingShard>().
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<LivingShard>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
