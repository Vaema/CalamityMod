using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.Localization;

namespace CalamityMod.Cooldowns
{
    public class Stooldown : CooldownHandler
    {
        public static new string ID => "Stooldown";
        public override bool ShouldDisplay => true;
        public override bool CanTickDown => true;
        public override void Tick()
        {
            base.Tick();
        }
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override string Texture => "CalamityMod/Cooldowns/Stooldown";
        public override Color OutlineColor => new Color(197, 165, 108);
        public override Color CooldownStartColor => new Color(144, 84, 29);
        public override Color CooldownEndColor => Color.Khaki;

        public override SoundStyle? EndSound => new("CalamityMod/Sounds/Item/AscendantOff");
    }
}
