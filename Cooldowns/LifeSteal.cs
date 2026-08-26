using System;
using CalamityMod.Balancing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;

namespace CalamityMod.Cooldowns;

public class LifeSteal : CooldownHandler
{
    public static new string ID => "LifeSteal";

    private float lifeStealCap => Main.expertMode ? BalancingConstants.LifeStealCap_Expert : BalancingConstants.LifeStealCap_Classic;
    public override bool ShouldDisplay => instance.player.lifeSteal < lifeStealCap;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/LifeSteal";

    public override bool CanTickDown => false;
    public override Color OutlineColor => instance.player!.lifeSteal < 0
        ? new Color(255, 142, 165)
        : new Color(255, 142, 165);
    public override Color CooldownStartColor => instance.player!.lifeSteal < 0
        ? new Color(145, 59, 59)
        : new Color(255, 181, 181);
    public override Color CooldownEndColor => CooldownStartColor;
    private Color TextColor => Color.White;
    private Color TextBorderColor => new(40, 0, 0);
    public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        base.DrawExpanded(spriteBatch, position, opacity, scale);
        var value = instance.player.lifeSteal;
        bool negate = value < 0;
        var valueToMeasure = Math.Abs(value);
        float Xoffset = valueToMeasure > 9 ? valueToMeasure > 99 ? -12.5f : -10f : -5;
        if (negate)
            Xoffset -= 8;
        CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, (value).ToString("#"), position + new Vector2(Xoffset, 8) * scale, TextColor, TextBorderColor, scale);
    }

    public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        base.DrawCompact(spriteBatch, position, opacity, scale);
        var value = instance.player.lifeSteal;
        bool negate = value < 0;
        var valueToMeasure = Math.Abs(value);
        float Xoffset = valueToMeasure > 9 ? valueToMeasure > 99 ? -12.5f : -10f : -5;
        if (negate)
            Xoffset -= 8;
        CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, (value).ToString("#"), position + new Vector2(Xoffset, 8) * scale, TextColor, TextBorderColor, scale);
    }
}
