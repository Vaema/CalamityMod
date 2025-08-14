using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureDriftwood
{
    public class DriftwoodLantern : ModTile
    {
        public override void SetStaticDefaults() => this.SetUpLantern(ModContent.ItemType<Items.Placeables.FurnitureDriftwood.DriftwoodLantern>(), true);
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => CalamityUtils.DrawSwayingMultiTile(i, j);

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 235, 0f, 0f, 1, new Color(255, 255, 255), 1f);
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
            CalamityUtils.LightHitWire(Type, i, j, 1, 2);
        }
    }
}
