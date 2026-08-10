using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Abyss.AbyssAmbient;

public class TenebrisRemnant : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(46, 103, 96));
        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        base.SetStaticDefaults();
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile tileBelow = Framing.GetTileSafely(i, j + 1);
			int type = -1;

			if (tileBelow.HasTile)
        {
				type = tileBelow.TileType;
			}

			if (type == ModContent.TileType<Voidstone>())
        {
				return true;
			}

			WorldGen.KillTile(i, j);

			return true;
		}

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        offsetY = -30;
        height = 48;
    }
}
