using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SunkenSeaBurrowsWaterflow : ModWaterfallStyle { }

    public class SunkenSeaBurrowsWater : CalamityModWaterStyle
    {
        private readonly Vector3 WaterGlowColor = new Color(76, 211, 231).ToVector3();

        public static int Type { get; private set; }
        public static CalamityModWaterStyle Instance { get; private set; }

        public override void SetStaticDefaults()
        {
            Type = Slot;
            Instance = this;
        }

        public override void Unload()
        {
            Type = -1;
            Instance = null;
        }

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 1.02f;
            g = 1.03f;
            b = 1.075f;
        }

        public override void ModifyLight(ref readonly Tile tile, int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 outputColor = new Vector3(r, g, b);

            if (tile.TileType != RustyChestTile.TileType)
            {
                CalamityUtils.SunkenSeaWaterLighting(i, j, WaterGlowColor, ref outputColor.X, ref outputColor.Y, ref outputColor.Z);
            }

            r = outputColor.X;
            g = outputColor.Y;
            b = outputColor.Z;
        }
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaBurrowsWaterflow").Slot;
        public override int GetSplashDust() => ModContent.DustType<SunkenSeaBurrowsSplash>();
        public override int GetDropletGore() => ModContent.GoreType<SunkenSeaBurrowsWaterDroplet>();
        public override Color BiomeHairColor() => Color.Blue;
    }
}
