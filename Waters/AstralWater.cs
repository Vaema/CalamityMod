using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class AstralWater : ModWaterStyle
    {
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/AstralWaterflow").Slot;
        public override int GetSplashDust() => 52; //corruption water?
        public override int GetDropletGore() => ModContent.Find<ModGore>("CalamityMod/AstralWaterDroplet").Type;
        public override Asset<Texture2D> GetRainTexture() => ModContent.Request<Texture2D>("CalamityMod/Waters/AstralRain");
        public override byte GetRainVariant() => (byte)Main.rand.Next(3);
        public override Color BiomeHairColor() => Color.MediumPurple;
    }
}
