using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Items;

// Note the lack of ILocalizedModType.
// Unfortunately, since all exhumed items do not use the same localization category, ILocalizedModType must be placed on each individual item file.
public abstract class ExhumedItem : ModItem, IHoldShiftTooltipItem
{
    // All exhumed items use the same extension indicator tooltip.
    // This is given a null color, as it is colored manually.
    public string ExtensionIndicatorKey => "Items.Misc.ExhumeShortTooltip";
    public Color? ExtensionIndicatorColor => null;

    // All full flavor tooltips use the same localization key.
    // All full tooltips also use the same color: #C61B40. This is the standard tooltip color for Calamitas text.
    public string TooltipExtensionKey => "Calamitas";
    public Color? TooltipExtensionColor => new(198, 27, 64);
}
