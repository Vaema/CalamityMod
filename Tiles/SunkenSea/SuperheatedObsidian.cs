using System;
using CalamityMod.Sounds;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SuperheatedObsidian : ModTile
    {
        public Asset<Texture2D> GlintTexture;

        public Vector2 GlintDir;

        public override void SetStaticDefaults()
        {
            GlintTexture = ModContent.Request<Texture2D>(Texture + "_Glint");

            GlintDir = new Vector2(1f, 1f);
            GlintDir.Normalize();

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

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            float transparency = 1f;

            // Must be set here 
            Main.tileBlockLight[Type] = false;

            Tile tile = Main.tile[i, j];
            Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

            Color color = Lighting.GetColor(i, j) * transparency;
            TileFramingSystem.SlopedGlowmask(in tile, i, j, TextureAssets.Tile[Type].Value, frame, CalamityUtils.ApplyPaint(tile.TileColor, color, false), default);

            //IF this glint effect below runs poorly on lower end PC's we should keep it as a setting for those with good PC's

            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + offScreen;

            Vector2 screenPos = position;

            float projection = Vector2.Dot(screenPos, GlintDir);

            // this sets the length between the glints diagonally 
            float screenDiagonalLength = Vector2.Dot(new Vector2(Main.screenWidth, Main.screenHeight), GlintDir);

            float stripeWidth = 100f;
            Color lightColor = Lighting.GetColor(i, j);

            DrawGlint(screenDiagonalLength * 0.53f);
            DrawGlint(screenDiagonalLength * 0.63f);
            DrawGlint(screenDiagonalLength * 0.73f);

            void DrawGlint(float beamCenter)
            {
                float dist = Math.Abs(projection - beamCenter);
                float strength = MathHelper.Clamp(1f - dist / stripeWidth, 0f, 1f) * 0.4f;

                if (strength > 0f)
                {
                    spriteBatch.Draw(GlintTexture.Value, position, frame, lightColor * strength, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }

            return false;
        }
    }
}
