using CalamityMod.Items.Placeables.FurnitureExo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FurnitureExo;

public class ExoChestTile : ModTile
{
    public Asset<Texture2D> GlowTexture;

    public override void SetStaticDefaults()
    {
        this.SetUpChest(ModContent.ItemType<ExoChest>(), true, 2);
        AddMapEntry(new Color(71, 95, 114), CalamityUtils.GetItemName<ExoChest>(), FurnitureCommon.GetMapChestName);
    }

    public override bool CanExplode(int i, int j) => false;
    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.TerraBlade, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override LocalizedText DefaultContainerName(int frameX, int frameY) => CalamityUtils.GetItemName<ExoChest>();
    public override void MouseOver(int i, int j) => FurnitureCommon.ChestMouseOver<ExoChest>(i, j);
    public override void MouseOverFar(int i, int j) => FurnitureCommon.ChestMouseFar<ExoChest>(i, j);
    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);
    public override bool RightClick(int i, int j) => FurnitureCommon.ChestRightClick(i, j);

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (tile.IsTileActuallyInvisible())
            return;

        int xPos = tile.TileFrameX;
        int yPos = tile.TileFrameY;
        int chestIndex = Chest.FindChest(i - (xPos / 18), j - (yPos / 18));
        if (chestIndex == -1)
            return;

        int yOffset = TileObjectData.GetTileData(tile).DrawYOffset;

        GlowTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/FurnitureExo/ExoChestGlow");
        Texture2D glowmask = GlowTexture.Value;

        Color drawColour = Color.White;
        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y + yOffset) + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange));
        Rectangle frame = new Rectangle(xPos, yPos + Main.chest[chestIndex].frame * 38, 18, 18);
        Main.spriteBatch.Draw(glowmask, drawPosition, frame, drawColour, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }
}
