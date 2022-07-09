using CalamityMod.ILEditing;
using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FloralParadise
{
    public class PinkFlower : ModTile
    {
        public const int Variants = 2;

        public const int WindPushLifetime = 45;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16,
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.RandomStyleRange = Variants;

            TileObjectData.addTile(Type);

            HitSound = SoundID.Grass;
            DustType = 44;
            AddMapEntry(new Color(255, 155, 202));
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX % 36 == 0 && drawData.tileFrameY == 0)
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            ILChanges.Windgrid.GetWindTime(i, j, WindPushLifetime, out int windTimeLeft, out int direction);
            float windInterpolant = windTimeLeft / (float)WindPushLifetime;
            float windRotation = Utils.GetLerpValue(0f, 0.5f, windInterpolant, true) * Utils.GetLerpValue(1f, 0.5f, windInterpolant, true) * direction * 0.34f;

            int frameX = Main.tile[i, j].TileFrameX;
            Color drawColor = Lighting.GetColor(i, j);
            Texture2D stamenTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/FloralParadise/PinkFlowerStamen").Value;
            Rectangle stamenFrame = stamenTexture.Frame(2, 1, frameX > 36 ? 1 : 0, 0);
            Vector2 stamenOrigin = stamenFrame.Size() * new Vector2(0.5f, 1f);
            Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + drawOffset + new Vector2(17f, 24f);

            // Create some pollen dust.
            if (!Main.gamePaused && windTimeLeft >= 2 && Main.rand.NextBool(6))
            {
                Vector2 pollenVelocity = -Vector2.UnitY.RotatedByRandom(windRotation * 2.3f) * 3f;
                Dust pollen = Dust.NewDustPerfect(drawPos + Main.screenPosition - drawOffset - Vector2.UnitY * 16f, 44);
                pollen.velocity = pollenVelocity;
                pollen.scale = 1.4f;
            }

            Main.spriteBatch.Draw(stamenTexture, drawPos, stamenFrame, drawColor, windRotation, stamenOrigin, 1f, SpriteEffects.None, 0f);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<PinkFlowerItem>());
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 4;
        }
    }
}
