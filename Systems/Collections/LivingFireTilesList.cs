using System.Collections.Generic;
using CalamityMod.Tiles.LivingFire;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> with all Tile IDs of those which are Living-Fire-type tiles.
    /// </summary>
    public sealed class LivingFireTilesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                TileType<LivingGodSlayerFireBlockTile>(),
                TileType<LivingHolyFireBlockTile>(),
                TileType<LivingBrimstoneFireBlockTile>(),
                TileType<LivingPlagueFireBlockTile>(),
                TileID.LivingFire,
                TileID.LivingCursedFire,
                TileID.LivingDemonFire,
                TileID.LivingFrostFire,
                TileID.LivingIchor,
                TileID.LivingUltrabrightFire
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not a tile is a Living-Fire-type of tile.
        /// </summary>
        public static bool Includes(int tileType) => List.Contains(tileType);
    }
}
