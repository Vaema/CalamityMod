using CalamityMod.Sounds;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureWulfrum;

public class WulfrumPanels : ModTile
{
    private const short subsheetWidth = 216;
    private const short subsheetHeight = 72;
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        CalamityUtils.MergeWithGeneral(Type);

        HitSound = CommonCalamitySounds.PlatingMine;
        AddMapEntry(new Color(89, 113, 91));
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.TerraBlade, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        TileFramingSystem.CompactFraming(i, j, resetFrame);
        return false;
    }
    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        int xPos = i % 2;
        int yPos = j % 2;
        frameXOffset = xPos * subsheetWidth;
        frameYOffset = yPos * subsheetHeight;
    }
}
