using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeSulphurousSandWall : ModWall
    {
        public override string Texture => "CalamityMod/Walls/SulphurousSandWall";
        public override void SetStaticDefaults()
        {
            DustType = DustID.Sand;
            AddMapEntry(new Color(84, 71, 46));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
