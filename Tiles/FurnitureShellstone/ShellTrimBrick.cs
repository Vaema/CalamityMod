using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Systems;
using CalamityMod.Tiles.SunkenSea;

namespace CalamityMod.Tiles.FurnitureShellstone
{
    public class ShellTrimBrick : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            TileID.Sets.HasSlopeFrames[Type] = true;

            HitSound = SoundID.Tink;
            DustType = DustID.CorruptPlants;
            AddMapEntry(new Color(156, 191, 199));

            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Runestone>(), "CalamityMod/Tiles/Merges/RunestoneMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
