using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;



namespace CalamityMod.Tiles.SunkenSea;

public class PolypSand : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = false;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithDesert(Type);

        TileID.Sets.HasSlopeFrames[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;

        DustType = DustID.Ice_Red;
        AddMapEntry(new Color(215, 170, 170));

        Main.tileSand[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Sand"]);
        TileID.Sets.Suffocate[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.Conversion.Sand[Type] = true;
        TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
        TileID.Sets.Falling[Type] = true;
        TileID.Sets.FallingBlockProjectile[Type] = new TileID.Sets.FallingBlockProjectileInfo(ModContent.ProjectileType<PolypSandBallFalling>(), 10);

        this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
        this.RegisterBlendMergeWith(TileID.Sandstone);
        this.RegisterBlendMergeWith(TileID.Sand);
        this.RegisterBlendMergeWith(TileID.HardenedSand);
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
