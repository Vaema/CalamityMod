using CalamityMod.Graphics.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.DrawLayers
{
    public class V8000CowcatcherLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.MountFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            CalamityPlayer modPlayer = drawPlayer.Calamity();
            return false;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/CalPlayer/DrawLayers/V8000Cowcatcher").Value;

            // Spitball an offset for the sprite that may or may not look right
            Vector2 pos = (drawInfo.drawPlayer.MountedCenter - Main.screenPosition).Floor();
            Vector2 offset = new Vector2(0f * drawInfo.drawPlayer.direction, -22f);
            pos += offset;

            SpriteEffects effects = drawInfo.drawPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            drawInfo.DrawDataCache.Add(new DrawData(texture, pos, null, drawInfo.colorArmorBody, drawInfo.drawPlayer.fullRotation, texture.Size() * 0.5f, 1f, effects, 0));
        }
    }
}
