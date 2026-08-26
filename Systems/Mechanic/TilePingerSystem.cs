using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems;

public interface IPingedTileEffect
{
    /// <summary>
    /// The blend state of the tile effect's main shader.
    /// </summary>
    public BlendState BlendState => BlendState.AlphaBlend;

    /// <summary>
    /// Create and set up an effect to be drawn over all the registered tiles for this shader.
    /// </summary>
    /// <returns>The configured effect</returns>
    public Effect SetupEffect();

    /// <summary>
    /// Modifies the shader for each tile. Use this if your shader is using tile-specific data.
    /// </summary>
    /// <param name="pos">The position of the tile</param>
    /// <param name="effect">The effect being used</param>
    public void PerTileSetup(Point pos, ref Effect effect) { }

    /// <summary>
    /// Draws the tile, or an overlay for it. The shader is automatically applied.
    /// </summary>
    /// <param name="pos">The position of the tile</param>
    public void DrawTile(Point pos);

    /// <summary>
    /// What happens when a ping for this effect gets requested. Return false if the ping couldn't get added.
    /// </summary>
    /// <param name="position">Position of the ping being requested</param>
    /// <param name="pinger">The player that initiated the ping</param>
    /// <returns>Wether or not the ping's setup was successful</returns>
    public bool TryAddPing(Vector2 position, Player pinger);

    /// <summary>
    /// Wether or not this effect is active.
    /// </summary>
    public bool Active => true;

    /// <summary>
    /// Check to know if a tile needs to be drawn with this effect. This is only called if the effect is active.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>Wether or not the tile should be drawn with the effect</returns>
    public bool ShouldRegisterTile(int x, int y);

    /// <summary>
    /// Modify Tile Light when tile has pinged
    /// </summary>
    /// <param name="x">Tile X</param>
    /// <param name="y">Tile Y</param>
    /// <param name="tileLight">tileLight Draw Data</param>
    /// <param name="resultColor">Original Tile Color to Draw, Modify this value</param>
    public void ModifyTileLight(int x, int y, Color tileLight, ref Color resultColor);

    /// <summary>
    /// Called after a tile has been queued to be drawn with this effect. Can be used to edit the draw data of the tile.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="drawData">The tile's draw data</param>
    public void EditDrawData(int i, int j, ref TileDrawInfo drawData) { }

    /// <summary>
    /// Update call ran once per frame.
    /// </summary>
    public void UpdateEffect() { }
}

public class TilePingerSystem : ModSystem
{
    private static Dictionary<IPingedTileEffect, List<Point>> pingedTiles = [];
    private static Dictionary<IPingedTileEffect, List<Point>> pingedNonSolidTiles = [];
    private static Dictionary<IPingedTileEffect, List<Point>> drawCache = [];
    private static Dictionary<string, IPingedTileEffect> tileEffectLookup = [];
    private static IPingedTileEffect[] tileEffects = [];

    public static void RegisterEffect(string name, IPingedTileEffect tileEffect)
    {
        tileEffectLookup[name] = tileEffect;
        tileEffects = [.. tileEffectLookup.Values];
    }

    public override void Load()
    {
        if (Main.dedServ)
            return;

        On_TileDrawing.DrawTiles_GetLightOverride += ForceSufficientLight;
    }

    public override void Unload()
    {
        drawCache = null;
        pingedTiles = null;
        pingedNonSolidTiles = null;
        tileEffectLookup = null;
        tileEffects = null;
    }

    private static Color ForceSufficientLight(On_TileDrawing.orig_DrawTiles_GetLightOverride orig, TileDrawing self, int j, int i, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight)
    {
        Color returnColor = orig(self, j, i, tileCache, typeCache, tileFrameX, tileFrameY, tileLight);
        foreach (IPingedTileEffect effect in tileEffects)
        {
            if (effect.Active && effect.ShouldRegisterTile(i, j))
            {
                effect.ModifyTileLight(i, j, tileLight, ref returnColor);
            }
        }
        return returnColor;
    }

    public static bool AddPing(IPingedTileEffect effect, Vector2 position, Player pinger) => effect.TryAddPing(position, pinger);
    public static bool AddPing(string effectName, Vector2 position, Player pinger)
    {
        if (!Main.dedServ)
            return AddPing(tileEffectLookup[effectName], position, pinger);

        return false;
    }

    public static void RegisterTileToDraw(Point tilePos, string effectName, bool solid = true) => RegisterTileToDraw(tilePos, tileEffectLookup[effectName], solid);
    public static void RegisterTileToDraw(Point tilePos, IPingedTileEffect effect, bool solid = true)
    {
        //Unless we are in color light mode, we do not need the distinction between solid and non solid tiles.
        if (solid || !(Lighting.Mode == LightMode.Color))
        {
            if (!pingedTiles.ContainsKey(effect))
                pingedTiles.Add(effect, []);

            if (!pingedTiles[effect].Contains(tilePos))
                pingedTiles[effect].Add(tilePos);
        }

        else
        {
            if (!pingedNonSolidTiles.ContainsKey(effect))
                pingedNonSolidTiles.Add(effect, []);

            if (!pingedNonSolidTiles[effect].Contains(tilePos))
                pingedNonSolidTiles[effect].Add(tilePos);
        }
    }

    public override void PostUpdateEverything()
    {
        if (Main.dedServ || tileEffects is null)
            return;

        foreach (IPingedTileEffect effect in tileEffects)
        {
            effect.UpdateEffect();
        }
    }

    public override void PostDrawTiles()
    {
        if (pingedTiles is null || (pingedTiles.Keys.Count + pingedNonSolidTiles.Count < 1))
            return;

        drawCache.Clear();

        foreach (IPingedTileEffect solidEffect in pingedTiles.Keys)
        {
            drawCache.Add(solidEffect, pingedTiles[solidEffect].ConvertAll(position => new Point(position.X, position.Y)));
        }

        if (Lighting.Mode == LightMode.Color)
        {
            foreach (IPingedTileEffect nonSolidEffect in pingedNonSolidTiles.Keys)
            {
                List<Point> clonedList = pingedNonSolidTiles[nonSolidEffect].ConvertAll(position => new Point(position.X, position.Y));

                if (!drawCache.TryGetValue(nonSolidEffect, out var value))
                {
                    drawCache.Add(nonSolidEffect, clonedList);
                }
                else
                {
                    value.AddRange(clonedList);
                }
            }
        }

        foreach (IPingedTileEffect tileEffect in drawCache.Keys)
        //foreach (IPingedTileEffect tileEffect in pingedTiles.Keys)
        {
            Effect effect = tileEffect.SetupEffect();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, tileEffect.BlendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

            foreach (Point tilePos in drawCache[tileEffect])
            //foreach (Point tilePos in pingedTiles[tileEffect])
            {
                tileEffect.PerTileSetup(tilePos, ref effect);
                tileEffect.DrawTile(tilePos);

            }

            Main.spriteBatch.End();
        }
    }

    public static void ClearTiles()
    {
        ClearTiles(true);
        ClearTiles(false);
    }

    public static void ClearTiles(bool solid)
    {
        if (solid)
            pingedTiles.Clear();

        else
            pingedNonSolidTiles.Clear();
    }

    #region GlobalTile Hooks
    private class GlobalPingableTile : GlobalTile
    {
        public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            foreach (IPingedTileEffect effect in tileEffects)
            {
                if (!effect.Active || !effect.ShouldRegisterTile(i, j))
                    continue;

                int tileType = Main.tile[i, j].TileType;
                bool solid = true;

                //Necessary separation in the color lighting mode.
                if (Lighting.Mode == LightMode.Color)
                {
                    if (TileID.Sets.DrawTileInSolidLayer[tileType].HasValue)
                        solid = TileID.Sets.DrawTileInSolidLayer[tileType].Value;
                    else
                        solid = Main.tileSolid[tileType];
                }

                RegisterTileToDraw(new Point(i, j), effect, solid);
                effect.EditDrawData(i, j, ref drawData);
            }
        }
    }
    #endregion
}
