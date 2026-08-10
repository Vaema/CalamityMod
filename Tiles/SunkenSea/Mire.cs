using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea;

public class Mire : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithDesert(Type);
        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        Main.tileShine2[Type] = false;

        DustType = DustID.Hive;
        AddMapEntry(new Color(71, 38, 30));

        //Stone merges
        this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
        this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
        this.RegisterBlendMergeWith(ModContent.TileType<Runestone>());

        //Sand merges
        this.RegisterBlendMergeWith(ModContent.TileType<PolypSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<ScarletSeaGrassTile>());
        this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<VolcanicSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<AridSoil>());
        this.RegisterBlendMergeWith(ModContent.TileType<Dunesand>());
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
}
