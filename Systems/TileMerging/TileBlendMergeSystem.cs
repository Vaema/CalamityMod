using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.NPCs.SunkenSea.PolypPanasea;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        public const byte EmptySheetIndex = 0;
        public const int VariantCount = 3;

        public const int BakedBlendTextureWidth = 18 * 16;
        public const int BakedBlendTextureHeight = 18 * 16;

        private static Asset<Texture2D>[] _MergeTextures;
        private static ManagedRenderTarget[,] _BakedBlendTextures;
        private static int _MaxTilesX;
        private static int _MaxTilesY;

        private static IEnumerable<MergeTextureID> AllMergeTextureIDs => Enum.GetValues<MergeTextureID>().Where(v => v != MergeTextureID.Everything);

        #region Load/Unload
        public override void OnModLoad()
        {
            _MergeTextures = new Asset<Texture2D>[256];
            _BakedBlendTextures = new ManagedRenderTarget[256, 3];

            foreach (var idValue in AllMergeTextureIDs)
            {
                var name = Enum.GetName(idValue);
                var ID = (int)idValue;
                _MergeTextures[ID] = ModContent.Request<Texture2D>($"CalamityMod/Tiles/Merges/{name}Merge");

                Main.QueueMainThreadAction(() =>
                {
                    for (int v = 0; v < VariantCount; v++)
                        _BakedBlendTextures[ID, v] = new ManagedRenderTarget(false, BlendSheetRTCondition);
                });
            }
        }

        public override void PostSetupContent()
        {
            CalamityMod.Instance.Logger.Error("PostSetupContent");
        }

        private static void SetupMergeData()
        {
            CalamityMod.Instance.Logger.Error("SetupMerge");
            Main.QueueMainThreadAction(() =>
            {
                foreach (var id in AllMergeTextureIDs)
                {
                    BakeBlendTexture(id, _MergeTextures[(int)id].Value);
                }
            });
        }

        public override void Unload()
        {
            if (_MergeTextures is not null)
            {
                Array.Clear(_MergeTextures);
            }

            if (_BakedBlendTextures is not null)
            {
                foreach (var rt in _BakedBlendTextures)
                {
                    if (rt is null)
                        continue;

                    if (rt.IsDisposed)
                        continue;

                    rt.Dispose();
                }

                Array.Clear(_BakedBlendTextures);
            }

            _MergeTextures = null;
            _BakedBlendTextures = null;
        }
        #endregion

        #region World Load/Unload
        public override void OnWorldLoad()
        {
            _MaxTilesX = Main.maxTilesX;
            _MaxTilesY = Main.maxTilesY;
        }

        public override void OnWorldUnload()
        {
            _MaxTilesX = 0;
            _MaxTilesY = 0;
        }
        #endregion

        #region Public API
        public static void ReplaceTexture(MergeTextureID textureID, Texture2D texture)
        {
            if (textureID == MergeTextureID.Everything)
                throw new ArgumentOutOfRangeException(nameof(textureID), "MergeTextureID.Everything is not allowed to use in here!");

            if (!Enum.IsDefined(textureID))
                throw new ArgumentOutOfRangeException(nameof(textureID), $"{textureID} is Invalid MergeTextureID!");

            BakeBlendTexture(textureID, texture);
        }
        #endregion

        #region Sheet Baking Process
        private static void BakeBlendTexture(MergeTextureID textureToBake, Texture2D texture)
        {
            if (!Enum.IsDefined(textureToBake))
                throw new ArgumentOutOfRangeException(nameof(textureToBake), $"{textureToBake} is Invalid MergeTextureID!");

            if (texture == null)
                throw new ArgumentNullException(nameof(texture), "Texture is Null!");

            if (texture.IsDisposed)
                throw new ArgumentException(paramName: nameof(texture), message: "Texture is Disposed!");

            for (int v = 0; v < VariantCount; v++)
            {
                var renderTarget = _BakedBlendTextures[(int)textureToBake, v];
                var graphicsDevice = Main.instance.GraphicsDevice;
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Transparent);

                Main.spriteBatch.SafeBegin(SpriteSortMode.Immediate, BatchSetting.AlphaBlend, null, Matrix.Identity, () =>
                {
                    for (int i = 0; i < 256; i++)
                    {
                        int y = Math.DivRem(i, 16, out int x);
                        var drawPos = new Vector2(18 * x, 18 * y);

                        var mergeSides = (MergeSideFlags)i;

                        // Easy cases, It match on Shape Lookup Sheet
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

#if DEBUG
                var pngPath = $"C:\\baked\\{textureToBake}_{v}.png";
                var fs = File.Open(pngPath, FileMode.Create, FileAccess.ReadWrite);
                renderTarget.Target.SaveAsPng(fs, BakedBlendTextureWidth, BakedBlendTextureHeight);
                fs.Close();
                fs.Dispose();
#endif
            }
        }

        private static IEnumerable<MergeSideFlags> ConsumeMergeSides(MergeSideFlags sideFlags)
        {
            if (sideFlags == MergeSideFlags.None)
                yield break;

            IEnumerable<IEnumerable<MergeSideFlags>> shapes = [
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

                if (sideFlags == MergeSideFlags.None)
                    yield break;
            }
            yield break;
        }
        #endregion

        #region Utils
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool InValidRange(int i, int j)
        {
            if (i < 0 || i >= _MaxTilesX) return false;
            if (j < 0 || j >= _MaxTilesY) return false;
            return true;
        }

        private static RenderTarget2D BlendSheetRTCondition(int width, int height)
        {
            return new(Main.instance.GraphicsDevice, BakedBlendTextureWidth, BakedBlendTextureHeight);
        }

        private static int HotFlagCount(MergeSideFlags flags)
        {
            var count = 0;
            for(int i = 0; i<8; i++)
            {
                var flag = (MergeSideFlags)(1 << i);
                if (flag == (flags & flag))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
    }
}
