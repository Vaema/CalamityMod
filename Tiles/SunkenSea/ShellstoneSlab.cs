using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Systems;

namespace CalamityMod.Tiles.SunkenSea
{
    public class ShellstoneSlab : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            TileID.Sets.HasSlopeFrames[Type] = true;

            HitSound = SoundID.Tink;
            DustType = 17;
            AddMapEntry(new Color(156, 191, 199));
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
