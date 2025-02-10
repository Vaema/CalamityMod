using System.Collections.Generic;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class DisabledSummonerNerfItemList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List = [];
        }

        public override void Unload() => List = null;

        public static bool Includes(int itemType) => List.Contains(itemType);
    }
}
