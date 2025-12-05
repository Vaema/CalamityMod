using System.Collections.Generic;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;
using CalamityMod.Items.Placeables.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    [LegacyName("SunkenStew")]
    public class HadalStew : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public static int BuffType = BuffID.WellFed2;
        public static int BuffDuration = CalamityUtils.MinutesToFrames(60);
        public static int SicknessDuration = CalamityUtils.SecondsToFrames(50);
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(BuffDuration / 3600);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            // Even though this uses the drinking animation, it does not use DrinkParticleColors
            // This is so that dusts fly out (like Bowl of Soup) instead of drip down (like Apple Juice)
            ItemID.Sets.FoodParticleColors[Type] = new Color[4] {
                new Color(185, 117, 70),
                new Color(214, 98, 44),
                new Color(235, 156, 117),
                new Color(89, 54, 46)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(28, 18, 120);
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            string duration = Main.LocalPlayer.pStone ? (SicknessDuration / 60 * 0.75f).ToString("N1") : (SicknessDuration / 60).ToString();
            list.FindAndReplace("[S]", duration);
        }

        public override void ModifyPotionDelay(Player player, ref int baseDelay) => baseDelay -= CalamityUtils.SecondsToFrames(60) - SicknessDuration;

        public override void OnConsumeItem(Player player) => player.AddBuff(BuffType, BuffDuration);

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<AbyssGravel>(10).
                AddIngredient<CoastalDemonfish>().
                AddIngredient(ItemID.Honeyfin).
                AddIngredient(ItemID.Bowl, 2).
                AddTile(TileID.CookingPots).
                Register();

            CreateRecipe(2).
                AddIngredient<Voidstone>(10).
                AddIngredient<CoastalDemonfish>().
                AddIngredient(ItemID.Honeyfin).
                AddIngredient(ItemID.Bowl, 2).
                AddTile(TileID.CookingPots).
                Register();
        }
    }
}
