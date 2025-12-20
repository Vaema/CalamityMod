using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public abstract partial class TileBlendTexture : ModTexturedType
    {
        internal static int BakedCountInFrame = 0;

        private bool[] _IsBaked = new bool[VariantCount];
        private bool[] _RequestedVariants = new bool[VariantCount];
        private bool _IsRequestedAny = false;
        private bool _IsSheetRequested = false;
        private bool _ShouldClearRT = true;

        internal void ClearBakeCache()
        {
            _IsBaked = new bool[VariantCount];
            _RequestedVariants = new bool[VariantCount];
            _IsRequestedAny = false;
            _ShouldClearRT = true;
        }

        internal void RequestBake(int sheetIndex)
        {
            if (!_IsBaked[sheetIndex])
            {
                _RequestedVariants[sheetIndex] = true;
                _IsRequestedAny = true;
            }
        }

        #region Sheet Baking Process
        internal void BakeRequestedBlendTextureCache()
        {
            if (!_IsRequestedAny)
                return;

            if (!TextureAsset.IsLoaded)
                return;

            var texture = TextureAsset.Value;
            if (texture == null)
                return;

            if (texture.IsDisposed)
                return;

            var graphicsDevice = Main.instance.GraphicsDevice;
            for (int v = 0; v < VariantCount; v++)
            {
                int variant = v;
                if (!_RequestedVariants[variant])
                    continue;

                if (BakedCountInFrame >= 3)
                    continue;

                var renderTarget = BakedBlendTexture;
                if (renderTarget != null && !renderTarget.IsDisposed && !renderTarget.IsContentLost)
                {
                    graphicsDevice.SetRenderTarget(renderTarget);
                    if (_ShouldClearRT)
                    {
                        graphicsDevice.Clear(Color.Transparent);
                        _ShouldClearRT = false;
                    }

                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

                    BakeBlendTextureCache(v);

                    Main.spriteBatch.End();
                    graphicsDevice.SetRenderTarget(null);

                    _RequestedVariants[variant] = false;
                    _IsBaked[variant] = true;
                }
                else if (!_IsSheetRequested)
                {
                    _IsSheetRequested = true;
                    Main.QueueMainThreadAction(() =>
                    {
                        BakedBlendTexture = new(
                                Main.instance.GraphicsDevice,
                                BlendTextureWidth,
                                BlendTextureFullHeight,
                                mipMap: false,
                                preferredFormat: SurfaceFormat.Color,
                                preferredDepthFormat: DepthFormat.None,
                                preferredMultiSampleCount: 0,
                                usage: RenderTargetUsage.PreserveContents);

                        _IsSheetRequested = false;
                    });
                }

                BakedCountInFrame++;
            }

            foreach (var requested in _RequestedVariants)
                _IsRequestedAny |= requested;
        }

        internal void BakeBlendTextureCache(int randomFrame)
        {
            for (int i = 0; i < 256; i++)
            {
                var mergeSides = (BlendSideFlags)i;
                var sheetPosition = _SheetPositionLookup[new SheetPositionKey(mergeSides, (byte)randomFrame)];

                // If it's basic shape, pull it from base texture instead
                if (sheetPosition.IsUsingBaseTexture)
                {
                    continue;
                }

                var drawPos = sheetPosition.GetDrawPosition();
                var extractedShapes = ConsumeMergeSides(mergeSides);
                foreach (var shape in extractedShapes)
                {
                    if (_BasicShapeLookup.TryGetValue(shape, out var shapeRects))
                    {
                        Main.spriteBatch.Draw(TextureAsset.Value, drawPos, shapeRects[randomFrame], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                    }
                }
            }
        }

        private static IEnumerable<BlendSideFlags> ConsumeMergeSides(BlendSideFlags sideFlags)
        {
            if (sideFlags == BlendSideFlags.None)
                yield break;

            foreach (var shapeGroup in _ShapeConsumeMap)
            {
                foreach (var shape in shapeGroup)
                {
                    if ((shape & sideFlags) == shape)
                    {
                        // Consume Shape it given and push the extracted shape
                        sideFlags &= ~shape;
                        yield return shape;
                    }
                }

                if (sideFlags == BlendSideFlags.None)
                    yield break;
            }
            yield break;
        }
        #endregion

        #region Utils
        public bool TryGetDrawingInfo(SheetPositionKey key, out Texture2D texture, out Rectangle sourceRect)
        {
            var pos = _SheetPositionLookup[key];

            sourceRect = pos.GetDrawRect();
            texture = pos.IsUsingBaseTexture ? TextureAsset.Value : BakedBlendTexture;

            return texture != null;
        }
        #endregion
    }
}
