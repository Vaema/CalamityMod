using System;
using System.Reflection;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Replaces the one constant NebulaStar glow request in PostDraw with a cached
/// Asset value. Frame selection, color, alpha, rotation, origin and draw order
/// remain in Calamity's original IL.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class NebulaStarDrawCachePatch
{
    private const string GlowPath = "CalamityMod/Projectiles/Typeless/NebulaStarGlow";

    private static Asset<Texture2D>? glowAsset;
    private static MethodInfo? postDraw;
    private static ILHook? hook;
    private static Mod? owner;
    private static bool enabled;
    private static bool available = true;
    private static string compatibility = "resolved-not-installed";
    private static long hits;
    private static long misses;

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static long Hits => hits;
    public static long Misses => misses;
    public static string Mode => enabled ? "one-constant-glow-asset" : "calamity-per-draw-request";

    public static void Load(Mod mod)
    {
        owner = mod;
        postDraw = typeof(NebulaStar).GetMethod(
            nameof(NebulaStar.PostDraw),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(Color)],
            modifiers: null)
            ?? throw new MissingMethodException(typeof(NebulaStar).FullName, nameof(NebulaStar.PostDraw));
        enabled = false;
        available = true;
        compatibility = "resolved-not-installed";
        ResetCounters();
        mod.Logger.Info("NebulaStar glow request target resolved; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        postDraw = null;
        owner = null;
        glowAsset = null;
        available = true;
        compatibility = "unloaded";
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (!value)
        {
            enabled = false;
            hook?.Dispose();
            hook = null;
            glowAsset = null;
            compatibility = available ? "resolved-not-installed" : compatibility;
            ResetCounters();
            return;
        }

        if (enabled || !available)
            return;

        try
        {
            hook = new ILHook(postDraw!, ReplaceGlowRequest);
            enabled = true;
            compatibility = "audited-calamity-2.2.2";
            ResetCounters();
        }
        catch (Exception exception)
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            available = false;
            compatibility = $"il-anchor-failed:{exception.Message}";
            owner?.Logger.Error("NebulaStar glow asset cache was disabled.", exception);
        }
    }

    public static void ResetCounters()
    {
        hits = 0;
        misses = 0;
    }

    private static void ReplaceGlowRequest(ILContext il)
    {
        MethodInfo replacement = typeof(NebulaStarDrawCachePatch).GetMethod(
            nameof(GetGlowTexture), BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(typeof(NebulaStarDrawCachePatch).FullName, nameof(GetGlowTexture));

        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdstr(GlowPath)))
            throw new InvalidOperationException("NebulaStar glow texture path was not found.");

        Instruction path = cursor.Next!;
        Instruction mode = path.Next!;
        Instruction request = mode.Next!;
        Instruction value = request.Next!;
        if (!mode.MatchLdcI4(2) ||
            request.Operand is not GenericInstanceMethod requestMethod ||
            requestMethod.Name != "Request" ||
            value.Operand is not MethodReference valueGetter ||
            valueGetter.Name != "get_Value")
        {
            throw new InvalidOperationException("NebulaStar texture request IL no longer matches the audited sequence.");
        }

        ILCursor duplicate = new(il);
        duplicate.Goto(value, MoveType.After);
        if (duplicate.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdstr(GlowPath)))
            throw new InvalidOperationException("NebulaStar contains more than one glow request site.");

        path.OpCode = OpCodes.Call;
        path.Operand = il.Method.Module.ImportReference(replacement);
        MakeNop(mode);
        MakeNop(request);
        MakeNop(value);
    }

    private static void MakeNop(Instruction instruction)
    {
        instruction.OpCode = OpCodes.Nop;
        instruction.Operand = null;
    }

    private static Texture2D GetGlowTexture()
    {
        if (glowAsset is not null)
        {
            CalamityPatchDiagnostics.Increment(ref hits);
            return glowAsset.Value;
        }

        glowAsset = ModContent.Request<Texture2D>(GlowPath);
        CalamityPatchDiagnostics.Increment(ref misses);
        return glowAsset.Value;
    }
}
