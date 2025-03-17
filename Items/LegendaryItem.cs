using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    // Since all legendary items do not use the same localization category, ILocalizedModType must be placed on each individual item file.
    public abstract class LegendaryItem : ModItem, IHoldShiftTooltipItem
    {
        // All legendary items use the same extension indicator tooltip.
        // This is given a null color, as it is colored manually.
        public string ExtensionIndicatorKey => "Items.Misc.LegendaryShortTooltip";
        public Color? ExtensionIndicatorColor => null;

        // All full flavor tooltips use the same localization key.
        // Flavor tooltip color defaults to none, but can be overriden as desired for each item.
        public string TooltipExtensionKey => "LegendaryText";
        public virtual Color? TooltipExtensionColor => null;
    }
}
