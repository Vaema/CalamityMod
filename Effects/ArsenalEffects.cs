using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Effects;

public class ArsenalEffects
{
    public static int ArsenalDust = 278; // Colorable firework dust, simple and effective. Most visuals use a mix of the energy respective dust and this

    // Plasma
    public static int ArsenalPlasmaDust = ModContent.DustType<SquashDustTileTouch>();
    public static Color ArsenalPlasmaColor = new(154, 255, 0);
    // Laser
    public static int ArsenalLaserDust = ModContent.DustType<DiamondDust>();
    public static Color ArsenalLaserColor = Color.Lerp(Color.Crimson, Color.Red, 0.45f);
    // Pulse
    public static int ArsenalPulseDust = ModContent.DustType<SquashDustHollow>();
    public static Color ArsenalPulseColor = Color.Lerp(Color.DarkOrchid, Color.Magenta, 0.35f);
    // Electric
    public static int ArsenalElectricDust = ModContent.DustType<UnstableDust>();
    public static Color ArsenalElectricColor = Color.Lerp(Color.Aqua, Color.Aquamarine, 0.35f);
    // Gauss
    public static int ArsenalGaussDust = ModContent.DustType<SquareDust>();
    public static Color ArsenalGaussColor = Color.Lerp(Color.Yellow, Color.Goldenrod, 0.25f);
}
