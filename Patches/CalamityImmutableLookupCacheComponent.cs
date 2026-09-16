using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CalamityMod.NPCs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class CalamityImmutableLookupCacheComponent
{
    private static readonly Dictionary<string, string> paths = new(StringComparer.Ordinal);
    private static readonly List<ILHook> hooks = [];
    private static Mod? owner;
    private static MethodInfo? miracleGetter, musicMethod, concatMethod;
    private static List<int>? miracleExcluded;
    private static bool enabled;
    private static long miracleHits, musicHits, musicMisses;
    public static string Compatibility { get; private set; } = "not-resolved";
    public static long MiracleHits => Interlocked.Read(ref miracleHits);
    public static long MusicHits => Interlocked.Read(ref musicHits);
    public static long MusicMisses => Interlocked.Read(ref musicMisses);

    public static void Load(Mod mod)
    {
        owner = mod;
        miracleGetter = typeof(CalamityDrawParameterNPC).GetProperty(nameof(CalamityDrawParameterNPC.MiracleBlightExcludedNPCs), BindingFlags.Static | BindingFlags.Public)?.GetMethod
            ?? throw new MissingMethodException(typeof(CalamityDrawParameterNPC).FullName, "get_MiracleBlightExcludedNPCs");
        musicMethod = typeof(CalamityMod).GetMethod(nameof(CalamityMod.GetMusicFromMusicMod), BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMethodException(typeof(CalamityMod).FullName, nameof(CalamityMod.GetMusicFromMusicMod));
        concatMethod = typeof(string).GetMethod(nameof(string.Concat), BindingFlags.Static | BindingFlags.Public, null, [typeof(string), typeof(string)], null)
            ?? throw new MissingMethodException(typeof(string).FullName, "Concat(string,string)");
        Compatibility = "audited-calamity-2.2.2";
        ResetCounters();
    }
    public static void PostSetupContent() => miracleExcluded = (List<int>?)miracleGetter!.Invoke(null, null)
        ?? throw new InvalidOperationException("MiracleBlightExcludedNPCs returned null.");
    public static void Unload()
    {
        SetEnabled(false); owner = null; miracleGetter = musicMethod = concatMethod = null; miracleExcluded = null; Compatibility = "unloaded";
    }
    public static bool SetEnabled(bool value)
    {
        if (!value)
        {
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear(); enabled = false; paths.Clear(); ResetCounters(); return true;
        }
        if (enabled) return true;
        List<ILHook> candidates = [];
        try
        {
            candidates.Add(new ILHook(miracleGetter!, InjectMiracle));
            candidates.Add(new ILHook(musicMethod!, ReplaceConcat));
            hooks.AddRange(candidates); enabled = true; ResetCounters(); return true;
        }
        catch (Exception e)
        {
            for (int i = candidates.Count - 1; i >= 0; i--) candidates[i].Dispose();
            hooks.Clear(); enabled = false; Compatibility = $"calamity-il-install-failed:{e.GetType().Name}:{e.Message}";
            owner?.Logger.Error($"Calamity immutable lookup cache disabled: {Compatibility}"); return false;
        }
    }
    public static void ResetCounters()
    {
        Interlocked.Exchange(ref miracleHits, 0); Interlocked.Exchange(ref musicHits, 0); Interlocked.Exchange(ref musicMisses, 0);
    }
    private static void InjectMiracle(ILContext il)
    {
        ILCursor c = new(il); ILLabel original = c.DefineLabel(); c.EmitDelegate<Func<List<int>?>>(GetMiracle); c.Emit(OpCodes.Dup);
        c.Emit(OpCodes.Brfalse, original); c.Emit(OpCodes.Ret); c.MarkLabel(original); c.Emit(OpCodes.Pop);
    }
    private static void ReplaceConcat(ILContext il)
    {
        ILCursor c = new(il); int n = 0;
        while (c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(concatMethod!))) { c.Remove(); c.EmitDelegate(GetPath); n++; }
        if (n != 1) throw new InvalidOperationException($"Expected one music path Concat call, found {n}.");
    }
    private static List<int>? GetMiracle()
    {
        if (!enabled) return null; CalamityPatchDiagnostics.Increment(ref miracleHits); return miracleExcluded;
    }
    private static string GetPath(string prefix, string song)
    {
        if (paths.TryGetValue(song, out string? path)) { CalamityPatchDiagnostics.Increment(ref musicHits); return path; }
        path = string.Concat(prefix, song); paths[song] = path; CalamityPatchDiagnostics.Increment(ref musicMisses); return path;
    }
}
