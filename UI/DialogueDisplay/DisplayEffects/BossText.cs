using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.UI.DialogueDisplay.DisplayEffects
{
    public class BossText : DisplayEffect
    {
        Vector2 lastPosition = Vector2.Zero;
        public override bool FadeWhenTooFar => false;

        public override Vector2 TextOffsetFromStart(Vector2 startPos, Vector2 textSize)
        {
            Vector2 playerPos = Main.LocalPlayer.Center;
            Vector2 halfSize = textSize * 0.5f;
            Vector2 newPos = playerPos - halfSize + (Vector2.UnitY * (textSize.Y + 54));

            Vector2 toBoss = (startPos - newPos);
            float dist = MathHelper.Clamp(toBoss.Length() / 100f, 0f, 36);
            toBoss = toBoss.SafeNormalize(Vector2.zeroVector);

            return newPos + (toBoss * dist);
        }

        public override void PreDraw(SpriteBatch spriteBatch, Vector2 textStart, Vector2 textSize, int textTimer, int switchTimer)
        {
            if (textTimer < 0)
                return;

            float Opacity = 1f;
            if (textTimer < 30f)
                Opacity = MathHelper.Lerp(0f, 1f, CalamityUtils.CircOutEasing(textTimer / 30f, 1));

            if (switchTimer > 0)
                Opacity *= 1 - CalamityUtils.CircOutEasing(switchTimer / 60f, 1);

            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/SmallBloom").Value;
            spriteBatch.Draw(tex, textStart + textSize * 0.5f - Main.screenPosition, null, Color.Black * 0.6f * Opacity, 0f, tex.Size() * 0.5f, new Vector2(textSize.X / 160f, textSize.Y / 120f), 0, 0);
        }
    }
}
