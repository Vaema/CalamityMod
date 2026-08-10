using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class BarrelCactus : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = false;
        Main.tileNoAttach[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.RandomStyleRange = 2;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(85, 143, 46));
        DustType = DustID.Coralstone;

        base.SetStaticDefaults();
    }
    //Bro idk how to make a boulder 
}
