using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SulphuricWater : ModWaterStyle
    {
        public static int Type;
        public override void SetStaticDefaults()
        {
            Type = Slot;
        }
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/SulphuricWaterflow").Slot;
        public override int GetSplashDust() => 101;
        public override int GetDropletGore() => 708;
        public override Asset<Texture2D> GetRainTexture() => ModContent.Request<Texture2D>("CalamityMod/Waters/SulphuricRain");
        public override byte GetRainVariant() => (byte)Main.rand.Next(3);
        public override Color BiomeHairColor() => Color.Turquoise;
    }
}
