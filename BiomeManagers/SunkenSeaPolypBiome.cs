using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.BiomeManagers
{
    public class SunkenSeaPolypBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("CalamityMod/SunkenSeaPolypWater");
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("CalamityMod/SunkenSeaBGStyle");
        public override int BiomeTorchItemType => ModContent.ItemType<NavyPrismTorch>();
        public override int Music => CalamityMod.Instance.GetMusicFromMusicMod("SunkenSea") ?? MusicID.OceanNight;
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override string BestiaryIcon => "CalamityMod/BiomeManagers/SunkenSeaIcon";
		// Placeholder until we get a dedicated Sunken Sea background
        public override string BackgroundPath => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";
        public override string MapBackground => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";

        public override bool IsBiomeActive(Player player)
        {
            bool MoreThanReefs = BiomeTileCounterSystem.SunkenSeaPolypTiles > BiomeTileCounterSystem.SunkenSeaReefsTiles;
            bool MoreThanBurrows = BiomeTileCounterSystem.SunkenSeaPolypTiles > BiomeTileCounterSystem.SunkenSeaBurrowsTiles;

            return BiomeTileCounterSystem.SunkenSeaPolypTiles > 1000 && MoreThanReefs && MoreThanBurrows;
        }
    }
}
