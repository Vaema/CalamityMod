using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class FryCoral : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.DrawYOffset = 3;

        TileObjectData.addTile(Type);
        AddMapEntry(new Color(79, 196, 149));
        DustType = DustID.BlueMoss;
        HitSound = SoundID.Dig;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.588f;
        g = 0.365f;
        b = 0.365f;
    }
}

public class FryCoral2 : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.DrawYOffset = 3;

        TileObjectData.addTile(Type);
        AddMapEntry(new Color(79, 196, 149));
        DustType = DustID.BlueMoss;
        HitSound = SoundID.Dig;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.588f;
        g = 0.365f;
        b = 0.365f;
    }
}
public class FryCoral3 : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
        TileObjectData.newTile.Width = 4;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.DrawYOffset = 3;

        TileObjectData.addTile(Type);
        AddMapEntry(new Color(79, 196, 149));
        DustType = DustID.BlueMoss;
        HitSound = SoundID.Dig;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.588f;
        g = 0.365f;
        b = 0.365f;
    }
}
