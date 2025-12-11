using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class IVDripOnTheRocks : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public static readonly float DamageBoostMultiplier = 1.25f; // Same as Old Fashioned but can be changed at any time
        public static readonly float DamageReductionMultiplier = 0.75f; // Same as Old Fashioned but can be changed at any time
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((DamageBoostMultiplier).ToString("N2"), (DamageReductionMultiplier).ToString("N2")); public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 60;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.ivDrip = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OldFashioned>().
                AddIngredient<DarkPlasma>().
                AddIngredient<ArmoredShell>().
                AddIngredient<TwistingNether>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
