using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;

namespace CalamityMod.UI.DialogueDisplay.DisplayEffects;

public class WhisperingPearlEffects : DisplayEffect
{
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
