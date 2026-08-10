using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureWulfrum.FurnitureAnodizedWulfrum;

public class AnodizedWulfrumSink : ModTile
{
    public override void SetStaticDefaults() => this.SetUpSink(ModContent.ItemType<Items.Placeables.FurnitureWulfrum.FurnitureAnodizedWulfrum.AnodizedWulfrumSink>(), true);

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Tungsten, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}
