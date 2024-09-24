using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Graphics;
using CalamityMod.Tiles.Abyss;
using CalamityMod.Tiles.Astral;
using CalamityMod.Tiles.AstralDesert;
using CalamityMod.Tiles.AstralSnow;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.NPCs.SunkenSea.PolypPanasea;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        #region Constants
        public const byte EmptySheetIndex = byte.MaxValue;
        public const int VariantCount = 3;

        public const int BlendTextureXCount = 16;
        public const int BlendTextureYCount = 16;
        public const int BlendTextureWidth = 18 * BlendTextureXCount;
        public const int BlendTextureHeight = 18 * BlendTextureYCount;
        #endregion

        private static bool[,] _TileBlendable; // dimension: [TileTypeCount, AllBlendTextureCount]
        private static BlendTextureID[] _TileTypeBlendTexture; // dimension: [TileTypeCount]
        private static Asset<Texture2D>[] _BaseBlendTextures; // dimension: [256]
        private static RenderTarget2D[,] _BlendTextures; // dimension: [256, 3]

        private static IEnumerable<BlendTextureID> AllBlendTextureIDs => Enum.GetValues<BlendTextureID>().Where(v => v != BlendTextureID.None);
        private static int AllBlendTextureCount => AllBlendTextureIDs.Count();

        #region Load/Unload
        public override void OnModLoad()
        {
            var tileCount = TileLoader.TileCount;
            _TileBlendable = new bool[tileCount, AllBlendTextureCount];
            _TileTypeBlendTexture = new BlendTextureID[tileCount]; 
            _BaseBlendTextures = new Asset<Texture2D>[256];
            _BlendTextures = new RenderTarget2D[256, 3];

            foreach (var idValue in AllBlendTextureIDs)
            {
                var name = Enum.GetName(idValue);
                var ID = (int)idValue;

                //TODO: Rename asset path and name to "Blend"
                _BaseBlendTextures[ID] = ModContent.Request<Texture2D>($"CalamityMod/Tiles/Merges/{name}Merge");
                _TileTypeBlendTexture[BlendTextureIDToTileType(idValue)] = idValue;

                Main.QueueMainThreadAction(() =>
                {
                    for (int v = 0; v < VariantCount; v++)
                        _BlendTextures[ID, v] = CreateBlendRT();
                });
            }

            // Draw Code
            On_TileDrawing.DrawSingleTile += OnDrawSingleTile;
        }

        private static void SetupMergeData()
        {
            Main.QueueMainThreadAction(() =>
            {
                foreach (var id in AllBlendTextureIDs)
                {
                    BakeBlendTexture(id, _BaseBlendTextures[(int)id].Value);
                }
            });
        }

        public override void Unload()
        {
            if (_BaseBlendTextures is not null)
            {
                Array.Clear(_BaseBlendTextures);
            }

            if (_BlendTextures is not null)
            {
                foreach (var rt in _BlendTextures)
                {
                    if (rt is null)
                        continue;

                    if (rt.IsDisposed)
                        continue;

                    rt.Dispose();
                }

                Array.Clear(_BlendTextures);
            }

            _TileBlendable = null;
            _BaseBlendTextures = null;
            _BlendTextures = null;
        }
        #endregion

        #region Public API
        public static void RegisterMerge(int myType, int blendTileType)
        {

            if (myType == blendTileType)
                return;

            if (!_TileTypeBlendTexture.IndexInRange(myType))
                return;

            if (!_TileTypeBlendTexture.IndexInRange(blendTileType))
                return;

            var blendTextureID = _TileTypeBlendTexture[blendTileType];
            if (blendTextureID == BlendTextureID.None)
                return;

            _TileBlendable[myType, (int)blendTextureID] = true;
            CalamityUtils.SetMerge(myType, blendTileType, true);
        }

        public static void RegisterMerge(int myType, BlendTextureID blendTextureID)
        {
            if (blendTextureID == BlendTextureID.None)
                return;

            if (!Enum.IsDefined(blendTextureID))
                return;

            if (!_TileTypeBlendTexture.IndexInRange(myType))
                return;

            var blendTileType = BlendTextureIDToTileType(blendTextureID);
            if (myType == blendTileType) // Self blending should never be case, That will be extremely heavy to render!
                return;

            _TileBlendable[myType, (int)blendTextureID] = true;
            CalamityUtils.SetMerge(myType, blendTileType, true);
        }

        public static void ReplaceMergeTexture(BlendTextureID textureID, Texture2D texture)
        {
            if (textureID == BlendTextureID.None)
                throw new ArgumentOutOfRangeException(nameof(textureID), "MergeTextureID.Everything is not allowed to use in here!");

            if (!Enum.IsDefined(textureID))
                throw new ArgumentOutOfRangeException(nameof(textureID), $"{textureID} is Invalid MergeTextureID!");

            // It's baking moment
            Main.QueueMainThreadAction(() =>
            {
                BakeBlendTexture(textureID, texture);
            });
        }
        #endregion

        #region Sheet Baking Process
        private static void BakeBlendTexture(BlendTextureID textureToBake, Texture2D texture)
        {
            if (!Enum.IsDefined(textureToBake))
                throw new ArgumentOutOfRangeException(nameof(textureToBake), $"{textureToBake} is Invalid MergeTextureID!");

            if (texture == null)
                throw new ArgumentNullException(nameof(texture), "Texture is Null!");

            if (texture.IsDisposed)
                throw new ArgumentException(paramName: nameof(texture), message: "Texture is Disposed!");

            for (int v = 0; v < VariantCount; v++)
            {
                var renderTarget = _BlendTextures[(int)textureToBake, v];
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
        private static RenderTarget2D CreateBlendRT()
        {
            return new(Main.instance.GraphicsDevice, BlendTextureWidth, BlendTextureHeight);
        }

        private static int HotFlagCount(BlendSideFlags flags)
        {
            var count = 0;
            for(int i = 0; i<8; i++)
            {
                var flag = (BlendSideFlags)(1 << i);
                if (flag == (flags & flag))
                {
                    count++;
                }
            }
            return count;
        }

        private static int BlendTextureIDToTileType(BlendTextureID id)
        {
            return id switch
            {
                BlendTextureID.AbyssGravel => ModContent.TileType<AbyssGravel>(),
                BlendTextureID.Ash => TileID.Ash,
                BlendTextureID.AstralDirt => ModContent.TileType<AstralDirt>(),
                BlendTextureID.AstralSand => ModContent.TileType<AstralSand>(),
                BlendTextureID.AstralSandstone => ModContent.TileType<AstralSandstone>(),
                BlendTextureID.AstralSnow => ModContent.TileType<AstralSnow>(),
                BlendTextureID.BrimstoneSlag => ModContent.TileType<AstralDirt>(),
                BlendTextureID.Cloud => TileID.Cloud,
                BlendTextureID.Dirt => TileID.Dirt,
                BlendTextureID.EutrophicSand => ModContent.TileType<EutrophicSand>(),
                BlendTextureID.HardenedSand => TileID.HardenedSand,
                BlendTextureID.HardenedSulphurousSandstone => ModContent.TileType<HardenedSulphurousSandstone>(),
                BlendTextureID.Luminite => TileID.LunarOre,
                BlendTextureID.Mud => TileID.Mud,
                BlendTextureID.Navystone => ModContent.TileType<Navystone>(),
                BlendTextureID.PyreMantle => ModContent.TileType<PyreMantle>(),
                BlendTextureID.RainCloud => TileID.RainCloud,
                BlendTextureID.Sand => TileID.Sand,
                BlendTextureID.Sandstone => TileID.Sandstone,
                BlendTextureID.SnowCloud => TileID.SnowCloud,
                BlendTextureID.Snow => TileID.SnowBlock,
                BlendTextureID.Stone => TileID.Stone,
                BlendTextureID.SulphurousSand => ModContent.TileType<SulphurousSand>(),
                BlendTextureID.SulphurousSandstone => ModContent.TileType<SulphurousSandstone>(),
                BlendTextureID.SulphurousShale => ModContent.TileType<SulphurousShale>(),
                BlendTextureID.Voidstone => ModContent.TileType<Voidstone>(),
                _ => throw new ArgumentOutOfRangeException(nameof(id), $"{id} is not valid index!")
            };
        }

        private static Rectangle SideDataToSheetRect(byte data)
        {
            int y = Math.DivRem(data, 16, out int x);
            return new Rectangle(x * 18, y * 18, 16, 16);
        }

        private static Vector2 SideDataToPositionInSheet(byte data)
        {
            int y = Math.DivRem(data, 16, out int x);
            return new Vector2(x * 18, y * 18);
        }
        #endregion
    }
}
