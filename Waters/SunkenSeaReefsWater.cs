using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SunkenSeaReefsWaterflow : ModWaterfallStyle { }

    public class SunkenSeaReefsWater : CalamityModWaterStyle
    {
        private readonly Vector3 WaterGlowColor = new Color(140, 222, 240).ToVector3();

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

        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaReefsWaterflow").Slot;
        public override int GetSplashDust() => ModContent.DustType<SunkenSeaReefsSplash>();
        public override int GetDropletGore() => ModContent.GoreType<SunkenSeaReefsWaterDroplet>();
        public override Color BiomeHairColor() => Color.SkyBlue;
    }
}
