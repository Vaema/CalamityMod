using CalamityMod.Tiles.Astral;
using CalamityMod.Tiles.AstralDesert;
using CalamityMod.Tiles.AstralSnow;
using CalamityMod.Tiles.FurnitureMonolith;
using CalamityMod.Tiles.Ores;
using CalamityMod.Walls;
using CalamityMod.Walls.UnsafeWalls;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.World
{
    public class AstralConversion : ModBiomeConversion
    {
        public static int GrassType;
        public static int DirtType;
        public static int StoneType;
        public static int SandType;
        public static int SandstoneType;
        public static int HardenedSandType;
        public static int SnowType;
        public static int IceType;
        public static int ClayType;
        public static int SiltType;
        public static int FossilType;
        public static int WoodType;
        public static int OreType;

        public static int GrassWallType;
        public static int DirtWallType;
        public static int StoneWallType;
        public static int SnowWallType;
        public static int IceWallType;
        public static int SandstoneWallType;
        public static int HardenedSandWallType;
        public static int FossilWallType;
        public static int WoodWallType;

        public override void PostSetupContent()
        {
            GrassType = TileType<AstralGrass>();
            DirtType = TileType<AstralDirt>();
            StoneType = TileType<AstralStone>();
            SnowType = TileType<AstralSnow>();
            IceType = TileType<AstralIce>();
            SandType = TileType<AstralSand>();
            SandstoneType = TileType<AstralSandstone>();
            HardenedSandType = TileType<HardenedAstralSand>();
            ClayType = TileType<AstralClay>();
            SiltType = TileType<NovaeSlag>();
            FossilType = TileType<CelestialRemains>();
            WoodType = TileType<AstralMonolith>();
            OreType = TileType<AstralOre>();

            DirtWallType = WallType<UnsafeAstralDirtWall>();
            GrassWallType = WallType<UnsafeAstralGrassWall>();
            StoneWallType = WallType<UnsafeAstralStoneWall>();
            SnowWallType = WallType<UnsafeAstralSnowWall>();
            IceWallType = WallType<UnsafeAstralIceWall>();
            SandstoneWallType = WallType<UnsafeAstralSandstoneWall>();
            HardenedSandWallType = WallType<UnsafeHardenedAstralSandWall>();
            FossilWallType = WallType<CelestialRemainsWall>();
            WoodWallType = WallType<AstralMonolithWall>();

            TileLoader.RegisterSimpleConversion(TileID.Grass, Type, GrassType);
            TileLoader.RegisterSimpleConversion(TileID.Dirt, Type, DirtType);
            TileLoader.RegisterSimpleConversion(TileID.Stone, Type, StoneType);
            TileLoader.RegisterSimpleConversion(TileID.SnowBlock, Type, SnowType);
            TileLoader.RegisterSimpleConversion(TileID.IceBlock, Type, IceType);
            TileLoader.RegisterSimpleConversion(TileID.Sand, Type, SandType);
            TileLoader.RegisterSimpleConversion(TileID.Sandstone, Type, SandstoneType);
            TileLoader.RegisterSimpleConversion(TileID.HardenedSand, Type, HardenedSandType);
            TileLoader.RegisterSimpleConversion(TileID.ClayBlock, Type, ClayType);
            TileLoader.RegisterSimpleConversion(TileID.Silt, Type, SiltType);
            TileLoader.RegisterSimpleConversion(TileID.DesertFossil, Type, FossilType);
            TileLoader.RegisterSimpleConversion(TileID.LivingWood, Type, WoodType);
            TileLoader.RegisterSimpleConversion(TileID.Meteorite, Type, OreType);

            WallLoader.RegisterSimpleConversion(WallID.GrassUnsafe, Type, GrassWallType);
            WallLoader.RegisterSimpleConversion(WallID.DirtUnsafe, Type, DirtWallType);
            WallLoader.RegisterSimpleConversion(WallID.Stone, Type, StoneWallType);
            WallLoader.RegisterSimpleConversion(WallID.SnowWallUnsafe, Type, SnowWallType);
            WallLoader.RegisterSimpleConversion(WallID.IceUnsafe, Type, IceWallType);
            WallLoader.RegisterSimpleConversion(WallID.Sandstone, Type, SandstoneWallType);
            WallLoader.RegisterSimpleConversion(WallID.HardenedSand, Type, HardenedSandWallType);
            WallLoader.RegisterSimpleConversion(WallID.DesertFossil, Type, FossilWallType);
            WallLoader.RegisterSimpleConversion(WallID.LivingWoodUnsafe, Type, WoodWallType);

            // Evil solutions cleanse back certain common/abundant Astral tiles
            TileLoader.RegisterConversion(DirtType, BiomeConversionID.Corruption, TileID.Dirt);
            TileLoader.RegisterConversion(DirtType, BiomeConversionID.Crimson, TileID.Dirt);
            TileLoader.RegisterConversion(DirtType, BiomeConversionID.Hallow, TileID.Dirt);
            TileLoader.RegisterConversion(SnowType, BiomeConversionID.Corruption, TileID.SnowBlock);
            TileLoader.RegisterConversion(SnowType, BiomeConversionID.Crimson, TileID.SnowBlock);
            TileLoader.RegisterConversion(SnowType, BiomeConversionID.Hallow, TileID.SnowBlock);
        }
    }
}
