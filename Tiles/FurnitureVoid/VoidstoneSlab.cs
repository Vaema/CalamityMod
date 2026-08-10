using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.FurnitureVoid;

public class VoidstoneSlab : GlowMaskTile
{
    public override void SetupStatic()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        HitSound = SoundID.Tink;
        MineResist = 7f;
        MinPick = 180;
        AddMapEntry(new Color(27, 24, 31));
        AnimationFrameHeight = 270;
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        frameYOffset = AnimationFrameHeight * TileFramingSystem.GetVariation3x3_01234_Low3(i, j);
    }

    public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
    {
        return new Color(75, 75, 75);
    }
}
