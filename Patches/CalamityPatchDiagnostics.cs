using System.Diagnostics;
using System.Threading;

namespace CalamityMod.Patches;

/// <summary>
/// Release builds compile hot-path counter calls out completely. Define
/// TOPTIMIZATION_HOT_COUNTERS only for a dedicated diagnostic build; ordinary
/// players should not pay an atomic-operation cost for chat statistics.
/// </summary>
internal static class CalamityPatchDiagnostics
{
    public const string Mode = "compiled-out-release";

    [Conditional("TOPTIMIZATION_HOT_COUNTERS")]
    public static void Increment(ref long counter) => Interlocked.Increment(ref counter);

    [Conditional("TOPTIMIZATION_HOT_COUNTERS")]
    public static void Add(ref long counter, long value) => Interlocked.Add(ref counter, value);
}
