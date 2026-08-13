using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.PixelationSystem;

public class PixelationManager : ModSystem
{
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_BeforeAllTiles;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_BeforeSolidTiles;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_BeforeNPCs;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_AfterNPCs;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_BeforeProjectiles;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_AfterProjectiles;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_AfterPlayers;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_AfterDusts;
    private static Dictionary<BlendState, RenderTargetLease> PixelTargets_AfterEverything;

    private static List<PixelatedDrawer> ActivePixelatedDrawers_BeforeAllTiles;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_BeforeSolidTiles;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_BeforeNPCs;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_AfterNPCs;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_BeforeProjectiles;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_AfterProjectiles;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_AfterPlayers;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_AfterDusts;
    private static List<PixelatedDrawer> ActivePixelatedDrawers_AfterEverything;

    /// <summary>
    /// The resolution ratio at which pixelated drawers should draw.<br/>
    /// <c>0.5f</c> will draw at half resolution, <c>0.25f</c> will draw at quarter resolution, etc.
    /// </summary>
    internal const float PixelationResolution = 0.5f;

    /// <summary>
    /// The transformation matrix used by <see cref="PixelationManager"/> to draw pixelated assets.
    /// </summary>
    internal static Matrix PixelationMatrix
    {
        get
        {
            return Main.GameViewMatrix.TransformationMatrix
               * Matrix.CreateScale(PixelationResolution / Main.GameViewMatrix.Zoom.X, PixelationResolution / Main.GameViewMatrix.Zoom.Y, 1f)
               * Matrix.CreateTranslation(Main.GameViewMatrix.Translation.X * PixelationResolution, Main.GameViewMatrix.Translation.Y * PixelationResolution, 0f);
        }
    }

    public override void Load()
    {
        if (Main.dedServ)
            return;

        PixelTargets_BeforeAllTiles = [];
        PixelTargets_BeforeSolidTiles = [];
        PixelTargets_BeforeNPCs = [];
        PixelTargets_AfterNPCs = [];
        PixelTargets_BeforeProjectiles = [];
        PixelTargets_AfterProjectiles = [];
        PixelTargets_AfterPlayers = [];
        PixelTargets_AfterDusts = [];
        PixelTargets_AfterEverything = [];

        ActivePixelatedDrawers_BeforeAllTiles = [];
        ActivePixelatedDrawers_BeforeSolidTiles = [];
        ActivePixelatedDrawers_BeforeNPCs = [];
        ActivePixelatedDrawers_AfterNPCs = [];
        ActivePixelatedDrawers_BeforeProjectiles = [];
        ActivePixelatedDrawers_AfterProjectiles = [];
        ActivePixelatedDrawers_AfterPlayers = [];
        ActivePixelatedDrawers_AfterDusts = [];
        ActivePixelatedDrawers_AfterEverything = [];

        GeneralDrawLayerSystem.OnDrawLayerLate += DrawPixelatedTargets;
        GeneralDrawLayerSystem.OnPrepareDraw += PrepareTargets;
    }

    public override void Unload()
    {
        if (Main.dedServ)
            return;

        PixelTargets_BeforeAllTiles = null;
        PixelTargets_BeforeSolidTiles = null;
        PixelTargets_BeforeNPCs = null;
        PixelTargets_AfterNPCs = null;
        PixelTargets_BeforeProjectiles = null;
        PixelTargets_AfterProjectiles = null;
        PixelTargets_AfterPlayers = null;
        PixelTargets_AfterDusts = null;
        PixelTargets_AfterEverything = null;

        

        ActivePixelatedDrawers_BeforeAllTiles = null;
        ActivePixelatedDrawers_BeforeSolidTiles = null;
        ActivePixelatedDrawers_BeforeNPCs = null;
        ActivePixelatedDrawers_AfterNPCs = null;
        ActivePixelatedDrawers_BeforeProjectiles = null;
        ActivePixelatedDrawers_AfterProjectiles = null;
        ActivePixelatedDrawers_AfterPlayers = null;
        ActivePixelatedDrawers_AfterDusts = null;
        ActivePixelatedDrawers_AfterEverything = null;
    }

    public override void OnWorldUnload()
    {
        if (Main.dedServ)
            return;

        PixelTargets_BeforeAllTiles.Clear();
        PixelTargets_BeforeSolidTiles.Clear();
        PixelTargets_BeforeNPCs.Clear();
        PixelTargets_AfterNPCs.Clear();
        PixelTargets_BeforeProjectiles.Clear();
        PixelTargets_AfterProjectiles.Clear();
        PixelTargets_AfterPlayers.Clear();
        PixelTargets_AfterDusts.Clear();
        PixelTargets_AfterEverything.Clear();

        ActivePixelatedDrawers_BeforeAllTiles.Clear();
        ActivePixelatedDrawers_BeforeSolidTiles.Clear();
        ActivePixelatedDrawers_BeforeNPCs.Clear();
        ActivePixelatedDrawers_AfterNPCs.Clear();
        ActivePixelatedDrawers_BeforeProjectiles.Clear();
        ActivePixelatedDrawers_AfterProjectiles.Clear();
        ActivePixelatedDrawers_AfterPlayers.Clear();
        ActivePixelatedDrawers_AfterDusts.Clear();
        ActivePixelatedDrawers_AfterEverything.Clear();
    }


    /// <summary>
    /// Queues a <see cref="PixelatedDrawer"/> instance to draw pixelated assets.
    /// </summary>
    /// <param name="drawAction">All relevant drawing code should be written here.</param>
    /// <param name="drawLayer">The layer at which you'd like these assets to be drawn on. <br><b>e.g.</b> <see cref="GeneralDrawLayer.BeforeProjectiles"/> will make all drawn assets render behind all projectiles.</br></param>
    /// <param name="defaultBlendState">The default <see cref="BlendState"/> that your pixelated assets will be rendered in.
    /// <br>Leave as null to render them using <see cref="BlendState.AlphaBlend"/>.</br></param>
    public static void AddPixelatedDrawer(Action<Matrix> drawAction, GeneralDrawLayer drawLayer, BlendState defaultBlendState = null)
    {
        if (Main.dedServ || Main.gameMenu)
            return;

        BlendState blendState = defaultBlendState ?? BlendState.AlphaBlend;

        // Check if the associated render target for this drawer exists and create it if necessary.
        VerifyTargetExistence(drawLayer, blendState);

        PixelatedDrawer drawer = new(drawAction, drawLayer, blendState);
        ReturnAssociatedDrawerCollection(drawLayer).Add(drawer);
    }

    private static void PrepareTargets()
    {
        if (!Main.gameMenu && !Main.dedServ)
        {
            // Draw all collections to their respctive render targets.
            DrawCollectionsToTarget(PixelTargets_BeforeAllTiles, PixelationMatrix, ActivePixelatedDrawers_BeforeAllTiles);
            DrawCollectionsToTarget(PixelTargets_BeforeSolidTiles, PixelationMatrix, ActivePixelatedDrawers_BeforeSolidTiles);
            DrawCollectionsToTarget(PixelTargets_BeforeNPCs, PixelationMatrix, ActivePixelatedDrawers_BeforeNPCs);
            DrawCollectionsToTarget(PixelTargets_AfterNPCs, PixelationMatrix, ActivePixelatedDrawers_AfterNPCs);
            DrawCollectionsToTarget(PixelTargets_BeforeProjectiles, PixelationMatrix, ActivePixelatedDrawers_BeforeProjectiles);
            DrawCollectionsToTarget(PixelTargets_AfterProjectiles, PixelationMatrix, ActivePixelatedDrawers_AfterProjectiles);
            DrawCollectionsToTarget(PixelTargets_AfterPlayers, PixelationMatrix, ActivePixelatedDrawers_AfterPlayers);
            DrawCollectionsToTarget(PixelTargets_AfterDusts, PixelationMatrix, ActivePixelatedDrawers_AfterDusts);
            DrawCollectionsToTarget(PixelTargets_AfterEverything, PixelationMatrix, ActivePixelatedDrawers_AfterEverything);
        }
    }

    private static void DrawCollectionsToTarget(Dictionary<BlendState, RenderTargetLease> targetCollection, Matrix pixelationMatrix, List<PixelatedDrawer> drawerCollection)
    {
        foreach (var blendStateTargetPair in targetCollection)
        {
            using (blendStateTargetPair.Value.Scope(clearColor: Color.Transparent))
            {
                Main.spriteBatch.Begin(default, blendStateTargetPair.Key, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, pixelationMatrix);

                // Select only drawers who match this target's BlendState.
                var drawersByBlendState = drawerCollection.Where(d => d.DefaultBlendState == blendStateTargetPair.Key).ToList();
                if (drawersByBlendState.Count > 0)
                {
                    foreach (PixelatedDrawer drawer in drawersByBlendState)
                        drawer.DrawAction.Invoke(pixelationMatrix);
                }

                Main.spriteBatch.End();
            }
        }

        drawerCollection.Clear();
    }

    private static void DrawPixelatedTargets(GeneralDrawLayer drawLayer)
    {
        var targetCollection = ReturnAssociatedTargetCollection(drawLayer);
        foreach (var keyValuePair in targetCollection)
        {
            Main.spriteBatch.Begin(default, keyValuePair.Key, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float targetScale = 1f / PixelationResolution;
            Main.spriteBatch.Draw(keyValuePair.Value.Target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, targetScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
        }
    }

    private static List<PixelatedDrawer> ReturnAssociatedDrawerCollection(GeneralDrawLayer drawLayer)
    {
        List<PixelatedDrawer> drawerCollection = drawLayer switch
        {
            GeneralDrawLayer.BeforeAllTiles => ActivePixelatedDrawers_BeforeAllTiles,
            GeneralDrawLayer.BeforeSolidTiles => ActivePixelatedDrawers_BeforeSolidTiles,
            GeneralDrawLayer.BeforeNPCs => ActivePixelatedDrawers_BeforeNPCs,
            GeneralDrawLayer.AfterNPCs => ActivePixelatedDrawers_AfterNPCs,
            GeneralDrawLayer.BeforeProjectiles => ActivePixelatedDrawers_BeforeProjectiles,
            GeneralDrawLayer.AfterProjectiles => ActivePixelatedDrawers_AfterProjectiles,
            GeneralDrawLayer.AfterPlayers => ActivePixelatedDrawers_AfterPlayers,
            GeneralDrawLayer.AfterDusts => ActivePixelatedDrawers_AfterDusts,
            _ => ActivePixelatedDrawers_AfterEverything,
        };

        return drawerCollection;
    }

    private static Dictionary<BlendState, RenderTargetLease> ReturnAssociatedTargetCollection(GeneralDrawLayer drawLayer)
    {
        Dictionary<BlendState, RenderTargetLease> targetCollection = drawLayer switch
        {
            GeneralDrawLayer.BeforeAllTiles => PixelTargets_BeforeAllTiles,
            GeneralDrawLayer.BeforeSolidTiles => PixelTargets_BeforeSolidTiles,
            GeneralDrawLayer.BeforeNPCs => PixelTargets_BeforeNPCs,
            GeneralDrawLayer.AfterNPCs => PixelTargets_AfterNPCs,
            GeneralDrawLayer.BeforeProjectiles => PixelTargets_BeforeProjectiles,
            GeneralDrawLayer.AfterProjectiles => PixelTargets_AfterProjectiles,
            GeneralDrawLayer.AfterPlayers => PixelTargets_AfterPlayers,
            GeneralDrawLayer.AfterDusts => PixelTargets_AfterDusts,
            _ => PixelTargets_AfterEverything,
        };

        return targetCollection;
    }

    private static void VerifyTargetExistence(GeneralDrawLayer drawLayer, BlendState blendState)
    {
        switch (drawLayer)
        {
            case GeneralDrawLayer.BeforeAllTiles:
                if (!PixelTargets_BeforeAllTiles.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_BeforeAllTiles[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.BeforeSolidTiles:
                if (!PixelTargets_BeforeSolidTiles.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_BeforeSolidTiles[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.BeforeNPCs:
                if (!PixelTargets_BeforeNPCs.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_BeforeNPCs[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.AfterNPCs:
                if (!PixelTargets_AfterNPCs.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_AfterNPCs[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.BeforeProjectiles:
                if (!PixelTargets_BeforeProjectiles.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_BeforeProjectiles[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.AfterProjectiles:
                if (!PixelTargets_AfterProjectiles.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_AfterProjectiles[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.AfterPlayers:
                if (!PixelTargets_AfterPlayers.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_AfterPlayers[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.AfterDusts:
                if (!PixelTargets_AfterDusts.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_AfterDusts[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;

            case GeneralDrawLayer.AfterEverything:
                if (!PixelTargets_AfterEverything.ContainsKey(blendState))
                    Main.QueueMainThreadAction(() => PixelTargets_AfterEverything[blendState] = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (w, h) => ((int)(w * PixelationResolution), (int)(h * PixelationResolution))));
                break;
        }
    }
}
