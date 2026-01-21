using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns
{
    public class HolyProtection : CooldownHandler
    {
        public static new string ID => "HolyProtection";

        public override bool ShouldDisplay => CalamityClientConfig.Instance.VanillaCooldownDisplay;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override string Texture => "CalamityMod/Cooldowns/HolyProtection";
        public override Color OutlineColor => Color.Orange;
        public override Color CooldownStartColor => Color.Lerp(Color.Yellow, Color.White, 1 - instance.Completion);
        public override Color CooldownEndColor => Color.Lerp(Color.Yellow, Color.White, 1 - instance.Completion);
    }
}
