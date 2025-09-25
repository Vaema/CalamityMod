using System;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class SmallBrainCoral : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.addTile(Type);
            DustType = DustID.SailfishBoots;
            AddMapEntry(new Color(36, 61, 111));

            base.SetStaticDefaults();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            float glowbrightness = 1f;
            if (Main.tile[i, j].IsTileActuallyInvisible())
                return;
            float glowspeed = (float)(Main.timeForVisualEffects * 0.01);
            glowbrightness *= MathF.Sin(i / 60f + glowspeed);

            int xFrameOffset = Main.tile[i, j].TileFrameX;
            int yFrameOffset = Main.tile[i, j].TileFrameY;
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/Ambient/SmallBrainCoral").Value;
            Vector2 drawOffest = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + drawOffest;
            Color drawColour = Color.White * glowbrightness;
            Tile trackTile = Main.tile[i, j];
            if (!trackTile.IsHalfBlock && trackTile.Slope == 0)
                spriteBatch.Draw(glowmask, drawPosition, new Rectangle(xFrameOffset, yFrameOffset, 18, 18), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            else if (trackTile.IsHalfBlock)
                spriteBatch.Draw(glowmask, drawPosition + new Vector2(0f, 8f), new Rectangle(xFrameOffset, yFrameOffset, 18, 8), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.gamePaused)
                return;

            if (closer)
            {
                if (Main.rand.NextBool(300))
                {
                    int tileLocationY = j - 1;
                    if (Main.tile[i, tileLocationY] != null)
                    {
                        if (!Main.tile[i, tileLocationY].HasTile)
                        {
                            if (Main.tile[i, tileLocationY].LiquidAmount == 255 && Main.tile[i, tileLocationY - 1].LiquidAmount == 255 && Main.tile[i, tileLocationY - 2].LiquidAmount == 255)
                            {
                                for (int t = 0; t < 5; t++)
                                {
                                    Dust dust = Dust.NewDustDirect(new Vector2(i, j + 0.5f) * 16, 16, 16, DustID.BlueTorch, 0, 0, 1, default, 1.5f);
                                    dust.velocity *= 0.2f;
                                    dust.noGravity = true;
                                    dust.noLight = true;
                                    dust.noLightEmittence = true;
                                }
                                Dust dust2 = Dust.NewDustDirect(new Vector2(i, j + 0.5f) * 16, 16, 16, DustID.MagicMirror, 0, 0, 1, Color.LightSkyBlue, 0.5f);
                                dust2.velocity *= 0.2f;

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(new EntitySource_WorldEvent(), i * 16 + 16, tileLocationY * 16 + 16, 0f, -0.1f, ModContent.ProjectileType<CoralBubbleSmall>(), 0, 1f, Main.myPlayer);
                            }
                        }
                    }
                }
            }
        }
    }
}
