using System;
using CalamityMod.ILEditing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FloralParadise
{
    public class PinkFlowerBig : ModTile
    {
        public const int Variants = 2;

        public const int WindPushLifetime = 48;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(2, 2);
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16,
                16,
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.RandomStyleRange = Variants;

            HitSound = SoundID.Grass;
            TileObjectData.addTile(Type);

            DustType = 44;
            AddMapEntry(new Color(255, 155, 202));
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.5f;
            g = 0.4f;
            b = 0.16f;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX % 72 == 0 && drawData.tileFrameY == 0)
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            ILChanges.Windgrid.GetWindTime(i, j, WindPushLifetime, out int windTimeLeft, out int direction);
            float windInterpolant = windTimeLeft / (float)WindPushLifetime;
            float windRotation = Utils.GetLerpValue(0f, 0.5f, windInterpolant, true) * Utils.GetLerpValue(1f, 0.5f, windInterpolant, true) * direction * 0.34f;

            int frameX = Main.tile[i, j].TileFrameX;
            Color drawColor = Lighting.GetColor(i, j);
            Texture2D stamenTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/FloralParadise/PinkFlowerBigStamen").Value;
            Rectangle stamenFrame = stamenTexture.Frame(2, 1, frameX > 72 ? 1 : 0, 0);
            Vector2 stamenOrigin = stamenFrame.Size() * new Vector2(0.5f, 1f);
            Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + drawOffset + new Vector2(31f, 38f);
            if (frameX < 72)
                drawPos.X += 4f;

            // Create some pollen dust.
            if (!Main.gamePaused && windTimeLeft >= 2 && Main.rand.NextBool(4))
            {
                Vector2 pollenVelocity = -Vector2.UnitY.RotatedByRandom(windRotation * 2.3f) * 3f;
                Dust pollen = Dust.NewDustPerfect(drawPos + Main.screenPosition - drawOffset - Vector2.UnitY * 16f, 44);
                pollen.velocity = pollenVelocity;
                pollen.scale = 1.6f;
            }

            Main.spriteBatch.Draw(stamenTexture, drawPos, stamenFrame, drawColor, windRotation, stamenOrigin, 1f, SpriteEffects.None, 0f);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 4;
        }
    }
}
