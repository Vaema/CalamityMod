using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureWulfrum.FurnitureAnodizedWulfrum;

public class AnodizedWulfrumChandelier : ModTile
{
    public Asset<Texture2D> GlowTexture;

    public override void SetStaticDefaults() => this.SetUpChandelier(ModContent.ItemType<Items.Placeables.FurnitureWulfrum.FurnitureAnodizedWulfrum.AnodizedWulfrumChandelier>(), true);

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => CalamityUtils.DrawSwayingMultiTile(i, j);

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Tungsten, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (Main.tile[i, j].TileFrameX < 18)
        {
            r = 201f / 255f;
            g = 226f / 255f;
            b = 122f / 255f;
        }
        else
        {
            r = 0f;
            g = 0f;
            b = 0f;
        }
    }

    public override void HitWire(int i, int j)
    {
        FurnitureCommon.LightHitWire(Type, i, j, 3, 3);
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (Main.tile[i, j].IsTileActuallyInvisible())
            return;

        int xFrameOffset = Main.tile[i, j].TileFrameX;
        int yFrameOffset = Main.tile[i, j].TileFrameY;

        GlowTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/FurnitureWulfrum/FurnitureAnodizedWulfrum/AnodizedWulfrumChandelier_Glow");
        Texture2D glowmask = GlowTexture.Value;

        Vector2 drawOffest = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y - 2) + drawOffest;
        Color drawColour = Color.White;
        Tile trackTile = Main.tile[i, j];
        if (!trackTile.IsHalfBlock && trackTile.Slope == 0)
            spriteBatch.Draw(glowmask, drawPosition, new Rectangle(xFrameOffset, yFrameOffset, 18, 18), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
        else if (trackTile.IsHalfBlock)
            spriteBatch.Draw(glowmask, drawPosition + new Vector2(0f, 8f), new Rectangle(xFrameOffset, yFrameOffset, 18, 8), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
    }
}
