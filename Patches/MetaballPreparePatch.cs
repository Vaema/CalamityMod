using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Uses an indexed active-metaball pass instead of Calamity's repeated LINQ
/// enumeration. For the audited two-layer SCal arena, the identical raw mask
/// is drawn once and copied to the second target before its distinct shader.
/// </summary>
[JITWhenModsEnabled("CalamityMod")]
[ExtendsFromMod("CalamityMod")]
public static class MetaballPreparePatch
{
    private static Mod? owner;
    private static MethodInfo? prepareMethod;
    private static List<Metaball>? metaballs;
    private static Func<Metaball, List<RenderTargetLease>>? getTargets;
    private static ILHook? hook;
    private static bool enabled;
    private static long preparedFrames;
    private static long activeMetaballs;
    private static long duplicateMasksCopied;

    public static bool Enabled => enabled;
    public static long PreparedFrames => preparedFrames;
    public static long ActiveMetaballs => activeMetaballs;
    public static long DuplicateMasksCopied => duplicateMasksCopied;
    public static string Mode => enabled ? "indexed-plus-audited-dedup" : "calamity-linq";

    public static void Load(Mod mod)
    {
        owner = mod;
        Type manager = typeof(MetaballManager);
        prepareMethod = manager.GetMethod("PrepareMetaballTargets", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(manager.FullName, "PrepareMetaballTargets");
        FieldInfo listField = manager.GetField("metaballs", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(manager.FullName, "metaballs");
        metaballs = listField.GetValue(null) as List<Metaball>
            ?? throw new InvalidOperationException("Calamity metaball list is unavailable.");

        FieldInfo targetsField = typeof(Metaball).GetField("LayerTargets", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(Metaball).FullName, "LayerTargets");
        getTargets = CreateTargetGetter(targetsField);
        mod.Logger.Info("Metaball indexed prepare/dedup target resolved; state is OFF.");
    }

    public static void Unload()
    {
        SetEnabled(false);
        owner = null;
        prepareMethod = null;
        metaballs = null;
        getTargets = null;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        if (value == enabled)
            return;

        if (value)
        {
            hook = new ILHook(prepareMethod!, InjectOptimizedDispatch);
            enabled = true;
            owner?.Logger.Info("Metaball preparation now uses indexed enumeration and audited layer deduplication.");
        }
        else
        {
            hook?.Dispose();
            hook = null;
            enabled = false;
            owner?.Logger.Info("Metaball preparation restored to Calamity's original implementation.");
        }

        ResetCounters();
    }

    public static void ResetCounters()
    {
        preparedFrames = 0;
        activeMetaballs = 0;
        duplicateMasksCopied = 0;
    }

    private static void InjectOptimizedDispatch(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.EmitDelegate(PrepareOptimized);
        ILLabel original = cursor.DefineLabel();
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Brfalse, original);
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
        cursor.MarkLabel(original);
    }

    private static bool PrepareOptimized()
    {
        List<Metaball> all = metaballs!;
        int firstActive = -1;
        for (int index = 0; index < all.Count; index++)
        {
            if (all[index].AnythingToDraw)
            {
                firstActive = index;
                break;
            }
        }

        if (firstActive < 0)
        {
            preparedFrames++;
            return true;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);

        for (int index = firstActive; index < all.Count; index++)
        {
            Metaball metaball = all[index];
            if (!metaball.AnythingToDraw)
                continue;

            activeMetaballs++;
            if (!Main.gamePaused && !metaball.IgnoreFPS)
                metaball.Update();
            metaball.PrepareSpriteBatch(Main.spriteBatch);

            List<RenderTargetLease> targets = getTargets!(metaball);
            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                using (targets[targetIndex].Scope(clearColor: Color.Transparent))
                {
                    if (targetIndex > 0 && metaball is ScalArenaMetaball)
                    {
                        // DrawInstances has no layer parameter and the audited SCal method is draw-only,
                        // so both raw masks are identical before their separate layer shaders.
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                            DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);
                        Main.spriteBatch.Draw(targets[0].Target, Vector2.Zero, Color.White);
                        duplicateMasksCopied++;
                    }
                    else
                    {
                        Vector2 offset = new(-2f, -2f);
                        Main.screenPosition += offset;
                        metaball.DrawInstances();
                        Main.screenPosition -= offset;
                    }

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);
                }
            }
        }

        Main.spriteBatch.End();
        preparedFrames++;
        return true;
    }

    private static Func<Metaball, List<RenderTargetLease>> CreateTargetGetter(FieldInfo field)
    {
        DynamicMethod method = new("topt_get_metaball_targets", typeof(List<RenderTargetLease>),
            [typeof(Metaball)], typeof(Metaball), skipVisibility: true);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ret);
        return method.CreateDelegate<Func<Metaball, List<RenderTargetLease>>>();
    }
}
