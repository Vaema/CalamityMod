using System;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee.Shortswords;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Cooldowns
{
    public class LucreciaEnergy : CooldownHandler
    {
        private float AdjustedCompletion => instance.timeLeft / (float)Lucrecia.MaxEnergy;
        private Color TextColor => Color.MediumPurple;
        private Color TextBorderColor = Color.AntiqueWhite;

        public static new string ID => "LucreciaEnergy";
        public override bool CanTickDown => false;
        public override bool ShouldDisplay => instance.player.HeldItem.type == ItemType<Lucrecia>();
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override string Texture => $"CalamityMod/Cooldowns/{ID}";
        public override string OutlineTexture => $"CalamityMod/Cooldowns/{ID}Outline";
        public override string OverlayTexture => $"CalamityMod/Cooldowns/{ID}Overlay";

        public override Color OutlineColor => Color.SlateGray;
        public override Color CooldownStartColor => Color.Lerp(Color.Purple, Color.SlateGray, instance.Completion);
        public override Color CooldownEndColor => Color.Lerp(Color.DarkBlue, Color.SlateGray, instance.Completion);

        public override void ApplyBarShaders(float opacity)
        {
            // Use the adjusted completion
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseSaturation(AdjustedCompletion);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseColor(CooldownStartColor);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseSecondaryColor(CooldownEndColor);
            GameShaders.Misc["CalamityMod:CircularBarShader"].Apply();
        }

        public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawExpanded(spriteBatch, position, opacity, scale);

            Vector2 textOffset = new Vector2(instance.timeLeft > 9 ? -11f : -6f, 10f);
            DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.timeLeft.ToString(), position + textOffset * scale, TextColor, TextBorderColor, scale);
        }

        public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            Texture2D sprite = Request<Texture2D>(Texture).Value;
            Texture2D outline = Request<Texture2D>(OutlineTexture).Value;
            Texture2D overlay = Request<Texture2D>(OverlayTexture).Value;

            // Draw the outline
            spriteBatch.Draw(outline, position, null, OutlineColor * opacity, 0, outline.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            // Draw the icon
            spriteBatch.Draw(sprite, position, null, Color.White * opacity, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            // Draw the small overlay
            int lostHeight = (int)Math.Ceiling(overlay.Height * AdjustedCompletion);
            Rectangle crop = new Rectangle(0, lostHeight, overlay.Width, overlay.Height - lostHeight);
            spriteBatch.Draw(overlay, position + Vector2.UnitY * lostHeight * scale, crop, OutlineColor * opacity * 0.9f, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            Vector2 textOffset = new Vector2(instance.timeLeft > 9 ? -11f : -6f, 10f);
            DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.timeLeft.ToString(), position + textOffset * scale, TextColor, TextBorderColor, scale);
        }
    }
}
