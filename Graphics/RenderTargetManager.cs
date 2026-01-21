using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics
{
    /// <summary>
    /// Manager class that handles all created instances of <see cref="ManagedRenderTarget"/>
    /// </summary>
    [Obsolete("ManagedRenderTarget is obsolete; developers should use RenderTarget2Ds and RenderTargetLeases")]
    public class RenderTargetManager : ModSystem
    {
        public delegate void RenderTargetUpdateDelegate();

        /// <summary>
        /// Use this event to update/draw things to the target before anything else is drawn to the game.
        /// </summary>
        public static event RenderTargetUpdateDelegate RenderTargetUpdateLoopEvent;

        /// <summary>
        /// How many frames should pass since a target was last accessed before automatically disposing of it.
        /// </summary>
        public const int TimeBeforeAutoDispose = 600;

        public override void OnModLoad()
        {
            Main.OnPreDraw += HandleTargetUpdateLoop;
        }

        public override void OnModUnload()
        {
            Main.OnPreDraw -= HandleTargetUpdateLoop;
            RenderTargetUpdateLoopEvent = null;
        }

        private void HandleTargetUpdateLoop(GameTime obj)
        {
            RenderTargetUpdateLoopEvent?.Invoke();
        }
    }
}
