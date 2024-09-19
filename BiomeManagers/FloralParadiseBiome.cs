using CalamityMod.Systems;
using CalamityMod.Waters;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.BiomeManagers
{
    public class FloralParadiseBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => FloralParadiseWater.Instance;
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("CalamityMod/FloralParadiseBGStyle");
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return BiomeTileCounterSystem.FloralParadiseTiles >= 300;
        }
    }
}
