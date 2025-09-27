using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Drawing;
using System.Reflection;
using Terraria.ID;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static readonly BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly FastField<TileDrawing, Color> MediumQualityLightRequirement = new("_mediumQualityLightingRequirement", NonPublicInstance);
        private static readonly FastField<TileDrawing, Color> HighQualityLightRequirement = new("_highQualityLightingRequirement", NonPublicInstance);

        private readonly static Rectangle[] Rects9Slice = [
            new Rectangle(x: 0, y: 0, width: 4, height: 4),
            new Rectangle(x: 4, y: 0, width: 8, height: 4),
            new Rectangle(x: 12, y: 0, width: 4, height: 4),

            new Rectangle(x: 0, y: 4, width: 4, height: 8),
            new Rectangle(x: 4, y: 4, width: 8, height: 8),
            new Rectangle(x: 12, y: 4, width: 4, height: 8),

            new Rectangle(x: 0, y: 12, width: 4, height: 4),
            new Rectangle(x: 4, y: 12, width: 8, height: 4),
            new Rectangle(x: 12, y: 12, width: 4, height: 4),
        ];

        private readonly static Rectangle[] Rects4Slice = [
            new Rectangle(x: 0, y: 0, width: 8, height: 8),
            new Rectangle(x: 8, y: 0, width: 8, height: 8),
            new Rectangle(x: 0, y: 8, width: 8, height: 8),
            new Rectangle(x: 8, y: 8, width: 8, height: 8),
        ];

        private enum SliceState : byte
        {
            None,
            Slice_4,
            Slice_9
        }

        private static void OnDrawSingleTile(On_TileDrawing.orig_DrawSingleTile orig, TileDrawing self, TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY)
        {
            orig(self, drawData, solidLayer, waterStyleOverride, screenPosition, screenOffset, tileX, tileY);

            var tile = drawData.tileCache;
            var tileType = tile.TileType;

            Color tileLight = drawData.tileLight;
            if (tileLight.R <= 0 && tileLight.G <= 0 && tileLight.B <= 0)
                return;

            // Generic Drawing Parameter
            Vector2 drawPos = new Vector2(tileX * 16, tileY * 16) - screenPosition + screenOffset;
            var tileRandomFrame = Math.Clamp(tile.TileFrameNumber, 0, 2);
            var isFullBright = tile.IsTileFullbright;

            // Sliced Rendering
            var sliceRenderingPrepared = false;
            var slices = drawData.colorSlices;
            int sliceLength = 0;
            var sliceRects = Array.Empty<Rectangle>();
            var sliceState = SliceState.None;
            var shouldTileShine = ShouldTileShine(tileType, (short)(drawData.tileFrameX + drawData.addFrX));

            var blendingData = tile.Get<TileBlendingData>(); // Since we are not editing the value, we can just copy the values from here
            for (int idx = 0; idx < TileBlendingData.Length; idx++)
            {
                blendingData.Get(idx, out var sheetIdx, out var data);

                // Break here as standard for TileBlendingData is 0->Count fill, so further fields should be also Invalid
                if (sheetIdx == TileBlendTextureLoader.EmptySlot)
                    break;

                var rect = TileBlendTexture.SideFlagsToSheetRect(data);
                var texture = TileBlendTextureLoader.Registry[sheetIdx].BlendTextures[tileRandomFrame];

                // Prepare Slice Rendering
                if (!sliceRenderingPrepared)
                {
                    // Is HalfBlock condition is also in vanilla, so we follow that
                    if (Lighting.NotRetro && !tile.IsHalfBlock && !TileID.Sets.DontDrawTileSliced[tileType])
                    {
                        var midQualityThreshold = MediumQualityLightRequirement.Get(self);
                        var highQualityThreshold = HighQualityLightRequirement.Get(self);
                        if (tileLight.IsAnyChannelGreaterThan(highQualityThreshold))
                        {
                            sliceLength = 9;
                            sliceState = SliceState.Slice_9;
                            sliceRects = Rects9Slice;
                            Lighting.GetColor9Slice(tileX, tileY, ref slices);
                        }
                        else if (tileLight.IsAnyChannelGreaterThan(midQualityThreshold))
                        {
                            sliceLength = 4;
                            sliceState = SliceState.Slice_4;
                            sliceRects = Rects4Slice;
                            Lighting.GetColor4Slice(tileX, tileY, ref slices);
                        }
                    }

                    sliceRenderingPrepared = true;
                }

                // No Slice Drawing
                if (sliceState == SliceState.None || isFullBright)
                {
                    var drawColor = isFullBright ? Color.White : tileLight;
                    Main.spriteBatch.Draw(texture, drawPos, rect, drawColor, rotation: 0.0f, origin: default, scale: 1.0f, SpriteEffects.None, layerDepth: 0.0f);
                    continue;
                }

                // Sliced Drawing
                for (int i = 0; i < sliceLength; i++)
                {
                    // Calculate the source rectangle for the specific slice from the blend texture sheet
                    var sourceSliceRect = sliceRects[i];
                    sourceSliceRect.X += rect.X;
                    sourceSliceRect.Y += rect.Y;

                    // Calculate the destination position for the slice on the screen
                    var destinationSlicePos = drawPos + sliceRects[i].Location.ToVector2();

                    var drawColorVec = (tileLight.ToVector3() + slices[i]) * 0.5f;
                    drawColorVec *= drawData.colorTint.ToVector3();

                    // Tile is Actuated, Reduce brightness
                    if (tile.IsActuated)
                    {
                        drawColorVec *= 0.4f;
                    }

                    // Tile is shining
                    if (shouldTileShine)
                    {
                        Main.shine(ref drawColorVec, tileType);
                    }

                    Main.spriteBatch.Draw(texture, destinationSlicePos, sourceSliceRect, new Color(drawColorVec), 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
                }
            }
        }

        private static MethodInfo ShouldTileShineMethod;
        private static bool ShouldTileShine(ushort type, short frameX)
        {
            ShouldTileShineMethod ??= typeof(TileDrawing).GetMethod("ShouldTileShine", NonPublicStatic);
            return (bool)ShouldTileShineMethod.Invoke(null, [type, frameX]);
        }
    }
}
