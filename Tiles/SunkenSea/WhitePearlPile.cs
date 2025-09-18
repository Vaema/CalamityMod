using System;
using CalamityMod.Sounds;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class WhitePearlPile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;

            Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = true;

            HitSound = CommonCalamitySounds.VoidstoneMine;
            DustType = DustID.Slush;
            AddMapEntry(new Color(217, 209, 213));

            TileID.Sets.CanBeDugByShovel[Type] = true;

            this.RegisterUniversalMerge(ModContent.TileType<Shellstone>(), "CalamityMod/Tiles/Merges/ShellstoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<AbyssGravel>(), "CalamityMod/Tiles/Merges/AbyssGravelMerge");
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Runestone>(), "CalamityMod/Tiles/Merges/RunestoneMerge");
            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
            this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            this.RegisterUniversalMerge(TileID.Ash, "CalamityMod/Tiles/Merges/AshMerge");
            this.RegisterUniversalMerge(TileID.Mud, "CalamityMod/Tiles/Merges/MudMerge");
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
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //IF this glint effect below runs poorly on lower end PC's we should keep it as a setting for those with good PC's

            Tile tile = Framing.GetTileSafely(i, j);
            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + offScreen;

            int frameX = tile.TileFrameX + (i % 1);
            int frameY = tile.TileFrameY + (j % 1);

            Rectangle sourceRect = new Rectangle(frameX, frameY, 16, 16);

            Texture2D GlintTex = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/WhitePearlPile_Glint").Value;

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
                    spriteBatch.Draw(GlintTex, position, sourceRect, (Lighting.GetColor(i, j) * 6) * (strength * 0.4f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // figure out if the tile is exposed - taken from the coral tiles
            if (Main.tile[i, j] == null ||
                (Main.tile[i - 1, j].HasTile && Main.tile[i + 1, j].HasTile && Main.tile[i, j - 1].HasTile && Main.tile[i, j + 1].HasTile))
                return;

            // get in-game lighting color at this tile
            Color litColor = Lighting.GetColor(i, j);

            // convert to brightness factor (0 = dark, 1 = full bright)
            float brightness = (litColor.R + litColor.G + litColor.B) / (3f * 255f);

            // flip it so the glow is strongest in the dark
            float darknessFactor = 1f - brightness;

            // diagonal glint math
            Vector2 glintDir = new Vector2(1f, -1f);
            glintDir.Normalize();

            Vector2 screenPos = new Vector2(i * 16, j * 16) - Main.screenPosition;
            float projection = Vector2.Dot(screenPos, glintDir);
            float screenDiagonalLength = Vector2.Dot(new Vector2(Main.screenWidth, Main.screenHeight), glintDir);

            float[] beamCenters =
            {
             screenDiagonalLength * 0.05f,
             screenDiagonalLength * 0.5f,
             screenDiagonalLength * 1.05f
          };

            float stripeWidth = 100f;
            float maxStrength = 0f;

            foreach (float bc in beamCenters)
            {
                float dist = Math.Abs(projection - bc);
                float strength = MathHelper.Clamp(1f - dist / stripeWidth, 0f, 1f);
                if (strength > maxStrength)
                    maxStrength = strength;
            }

            if (maxStrength > 0f)
            {
                // base intensity scaled by glint strength and darkness (min = 0.0f, max = 1.0f)
                float intensity = 0.6f * maxStrength * (0.5f + darknessFactor * 0.5f);
                //float intensity = ((float)(Math.Pow(10.0, (brightness * 2 - 2))) - 0.01f) / 0.99f; was trying to do something cool, may come back to it later


                r = (185f / 255f) * intensity;
                g = (164f / 255f) * intensity;
                b = (184f / 255f) * intensity;
            }
        }
    }
}
