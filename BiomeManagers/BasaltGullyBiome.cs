using CalamityMod.Systems;
using CalamityMod.Waters;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Placeables.Furniture;

namespace CalamityMod.BiomeManagers;

public class BasaltGullyBiome : ModBiome
{
    public override ModWaterStyle WaterStyle => SunkenSeaBurrowsWater.Instance;
    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("CalamityMod/SunkenSeaBGStyle");
    public override int BiomeTorchItemType => ModContent.ItemType<NavyPrismTorch>();
    public override int Music => CalamityMod.Instance.GetMusicFromMusicMod("SunkenSea") ?? MusicID.OceanNight;
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override string BestiaryIcon => "CalamityMod/BiomeManagers/SunkenSeaIcon";
    // Placeholder until we get a dedicated Basalt Gully background
    public override string BackgroundPath => "Terraria/Images/MapBG3";
    public override string MapBackground => "Terraria/Images/MapBG3";

    public override bool IsBiomeActive(Player player)
    {
        bool MoreThanShores = BiomeTileCounterSystem.SunkenSeaBasaltTiles > BiomeTileCounterSystem.SunkenSeaShoresTiles;
        bool MoreThanReefs = BiomeTileCounterSystem.SunkenSeaBasaltTiles > BiomeTileCounterSystem.SunkenSeaReefsTiles;
        bool MoreThanPolyp = BiomeTileCounterSystem.SunkenSeaBasaltTiles > BiomeTileCounterSystem.SunkenSeaPolypTiles;
        bool MoreThanBurrows = BiomeTileCounterSystem.SunkenSeaBasaltTiles > BiomeTileCounterSystem.SunkenSeaBurrowsTiles;
        bool MoreThanUnderground = BiomeTileCounterSystem.SunkenSeaShoresTiles > BiomeTileCounterSystem.UndergroundTiles;

        return BiomeTileCounterSystem.SunkenSeaBasaltTiles > 2500 && (MoreThanShores || MoreThanReefs || MoreThanPolyp || MoreThanBurrows);
    }
}
