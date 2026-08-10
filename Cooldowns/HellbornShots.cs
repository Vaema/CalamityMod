using System;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Cooldowns;

public class HellbornShots : CooldownHandler
{
    public float CompletionPercentage => MathF.Round(instance.Completion * 100);
    private bool IsEmpty => CompletionPercentage == 100;
    private float TextXOffset => -5.4f;
    private Vector2 TextPosition => new(TextXOffset, 15);
    private Color TextColor => Color.Gold;
    private Color TextBorderColor = Color.DarkRed;

    public static new string ID => "HellbornShots";
    public override bool CanTickDown => false;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override bool ShouldDisplay => instance.player.HeldItem.type == ItemType<Hellborn>();
    public override string Texture => "CalamityMod/Cooldowns/" + ID;

    public override Color CooldownStartColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.Red, Color.SlateGray, instance.Completion);
    public override Color CooldownEndColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.Gold, Color.SlateGray, instance.Completion);

    public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        base.DrawExpanded(spriteBatch, position, opacity, scale);

        CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.player.Calamity().hellbornShots.ToString(), position + TextPosition, TextColor, TextBorderColor);
    }

    public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        base.DrawCompact(spriteBatch, position, opacity, scale);

        CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.player.Calamity().hellbornShots.ToString(), position + TextPosition, TextColor, TextBorderColor);
    }
}
