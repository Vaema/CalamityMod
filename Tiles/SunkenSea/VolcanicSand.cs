using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class VolcanicSand : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.t_PearlWood;
            AddMapEntry(new Color(102, 101, 106));

            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
        public override void RandomUpdate(int i, int j)
        {
            Tile tUP = Main.tile[i, j - 1];
            Tile tDOWN = Main.tile[i, j + 1];
            Tile tLEFT = Main.tile[i - 1, j];
            Tile tRIGHT = Main.tile[i + 1, j];
            Tile tUPLEFT = Main.tile[i - 1, j - 1];
            Tile tDOWNLEFT = Main.tile[i - 1, j + 1];
            Tile tUPRIGHT = Main.tile[i + 1, j - 1];
            Tile tDOWNRIGHT = Main.tile[i + 1, j + 1];
            if (tUP.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tUP.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tDOWN.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tDOWN.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tLEFT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tLEFT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tRIGHT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tRIGHT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }

            if (tUPLEFT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tUPLEFT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tDOWNLEFT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tDOWNLEFT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tUPRIGHT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tUPRIGHT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            if (tDOWNRIGHT.LiquidType == LiquidID.Water && j < Main.maxTilesY - 205)
            {
                tDOWNRIGHT.LiquidAmount = 0;
                WorldGen.SquareTileFrame(i, j);
            }
            Dust dust;
            dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 16, DustID.Smoke, 0f, -1.9069767f, 195, new Color(255, 255, 255), 1f)];
            dust.noGravity = false;
            dust.fadeIn = 1.4209302f;
        }
    }
}
