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
    public class SuperheatedObsidian : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;

            //Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = true;

            HitSound = CommonCalamitySounds.VoidstoneMine;
            DustType = DustID.Slush;
            AddMapEntry(new Color(67, 61, 91));

            this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
            this.RegisterBlendMergeWith(ModContent.TileType<AbyssGravel>());
            this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
            this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
            this.RegisterBlendMergeWith(ModContent.TileType<Runestone>());
            this.RegisterBlendMergeWith(ModContent.TileType<Basalt>());

            this.RegisterBlendMergeWith(TileID.Sandstone);
            this.RegisterBlendMergeWith(TileID.Sand);
            this.RegisterBlendMergeWith(TileID.HardenedSand);
            this.RegisterBlendMergeWith(TileID.Stone);
            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Ash);
            this.RegisterBlendMergeWith(TileID.Mud);
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
            float transparency = 1f;

            // Must be set here 
            Main.tileBlockLight[Type] = false;

            Texture2D tex = ModContent.Request<Texture2D>(Texture + "_Tile").Value;

            Tile tile = Main.tile[i, j];
            int xPos = i % 1;
            int yPos = j % 1;
            int frameXOffset = xPos * 1;
            int frameYOffset = yPos * 1;
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
                    spriteBatch.Draw(GlintTex, position, frame, (Lighting.GetColor(i, j)) * (strength * 1f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }
}
