using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class AbyssalMirror : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";
        public bool HasFlavorTooltip => true;

        public static int AggroReduction = 450;
        public static float StandingStealthRegenBoost = 0.25f;
        public static float MovingStealthRegenBoost = 0.12f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StandingStealthRegenBoost.ToPercent(), MovingStealthRegenBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 38;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.stealthGenStandstill += StandingStealthRegenBoost;
            modPlayer.stealthGenMoving += MovingStealthRegenBoost;
            modPlayer.abyssalMirror = true;
            player.aggro -= AggroReduction;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MirageMirror>().
                AddIngredient<InkBomb>().
                AddIngredient<DepthCells>(5).
                AddIngredient<Lumenyl>(5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
