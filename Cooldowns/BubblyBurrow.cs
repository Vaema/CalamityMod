using System;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Cooldowns
{
    public class BubblyBurrow : CooldownHandler
    {
        public bool PowerActive => instance.timeLeft > VictideHeadBurrow.BurrowCooldown;
        public float PowerPercent => (instance.timeLeft - VictideHeadBurrow.BurrowCooldown) / (float)VictideHeadBurrow.BurrowDuration;

        public static new string ID => "BubblyBurrow";
        public override bool ShouldDisplay => true;
        public override LocalizedText DisplayName => CalamityUtils.GetText("UI.Cooldowns.BubblyBurrow" + (PowerActive ? "Active" : "Cooldown"));
        public override string Texture => PowerActive ? "CalamityMod/Cooldowns/BubblyBurrow" : "CalamityMod/Cooldowns/BubblyBurrowCooldown";
        public override string OutlineTexture => "CalamityMod/Cooldowns/BubblyBurrowOutline";
        public override string OverlayTexture => "CalamityMod/Cooldowns/BubblyBurrowOverlay";
        public override Color OutlineColor => new Color(158, 158, 255);
        public override Color CooldownStartColor => PowerActive ? Color.Lerp(new Color(255, 170, 204), new Color(72, 125, 204), PowerPercent) : new Color(72, 125, 204);
        public override Color CooldownEndColor => PowerActive ? Color.Lerp(new Color(255, 170, 204), new Color(97, 255, 255), PowerPercent) : new Color(97, 255, 255);
        public override SoundStyle? EndSound => SoundID.Item85;

        public override void OnCompleted()
        {
            Vector2 playerVelocity = instance.player.velocity / 8f;
            if (!Main.dedServ)
            {
                for (int i = 0; i < 16; i++)
                {
                    Vector2 bubblePos = instance.player.MountedCenter + Main.rand.NextVector2Circular(50f, 50f);
                    Vector2 bubbleSpeed = Main.rand.NextVector2Circular(1f, 1f) + playerVelocity;
                    Gore bubble = Gore.NewGoreDirect(instance.player.GetSource_Misc("1"), bubblePos, bubbleSpeed, 411, Main.rand.NextFloat(0.8f, 1.6f));
                    bubble.timeLeft = 24 + Main.rand.Next(13);
                    bubble.type = 411;
                }
            }
        }

        //Charge down at first, and then charge back up
        private float AdjustedCompletion => CooldownRackUI.DebugFullDisplay ? CooldownRackUI.DebugForceCompletion : PowerActive ? PowerPercent : 1 - (instance.timeLeft / (float)VictideHeadBurrow.BurrowCooldown);

        public override void ApplyBarShaders(float opacity)
        {
            //Use the adjusted completion
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseSaturation(AdjustedCompletion);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseColor(CooldownStartColor);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseSecondaryColor(CooldownEndColor);
            GameShaders.Misc["CalamityMod:CircularBarShader"].Apply();
        }

        public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            Texture2D sprite = Request<Texture2D>(Texture).Value;
            Texture2D outline = Request<Texture2D>(OutlineTexture).Value;
            Texture2D overlay = Request<Texture2D>(OverlayTexture).Value;

            //Draw the outline
            spriteBatch.Draw(outline, position, null, OutlineColor * opacity, 0, outline.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            //Draw the icon
            spriteBatch.Draw(sprite, position, null, Color.White * opacity, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            //Draw the small overlay
            int lostHeight = (int)Math.Ceiling(overlay.Height * AdjustedCompletion);
            Rectangle crop = new Rectangle(0, lostHeight, overlay.Width, overlay.Height - lostHeight);
            spriteBatch.Draw(overlay, position + Vector2.UnitY * lostHeight * scale, crop, OutlineColor * opacity * 0.9f, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
    }
}
