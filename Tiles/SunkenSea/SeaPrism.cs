using System;
using CalamityMod.Effects;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SeaPrism : ModTile
    {
        internal const short subsheetWidth = 468;
        internal const short subsheetHeight = 90;

        internal static Asset<Texture2D> Blue;
        internal static Asset<Texture2D> Purple;
        internal static Asset<Texture2D> Green;
        internal static Asset<Texture2D> Glint;

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

            Blue = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_Blue");
            Purple = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_Purple");
            Green = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_Green");
            Glint = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrism_GlintMask");
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

        internal static float GetFade1(int i, int j) => (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.2f) + 1f) / 2f;

        internal static float GetFade2(int i, int j) => (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.1f + i * 0.08f - j * 0.05f) + 1f) / 2f;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;
    }

    public class SeaPrismShaderDrawing : ModSystem
    {
        public override void OnModLoad()
        {
            On_Main.DrawTiles += DrawSeaPrisms;
        }

        private void GetScreenDrawArea(Vector2 screenPosition, Vector2 offSet, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY)
        {
            firstTileX = (int)((screenPosition.X - offSet.X) / 16f - 1f);
            lastTileX = (int)((screenPosition.X + (float)Main.screenWidth + offSet.X) / 16f) + 2;
            firstTileY = (int)((screenPosition.Y - offSet.Y) / 16f - 1f);
            lastTileY = (int)((screenPosition.Y + (float)Main.screenHeight + offSet.Y) / 16f) + 5;
            if (firstTileX < 4)
            {
                firstTileX = 4;
            }
            if (lastTileX > Main.maxTilesX - 4)
            {
                lastTileX = Main.maxTilesX - 4;
            }
            if (firstTileY < 4)
            {
                firstTileY = 4;
            }
            if (lastTileY > Main.maxTilesY - 4)
            {
                lastTileY = Main.maxTilesY - 4;
            }
        }

        private void DrawSeaPrisms(On_Main.orig_DrawTiles orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
        {
            if(!solidLayer)
            {
                orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
                return;
            }

            Vector2 offscreenPosition = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.Textures[1] = SeaPrism.Green.Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            Main.instance.GraphicsDevice.Textures[2] = SeaPrism.Purple.Value;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
            Main.instance.GraphicsDevice.Textures[3] = SeaPrism.Glint.Value;
            Main.instance.GraphicsDevice.SamplerStates[3] = SamplerState.LinearClamp;

            Effect shader = CalamityShaders.SeaPrismColorBlendingShader;
            shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["screenOffset"].SetValue(Main.screenPosition);
            shader.Parameters["offscreenOffset"].SetValue(offscreenPosition);
            shader.Parameters["diagonalScreenLength"].SetValue((Main.screenWidth / 2f) - (Main.screenHeight / 2f));

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shader, Main.Transform);

            Vector2 unscaledPosition = Main.Camera.UnscaledPosition;

            GetScreenDrawArea(unscaledPosition, offscreenPosition + (Main.Camera.UnscaledPosition - Main.Camera.ScaledPosition), out var firstTileX, out var lastTileX, out var firstTileY, out var lastTileY);

            for (int y = firstTileY; y < lastTileY + 4; y++)
            {
                for (int x = firstTileX - 2; x < lastTileX + 2; x++)
                {
                    if (!WorldGen.InWorld(x, y))
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (tile == null)
                        continue;

                    int type = tile.TileType;
                    if (type != ModContent.TileType<SeaPrism>())
                        continue;

                    Vector2 position = new Vector2(x * 16, y * 16) - Main.screenPosition + offscreenPosition;

                    int frameX = tile.TileFrameX + (x % 8 * SeaPrism.subsheetWidth);
                    int frameY = tile.TileFrameY + (y % 8 * SeaPrism.subsheetHeight);

                    Rectangle sourceRect = new Rectangle(frameX, frameY, 16, 16);
                    Color light = Lighting.GetColor(x, y) * 1.5f;

                    Main.spriteBatch.Draw(SeaPrism.Blue.Value, position, sourceRect, light);
                }
            }

            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.Textures[1] = null;
            Main.instance.GraphicsDevice.Textures[2] = null;
            Main.instance.GraphicsDevice.Textures[3] = null;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
        }
    }
}
