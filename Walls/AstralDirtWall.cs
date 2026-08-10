using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls;

[LegacyName("AstralDirtWallSafe")]
public class AstralDirtWall : ModWall
{

    public override void SetStaticDefaults()
    {
        // TODO -- Change this dust to be one more befitting Astral Dirt.
        DustType = DustID.Shadowflame;
        Main.wallHouse[Type] = true;
        WallID.Sets.Conversion.Dirt[Type] = true;
        AddMapEntry(new Color(26, 22, 32));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
