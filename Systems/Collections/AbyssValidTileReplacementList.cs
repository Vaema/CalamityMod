using System.Collections.Generic;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of modded Tile IDs which the Abyss can replace while generating.
    /// </summary>
    public sealed class AbyssValidTileReplacementList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                // By default, Calamity does not add anything to this list. Mods are free to do so with reflection.
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not a tile can be replaced by the Abyss while generating.
        /// </summary>
        public static bool Includes(int tileType) => List.Contains(tileType);
    }
}
