using CalamityMod.ExtraTextures.GreyscaleGradients;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.Plates;

public class Onyxplate : GlowMaskTile
{
    public override void SetupStatic()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);

        HitSound = CommonCalamitySounds.PlatingMine;
        MineResist = 1f;
        DustType = DustID.ShadowbeamStaff;
        AddMapEntry(new Color(182, 28, 232));
    }

    public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
    {
        float brightness = GreyscaleGradient.OnyxplatePulse.GetRepeat((int)Main.GameUpdateCount);
        brightness = 0.04f + (brightness * 0.31f);
        return Color.White * brightness;
    }
}
