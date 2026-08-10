using System;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using static CalamityMod.Items.Accessories.DaawnlightSpiritOrigin;

namespace CalamityMod.Cooldowns;

public class DaawnlightSpiritOriginExtraCrit : CooldownHandler
{
    public static new string ID => "DSOCritCooldown";

    // If the player doesn't have DSO, this cooldown should not appear.
    public override bool ShouldDisplay => instance.player.GetModPlayer<CalamityPlayer>().spiritOrigin;

    private float ExtraCritChance => Math.Min(instance.player.GetModPlayer<CalamityPlayer>().spiritOriginCritBoost, CritHardCap);

    private float TextXOffset => (ExtraCritChance > 99 ? -24 : ExtraCritChance > 9 ? -20 : -16) * TextScale;
    private Vector2 TextPosition => new(TextXOffset , 25);
    private Color TextColor => Color.Lerp(Color.White , Color.Tomato , Utils.GetLerpValue(CritDecayThreshold - 10 , CritDecayThreshold , instance.player.GetModPlayer<CalamityPlayer>().spiritOriginCritBoost , false));
    private Color TextBorderColor = Color.Black;
    private float TextScale => Utils.Remap(ExtraCritChance, 0, CritDecayThreshold, 1f, 1.5f);

    public override bool CanTickDown => false;

    public override Color CooldownStartColor => new(Main.DiscoR, Main.DiscoG, Main.DiscoB);

    public override Color CooldownEndColor => new(Main.DiscoB, Main.DiscoR, Main.DiscoG);

    public override string Texture => "CalamityMod/Cooldowns/" + ID;

    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");

    public override void DrawExpanded(SpriteBatch spriteBatch , Vector2 position , float opacity , float scale)
    {
        base.DrawExpanded(spriteBatch , position , opacity , scale);
        CalamityUtils.DrawBorderStringEightWay(spriteBatch , FontAssets.MouseText.Value , $"+{ExtraCritChance}%" , position + TextPosition , TextColor , TextBorderColor , TextScale);
    }

    public override void DrawCompact(SpriteBatch spriteBatch , Vector2 position , float opacity , float scale)
    {
        base.DrawCompact(spriteBatch , position , opacity , scale);
        CalamityUtils.DrawBorderStringEightWay(spriteBatch , FontAssets.MouseText.Value , $"+{ExtraCritChance}%" , position + TextPosition , TextColor , TextBorderColor , TextScale);
    }
}
