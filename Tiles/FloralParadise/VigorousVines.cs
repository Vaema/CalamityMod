using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FloralParadise
{
    public class VigorousVines : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileFrameImportant[Type] = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Vigorous Vines");
            AddMapEntry(new Color(145, 203, 102), name);

            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<FloralGrass>() };
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 2;
            TileObjectData.addTile(Type);

            soundType = SoundID.Grass;
            dustType = 2;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = CalamityUtils.ParanoidTileRetrieval(i, j);
            if (tile.frameX % 36 != 0 || tile.frameY % 36 != 0)
            {
                WorldGen.KillTile(i - tile.frameX % 36 / 18, j - tile.frameY % 36 / 18);

                if (CalamityUtils.ParanoidTileRetrieval(i - tile.frameX % 36 / 18, j - tile.frameY % 36 / 18 - 1).type == Type)
                    WorldGen.KillTile(i - tile.frameX % 36 / 18, j - tile.frameY % 36 / 18 - 1);
                return;
            }

            else if (WorldGen.genRand.NextBool(2) && Main.player[Player.FindClosest(new Vector2(i, j) * 16f, 16, 16)].cordage)
            {
                Item.NewItem(new Vector2(i * 16 + 24f, j * 16 + 24f), ItemID.VineRope);
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Main.tile[i, j];
            int frameX = t.frameX;
            int frameY = t.frameY;
            Texture2D tex = Main.tileTexture[Type];
            Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + drawOffset;
            Color drawColor = Lighting.GetColor(i, j);
            if (!t.halfBrick() && t.slope() == 0)
                spriteBatch.Draw(tex, drawPosition, new Rectangle(frameX, frameY, 18, 18), drawColor, 0f, Vector2.Zero, 1f, 0, 0f);
            else if (t.halfBrick())
                spriteBatch.Draw(tex, drawPosition + new Vector2(0f, 8f), new Rectangle(frameX, frameY, 18, 8), drawColor, 0f, Vector2.Zero, 1f, 0, 0f);

            return false;
        }

        public override void RandomUpdate(int i, int j)
        {

        }
    }
}
