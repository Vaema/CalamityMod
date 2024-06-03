using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public abstract class WallVisibleThroughWater : ModWall
    {
        public const float WaterTransparency = 0.5f;
        public static readonly Color WaterColor = new Color(9, 61, 191);

        public int WaterMapEntry { get; private set; }

        /// <summary>
        /// Adds the map entry, and further entries for being covered in water.
        /// Please use this over AddMapEntry in walls which utilize this base class.
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="text"></param>
        public void AddEntries(Color baseColor, LocalizedText text = null)
        {
            AddMapEntry(baseColor, text);
            AddMapEntry(Color.Lerp(baseColor, WaterColor, WaterTransparency), text);
        }

        public static void InitializeWaterMapEntryLookups()
        {
            foreach (var wall in CalamityMod.Instance.GetContent<WallVisibleThroughWater>())
            {
                int lookup = MapHelper.wallLookup[wall.Type];
                wall.WaterMapEntry = lookup + 1;
            }
        }
    }
}
