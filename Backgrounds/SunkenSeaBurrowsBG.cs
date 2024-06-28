using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CalamityMod.Backgrounds
{
    internal static class SunkenSeaBurrowsBG
    {
        public static void Load()
        {
            if (Main.dedServ)
            {
                return;
            }

            On_Main.DrawBackgroundBlackFill += DrawBurrowsBG;
        }

        public static float Transparency;

        public static float TransitionSpeed => 0.02f;

        private static void DrawBurrowsBG(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
        {
            if (Main.gameMenu || Main.screenPosition.Y + Main.screenHeight < ((int)Main.worldSurface) * 16f)
            {
                orig(self);

                return;
            }

            orig(self);

            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.SunkenSeaBurrowsBiome>()))
            {
                Transparency += TransitionSpeed;

                if (Transparency > 1f)
                {
                    Transparency = 1f;
                }
            }
            else
            {
                //make transparency immediately go down so it doesnt look weird, since vanilla underground backgrounds dont have the fade-in effect this background has
                Transparency -= 1f;

                if (Transparency < 0f)
                {
                    Transparency = 0f;
                }
            }

            //dont bother running any of the background drawing if the transparency is zero (meaning the background isnt actually active)
            //also do not run any of the background drawing if you have the vanilla background config option turned off
            if (Transparency > 0f && Main.BackgroundEnabled)
            {
                Vector2 vector = Main.screenPosition + new Vector2((Main.screenWidth >> 1), (Main.screenHeight >> 1));
                float num = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;
                float Scale = 1.5f;

                for (int Layers = 4; Layers >= 0; Layers--)
                {
                    //get each background texture
                    Texture2D BGTexture = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/SunkenSeaBurrowsBG" + Layers).Value;

                    Vector2 vector2 = new Vector2(BGTexture.Width, BGTexture.Height) * 0.5f;
                    float num2 = (Layers * 2 + 3f);
                    Vector2 vector3 = new Vector2(1f / num2);
                    Rectangle rectangle = new Rectangle(0, 0, BGTexture.Width, BGTexture.Height);
                    Vector2 zero = Vector2.Zero;

                    switch (Layers)
                    {
                        case 0:
                        {
                            zero.Y += 800f;
                            break;
                        }
                        case 1:
                        {
                            zero.Y += 200f;
                            break;
                        }
                        case 2:
                        {
                            zero.Y += 450f;
                            break;
                        }
                        case 3:
                        {
                            zero.Y += 45f;
                            break;
                        }
                        case 4:
                        {
                            zero.Y += 45f;
                            break;
                        }
                    }

                    vector2 *= Scale;

                    zero.Y -= num;
                    float LoopWidth = Scale * rectangle.Width;
                    //float LoopHeight = Scale * rectangle.Height;
                    int LoopX = (int)((vector.X * vector3.X - vector2.X + zero.X - (Main.screenWidth >> 1)) / LoopWidth);
                    //int LoopY = (int)((vector.Y * vector3.Y - vector2.Y + zero.Y - (Main.screenWidth >> 1)) / LoopWidth);

                    //for (int i = LoopY - 2; i < LoopY + 4 + (int)(Main.screenWidth / LoopHeight); i++)
                    //{
                        for (int j = LoopX - 2; j < LoopX + 4 + (int)(Main.screenWidth / LoopWidth); j++)
                        {
                            Vector2 drawPosition = (new Vector2(j * Scale * (rectangle.Width / vector3.X), ((Main.LocalPlayer.Center.Y / 16f) - 90) * 16f) + vector2 - vector) * vector3 + vector - Main.screenPosition - vector2 + zero;

                            var frame = rectangle;
                            var color = Color.MediumTurquoise * Transparency;

                            Main.spriteBatch.Draw(BGTexture, drawPosition, frame, color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                        }
                    //}
                }
            }
        }
    }
}