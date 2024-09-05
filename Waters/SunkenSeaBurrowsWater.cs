using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SunkenSeaBurrowsWater : ModWaterStyle
    {
        public static int Type;
        public override void SetStaticDefaults() => Type = Slot;
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaBurrowsWaterflow").Slot;

        public override int GetSplashDust() => 33;

        public override int GetDropletGore() => 713;

        public override Color BiomeHairColor() => Color.Blue;

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 1.02f;
            g = 1.03f;
            b = 1.075f;
        }
    }
}
