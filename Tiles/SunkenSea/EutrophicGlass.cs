using System;
using CalamityMod.Systems;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{

    public class EutrophicGlass : ModTile
    {
        private static int sheetWidth = 216;
        private static int sheetHeight = 72;

        public static int TypeCache;

        public override void SetStaticDefaults()
        {
            TypeCache = Type;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeSmoothTiles(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            Main.tileLighted[Type] = true;
            Main.tileShine2[Type] = false;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.WallsMergeWith[Type] = true;
            DustType = 108;
            AddMapEntry(new Color(197, 220, 220));
            HitSound = SoundID.Shatter;
            MinPick = 55;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        private static float GetFade1(int i, int j)
        {
            return (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.2f) + 1f) / 2f;
        }

        private static float GetFade2(int i, int j)
        {
            return (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.1f + i * 0.08f - j * 0.05f) + 1f) / 2f;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].IsTileActuallyInvisible())
                return;

            float transparency = 0.4f;

            // Must be set here 
            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileNoSunLight[Type] = false;

            Texture2D tex = ModContent.Request<Texture2D>(Texture + "_Tile").Value;

            Tile tile = Main.tile[i, j];
            int xPos = i % 10;
            int yPos = j % 10;
            int frameXOffset = xPos * sheetWidth;
            int frameYOffset = yPos * sheetHeight;
            Rectangle frame = new Rectangle(tile.TileFrameX + frameXOffset, tile.TileFrameY + frameYOffset, 16, 16);

            Color color = Lighting.GetColor(i, j) * transparency;
            TileFramingSystem.SlopedGlowmask(in tile, i, j, tex, frame, CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, color, false), default);

            //IF this glint effect below runs poorly on lower end PC's we should keep it as a setting for those with good PC's

            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + offScreen;
            Texture2D GlintTex = ModContent.Request<Texture2D>(Texture + "_Glint").Value;

            Vector2 glintDir = new Vector2(1f, 1f);
            glintDir.Normalize();

            Vector2 screenPos = position;

            float projection = Vector2.Dot(screenPos, glintDir);

            // this sets the length between the glints diagonally 
            float screenDiagonalLength = Vector2.Dot(new Vector2(Main.screenWidth, Main.screenHeight), glintDir);
            float[] beamCenters = new float[]
            {
              screenDiagonalLength * 0.53f, // upper glint
              screenDiagonalLength * 0.63f,  // middle glint
              screenDiagonalLength * 0.73f  // lower glint
            };

            float stripeWidth = 100f;

            foreach (float bc in beamCenters)
            {
                float dist = Math.Abs(projection - bc);
                float strength = MathHelper.Clamp(1f - dist / stripeWidth, 0f, 1f);

                if (strength > 0f)
                {
                    spriteBatch.Draw(GlintTex, position, frame, (Lighting.GetColor(i, j) * 2 ) * (strength * 0.4f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFramingSystem.CompactFraming(i, j, resetFrame);
            return false;
        }
    }
}
