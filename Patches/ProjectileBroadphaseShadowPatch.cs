using System;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Measures an exact early AABB reject before CanHitNPCWithProj without changing
/// any branch or return value. Eligibility is deliberately conservative: a type
/// is included when ModProjectile does not override Colliding. GlobalProjectile
/// results are observed separately per actual call, because broad global hooks
/// such as Fargo Souls apply to every type but return null for almost all of them.
/// </summary>
public static class ProjectileBroadphaseShadowPatch
{
    private static Mod? owner;
    private static MethodInfo? damageMethod;
    private static MethodInfo? collidingMethod;
    private static MethodInfo? loaderCollidingMethod;
    private static ILHook? damageHook;
    private static ILHook? collidingHook;
    private static ILHook? loaderCollidingHook;
    private static bool[] eligibleTypes = [];
    private static bool enabled;
    private static long totalChecks;
    private static long eligibleChecks;
    private static long potentialRejects;
    private static long excludedCustomChecks;
    private static long verifiedRejects;
    private static long violations;
    private static long globalNull;
    private static long globalTrue;
    private static long globalFalse;
    private static bool[] globalCustomSeen = [];
    private static long[] potentialByType = [];
    private static long[] verifiedByType = [];
    private static long[] violationsByType = [];
    private static long[] globalTrueByType = [];
    private static long[] globalFalseByType = [];

    [ThreadStatic] private static Projectile? candidateProjectile;
    [ThreadStatic] private static bool candidateReject;
    [ThreadStatic] private static bool currentCollisionCandidate;
    [ThreadStatic] private static int loaderCollisionType;
    [ThreadStatic] private static int candidateType;
    [ThreadStatic] private static int currentCollisionType;
    [ThreadStatic] private static Rectangle candidateTarget;

    public static bool Enabled => enabled;
    public static long TotalChecks => totalChecks;
    public static long EligibleChecks => eligibleChecks;
    public static long PotentialRejects => potentialRejects;
    public static long ExcludedCustomChecks => excludedCustomChecks;
    public static long VerifiedRejects => verifiedRejects;
    public static long Violations => violations;
    public static long GlobalNull => globalNull;
    public static long GlobalTrue => globalTrue;
    public static long GlobalFalse => globalFalse;
    public static int GlobalCustomTypeCount { get; private set; }
    public static int EligibleTypeCount { get; private set; }
    public static string Mode => enabled ? "shadow-no-branch-change" : "off";
    public static string ViolationTypes => Summarize(violationsByType, potentialByType, "v", "p", requirePrimary: true);
    public static string GlobalCustomTypes => Summarize(globalTrueByType, globalFalseByType, "t", "f", requirePrimary: false);

    public static void Load(Mod mod)
    {
        owner = mod;
        damageMethod = typeof(Projectile).GetMethod(nameof(Projectile.Damage), BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMethodException(typeof(Projectile).FullName, nameof(Projectile.Damage));
        collidingMethod = typeof(Projectile).GetMethod(nameof(Projectile.Colliding), BindingFlags.Instance | BindingFlags.Public,
            null, [typeof(Rectangle), typeof(Rectangle)], null)
            ?? throw new MissingMethodException(typeof(Projectile).FullName, nameof(Projectile.Colliding));
        loaderCollidingMethod = typeof(ProjectileLoader).GetMethod(nameof(ProjectileLoader.Colliding), BindingFlags.Static | BindingFlags.Public,
            null, [typeof(Projectile), typeof(Rectangle), typeof(Rectangle)], null)
            ?? throw new MissingMethodException(typeof(ProjectileLoader).FullName, nameof(ProjectileLoader.Colliding));
    }

    public static void PostSetupContent()
    {
        // ProjectileLoader.FinishSetup builds GlobalTypeLookups after individual
        // mods receive PostSetupContent. Resolve the private per-type hook table
        // lazily when the player enables shadow mode inside a loaded world.
        owner?.Logger.Info("Projectile broadphase shadow target resolved; eligibility map is deferred until activation.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        owner = null;
        damageMethod = null;
        collidingMethod = null;
        loaderCollidingMethod = null;
        eligibleTypes = [];
        globalCustomSeen = [];
        potentialByType = [];
        verifiedByType = [];
        violationsByType = [];
        globalTrueByType = [];
        globalFalseByType = [];
        EligibleTypeCount = 0;
        GlobalCustomTypeCount = 0;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
        {
            if (eligibleTypes.Length != ProjectileLoader.ProjectileCount)
                BuildEligibilityMap();
            damageHook = new ILHook(damageMethod!, InstrumentDamage);
            collidingHook = new ILHook(collidingMethod!, InstrumentCollision);
            loaderCollidingHook = new ILHook(loaderCollidingMethod!, InstrumentLoaderCollision);
            enabled = true;
            owner?.Logger.Info("Projectile broadphase shadow enabled; gameplay branches remain original.");
        }
        else
        {
            loaderCollidingHook?.Dispose();
            collidingHook?.Dispose();
            damageHook?.Dispose();
            loaderCollidingHook = null;
            collidingHook = null;
            damageHook = null;
            enabled = false;
            candidateProjectile = null;
            candidateReject = false;
            currentCollisionCandidate = false;
            candidateType = -1;
            currentCollisionType = -1;
        }
        ResetCounters();
    }

    public static void ResetCounters()
    {
        totalChecks = 0;
        eligibleChecks = 0;
        potentialRejects = 0;
        excludedCustomChecks = 0;
        verifiedRejects = 0;
        violations = 0;
        globalNull = 0;
        globalTrue = 0;
        globalFalse = 0;
        GlobalCustomTypeCount = 0;
        if (globalCustomSeen.Length != 0)
            Array.Clear(globalCustomSeen);
        if (potentialByType.Length != 0) Array.Clear(potentialByType);
        if (verifiedByType.Length != 0) Array.Clear(verifiedByType);
        if (violationsByType.Length != 0) Array.Clear(violationsByType);
        if (globalTrueByType.Length != 0) Array.Clear(globalTrueByType);
        if (globalFalseByType.Length != 0) Array.Clear(globalFalseByType);
    }

    private static void BuildEligibilityMap()
    {
        int count = ProjectileLoader.ProjectileCount;
        bool[] map = new bool[count];

        int eligible = 0;
        for (int type = 0; type < count; type++)
        {
            ModProjectile? template = ProjectileLoader.GetProjectile(type);
            bool modOverride = template is not null &&
                template.GetType().GetMethod(nameof(ModProjectile.Colliding), BindingFlags.Instance | BindingFlags.Public)?.DeclaringType != typeof(ModProjectile);
            if (!modOverride)
            {
                map[type] = true;
                eligible++;
            }
        }

        eligibleTypes = map;
        globalCustomSeen = new bool[count];
        potentialByType = new long[count];
        verifiedByType = new long[count];
        violationsByType = new long[count];
        globalTrueByType = new long[count];
        globalFalseByType = new long[count];
        EligibleTypeCount = eligible;
    }

    private static void InstrumentDamage(ILContext il)
    {
        ILCursor cursor = new(il);
        int matches = 0;
        while (cursor.TryGotoNext(MoveType.Before, instruction =>
                   instruction.MatchCall(typeof(CombinedHooks), nameof(CombinedHooks.CanHitNPCWithProj))))
        {
            // Preserve the exact original nullable result while recording a
            // candidate only when hooks return null and collision fallback will
            // decide the hit. Explicit true/false hook results are never broadphase
            // candidates in the corrected production wrapper.
            cursor.Remove();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(ObserveCanHit);
            matches++;
        }

        if (matches != 1)
            throw new InvalidOperationException($"Projectile.Damage CanHitNPCWithProj anchor count changed: {matches}.");
    }

    private static void InstrumentCollision(ILContext il)
    {
        ILCursor entry = new(il);
        entry.Emit(OpCodes.Ldarg_0);
        entry.Emit(OpCodes.Ldarg_2);
        entry.EmitDelegate(BeginCollision);

        Instruction[] returns = System.Linq.Enumerable.ToArray(
            System.Linq.Enumerable.Where(il.Body.Instructions, instruction => instruction.OpCode == OpCodes.Ret));
        ILCursor cursor = new(il);
        foreach (Instruction ret in returns)
        {
            ret.OpCode = OpCodes.Nop;
            ret.Operand = null;
            cursor.Goto(ret, MoveType.After);
            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate(EndCollision);
            cursor.Emit(OpCodes.Ret);
        }
    }

    private static void InstrumentLoaderCollision(ILContext il)
    {
        ILCursor entry = new(il);
        entry.Emit(OpCodes.Ldarg_0);
        entry.EmitDelegate(BeginLoaderCollision);

        Instruction[] returns = System.Linq.Enumerable.ToArray(
            System.Linq.Enumerable.Where(il.Body.Instructions, instruction => instruction.OpCode == OpCodes.Ret));
        ILCursor cursor = new(il);
        foreach (Instruction ret in returns)
        {
            ret.OpCode = OpCodes.Nop;
            ret.Operand = null;
            cursor.Goto(ret, MoveType.After);
            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate(EndLoaderCollision);
            cursor.Emit(OpCodes.Ret);
        }
    }

    private static bool? ObserveCanHit(Projectile projectile, NPC npc, Rectangle projectileHitbox)
    {
        totalChecks++;
        bool? result = CombinedHooks.CanHitNPCWithProj(projectile, npc);
        candidateProjectile = projectile;
        candidateReject = false;
        candidateType = projectile.type;

        if (result.HasValue)
        {
            candidateProjectile = null;
            candidateType = -1;
            return result;
        }

        int type = projectile.type;
        if ((uint)type >= (uint)eligibleTypes.Length || !eligibleTypes[type])
        {
            excludedCustomChecks++;
            candidateProjectile = null;
            candidateType = -1;
            return null;
        }

        eligibleChecks++;
        Rectangle target = new(
            (int)(npc.position.X + npc.netOffset.X),
            (int)(npc.position.Y + npc.netOffset.Y),
            npc.width,
            npc.height);
        // Projectile.Damage expands this vanilla target before the collision
        // call. Mirror that exact exception so the shadow decision measures the
        // same rectangle used by the shipped fast path.
        if (npc.type == Terraria.ID.NPCID.SolarCrawltipedeTail)
            target.Inflate(8, 8);
        if (!projectileHitbox.Intersects(target))
        {
            candidateReject = true;
            candidateTarget = target;
            potentialRejects++;
            potentialByType[type]++;
        }
        return null;
    }

    private static void BeginCollision(Projectile projectile, Rectangle targetHitbox)
    {
        currentCollisionCandidate = candidateReject &&
            ReferenceEquals(candidateProjectile, projectile) &&
            candidateTarget.Equals(targetHitbox);
        currentCollisionType = currentCollisionCandidate ? candidateType : -1;
        candidateReject = false;
        candidateProjectile = null;
        candidateType = -1;
    }

    private static void EndCollision(bool result)
    {
        if (!currentCollisionCandidate)
            return;
        if (result)
        {
            violations++;
            if ((uint)currentCollisionType < (uint)violationsByType.Length)
                violationsByType[currentCollisionType]++;
        }
        else
        {
            verifiedRejects++;
            if ((uint)currentCollisionType < (uint)verifiedByType.Length)
                verifiedByType[currentCollisionType]++;
        }
        currentCollisionCandidate = false;
        currentCollisionType = -1;
    }

    private static void BeginLoaderCollision(Projectile projectile) => loaderCollisionType = projectile.type;

    private static void EndLoaderCollision(bool? result)
    {
        if (!result.HasValue)
        {
            globalNull++;
            return;
        }

        int type = loaderCollisionType;
        if (result.Value)
        {
            globalTrue++;
            if ((uint)type < (uint)globalTrueByType.Length) globalTrueByType[type]++;
        }
        else
        {
            globalFalse++;
            if ((uint)type < (uint)globalFalseByType.Length) globalFalseByType[type]++;
        }
        if ((uint)type < (uint)globalCustomSeen.Length && !globalCustomSeen[type])
        {
            globalCustomSeen[type] = true;
            GlobalCustomTypeCount++;
        }
    }

    private static string Summarize(long[] primary, long[] secondary, string primaryLabel, string secondaryLabel, bool requirePrimary)
    {
        if (primary.Length == 0)
            return "none";

        int[] top = new int[Math.Min(8, primary.Length)];
        Array.Fill(top, -1);
        for (int type = 0; type < primary.Length; type++)
        {
            long score = primary[type] + secondary[type];
            if (score == 0 || requirePrimary && primary[type] == 0)
                continue;
            for (int slot = 0; slot < top.Length; slot++)
            {
                int other = top[slot];
                if (other < 0 || score > primary[other] + secondary[other])
                {
                    for (int move = top.Length - 1; move > slot; move--)
                        top[move] = top[move - 1];
                    top[slot] = type;
                    break;
                }
            }
        }

        StringBuilder text = new();
        foreach (int type in top)
        {
            if (type < 0)
                break;
            if (text.Length != 0)
                text.Append('|');
            text.Append(ContentName(type)).Append(':').Append(primaryLabel).Append(primary[type])
                .Append('/').Append(secondaryLabel).Append(secondary[type]);
        }
        return text.Length == 0 ? "none" : text.ToString();
    }

    private static string ContentName(int type)
    {
        ModProjectile? template = ProjectileLoader.GetProjectile(type);
        return template is null ? $"Terraria/{type}" : $"{template.Mod.Name}/{template.Name}";
    }
}
