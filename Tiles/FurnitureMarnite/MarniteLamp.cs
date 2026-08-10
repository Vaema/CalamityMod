using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureMarnite;

public class MarniteLamp : ModTile
{
    public override void SetStaticDefaults() => this.SetUpLamp(ModContent.ItemType<Items.Placeables.FurnitureMarnite.MarniteLamp>(), true);

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Granite, 0f, 0f, 1, new Color(255, 255, 255), 1f);
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
            r = 57f / 255f;
            g = 246f / 255f;
            b = 255f / 255f;
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
        FurnitureCommon.LightHitWire(Type, i, j, 1, 3);
    }
}
