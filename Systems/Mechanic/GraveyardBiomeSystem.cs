using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class GraveyardBiomeSystem : ModSystem
    {
        // For now it literally adjust the graveyard count required for GraveyardBiome
        // But we can add something later on /shrug

        public override void OnModLoad()
        {
            // Make Graveyard biomes require more Gravestones
            SceneMetrics.GraveyardTileMax = 60;
            SceneMetrics.GraveyardTileMin = 40;
            SceneMetrics.GraveyardTileThreshold = 52;
        }

        public override void Unload()
        {
            SceneMetrics.GraveyardTileMax = 36;
            SceneMetrics.GraveyardTileMin = 16;
            SceneMetrics.GraveyardTileThreshold = 28;
        }
    }
}
