using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls;

[LegacyName("SulphurousSandstoneWallSafe")]
public class SafeSulphurousSandstoneWall : ModWall
{
    public override string Texture => "CalamityMod/Walls/SulphurousSandstoneWall";
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;
        DustType = DustID.Sand;
        AddMapEntry(new Color(57, 45, 38));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
