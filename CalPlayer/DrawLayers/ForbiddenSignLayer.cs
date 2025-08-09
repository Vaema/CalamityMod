using System;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.DrawLayers
{
    public class ForbiddenSignLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Skin);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            CalamityPlayer modPlayer = drawPlayer.Calamity();
            return drawInfo.shadow == 0f && !drawPlayer.dead && modPlayer.forbiddenCirclet;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            Color color = Color.Lerp(drawInfo.colorArmorBody, Color.White, 0.7f);
            Texture2D texture = TextureAssets.Extra[ExtrasID.ForbiddenSign].Value;
            Texture2D glowmask = TextureAssets.GlowMask[GlowMaskID.ForbiddenSign].Value;
            int offsetY = (int)(MathF.Sin(drawPlayer.miscCounter / 300f * MathHelper.TwoPi) * 6f);
            float shadowMult = MathF.Cos(drawPlayer.miscCounter / 75f * MathHelper.TwoPi) * 4f;
            Color afterimageColor = new Color(80, 70, 40, 0) * (shadowMult * 0.125f + 0.5f) * 0.8f;

            if (drawPlayer.ownedProjectileCounts[ModContent.ProjectileType<CircletTornado>()] > 1)
            {
                offsetY = 0;
                shadowMult = 2f;
                afterimageColor = new Color(80, 70, 40, 0) * 0.3f;
                color = color.MultiplyRGB(new Color(0.5f, 0.5f, 1f));
            }

            Vector2 drawPos = drawInfo.Position + drawPlayer.bodyPosition - Main.screenPosition;
            drawPos += new Vector2(drawPlayer.width * 0.5f - drawPlayer.direction * 10f, drawPlayer.height - drawPlayer.bodyFrame.Height * 0.5f + 4f + drawPlayer.gravDir * (offsetY - 20f));

            // Draw the original sign.
            DrawData drawData = new(texture, drawPos, null, color, drawPlayer.bodyRotation, texture.Size() * 0.5f, 1f, drawInfo.playerEffect)
            {
                shader = drawInfo.cBody
            };
            drawInfo.DrawDataCache.Add(drawData);

            // Draw 4 semi-transparent copies.
            Vector2 origin = texture.Size() * 0.5f;
            for (float i = 0f; i < 4f; i++)
            {
                float angle = MathHelper.PiOver2 * i;
                drawData = new(glowmask, drawPos + angle.ToRotationVector2() * shadowMult, null, afterimageColor, drawPlayer.bodyRotation, origin, 1f, drawInfo.playerEffect);
                drawInfo.DrawDataCache.Add(drawData);
            }
        }
    }
}
