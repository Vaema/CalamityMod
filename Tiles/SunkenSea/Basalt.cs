using System;
using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Basalt : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.GeneralPlacementTiles[Type] = false;
            TileID.Sets.HasSlopeFrames[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileShine2[Type] = false;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);

            DustType = DustID.Ash;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(77, 75, 86));

            MinPick = 110;

            //Stone merges
            this.RegisterUniversalMerge(ModContent.TileType<Shellstone>(), "CalamityMod/Tiles/Merges/ShellstoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Runestone>(), "CalamityMod/Tiles/Merges/RunestoneMerge");
            //Sand merges
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<VolcanicSand>(), "CalamityMod/Tiles/Merges/VolcanicSandMerge");
            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
            //Normal merges
            this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            this.RegisterUniversalMerge(TileID.Ash, "CalamityMod/Tiles/Merges/AshMerge");
            this.RegisterUniversalMerge(TileID.Mud, "CalamityMod/Tiles/Merges/MudMerge");
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 1, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
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
