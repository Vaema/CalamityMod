using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Astral;

public class AstralStone : ModTile
{

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeAstralTiles(Type);
        CalamityUtils.MergeWithOres(Type);

        DustType = ModContent.DustType<AstralBasic>();

        HitSound = SoundID.Tink;

        AddMapEntry(new Color(93, 78, 107));

        TileID.Sets.Stone[Type] = true;
        TileID.Sets.Conversion.Stone[Type] = true;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

        this.RegisterBlendMergeWith(TileID.Dirt);
        this.RegisterBlendMergeWith(TileID.Stone);
        this.RegisterBlendMergeWith(ModContent.TileType<AstralDirt>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor)
    {
        sightColor = Color.Cyan;
        return true;
    }
}
