using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea;

public class MossyStone : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithDesert(Type);

        TileID.Sets.ChecksForMerge[Type] = true;
        HitSound = SoundID.Tink;
        DustType = DustID.GreenMoss;
        AddMapEntry(new Color(124, 126, 127));

        //Sand merges
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
}
