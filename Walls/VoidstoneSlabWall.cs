using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Walls;

public class VoidstoneSlabWall : MultiVariantModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        AddMapEntry(new Color(19, 17, 22));
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void PopulateWallVariant(int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        int xPos = i % 3;
        int yPos = j % 3;
        int rel = (xPos + yPos * 3) % 5;
        frameXOffset = 0;
        frameYOffset = rel * 180;
    }
}
