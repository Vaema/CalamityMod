using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class SpeedBlasterBoost : CooldownHandler
{
    public static new string ID => "SpeedBlasterBoost";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/SpeedBlasterBoost";
    public override Color OutlineColor => new(207, 207, 207);
    public override Color CooldownStartColor => new(235, 33, 130);
    public override Color CooldownEndColor => new(39, 227, 208);
}
