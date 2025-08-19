using System;
using CalamityMod.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace CalamityMod.Walls
{
    public class SeaPrismWall : MultiVariantModWall
    {
        internal static FramedMaskTexture GlowMaskBlue;
        internal static FramedMaskTexture GlowMaskPurple;
        internal static FramedMaskTexture GlowMaskGreen;
        public override void SetStaticDefaults()
        {
            GlowMaskBlue = new("CalamityMod/Walls/SeaPrismWall_Blue", 36, 36);
            GlowMaskPurple = new("CalamityMod/Walls/SeaPrismWall_Purple", 36, 36);
            GlowMaskGreen = new("CalamityMod/Walls/SeaPrismWall_Green", 36, 36);
            Main.wallHouse[Type] = true;
            DustType = 108;

            AddMapEntry(new Color(11, 56, 81));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = (i % 8) * 468;
            frameYOffset = (j % 8) * 180;
        }
        private static float GetFade1(int i, int j) =>
           (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.2f) + 1f) / 2f;

        private static float GetFade2(int i, int j) =>
            (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.1f + i * 0.08f - j * 0.05f) + 1f) / 2f;

        public static void DrawWallGlow(int wallType, int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            int xLength = 32;

            int frameXOffset = (i % 8) * 468;
            int frameYOffset = (j % 8) * 180;

            int xPos = tile.WallFrameX + frameXOffset;
            int yPos = tile.WallFrameY + frameYOffset;

            Rectangle frame = new Rectangle(xPos, yPos, xLength, 32);
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;

            // base wall
            spriteBatch.Draw(TextureAssets.Wall[wallType].Value, pos + new Vector2(-8, -8), frame, Lighting.GetColor(i, j));

            // glow layers
            float fade1 = GetFade1(i, j);
            float fade2 = GetFade2(i, j);

            if (GlowMaskBlue.HasContentInFramePos(xPos, yPos))
            {
                spriteBatch.Draw(GlowMaskBlue.Texture, pos + new Vector2(-8, -8), frame, (Color.White * 1.1f));
            }
            if (GlowMaskPurple.HasContentInFramePos(xPos, yPos))
            {
                spriteBatch.Draw(GlowMaskPurple.Texture, pos + new Vector2(-8, -8), frame, Color.White * (fade1 * 1.1f));
            }
            if (GlowMaskGreen.HasContentInFramePos(xPos, yPos))
            {
                spriteBatch.Draw(GlowMaskGreen.Texture, pos + new Vector2(-8, -8), frame, Color.White * (fade2 * 1.5f));
            }

            Color wallColor = Lighting.GetColor(i, j) * 1.2f; // brighten a bit
            wallColor.A = 255;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            DrawWallGlow(Type, i, j, spriteBatch);
            return false;
        }
    }
}
