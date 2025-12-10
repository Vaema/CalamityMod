using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static bool[,] _TileBlendable = new bool[TileID.Count, 1]; // dimension: [TileTypeCount, AllBlendTextureCount]
        private static bool[,] _TileBlendLooselyFillDiagonal = new bool[TileID.Count, 1]; // dimension: [TileTypeCount, AllBlendTextureCount]
        private static byte[] _TileTypeToBlendTextureSlot = new byte[TileID.Count]; // dimension: [TileTypeCount]

        #region Load/Unload
        public override void OnModLoad()
        {
            // Draw Code
            On_Main.DrawTiles += OnDrawTiles;
            Main.OnPreDraw += UpdateBaking;
        }

        public override void Unload()
        {
            Main.OnPreDraw -= UpdateBaking;
            _TileBlendable = null;
            _TileBlendLooselyFillDiagonal = null;
            _TileTypeToBlendTextureSlot = null;
        }

        public override void ResizeArrays()
        {
            var tileCount = TileLoader.TileCount;
            var blendTextureCount = TileBlendTextureLoader.Count; // This count also should be updated on ResizeArrays call

            ResizeArray2D(ref _TileBlendable, tileCount, blendTextureCount + 1);
            ResizeArray2D(ref _TileBlendLooselyFillDiagonal, tileCount, blendTextureCount + 1);
            Array.Resize(ref _TileTypeToBlendTextureSlot, tileCount);

            foreach (var blendTexture in TileBlendTextureLoader.AllTextures)
            {
                _TileTypeToBlendTextureSlot[blendTexture.TileType] = (byte)blendTexture.Slot;
            }
        }

        private static void ResizeArray2D<T>(ref T[,] array, int newColNum, int newRowNum)
        {
            var newArray = new T[newColNum, newRowNum];
            int colCount = array.GetLength(1);
            int newColCount = newRowNum;
            int cols = array.GetUpperBound(0);
            for (int co = 0; co <= cols; co++)
                Array.Copy(array, co * colCount, newArray, co * newColCount, colCount);
            array = newArray;
        }

        private static void SetupMergeData()
        {
            foreach (var blendTexture in TileBlendTextureLoader.AllTextures)
            {
                blendTexture.ClearBakeCache();
            }
        }

        private static void UpdateBaking(GameTime obj)
        {
            TileBlendTexture.BakedCountInFrame = 0;
            foreach (var blendTexture in TileBlendTextureLoader.AllTextures)
            {
                blendTexture.BakeRequestedBlendTextureCache();
            }
        }
        #endregion

        #region Public API
        public static void RegisterMerge(int myType, int blendTileType, bool looselyFillDiagonal = false)
        {
            if (Main.dedServ)
                return;

            if (myType == blendTileType)
                return;

            if (!_TileTypeToBlendTextureSlot.IndexInRange(myType))
                return;

            if (!_TileTypeToBlendTextureSlot.IndexInRange(blendTileType))
                return;

            var blendTextureSlot = _TileTypeToBlendTextureSlot[blendTileType];
            if (blendTextureSlot == TileBlendTextureLoader.EmptySlot)
            {
                var tileName = TileLoader.GetTile(blendTileType)?.FullName ?? "Vanilla Tile";
                CalamityMod.Log.Error($"[BlendMergeSystem] BlendTileType: {blendTileType} ({tileName}) does not have TileBlendTexture! StackTrace:\n{Environment.StackTrace}");
                return;
            }

            _TileBlendable[myType, blendTextureSlot] = true;
            _TileBlendLooselyFillDiagonal[myType, blendTextureSlot] = looselyFillDiagonal;
            CalamityUtils.SetMerge(myType, blendTileType, true);
        }

        public static void RegisterMerge(int myType, TileBlendTexture blendTexture, bool looselyFillDiagonal = false)
        {
            if (Main.dedServ)
                return;

            if (blendTexture == null)
                return;

            if (blendTexture.Slot < 0)
                return;

            if (!_TileTypeToBlendTextureSlot.IndexInRange(myType))
                return;

            var blendTileType = blendTexture.TileType;
            if (myType == blendTileType) // Self blending should never be case, That will be extremely heavy to render!
                return;

            _TileBlendable[myType, blendTexture.Slot] = true;
            _TileBlendLooselyFillDiagonal[myType, blendTexture.Slot] = looselyFillDiagonal;
            CalamityUtils.SetMerge(myType, blendTileType, true);
        }

        public static void RegisterMerge<T>(int myType, bool looselyFillDiagonal = false) where T : TileBlendTexture
        {
            if (Main.dedServ)
                return;

            var blendTexture = ModContent.GetInstance<T>();
            RegisterMerge(myType, blendTexture, looselyFillDiagonal);
        }
        #endregion
    }
}
