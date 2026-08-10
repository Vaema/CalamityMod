using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class SmallCorals : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        AddMapEntry(new Color(178, 28, 153));
        DustType = DustID.Coralstone;
        HitSound = SoundID.Grass;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool CanPlace(int i, int j)
    {
        Tile belowTile = Main.tile[i, j + 1];
        Tile aboveTile = Main.tile[i, j - 1];
        Tile rightTile = Main.tile[i + 1, j];
        Tile leftTile = Main.tile[i - 1, j];

        if ((belowTile.Slope == SlopeType.Solid && !belowTile.IsHalfBlock && belowTile.HasTile && belowTile.IsTileSolid()) ||
            (aboveTile.Slope == SlopeType.Solid && !aboveTile.IsHalfBlock && aboveTile.HasTile && aboveTile.IsTileSolid()) ||
            (rightTile.Slope == SlopeType.Solid && !rightTile.IsHalfBlock && rightTile.HasTile && rightTile.IsTileSolid()) ||
            (leftTile.Slope == SlopeType.Solid && !leftTile.IsHalfBlock && leftTile.HasTile && leftTile.IsTileSolid()))
            return true;

        return false;
    }
    public override void PlaceInWorld(int i, int j, Item item)
    {
        Tile belowTile = Main.tile[i, j + 1];
        Tile aboveTile = Main.tile[i, j - 1];
        Tile rightTile = Main.tile[i + 1, j];
        Tile leftTile = Main.tile[i - 1, j];

        if (belowTile.Slope == SlopeType.Solid && !belowTile.IsHalfBlock && belowTile.HasTile && belowTile.IsTileSolid())
            Main.tile[i, j].TileFrameY = 0;
        else if (aboveTile.Slope == SlopeType.Solid && !aboveTile.IsHalfBlock && aboveTile.HasTile && aboveTile.IsTileSolid())
            Main.tile[i, j].TileFrameY = 18;
        else if (rightTile.Slope == SlopeType.Solid && !rightTile.IsHalfBlock && rightTile.HasTile && rightTile.IsTileSolid())
            Main.tile[i, j].TileFrameY = 36;
        else if (leftTile.Slope == SlopeType.Solid && !leftTile.IsHalfBlock && leftTile.HasTile && leftTile.IsTileSolid())
            Main.tile[i, j].TileFrameY = 54;

        Main.tile[i, j].TileFrameX = (short)(WorldGen.genRand.Next(18) * 18);
    }
}
