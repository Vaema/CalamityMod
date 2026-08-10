using CalamityMod.CalPlayer;
using CalamityMod.TileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace CalamityMod.UI;

public class CanvasPaintingUIState
{
    public static float scrollOld = 0;
    public static float scrollNew = 0;
    public static bool moving = false;
    public static bool justClicked = false;

    public static void DrawCanvasUI(SpriteBatch spriteBatch)
    {
        CalamityPlayer p = Main.LocalPlayer.Calamity();
        // The UI only draws if the player is viewing a painting.
        if (p.CurrentlyViewedCanvasID == -1)
            return;
        if (p.CurrentlyViewedCanvasType == -1)
            return;

        // Check if this tile entity ID is actually a painting. If it's not, immediately destroy this UI.
        TECanvasPainting painting;
        bool validPainting = TileEntity.ByID.TryGetValue(p.CurrentlyViewedCanvasID, out TileEntity te);
        if (validPainting && te is TECanvasPainting cast)
            painting = cast;
        else
        {
            p.CurrentlyViewedCanvasID = -1;
            p.CurrentlyViewedCanvasType = -1;
            ResetVars();
            return;
        }

        // If the player's inventory isn't open, or they have a chest open, OR they are using a channelled item, immediately destroy this UI.
        if (!Main.playerInventory || Main.LocalPlayer.chest != -1 || Main.LocalPlayer.channel)
        {
            ClosePainting(ref p, painting);
            return;
        }

        // If the player is too far away from their viewed canvas, immediately destroy this UI and play the menu close sound.
        // Currently commented out as moving away lets player's see adjacent paintings
        /*Vector2 paintingPosition = painting.Position.ToWorldCoordinates() + new Vector2(20, 20);
        if (Main.LocalPlayer.DistanceSQ(paintingPosition) > 160f * 160f)
        {
            ClosePainting(ref p, painting);
            return;
        }*/

        int paintingTileSize = 80;

        // The zoom modifier of the painting
        float paintingFrameScale = painting.scale;
        Vector2 paintingFramePosition = painting.framePosition;

        bool hideUI = Main.keyState.PressingShift();

        Texture2D tex = TextureAssets.Tile[p.CurrentlyViewedCanvasType].Value;

        // This is the length and width of the UI box, which is a square
        float dimension = Main.screenHeight * 0.66f;

        // How large the base texture is compared to the UI box
        float sizeRatio = dimension / tex.Height;

        // The positions for both the square UI box and the painting
        Vector2 baseDrawPos = new Vector2(Main.screenWidth * 0.5f - dimension * 0.5f, Main.screenHeight * 0.5f - dimension * 0.5f);
        Vector2 posterDrawPos = baseDrawPos + Vector2.UnitX * ((dimension - tex.Width * sizeRatio) * 0.5f);

        // Draw a background square panel, then draw the actual painting
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null);
        if (!hideUI)
        {
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, baseDrawPos, new Rectangle(0, 0, (int)dimension, (int)dimension), Color.Gray, 0, new Vector2(0, 0), 1f, 0, 0);
            spriteBatch.Draw(tex, posterDrawPos, null, Color.White, 0, new Vector2(0, 0), sizeRatio, 0, 0);
        }

        // How large one pixel is on the painting
        float pixelRatio = paintingTileSize * sizeRatio;

        MouseState state = Microsoft.Xna.Framework.Input.Mouse.GetState();
        // Handle size scrolling. Scrolling is more precise at smaller values
        float scrollAmount = painting.scale >= 1 ? 1f : 0.25f;
        // If the player scrolls down, the box grows
        if (scrollOld > scrollNew)
        {
            float increasedScale = MathHelper.Clamp(painting.scale + scrollAmount, 0.25f, 10);
            painting.scale = increasedScale;
            painting.SendSyncPacket();
        }
        // If the player scrolls up, the box shrinks
        else if (scrollNew > scrollOld)
        {
            // Needs to be gate at 2 when going down
            scrollAmount = painting.scale >= 2 ? 1f : 0.25f;
            float decreasedScale = MathHelper.Clamp(painting.scale - scrollAmount, 0.25f, 10);
            painting.scale = decreasedScale;
            painting.SendSyncPacket();
        }

        // Border buffer size
        int borderSize = 4;
        // The size of the cursor box
        Vector2 cursorDimension = tex.Size() * sizeRatio * painting.scale * 0.1f;

        // The position of the cursor box. Clamped to be inside of the painting
        // Moving
        Vector2 movingCursorPos = Vector2.Clamp(Main.MouseScreen - cursorDimension * 0.5f + new Vector2(10, 10), posterDrawPos, posterDrawPos + tex.Size() * sizeRatio - cursorDimension);
        // Static
        Vector2 currentPos = Vector2.Clamp(paintingFramePosition / (float)(tex.Height / (float)dimension) + posterDrawPos, posterDrawPos, posterDrawPos + tex.Size() * sizeRatio - cursorDimension);
        // Decide whether the cursor should be following the mouse or not
        Vector2 cursorPosition = moving ? movingCursorPos : currentPos;

        bool clicked = Main.mouseLeft && Main.mouseLeftRelease;
        if (moving)
        {
            // Set the variables for the painting's frame position and zoom scale
            painting.framePosition = (cursorPosition - posterDrawPos) * (tex.Height / dimension);
            // Deactivate if the player left clicks
            if (clicked)
            {
                moving = false;
                SoundEngine.PlaySound(SoundID.MenuTick);
                painting.SendSyncPacket();
            }
        }
        else
        {
            // Activate if the player left clicks or scrolls
            if (clicked || scrollOld != scrollNew)
            {
                moving = true;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            // Allow precise pixel increments with arrow keys
            else
            {
                if (Main.keyState.IsKeyDown(Keys.Left) && Main.oldKeyState.IsKeyUp(Keys.Left))
                {
                    painting.framePosition.X = MathHelper.Clamp(painting.framePosition.X - 1, 0, tex.Width);
                    painting.SendSyncPacket();
                }
                if (Main.keyState.IsKeyDown(Keys.Right) && Main.oldKeyState.IsKeyUp(Keys.Right))
                {
                    painting.framePosition.X = MathHelper.Clamp(painting.framePosition.X + 1, 0, tex.Width);
                    painting.SendSyncPacket();
                }
                if (Main.keyState.IsKeyDown(Keys.Up) && Main.oldKeyState.IsKeyUp(Keys.Up))
                {
                    painting.framePosition.Y = MathHelper.Clamp(painting.framePosition.Y - 1, 0, tex.Height);
                    painting.SendSyncPacket();
                }
                if (Main.keyState.IsKeyDown(Keys.Down) && Main.oldKeyState.IsKeyUp(Keys.Down))
                {
                    painting.framePosition.Y = MathHelper.Clamp(painting.framePosition.Y + 1, 0, tex.Height);
                    painting.SendSyncPacket();
                }
            }
        }

        // Draw the cursor
        if (!hideUI)
        {
            // Top row
            DrawRectangle(spriteBatch, cursorPosition - new Vector2(borderSize), new Vector2(cursorDimension.X + borderSize * 2, borderSize));
            // Bottom row
            DrawRectangle(spriteBatch, cursorPosition + new Vector2(-borderSize, cursorDimension.Y), new Vector2(cursorDimension.X + borderSize * 2, borderSize));
            // Left column
            DrawRectangle(spriteBatch, cursorPosition - new Vector2(borderSize, 0), new Vector2(borderSize, cursorDimension.Y + borderSize));
            // Right column
            DrawRectangle(spriteBatch, cursorPosition + new Vector2(cursorDimension.X, 0), new Vector2(borderSize, cursorDimension.Y + borderSize));
        }

        // Draw a preview of the painting on the side
        float extraScale = 3f;
        float halfDim = paintingTileSize * sizeRatio * extraScale / 2;
        int previewSliceSize = (int)(paintingFrameScale * 0.1f * tex.Width);
        float previewScale = sizeRatio / paintingFrameScale * extraScale;
        int previewDimension = (int)(previewSliceSize * previewScale);
        Vector2 demoPosition = posterDrawPos + new Vector2(dimension + halfDim, dimension / 2 - halfDim);
        if (!hideUI)
            spriteBatch.Draw(tex, demoPosition, new Rectangle((int)paintingFramePosition.X, (int)paintingFramePosition.Y, previewSliceSize, previewSliceSize), Color.White, 0, new Vector2(0, 0), previewScale, 0, 0);

        // Block the mouse if intersecting with the painting area
        bool intersectingMain = Mouse().Intersects(new Rectangle((int)baseDrawPos.X, (int)baseDrawPos.Y, (int)dimension, (int)dimension));
        bool intersectingPrev = Mouse().Intersects(new Rectangle((int)demoPosition.X, (int)demoPosition.Y, previewDimension, previewDimension));
        if (intersectingMain || intersectingPrev)
        {
            Main.blockMouse = Main.LocalPlayer.mouseInterface = true;
        }

        // Set both scroll states to the mouse's current scroll value
        if (scrollOld == 0 && scrollNew == 0)
        {
            scrollOld = state.ScrollWheelValue;
            scrollNew = state.ScrollWheelValue;
        }
        // Update scroll values
        else
        {
            scrollOld = scrollNew;
            scrollNew = state.ScrollWheelValue;
        }
        spriteBatch.ExitShaderRegion();
    }

    public static void ClosePainting(ref CalamityPlayer clam, TECanvasPainting te)
    {
        clam.CurrentlyViewedCanvasID = -1;
        ResetVars();
        te.SendSyncPacket();
    }

    public static void ResetVars()
    {
        scrollOld = 0;
        scrollNew = 0;
        moving = false;
    }

    public static void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, Vector2 dimensions)
    {
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y), Main.DiscoColor, 0, new Vector2(0, 0), 1f, 0, 0);
    }

    private static Rectangle Mouse()
    {
        return new Rectangle((int)(Main.MouseWorld.X - Main.screenPosition.X), (int)(Main.MouseWorld.Y - Main.screenPosition.Y), 10, 10);
    }
}
