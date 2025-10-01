using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class ShoresFlower : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true; 
            Main.tileLavaDeath[Type] = true;   
            Main.tileLighted[Type] = true;

            TileID.Sets.FramesOnKillWall[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };

            TileObjectData.newTile.AnchorWall = true;
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
            TileObjectData.newTile.AnchorRight = AnchorData.Empty;

            TileObjectData.newTile.Origin = new Point16(1, 1);

            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleWrapLimit = 3;
            TileObjectData.newTile.RandomStyleRange = 3;

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(20, 100, 220)); 
            DustType = DustID.MagicMirror; 
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float pulse = 0.95f + 0.15f * (0.5f + 0.5f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4.2f));
            r = 0.1f * pulse;
            g = 0.3f * pulse;
            b = 0.8f * pulse;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].IsTileActuallyInvisible())
                return;
            float pulse = 0.95f + 0.15f * ((float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4.2f));

            int xFrameOffset = Main.tile[i, j].TileFrameX;
            int yFrameOffset = Main.tile[i, j].TileFrameY;
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/Ambient/ShoresFlower").Value;
            Vector2 drawOffest = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + drawOffest;
            Color drawColour = Color.White * pulse;
            Tile trackTile = Main.tile[i, j];

            if (!trackTile.IsHalfBlock && trackTile.Slope == 0)
                spriteBatch.Draw(glowmask, drawPosition, new Rectangle(xFrameOffset, yFrameOffset, 18, 18), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            else if (trackTile.IsHalfBlock)
                spriteBatch.Draw(glowmask, drawPosition + new Vector2(0f, 8f), new Rectangle(xFrameOffset, yFrameOffset, 18, 8), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);

            CalamityUtils.DrawFlameEffect(ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/Ambient/ShoresFlower_Center").Value, i, j, 2);
        }
    }
}
