using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
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
        public static readonly FastField<WaterfallManager, Asset<Texture2D>[]> WaterfallTextureField = new("waterfallTexture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public override void OnModLoad()
        {
            On_TileLightScanner.GetTileLight += ApplyLiquidEmit;
            IL_LiquidRenderer.DrawNormalLiquids += LiquidDrawColorAndPostDraw; //Liquid Light
            On_TileDrawing.DrawPartialLiquid += LiquidSlopeDrawColors;
            IL_Main.oldDrawWater += OldLiquidPostDraw;

            On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += ModifyWaterfallColor;
        }

        // TODO: Better Transition Support.
        private static void ModifyEmit(Tile tile, int x, int y, ref Vector3 lightColor)
        {
            if (tile.HasTile || tile.LiquidAmount <= 0)
                return;

            if (tile.LiquidType == LiquidID.Water && TryGetModWaterStyleAs<IEmittableWaterStyle>(Main.waterStyle, out var waterStyle))
            {
                float R = 0f;
                float G = 0f;
                float B = 0f;

                waterStyle.ModifyLight(in tile, x, y, ref R, ref G, ref B);

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

        // TODO: Better Transition Support.
        private static void ModifyColor(int x, int y, int liquidStyle, ref VertexColors initialColor, bool isSlope = false)
        {
            if (TryGetModWaterStyleAs(liquidStyle, out IPaintableWaterStyle waterStyle))
            {
                var tile = Main.tile[x, y];
                waterStyle.ModifyDrawColor(in tile, x, y, ref initialColor, isSlope);
            }
            else if (liquidStyle == LiquidID.Lava && ModLavaStyleSystem.Initialized)
            {
                ModLavaStyleSystem.DrawColorSetup(x, y, ModLavaStyleSystem.LavaStyle, ref initialColor, isSlope);
            }
        }

        private static void PostDrawEffect(int x, int y, int liquidStyle)
        {
            if (TryGetModWaterStyleAs(liquidStyle, out IPostDrawEffectWaterStyle waterStyle))
            {
                var tile = Main.tile[x, y];
                waterStyle.PostDrawEffect(in tile, x, y);
            }
        }

        #region IL Edits

        private static void ApplyLiquidEmit(On_TileLightScanner.orig_GetTileLight orig, TileLightScanner self, int x, int y, out Vector3 outputColor)
        {
            orig(self, x, y, out outputColor);
            ModifyEmit(Main.tile[x, y], x, y, ref outputColor);
        }

        private static void LiquidDrawColorAndPostDraw(ILContext il)
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

            int liquidStyleLocalIdx = 0;
            if (!cursor.TryGotoNext(MoveType.Before,
                c => c.MatchLdloc(out _), // This is the local index for LiquidDrawCache*. Use it if you want for future
                c => c.MatchLdfld(typeField),
                c => c.MatchStloc(out liquidStyleLocalIdx)))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate the local index for Liquid Type (Style)");
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

            cursor.EmitLdloc(liquidStyleLocalIdx);
            cursor.EmitLdloc(xLocalIdx);
            cursor.EmitLdloc(yLocalIdx);
            cursor.EmitLdloca(vertexColorLocalIdx);
            cursor.EmitDelegate((int liquidStyle, int x, int y, ref VertexColors initialColor) =>
            {
                ModifyColor(x, y, liquidStyle, ref initialColor);
            });

            if (!cursor.TryGotoNext(MoveType.After,
                c => c.MatchCallOrCallvirt<TileBatch>(nameof(TileBatch.Draw))))
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate TileBatch.Draw call");
                return;
            }

            cursor.EmitLdloc(liquidStyleLocalIdx);
            cursor.EmitLdloc(xLocalIdx);
            cursor.EmitLdloc(yLocalIdx);
            cursor.EmitDelegate((int liquidStyle, int x, int y) =>
            {
                PostDrawEffect(x, y, liquidStyle);
            });
        }

        private static void LiquidSlopeDrawColors(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors)
        {
            tileCache.TilePos(out var x, out var y);
            var type = tileCache.TileType;
            var isFullblock = type == 0 || (!TileID.Sets.BlocksWaterDrawingBehindSelf[type] && behindBlocks);
            ModifyColor(x, y, liquidType, ref colors, isSlope: !isFullblock);
            orig(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
        }

        private static void OldLiquidPostDraw(ILContext il)
        {
            const string PatchName = "Old Liquid PostDraw";

            ILCursor cursor = new ILCursor(il);
            var typeRef = cursor.Context.Import(typeof(bool));
            var isDrawnVarDef = new VariableDefinition(typeRef);
            il.Body.Variables.Add(isDrawnVarDef);

            // Start of method, Reset IsDrawn Variable to False
            cursor.EmitLdcI4(0); // False
            cursor.EmitStloc(isDrawnVarDef);

            int xLocalIdx = 0;
            int yLocalIdx = 0;
            ILLabel endLoopLabal = null;
            if (!cursor.TryGotoNext(MoveType.After,
                c => c.MatchBrfalse(out endLoopLabal),
                c => c.MatchLdloc(out xLocalIdx),
                c => c.MatchLdloc(out yLocalIdx),
                c => c.MatchCallOrCallvirt<Lighting>(nameof(Lighting.GetColor)),
                c => c.MatchStloc(out _))) // Color color;
            {
                CalamityMod.Log.ILFailure(PatchName, "Could not locate Lighting.GetColor call");
                return;
            }

            // Start of loop, Passed Draw Condition, Set IsDrawn Variable to True
            cursor.EmitLdcI4(1); // True
            cursor.EmitStloc(isDrawnVarDef);

            // Jump to end of the loop
            cursor.GotoLabel(endLoopLabal, MoveType.AfterLabel);

            // Load Variables to stack
            cursor.EmitLdloc(isDrawnVarDef);
            cursor.EmitLdloc(xLocalIdx);
            cursor.EmitLdloc(yLocalIdx);
            cursor.EmitLdarg(1); // bool background
            cursor.EmitLdarg(2); // int WaterStyle
            cursor.EmitDelegate((bool isDrawn, int x, int y, bool isBackground, int waterStyle) =>
            {
                if (!isDrawn || isBackground)
                    return;

                if (Main.waterStyle != waterStyle)
                    return;

                var tile = Main.tile[x, y];
                var liquidType = tile.LiquidType;
                if (liquidType == LiquidID.Water)
                {
                    PostDrawEffect(x, y, waterStyle);
                }
            });

            // End, Reset IsDrawn to False
            cursor.EmitLdcI4(0); // False
            cursor.EmitStloc(isDrawnVarDef);
        }

        private static void ModifyWaterfallColor(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects)
        {
            if (TryGetModWaterfallStyleAs(waterfallType, out IPaintableWaterfallStyle style))
            {
                Tile tile = Main.tile[x, y];
                Texture2D texture = WaterfallTextureField.Get(self)[waterfallType].Value;
                Lighting.GetCornerColors(x, y, out var vertices, 1f);
                style.ModifyDrawColor(in tile, x, y, ref vertices);
                Main.tileBatch.Draw(texture, position, sourceRect, vertices, Vector2.Zero, 1f, effects);
            }
            else
            {
                orig(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
            }
        }

        #endregion

        private static ModWaterStyle GetModWaterStyle(int type)
        {
            return LoaderManager.Get<WaterStylesLoader>().Get(type);
        }

        private static ModWaterfallStyle GetModWaterfallStyle(int type)
        {
            return LoaderManager.Get<WaterFallStylesLoader>().Get(type);
        }

        private static bool TryGetModWaterStyleAs<T>(int type, out T style) where T : class
        {
            style = GetModWaterStyle(type) as T;
            return style != null;
        }

        private static bool TryGetModWaterfallStyleAs<T>(int type, out T style) where T : class
        {
            style = GetModWaterfallStyle(type) as T;
            return style != null;
        }
    }
}
