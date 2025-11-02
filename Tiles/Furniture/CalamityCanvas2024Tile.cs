using CalamityMod.TileEntities;
using CalamityMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    // TODO: Probably make a base canvas painting class
    public class CalamityCanvas2024Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileWaterDeath[Type] = false;

            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = new int[] { 18, 18, 18, 18, 18 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            //TileObjectData.newTile.Origin = new Point16(2, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TECanvasPainting>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);

            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.FramesOnKillWall[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            AddMapEntry(new Color(99, 50, 30), Language.GetText("MapObject.Painting"));
        }
        public override bool HasSmartInteract(int i, int j, Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings settings) => true;

        public override bool RightClick(int i, int j)
        {
            Main.LocalPlayer.CancelSignsAndChests();
            TECanvasPainting cube = CalamityUtils.FindTileEntity<TECanvasPainting>(i, j, 5, 5);
            if (cube != null)
            {
                CanvasPaintingUIState.ResetVars();
                Main.LocalPlayer.Calamity().CurrentlyViewedCanvasID = cube.ID;
                Main.LocalPlayer.Calamity().CurrentlyViewedCanvasType = 1;
                SoundEngine.PlaySound(SoundID.MenuOpen);
                Main.playerInventory = true;
                Main.recBigList = false;
            }
            Recipe.FindRecipes();
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Items.Placeables.Furniture.CalamityCanvas2024>();
            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.cursorItemIconEnabled = true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile t = Main.tile[i, j];
            int left = i - t.TileFrameX % (5 * 18) / 18;
            int top = j - t.TileFrameY % (5 * 18) / 18;

            TECanvasPainting canvas = CalamityUtils.FindTileEntity<TECanvasPainting>(i, j, 5, 5, 18);

            canvas?.Kill(left, top);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Main.tile[i, j];
            Texture2D texture = TextureAssets.Tile[Type].Value;
            TECanvasPainting cube = CalamityUtils.FindTileEntity<TECanvasPainting>(i, j, 1, 1);

            Vector2 pos = new Vector2(i * 16, j * 16) + CalamityUtils.TileDrawOffset;
            if (cube != null && t.TileFrameX == 0)
            {
                int fPX = (int)cube.framePosition.X;
                int fPY = (int)cube.framePosition.Y;
                int scale = (int)(texture.Width * 0.1f * cube.scale);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null);
                // TODO: make the 0.8 in scale not a hardcoded number with seemingly no meaning
                spriteBatch.Draw(texture, pos - Main.screenPosition, new Rectangle(fPX, fPY, scale, scale), Lighting.GetColor(i, j), 0, new Vector2(0, 0), 1 / cube.scale * 0.8f, 0, 0);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null);
            }
            // Calculate and draw the borders
            if (t.TileFrameX == 0 && t.TileFrameY == 0)
                DrawBorders(spriteBatch, pos - Main.screenPosition, new Point(i, j));
            return false;
        }
        public static void DrawBorders(SpriteBatch spriteBatch, Vector2 pos, Point cords)
        {
            Texture2D texture = CalamityCanvas2023Tile.border.Value;
            Texture2D cornerTex = CalamityCanvas2023Tile.corner.Value;
            ushort canvasID = (ushort)ModContent.TileType<CalamityCanvas2024Tile>();
            int commonDim = 8;
            int finalCord = 72;
            int size = 80;
            Color light = Lighting.GetColor(cords.X, cords.Y);

            // Check for nearby Canvas paintings
            // If they are cleanly lined up with this Canvas painting, don't draw borders between them
            bool drawTop = true;
            bool drawLeft = true;
            bool drawRight = true;
            bool drawBottom = true;
            // Corners
            bool drawTopLeft = false;
            bool drawTopRight = false;
            bool drawBottomLeft = false;
            bool drawBottomRight = false;

            // The bottom right tile of the canvas painting
            Point bottomRight = new Point(cords.X + 4, cords.Y + 4);
            
            // Adjacent tiles that may or may not have other canvas paintings
            Tile left = CalamityUtils.ParanoidTileRetrieval(cords.X - 1, cords.Y);
            Tile top = CalamityUtils.ParanoidTileRetrieval(cords.X, cords.Y - 1);
            Tile right = CalamityUtils.ParanoidTileRetrieval(bottomRight.X + 1, bottomRight.Y);
            Tile bottom = CalamityUtils.ParanoidTileRetrieval(bottomRight.X, bottomRight.Y + 1);

            // Check if adjacent tiles are canvas paintings that are lined up with this one
            bool validTop = ValidCanvasFrame(top, 0, size, canvasID);
            bool validRight = ValidCanvasFrame(right, 0, size, canvasID);
            bool validLeft = ValidCanvasFrame(left, finalCord, 0, canvasID);
            bool validBottom = ValidCanvasFrame(bottom, finalCord, 0, canvasID);

            if (validTop)
            {
                drawTop = false;
            }
            if (validBottom)
            {
                drawBottom = false;
            }
            if (validRight)
            {
                drawRight = false;
                // Check for junction corners
                Tile topright = CalamityUtils.ParanoidTileRetrieval(bottomRight.X + 1, cords.Y - 1);
                Tile bottomright = CalamityUtils.ParanoidTileRetrieval(bottomRight.X + 1, bottomRight.Y + 1);
                if (!drawTop && !ValidCanvasFrame(topright, 0, size, canvasID))
                {
                    drawTopRight = true;
                }
                if (!drawBottom && !ValidCanvasFrame(bottomright, 0, 0, canvasID))
                {
                    drawBottomRight = true;
                }
            }
            if (validLeft)
            {
                drawLeft = false;
                // Check for junction corners
                Tile topleft = CalamityUtils.ParanoidTileRetrieval(cords.X - 1, cords.Y - 1);
                Tile bottomleft = CalamityUtils.ParanoidTileRetrieval(cords.X - 1, bottomRight.Y + 1);
                if (!drawTop && !ValidCanvasFrame(topleft, finalCord, size, canvasID))
                {
                    drawTopLeft = true;
                }
                if (!drawBottom && !ValidCanvasFrame(bottomleft, finalCord, 0, canvasID))
                {
                    drawBottomLeft = true;
                }
            }

            // Draw the sides
            if (drawBottom)
                spriteBatch.Draw(texture, pos + Vector2.UnitY * finalCord, new Rectangle(0, texture.Height - commonDim, texture.Width, commonDim), light, 0, new Vector2(0, 0), 1, 0, 0);
            if (drawTop)
                spriteBatch.Draw(texture, pos, new Rectangle(0, 0, texture.Width, commonDim), light, 0, new Vector2(0, 0), 1, 0, 0);
            if (drawRight)
                spriteBatch.Draw(texture, pos + Vector2.UnitX * finalCord, new Rectangle(texture.Width - commonDim, commonDim, commonDim, texture.Height - (2 * commonDim)), light, 0, new Vector2(0, 0), 1, 0, 0);
            if (drawLeft)
                spriteBatch.Draw(texture, pos, new Rectangle(0, commonDim, commonDim, texture.Height - (2 * commonDim)), light, 0, new Vector2(0, 0), 1, 0, 0);

            // Draw the corners 
            // All corner drawing is done with a single corner sprite that is rotated and flipped about depending on the situation
            if (drawTop && drawLeft)
                spriteBatch.Draw(cornerTex, pos, null, light, 0, new Vector2(0, 0), 1, 0, 0);
            if (drawTop && drawRight)
                spriteBatch.Draw(cornerTex, pos + Vector2.UnitX * finalCord, null, light, 0, new Vector2(0, 0), 1, SpriteEffects.FlipHorizontally, 0);
            if (drawBottom && drawLeft)
                spriteBatch.Draw(cornerTex, pos + Vector2.UnitY * finalCord, null, light, 0, new Vector2(0, 0), 1, SpriteEffects.FlipVertically, 0);
            if (drawBottom && drawRight)
                spriteBatch.Draw(cornerTex, pos + Vector2.One * size, null, light, MathHelper.Pi, new Vector2(0, 0), 1, 0, 0);

            // Draw junction corners
            if (drawTopLeft)
                spriteBatch.Draw(cornerTex, pos + CalamityCanvas2023Tile.corner.Size(), null, light, MathHelper.Pi, new Vector2(0, 0), 1, 0, 0);
            if (drawTopRight)
                spriteBatch.Draw(cornerTex, pos + Vector2.UnitX * finalCord, null, light, 0, new Vector2(0, 0), 1, SpriteEffects.FlipVertically, 0);
            if (drawBottomLeft)
                spriteBatch.Draw(cornerTex, pos + Vector2.UnitY * finalCord, null, light, 0, new Vector2(0, 0), 1, SpriteEffects.FlipHorizontally, 0);
            if (drawBottomRight)
                spriteBatch.Draw(cornerTex, pos - CalamityCanvas2023Tile.corner.Size() + Vector2.One * size, null, light, 0, new Vector2(0, 0), 1, 0, 0);
        }

        // Check if the tile is a canvas tile that has the correct tile frames
        public static bool ValidCanvasFrame(Tile t, int frameX, int frameY, ushort canvasID)
        {
            return t.HasTile && t.TileType == canvasID && t.TileFrameX == frameX && t.TileFrameY == frameY;
        }
    }
}
