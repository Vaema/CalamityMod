using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace CalamityMod.Tiles.SunkenSea.Ambient
{
	public class RefractiveHangingCoral : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileCut[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoFail[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.IsVine[Type] = true;
            TileID.Sets.VineThreads[Type] = true;
			AddMapEntry(new Color(136, 206, 215));
			DustType = DustID.Grass;
			HitSound = SoundID.Grass;
		}
		
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = Framing.GetTileSafely(i, j + 1);
            if (tile.HasTile && tile.TileType == Type)
            {
                WorldGen.KillTile(i, j + 1);
            }
        }

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile tileAbove = Framing.GetTileSafely(i, j - 1);
			int type = -1;
			if (tileAbove.HasTile && !tileAbove.BottomSlope) 
            {
				type = tileAbove.TileType;
			}

			if (type == ModContent.TileType<Shellstone>() || type == Type) 
            {
				return true;
			}

			WorldGen.KillTile(i, j);
			return true;
		}
        public override void NearbyEffects(int i, int j, bool closer)
        {
            // Baby blue
            if (closer && Main.rand.NextBool(1200))
            {
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 274, 279, DustID.Firefly, 0.23255825f, 10f, 0, new Color(106, 154, 238), 1.5f)];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5813954f;
            }
            // Light Cyan
            if (closer && Main.rand.NextBool(1200))
            {
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 274, 279, DustID.Firefly, 0.23255825f, 10f, 0, new Color(136, 206, 215), 1.7f)];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5813954f;
            }
            //Mint Green
            if (closer && Main.rand.NextBool(1200))
            {
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 274, 279, DustID.Firefly, 0.23255825f, 10f, 0, new Color(163, 252, 195), 1.4f)];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5813954f;
            }
            //Lilac
            if (closer && Main.rand.NextBool(1200))
            {
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 274, 279, DustID.Firefly, 0.23255825f, 10f, 0, new Color(236, 194, 252), 1.6f)];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5813954f;
            }
            //Dim Lilac
            if (closer && Main.rand.NextBool(1200))
            {
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 274, 279, DustID.Firefly, 0.23255825f, 10f, 0, new Color(192, 170, 233), 1.3f)];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5813954f;
            }
        }

        public override void RandomUpdate(int i, int j)
		{
			Tile tileBelow = Framing.GetTileSafely(i, j + 1);
			if (WorldGen.genRand.NextBool(5) && !tileBelow.HasTile && tileBelow.LiquidType != LiquidID.Lava)
            {
				bool PlaceVine = false;
				int Test = j;
				while (Test > j - 10) 
                {
					Tile testTile = Framing.GetTileSafely(i, Test);
					if (testTile.BottomSlope) 
                    {
						break;
					}
					else if (!testTile.HasTile || testTile.TileType != ModContent.TileType<Shellstone>()) 
                    {
						Test--;
						continue;
					}
					PlaceVine = true;
					break;
				}
				
				if (PlaceVine) 
                {
					tileBelow.TileType = Type;
					tileBelow.HasTile = true;
					WorldGen.SquareTileFrame(i, j + 1, true);
					if (Main.dedServ) 
                    {
						NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
					}
				}
			}
		}
	}
}
