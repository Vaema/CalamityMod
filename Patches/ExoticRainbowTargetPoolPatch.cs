using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using CalamityMod.Rarities;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Replaces ExoticRainbow's disposable full-screen render target with an
/// identically-sized lease from Calamity's own reusable RenderTargetPool.
/// The drawing code and clear operation remain untouched.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
public static class ExoticRainbowTargetPoolPatch
{
    private static Mod? owner;
    private static MethodInfo? uniqueDraw;
    private static MethodInfo? originalRent;
    private static ILHook? hook;
    private static bool enabled;
    private static long cachedRents;

    public static bool Enabled => enabled;
    public static long CachedRents => Interlocked.Read(ref cachedRents);
    public static string Mode => enabled ? "shared-reuse" : "calamity-dispose";

    public static void Load(Mod mod)
    {
        owner = mod;
        uniqueDraw = typeof(ExoticRainbow.CustomTextSnippet).GetMethod(
            nameof(ExoticRainbow.CustomTextSnippet.UniqueDraw),
            BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMethodException(typeof(ExoticRainbow.CustomTextSnippet).FullName, "UniqueDraw");

        Type screenspacePool = typeof(ExoticRainbow).Assembly.GetType(
            "CalamityMod.Utilities.Daybreak.Buffers.ScreenspaceTargetPool",
            throwOnError: true)!;
        originalRent = screenspacePool
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Single(method =>
            {
                ParameterInfo[] parameters = method.GetParameters();
                return method.Name == "Rent" &&
                       parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(GraphicsDevice) &&
                       parameters[1].ParameterType == typeof(RenderTargetDescriptor?);
            });

        mod.Logger.Info("ExoticRainbow target-pool patch resolved; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        owner = null;
        uniqueDraw = null;
        originalRent = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
        {
            ILHook candidate = new(uniqueDraw!, ReplaceScreenspaceRent);
            hook = candidate;
            enabled = true;
            owner?.Logger.Info("ExoticRainbow render targets now use Calamity's reusable shared pool.");
        }
        else
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            owner?.Logger.Info("ExoticRainbow render targets restored to Calamity's original disposable pool.");
        }

        ResetCounters();
    }

    public static void ResetCounters() => Interlocked.Exchange(ref cachedRents, 0);

    private static void ReplaceScreenspaceRent(ILContext il)
    {
        ILCursor cursor = new(il);
        int replacements = 0;
        while (cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchCallOrCallvirt(originalRent!)))
        {
            cursor.Remove();
            cursor.EmitDelegate(RentCached);
            replacements++;
        }

        if (replacements != 1)
            throw new InvalidOperationException($"Expected one ExoticRainbow screenspace Rent call, found {replacements}.");
    }

    private static RenderTargetLease RentCached(
        object ignoredScreenspacePool,
        GraphicsDevice device,
        RenderTargetDescriptor? descriptor)
    {
        _ = ignoredScreenspacePool;
        PresentationParameters presentation = device.PresentationParameters;
        CalamityPatchDiagnostics.Increment(ref cachedRents);
        return RenderTargetPool.Shared.Rent(
            device,
            presentation.BackBufferWidth,
            presentation.BackBufferHeight,
            descriptor ?? RenderTargetDescriptor.Default);
    }
}
