using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class PanaceaCooldown : CooldownHandler
{
    public static new string ID => "Panacea";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/PanaceaCooldown";
    public override Color OutlineColor => new Color(122, 198, 255);
    public override Color CooldownStartColor => new Color(0, 128, 255);
    public override Color CooldownEndColor => new Color(0, 255, 255);
    public override SoundStyle? EndSound => new("CalamityMod/Sounds/Custom/AbilitySounds/PotionSicknessOver");
}
