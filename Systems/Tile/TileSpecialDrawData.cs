using Terraria;
using Terraria.GameContent.Drawing;

namespace CalamityMod.Systems
{
    public struct TileSpecialDrawData : ITileData
    {
        private byte Data;

        /// <summary>
        /// Used for <see cref="TileBlendMergeSystem"/>
        /// </summary>
        public bool HasBlendMergeData
        {
            readonly get => TileDataPacking.GetBit(Data, offset: 0);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 0);
        }

        /// <summary>
        /// Used for Tiles that have complicated conditions for <see cref="TileDrawing.AddSpecialPoint(int, int, Terraria.GameContent.Drawing.TileDrawing.TileCounterType)"/>
        /// </summary>
        public bool HasSpecialPoint
        {
            readonly get => TileDataPacking.GetBit(Data, offset: 1);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 1);
        }

        // Empty Flags:
        // 2, 3

        /// <summary>
        /// Shared Flag0
        /// </summary>
        public bool Flag0
        {
            readonly get => TileDataPacking.GetBit(Data, offset: 4);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 4);
        }

        /// <summary>
        /// Shared Flag1
        /// </summary>
        public bool Flag1
        {
            readonly get => TileDataPacking.GetBit(Data, offset: 5);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 5);
        }

        /// <summary>
        /// Shared Flag2
        /// </summary>
        public bool Flag2
        {
            readonly get => TileDataPacking.GetBit(Data, offset: 6);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 6);
        }

        /// <summary>
        /// Shared Flag3
        /// </summary>
        public bool Flag3
        {
            readonly get => TileDataPacking.GetBit(Data, offset: 7);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 7);
        }

        /// <summary>
        /// Get All Flags
        /// </summary>
        /// <param name="flag0">Flag0 Value</param>
        /// <param name="flag1">Flag1 Value</param>
        /// <param name="flag2">Flag2 Value</param>
        /// <param name="flag3">Flag3 Value</param>
        public readonly void GetFlags(out bool flag0, out bool flag1, out bool flag2, out bool flag3)
        {
            flag0 = Flag0;
            flag1 = Flag1;
            flag2 = Flag2;
            flag3 = Flag3;
        }

        /// <summary>
        /// Set All Flags
        /// </summary>
        /// <param name="flag0">Flag0</param>
        /// <param name="flag1">Flag1</param>
        /// <param name="flag2">Flag2</param>
        /// <param name="flag3">Flag3</param>
        public void SetFlags(bool flag0, bool flag1, bool flag2, bool flag3)
        {
            Flag0 = flag0;
            Flag1 = flag1;
            Flag2 = flag2;
            Flag3 = flag3;
        }
    }
}
