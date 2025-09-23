using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeAstralDirtWall : ModWall
    {
        public override string Texture => "CalamityMod/Walls/AstralDirtWall";

        public override void SetStaticDefaults()
        {
            // TODO -- Change this dust to be one more befitting Astral Dirt.
            DustType = DustID.Shadowflame;
            WallID.Sets.Conversion.Dirt[Type] = true;
            AddMapEntry(new Color(26, 22, 32));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
