using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using CalamityMod.BiomeManagers;
using CalamityMod.World;

namespace CalamityMod.Backgrounds
{ 
    internal static class AstralSurfaceBG
    {
        public static void Load()
        {
            if (Main.dedServ)
            {
                return;
            }

            On_Main.DrawSurfaceBG += DrawAstralBG;
        }

        public static float Transparency;

        public static float TransitionSpeed => 0.02f;

        private static void DrawAstralBG(On_Main.orig_DrawSurfaceBG orig, Main self)
        {
            if (Main.gameMenu)
            {
                orig(self);

                return;
            }

            orig(self);

            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<AbovegroundAstralBiomeSurface>()))
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

            //dont bother running any of the background drawing if the transparency is zero (meaning the background isnt actually active)
            if (Transparency > 0f && Main.BackgroundEnabled)
            {
                Vector2 vector = Main.screenPosition + new Vector2((Main.screenWidth >> 1), (Main.screenHeight >> 1));
                float num = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;
                float Scale = 2f;

                for (int Layers = 4; Layers >= 0; Layers--)
                {
                    //get each background texture
                    Texture2D BGTexture = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceBG" + Layers).Value;

                    Vector2 vector2 = new Vector2(BGTexture.Width, BGTexture.Height) * 0.5f;
                    float num2 = (Layers * 2 + 3f);
                    Vector2 vector3 = new Vector2(1f / num2);
                    Rectangle rectangle = new Rectangle(0, 0, BGTexture.Width, BGTexture.Height);
                    Vector2 zero = Vector2.Zero;

                    switch (Layers)
                    {
                        case 0:
                        {
                            zero.Y += 125f;
                            break;
                        }
                        case 1:
                        {
                            zero.Y += 160f;
                            break;
                        }
                        case 2:
                        {
                            zero.Y += 280f;
                            break;
                        }
                        case 3:
                        {
                            zero.Y += 135f;
                            break;
                        }
                        case 4:
                        {
                            zero.Y += 110f;
                            break;
                        }
                    }

                    vector2 *= Scale;

                    zero.Y -= num;
                    float RectangleScale = Scale * rectangle.Width;
                    int num6 = (int)((vector.X * vector3.X - vector2.X + zero.X - (Main.screenWidth >> 1)) / RectangleScale);

                    for (int j = num6 - 2; j < num6 + 4 + (int)(Main.screenWidth / RectangleScale); j++)
                    {
                        int AstralBiomeHeight = (World.AstralBiome.YStart + (int)Main.worldSurface) / 2;

                        Vector2 drawPosition = (new Vector2(j * Scale * (rectangle.Width / vector3.X), AstralBiomeHeight * 16f) + vector2 - vector) * vector3 + vector - Main.screenPosition - vector2 + zero;
                        var frame = rectangle;
                        var clr = new Color(50, 50, 50) * Transparency;

                        Main.spriteBatch.Draw(BGTexture, drawPosition, frame, clr, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

                        //draw glowmasks
                        if (Layers < 3)
                        {
                            Texture2D BGTextureGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceBG" + Layers + "_Glow").Value;

                            Main.spriteBatch.Draw(BGTextureGlow, drawPosition, frame, Color.White * Transparency, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }
    }
}
