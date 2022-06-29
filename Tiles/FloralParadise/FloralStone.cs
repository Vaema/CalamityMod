using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FloralParadise
{
    public class FloralStone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;
            TileID.Sets.Stone[Type] = true;
            TileID.Sets.Conversion.Stone[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithOres(Type);
            CalamityUtils.MergeWithFloralParadise(Type);

            DustType = 78;
            // drop = ModContent.ItemType<FloralStoneItem>();

            HitSound = SoundID.Tink;

            AddMapEntry(new Color(33, 56, 27));
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.CustomMergeFrame(i, j, Type, TileID.Dirt);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
