using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Calamity custom particles repeatedly call ModContent.Request during drawing.
/// The asset repository already owns the texture; these calls still normalize
/// paths and perform lookups. Replace only Request&lt;Texture2D&gt; calls declared in
/// Particle.CustomDraw overrides and cache by the exact path/request-mode pair.
/// Registration, loading and all non-draw request sites remain original.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class ParticleTextureRequestCachePatch
{
    private readonly record struct CacheKey(string Path, AssetRequestMode Mode);

    private static readonly Dictionary<CacheKey, Asset<Texture2D>> Cache = new();
    private static readonly object CacheLock = new();
    private static readonly List<ILHook> Hooks = new();
    private static bool enabled;
    private static long hits;
    private static long misses;
    private static int requestSites;

    public static bool Enabled => enabled;
    public static long Hits => Interlocked.Read(ref hits);
    public static long Misses => Interlocked.Read(ref misses);
    public static int HookedMethods => Hooks.Count;
    public static int RequestSites => requestSites;
    public static string Mode => enabled ? "customdraw-path-mode-cache" : "calamity-request";

    public static void Load(Mod owner)
    {
        MethodInfo[] targets = typeof(Particle).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Particle).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(method => method.Name == nameof(Particle.CustomDraw) && method.ReturnType == typeof(void))
            .OrderBy(method => method.DeclaringType!.FullName, StringComparer.Ordinal)
            .ThenBy(method => method.GetParameters().Length)
            .ToArray();

        int sites = 0;
        foreach (MethodInfo method in targets)
        {
            int localSites = 0;
            ILHook hook = new(method, il => localSites = ReplaceTextureRequests(il));
            if (localSites == 0)
            {
                hook.Dispose();
                continue;
            }
            Hooks.Add(hook);
            sites += localSites;
        }

        requestSites = sites;
        enabled = false;
        ResetCounters();
        owner.Logger.Info($"Particle texture request cache installed for {Hooks.Count} CustomDraw methods / {requestSites} request sites; state is OFF.");
    }

    public static void Unload()
    {
        enabled = false;
        for (int i = Hooks.Count - 1; i >= 0; i--)
            Hooks[i].Dispose();
        Hooks.Clear();
        requestSites = 0;
        lock (CacheLock)
            Cache.Clear();
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        enabled = value;
        ResetCounters();
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref hits, 0);
        Interlocked.Exchange(ref misses, 0);
    }

    private static int ReplaceTextureRequests(ILContext il)
    {
        ILCursor cursor = new(il);
        int replaced = 0;
        while (cursor.TryGotoNext(MoveType.Before, IsTextureRequest))
        {
            cursor.Remove();
            cursor.EmitDelegate(RequestTexture);
            replaced++;
        }
        return replaced;
    }

    private static bool IsTextureRequest(Mono.Cecil.Cil.Instruction instruction)
    {
        if (instruction.Operand is not GenericInstanceMethod method || method.Name != "Request" || method.GenericArguments.Count != 1)
            return false;
        return method.DeclaringType.FullName == typeof(ModContent).FullName &&
               method.GenericArguments[0].FullName == typeof(Texture2D).FullName &&
               method.Parameters.Count == 2;
    }

    private static Asset<Texture2D> RequestTexture(string path, AssetRequestMode mode)
    {
        if (!enabled)
            return ModContent.Request<Texture2D>(path, mode);

        CacheKey key = new(path, mode);
        lock (CacheLock)
        {
            if (Cache.TryGetValue(key, out Asset<Texture2D>? asset))
            {
                CalamityPatchDiagnostics.Increment(ref hits);
                return asset;
            }
            asset = ModContent.Request<Texture2D>(path, mode);
            Cache.Add(key, asset);
            CalamityPatchDiagnostics.Increment(ref misses);
            return asset;
        }
    }
}
