using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class LargeDriftwoodWreckage : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);

        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.Width = 5;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;        

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
        TileObjectData.newTile.Origin = new Point16(1, 3);

        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.addTile(Type);
        AddMapEntry(new Color(146, 123, 127));
        DustType = DustID.Shadewood_Tree;
        HitSound = SoundID.Dig;
    }
}
