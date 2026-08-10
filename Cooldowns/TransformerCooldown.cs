using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class TransformerCooldown : CooldownHandler
{
    public static new string ID => "TransformerCooldown";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/TransformerCooldown";
    public override Color OutlineColor => Color.AliceBlue;
    public override Color CooldownStartColor => Color.Lerp(Color.SlateBlue, Color.DodgerBlue, 1 - instance.Completion);
    public override Color CooldownEndColor => Color.Lerp(Color.SlateGray, Color.Cyan, 1 - instance.Completion);
}
