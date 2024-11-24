using CalamityMod.TileEntities;
using CalamityMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class CalamityCanvasTile : ModTile
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
                SoundEngine.PlaySound(SoundID.MenuOpen);
                Main.playerInventory = true;
                Main.recBigList = false;
            }
            Recipe.FindRecipes();
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Items.Placeables.Furniture.CalamityCanvas>();
            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.cursorItemIconEnabled = true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile t = Main.tile[i, j];
            int left = i - t.TileFrameX % (5 * 18) / 18;
            int top = j - t.TileFrameY % (5 * 18) / 18;

            TECanvasPainting factory = CalamityUtils.FindTileEntity<TECanvasPainting>(i, j, 5, 5, 18);

            factory?.Kill(left, top);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Main.tile[i, j];
            Texture2D texture = TextureAssets.Tile[Type].Value;
            TECanvasPainting cube = CalamityUtils.FindTileEntity<TECanvasPainting>(i, j, 1, 1);
            float baseDimension = 80; // 5 * 16
            if (cube != null && t.TileFrameX == 0)
            {
                Vector2 pos = new Vector2(i * 16, j * 16) + CalamityUtils.TileDrawOffset;
                int fPX = (int)cube.framePosition.X;
                int fPY = (int)cube.framePosition.Y;
                int scale = (int)(baseDimension * cube.scale);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(texture, pos - Main.screenPosition, new Rectangle(fPX, fPY, scale, scale), Lighting.GetColor(i, j), 0, new Vector2(0, 0), 1 / cube.scale, 0, 0);
                spriteBatch.ExitShaderRegion();
            }
            return false;
        }
    }
}
