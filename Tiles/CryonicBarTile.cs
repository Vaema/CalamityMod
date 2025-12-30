using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public class CryonicBarTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            this.SetUpBar(ModContent.ItemType<CryonicBar>(), new Color(138, 43, 226));
            DustType = DustID.JungleSpore;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = Main.rand.NextBool() ? 56 : 73;
            return true;
        }
    }
}
