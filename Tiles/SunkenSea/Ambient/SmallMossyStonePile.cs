using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class SmallMossyStonePile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = false;
        
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.CoordinateHeights = new[] { 16 };
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(100, 112, 191));
        DustType = DustID.GreenMoss;

        base.SetStaticDefaults();
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        offsetY = 2;
    }
}
