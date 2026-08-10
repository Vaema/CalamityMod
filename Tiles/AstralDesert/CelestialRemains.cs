using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.AstralDesert;

[LegacyName("AstralFossil")]
public class CelestialRemains : ModTile
{

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithDesert(Type);
        CalamityUtils.MergeAstralTiles(Type);

        DustType = ModContent.DustType<AstralBasic>();

        AddMapEntry(new Color(59, 50, 77), CalamityUtils.GetItemName<Items.Placeables.Astral.CelestialRemains>());

        TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;

        this.RegisterBlendMergeWith(ModContent.TileType<AstralSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<AstralSandstone>());
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
