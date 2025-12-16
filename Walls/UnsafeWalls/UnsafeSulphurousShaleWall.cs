using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeSulphurousShaleWall : ModWall
    {
        public override string Texture => "CalamityMod/Walls/SulphurousShaleWall";
        public override void SetStaticDefaults()
        {
            DustType = DustID.Sand;
            AddMapEntry(new Color(59, 40, 63));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
