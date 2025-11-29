using System.Collections.Generic;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    public class ModLavaStyleLoader : ModSystem
    {
        private static readonly List<ModLavaStyle> ModStyleList = [];

        public static IReadOnlyList<ModLavaStyle> AllStyles => ModStyleList;


        public const int VanillaCount = 1;
        public static int ModCount { get; private set; } = 0;
        public static int TotalCount => ModCount + VanillaCount;

        internal static int Register(ModLavaStyle instance)
        {
            int type = TotalCount;
            ModTypeLookup<ModLavaStyle>.Register(instance);
            ModStyleList.Add(instance);
            ModCount = ModStyleList.Count;
            return type;
        }

        public override void Unload()
        {
            ModStyleList.Clear();
        }
    }
}
