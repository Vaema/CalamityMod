using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public abstract partial class TileBlendTexture : ModTexturedType
    {
        #region Sheet Baking Process
        public void BakeBlendTexture(Texture2D texture)
        {
            // It's baking moment
            Main.QueueMainThreadAction(() =>
            {
                BakeBlendTexture_Inner(texture);
            });
        }

        private void BakeBlendTexture_Inner(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture), "Texture is Null!");

            if (texture.IsDisposed)
                throw new ArgumentException(paramName: nameof(texture), message: "Texture is Disposed!");

            for (int v = 0; v < VariantCount; v++)
            {
                var renderTarget = BlendTextures[v];
                var graphicsDevice = Main.instance.GraphicsDevice;
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Transparent);

                Main.spriteBatch.SafeBegin(SpriteSortMode.Immediate, BatchSetting.AlphaBlend, null, Matrix.Identity, () =>
                {
                    for (int i = 0; i < 256; i++)
                    {
                        var drawPos = SideDataToPositionInSheet((byte)i);
                        var mergeSides = (BlendSideFlags)i;

                        // Easy cases, It match on Shape Lookup Sheet Directly
                        if (_ShapeLookup.TryGetValue(mergeSides, out var rects))
                        {
                            Main.spriteBatch.Draw(texture, drawPos, rects[v], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                            continue;
                        }

                        var extractedShapes = ConsumeMergeSides(mergeSides);
                        foreach (var shape in extractedShapes)
                        {
                            if (_ShapeLookup.TryGetValue(shape, out var shapeRects))
                            {
                                Main.spriteBatch.Draw(texture, drawPos, shapeRects[v], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                            }
                        }
                    }
                });

                graphicsDevice.SetRenderTarget(null);
            }
        }

        private static IEnumerable<BlendSideFlags> ConsumeMergeSides(BlendSideFlags sideFlags)
        {
            if (sideFlags == BlendSideFlags.None)
                yield break;

            IEnumerable<IEnumerable<BlendSideFlags>> shapes = [
                _U_Shapes,
                _L_Shapes,
                _I_Shapes, // I shape includes up, down, left, right
                _Corner_Shapes
            ];

            foreach (var shapeGroup in shapes)
            {
                foreach (var shape in shapeGroup.OrderByDescending(HotFlagCount))
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
        private static int HotFlagCount(BlendSideFlags flags)
        {
            var count = 0;
            for (int i = 0; i < 8; i++)
            {
                var flag = (BlendSideFlags)(1 << i);
                if (flag == (flags & flag))
                {
                    count++;
                }
            }
            return count;
        }

        public static Rectangle SideDataToSheetRect(byte data)
        {
            int y = Math.DivRem(data, 16, out int x);
            return new Rectangle(x * 18, y * 18, 16, 16);
        }

        public static Vector2 SideDataToPositionInSheet(byte data)
        {
            int y = Math.DivRem(data, 16, out int x);
            return new Vector2(x * 18, y * 18);
        }
        #endregion
    }
}
