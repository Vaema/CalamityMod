using System;
using CalamityMod.Tiles.Abyss;
using CalamityMod.Tiles.Astral;
using CalamityMod.Tiles.AstralDesert;
using CalamityMod.Tiles.AstralSnow;
using CalamityMod.Tiles.Crags;
using CalamityMod.Tiles.FloralParadise;
using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.Ores;
using CalamityMod.Tiles.SunkenSea;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Walls;
using Terraria.ID;

namespace CalamityMod.Systems
{
    public class BiomeTileCounterSystem : ModSystem
    {
        public static int BrimstoneCragTiles = 0;
        public static int SulphurTiles = 0;
        public static int AstralTiles = 0;
        public static int SunkenSeaTiles = 0;
        public static int SunkenSeaShoresTiles = 0;
        public static int SunkenSeaPolypTiles = 0;
        public static int SunkenSeaReefsTiles = 0;
        public static int SunkenSeaBurrowsTiles = 0;
        public static int SunkenSeaBasaltTiles = 0;
        public static int ArsenalLabTiles = 0;
        public static int AbyssTiles = 0;
        public static int FloralParadiseTiles = 0;
        public static int UndergroundTiles = 0;
        public static int Layer1Tiles = 0;
        public static int Layer2Tiles = 0;
        public static int Layer3Tiles = 0;
        public static int Layer4Tiles = 0;

        public override void ResetNearbyTileEffects()
        {
            BrimstoneCragTiles = 0;
            AstralTiles = 0;
            SunkenSeaTiles = 0;
            SunkenSeaShoresTiles = 0;
            SunkenSeaPolypTiles = 0;
            SunkenSeaReefsTiles = 0;
            SunkenSeaBurrowsTiles = 0;
            SunkenSeaBasaltTiles = 0;
            SulphurTiles = 0;
            AbyssTiles = 0;
            ArsenalLabTiles = 0;
            UndergroundTiles = 0;

            Layer1Tiles = 0;
            Layer2Tiles = 0;
            Layer3Tiles = 0;
            Layer4Tiles = 0;
            FloralParadiseTiles = 0;
        }

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            BrimstoneCragTiles = tileCounts[ModContent.TileType<InfernalSuevite>()] + tileCounts[ModContent.TileType<BrimstoneSlag>()];
            SunkenSeaTiles = tileCounts[ModContent.TileType<EutrophicSand>()] + tileCounts[ModContent.TileType<HardenedEutrophicSand>()] + tileCounts[ModContent.TileType<Navystone>()] + tileCounts[ModContent.TileType<Limestone>()] + tileCounts[ModContent.TileType<PolypSand>()] + tileCounts[ModContent.TileType<Dunesand>()] + tileCounts[ModContent.TileType<Runestone>()] + tileCounts[ModContent.TileType<Shellstone>()];
            SunkenSeaShoresTiles = tileCounts[ModContent.TileType<Runestone>()] + tileCounts[ModContent.TileType<Dunesand>()] + tileCounts[ModContent.WallType<RunestoneWall>()] + tileCounts[ModContent.TileType<AridSoil>()];
            SunkenSeaPolypTiles = tileCounts[ModContent.TileType<Limestone>()] + tileCounts[ModContent.TileType<PolypSand>()] + tileCounts[ModContent.TileType<ScarletSeaGrassTile>()];
            SunkenSeaReefsTiles = tileCounts[ModContent.TileType<Shellstone>()] + tileCounts[ModContent.TileType<EutrophicSand>()] + tileCounts[ModContent.TileType<YellowCoral>()] + tileCounts[ModContent.TileType<OrangeCoral>()] + tileCounts[ModContent.TileType<CyanCoral>()] + tileCounts[ModContent.TileType<LimeCoral>()] + tileCounts[ModContent.TileType<MagentaCoral>()];
            SunkenSeaBurrowsTiles = tileCounts[ModContent.TileType<Navystone>()] + tileCounts[ModContent.TileType<HardenedEutrophicSand>()] + tileCounts[ModContent.TileType<WhitePearlPile>()] + tileCounts[ModContent.TileType<BlackPearlPile>()] + tileCounts[ModContent.TileType<PinkPearlPile>()] + tileCounts[ModContent.TileType<SeaPrism>()];
            SunkenSeaBasaltTiles = tileCounts[ModContent.TileType<Basalt>()] + tileCounts[ModContent.TileType<VolcanicSand>()];
            AbyssTiles = tileCounts[ModContent.TileType<AbyssGravel>()] + tileCounts[ModContent.TileType<Voidstone>()];
            SulphurTiles = tileCounts[ModContent.TileType<SulphurousSand>()] + tileCounts[ModContent.TileType<SulphurousSandstone>()] + tileCounts[ModContent.TileType<HardenedSulphurousSandstone>()];
            FloralParadiseTiles = tileCounts[ModContent.TileType<AlgalSlate>()] + tileCounts[ModContent.TileType<PeatMoss>()] + tileCounts[ModContent.TileType<Peat>()];
            ArsenalLabTiles = tileCounts[ModContent.TileType<LaboratoryPanels>()] + tileCounts[ModContent.TileType<LaboratoryPlating>()] + tileCounts[ModContent.TileType<HazardChevronPanels>()];
            UndergroundTiles = tileCounts[TileID.Stone];

            Layer1Tiles = tileCounts[ModContent.TileType<SulphurousShale>()];
            Layer2Tiles = tileCounts[ModContent.TileType<AbyssGravel>()] + tileCounts[ModContent.TileType<PlantyMush>()];
            Layer3Tiles = tileCounts[ModContent.TileType<PyreMantle>()];
            Layer4Tiles = tileCounts[ModContent.TileType<Voidstone>()];

            int astralDesertTiles = tileCounts[ModContent.TileType<AstralSand>()] + tileCounts[ModContent.TileType<AstralSandstone>()] + tileCounts[ModContent.TileType<HardenedAstralSand>()] + tileCounts[ModContent.TileType<CelestialRemains>()];
            int astralSnowTiles = tileCounts[ModContent.TileType<AstralIce>()] + tileCounts[ModContent.TileType<AstralSnow>()];

            Main.SceneMetrics.SandTileCount += astralDesertTiles;
            Main.SceneMetrics.SnowTileCount += astralSnowTiles;

            AstralTiles = astralDesertTiles + astralSnowTiles + tileCounts[ModContent.TileType<AstralDirt>()] + tileCounts[ModContent.TileType<AstralStone>()] + tileCounts[ModContent.TileType<AstralGrass>()] + tileCounts[ModContent.TileType<AstralOre>()] + tileCounts[ModContent.TileType<NovaeSlag>()] + tileCounts[ModContent.TileType<AstralClay>()];
        }
    }
}
