using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureDriftwood;

public class DriftwoodCandle : ModTile
{
    public override void SetStaticDefaults() => this.SetUpCandle(ModContent.ItemType<Items.Placeables.FurnitureDriftwood.DriftwoodCandle>(), true);

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.LifeDrain, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (Main.tile[i, j].TileFrameX < 18)
        {
            r = 89f / 155f;
            g = 98f / 155f;
            b = 106f / 155f;
        }
        else
        {
            r = 0f;
            g = 0f;
            b = 0f;
        }
    }

    public override void HitWire(int i, int j)
    {
        FurnitureCommon.LightHitWire(Type, 1, j, 1, 1);
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Placeables.FurnitureDriftwood.DriftwoodCandle>();
    }

    public override bool RightClick(int i, int j)
    {
        FurnitureCommon.RightClickBreak(i, j);
        return true;
    }
}
