using CalamityMod.Systems;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.BiomeManagers
{
    public class FloralParadiseBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("CalamityMod/FloralParadiseWater");
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("CalamityMod/FloralParadiseBGStyle");
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Floral Paradise");
        }

        public override bool IsBiomeActive(Player player)
        {
            return BiomeTileCounterSystem.FloralParadiseTiles >= 300;
        }
    }
}
