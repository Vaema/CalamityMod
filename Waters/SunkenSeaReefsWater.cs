using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SunkenSeaReefsWaterflow : ModWaterfallStyle { }

    public class SunkenSeaReefsWater : CalamityModWaterStyle
    {
        public static int Type;

        private readonly Vector3 WaterGlowColor = new Color(140, 222, 239).ToVector3();

        public override void SetStaticDefaults()
        {
            Type = Slot;
        }

        public override void ModifyLight(ref readonly Tile tile, int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 outputColor = new Vector3(r, g, b);
            if (outputColor == Vector3.One || outputColor == new Vector3(0.25f, 0.25f, 0.25f) || outputColor == new Vector3(0.5f, 0.5f, 0.5f))
                return;

            if (tile.TileType != RustyChestTile.Type)
            {
                CalamityUtils.SunkenSeaWaterLighting(i, j, WaterGlowColor, ref outputColor.X, ref outputColor.Y, ref outputColor.Z);
            }

            r = outputColor.X;
            g = outputColor.Y;
            b = outputColor.Z;
        }

        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaReefsWaterflow").Slot;
        public override int GetSplashDust() => DustID.Water;
        public override int GetDropletGore() => GoreID.WaterDripCavern;
        public override Color BiomeHairColor() => Color.SkyBlue;
    }
}
