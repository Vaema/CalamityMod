using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles;

public class CryonicBrick : ModTile
{
    int subsheetHeight = 90;
    int subsheetWidth = 234;


    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeDecorativeTiles(Type);

        HitSound = SoundID.Tink;
        AddMapEntry(new Color(99, 131, 199));

        this.RegisterBlendMergeWith(TileID.Dirt);
        this.RegisterBlendMergeWith(TileID.SnowBlock);
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.BubbleBurst_Blue, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        int xPos = i % 2;
        int yPos = j % 4;
        frameXOffset = xPos * subsheetWidth;
        frameYOffset = yPos * subsheetHeight;
    }
}
