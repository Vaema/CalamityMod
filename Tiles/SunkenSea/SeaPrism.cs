using System;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SeaPrism : ModTile
    {
        private const short subsheetWidth = 450;
        private const short subsheetHeight = 198; 
        
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithDesert(Type);
            Main.tileLighted[Type] = true;
            Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            DustType = 33;
            AddMapEntry(new Color(97, 212, 223));
            HitSound = SoundID.Tink;
            Main.tileSpelunker[Type] = true;
            MinPick = 55;

            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            // (i & 0b0001) = (i % 2)
            frameXOffset = (i & 0b0001) * subsheetWidth;
            frameYOffset = (j & 0b0001) * subsheetHeight;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.9f;
            Color blue = new Color(67, 187, 204);
            Color darkviolet = new Color(18, 67, 116);
            Color value = Color.Lerp(blue, darkviolet, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.017f + i / 40f) + 1f) / 2f);
            Color value1 = Color.Lerp(blue, darkviolet, (MathF.Sin((j - 100) / 50f + Main.GameUpdateCount * 0.004f + -i / 30f) + 1f) / 2f);
            r = (value.R + value1.R) / 900f;
            g = (value.G + value1.G) / 900f;
            b = (value.B + value1.B) / 900f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BrimstoneFraming(i, j, resetFrame);
        }
    }
}
