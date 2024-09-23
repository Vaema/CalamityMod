using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.UI
{
    public class PunchCardGUI : PopupGUI
    {
        public bool HoveringOverBook = false;

        public override void Update()
        {
            if (Active)
            {
                if (FadeTime < FadeTimeMax)
                    FadeTime++;
            }
            else if (FadeTime > 0)
            {
                FadeTime--;
            }

            if (Main.mouseLeft && !HoveringOverBook && FadeTime >= 30)
            {
                Active = false;
            }

            HoveringOverBook = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pageTexture = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/PunchCardPopup").Value;
            float xScale = MathHelper.Lerp(0.004f, 1f, FadeTime / (float)FadeTimeMax);
            Vector2 scale = new Vector2(xScale, 1f) * new Vector2(Main.screenWidth, Main.screenHeight) / pageTexture.Size();
            scale.Y *= 1.5f;
            scale *= 0.5f;

            float bookScale = 0.5f;
            scale *= bookScale;

            float yPageTop = MathHelper.Lerp(Main.screenHeight * 2, Main.screenHeight * 0.5f, FadeTime / (float)FadeTimeMax);

            Rectangle mouseRectangle = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 2, 2);
            float drawPositionX = Main.screenWidth * 0.5f;
            Vector2 drawPosition = new Vector2(drawPositionX, yPageTop);
            Rectangle pageRectangle = new Rectangle((int)drawPosition.X - (int)(pageTexture.Width * 0.5f * scale.X), (int)drawPosition.Y - (int)(pageTexture.Height * 0.5f * scale.Y), (int)(pageTexture.Width * scale.X), (int)(pageTexture.Height * scale.Y));
            
            spriteBatch.Draw(pageTexture, drawPosition, null, Color.White, 0f, pageTexture.Size() / 2, scale, SpriteEffects.None, 0f);

            if (!HoveringOverBook)
                HoveringOverBook = mouseRectangle.Intersects(pageRectangle);            
        }
    }
}
