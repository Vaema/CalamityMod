using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles;

[LegacyName("ChaoticBrick")]
public class ScoriaBrick : GlowMaskTile
{
    int subsheetHeight = 72;

    public override void SetupStatic()
    {
        Main.tileLighted[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Water_BloodMoon;
        AddMapEntry(new Color(85, 87, 101));
        HitSound = SoundID.Tink;

        this.RegisterBlendMergeWith(TileID.Dirt);
        this.RegisterBlendMergeWith(TileID.Stone);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.04f;
        g = 0.00f;
        b = 0.00f;
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        int yPos = j % 2;
        frameYOffset = yPos * subsheetHeight;
    }

    public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
    {
        return Color.White;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        TileFramingSystem.CompactFraming(i, j, resetFrame);
        return false;
    }
}
