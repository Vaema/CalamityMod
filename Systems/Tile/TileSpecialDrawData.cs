using Terraria;

namespace CalamityMod.Systems
{
    public struct TileSpecialDrawData : ITileData
    {
        private byte Data;

        public bool HasBlendMergeData
        {
            get => TileDataPacking.GetBit(Data, offset: 0);
            set => TileDataPacking.SetBit(value, Data, offset: 0);
        }
    }
}
