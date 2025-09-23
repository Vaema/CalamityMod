
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeAstralStoneWall : ModWall
    {
        public override string Texture => "CalamityMod/Walls/AstralStoneWall";
        public override void SetStaticDefaults()
        {
            // TODO -- Change this dust to be one more befitting Astral Stone.
            DustType = DustID.Shadowflame;

            WallID.Sets.Conversion.Stone[Type] = true;

            AddMapEntry(new Color(15, 26, 31));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
