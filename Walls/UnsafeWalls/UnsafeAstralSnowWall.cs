
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Walls.UnsafeWalls;

public class UnsafeAstralSnowWall : ModWall
{
    public override string Texture => "CalamityMod/Walls/AstralSnowWall";
    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<Dusts.AstralBasic>();
        WallID.Sets.Conversion.Snow[Type] = true;
        AddMapEntry(new Color(135, 145, 149));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
