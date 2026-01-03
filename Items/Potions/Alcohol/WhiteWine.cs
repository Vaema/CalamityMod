using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class WhiteWine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float FlightTimeRecoveryAmount = 0.66f;
        public static float FlightTimeLoss = 0.5f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FlightTimeLoss.ToPercent());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            // Clear, yellow-green
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(242, 252, 177, 180),
                new Color(250, 252, 215, 180),
                new Color(228, 245, 181, 180)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 44, ModContent.BuffType<WhiteWineBuff>(), CalamityUtils.MinutesToFrames(6), true);
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(Item.buffType, Item.buffTime);
        }
        public override void AddRecipes()
        {
            CreateRecipe(20).
                AddIngredient(ItemID.Bottle, 20).
                AddIngredient(ItemID.GiantHarpyFeather).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
