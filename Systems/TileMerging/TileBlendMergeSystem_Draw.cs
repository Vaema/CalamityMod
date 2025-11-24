using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static Color MediumQualityLightRequirement;
        private static Color HighQualityLightRequirement;

        private static readonly Rectangle[] Rects9Slice = [
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

        private static readonly Rectangle[] Rects4Slice = [
            new Rectangle(x: 0, y: 0, width: 8, height: 8),
            new Rectangle(x: 8, y: 0, width: 8, height: 8),
            new Rectangle(x: 0, y: 8, width: 8, height: 8),
            new Rectangle(x: 8, y: 8, width: 8, height: 8),
        ];

        [ThreadStatic]
        private static Color[] ColorSliceBuffer;

        private static void OnQualityRequirementUpdate(Color highQualityLightReq, Color mediumQualityLightReq)
        {
            HighQualityLightRequirement = highQualityLightReq;
            MediumQualityLightRequirement = mediumQualityLightReq;
        }

        private void OnDrawTiles(On_Main.orig_DrawTiles orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
        {
            orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);

            if (!solidLayer)
                return;

            ColorSliceBuffer ??= new Color[9]; // Prepare ColorSliceBuffer if it's not ready

            var screenPosition = Main.Camera.UnscaledPosition;
            var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            var offset = zero + (Main.Camera.UnscaledPosition - Main.Camera.ScaledPosition);
            CalamityUtils.GetScreenDrawArea(screenPosition, offset, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY);

            for (int x = firstTileX; x <= lastTileX; x++)
            {
                for (int y = firstTileY; y <= lastTileY; y++)
                {
                    DrawOnTile(x, y);
                }
            }
        }

        public static void DrawOnTile(int tileX, int tileY)
        {
            var tile = CalamityUtils.ParanoidTileRetrieval(tileX, tileY);
            if (!tile.HasTile)
                return;

            var refLength = tile.Get<TileBlendingRefLengthData>().GetLength();
            if (refLength <= 0)
                return;

            if (!TryGetBlendingRefData(tileX, tileY, out var blendRefs))
                return;

            // Generic Drawing Parameter
            var tileType = tile.TileType;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(tileX * 16, tileY * 16) - Main.screenPosition + zero;
            var tileRandomFrame = Math.Clamp(tile.TileFrameNumber, 0, 2);
            var isFullBright = tile.IsTileFullbright;
            Color tileLight = Lighting.GetColor(tileX, tileY);

            // Sliced Rendering
            int sliceLength = 0;
            var sliceRects = Array.Empty<Rectangle>();

            // Is HalfBlock condition is also in vanilla, so we follow that
            if (Lighting.NotRetro && !tile.IsHalfBlock && !TileID.Sets.DontDrawTileSliced[tileType])
            {
                if (tileLight.IsAnyChannelGreaterThan(HighQualityLightRequirement))
                {
                    sliceLength = 9;
                    sliceRects = Rects9Slice;
                    Lighting.GetColor9Slice(tileX, tileY, ref ColorSliceBuffer);
                }
                else if (tileLight.IsAnyChannelGreaterThan(MediumQualityLightRequirement))
                {
                    sliceLength = 4;
                    sliceRects = Rects4Slice;
                    Lighting.GetColor4Slice(tileX, tileY, ref ColorSliceBuffer);
                }
            }

            // If tile is Actuated, Set brightness to 40%
            // Otherwise it sets to 100% or 160% (if shine)
            var finalColorMultiplier = tile.IsActuated ? 0.4f : (Main.tileShine2[tileType] ? 1.6f : 1.0f);

            foreach (var blendRef in blendRefs)
            {
                var sheetIdx = blendRef.SheetIndex;
                var data = blendRef.BlendData;

                // Break here as standard for TileBlendingData is 0->Count fill, so further fields should be also Invalid
                if (sheetIdx == TileBlendTextureLoader.EmptySlot)
                    break;

                var rect = TileBlendTexture.SideFlagsToSheetRect(data);
                var texture = TileBlendTextureLoader.Registry[sheetIdx].BlendTextures[tileRandomFrame];

                // No Slice Drawing
                if (sliceLength <= 0 || isFullBright)
                {
                    var drawColor = isFullBright ? Color.White : tileLight;
                    var finalColor = CalamityUtils.ApplyPaint(tile.TileColor, drawColor, deepPaintOnly: false) * finalColorMultiplier;
                    Main.spriteBatch.Draw(texture, drawPos, rect, finalColor, rotation: 0.0f, origin: default, scale: 1.0f, SpriteEffects.None, layerDepth: 0.0f);
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
                    var drawColorVec = (tileLight.ToVector3() + ColorSliceBuffer[i].ToVector3()) * 0.5f;
                    var finalColor = CalamityUtils.ApplyPaint(tile.TileColor, new Color(drawColorVec), deepPaintOnly: false) * finalColorMultiplier;
                    Main.spriteBatch.Draw(texture, destinationSlicePos, sourceSliceRect, finalColor, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
