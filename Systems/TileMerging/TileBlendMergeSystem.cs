using System;
using System.Reflection;
using CalamityMod.ILEditing;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static bool[,] _TileBlendable; // dimension: [TileTypeCount, AllBlendTextureCount]
        private static bool[,] _TileBlendLooselyFillDiagonal; // dimension: [TileTypeCount, AllBlendTextureCount]
        private static byte[] _TileTypeToBlendTextureSlot; // dimension: [TileTypeCount]
        private static bool _IDSetsInitialized = false;

        #region Load/Unload
        public override void OnModLoad()
        {
            // Draw Code
            On_Main.DrawTiles += OnDrawTiles;
            IL_TileDrawing.Draw += MakeQualityRequirementUpdateHook;
            ResizeArrayHook.OnPostResizeArrays += OnResizeArrays;
        }

        private void MakeQualityRequirementUpdateHook(ILContext il)
        {
            const string midQualityRequirementField = "_mediumQualityLightingRequirement";
            const string highQualityRequirementField = "_highQualityLightingRequirement";
            const string getScreenDrawAreaMethod = "GetScreenDrawArea";

            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<TileDrawing>(getScreenDrawAreaMethod)))
            {
                ILChanges.LogFailure("QualityRequirementHook", $"{getScreenDrawAreaMethod} call is missing!");
                return;
            }

            var type = typeof(TileDrawing);
            var mediumReqField = type.GetField(midQualityRequirementField, BindingFlags.NonPublic | BindingFlags.Instance);
            var highReqField = type.GetField(highQualityRequirementField, BindingFlags.NonPublic | BindingFlags.Instance);

            if (mediumReqField == null)
            {
                ILChanges.LogFailure("QualityRequirementHook", $"{midQualityRequirementField} field is missing!");
                return;
            }

            if (highReqField == null)
            {
                ILChanges.LogFailure("QualityRequirementHook", $"{highQualityRequirementField} field is missing!");
                return;
            }

            cursor.EmitLdarg0(); // self
            cursor.EmitLdfld(highReqField); // self.highReqField
            cursor.EmitLdarg0(); // self
            cursor.EmitLdfld(mediumReqField); // self.mediumReqField
            cursor.EmitDelegate(OnQualityRequirementUpdate); // call (highReq, mediumReq)
        }

        private static void OnResizeArrays(bool unloading)
        {
            var tileCount = TileLoader.TileCount;
            var blendTextureCount = TileBlendTextureLoader.Count; // This count also should be updated on ResizeArrays call

            static void SetupTileTypeToBlendTextureSlot()
            {
                if (TileBlendTextureLoader.AllTextures == null)
                    return;
                foreach (var blendTexture in TileBlendTextureLoader.AllTextures)
                {
                    _TileTypeToBlendTextureSlot[blendTexture.TileType] = (byte)blendTexture.Slot;
                }
            }

            if (!_IDSetsInitialized)
            {
                _TileBlendable = new bool[tileCount, blendTextureCount + 1];
                _TileBlendLooselyFillDiagonal = new bool[tileCount, blendTextureCount + 1];
                _TileTypeToBlendTextureSlot = new byte[tileCount];
                _IDSetsInitialized = true;
                SetupTileTypeToBlendTextureSlot();
                return;
            }

            if (unloading)
            {
                _TileBlendable = null;
                _TileBlendLooselyFillDiagonal = null;
                _TileTypeToBlendTextureSlot = null;
                return;
            }

            ResizeArray2D(ref _TileBlendable, tileCount, blendTextureCount + 1);
            ResizeArray2D(ref _TileBlendLooselyFillDiagonal, tileCount, blendTextureCount + 1);
            Array.Resize(ref _TileTypeToBlendTextureSlot, tileCount);
            SetupTileTypeToBlendTextureSlot();
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
                blendTexture.BakeBlendTexture(blendTexture.TextureAsset.Value);
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
                CalamityMod.Instance.Logger.Error($"[BlendMergeSystem] BlendTileType: {blendTileType} ({tileName}) does not have TileBlendTexture! StackTrace:\n{Environment.StackTrace}");
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
