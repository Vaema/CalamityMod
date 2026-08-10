using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class LargeMossyStonePile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);

        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.AnchorBottom = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidTile, 3, 0);
        TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(1, 1);

        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.addTile(Type);
        AddMapEntry(new Color(100, 112, 191));
        DustType = DustID.GreenMoss;
        HitSound = SoundID.Dig;
    }
}
