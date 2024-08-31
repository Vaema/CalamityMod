using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
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

        public override int ChooseWaterfallStyle()
        {
            return ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaBurrowsWaterflow").Slot;
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

        public override int GetSplashDust() => DustID.Water;
        public override int GetDropletGore() => GoreID.WaterDripCavern;
        public override Color BiomeHairColor() => Color.Blue;
    }
}
