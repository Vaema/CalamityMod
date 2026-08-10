using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea;

public class LimestoneCobble : ModTile
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
        AddMapEntry(new Color(200, 120, 101));

        //Stone merges
        this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
        this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
        this.RegisterBlendMergeWith(ModContent.TileType<Runestone>());

        //Sand merges
        this.RegisterBlendMergeWith(ModContent.TileType<PolypSand>());
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

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
    }
}
