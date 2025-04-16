using CalamityMod.Systems;
using CalamityMod.Tiles.Merges;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class Limestone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = SoundID.Tink;
            DustType = DustID.Pot;
            AddMapEntry(new Color(154, 119, 119));

            this.RegisterUniversalMerge(ModContent.TileType<Shellstone>(), "CalamityMod/Tiles/Merges/ShellstoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<ScarletSeaGrassTile>(), "CalamityMod/Tiles/Merges/ScarletSeaGrassTile");
            this.RegisterUniversalMerge(ModContent.TileType<PolypSand>(), "CalamityMod/Tiles/Merges/PolypSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
