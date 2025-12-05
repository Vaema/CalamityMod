using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;

namespace CalamityMod.Cooldowns
{
    public class WarbanneroftheRighteousBuff : CooldownHandler
    {
        public float CompletionPercentage => Utils.GetLerpValue(70, 0, instance.timeLeft);
        private bool IsEmpty => CompletionPercentage == 0;
        private float TextXOffset => instance.timeLeft <= 20 ? -11f : -18f;
        private Vector2 TextPosition => new(TextXOffset, 15);
        private Color TextColor => Color.White;
        private Color TextBorderColor => Color.Black;

        public static new string ID => "WarbanneroftheRighteousBuff";
        public override bool CanTickDown => false;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override bool ShouldDisplay => instance.player.Calamity().WarbanneroftheRighteous;
        public override string Texture => "CalamityMod/Cooldowns/" + ID;

        public override Color CooldownStartColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.SlateGray, Color.DarkGoldenrod, CompletionPercentage);
        public override Color CooldownEndColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.SlateGray, Color.Gold, CompletionPercentage);

        public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawExpanded(spriteBatch, position, opacity, scale);

            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, (-instance.timeLeft + 15).ToString() + "%", position + TextPosition, TextColor, TextBorderColor);
        }

        public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawCompact(spriteBatch, position, opacity, scale);

            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, (-instance.timeLeft + 15).ToString() + "%", position + TextPosition, TextColor, TextBorderColor);
        }
    }
}
