using System;
using System.Reflection;
using System.Threading;
using CalamityMod.CalPlayer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// CalamityPlayer.PreUpdateMovement constructs a display-class at the start of
/// every Main.ActiveNPCs iteration, before it knows whether that NPC is a
/// Ravager RockPillar. The object only carries the current CalamityPlayer and
/// NPC into a synchronous Enumerable.Any predicate. Reuse one instance per
/// thread while leaving every original field store, collision test, predicate,
/// branch and movement response intact.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class PlayerMovementNpcClosureReusePatch
{
    private const string ClosureName = "<>c__DisplayClass1173_0";

    [ThreadStatic]
    private static object? threadClosure;

    private static MethodInfo? targetMethod;
    private static Type? closureType;
    private static ILHook? hook;
    private static bool enabled;
    private static long hits;
    private static long misses;

    public static bool Enabled => enabled;
    public static long Hits => Interlocked.Read(ref hits);
    public static long Misses => Interlocked.Read(ref misses);
    public static string Mode => enabled ? "thread-local-exact-active-npc-closure-reuse" : "calamity-new-closure-per-active-npc";

    public static void Load(Mod owner)
    {
        targetMethod = typeof(CalamityPlayer).GetMethod(
            nameof(CalamityPlayer.PreUpdateMovement),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null)
            ?? throw new MissingMethodException(typeof(CalamityPlayer).FullName, nameof(CalamityPlayer.PreUpdateMovement));

        closureType = typeof(CalamityPlayer).GetNestedType(ClosureName, BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(CalamityPlayer).FullName, ClosureName);

        enabled = false;
        ResetCounters();
        owner.Logger.Info("Player movement active-NPC display-class reuse resolved for CalamityPlayer.PreUpdateMovement; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        targetMethod = null;
        closureType = null;
        threadClosure = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
        {
            hook = new ILHook(targetMethod!, ReplaceClosureConstruction);
            enabled = true;
        }
        else
        {
            enabled = false;
            hook?.Dispose();
            hook = null;
            threadClosure = null;
        }

        ResetCounters();
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref hits, 0);
        Interlocked.Exchange(ref misses, 0);
    }

    private static void ReplaceClosureConstruction(ILContext il)
    {
        MethodInfo genericGetter = typeof(PlayerMovementNpcClosureReusePatch).GetMethod(
            nameof(GetReusableClosure),
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(typeof(PlayerMovementNpcClosureReusePatch).FullName, nameof(GetReusableClosure));
        MethodInfo closedGetter = genericGetter.MakeGenericMethod(closureType!);

        ILCursor cursor = new(il);
        int replaced = 0;
        while (cursor.TryGotoNext(MoveType.Before, IsTargetClosureConstructor))
        {
            // This newobj is the loop header and therefore a branch target.
            // Removing it makes the back-edge jump past the replacement call,
            // leaving the display-class local uninitialized and producing
            // InvalidProgramException during JIT compilation. Mutate the
            // instruction in place so all incoming branch labels remain bound.
            Instruction constructor = cursor.Next!;
            constructor.OpCode = OpCodes.Call;
            constructor.Operand = il.Method.Module.ImportReference(closedGetter);
            cursor.Index++;
            replaced++;
        }

        if (replaced != 1)
            throw new InvalidOperationException($"Expected one {ClosureName} construction in CalamityPlayer.PreUpdateMovement, found {replaced}.");
    }

    private static bool IsTargetClosureConstructor(Instruction instruction) =>
        instruction.Operand is MethodReference method &&
        method.Name == ".ctor" &&
        method.Parameters.Count == 0 &&
        method.DeclaringType.Name == ClosureName &&
        method.DeclaringType.DeclaringType?.FullName == typeof(CalamityPlayer).FullName;

    private static T GetReusableClosure<T>() where T : class
    {
        if (threadClosure is T cached)
        {
            CalamityPatchDiagnostics.Increment(ref hits);
            return cached;
        }

        T created = (T)(Activator.CreateInstance(typeof(T), nonPublic: true)
            ?? throw new InvalidOperationException($"Could not create {typeof(T).FullName}."));
        threadClosure = created;
        CalamityPatchDiagnostics.Increment(ref misses);
        return created;
    }
}
