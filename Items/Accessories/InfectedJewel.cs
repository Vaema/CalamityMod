using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("CelestialJewel")]
    public class InfectedJewel : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int RegenBoost = 2;
        public static int DebuffedRegenBoost = 3; // Added on top of the baseline regen boost
        public static int DebuffedDefenseBoost = 8;
        public static int ExtraDebuffDefenseBoost = 4; // Per additional debuff
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), DebuffedDefenseBoost, DebuffedRegenBoost.ToRegenPerSecond(), ExtraDebuffDefenseBoost);
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.defense = 2;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.infectedJewel = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CrownJewel>().
                AddIngredient<AureusCell>(10).
                AddIngredient<StarblightSoot>(25).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
