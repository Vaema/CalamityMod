using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Fargo/Calamity's NearbyEffects hook only handles four candle tiles but its
/// original body resolves buff and tile IDs for every visited tile.  Reject all
/// other tile IDs before entering that body; matching tiles execute the exact
/// original implementation.
/// </summary>
internal static class BuffStationTileFastRejectPatch
{
    private const string CrossmodName = "FargowiltasCrossmod";
    private const string TargetTypeName = "FargowiltasCrossmod.Core.Calamity.Globals.CalDLCTileChanges";

    private static readonly string[] CandleNames =
    {
        "BlueCandle", "PurpleCandle", "PinkCandle", "YellowCandle"
    };

    private static Mod? owner;
    private static MethodInfo? target;
    private static ILHook? hook;
    private static int[] candleTypes = Array.Empty<int>();
    private static volatile bool enabled;
    private static bool available;
    private static string compatibility = "not-resolved";
    private static long checks;
    private static long rejected;
    private static long passed;

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static long Checks => Interlocked.Read(ref checks);
    public static long Rejected => Interlocked.Read(ref rejected);
    public static long Passed => Interlocked.Read(ref passed);
    public static string Mode => enabled ? "four-tile-exact-gate" : "crossmod-original";

    public static void Load(Mod mod)
    {
        owner = mod;
        enabled = false;
        available = false;
        compatibility = "awaiting-post-setup";
        ResetCounters();
    }

    public static void PostSetupContent()
    {
        available = false;
        target = null;
        candleTypes = Array.Empty<int>();

        if (!ModLoader.TryGetMod(CrossmodName, out Mod? crossmod))
        {
            compatibility = "crossmod-not-loaded";
            return;
        }

        try
        {
            Type type = crossmod.Code.GetType(TargetTypeName, throwOnError: true)!;
            target = type.GetMethod(
                "BuffStationEffect",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(int), typeof(int), typeof(int), typeof(bool) },
                modifiers: null)
                ?? throw new MissingMethodException(TargetTypeName, "BuffStationEffect(int,int,int,bool)");

            Mod calamity = ModLoader.GetMod("CalamityMod");
            candleTypes = CandleNames.Select(name => (int)calamity.Find<ModTile>(name).Type).ToArray();
            if (candleTypes.Length != 4 || candleTypes.Distinct().Count() != 4)
                throw new InvalidOperationException("The four Calamity candle tile IDs were not unique.");

            available = true;
            compatibility = $"audited-{crossmod.Version};tiles={string.Join(',', candleTypes)}";
            owner?.Logger.Info($"Fargo buff-station fast reject resolved: {compatibility}; state is OFF.");
        }
        catch (Exception exception)
        {
            compatibility = $"resolve-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"Fargo buff-station fast reject unavailable: {compatibility}");
        }
    }

    public static void Unload()
    {
        SetEnabled(false);
        owner = null;
        target = null;
        candleTypes = Array.Empty<int>();
        available = false;
        compatibility = "unloaded";
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (!value)
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            ResetCounters();
            return;
        }

        if (enabled)
            return;
        if (!available || target is null)
        {
            enabled = false;
            return;
        }

        try
        {
            hook = new ILHook(target, InjectTileGate);
            enabled = true;
            ResetCounters();
        }
        catch (Exception exception)
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            available = false;
            compatibility = $"il-install-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Error($"Fargo buff-station fast reject disabled: {compatibility}");
        }
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref checks, 0);
        Interlocked.Exchange(ref rejected, 0);
        Interlocked.Exchange(ref passed, 0);
    }

    private static void InjectTileGate(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel continueOriginal = cursor.DefineLabel();
        cursor.Emit(OpCodes.Ldarg_2);
        cursor.EmitDelegate(ShouldReject);
        cursor.Emit(OpCodes.Brfalse, continueOriginal);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(continueOriginal);
    }

    private static bool ShouldReject(int tileType)
    {
        CalamityPatchDiagnostics.Increment(ref checks);
        int[] ids = candleTypes;
        for (int i = 0; i < ids.Length; i++)
        {
            if (tileType == ids[i])
            {
                CalamityPatchDiagnostics.Increment(ref passed);
                return false;
            }
        }

        CalamityPatchDiagnostics.Increment(ref rejected);
        return true;
    }
}
