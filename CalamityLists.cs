using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod
{
    public sealed class CalamityLists : ModSystem
    {
        /// <summary>
        /// Each Sunken Sea subbiome has a correspoding spawn condition boolean value and a biome type.
        /// </summary>
        public static SortedDictionary<SunkenSeaBiomeFlags, (Func<NPCSpawnInfo, bool> SpawnCondition, int BiomeType)> SunkenSeaBiomeCorrespondentValues { get; private set; }

        public override void OnModLoad()
        {
            SunkenSeaBiomeCorrespondentValues = new()
            {
                { SunkenSeaBiomeFlags.UndergroundDesert, (spawnInfo => spawnInfo.Player.ZoneDesert, -1 /* None needed. */) },
                { SunkenSeaBiomeFlags.TimelessShores, (spawnInfo => spawnInfo.Player.Calamity().ZoneTimelessShores, GetInstance<TimelessShoresBiome>().Type) },
                { SunkenSeaBiomeFlags.RadiantReefs, (spawnInfo => spawnInfo.Player.Calamity().ZoneRadiantReefs, GetInstance<RadiantReefsBiome>().Type) },
                { SunkenSeaBiomeFlags.PolypForest, (spawnInfo => spawnInfo.Player.Calamity().ZonePolypForest, GetInstance<PolypForestBiome>().Type) },
                { SunkenSeaBiomeFlags.GleamingBurrows, (spawnInfo => spawnInfo.Player.Calamity().ZoneGleamingBurrows, GetInstance<GleamingBurrowsBiome>().Type) },
                { SunkenSeaBiomeFlags.BasaltGully, (spawnInfo => spawnInfo.Player.Calamity().ZoneBasaltGully, GetInstance<BasaltGullyBiome>().Type) },
            };
        }

        public override void Unload()
        {
            SunkenSeaBiomeCorrespondentValues = null;
        }
    }
}
