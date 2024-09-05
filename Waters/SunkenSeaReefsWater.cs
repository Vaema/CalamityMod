using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SunkenSeaReefsWater : ModWaterStyle
    {
        public static int Type;
        public override void SetStaticDefaults() => Type = Slot;
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaReefsWaterflow").Slot;

        public override int GetSplashDust() => 33;

        public override int GetDropletGore() => 713;

        public override Color BiomeHairColor() => Color.SkyBlue;
    }
}
