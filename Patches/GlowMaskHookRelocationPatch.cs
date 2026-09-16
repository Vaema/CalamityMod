using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Removes Calamity's MonoMod hook from DrawSingleTile and invokes the same glow-mask
/// renderer from TileDrawing.Draw. The detour therefore runs once per drawing layer,
/// while the per-tile path is a direct static call and rejects non-glow tile types
/// before invoking Calamity's delegate.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
public static class GlowMaskHookRelocationPatch
{
    private static On_TileDrawing.hook_DrawSingleTile? calamityGlowHook;
    private static readonly On_TileDrawing.orig_DrawSingleTile NoopOriginalDelegate = NoopOriginal;
    private static FieldInfo? instanceLookupField;
    private static bool[] glowTileTypes = [];
    private static ILHook? drawHook;
    private static bool enabled;

    public static bool Enabled => enabled;
    public static string Mode => enabled ? "draw-loop-direct" : "calamity-original";
    public static int GlowTypeCount { get; private set; }

    public static void Load(Mod owner)
    {
        Assembly calamity = typeof(CalamityMod).Assembly;
        calamityGlowHook = (On_TileDrawing.hook_DrawSingleTile)FindHook(
            calamity,
            "GlowMaskTileRender",
            typeof(On_TileDrawing.hook_DrawSingleTile));

        Type glowMaskType = calamity.GetType("CalamityMod.Tiles.GlowMaskTile", throwOnError: true)!;
        instanceLookupField = glowMaskType.GetField("InstanceLookup", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(glowMaskType.FullName, "InstanceLookup");

        MethodInfo draw = FindDrawTarget();
        owner.Logger.Info($"P1 glow target resolved: {draw.DeclaringType!.FullName}.{draw.Name}; state is OFF.");
    }

    public static void PostSetupContent(Mod owner)
    {
        RefreshLookup();
        owner.Logger.Info($"P1 glow lookup ready: {GlowTypeCount} glow tile types.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        calamityGlowHook = null;
        instanceLookupField = null;
        glowTileTypes = [];
        GlowTypeCount = 0;
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
        // Deliberately no hot-loop counters.
    }

    private static void Enable()
    {
        if (glowTileTypes.Length == 0)
            RefreshLookup();
        if (GlowTypeCount == 0)
            throw new InvalidOperationException("Calamity glow-mask lookup is empty after content setup.");

        On_TileDrawing.DrawSingleTile -= calamityGlowHook!;
        try
        {
            drawHook = new ILHook(FindDrawTarget(), InjectAfterDrawSingleTile);
        }
        catch
        {
            drawHook?.Dispose();
            drawHook = null;
            On_TileDrawing.DrawSingleTile += calamityGlowHook!;
            throw;
        }
    }

    private static void Disable()
    {
        drawHook?.Dispose();
        drawHook = null;
        On_TileDrawing.DrawSingleTile += calamityGlowHook!;
    }

    private static void RefreshLookup()
    {
        Array instances = instanceLookupField?.GetValue(null) as Array
            ?? throw new InvalidOperationException("Calamity GlowMaskTile.InstanceLookup is not initialized.");

        bool[] lookup = new bool[instances.Length];
        int count = 0;
        for (int type = 0; type < instances.Length; type++)
        {
            if (instances.GetValue(type) is null)
                continue;
            lookup[type] = true;
            count++;
        }

        glowTileTypes = lookup;
        GlowTypeCount = count;
    }

    private static Delegate FindHook(Assembly assembly, string methodName, Type delegateType) =>
        assembly.GetTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(field => field.FieldType == delegateType)
            .Select(field => field.GetValue(null))
            .OfType<Delegate>()
            .Single(hook => hook.Method.Name == methodName);

    private static MethodInfo FindDrawTarget() =>
        typeof(TileDrawing)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(method => method.Name == nameof(TileDrawing.Draw) && method.GetParameters().Length == 4);

    private static void InjectAfterDrawSingleTile(ILContext il)
    {
        MethodInfo drawSingleTile = typeof(TileDrawing)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method => method.Name == "DrawSingleTile" && method.GetParameters().Length == 7);
        MethodInfo directApply = typeof(GlowMaskHookRelocationPatch).GetMethod(
            nameof(ApplyGlowDirect),
            BindingFlags.Static | BindingFlags.Public)!;

        ILCursor cursor = new(il);
        int drawDataLocal = -1;
        int screenPositionLocal = -1;
        int screenOffsetLocal = -1;
        int tileXLocal = -1;
        int tileYLocal = -1;

        bool found = cursor.TryGotoNext(
            MoveType.After,
            instruction => instruction.MatchLdarg(0),
            instruction => instruction.MatchLdloc(out drawDataLocal),
            instruction => instruction.MatchLdarg(1),
            instruction => instruction.MatchLdarg(4),
            instruction => instruction.MatchLdloc(out screenPositionLocal),
            instruction => instruction.MatchLdloc(out screenOffsetLocal),
            instruction => instruction.MatchLdloc(out tileXLocal),
            instruction => instruction.MatchLdloc(out tileYLocal),
            instruction => instruction.MatchCallOrCallvirt(drawSingleTile));

        if (!found || drawDataLocal < 0 || screenPositionLocal < 0 || screenOffsetLocal < 0 || tileXLocal < 0 || tileYLocal < 0)
            throw new InvalidOperationException("Could not locate TileDrawing.Draw's unique DrawSingleTile call and locals.");

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitLdloc(drawDataLocal);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldarg_S, (byte)4);
        cursor.EmitLdloc(screenPositionLocal);
        cursor.EmitLdloc(screenOffsetLocal);
        cursor.EmitLdloc(tileXLocal);
        cursor.EmitLdloc(tileYLocal);
        cursor.Emit(OpCodes.Call, directApply);

        ILCursor uniquenessCheck = new(il);
        int callCount = 0;
        while (uniquenessCheck.TryGotoNext(MoveType.After,
                   instruction => instruction.MatchCallOrCallvirt(drawSingleTile)))
            callCount++;

        if (callCount != 1)
            throw new InvalidOperationException($"Expected one DrawSingleTile call in TileDrawing.Draw, found {callCount}.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyGlowDirect(
        TileDrawing self,
        TileDrawInfo drawData,
        bool solidLayer,
        int waterStyleOverride,
        Vector2 screenPosition,
        Vector2 screenOffset,
        int tileX,
        int tileY)
    {
        int type = drawData.typeCache;
        bool[] lookup = glowTileTypes;
        if ((uint)type >= (uint)lookup.Length || !lookup[type])
            return;

        calamityGlowHook!(NoopOriginalDelegate, self, drawData, solidLayer, waterStyleOverride,
            screenPosition, screenOffset, tileX, tileY);
    }

    private static void NoopOriginal(
        TileDrawing self,
        TileDrawInfo drawData,
        bool solidLayer,
        int waterStyleOverride,
        Vector2 screenPosition,
        Vector2 screenOffset,
        int tileX,
        int tileY)
    {
    }
}
