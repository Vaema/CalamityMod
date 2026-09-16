using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Relocates Calamity's special-liquid light hook from GetTileLight (once per tile)
/// to the ExportTo worker (once per parallel range). The per-tile path contains only
/// one normal static call and a cheap predicate; it contains no MonoMod delegate lookup,
/// synchronization proxy, atomic counter, or allocation.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
public static class PerTileFastPathPatch
{
    private delegate void ModifyEmitDelegate(Tile tile, int x, int y, ref Vector3 color);

    private static On_TileLightScanner.hook_GetTileLight? calamityLiquidHook;
    private static ModifyEmitDelegate? modifyEmit;
    private static ILHook? exportWorkerHook;
    private static bool enabled;

    public static bool Enabled => enabled;
    public static string Mode => enabled ? "export-loop-direct" : "calamity-original";

    public static void Load(Mod owner)
    {
        Assembly calamity = typeof(CalamityMod).Assembly;
        calamityLiquidHook = (On_TileLightScanner.hook_GetTileLight)FindHook(
            calamity,
            "ApplyLiquidEmit",
            typeof(On_TileLightScanner.hook_GetTileLight));

        Type liquidType = calamity.GetType(
            "CalamityMod.Systems.Graphic.LiquidSystem.SpecialLiquidDrawingSystem",
            throwOnError: true)!;
        modifyEmit = liquidType
            .GetMethod("ModifyEmit", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<ModifyEmitDelegate>();

        // Resolve and validate the exact tML 2026.06 compiler-generated range worker now,
        // before any state is changed.
        MethodInfo worker = FindExportWorker();
        owner.Logger.Info($"P0 lighting target resolved: {worker.DeclaringType!.FullName}.{worker.Name}; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        calamityLiquidHook = null;
        modifyEmit = null;
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
            Enable();
        else
            Disable();

        enabled = value;
    }

    public static void ResetCounters()
    {
        // Intentionally empty. Per-tile counters caused measurable contention and
        // allocations in 0.2.2, so P0 has no instrumentation in the hot loop.
    }

    private static void Enable()
    {
        On_TileLightScanner.GetTileLight -= calamityLiquidHook!;
        try
        {
            exportWorkerHook = new ILHook(FindExportWorker(), InjectAfterVanillaGetTileLight);
        }
        catch
        {
            exportWorkerHook?.Dispose();
            exportWorkerHook = null;
            On_TileLightScanner.GetTileLight += calamityLiquidHook!;
            throw;
        }
    }

    private static void Disable()
    {
        exportWorkerHook?.Dispose();
        exportWorkerHook = null;
        On_TileLightScanner.GetTileLight += calamityLiquidHook!;
    }

    private static Delegate FindHook(Assembly assembly, string methodName, Type delegateType) =>
        assembly.GetTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(field => field.FieldType == delegateType)
            .Select(field => field.GetValue(null))
            .OfType<Delegate>()
            .Single(hook => hook.Method.Name == methodName);

    private static MethodInfo FindExportWorker()
    {
        MethodInfo[] matches = typeof(TileLightScanner)
            .GetNestedTypes(BindingFlags.NonPublic)
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(method => method.Name.Contains("<ExportTo>", StringComparison.Ordinal))
            .Where(method =>
            {
                ParameterInfo[] parameters = method.GetParameters();
                return method.ReturnType == typeof(void) &&
                       parameters.Length == 3 &&
                       parameters[0].ParameterType == typeof(int) &&
                       parameters[1].ParameterType == typeof(int) &&
                       parameters[2].ParameterType == typeof(object);
            })
            .ToArray();

        if (matches.Length != 1)
            throw new InvalidOperationException($"Expected one TileLightScanner.ExportTo worker, found {matches.Length}.");

        return matches[0];
    }

    private static void InjectAfterVanillaGetTileLight(ILContext il)
    {
        MethodInfo getTileLight = typeof(TileLightScanner).GetMethod(
            nameof(TileLightScanner.GetTileLight),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(int), typeof(int), typeof(Vector3).MakeByRefType()],
            modifiers: null)!;

        MethodInfo directApply = typeof(PerTileFastPathPatch).GetMethod(
            nameof(ApplyLiquidEmitDirect),
            BindingFlags.Static | BindingFlags.Public)!;

        ILCursor cursor = new(il);
        int xLocal = -1;
        int yLocal = -1;
        int colorLocal = -1;

        bool found = cursor.TryGotoNext(
            MoveType.After,
            instruction => instruction.MatchLdloc(out xLocal),
            instruction => instruction.MatchLdloc(out yLocal),
            instruction => instruction.MatchLdloca(out colorLocal),
            instruction => instruction.MatchCallOrCallvirt(getTileLight));

        if (!found || xLocal < 0 || yLocal < 0 || colorLocal < 0)
            throw new InvalidOperationException("Could not locate ExportTo's unique GetTileLight call and locals.");

        cursor.EmitLdloc(xLocal);
        cursor.EmitLdloc(yLocal);
        cursor.EmitLdloca(colorLocal);
        cursor.Emit(OpCodes.Call, directApply);

        // This exact build has one GetTileLight call in the worker. Refuse to load if
        // an update changes that invariant instead of silently double-applying light.
        ILCursor uniquenessCheck = new(il);
        int callCount = 0;
        while (uniquenessCheck.TryGotoNext(MoveType.After,
                   instruction => instruction.MatchCallOrCallvirt(getTileLight)))
            callCount++;

        if (callCount != 1)
            throw new InvalidOperationException($"Expected one GetTileLight call in ExportTo worker, found {callCount}.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyLiquidEmitDirect(int x, int y, ref Vector3 color)
    {
        Tile tile = Main.tile[x, y];
        if (!tile.HasTile && tile.LiquidAmount > 0)
            modifyEmit!(tile, x, y, ref color);
    }
}
