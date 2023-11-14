using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.BiomeManagers
{
    public class SunkenSeaBiome : ModBiome
    {
        //this is just a global sunken sea biome to check if you are in any of the existing biomes
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override string BestiaryIcon => "CalamityMod/BiomeManagers/SunkenSeaIcon";
		// Placeholder until we get a dedicated Sunken Sea background
        public override string BackgroundPath => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";
        public override string MapBackground => "CalamityMod/Backgrounds/MapBackgrounds/AbyssBGLayer1";

        public override bool IsBiomeActive(Player player)
        {
            return BiomeTileCounterSystem.SunkenSeaTiles > 150;
        }
    }
}
