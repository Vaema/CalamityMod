using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Walls;

[LegacyName("AstralSnowWallSafe")]
public class AstralSnowWall : ModWall
{

    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<Dusts.AstralBasic>();
        Main.wallHouse[Type] = true;
        WallID.Sets.Conversion.Snow[Type] = true;
        AddMapEntry(new Color(135, 145, 149));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
