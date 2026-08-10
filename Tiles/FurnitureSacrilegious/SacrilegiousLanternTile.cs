using CalamityMod.Items.Placeables.FurnitureSacrilegious;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureSacrilegious;

public class SacrilegiousLanternTile : ModTile
{
    public Asset<Texture2D> FlameTexture;

    public override void Load() => FlameTexture = ModContent.Request<Texture2D>(Texture + "Flame");

    public override void SetStaticDefaults() => this.SetUpLantern(ModContent.ItemType<SacrilegiousLantern>(), true);
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => CalamityUtils.DrawSwayingMultiTile(i, j);

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Iron, 0f, 0f, 1, new Color(100, 100, 100), 1f);
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
            r = 3f;
            g = 0.6f;
            b = 0.6f;
        }
        else
        {
            r = 0f;
            g = 0f;
            b = 0f;
        }
    }

    public override void GetTileFlameData(int i, int j, ref TileDrawing.TileFlameData tileFlameData)
    {
        ulong flameSeed = Main.TileFrameSeed ^ (ulong)(((long)i << 32) | (uint)j);
        tileFlameData.flameSeed = flameSeed;
        tileFlameData.flameTexture = FlameTexture.Value;
        tileFlameData.flameColor = new Color(128, 26, 26, 0);
        tileFlameData.flameCount = 3;
        tileFlameData.flameRangeXMin = -10;
        tileFlameData.flameRangeXMax = 11;
        tileFlameData.flameRangeYMin = -10;
        tileFlameData.flameRangeYMax = 11;
        tileFlameData.flameRangeMultX = 0.1f;
        tileFlameData.flameRangeMultY = 0.1f;
    }

    public override void HitWire(int i, int j)
    {
        FurnitureCommon.LightHitWire(Type, i, j, 1, 2);
    }
}
