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
            Item.DefaultToFood(20, 32, ModContent.BuffType<BloodyMaryBuff>(), CalamityUtils.MinutesToFrames(6), true);
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Lime;
        }

         public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.BloodMoonStarter).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
