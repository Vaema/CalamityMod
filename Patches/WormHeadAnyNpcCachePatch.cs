using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Exact positive-result cache for the NPC.AnyNPCs(headType) call made by
/// every DoG and Thanatos body segment. A cached index is revalidated on every
/// read; invalid or negative results always rescan, so the answer is identical
/// to NPC.AnyNPCs at the point of the call.
/// </summary>
internal static class WormHeadAnyNpcCachePatch
{
    private static readonly MethodInfo AnyNpcsMethod = typeof(NPC).GetMethod(
        nameof(NPC.AnyNPCs), BindingFlags.Public | BindingFlags.Static,
        binder: null, types: [typeof(int)], modifiers: null)
        ?? throw new MissingMethodException(typeof(NPC).FullName, nameof(NPC.AnyNPCs));

    private static Mod? owner;
    private static MethodInfo[] aiMethods = [];
    private static ILHook[] hooks = [];
    private static int dogHeadType = -1;
    private static int thanatosHeadType = -1;
    private static int dogCachedIndex = -1;
    private static int thanatosCachedIndex = -1;
    private static ulong dogCachedTick = ulong.MaxValue;
    private static ulong thanatosCachedTick = ulong.MaxValue;
    private static bool enabled;
    private static bool available;
    private static string compatibility = "content-not-resolved";
    private static long calls;
    private static long cacheHits;
    private static long scans;
    private static long scannedSlots;
    private static long invalidations;

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static string Mode => enabled ? "exact-positive-index-per-update" : "calamity-anynpcs-scan";
    public static long Calls => calls;
    public static long CacheHits => cacheHits;
    public static long Scans => scans;
    public static long ScannedSlots => scannedSlots;
    public static long Invalidations => invalidations;

    public static void Load(Mod mod)
    {
        owner = mod;
        aiMethods = [];
        hooks = [];
        dogHeadType = -1;
        thanatosHeadType = -1;
        enabled = false;
        available = false;
        compatibility = "content-not-resolved";
        ResetCounters();
        ResetCache();
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
            ModNPC dogBody = calamity.Find<ModNPC>("DevourerofGodsBody");
            ModNPC thanatosBody1 = calamity.Find<ModNPC>("ThanatosBody1");
            ModNPC thanatosBody2 = calamity.Find<ModNPC>("ThanatosBody2");
            dogHeadType = calamity.Find<ModNPC>("DevourerofGodsHead").Type;
            thanatosHeadType = calamity.Find<ModNPC>("ThanatosHead").Type;
            aiMethods =
            [
                ResolveAi(dogBody.GetType()),
                ResolveAi(thanatosBody1.GetType()),
                ResolveAi(thanatosBody2.GetType()),
            ];

            // Validate all three audited anchors during loading, then restore
            // the original IL until an explicit preset/benchmark enables us.
            foreach (MethodInfo method in aiMethods)
            {
                using ILHook validation = new(method, ReplaceSingleAnyNpcsCall);
            }

            available = true;
            compatibility = "audited-calamity-2.2.2-dog+thanatos-body-anynpcs";
            owner?.Logger.Info("Worm head AnyNPCs cache resolved and IL anchors validated; state is OFF.");
        }
        catch (Exception exception)
        {
            available = false;
            compatibility = $"anchor-failed:{exception.GetType().Name}:{exception.Message}";
            owner?.Logger.Warn($"Worm head AnyNPCs cache unavailable: {exception}");
        }
    }

    public static void Unload()
    {
        SetEnabled(false);
        aiMethods = [];
        dogHeadType = -1;
        thanatosHeadType = -1;
        available = false;
        compatibility = "unloaded";
        owner = null;
        ResetCounters();
        ResetCache();
    }

    public static void SetEnabled(bool value)
    {
        bool requested = value && available;
        if (requested == enabled)
        {
            ResetCounters();
            ResetCache();
            return;
        }

        DisposeHooks();
        enabled = false;
        if (requested)
        {
            try
            {
                hooks = new ILHook[aiMethods.Length];
                for (int index = 0; index < aiMethods.Length; index++)
                    hooks[index] = new ILHook(aiMethods[index], ReplaceSingleAnyNpcsCall);
                enabled = true;
            }
            catch (Exception exception)
            {
                DisposeHooks();
                available = false;
                compatibility = $"enable-failed:{exception.GetType().Name}:{exception.Message}";
                owner?.Logger.Warn($"Worm head AnyNPCs cache failed to enable: {exception}");
            }
        }

        ResetCounters();
        ResetCache();
    }

    public static void ResetCounters()
    {
        calls = 0;
        cacheHits = 0;
        scans = 0;
        scannedSlots = 0;
        invalidations = 0;
    }

    private static MethodInfo ResolveAi(Type type) => type.GetMethod(
        nameof(ModNPC.AI), BindingFlags.Instance | BindingFlags.Public,
        binder: null, types: Type.EmptyTypes, modifiers: null)
        ?? throw new MissingMethodException(type.FullName, nameof(ModNPC.AI));

    private static void ReplaceSingleAnyNpcsCall(ILContext il)
    {
        ILCursor cursor = new(il);
        int replacements = 0;
        while (cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchCall(AnyNpcsMethod)))
        {
            cursor.Remove();
            cursor.EmitDelegate(AnyNpcsExactCached);
            replacements++;
        }

        if (replacements != 1)
            throw new InvalidOperationException($"Expected one NPC.AnyNPCs call in {il.Method.FullName}, found {replacements}.");
    }

    private static bool AnyNpcsExactCached(int type)
    {
        calls++;
        if (type != dogHeadType && type != thanatosHeadType)
            return NPC.AnyNPCs(type);

        ref int cachedIndex = ref (type == dogHeadType ? ref dogCachedIndex : ref thanatosCachedIndex);
        ref ulong cachedTick = ref (type == dogHeadType ? ref dogCachedTick : ref thanatosCachedTick);

        ulong tick = Main.GameUpdateCount;
        if (cachedTick == tick && (uint)cachedIndex < Main.maxNPCs)
        {
            NPC cached = Main.npc[cachedIndex];
            if (cached.active && cached.type == type)
            {
                cacheHits++;
                return true;
            }
            invalidations++;
        }

        scans++;
        for (int index = 0; index < Main.maxNPCs; index++)
        {
            scannedSlots++;
            NPC npc = Main.npc[index];
            if (!npc.active || npc.type != type)
                continue;
            cachedIndex = index;
            cachedTick = tick;
            return true;
        }

        // Deliberately do not reuse a negative result. A head spawned later in
        // this same update must be observed exactly as NPC.AnyNPCs would.
        cachedIndex = -1;
        cachedTick = ulong.MaxValue;
        return false;
    }

    private static void DisposeHooks()
    {
        foreach (ILHook? hook in hooks)
            hook?.Dispose();
        hooks = [];
    }

    private static void ResetCache()
    {
        dogCachedIndex = -1;
        thanatosCachedIndex = -1;
        dogCachedTick = ulong.MaxValue;
        thanatosCachedTick = ulong.MaxValue;
    }
}
