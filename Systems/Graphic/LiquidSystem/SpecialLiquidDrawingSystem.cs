using System;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    [Autoload(Side = ModSide.Client)]
    public sealed class SpecialLiquidDrawingSystem : ModSystem
    {
        public override void OnModLoad()
        {
            On_TileLightScanner.GetTileLight += ApplyLiquidEmit;
            IL_LiquidRenderer.DrawNormalLiquids += LiquidDrawColors; //Liquid Light
            On_TileDrawing.DrawPartialLiquid += LiquidSlopeDrawColors;
        }

        private static void ModifyEmit(Tile tile, int x, int y, ref Vector3 lightColor)
        {
            if (tile.HasTile || tile.LiquidAmount <= 0)
                return;

            if (tile.LiquidType == LiquidID.Water && TryGetModWaterStyleAs<IEmittableWaterStyle>(Main.waterStyle, out var waterStyle))
            {
                float R = 0f;
                float G = 0f;
                float B = 0f;

                waterStyle.ModifyLight(in tile, x, y, Main.waterStyle, ref R, ref G, ref B);

                lightColor.X = Math.Max(lightColor.X, R);
                lightColor.Y = Math.Max(lightColor.Y, G);
                lightColor.Z = Math.Max(lightColor.Z, B);
            }
            else if (tile.LiquidType == LiquidID.Lava && ModLavaStyleSystem.Initialized)
            {
                Vector3 lavaLight = new Vector3(0.55f, 0.33f, 0.11f);

                float R = ModLavaStyleSystem.LavaStyle == 0 ? lavaLight.X : 0f;
                float G = ModLavaStyleSystem.LavaStyle == 0 ? lavaLight.Y : 0f;
                float B = ModLavaStyleSystem.LavaStyle == 0 ? lavaLight.Z : 0f;
                ModLavaStyleSystem.ModifyLightSetup(x, y, ModLavaStyleSystem.LavaStyle, ref R, ref G, ref B);

                for (int styleIndex = 0; styleIndex < ModLavaStyleLoader.TotalCount; styleIndex++)
                {
                    if (ModLavaStyleSystem.LavaAlpha[styleIndex] > 0f && styleIndex != ModLavaStyleSystem.LavaStyle)
                    {
                        float r = styleIndex == 0 ? lavaLight.X : 0f;
                        float g = styleIndex == 0 ? lavaLight.Y : 0f;
                        float b = styleIndex == 0 ? lavaLight.Z : 0f;
                        ModLavaStyleSystem.ModifyLightSetup(x, y, styleIndex, ref r, ref g, ref b);

                        float r2 = ModLavaStyleSystem.LavaStyle == 0 ? lavaLight.X : 0f;
                        float g2 = ModLavaStyleSystem.LavaStyle == 0 ? lavaLight.Y : 0f;
                        float b2 = ModLavaStyleSystem.LavaStyle == 0 ? lavaLight.Z : 0f;
                        ModLavaStyleSystem.ModifyLightSetup(x, y, ModLavaStyleSystem.LavaStyle, ref r2, ref g2, ref b2);

                        R = float.Lerp(r, r2, ModLavaStyleSystem.LavaAlpha[ModLavaStyleSystem.LavaStyle]);
                        G = float.Lerp(g, g2, ModLavaStyleSystem.LavaAlpha[ModLavaStyleSystem.LavaStyle]);
                        B = float.Lerp(b, b2, ModLavaStyleSystem.LavaAlpha[ModLavaStyleSystem.LavaStyle]);
                    }
                }

                if (R != 0.0f || G != 0.0f || B != 0.0f)
                {
                    float colorManipulator = (float)(270 - Main.mouseTextColor) / 900f;
                    R += colorManipulator;
                    G += colorManipulator;
                    B += colorManipulator;
                }

                lightColor.X = Math.Max(lightColor.X, R);
                lightColor.Y = Math.Max(lightColor.Y, G);
                lightColor.Z = Math.Max(lightColor.Z, B);
            }
        }

        private static void ModifyColor(int x, int y, byte liquidType, ref VertexColors initialColor, bool isSlope = false)
        {
            if (liquidType == LiquidID.Water && TryGetModWaterStyleAs<IPaintableWaterStyle>(Main.waterStyle, out var waterStyle))
            {
                waterStyle.DrawColor(x, y, Main.waterStyle, ref initialColor, isSlope);
            }
            else if (liquidType == LiquidID.Lava && ModLavaStyleSystem.Initialized)
            {
                ModLavaStyleSystem.DrawColorSetup(x, y, ModLavaStyleSystem.LavaStyle, ref initialColor, isSlope);
            }
        }

        #region IL Edits

        private static void ApplyLiquidEmit(On_TileLightScanner.orig_GetTileLight orig, TileLightScanner self, int x, int y, out Vector3 outputColor)
        {
            orig(self, x, y, out outputColor);
            ModifyEmit(Main.tile[x, y], x, y, ref outputColor);
        }

        private static void LiquidDrawColors(ILContext il)
        {
            const string PatchName = "Liquid Draw Colors";

            ILCursor cursor = new ILCursor(il);

            var typeField = typeof(LiquidRenderer.LiquidDrawCache).GetField(nameof(LiquidRenderer.LiquidDrawCache.Type));
            if (typeField == null)
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not find FieldInfo for LiquidDrawCache.Type");
                return;
            }

            var lightingGetCornerColorsMethod = typeof(Lighting).GetMethod(nameof(Lighting.GetCornerColors));
            if (lightingGetCornerColorsMethod == null)
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not find MethodInfo for GetCornerColors");
                return;
            }

            var mainDrawTileInWaterMethod = typeof(Main).GetMethod(nameof(Main.DrawTileInWater));
            if (mainDrawTileInWaterMethod == null)
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not find FieldInfo for DrawTileInWater");
                return;
            }

            int typeLocalIdx = 0;
            if (!cursor.TryGotoNext(MoveType.Before,
                c => c.MatchLdloc(out _), // This is the local index for LiquidDrawCache*. Use it if you want for future
                c => c.MatchLdfld(typeField),
                c => c.MatchStloc(out typeLocalIdx)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the local index for Liquid Type");
                return;
            }

            int vertexColorLocalIdx = 0;
            if (!cursor.TryGotoNext(MoveType.Before,
                c => c.MatchLdloca(out vertexColorLocalIdx),
                c => c.MatchLdcR4(out _),
                c => c.MatchCallOrCallvirt(lightingGetCornerColorsMethod)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the local index for VertexColors");
                return;
            }

            int xLocalIdx = 0;
            int yLocalIdx = 0;
            if (!cursor.TryGotoNext(MoveType.Before,
                c => c.MatchLdarg(out _), // Vector2 drawOffset
                c => c.MatchLdloc(out xLocalIdx),
                c => c.MatchLdloc(out yLocalIdx),
                c => c.MatchCallOrCallvirt(mainDrawTileInWaterMethod)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the liquid vertex colors for drawing");
                return;
            }

            cursor.EmitLdloc(typeLocalIdx);
            cursor.EmitLdloc(xLocalIdx);
            cursor.EmitLdloc(yLocalIdx);
            cursor.EmitLdloca(vertexColorLocalIdx);
            cursor.EmitDelegate((int type, int x, int y, ref VertexColors initialColor) =>
            {
                ModifyColor(x, y, (byte)type, ref initialColor);
            });
        }

        private void LiquidSlopeDrawColors(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors)
        {
            tileCache.TilePos(out var x, out var y);
            var type = tileCache.TileType;
            var isFullblock = type == 0 || (!TileID.Sets.BlocksWaterDrawingBehindSelf[type] && behindBlocks);
            ModifyColor(x, y, (byte)liquidType, ref colors, isSlope: !isFullblock);
            orig(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
        }

        #endregion

        private static ModWaterStyle GetModWaterStyle(int type)
        {
            return LoaderManager.Get<WaterStylesLoader>().Get(type);
        }

        private static bool TryGetModWaterStyleAs<T>(int type, out T style) where T : class
        {
            style = GetModWaterStyle(type) as T;
            return style != null;
        }
    }
}
