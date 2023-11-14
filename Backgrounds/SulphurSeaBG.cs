using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using CalamityMod.World;
using CalamityMod.Systems;

namespace CalamityMod.Backgrounds
{ 
    internal static class SulphurSeaBG
    {
        public static void Load()
        {
            if (Main.dedServ)
            {
                return;
            }

            On_Main.DrawBG += DrawSulphurBG;
        }

        public static float Transparency;

        public static float TransitionSpeed => 0.02f;

        private static void DrawSulphurBG(On_Main.orig_DrawBG orig, Main self)
        {
            //conditions for the sulphur sea biome (copied directly from the biome manager)
            Point point = Main.LocalPlayer.Center.ToTileCoordinates();
            bool sulphurPosX = false;

            if (Abyss.AtLeftSideOfWorld)
            {
                if (point.X < 435)
                {
                    sulphurPosX = true;
                }
            }
            else
            {
                if (point.X > Main.maxTilesX - 435)
                {
                    sulphurPosX = true;
                }
            }

            bool inSulphurSeaBiome = (BiomeTileCounterSystem.SulphurTiles >= 300 || (point.Y < (Main.rockLayer - Main.maxTilesY / 13) && sulphurPosX && !WeakReferenceSupport.InAnySubworld())) && !Main.LocalPlayer.Calamity().ZoneAbyss;

            //dont run if you are on the menu
            if (Main.gameMenu)
            {
                orig(self);

                return;
            }

            //always call orig
            orig(self);

            //increase transparency while in the biome, and decrease it if not
            if (inSulphurSeaBiome)
            {
                Transparency += TransitionSpeed;

                if (Transparency > 1f)
                {
                    Transparency = 1f;
                }
            }
            else
            {
                Transparency -= TransitionSpeed;

                if (Transparency < 0f)
                {
                    Transparency = 0f;
                }
            }

            //dont bother running any of the background drawing if the transparency is zero (meaning the background isnt actually being used in game)
            if (Transparency > 0f)
            {
                Vector2 vector = Main.screenPosition + new Vector2((Main.screenWidth >> 1), (Main.screenHeight >> 1));
                float ZoomPos = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;

                //the scale of the entire background
                float Scale = 2f;

                //get each background texture
                Texture2D BGTexture = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/SulphurSeaBG").Value;
                Texture2D BGFrontTexture = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/SulphurSeaBGFront").Value;

                Vector2 BackgroundTexCenter = new Vector2(BGTexture.Width, BGTexture.Height) * 0.5f;
                Vector2 vector3 = new Vector2(1f / 3f);
                Rectangle rectangle = new Rectangle(0, 0, BGTexture.Width, BGTexture.Height);
                Vector2 zero = Vector2.Zero;

                BackgroundTexCenter *= Scale;

                zero.Y -= ZoomPos;
                float RectangleScale = Scale * rectangle.Width;
                int num6 = (int)((vector.X * vector3.X - BackgroundTexCenter.X + zero.X - (Main.screenWidth >> 1)) / RectangleScale);

                for (int j = num6 - 2; j < num6 + 4 + (int)(Main.screenWidth / RectangleScale); j++)
                {
                    int sulphurSeaHeight = SulphurousSea.YStart;

                    Vector2 drawPosition = (new Vector2(j * Scale * (rectangle.Width / vector3.X), sulphurSeaHeight * 16f) + BackgroundTexCenter - vector) * vector3 + vector - Main.screenPosition - BackgroundTexCenter + zero;

                    var frame = rectangle;
                    var color =  Main.ColorOfTheSkies * Transparency;
                    var frontColor = Color.LightSeaGreen * 0.5f;

                    Main.spriteBatch.Draw(BGTexture, drawPosition, frame, color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(BGFrontTexture, drawPosition, frame, frontColor * Transparency, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
