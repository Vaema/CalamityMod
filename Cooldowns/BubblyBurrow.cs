using System;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Particles;
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
        public override string Texture => "CalamityMod/Cooldowns/BubblyBurrow";
        public override string OutlineTexture => "CalamityMod/Cooldowns/BubblyBurrowOutline";
        public override string OverlayTexture => "CalamityMod/Cooldowns/BubblyBurrowOverlay";
        public override Color OutlineColor => new Color(158, 158, 255);
        public override Color CooldownStartColor => PowerActive ? Color.Lerp(new Color(72, 125, 204), new Color(97, 200, 255), PowerPercent) : new Color(72, 125, 204);
        public override Color CooldownEndColor => PowerActive ? Color.Lerp(new Color(72, 125, 204), new Color(97, 200, 255), PowerPercent) : new Color(97, 200, 255);
        public override SoundStyle? EndSound => SoundID.Item85;

        public override void OnCompleted()
        {
            // need to have proper visuals
            // currently not a lot of ideas rn
            Vector2 playerVelocity = instance.player.velocity / 8f;
            Vector2 particleGravity = Vector2.UnitY * 0.03f;
            for (int i = 0; i < 16; i++)
            {
                Vector2 dustDisplace = Main.rand.NextVector2Circular(80f, 50f);
                Vector2 dustPosition = instance.player.MountedCenter + dustDisplace;
                Vector2 dustSpeed = Main.rand.NextVector2Circular(0.5f, 0.5f) + playerVelocity - Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * 0.06f;
                dustSpeed.X += 1.4f * (float)Math.Sin(((dustDisplace.X + 80f) / 160f) * MathHelper.Pi) * (Main.rand.NextBool() ? -1 : 1);
                Particle dust = new SandyDustParticle(dustPosition, dustSpeed, Color.White, Main.rand.NextFloat(0.7f, 1.2f), Main.rand.Next(20, 50), 0.03f, particleGravity);
                GeneralParticleHandler.SpawnParticle(dust);
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
