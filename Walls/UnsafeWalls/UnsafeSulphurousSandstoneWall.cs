using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeSulphurousSandstoneWall : ModWall
    {
        public override string Texture => "CalamityMod/Walls/SulphurousSandstoneWall";
        public override void SetStaticDefaults()
        {
            DustType = DustID.Sand;
            AddMapEntry(new Color(57, 45, 38));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
