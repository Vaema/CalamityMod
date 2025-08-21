using System;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SeaPrism : ModTile
    {
        private const short subsheetWidth = 468;
        private const short subsheetHeight = 90;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            TileID.Sets.HasSlopeFrames[Type] = true;
            Main.tileMerge[Type][ModContent.TileType<MediumSeaPrismCrystal>()] = true;

            Main.tileMerge[Type][ModContent.TileType<SeaPrismCrystals>()] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);
            Main.tileLighted[Type] = true;
            Main.tileShine2[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            DustType = DustID.Water;
            AddMapEntry(new Color(97, 212, 223));
            HitSound = SoundID.Tink;
            Main.tileSpelunker[Type] = true;
            MinPick = 55;

            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = i % 8 * subsheetWidth;
            frameYOffset = j % 8 * subsheetHeight;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float fade1 = GetFade1(i, j);
            float fade2 = GetFade2(i, j);

            Color baseColor = new Color(162, 216, 218); //Blue
            Color glow1 = new Color(171, 113, 215);    //Purple
            Color glow2 = new Color(56, 174, 117);     //Green

            Vector3 blended = baseColor.ToVector3();

            blended = Vector3.Lerp(blended, glow1.ToVector3(), fade1 * 0.5f);
            blended = Vector3.Lerp(blended, glow2.ToVector3(), fade2 * 0.5f);

            float brightness = 0.6f; 
            blended *= brightness;

            r = blended.X;
            g = blended.Y;
            b = blended.Z;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }

        private static float GetFade1(int i, int j)
        {
            return (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.2f) + 1f) / 2f;
        }

        private static float GetFade2(int i, int j)
        {
            return (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.1f + i * 0.08f - j * 0.05f) + 1f) / 2f;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + offScreen;

            int frameX = tile.TileFrameX + (i % 8 * subsheetWidth);
            int frameY = tile.TileFrameY + (j % 8 * subsheetHeight);

            Rectangle sourceRect = new Rectangle(frameX, frameY, 16, 16);

            Texture2D baseTex = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_Blue").Value;
            spriteBatch.Draw(baseTex, position, sourceRect, Lighting.GetColor(i, j) * 1.5f);

            Texture2D tex1 = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_Purple").Value;
            Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_Green").Value;

            spriteBatch.Draw(tex1, position, sourceRect, Lighting.GetColor(i, j) * 1.5f * GetFade1(i, j));
            spriteBatch.Draw(tex2, position, sourceRect, Lighting.GetColor(i, j) * 1.5f * GetFade2(i, j));

            //IF this glint effect below runs poorly on lower end PC's we should keep it as a setting for those with good PC's

            Texture2D GlintTex = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_GlintMask").Value;

            Vector2 glintDir = new Vector2(1f, -1f);
            glintDir.Normalize();

            Vector2 screenPos = position;

            float projection = Vector2.Dot(screenPos, glintDir);

            // this sets the length between the glints diagonally 
            float screenDiagonalLength = Vector2.Dot(new Vector2(Main.screenWidth, Main.screenHeight), glintDir);
            float[] beamCenters = new float[]
            {
              screenDiagonalLength * 0.05f, // upper glint
              screenDiagonalLength * 0.5f,  // middle glint
              screenDiagonalLength * 1.05f  // lower glint
            };

            float stripeWidth = 100f;

            foreach (float bc in beamCenters)
            {
             float dist = Math.Abs(projection - bc);
             float strength = MathHelper.Clamp(1f - dist / stripeWidth, 0f, 1f);

                if (strength > 0f)
              {
                    spriteBatch.Draw(GlintTex, position, sourceRect, Color.White * strength, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
}
            return true;

        }
    }
}
