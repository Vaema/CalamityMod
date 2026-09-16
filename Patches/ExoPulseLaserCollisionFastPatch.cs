using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Exact broad reject for Calamity 2.2.2 ThanatosLaser and ArtemisLaser.
/// Both audited Colliding overrides use a circle centered on the projectile
/// with radius Projectile.Size.Length() * 0.5f. A target rectangle disjoint
/// from that circle's bounding square cannot collide; every square candidate
/// continues through CombinedHooks and Calamity's original circle test.
/// </summary>
internal static class ExoPulseLaserCollisionFastPatch
{
    private static int thanatosLaserType = -1;
    private static int artemisLaserType = -1;
    private static bool enabled;
    private static bool preHookCandidateEnabled;
    private static bool preHookAvailable;
    private static bool[] auditedPreHookTargets = [];
    private static string preHookCompatibility = "not-audited";
    private static bool expandedPreHookCandidateEnabled;
    private static bool expandedPreHookAvailable;
    private static bool[] expandedPreHookTargets = [];
    private static bool[] expandedTargetsRequireNullCalamityAiOverride = [];
    private static string expandedPreHookCompatibility = "not-audited";
    private static GlobalNPC? calamityAiOverrideGlobal;
    private static Func<GlobalNPC, bool>? calamityAiOverrideIsNull;
    private static bool available;
    private static string compatibility = "content-not-resolved";
    private static long checks;
    private static long rejected;
    private static long passed;
    private static long thanatosChecks;
    private static long thanatosRejected;
    private static long artemisChecks;
    private static long artemisRejected;

    public static bool Enabled => enabled;
    public static bool PreHookCandidateEnabled => preHookCandidateEnabled;
    public static bool PreHookAvailable => preHookAvailable;
    public static string PreHookCompatibility => preHookCompatibility;
    public static bool ExpandedPreHookCandidateEnabled => expandedPreHookCandidateEnabled;
    public static bool ExpandedPreHookAvailable => expandedPreHookAvailable;
    public static string ExpandedPreHookCompatibility => expandedPreHookCompatibility;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static string Mode => !enabled ? "calamity-original" : expandedPreHookCandidateEnabled
        ? "exact-circle-bounds-before-canhit-hooks-instance-guard-audited"
        : preHookCandidateEnabled
        ? "exact-circle-bounds-before-canhit-hooks-audited"
        : "exact-circle-bounds-before-original";
    public static long Checks => checks;
    public static long Rejected => rejected;
    public static long Passed => passed;
    public static long ThanatosChecks => thanatosChecks;
    public static long ThanatosRejected => thanatosRejected;
    public static long ArtemisChecks => artemisChecks;
    public static long ArtemisRejected => artemisRejected;

    public static void Load(Mod mod)
    {
        enabled = false;
        preHookCandidateEnabled = false;
        preHookAvailable = false;
        auditedPreHookTargets = [];
        preHookCompatibility = "not-audited";
        expandedPreHookCandidateEnabled = false;
        expandedPreHookAvailable = false;
        expandedPreHookTargets = [];
        expandedTargetsRequireNullCalamityAiOverride = [];
        expandedPreHookCompatibility = "not-audited";
        calamityAiOverrideGlobal = null;
        calamityAiOverrideIsNull = null;
        available = false;
        compatibility = "content-not-resolved";
        thanatosLaserType = -1;
        artemisLaserType = -1;
        ResetCounters();
    }

    public static void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out Mod? calamity))
        {
            compatibility = "missing-calamity";
            return;
        }
        if (calamity.Version.ToString() != "2.2.2")
        {
            compatibility = $"unaudited-calamity:{calamity.Version}";
            return;
        }
        if (!calamity.TryFind("ThanatosLaser", out ModProjectile thanatos) ||
            !calamity.TryFind("ArtemisLaser", out ModProjectile artemis))
        {
            compatibility = "missing-exo-pulse-laser-types";
            return;
        }

        thanatosLaserType = thanatos.Type;
        artemisLaserType = artemis.Type;
        available = true;
        compatibility = "audited-calamity-2.2.2-circle-size-length-half";
    }

    public static void Unload()
    {
        enabled = false;
        preHookCandidateEnabled = false;
        preHookAvailable = false;
        auditedPreHookTargets = [];
        preHookCompatibility = "unloaded";
        expandedPreHookCandidateEnabled = false;
        expandedPreHookAvailable = false;
        expandedPreHookTargets = [];
        expandedTargetsRequireNullCalamityAiOverride = [];
        expandedPreHookCompatibility = "unloaded";
        calamityAiOverrideGlobal = null;
        calamityAiOverrideIsNull = null;
        available = false;
        compatibility = "unloaded";
        thanatosLaserType = -1;
        artemisLaserType = -1;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        enabled = value && available;
        if (!enabled)
        {
            preHookCandidateEnabled = false;
            expandedPreHookCandidateEnabled = false;
        }
        ResetCounters();
    }

    /// <summary>
    /// Applies the accepted public state. The original exact-bound reject is
    /// always the fallback. Early rejection is enabled only for audited target
    /// providers; Calamity's broad AI-override provider additionally requires
    /// a null AIOverride on the concrete NPC instance.
    /// </summary>
    public static void SetAcceptedEnabled(bool value)
    {
        SetEnabled(value);
        SetPreHookCandidateEnabled(value);
        SetExpandedPreHookCandidateEnabled(value);
    }

    public static void SetPreHookCandidateEnabled(bool value)
    {
        if (value && enabled && available)
            AuditPreHookCompatibility();
        preHookCandidateEnabled = value && enabled && available && preHookAvailable;
        if (!preHookCandidateEnabled)
            expandedPreHookCandidateEnabled = false;
        ResetCounters();
    }

    /// <summary>
    /// Experimental second tier. It retains the accepted Exo target list and
    /// additionally admits NPC types whose ModNPC predicate is inherited. A
    /// target with only CalamityVanillaAIOverrideNPC is admitted conditionally:
    /// the concrete per-NPC AIOverride field must be null at the call site.
    /// Any other custom predicate is excluded rather than guessed about.
    /// </summary>
    public static void SetExpandedPreHookCandidateEnabled(bool value)
    {
        if (value && enabled && preHookCandidateEnabled && preHookAvailable)
            AuditExpandedPreHookCompatibility();
        expandedPreHookCandidateEnabled = value && enabled && preHookCandidateEnabled &&
                                          preHookAvailable && expandedPreHookAvailable;
        ResetCounters();
    }

    /// <summary>
    /// Re-runs the expanded target audit without enabling the experimental path.
    /// This is diagnostics-only and does not alter the accepted gameplay state.
    /// </summary>
    public static void AuditExpandedPreHookForDiagnostics()
    {
        bool restoreEnabled = expandedPreHookCandidateEnabled;
        if (!enabled || !available || !preHookCandidateEnabled || !preHookAvailable)
        {
            expandedPreHookCandidateEnabled = false;
            expandedPreHookAvailable = false;
            expandedPreHookTargets = [];
            expandedTargetsRequireNullCalamityAiOverride = [];
            expandedPreHookCompatibility = "accepted-prehook-unavailable";
            return;
        }

        AuditExpandedPreHookCompatibility();
        expandedPreHookCandidateEnabled = restoreEnabled && expandedPreHookAvailable;
    }

    public static void ResetCounters()
    {
        checks = 0;
        rejected = 0;
        passed = 0;
        thanatosChecks = 0;
        thanatosRejected = 0;
        artemisChecks = 0;
        artemisRejected = 0;
    }

    public static bool TryHandle(Projectile projectile, NPC npc, out bool reject)
    {
        reject = false;
        if (!enabled)
            return false;

        bool thanatos = projectile.type == thanatosLaserType;
        bool artemis = projectile.type == artemisLaserType;
        if (!thanatos && !artemis)
            return false;

        // In candidate mode the exact geometry was already evaluated before
        // CombinedHooks. Square candidates continue to the original Calamity
        // collision method without evaluating the same bound a second time.
        if (preHookCandidateEnabled)
            return true;

        return EvaluateExactBound(projectile, npc, thanatos, out reject);
    }

    public static bool TryRejectBeforeHooks(Projectile projectile, NPC npc)
    {
        if (!enabled || !preHookCandidateEnabled)
            return false;
        bool thanatos = projectile.type == thanatosLaserType;
        bool artemis = projectile.type == artemisLaserType;
        if (!thanatos && !artemis)
            return false;
        bool[] targets = expandedPreHookCandidateEnabled ? expandedPreHookTargets : auditedPreHookTargets;
        if ((uint)npc.type >= (uint)targets.Length || !targets[npc.type])
            return false;
        EvaluateExactBound(projectile, npc, thanatos, out bool reject);
        if (reject && expandedPreHookCandidateEnabled &&
            expandedTargetsRequireNullCalamityAiOverride[npc.type] &&
            !HasNullCalamityAiOverride(npc))
            return false;
        return reject;
    }

    private static void AuditPreHookCompatibility()
    {
        preHookAvailable = false;
        auditedPreHookTargets = new bool[NPCLoader.NPCCount];
        try
        {
            HashSet<string> allowedProjectileGlobals = new(StringComparer.Ordinal)
            {
                "CalamityMod.Projectiles.CalamityGlobalProjectile",
            };
            HashSet<string> allowedNpcGlobals = new(StringComparer.Ordinal)
            {
                "CalamityMod.NPCs.CalamityVanillaAIOverrideNPC",
            };

            List<string> unknown = [];
            AuditProviderTypes(ReadGlobalProviders(typeof(ProjectileLoader), "HookCanHitNPC", thanatosLaserType), allowedProjectileGlobals, unknown);
            AuditProviderTypes(ReadGlobalProviders(typeof(ProjectileLoader), "HookCanHitNPC", artemisLaserType), allowedProjectileGlobals, unknown);
            AuditProviderTypes(ReadHookInstances(typeof(PlayerLoader), "HookCanHitNPC"), [], unknown);
            AuditProviderTypes(ReadHookInstances(typeof(PlayerLoader), "HookCanHitNPCWithProj"), [], unknown);
            if (unknown.Count != 0)
            {
                preHookCompatibility = $"unknown-shared-canhit-provider:{string.Join("+", unknown.Distinct())}";
                return;
            }

            int safeTargets = 0;
            List<string> excludedTargetProviders = [];
            for (int type = 0; type < auditedPreHookTargets.Length; type++)
            {
                ModNPC? template = NPCLoader.GetNPC(type);
                if (!IsAuditedExoTarget(template))
                    continue;

                List<string> targetUnknown = [];
                AuditProviderTypes(ReadGlobalProviders(typeof(NPCLoader), "HookCanBeHitByProjectile", type), allowedNpcGlobals, targetUnknown);
                if (targetUnknown.Count != 0)
                {
                    excludedTargetProviders.AddRange(targetUnknown);
                    continue;
                }

                auditedPreHookTargets[type] = true;
                safeTargets++;
            }

            if (safeTargets == 0)
            {
                preHookCompatibility = excludedTargetProviders.Count == 0
                    ? "no-audited-exomech-targets"
                    : $"all-targets-excluded:{string.Join("+", excludedTargetProviders.Distinct())}";
                return;
            }

            preHookAvailable = true;
            preHookCompatibility = excludedTargetProviders.Count == 0
                ? $"audited-exomech-targets:{safeTargets};shared-providers-clean"
                : $"audited-exomech-targets:{safeTargets};unknown-target-providers-excluded:{string.Join("+", excludedTargetProviders.Distinct())}";
        }
        catch (Exception exception)
        {
            preHookAvailable = false;
            preHookCompatibility = $"provider-audit-failed:{exception.GetType().Name}";
        }
    }

    private static void AuditExpandedPreHookCompatibility()
    {
        expandedPreHookAvailable = false;
        expandedPreHookTargets = new bool[NPCLoader.NPCCount];
        expandedTargetsRequireNullCalamityAiOverride = new bool[NPCLoader.NPCCount];
        try
        {
            if (!preHookAvailable || auditedPreHookTargets.Length != expandedPreHookTargets.Length)
            {
                expandedPreHookCompatibility = "accepted-target-audit-unavailable";
                return;
            }

            int safeTargets = 0;
            int inheritedTargets = 0;
            int customPredicateExclusions = 0;
            int globalProviderExclusions = 0;
            int calamityInstanceGuardTargets = 0;
            HashSet<string> excludedProviders = new(StringComparer.Ordinal);

            for (int type = 0; type < expandedPreHookTargets.Length; type++)
            {
                if (auditedPreHookTargets[type])
                {
                    expandedPreHookTargets[type] = true;
                    safeTargets++;
                    continue;
                }

                ModNPC? template = NPCLoader.GetNPC(type);
                if (HasCustomTargetPredicate(template))
                {
                    customPredicateExclusions++;
                    continue;
                }

                object[] providers = ReadGlobalProviders(typeof(NPCLoader), "HookCanBeHitByProjectile", type).ToArray();
                if (providers.Length != 0)
                {
                    bool onlyCalamityAiOverride = providers.All(provider =>
                        (provider.GetType().FullName ?? provider.GetType().Name) ==
                        "CalamityMod.NPCs.CalamityVanillaAIOverrideNPC");
                    if (!onlyCalamityAiOverride)
                    {
                        globalProviderExclusions++;
                        foreach (object provider in providers)
                            excludedProviders.Add(provider.GetType().FullName ?? provider.GetType().Name);
                        continue;
                    }

                    if (!BindCalamityAiOverrideReader())
                    {
                        expandedPreHookCompatibility = "calamity-ai-override-reader-unavailable";
                        return;
                    }
                    expandedTargetsRequireNullCalamityAiOverride[type] = true;
                    calamityInstanceGuardTargets++;
                }

                expandedPreHookTargets[type] = true;
                safeTargets++;
                inheritedTargets++;
            }

            if (inheritedTargets == 0)
            {
                expandedPreHookCompatibility = "no-additional-passive-targets";
                return;
            }

            expandedPreHookAvailable = true;
            string providerSummary = excludedProviders.Count == 0
                ? "none"
                : string.Join("+", excludedProviders.OrderBy(name => name, StringComparer.Ordinal));
            expandedPreHookCompatibility =
                $"audited-passive-targets:{safeTargets};additional:{inheritedTargets};" +
                $"custom-predicate-exclusions:{customPredicateExclusions};" +
                $"calamity-instance-guard-targets:{calamityInstanceGuardTargets};" +
                $"global-provider-exclusions:{globalProviderExclusions}:{providerSummary}";
        }
        catch (Exception exception)
        {
            expandedPreHookAvailable = false;
            expandedPreHookCompatibility = $"expanded-provider-audit-failed:{exception.GetType().Name}";
        }
    }

    private static bool BindCalamityAiOverrideReader()
    {
        if (calamityAiOverrideGlobal is not null && calamityAiOverrideIsNull is not null)
            return true;
        if (!ModLoader.TryGetMod("CalamityMod", out Mod? calamity) ||
            !calamity.TryFind("CalamityVanillaAIOverrideNPC", out GlobalNPC global))
            return false;

        FieldInfo? field = global.GetType().GetField("AIOverride", BindingFlags.Instance | BindingFlags.Public);
        if (field is null || field.FieldType.IsValueType)
            return false;

        ParameterExpression instance = Expression.Parameter(typeof(GlobalNPC), "instance");
        UnaryExpression concrete = Expression.Convert(instance, global.GetType());
        MemberExpression value = Expression.Field(concrete, field);
        calamityAiOverrideIsNull = Expression.Lambda<Func<GlobalNPC, bool>>(
            Expression.Equal(value, Expression.Constant(null, field.FieldType)), instance).Compile();
        calamityAiOverrideGlobal = global;
        return true;
    }

    private static bool HasNullCalamityAiOverride(NPC npc)
    {
        try
        {
            return calamityAiOverrideGlobal is not null && calamityAiOverrideIsNull is not null &&
                   npc.TryGetGlobalNPC(calamityAiOverrideGlobal, out GlobalNPC instance) &&
                   calamityAiOverrideIsNull(instance);
        }
        catch (Exception exception)
        {
            expandedPreHookCandidateEnabled = false;
            expandedPreHookAvailable = false;
            expandedPreHookCompatibility = $"calamity-ai-override-runtime-failed:{exception.GetType().Name}";
            return false;
        }
    }

    private static bool IsAuditedExoTarget(ModNPC? template)
    {
        if (template is null)
            return false;
        string name = template.GetType().FullName ?? string.Empty;
        if (!name.StartsWith("CalamityMod.NPCs.ExoMechs.", StringComparison.Ordinal))
            return false;

        MethodInfo? method = template.GetType().GetMethod(nameof(ModNPC.CanBeHitByProjectile),
            BindingFlags.Instance | BindingFlags.Public, null, [typeof(Projectile)], null);
        if (method is null || method.DeclaringType == typeof(ModNPC))
            return true;
        return false;
    }

    private static bool HasCustomTargetPredicate(ModNPC? template)
    {
        if (template is null)
            return false;
        MethodInfo? method = template.GetType().GetMethod(nameof(ModNPC.CanBeHitByProjectile),
            BindingFlags.Instance | BindingFlags.Public, null, [typeof(Projectile)], null);
        return method is not null && method.DeclaringType != typeof(ModNPC);
    }

    private static IEnumerable<object> ReadGlobalProviders(Type loaderType, string fieldName, int contentType)
    {
        FieldInfo field = loaderType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(loaderType.FullName, fieldName);
        object hook = field.GetValue(null) ?? throw new InvalidOperationException($"{loaderType.Name}.{fieldName} is null.");
        FieldInfo byTypeField = hook.GetType().GetField("hookGlobalsByType", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(hook.GetType().FullName, "hookGlobalsByType");
        Array byType = byTypeField.GetValue(hook) as Array
            ?? throw new InvalidOperationException($"{fieldName}.hookGlobalsByType is unavailable.");
        return contentType >= 0 && contentType < byType.Length && byType.GetValue(contentType) is Array providers
            ? providers.Cast<object>()
            : [];
    }

    private static IEnumerable<object> ReadHookInstances(Type loaderType, string fieldName)
    {
        FieldInfo field = loaderType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(loaderType.FullName, fieldName);
        object hook = field.GetValue(null) ?? throw new InvalidOperationException($"{loaderType.Name}.{fieldName} is null.");
        FieldInfo instancesField = hook.GetType().GetField("defaultInstances", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(hook.GetType().FullName, "defaultInstances");
        return instancesField.GetValue(hook) is Array instances ? instances.Cast<object>() : [];
    }

    private static void AuditProviderTypes(IEnumerable<object> providers, HashSet<string> allowed, List<string> unknown)
    {
        foreach (object provider in providers)
        {
            string name = provider.GetType().FullName ?? provider.GetType().Name;
            if (!allowed.Contains(name))
                unknown.Add(name);
        }
    }

    private static bool EvaluateExactBound(Projectile projectile, NPC npc, bool thanatos, out bool reject)
    {
        reject = false;

        checks++;
        if (thanatos)
            thanatosChecks++;
        else
            artemisChecks++;

        float radius = projectile.Size.Length() * 0.5f;
        Rectangle target = new(
            (int)(npc.position.X + npc.netOffset.X),
            (int)(npc.position.Y + npc.netOffset.Y),
            npc.width,
            npc.height);
        if (npc.type == NPCID.SolarCrawltipedeTail)
            target.Inflate(8, 8);

        Vector2 center = projectile.Center;
        reject = radius <= 0f ||
                 center.X + radius < target.Left ||
                 center.X - radius > target.Right ||
                 center.Y + radius < target.Top ||
                 center.Y - radius > target.Bottom;
        if (reject)
        {
            rejected++;
            if (thanatos)
                thanatosRejected++;
            else
                artemisRejected++;
        }
        else
            passed++;
        return true;
    }
}
