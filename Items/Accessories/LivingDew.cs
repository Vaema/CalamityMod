using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class LivingDew : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int MaxLifeBoost = 25;
        public static int RegenTimeBoost = 1;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxLifeBoost, RegenTimeBoost.ToPercent(), (HoneyDew.NaturalRegenPower - 1f).ToPercent());
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 22;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statLifeMax2 += MaxLifeBoost;

            // Inherits all effects of Honey Dew
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.honeyDew = true;
            modPlayer.livingDew = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<HoneyDew>().
                AddIngredient<LivingShard>(6).
                // TODO -- Replace with Water Essence
                AddIngredient<EssenceofSunlight>(5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
