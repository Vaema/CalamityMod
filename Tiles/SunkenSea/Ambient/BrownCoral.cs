using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Microsoft.Xna.Framework;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

	public class BrownCoral1 : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(0, 2);
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 3;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(113, 80, 28));
        DustType = DustID.Ambient_DarkBrown;
        HitSound = SoundID.Dig;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.542f;
        g = 0.529f;
        b = 0.36f;
    }
}

public class BrownCoral2 : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.Width = 4;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.Origin = new Point16(1, 3);
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 3;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(113, 80, 28));
        DustType = DustID.Ambient_DarkBrown;
        HitSound = SoundID.Dig;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.61f;
        g = 0.595f;
        b = 0.405f;
    }
}
