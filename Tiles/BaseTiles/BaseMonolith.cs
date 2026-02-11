using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.BaseTiles
{
    public abstract class BaseMonolith : ModTile
    {
        public abstract int TileWidth { get; }
        public abstract int TileHeight { get; }
        public abstract int AnimationFrameCount { get; }
        public abstract int AnimationDelay { get; }
        public abstract int CursorItemType { get; }
        public virtual bool HasBottomTile18PixelsHeight => true;
        public int EnabledFrameY => AnimationFrameHeight;

        /// <summary>
        /// GlowMask to use for Monolith
        /// </summary>
        public Asset<Texture2D> GlowMask;

        /// <summary>
        /// Sound to play on right-clicked by player
        /// </summary>
        public SoundStyle? RightClickSound = SoundID.Mech;

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = CursorItemType != 0;
            player.cursorItemIconID = CursorItemType;
        }

        public override bool RightClick(int i, int j)
        {
            ToggleMonolith(i, j);
            SoundEngine.PlaySound(RightClickSound, new Vector2(i * 16, j * 16));
            return true;
        }

        public override void HitWire(int i, int j)
        {
            ToggleMonolith(i, j);
        }

        private void ToggleMonolith(int i, int j)
        {
            var tile = Main.tile[i, j];
            var width = 18 * TileWidth;
            var height = AnimationFrameHeight;
            var leftTopI = i - ((tile.TileFrameX % width) / 18);
            var leftTopJ = j - ((tile.TileFrameY % height) / 18);
            var enabled = tile.TileFrameY >= height;

            for (int o = 0; o < TileWidth; o++)
            {
                for (int p = 0; p < TileHeight; p++)
                {
                    var relI = leftTopI + o;
                    var relJ = leftTopJ + p;
                    var relTile = Main.tile[relI, relJ];

                    if (enabled) relTile.TileFrameY -= (short)height;
                    else relTile.TileFrameY += (short)height;

                    if (Wiring.running)
                    {
                        Wiring.SkipWire(relI, relJ);
                    }
                }
            }

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendTileSquare(-1, leftTopI, leftTopJ, TileWidth, TileHeight);
            }
        }

        public sealed override void NearbyEffects(int i, int j, bool closer)
        {
            var enabled = Main.tile[i, j].TileFrameY >= EnabledFrameY;
            NearbyEffects(i, j, closer, enabled, Main.LocalPlayer);
        }

        public virtual void NearbyEffects(int i, int j, bool closer, bool monolithEnabled, Player localPlayer)
        {

        }

        public virtual Color GetGlowMaskDrawColor(int i, int j)
        {
            return Color.White;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter >= AnimationDelay)
            {
                frameCounter = 0;
                if (++frame >= AnimationFrameCount)
                {
                    frame = 0;
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].IsTileActuallyInvisible())
                return false;

            var tile = Main.tile[i, j];
            var texture = TextureAssets.Tile[Type].Value;

            var zero = Main.drawToScreen ? Vector2.Zero : new(Main.offScreenRange, Main.offScreenRange);
            var drawPos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;

            var animateFrameOffset = (tile.TileFrameY >= EnabledFrameY) ? Main.tileFrame[Type] * AnimationFrameHeight : 0;
            var isHeight18Pixels = HasBottomTile18PixelsHeight && (tile.TileFrameY % AnimationFrameHeight) >= (18 * (TileHeight - 1));
            var height = isHeight18Pixels ? 18 : 16;

            var rect = new Rectangle(tile.TileFrameX, tile.TileFrameY + animateFrameOffset, 16, height);

            var drawColor = Lighting.GetColor(i, j);
            var glowColor = GetGlowMaskDrawColor(i, j);

            Main.spriteBatch.Draw(texture, drawPos, rect, drawColor, 0f, default, 1f, SpriteEffects.None, 0f);
            if (GlowMask != null)
                Main.spriteBatch.Draw(GlowMask.Value, drawPos, rect, glowColor, 0f, default, 1f, SpriteEffects.None, 0f);

            DrawExtra(drawPos, rect, drawColor);

            // 02FEB2025: Ozzatron: code lifted from https://github.com/CalamityTeam/CalamityModPublic/pull/77
            // transplanted into base monolith as part of manual cherry pick merge
            //
            // Draws the Smart Cursor Highlight.
            Color highlightColor;
            var highlight = TextureAssets.HighlightMask[Type];
            if (highlight != null && highlight.IsLoaded && Main.InSmartCursorHighlightArea(i, j, out var actuallySelected))
            {
                int avgBrightness = (drawColor.R + drawColor.G + drawColor.B) / 3;
                if (avgBrightness > 10)
                {   
                    highlightColor = Colors.GetSelectionGlowColor(actuallySelected, avgBrightness); 
                    Main.spriteBatch.Draw(highlight.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, rect, highlightColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                }
            }

            return false;
        }

        public virtual void DrawExtra(Vector2 drawPos, Rectangle rect, Color tileColor)
        {

        }
    }
}
