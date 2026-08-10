using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.FurnitureExo;

public class ExoPlatingTile : GlowMaskTile
{
    public override void SetupStatic()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeDecorativeTiles(Type);

        MineResist = 3f;
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(52, 67, 78));
        AnimationFrameHeight = 90;
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.TerraBlade, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        int yPos = j % 2;
        frameYOffset = yPos * AnimationFrameHeight;
    }

    public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
    {
        return Color.White;
    }
}
