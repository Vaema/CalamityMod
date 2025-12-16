using CalamityMod.Tiles.SunkenSea;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    [ReinitializeDuringResizeArrays]
    public static class CalamityTileSets
    {
        public static SetFactory Factory = new SetFactory(TileLoader.TileCount, "CalamityMod/TileID", Search);
        public static IdDictionary Search = IdDictionary.Create<TileID, int>();

        /// <summary>
        /// Should only contain modded tiles. If <see langword="true"/> for a tile type, then that tile can be replaced by the Abyss during world generation.<br/>
        /// Unused by Calamity itself, and is only used for external mods to add to through reflection.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] CanBeReplacedByAbyssGeneration = Factory.CreateBoolSet();

        /// <summary>
        /// If <see langword="true"/> for a tile type, It does not draw BlendMerge on PostDraw hook. Instead it will be drawn after every solid tiles has drawn to screen<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] DrawBlendMergeAfterSolidTile = Factory.CreateBoolSet(TileType<SeaPrism>());
    }
}
