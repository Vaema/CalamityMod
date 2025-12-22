using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Utilities.Daybreak.Buffers;

/// <summary>
///     Handles loading and unloading for the Daybreak buffers systems.
/// </summary>
public class BuffersLoadingSystem : ModSystem
{
    public override void Load()
    {
        if (Main.dedServ)
            return;

        Main.RunOnMainThread(
            () =>
            {
                // RenderTargetPool: Register frame lease disposal hook.
                On_Main.DoDraw += RenderTargetPool_DoDrawHook;

                // RenderTargetPreserver: Set presentation parameters.
                Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = Microsoft.Xna.Framework.Graphics.RenderTargetUsage.PreserveContents;
                Main.graphics.ApplyChanges();

                // ScreenspaceTargetPool: Add resize hook.
                On_Main.EnsureRenderTargetContent += ScreenspaceTargetPool_EnsureRenderTargetContentHook;
            }
        );
    }

    public override void Unload()
    {
        if (Main.dedServ)
            return;

        Main.RunOnMainThread(
            () =>
            {
                // Unhook events.
                On_Main.DoDraw -= RenderTargetPool_DoDrawHook;
                On_Main.EnsureRenderTargetContent -= ScreenspaceTargetPool_EnsureRenderTargetContentHook;

                // Dispose shared pools.
                RenderTargetPool.Shared.Dispose();
                ScreenspaceTargetPool.Shared.Dispose();
            }
        );
    }

    private static void RenderTargetPool_DoDrawHook(On_Main.orig_DoDraw orig, Main self, Microsoft.Xna.Framework.GameTime time)
    {
        RenderTargetPool.ClearPendingLeases();
        orig(self, time);
    }

    private static void ScreenspaceTargetPool_EnsureRenderTargetContentHook(On_Main.orig_EnsureRenderTargetContent orig, Main self)
    {
        // Let it run first to ensure tileTarget is initialized. We depend
        // on it as an arbitrary target to provide us a fully-sized target
        // when includes offscreenRange in the target size.
        orig(self);

        ScreenspaceTargetPool.Shared.ResizeCachedTargets(self.GraphicsDevice);
    }
}
