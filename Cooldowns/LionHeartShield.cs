using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class LionHeartShield : CooldownHandler
{
    public static new string ID => "LionHeartShield";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/LionHeartShield";
    public override Color OutlineColor => new(232, 239, 239);
    public override Color CooldownStartColor => new(17, 242, 244);
    public override Color CooldownEndColor => Color.White;
}
