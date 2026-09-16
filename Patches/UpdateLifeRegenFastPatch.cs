using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityMod.NPCs;
using CalamityMod.Systems.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Removes three independent pieces of repeated work from CalamityGlobalNPC.UpdateLifeRegen:
/// Enumerable.Any's array enumerator allocation, one dead SlimeGod.Contains result, and six
/// linear worm-type list scans. Hooks are rebuilt between benchmark cells, so enabled hot
/// paths contain direct IL calls and no diagnostic branches or counters.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class UpdateLifeRegenFastPatch
{
    private static readonly string[] WormSetNames =
    [
        "DesertScourge", "EaterOfWorlds", "Perforators",
        "AquaticScourge", "AstrumDeus", "StormWeaver",
    ];

    private static Mod? owner;
    private static MethodInfo? target;
    private static ILHook? hook;
    private static bool[] wormTypes = [];
    private static bool anyEnabled;
    private static bool deadCallEnabled;
    private static bool wormLookupEnabled;
    private static bool available;
    private static string compatibility = "content-not-resolved";

    public static bool AnyEnabled => anyEnabled;
    public static bool DeadCallEnabled => deadCallEnabled;
    public static bool WormLookupEnabled => wormLookupEnabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static string Mode => $"any={(anyEnabled ? "direct-loop" : "linq")};dead={(deadCallEnabled ? "removed" : "original")};worm={(wormLookupEnabled ? "bool-lookup" : "six-lists")}";

    public static void Load(Mod mod)
    {
        owner = mod;
        target = null;
        hook = null;
        wormTypes = [];
        anyEnabled = deadCallEnabled = wormLookupEnabled = available = false;
        compatibility = "content-not-resolved";
    }

    public static void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out Mod? calamity) || calamity.Version.ToString() != "2.2.2")
        {
            compatibility = "requires-calamity-2.2.2";
            return;
        }

        try
        {
            target = typeof(CalamityGlobalNPC).GetMethod(
                nameof(GlobalNPC.UpdateLifeRegen), BindingFlags.Instance | BindingFlags.Public,
                binder: null, types: [typeof(NPC), typeof(int).MakeByRefType()], modifiers: null)
                ?? throw new MissingMethodException(typeof(CalamityGlobalNPC).FullName, nameof(GlobalNPC.UpdateLifeRegen));

            BuildAndVerifyWormLookup();
            using ILHook validation = new(target, il => Rewrite(il, replaceAny: true, removeDeadCall: true, replaceWormChain: true));
            available = true;
            compatibility = $"audited-calamity-2.2.2-globalnpc-updateliferegen;wormtypes={CountTrue(wormTypes)}";
            owner?.Logger.Info("UpdateLifeRegen R10 Any/dead-call/worm-lookup anchors validated; all components are OFF.");
        }
        catch (Exception exception)
        {
            available = false;
            compatibility = $"anchor-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"UpdateLifeRegen R10 unavailable: {exception}");
        }
    }

    public static void SetComponents(bool directAny, bool removeDeadCall, bool wormLookup)
    {
        bool nextAny = directAny && available;
        bool nextDead = removeDeadCall && available;
        bool nextWorm = wormLookup && available;
        if (nextAny == anyEnabled && nextDead == deadCallEnabled && nextWorm == wormLookupEnabled)
            return;

        hook?.Dispose();
        hook = null;
        anyEnabled = nextAny;
        deadCallEnabled = nextDead;
        wormLookupEnabled = nextWorm;
        if (!anyEnabled && !deadCallEnabled && !wormLookupEnabled)
            return;

        try
        {
            bool any = anyEnabled;
            bool dead = deadCallEnabled;
            bool worm = wormLookupEnabled;
            hook = new ILHook(target!, il => Rewrite(il, any, dead, worm));
        }
        catch (Exception exception)
        {
            hook?.Dispose();
            hook = null;
            anyEnabled = deadCallEnabled = wormLookupEnabled = false;
            available = false;
            compatibility = $"enable-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"UpdateLifeRegen R10 failed to enable: {exception}");
        }
    }

    /// <summary>
    /// Enables only the two R10 components accepted by the eight-cell factorial
    /// benchmark. The dead SlimeGod lookup branch remains available for historical
    /// benchmark replay through SetComponents, but is not part of the shipped preset.
    /// </summary>
    public static void SetEnabled(bool value) => SetComponents(value, false, value);

    public static void Unload()
    {
        hook?.Dispose();
        hook = null;
        target = null;
        wormTypes = [];
        anyEnabled = deadCallEnabled = wormLookupEnabled = available = false;
        compatibility = "unloaded";
        owner = null;
    }

    private static void Rewrite(ILContext il, bool replaceAny, bool removeDeadCall, bool replaceWormChain)
    {
        if (replaceAny)
            ReplaceBuffAny(il);
        if (removeDeadCall)
            RemoveDeadSlimeGodContains(il);
        if (replaceWormChain)
            ReplaceWormContainsChain(il);
    }

    private static void ReplaceBuffAny(ILContext il)
    {
        MethodInfo helper = typeof(UpdateLifeRegenFastPatch).GetMethod(nameof(AnyDirect), BindingFlags.Static | BindingFlags.Public)!;
        MethodReference imported = il.Method.Module.ImportReference(helper);
        ILCursor cursor = new(il);
        int replaced = 0;
        while (cursor.TryGotoNext(MoveType.Before, IsBuffTypeEnumerableAny))
        {
            Instruction call = cursor.Next!;
            call.OpCode = OpCodes.Call;
            call.Operand = imported;
            cursor.Index++;
            replaced++;
        }
        if (replaced != 1)
            throw new InvalidOperationException($"Expected one npc.buffType Enumerable.Any call; found {replaced}.");
    }

    private static void RemoveDeadSlimeGodContains(ILContext il)
    {
        ILCursor cursor = new(il);
        int removed = 0;
        while (cursor.TryGotoNext(MoveType.Before,
                   instruction => IsTypeSetField(instruction, "SlimeGod"),
                   instruction => instruction.OpCode == OpCodes.Ldarg_1,
                   IsNpcTypeFieldLoad,
                   IsIntListContains,
                   instruction => instruction.OpCode == OpCodes.Pop))
        {
            for (int index = 0; index < 5; index++)
            {
                Instruction instruction = cursor.Next!;
                instruction.OpCode = OpCodes.Nop;
                instruction.Operand = null;
                cursor.Index++;
            }
            removed++;
        }
        if (removed != 1)
            throw new InvalidOperationException($"Expected one dead SlimeGod.Contains/pop sequence; found {removed}.");
    }

    private static void ReplaceWormContainsChain(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before, instruction => IsTypeSetField(instruction, WormSetNames[0])))
            throw new InvalidOperationException("DesertScourge start of the six-list worm chain was not found.");

        Instruction start = cursor.Next!;
        int startIndex = il.Body.Instructions.IndexOf(start);
        string[] observed = new string[6];
        for (int group = 0; group < 6; group++)
        {
            int offset = group * 5;
            Instruction field = il.Body.Instructions[startIndex + offset];
            if (field.Operand is not FieldReference fieldReference)
                throw new InvalidOperationException($"Worm chain group {group} does not start with a field load.");
            observed[group] = fieldReference.Name;
            if (observed[group] != WormSetNames[group] ||
                il.Body.Instructions[startIndex + offset + 1].OpCode != OpCodes.Ldarg_1 ||
                !IsNpcTypeFieldLoad(il.Body.Instructions[startIndex + offset + 2]) ||
                !IsIntListContains(il.Body.Instructions[startIndex + offset + 3]) ||
                (group < 5 && il.Body.Instructions[startIndex + offset + 4].OpCode.FlowControl != FlowControl.Cond_Branch) ||
                (group == 5 && il.Body.Instructions[startIndex + offset + 4].OpCode.FlowControl != FlowControl.Branch))
                throw new InvalidOperationException($"Worm chain group {group} no longer matches the audited sequence.");
        }

        Instruction trueConstant = il.Body.Instructions[startIndex + 30];
        Instruction storeResult = il.Body.Instructions[startIndex + 31];
        if (trueConstant.OpCode != OpCodes.Ldc_I4_1 || !IsStoreLocal(storeResult))
            throw new InvalidOperationException("Worm chain result tail no longer matches ldc.i4.1/stloc.");

        // Refuse if unrelated control flow enters the middle of the soon-unreachable
        // original chain. Its own short-circuit branches are the only allowed targets.
        for (int index = 4; index <= 30; index++)
        {
            Instruction candidate = il.Body.Instructions[startIndex + index];
            foreach (Instruction instruction in il.Body.Instructions)
            {
                if (instruction.Offset >= start.Offset && instruction.Offset <= trueConstant.Offset)
                    continue;
                if (ReferenceEquals(instruction.Operand, candidate))
                    throw new InvalidOperationException($"External branch targets worm-chain instruction {candidate.Offset:X4}.");
            }
        }

        MethodInfo helper = typeof(UpdateLifeRegenFastPatch).GetMethod(nameof(IsSegmentedWormType), BindingFlags.Static | BindingFlags.Public)!;
        FieldInfo npcType = typeof(NPC).GetField(nameof(NPC.type), BindingFlags.Instance | BindingFlags.Public)!;
        start.OpCode = OpCodes.Ldarg_1;
        start.Operand = null;
        Instruction second = il.Body.Instructions[startIndex + 1];
        second.OpCode = OpCodes.Ldfld;
        second.Operand = il.Method.Module.ImportReference(npcType);
        Instruction third = il.Body.Instructions[startIndex + 2];
        third.OpCode = OpCodes.Call;
        third.Operand = il.Method.Module.ImportReference(helper);
        Instruction fourth = il.Body.Instructions[startIndex + 3];
        fourth.OpCode = OpCodes.Br;
        fourth.Operand = storeResult;
    }

    private static bool IsBuffTypeEnumerableAny(Instruction instruction) =>
        instruction.OpCode == OpCodes.Call &&
        instruction.Operand is GenericInstanceMethod method &&
        method.Name == "Any" &&
        method.DeclaringType.FullName == "System.Linq.Enumerable" &&
        method.GenericArguments.Count == 1 &&
        method.GenericArguments[0].FullName == "System.Int32" &&
        method.Parameters.Count == 2;

    private static bool IsTypeSetField(Instruction instruction, string name) =>
        instruction.OpCode == OpCodes.Ldsfld &&
        instruction.Operand is FieldReference field &&
        field.DeclaringType.FullName == typeof(CalamityNPCTypeSets).FullName &&
        field.Name == name;

    private static bool IsNpcTypeFieldLoad(Instruction instruction) =>
        instruction.OpCode == OpCodes.Ldfld &&
        instruction.Operand is FieldReference field &&
        field.DeclaringType.FullName == typeof(NPC).FullName &&
        field.Name == nameof(NPC.type);

    private static bool IsIntListContains(Instruction instruction) =>
        instruction.OpCode == OpCodes.Callvirt &&
        instruction.Operand is MethodReference method &&
        method.Name == nameof(List<int>.Contains) &&
        method.DeclaringType.FullName == "System.Collections.Generic.List`1<System.Int32>";

    private static bool IsStoreLocal(Instruction instruction) =>
        instruction.OpCode == OpCodes.Stloc || instruction.OpCode == OpCodes.Stloc_S ||
        instruction.OpCode == OpCodes.Stloc_0 || instruction.OpCode == OpCodes.Stloc_1 ||
        instruction.OpCode == OpCodes.Stloc_2 || instruction.OpCode == OpCodes.Stloc_3;

    public static bool AnyDirect(int[] values, Func<int, bool> predicate)
    {
        for (int index = 0; index < values.Length; index++)
            if (predicate(values[index]))
                return true;
        return false;
    }

    public static bool IsSegmentedWormType(int npcType)
    {
        if ((uint)npcType < (uint)wormTypes.Length)
            return wormTypes[npcType];
        return OriginalWormContains(npcType);
    }

    private static void BuildAndVerifyWormLookup()
    {
        wormTypes = new bool[NPCLoader.NPCCount];
        foreach (List<int> list in GetWormLists())
            foreach (int type in list)
                if ((uint)type < (uint)wormTypes.Length)
                    wormTypes[type] = true;

        for (int type = 0; type < wormTypes.Length; type++)
            if (wormTypes[type] != OriginalWormContains(type))
                throw new InvalidOperationException($"Worm lookup differs from Calamity lists at NPC type {type}.");
    }

    private static List<int>[] GetWormLists() =>
    [
        CalamityNPCTypeSets.DesertScourge,
        CalamityNPCTypeSets.EaterOfWorlds,
        CalamityNPCTypeSets.Perforators,
        CalamityNPCTypeSets.AquaticScourge,
        CalamityNPCTypeSets.AstrumDeus,
        CalamityNPCTypeSets.StormWeaver,
    ];

    private static bool OriginalWormContains(int type) =>
        CalamityNPCTypeSets.DesertScourge.Contains(type) ||
        CalamityNPCTypeSets.EaterOfWorlds.Contains(type) ||
        CalamityNPCTypeSets.Perforators.Contains(type) ||
        CalamityNPCTypeSets.AquaticScourge.Contains(type) ||
        CalamityNPCTypeSets.AstrumDeus.Contains(type) ||
        CalamityNPCTypeSets.StormWeaver.Contains(type);

    private static int CountTrue(bool[] values)
    {
        int count = 0;
        foreach (bool value in values)
            if (value)
                count++;
        return count;
    }
}
