using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class SmallCorals : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = false;
            Main.tileCut[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.ReplaceTileBreakUp[Type] = true;
            TileID.Sets.SwaysInWindBasic[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(178, 28, 153));
            DustType = DustID.Coralstone;
            HitSound = SoundID.Grass;

            base.SetStaticDefaults();
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile tileBelow = Framing.GetTileSafely(i, j + 1);
			int type = -1;

			if (tileBelow.HasTile)
				type = tileBelow.TileType;

			if (type == ModContent.TileType<EutrophicSand>())
				return true;

			WorldGen.KillTile(i, j);

			return true;
		}

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 0;
            height = 16;
        }
    }
}
