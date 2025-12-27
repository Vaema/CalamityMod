using System;
using System.Collections.Generic;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod.Graphics;

internal sealed class ManagedRenderTargetPool : RenderTargetPool
{
    public new static ManagedRenderTargetPool Shared { get; } = new();

    private static readonly List<ManagedRenderTarget> managedTargets = [];

    public override RenderTargetLease Rent(GraphicsDevice device, int width, int height, RenderTargetDescriptor descriptor)
    {
        throw new InvalidOperationException("ManagedRenderTargetPool does not support directly leasing targets");
    }

    public RenderTargetLease Register(ManagedRenderTarget managedTarget)
    {
        managedTargets.Add(managedTarget);
        return new RenderTargetLease(managedTarget.CreationCondition(Main.screenWidth, Main.screenHeight), this);
    }

    public override void Return(RenderTargetLease lease)
    {
        lease.Target.Dispose();
    }

    public void Return(ManagedRenderTarget managedRt)
    {
        _ = managedTargets.Remove(managedRt);
        Return(managedRt.lease);
    }

    public override void Dispose()
    {
        foreach (var target in managedTargets)
        {
            target.Dispose();
        }

        managedTargets.Clear();
    }

    internal void ResizeTargets()
    {
        foreach (var target in managedTargets)
        {
            if (target.ShouldResetUponScreenResize)
            {
                target.lease.Target.Dispose();
                target.lease.Target = target.CreationCondition(Main.screenWidth, Main.screenHeight);
            }
        }
    }
}

[Obsolete("ManagedRenderTarget is obsolete; developers should use RenderTarget2Ds and RenderTargetLeases")]
public class ManagedRenderTarget : IDisposable
{
    internal RenderTargetLease lease;

    public readonly RenderTargetCreationCondition CreationCondition;

    public readonly bool ShouldResetUponScreenResize;

    public readonly bool ShouldAutoDispose;

    public bool WaitingForFirstInitialization => false;

    public bool IsUninitialized => false;

    public bool IsDisposed
    {
        get;
        private set;
    }

    /// <summary>
    /// The actual <see cref="RenderTarget2D"/> instance that the wrapper contains.
    /// </summary>
    public RenderTarget2D Target => lease.Target;

    public int Width => Target.Width;

    public int Height => Target.Height;

    public Vector2 Size => new(Width, Height);

    public int TimeSinceLastAccessed;

    public delegate RenderTarget2D RenderTargetCreationCondition(int screenWidth, int screenHeight);

    [Obsolete("use ScreenspaceTargetPool")]
    public static RenderTarget2D CreateScreenSizedTarget(int screenWidth, int screenHeight) => new(Main.instance.GraphicsDevice, screenWidth, screenHeight);

    public ManagedRenderTarget(bool shouldResetUponScreenResize, RenderTargetCreationCondition creationCondition, bool shouldAutoDispose = true)
    {
        ShouldResetUponScreenResize = shouldResetUponScreenResize;
        CreationCondition = creationCondition;
        ShouldAutoDispose = shouldAutoDispose;
        lease = ManagedRenderTargetPool.Shared.Register(this);
    }

    /// <summary>
    /// Sets the current render target to the provided one.
    /// </summary>
    /// <param name="target">The render target to swap to</param>
    /// <param name="flushColor">The color to clear the screen with. Transparent by default</param>
    [Obsolete("Use RenderTargetScope")]
    public void SwapTo(Color? flushColor = null) => Target.SwapTo(flushColor);

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        ManagedRenderTargetPool.Shared.Return(this);
    }

    // Does nothing
    public void Recreate(int screenWidth, int screenHeight) { }

    public static implicit operator RenderTarget2D(ManagedRenderTarget target) => target.Target;
}
