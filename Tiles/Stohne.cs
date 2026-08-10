using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles;

public class Stohne : ModTile
{

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(117, 42, 14));
        DustType = DustID.Lihzahrd;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithOres(Type);

        TileID.Sets.Stone[Type] = true;
        TileID.Sets.Conversion.Stone[Type] = true;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
        TileID.Sets.HasSlopeFrames[Type] = true;

        this.RegisterBlendMergeWith(TileID.Dirt);
        this.RegisterBlendMergeWith(TileID.Stone);
        this.RegisterBlendMergeWith(TileID.Mud);
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
    }
}
