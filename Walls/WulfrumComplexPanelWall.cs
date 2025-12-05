using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class WulfrumComplexPanelWall : ModWall
    {
        internal static FramedMaskTexture GlowMask;

        public override void SetStaticDefaults()
        {
            GlowMask = new("CalamityMod/Walls/WulfrumComplexPanelWall_Glow", 36, 36);

            Main.wallHouse[Type] = true;
            AddMapEntry(new Color(28, 28, 29));
        }

        public override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }
        public static void DrawWallGlow(int wallType, int i, int j, SpriteBatch spriteBatch)
        {
            if (GlowMask.Texture is null)
                return;

            Tile tile = Main.tile[i, j];
            int xLength = 32;
            int xOff = 0;

            int xPos = tile.WallFrameX + xOff;
            int yPos = tile.WallFrameY;

            Rectangle frame = new Rectangle(xPos, yPos, xLength, 32);
            Color drawcolor;
            drawcolor = WorldGen.paintColor(tile.WallColor);
            drawcolor.A = 255;
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

            if (Main.drawToScreen)
                zero = Vector2.Zero;

            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
            Color lightColor = Lighting.GetColor(i, j, Color.White);

            spriteBatch.Draw(TextureAssets.Wall[wallType].Value, pos + new Vector2(-8 + xOff, -8), frame, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (GlowMask.HasContentInFramePos(xPos, yPos))
            {
                float brightness = MathHelper.Clamp(0.2f - (j / 680), 0f, 0.2f);

                float time = Main.GameUpdateCount;
                float waveScale1 = time * 0.064f;
                int scalar = i - (j / 2);
                float wave1 = waveScale1 * -50 + scalar * 12;
                float wave1angle = 0.30f + 0.25f * MathF.Sin(MathHelper.ToRadians(wave1));

                drawcolor *= brightness;

                float transparency = 0.02f + (wave1angle / 4);
                Color glowColor = Color.White * transparency;

                for (int k = 0; k < 3; k++)
                {
                    //Vector2 offset = new Vector2(Main.rand.NextFloat(-1, 1f), Main.rand.NextFloat(-1, 1f)) * 0.2f * k;
                    spriteBatch.Draw(GlowMask.Texture, pos + new Vector2(-8 + xOff, -8), frame, glowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            DrawWallGlow(Type, i, j, spriteBatch);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
