using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// CalamityGlobalNPC.PostAI builds a perimeter List&lt;Point&gt; every tick for
/// town NPCs and SuperDummyNPCs while checking Auric Ore/Repulsers. The list is
/// temporary, fully overwritten before it is read, and PostAI runs on the game
/// thread. Reuse one list per thread while preserving Calamity's original Add,
/// enumeration, tile-check order, collision response, damage and sound logic.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class AuricEdgeTileListReusePatch
{
    [ThreadStatic]
    private static List<Point>? threadEdgeTiles;

    private static ILHook? hook;
    private static bool enabled;
    private static long rents;
    private static long observedCapacityGrowths;
    private static int highWaterCapacity;

    public static bool Enabled => enabled;
    public static long Rents => Interlocked.Read(ref rents);
    public static long ObservedCapacityGrowths => Interlocked.Read(ref observedCapacityGrowths);
    public static int HighWaterCapacity => Volatile.Read(ref highWaterCapacity);
    public static string Mode => enabled ? "thread-local-exact-list-reuse" : "calamity-new-list-per-npc-tick";

    public static void Load(Mod owner)
    {
        MethodInfo postAi = typeof(CalamityGlobalNPC).GetMethod(
            nameof(CalamityGlobalNPC.PostAI),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(NPC)],
            modifiers: null)
            ?? throw new MissingMethodException(typeof(CalamityGlobalNPC).FullName, nameof(CalamityGlobalNPC.PostAI));

        hook = new ILHook(postAi, ReplaceEdgeTileListConstruction);
        enabled = false;
        ResetCounters();
        owner.Logger.Info("Auric NPC edge-tile list reuse installed at one exact CalamityGlobalNPC.PostAI site; state is OFF.");
    }

    public static void Unload()
    {
        enabled = false;
        hook?.Dispose();
        hook = null;
        threadEdgeTiles = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        enabled = value;
        ResetCounters();
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref rents, 0);
        Interlocked.Exchange(ref observedCapacityGrowths, 0);
        Volatile.Write(ref highWaterCapacity, threadEdgeTiles?.Capacity ?? 0);
    }

    private static void ReplaceEdgeTileListConstruction(ILContext il)
    {
        ILCursor cursor = new(il);
        int replaced = 0;
        while (cursor.TryGotoNext(MoveType.Before, IsPointListConstructor))
        {
            cursor.Remove();
            cursor.EmitDelegate(GetEdgeTiles);
            replaced++;
        }

        if (replaced != 1)
            throw new InvalidOperationException($"Expected one Auric edge-tile List<Point> construction in CalamityGlobalNPC.PostAI, found {replaced}.");
    }

    private static bool IsPointListConstructor(Mono.Cecil.Cil.Instruction instruction) =>
        instruction.Operand is MethodReference method &&
        method.Name == ".ctor" &&
        method.Parameters.Count == 0 &&
        method.DeclaringType.FullName == "System.Collections.Generic.List`1<Microsoft.Xna.Framework.Point>";

    private static List<Point> GetEdgeTiles()
    {
        if (!enabled)
            return new List<Point>();

        List<Point> list = threadEdgeTiles ??= new List<Point>(64);
        int capacity = list.Capacity;
        int previousHighWater = Volatile.Read(ref highWaterCapacity);
        if (capacity > previousHighWater)
        {
            Volatile.Write(ref highWaterCapacity, capacity);
            CalamityPatchDiagnostics.Increment(ref observedCapacityGrowths);
        }

        list.Clear();
        CalamityPatchDiagnostics.Increment(ref rents);
        return list;
    }
}
