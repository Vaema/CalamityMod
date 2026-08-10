using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class OblivionCooldown : CooldownHandler
{
    public static new string ID => "OblivionCooldown";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Projectiles/Melee/Yoyos/OblivionYoyo";
    public override string OutlineTexture => "CalamityMod/Cooldowns/OblivionCooldownOutline";
    public override Color OutlineColor => Color.MediumVioletRed;
    public override Color CooldownStartColor => Color.DarkRed;
    public override Color CooldownEndColor => Color.Red;
}
