using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Localization;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Cooldowns
{
    public class SpeedBlasterBoost : CooldownHandler
    {
        public static new string ID => "SpeedBlasterBoost";
        public override bool ShouldDisplay => true;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override string Texture => "CalamityMod/Cooldowns/SpeedBlasterBoost";
        public override Color OutlineColor => new Color(207, 207, 207);
        public override Color CooldownStartColor => new Color(235, 33, 130);
        public override Color CooldownEndColor => new Color(39, 227, 208);
    }
}
