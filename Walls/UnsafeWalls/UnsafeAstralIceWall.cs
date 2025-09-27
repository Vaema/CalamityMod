
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeAstralIceWall : ModWall
    {
        public override string Texture => "CalamityMod/Walls/AstralIceWall";
        public override void SetStaticDefaults()
        {
            // TODO -- Change this dust to be one more befitting Astral Ice.
            DustType = DustID.Shadowflame;
            WallID.Sets.Conversion.Ice[Type] = true;
            AddMapEntry(new Color(83, 76, 92));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
