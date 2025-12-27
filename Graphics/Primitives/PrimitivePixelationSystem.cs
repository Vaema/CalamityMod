using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Systems.Graphic;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Primitives
{
    public class PrimitivePixelationSystem : ModSystem
    {
        #region Fields/Properties
        private static RenderTargetLease PixelationTarget_BeforeAllTiles;

        private static RenderTargetLease PixelationTarget_BeforeSolidTiles;

        private static RenderTargetLease PixelationTarget_BeforeNPCs;

        private static RenderTargetLease PixelationTarget_AfterNPCs;

        private static RenderTargetLease PixelationTarget_BeforeProjectiles;

        private static RenderTargetLease PixelationTarget_AfterProjectiles;

        private static RenderTargetLease PixelationTarget_AfterPlayers;

        private static RenderTargetLease PixelationTarget_AfterDusts;

        private static RenderTargetLease PixelationTarget_AfterEverything;

        /// <summary>
        /// Whether the system is currently rendering any primitives.
        /// </summary>
        public static bool CurrentlyRendering
        {
            get;
            private set;
        }
        #endregion

        #region Loading
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() =>
            {
                PixelationTarget_BeforeAllTiles = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_BeforeSolidTiles = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_BeforeNPCs = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_AfterNPCs = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_BeforeProjectiles = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_AfterProjectiles = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_AfterPlayers = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_AfterDusts = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
                PixelationTarget_AfterEverything = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => (w / 2, h / 2));
            });

            GeneralDrawLayerSystem.OnDrawLayer += DrawTargetScaled;
            GeneralDrawLayerSystem.OnPrepareDraw += DrawToTargets;
            //On_Main.DrawBackgroundBlackFill += DrawTarget_BeforeAllTiles;
            //On_Main.DoDraw_Tiles_Solid += DrawTarget_BeforeSolidTiles;
            //On_Main.DoDraw_DrawNPCsOverTiles += DrawTarget_NPCs;
            //On_Main.DrawProjectiles += DrawTarget_Projectiles;
            //On_Main.DrawPlayers_AfterProjectiles += DrawTarget_Players;
            //On_Main.DrawDust += DrawTarget_AfterDusts;
            //On_Main.DrawInfernoRings += DrawTarget_AfterEverything;
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            GeneralDrawLayerSystem.OnDrawLayer -= DrawTargetScaled;
            GeneralDrawLayerSystem.OnPrepareDraw -= DrawToTargets;
        }
        #endregion

        #region Drawing To Targets
        private void DrawToTargets()
        {
            if (Main.gameMenu)
            {
                return;
            }

            var beforeAllTiles = new List<IPixelatedPrimitiveRenderer>();
            var beforeSolidTiles = new List<IPixelatedPrimitiveRenderer>();
            var beforeNPCs = new List<IPixelatedPrimitiveRenderer>();
            var afterNPCs = new List<IPixelatedPrimitiveRenderer>();
            var beforeProjectiles = new List<IPixelatedPrimitiveRenderer>();
            var afterProjectiles = new List<IPixelatedPrimitiveRenderer>();
            var afterPlayers = new List<IPixelatedPrimitiveRenderer>();
            var afterDusts = new List<IPixelatedPrimitiveRenderer>();
            var afterEverything = new List<IPixelatedPrimitiveRenderer>();

            // Check every active projectile.
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                // If the projectile is active, a mod projectile, and uses the interface, add it to the list of primitives to draw this frame.
                if (projectile.ModProjectile != null && projectile.ModProjectile is IPixelatedPrimitiveRenderer pixelPrimitiveProjectile)
                {
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeAllTiles))
                        beforeAllTiles.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeSolidTiles))
                        beforeSolidTiles.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeNPCs))
                        beforeNPCs.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterNPCs))
                        afterNPCs.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeProjectiles))
                        beforeProjectiles.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterProjectiles))
                        afterProjectiles.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterPlayers))
                        afterPlayers.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterDusts))
                        afterDusts.Add(pixelPrimitiveProjectile);
                    if (pixelPrimitiveProjectile.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterEverything))
                        afterEverything.Add(pixelPrimitiveProjectile);
                }
            }

            // Check every active NPC.
            foreach (NPC npc in Main.ActiveNPCs)
            {
                // If the NPC is active, a mod NPC, and uses the interface, add it to the list of primitives to draw this frame.
                if (npc.ModNPC != null && npc.ModNPC is IPixelatedPrimitiveRenderer pixelPrimitiveNPC)
                {
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeAllTiles))
                        beforeAllTiles.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeSolidTiles))
                        beforeSolidTiles.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeNPCs))
                        beforeNPCs.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterNPCs))
                        afterNPCs.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.BeforeProjectiles))
                        beforeProjectiles.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterProjectiles))
                        afterProjectiles.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterPlayers))
                        afterPlayers.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterDusts))
                        afterDusts.Add(pixelPrimitiveNPC);
                    if (pixelPrimitiveNPC.LayerToRenderTo.HasFlag(GeneralDrawLayer.AfterEverything))
                        afterEverything.Add(pixelPrimitiveNPC);
                }
            }

            CurrentlyRendering = true;

            DrawPrimsToRenderTarget(PixelationTarget_BeforeAllTiles, GeneralDrawLayer.BeforeAllTiles, beforeAllTiles);
            DrawPrimsToRenderTarget(PixelationTarget_BeforeSolidTiles, GeneralDrawLayer.BeforeSolidTiles, beforeSolidTiles);
            DrawPrimsToRenderTarget(PixelationTarget_BeforeNPCs, GeneralDrawLayer.BeforeNPCs, beforeNPCs);
            DrawPrimsToRenderTarget(PixelationTarget_AfterNPCs, GeneralDrawLayer.AfterNPCs, afterNPCs);
            DrawPrimsToRenderTarget(PixelationTarget_BeforeProjectiles, GeneralDrawLayer.BeforeProjectiles, beforeProjectiles);
            DrawPrimsToRenderTarget(PixelationTarget_AfterProjectiles, GeneralDrawLayer.AfterProjectiles, afterProjectiles);
            DrawPrimsToRenderTarget(PixelationTarget_AfterPlayers, GeneralDrawLayer.AfterPlayers, afterPlayers);
            DrawPrimsToRenderTarget(PixelationTarget_AfterDusts, GeneralDrawLayer.AfterDusts, afterDusts);
            DrawPrimsToRenderTarget(PixelationTarget_AfterEverything, GeneralDrawLayer.AfterEverything, afterEverything);

            Main.instance.GraphicsDevice.SetRenderTarget(null);

            CurrentlyRendering = false;
        }

        private static void DrawPrimsToRenderTarget(RenderTarget2D renderTarget, GeneralDrawLayer layer, List<IPixelatedPrimitiveRenderer> pixelPrimitives)
        {
            // Swap to the target regardless, in order to clear any leftover content from last frame. Not doing this results in the final frame lingering once it stops rendering.
            renderTarget.SwapTo();

            if (pixelPrimitives.Any())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

                foreach (var pixelPrimitiveDrawer in pixelPrimitives)
                    pixelPrimitiveDrawer.RenderPixelatedPrimitives(Main.spriteBatch, layer);

                Main.spriteBatch.End();
            }
        }
        #endregion

        #region Target Drawing
        private static void DrawTargetScaled(GeneralDrawLayer drawLayer)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(ReturnAssociatedRenderTarget(drawLayer), Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
        }

        private static RenderTarget2D ReturnAssociatedRenderTarget(GeneralDrawLayer drawLayer)
        {
            return drawLayer switch
            {
                GeneralDrawLayer.BeforeAllTiles => PixelationTarget_BeforeAllTiles,
                GeneralDrawLayer.BeforeSolidTiles => PixelationTarget_BeforeSolidTiles,
                GeneralDrawLayer.BeforeNPCs => PixelationTarget_BeforeNPCs,
                GeneralDrawLayer.AfterNPCs => PixelationTarget_AfterNPCs,
                GeneralDrawLayer.BeforeProjectiles => PixelationTarget_BeforeProjectiles,
                GeneralDrawLayer.AfterProjectiles => PixelationTarget_AfterProjectiles,
                GeneralDrawLayer.AfterPlayers => PixelationTarget_AfterPlayers,
                GeneralDrawLayer.AfterDusts => PixelationTarget_AfterDusts,
                _ => PixelationTarget_AfterEverything
            };
        }
        #endregion
    }
}
