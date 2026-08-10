using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.FurnitureMonolith;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureMonolith;

public class MonolithDoorOpen : ModTile
{
    public Asset<Texture2D> GlowTexture;

    public override void SetStaticDefaults()
    {
        this.SetUpDoorOpen(ModContent.ItemType<MonolithDoor>(), true);
        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<MonolithDoorClosed>();
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<AstralBasic>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (Main.tile[i, j].IsTileActuallyInvisible())
            return;

        int xPos = Main.tile[i, j].TileFrameX;
        int yPos = Main.tile[i, j].TileFrameY;
        GlowTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/FurnitureMonolith/MonolithDoorOpenGlow");
        Color drawColour = CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, new Color(100, 100, 100, 100));
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
        Main.spriteBatch.Draw(GlowTexture.Value, drawOffset, new Rectangle?(new Rectangle(xPos, yPos, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<MonolithDoor>();
    }
}
