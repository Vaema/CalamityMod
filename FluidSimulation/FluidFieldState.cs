using System.Collections.Generic;
using CalamityMod.Graphics;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod.FluidSimulation
{
    // Calculations for these are done primarily on the GPU in shaders for performance-sake.
    public class FluidFieldState
    {
        public RenderTargetLease PreviousState;

        public RenderTargetLease NextState;

        public Queue<PixelQueueValue> PendingChanges = new();

        public readonly int Size;

        public readonly SurfaceFormat FieldContents;

        public void SwapState() => Utils.Swap(ref PreviousState, ref NextState);

        public FluidFieldState(int size, SurfaceFormat fieldContents = SurfaceFormat.Color)
        {
            if (Main.dedServ)
                return;

            Size = size;
            FieldContents = fieldContents;

            var descriptor = new RenderTargetDescriptor(FieldContents, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents, true);
            PreviousState = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice, Size, Size, descriptor);
            NextState = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice, Size, Size, descriptor);
        }
    }
}
