using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Waters;

public class BasaltGullyLava : ModLavaStyle
{
    public override string WaterfallTexture => "CalamityMod/Waters/BasaltGullyLavaflow";

    public override int GetSplashDust() => ModContent.DustType<BasaltGullyLavaSplash>();

    public override int GetDropletGore() => ModContent.GoreType<BasaltGullyLavaDroplet>();

    public override bool IsLavaActive() => Main.LocalPlayer.Calamity().ZoneBasaltGully;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 248f / 255;
        g = 73f / 255;
        b = 73f / 255;
    }
}
