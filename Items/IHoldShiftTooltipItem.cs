using Microsoft.Xna.Framework;

namespace CalamityMod.Items
{
    // This interface is used on items where you can hold Left SHIFT for an expanded tooltip.
    // All actual implementation of this behavior is done in CalamityGlobalItemTooltip.
    public interface IHoldShiftTooltipItem
    {
        /// <summary>
        /// Internal ID for the TooltipLine which contains Calamity items' "Hold SHIFT" extension indicators.
        /// </summary>
        internal const string ExtensionIndicatorTooltipID = "CalamityMod:HoldShiftExtensionIndicator";
        
        /// <summary>
        /// The lang/localization key of the default tooltip extension indicator message.
        /// </summary>
        public const string DefaultExtensionIndicatorKey = "UI.HoldShiftTooltipExtensionIndicator";

        /// <summary>
        /// The default color of the default tooltip extension indicator message.
        /// </summary>
        public static readonly Color DefaultExtensionIndicatorColor = new Color(184, 184, 184); // #B8B8B8
        
        /// <summary>
        /// The default lang/localization key which is queried to show an item's tooltip extension.
        /// </summary>
        public const string DefaultTooltipExtensionKey = "HoldShiftTooltip";

        /// <summary>
        /// Whether holding Left SHIFT for the tooltip extension hides the normal tooltip. Typically this is false.
        /// </summary>
        public virtual bool HidesNormalTooltip => false;

        /// <summary>
        /// Whether the "Hold SHIFT for more" extension indicator appears by default. Should essentially always be true.<br />
        /// You can set this to false for "secret" flavor or easter egg tooltips, like the one on Midas Prime.
        /// </summary>
        public virtual bool ShowExtensionIndicator => true;

        /// <summary>
        /// The lang/localization key of this item's tooltip extension indicator message.
        /// </summary>
        public virtual string ExtensionIndicatorKey => DefaultExtensionIndicatorKey;

        /// <summary>
        /// The color of this item's tooltip extension indicator message. If set to <b>null</b>, the indicator will not be colored.
        /// </summary>
        public virtual Color? ExtensionIndicatorColor => DefaultExtensionIndicatorColor;

        /// <summary>
        /// The lang/localization key which this item uses for its tooltip extension.
        /// </summary>
        public virtual string TooltipExtensionKey => DefaultTooltipExtensionKey;

        /// <summary>
        /// The color of this item's tooltip extension. If set to <b>null</b>, the tooltip extension will not be colored.
        /// </summary>
        public virtual Color? TooltipExtensionColor => null;

    }
}
