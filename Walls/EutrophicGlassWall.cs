using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Walls;

public class EutrophicGlassWall : MultiVariantModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;
        Main.BackgroundEnabled = true;
        Main.wallLight[Type] = true;

        AddMapEntry(new Color(23, 39, 48));
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Chlorophyte, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
    {

        frameYOffset = (j % 10) * 180;
        frameXOffset = (i % 10) * 468;
    }
}
