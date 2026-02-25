namespace CalamityMod.Graphics.Primitives;

/// <summary>
/// Specifies how to join segments in a strip mesh.
/// 
/// <list type="bullet">
/// <item><description><see cref="Perpendicular"/>: Uses the averaged per-segment right vectors to build a perpendicular join at corners. This is the default and most performant option.</description></item>
/// <item><description><see cref="Miter"/>: Uses a miter join that extends the edges to their intersection, with a built-in miter limit to avoid extreme spikes on sharp angles.</description></item>
/// </list>
/// </summary>
public enum StripJoinStyle
{
    Perpendicular,
    Miter
}
