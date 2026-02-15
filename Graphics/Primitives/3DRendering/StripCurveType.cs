namespace CalamityMod.Graphics.Primitives;


/// <summary>
/// Specifies the type of curve interpolation to use when generating a strip mesh from a sequence of points.
/// 
/// <remarks>
/// The curve type determines how the strip will be shaped between the control points. 
/// Different curve types can produce smoother or sharper results, and may have different performance characteristics.
/// </remarks>
/// </summary>
public enum StripCurveType
{
    CatmullRom,
    Linear,
    CubicBezier,
    Hermite
}
