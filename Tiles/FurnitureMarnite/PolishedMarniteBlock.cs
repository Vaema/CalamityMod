
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureMarnite
{
    public class PolishedMarniteBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeSmoothTiles(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            CalamityUtils.MergeWithAbyss(Type);

            HitSound = SoundID.Tink;
            MineResist = 2.1f;
            AddMapEntry(new Color(49, 56, 77));
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFraming.BetterGemsparkFraming(i, j, resetFrame);
        }


        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 130, 150), 1f);
            return false;
        }
    }
}
