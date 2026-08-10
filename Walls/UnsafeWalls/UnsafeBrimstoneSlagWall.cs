using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls.UnsafeWalls;

[LegacyName("BrimstoneSlagWallUnsafe")]
public class UnsafeBrimstoneSlagWall : MultiVariantModWall
{
    public override string Texture => "CalamityMod/Walls/BrimstoneSlagWall";
    public override void SetStaticDefaults()
    {
        AddMapEntry(new Color(24, 16, 29));
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        frameXOffset = (i % 2) * 468;
        frameYOffset = (j % 2) * 180;
    }
}
