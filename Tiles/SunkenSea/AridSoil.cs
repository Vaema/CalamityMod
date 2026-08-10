using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea;

public class AridSoil : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithDesert(Type);

        TileID.Sets.HasSlopeFrames[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        Main.tileShine2[Type] = false;

        DustType = DustID.Hive;
        AddMapEntry(new Color(203, 123, 107));

        //Sand merges
        this.RegisterBlendMergeWith(ModContent.TileType<PolypSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<Dunesand>());
        this.RegisterBlendMergeWith(ModContent.TileType<ScarletSeaGrassTile>());
        this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<VolcanicSand>());
        this.RegisterBlendMergeWith(TileID.Sandstone);
        this.RegisterBlendMergeWith(TileID.Sand);
        this.RegisterBlendMergeWith(TileID.HardenedSand);

        //Normal merges
        this.RegisterBlendMergeWith(TileID.Stone);
        this.RegisterBlendMergeWith(TileID.Dirt);
        this.RegisterBlendMergeWith(TileID.Ash);
        this.RegisterBlendMergeWith(TileID.Mud);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
    }
}
