using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// ExoVortex2.PreDraw requests one constant texture and constructs three bound
/// delegates for every visible vortex on every frame. Cache the asset once and
/// the delegates once per ModProjectile instance. The original PrimitiveSettings,
/// shader selection, trail renderer, draw loop and dynamic instance methods stay
/// in Calamity's IL unchanged.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
internal static class ExoVortexDrawCachePatch
{
    private const string WorleyPath = "CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise";
    private const string PerlinPath = "Images/Misc/Perlin";

    private sealed class DelegateBundle
    {
        public readonly PrimitiveSettings.VertexWidthFunction Width;
        public readonly PrimitiveSettings.VertexColorFunction Color;
        public readonly PrimitiveSettings.VertexOffsetFunction Offset;

        public DelegateBundle(ExoVortex2 vortex)
        {
            Width = vortex.PrimitiveWidthFunction;
            Color = vortex.PrimitiveTrailColor;
            Offset = vortex.PrimitiveOffsetFunction;
        }
    }

    private static ConditionalWeakTable<ExoVortex2, DelegateBundle> delegateCache = new();
    private static Asset<Texture2D>? worleyAsset;
    private static Asset<Texture2D>? perlinAsset;
    private static MethodInfo? preDraw;
    private static ILHook? hook;
    private static bool enabled;
    private static long textureHits;
    private static long textureMisses;
    private static long delegateHits;
    private static long delegateMisses;
    private static long perlinHits;
    private static long perlinMisses;

    public static bool Enabled => enabled;
    public static long TextureHits => Interlocked.Read(ref textureHits);
    public static long TextureMisses => Interlocked.Read(ref textureMisses);
    public static long DelegateHits => Interlocked.Read(ref delegateHits);
    public static long DelegateMisses => Interlocked.Read(ref delegateMisses);
    public static long PerlinHits => Interlocked.Read(ref perlinHits);
    public static long PerlinMisses => Interlocked.Read(ref perlinMisses);
    public static string Mode => enabled ? "two-constant-assets-plus-per-instance-delegates" : "calamity-per-draw-allocation";

    public static void Load(Mod owner)
    {
        preDraw = typeof(ExoVortex2).GetMethod(
            nameof(ExoVortex2.PreDraw),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(Microsoft.Xna.Framework.Color).MakeByRefType()],
            modifiers: null)
            ?? throw new MissingMethodException(typeof(ExoVortex2).FullName, nameof(ExoVortex2.PreDraw));
        enabled = false;
        ResetCounters();
        owner.Logger.Info("ExoVortex2 draw cache resolved for one constant texture request and three bound delegates; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        preDraw = null;
        worleyAsset = null;
        perlinAsset = null;
        delegateCache = new();
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
        {
            hook = new ILHook(preDraw!, ReplacePerDrawAllocations);
            enabled = true;
        }
        else
        {
            enabled = false;
            hook?.Dispose();
            hook = null;
            worleyAsset = null;
            perlinAsset = null;
            delegateCache = new();
        }

        ResetCounters();
    }

    public static void ResetCounters()
    {
        Interlocked.Exchange(ref textureHits, 0);
        Interlocked.Exchange(ref textureMisses, 0);
        Interlocked.Exchange(ref delegateHits, 0);
        Interlocked.Exchange(ref delegateMisses, 0);
        Interlocked.Exchange(ref perlinHits, 0);
        Interlocked.Exchange(ref perlinMisses, 0);
    }

    private static void ReplacePerDrawAllocations(ILContext il)
    {
        MethodInfo textureGetter = GetHelper(nameof(GetWorleyTexture));
        MethodInfo widthGetter = GetHelper(nameof(GetWidthDelegate));
        MethodInfo colorGetter = GetHelper(nameof(GetColorDelegate));
        MethodInfo offsetGetter = GetHelper(nameof(GetOffsetDelegate));
        MethodInfo perlinGetter = GetHelper(nameof(GetPerlinAsset));
        MethodInfo useImage1Asset = typeof(MiscShaderData).GetMethod(
            nameof(MiscShaderData.UseImage1),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(Asset<Texture2D>)],
            modifiers: null)
            ?? throw new MissingMethodException(typeof(MiscShaderData).FullName, nameof(MiscShaderData.UseImage1));

        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdstr(WorleyPath)))
            throw new InvalidOperationException("ExoVortex2 Worley texture path was not found.");

        Instruction path = cursor.Next!;
        Instruction mode = path.Next!;
        Instruction request = mode.Next!;
        Instruction value = request.Next!;
        // The source omits the mode argument. Its compile-time default in the
        // installed tML build is encoded as literal 2; do not compare it with
        // this addon's enum member because enum names changed across tML APIs.
        if (!mode.MatchLdcI4(2) ||
            request.Operand is not GenericInstanceMethod requestMethod ||
            requestMethod.Name != "Request" ||
            value.Operand is not MethodReference valueGetter ||
            valueGetter.Name != "get_Value")
        {
            throw new InvalidOperationException("ExoVortex2 texture request IL no longer matches the audited sequence.");
        }

        path.OpCode = OpCodes.Call;
        path.Operand = il.Method.Module.ImportReference(textureGetter);
        MakeNop(mode);
        MakeNop(request);
        MakeNop(value);

        ILCursor perlinCursor = new(il);
        if (!perlinCursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdstr(PerlinPath)))
            throw new InvalidOperationException("ExoVortex2 Perlin shader image path was not found.");
        Instruction perlinPath = perlinCursor.Next!;
        Instruction useImage1 = perlinPath.Next!;
        if (useImage1.Operand is not MethodReference useImage1String ||
            useImage1String.Name != nameof(MiscShaderData.UseImage1) ||
            useImage1String.Parameters.Count != 1 ||
            useImage1String.Parameters[0].ParameterType.FullName != typeof(string).FullName)
        {
            throw new InvalidOperationException("ExoVortex2 UseImage1(string) IL no longer matches the audited sequence.");
        }
        perlinPath.OpCode = OpCodes.Call;
        perlinPath.Operand = il.Method.Module.ImportReference(perlinGetter);
        useImage1.OpCode = OpCodes.Callvirt;
        useImage1.Operand = il.Method.Module.ImportReference(useImage1Asset);

        int width = 0;
        int color = 0;
        int offset = 0;
        foreach (Instruction instruction in il.Body.Instructions)
        {
            if (instruction.OpCode != OpCodes.Newobj || instruction.Operand is not MethodReference constructor)
                continue;

            MethodInfo? replacement = constructor.DeclaringType.Name switch
            {
                "VertexWidthFunction" => width++ == 0 ? widthGetter : null,
                "VertexColorFunction" => color++ == 0 ? colorGetter : null,
                "VertexOffsetFunction" => offset++ == 0 ? offsetGetter : null,
                _ => null
            };
            if (replacement is null)
                continue;

            Instruction functionPointer = instruction.Previous!;
            Instruction instance = functionPointer.Previous!;
            if (functionPointer.OpCode != OpCodes.Ldftn || !instance.MatchLdarg(0))
                throw new InvalidOperationException($"Unexpected {constructor.DeclaringType.Name} construction sequence.");

            MakeNop(functionPointer);
            instruction.OpCode = OpCodes.Call;
            instruction.Operand = il.Method.Module.ImportReference(replacement);
        }

        if (width != 1 || color != 1 || offset != 1)
            throw new InvalidOperationException($"Expected one ExoVortex delegate of each kind, found width={width}, color={color}, offset={offset}.");
    }

    private static MethodInfo GetHelper(string name) =>
        typeof(ExoVortexDrawCachePatch).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new MissingMethodException(typeof(ExoVortexDrawCachePatch).FullName, name);

    private static void MakeNop(Instruction instruction)
    {
        instruction.OpCode = OpCodes.Nop;
        instruction.Operand = null;
    }

    private static Texture2D GetWorleyTexture()
    {
        if (worleyAsset is not null)
        {
            CalamityPatchDiagnostics.Increment(ref textureHits);
            return worleyAsset.Value;
        }

        // Preserve Calamity's source-level request semantics by using the same
        // omitted/default mode rather than naming a version-sensitive enum value.
        worleyAsset = ModContent.Request<Texture2D>(WorleyPath);
        CalamityPatchDiagnostics.Increment(ref textureMisses);
        return worleyAsset.Value;
    }

    private static Asset<Texture2D> GetPerlinAsset()
    {
        if (perlinAsset is not null)
        {
            CalamityPatchDiagnostics.Increment(ref perlinHits);
            return perlinAsset;
        }

        // MiscShaderData.UseImage1(string) calls Main.Assets.Request internally.
        // Cache that exact repository result and select its Asset overload.
        perlinAsset = Main.Assets.Request<Texture2D>(PerlinPath);
        CalamityPatchDiagnostics.Increment(ref perlinMisses);
        return perlinAsset;
    }

    private static DelegateBundle GetBundle(ExoVortex2 vortex)
    {
        if (delegateCache.TryGetValue(vortex, out DelegateBundle? cached))
        {
            CalamityPatchDiagnostics.Increment(ref delegateHits);
            return cached;
        }

        DelegateBundle created = new(vortex);
        delegateCache.Add(vortex, created);
        CalamityPatchDiagnostics.Increment(ref delegateMisses);
        return created;
    }

    private static PrimitiveSettings.VertexWidthFunction GetWidthDelegate(ExoVortex2 vortex) => GetBundle(vortex).Width;
    private static PrimitiveSettings.VertexColorFunction GetColorDelegate(ExoVortex2 vortex) => GetBundle(vortex).Color;
    private static PrimitiveSettings.VertexOffsetFunction GetOffsetDelegate(ExoVortex2 vortex) => GetBundle(vortex).Offset;
}
