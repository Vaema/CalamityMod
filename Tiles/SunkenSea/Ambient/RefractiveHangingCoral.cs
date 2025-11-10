using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class RefractiveHangingCoral : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoFail[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileNoSunLight[Type] = false;
            TileID.Sets.IsVine[Type] = true;
            TileID.Sets.VineThreads[Type] = true;
            AddMapEntry(new Color(76, 133, 191));
            DustType = DustID.Grass;
            HitSound = SoundID.Grass;
        }
        
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = Framing.GetTileSafely(i, j + 1);
            if (tile.HasTile && tile.TileType == Type)
                WorldGen.KillTile(i, j + 1);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileAbove = Framing.GetTileSafely(i, j - 1);
            int type = -1;
            if (tileAbove.HasTile && !tileAbove.BottomSlope) 
                type = tileAbove.TileType;

            if (type == ModContent.TileType<Shellstone>() || type == Type) 
                return true;

            WorldGen.KillTile(i, j);
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer && Main.rand.NextBool(300))
            {
                // this comment will exist until The Great Dustpan is merged:
                // vanilla's Firefly dust (304) is completely yellow.
                // therefore, it is completely unable to be turned blue due to how draw colour works!
                // so it will always appear grey when you try to draw blue, and red/green/yellow otherwise.
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 280, 280, DustID.Firefly, 0.2f, 0f, 0, Color.Lerp(new Color(0, 76, 255), new Color(76, 0, 255), Main.rand.NextFloat()), Main.rand.NextFloat(1f, 2f))];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5f;
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Quite possibly some of the laggiest calculations I've ever seen
            float brightness = 0.9f;
            brightness *= (float)MathF.Sin(-j / 40f + Main.GameUpdateCount * 0.01f + i);
            Color lilac = new Color(126, 94, 252);
            Color mint = new Color(96, 252, 186);
            Color value = Color.Lerp(lilac, mint, (MathF.Sin(j / 30f + Main.GameUpdateCount * 0.017f + -i / 40f) + 1f) / 2f);
            Color value1 = Color.Lerp(lilac, mint, (MathF.Sin((-j - 100) / 40f + Main.GameUpdateCount * 0.014f + i / 20f) + 1f) / 2f);
            r = (value.R + value1.R) / 450f;
            g = (value.G + value1.G) / 450f;
            b = (value.B + value1.B) / 450f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
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
                        NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
                }
            }
        }
    }
}
