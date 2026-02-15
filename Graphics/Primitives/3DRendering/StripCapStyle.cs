namespace CalamityMod.Graphics.Primitives;

/// <summary>
/// Determines the shape of end caps for strip primitives. This is used to give a more polished look to the start and end of strips, especially when they are thick. The cap is drawn at the end of the strip, centered on the last vertex, and oriented to match the strip's direction at that point. The options are:
/// <list type="bullet">
/// <item><description><see cref="None"/>: No cap is drawn, leaving the strip to end abruptly at the last vertex. This is the default and most performant option.</description></item>
/// <item><description><see cref="Triangle"/>: A simple triangular cap is drawn, extending the strip in a pointed shape. The triangle's base is aligned with the last segment of the strip, and its height is proportional to the strip's width.</description></item>
/// <item><description><see cref="HalfCircle"/>: A semicircular cap is drawn, creating a rounded end to the strip.</description></item>
/// </list>
/// </summary>
public enum StripCapStyle
{
    None,
    Triangle,
    HalfCircle
}
