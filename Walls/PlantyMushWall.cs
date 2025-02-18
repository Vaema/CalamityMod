using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class PlantyMushWall : ModWall
    {
        public static Asset<Texture2D> leafTexture = null;

        public static Vector2 TileAdj => (Lighting.Mode == Terraria.Graphics.Light.LightMode.Retro || Lighting.Mode == Terraria.Graphics.Light.LightMode.Trippy) ? Vector2.Zero : Vector2.One * 12;
        /*public override bool Autoload(ref string name, ref string texture)
        {
            mod.AddWall("AstralGrassWallUnsafe", this, texture);
            return base.Autoload(ref name, ref texture);
        }*/
        public override void SetStaticDefaults()
        {
            DustType = DustID.Grass;
            AddMapEntry(new Color(54, 72, 9));
            if (!Main.dedServ)
            {
                leafTexture = ModContent.Request<Texture2D>("CalamityMod/Walls/PlantyMushWallLeaves");
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (i > Main.screenPosition.X / 16 && i < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j > Main.screenPosition.Y / 16 && j < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
            {
                Texture2D tex = leafTexture.Value;
                var rand = new Random(i + (j * 100000));

                float offset = i * j % 6.28f + (float)rand.NextDouble() / 8f;
                float sin = (float)Math.Sin(Main.GameUpdateCount / 45f + offset);

                spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 2.2f - Main.screenPosition,
                new Rectangle(rand.Next(4) * 26, 0, 24, 24), Lighting.GetColor(i, j), offset + sin * 0.09f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j].LiquidAmount <= 0 && j < Main.maxTilesY - 205)
            {
                Main.tile[i, j].LiquidAmount = 255;
                Main.tile[i, j].Get<LiquidData>().LiquidType = LiquidID.Water;
            }
        }

        public override void KillWall(int i, int j, ref bool fail) => fail = true;

        public override bool CanExplode(int i, int j) => false;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
