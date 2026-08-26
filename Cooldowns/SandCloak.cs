using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class SandCloak : CooldownHandler
{
    public static new string ID => "SandCloak";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/SandCloak";
    public override Color OutlineColor => new(209, 176, 114);
    public override Color CooldownStartColor => new(100, 64, 44);
    public override Color CooldownEndColor => new(132, 95, 54);
}
