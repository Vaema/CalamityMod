using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles;

[LegacyName("ChaoticBarPlaced")]
public class ScoriaBarPlaced : ModTile
{
    public override void SetStaticDefaults()
    {
        this.SetUpBar(ModContent.ItemType<ScoriaBar>(), new Color(255, 165, 0));
        DustType = DustID.GemTopaz;
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        type = Main.rand.NextBool() ? 87 : 6;
        return true;
    }
}
