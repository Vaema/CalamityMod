namespace CalamityMod.Graphics.Primitives;

/// <summary>
/// Controls how strip width is attenuated along the path.
/// 
/// <list type="bullet">
/// <item><description><see cref="None"/>: No attenuation is applied; width comes solely from the provided width function/constant.</description></item>
/// <item><description><see cref="ContinuitySquared"/>: Attenuates width based on directional continuity between consecutive tangents.</description></item>
/// </list>
/// </summary>
public enum StripWidthAttenuation
{
    None,
    ContinuitySquared
}
