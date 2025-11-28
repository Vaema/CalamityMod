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
            get => TileDataPacking.GetBit(Data, offset: 0);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 0);
        }

        /// <summary>
        /// Used for Tiles that have complicated conditions for <see cref="TileDrawing.AddSpecialPoint(int, int, Terraria.GameContent.Drawing.TileDrawing.TileCounterType)"/>
        /// </summary>
        public bool HasSpecialPoint
        {
            get => TileDataPacking.GetBit(Data, offset: 1);
            set => Data = (byte)TileDataPacking.SetBit(value, Data, offset: 1);
        }
    }
}
