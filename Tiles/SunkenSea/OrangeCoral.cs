using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea;

public class OrangeCoral : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        Main.tileLighted[Type] = true;

        TileID.Sets.HasSlopeFrames[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;
        HitSound = SoundID.Dig;
        DustType = DustID.Pixie;
        AddMapEntry(new Color(255, 144, 63));
        Main.tileShine2[Type] = true;

        TileID.Sets.CanBeDugByShovel[Type] = true;

        this.RegisterBlendMergeWith(ModContent.TileType<Shellstone>());
        this.RegisterBlendMergeWith(ModContent.TileType<EutrophicSand>());
        this.RegisterBlendMergeWith(ModContent.TileType<Navystone>());
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (Main.tile[i, j].Get<TileSpecialDrawData>().Flag0)
        {
            r = 0.92f;
            g = 0.62f;
            b = 0.42f;
        }
    }

    public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight)
    {
        Main.tile[i, j].Get<TileSpecialDrawData>().Flag0 = !Main.tile[i - 1, j].HasTile || !Main.tile[i + 1, j].HasTile || !Main.tile[i, j - 1].HasTile || !Main.tile[i, j + 1].HasTile;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
    }
}
