using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureDriftwood
{
    public class Driftwood : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Wood"]);

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            CalamityUtils.MergeWithDesert(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.HasSlopeFrames[Type] = true;
            HitSound = SoundID.Dig;
            DustType = DustID.Shadewood_Tree;
            AddMapEntry(new Color(136, 129, 154));

            // 02JUN2024: Ozzatron: Dunesand has no merge
            //TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<Dunesand>(), out tileAdjacency);
            // 02JUN2024: Ozzatron: Shellstone has no merge tile sheet defined
            // TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<Shellstone>(), out secondTileAdjacency);

            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
