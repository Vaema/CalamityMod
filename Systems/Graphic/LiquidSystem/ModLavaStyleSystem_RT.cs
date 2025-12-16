using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class ModLavaStyleSystem : ModSystem
    {
        public static RenderTarget2D LavaRT { get; private set; }
        public static RenderTarget2D LavaBlockRT { get; private set; }
        public static RenderTarget2D LavaSlopeRT { get; private set; }
        public static RenderTarget2D LavaWaterfallRT { get; private set; }

        public const int TextureWidth = 48;
        public const int TextureHeight = 1360;

        public const int BlockTextureWidth = 16;
        public const int BlockTextureHeight = 16;

        public const int SlopeTextureWidth = 72;
        public const int SlopeTextureHeight = 16;

        public const int WaterfallTextureWidth = 512;
        public const int WaterfallTextureHeight = 40;

        private static void PrepareRT()
        {
            LavaRT = CreateRT(TextureWidth, TextureHeight);
            LavaBlockRT = CreateRT(BlockTextureWidth, BlockTextureHeight);
            LavaSlopeRT = CreateRT(SlopeTextureWidth, SlopeTextureHeight);
            LavaWaterfallRT = CreateRT(WaterfallTextureWidth, WaterfallTextureHeight);
        }

        private static RenderTarget2D CreateRT(int width, int height)
        {
            return new(Main.instance.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private static void DisposeRT()
        {
            LavaRT?.Dispose();
            LavaBlockRT?.Dispose();
            LavaSlopeRT?.Dispose();
            LavaWaterfallRT?.Dispose();

            LavaRT = null;
            LavaBlockRT = null;
            LavaSlopeRT = null;
            LavaWaterfallRT = null;
        }
        
        private static void UpdateRT(GameTime time)
        {
            
            if (!Initialized || !TextureArrayReady || Main.gameMenu)
                return;

            var graphicsDevice = Main.instance.GraphicsDevice;
            graphicsDevice.SetRenderTarget(LavaRT);
            graphicsDevice.Clear(Color.Transparent);
            Begin();
            DrawTextures(Textures);
            End();

            graphicsDevice.SetRenderTarget(LavaBlockRT);
            graphicsDevice.Clear(Color.Transparent);
            Begin();
            DrawTextures(BlockTextures);
            End();

            graphicsDevice.SetRenderTarget(LavaSlopeRT);
            graphicsDevice.Clear(Color.Transparent);
            Begin();
            DrawTextures(SlopeTextures);
            End();

            graphicsDevice.SetRenderTarget(LavaWaterfallRT);
            graphicsDevice.Clear(Color.Transparent);
            Begin();
            DrawTextures(WaterfallTextures);
            End();
            
            graphicsDevice.SetRenderTarget(null);
        }

        private static readonly BlendState ActualAdditive = new()
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One
        };

        private static void Begin()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, ActualAdditive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
        }

        private static void End()
        {
            Main.spriteBatch.End();
        }

        private static void DrawTextures(Asset<Texture2D>[] textures)
        {
            var totalCount = ModLavaStyleLoader.TotalCount;
            for (int i = 0; i < totalCount; i++)
            {
                var alpha = LavaAlpha[i];
                if (alpha <= 0.0f)
                    continue;

                Main.spriteBatch.Draw(textures[i].Value, Vector2.Zero, null, Color.White * alpha);
            }
        }
    }
}
