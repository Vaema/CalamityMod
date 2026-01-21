using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Walls.DraedonStructures
{
    public class LaboratoryPlatingWall : ModWall
    {

        public override void SetStaticDefaults()
        {
            DustType = DustID.Web;
            Main.wallHouse[Type] = true;

            AddMapEntry(new Color(105, 102, 98));
        }

        public override bool CanExplode(int i, int j) => false;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
