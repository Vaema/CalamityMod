using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CalamityMod.Tiles.SunkenSea
{
    public class CyanCoral : ModTile
    {
        public byte[,] tileAdjacency;
        public byte[,] secondTileAdjacency;
        public byte[,] thirdTileAdjacency;
        public byte[,] fourthTileAdjacency;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            Main.tileLighted[Type] = true;

            TileID.Sets.HasSlopeFrames[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = SoundID.Dig;
            DustType = 119;
            AddMapEntry(new Color(92, 168, 198));
            Main.tileShine2[Type] = true;

            TileID.Sets.CanBeDugByShovel[Type] = true;

            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<RuneSand>(), out tileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<Shellstone>(), out secondTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<EutrophicSand>(), out thirdTileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<Navystone>(), out fourthTileAdjacency);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //TileFraming.DrawUniversalMergeFrames(i, j, tileAdjacency, "CalamityMod/Tiles/Merges/TimelessSandMerge");
            //TileFraming.DrawUniversalMergeFrames(i, j, secondTileAdjacency, "CalamityMod/Tiles/Merges/ShellstoneMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, thirdTileAdjacency, "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            TileFraming.DrawUniversalMergeFrames(i, j, fourthTileAdjacency, "CalamityMod/Tiles/Merges/NavystoneMerge");
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (!Main.tile[i - 1, j].HasTile || !Main.tile[i + 1, j].HasTile || !Main.tile[i, j - 1].HasTile || !Main.tile[i, j + 1].HasTile)
            {
                float brightness = 0.9f;
                Color Color1 = new Color(130, 213, 247);
                Color Color2 = new Color(26, 68, 85);
                Color value = Color.Lerp(Color1, Color2, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.017f + i / 40f) + 1f) / 2f);
                Color value1 = Color.Lerp(Color1, Color2, (MathF.Sin((j - 100) / 50f + Main.GameUpdateCount * 0.004f + -i / 30f) + 1f) / 2f);
                r = (value.R + value1.R) / 400f;
                g = (value.G + value1.G) / 400f;
                b = (value.B + value1.B) / 400f;
                r *= brightness;
                g *= brightness;
                b *= brightness;
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<RuneSand>(), out tileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<Shellstone>(), out secondTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<EutrophicSand>(), out thirdTileAdjacency[i, j]);
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<Navystone>(), out fourthTileAdjacency[i, j]);
            return TileFraming.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
