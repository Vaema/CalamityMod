using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class WhitePearlPile : ModTile
    {
        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/VoidstoneMine", 3) { Volume = 0.4f };
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;

            Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = true;

            HitSound = MineSound;
            DustType = DustID.Slush;
            AddMapEntry(new Color(217, 209, 213));

            TileID.Sets.CanBeDugByShovel[Type] = true;

            this.RegisterUniversalMerge(ModContent.TileType<Shellstone>(), "CalamityMod/Tiles/Merges/ShellstoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<AbyssGravel>(), "CalamityMod/Tiles/Merges/AbyssGravelMerge");
            this.RegisterUniversalMerge(ModContent.TileType<EutrophicSand>(), "CalamityMod/Tiles/Merges/EutrophicSandMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Navystone>(), "CalamityMod/Tiles/Merges/NavystoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<Runestone>(), "CalamityMod/Tiles/Merges/RunestoneMerge");
            this.RegisterUniversalMerge(TileID.Sandstone, "CalamityMod/Tiles/Merges/SandstoneMerge");
            this.RegisterUniversalMerge(TileID.Sand, "CalamityMod/Tiles/Merges/SandMerge");
            this.RegisterUniversalMerge(TileID.HardenedSand, "CalamityMod/Tiles/Merges/HardenedSandMerge");
            this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            this.RegisterUniversalMerge(TileID.Ash, "CalamityMod/Tiles/Merges/AshMerge");
            this.RegisterUniversalMerge(TileID.Mud, "CalamityMod/Tiles/Merges/MudMerge");
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
