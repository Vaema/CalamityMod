using System;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class MagentaCoral : ModTile
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
            DustType = DustID.Ice_Pink;
            AddMapEntry(new Color(206, 56, 123));
            Main.tileShine2[Type] = true;

            TileID.Sets.CanBeDugByShovel[Type] = true;

            this.RegisterUniversalMerge(ModContent.TileType<Shellstone>(), "CalamityMod/Tiles/Merges/ShellstoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (!Main.tile[i - 1, j].HasTile || !Main.tile[i + 1, j].HasTile || !Main.tile[i, j - 1].HasTile || !Main.tile[i, j + 1].HasTile)
            {
                float brightness = 0.9f;
                Color Color1 = new Color(242, 96, 161);
                Color Color2 = new Color(156, 51, 83);
                Color value = Color.Lerp(Color1, Color2, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.017f + i / 40f) + 1f) / 2f);
                Color value1 = Color.Lerp(Color1, Color2, (MathF.Sin((j - 100) / 50f + Main.GameUpdateCount * 0.004f + -i / 30f) + 1f) / 2f);
                r = (value.R + value1.R) / 300f;
                g = (value.G + value1.G) / 300f;
                b = (value.B + value1.B) / 300f;
                r *= brightness;
                g *= brightness;
                b *= brightness;
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
