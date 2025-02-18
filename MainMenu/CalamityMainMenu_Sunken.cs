using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace CalamityMod.MainMenu
{
    public class CalamityMainMenu_Sunken : ModMenu
    {
        public class Bubble
        {
            public int Time;
            public int Lifetime;
            public int IdentityIndex;
            public float Scale;
            public float Depth;
            public Color DrawColor;
            public Vector2 Velocity;
            public Vector2 Center;

            public Bubble(int lifetime, int identity, float depth, Color color, Vector2 startingPosition, Vector2 startingVelocity)
            {
                Lifetime = lifetime;
                IdentityIndex = identity;
                Depth = depth;
                DrawColor = color;
                Center = startingPosition;
                Velocity = startingVelocity;
            }
        }

        public static List<Bubble> Bubbles
        {
            get;
            internal set;
        } = new();

        public float remixLogoRotation = 0f;
        public override string DisplayName => "Calamity Style - Sunken";

        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("CalamityMod/MainMenu/LogoSunken");
        Asset<Texture2D> LogoWater => ModContent.Request<Texture2D>("CalamityMod/MainMenu/LogoSunken_Water");
        public override Asset<Texture2D> SunTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");
        public override Asset<Texture2D> MoonTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");

        public override int Music => CalamityMod.Instance.GetMusicFromMusicMod("SunkenSea") ?? MusicID.OceanNight;

        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<NullSurfaceBackground>();

        // Before drawing the logo, draw the entire Calamity background. This way, the typical parallax background is skipped entirely.
        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/MainMenu/SunkenMenuBackground").Value;

            // Calculate the draw position offset and scale in the event that someone is using a non-16:9 monitor
            Vector2 drawOffset = Vector2.Zero;
            float xScale = (float)Main.screenWidth / texture.Width;
            float yScale = (float)Main.screenHeight / texture.Height;
            float scale = xScale;

            // if someone's monitor isn't in wacky dimensions, no calculations need to be performed at all
            if (xScale != yScale)
            {
                // If someone's monitor is tall, it needs to be shifted to the left so that it's still centered on screen
                // Additionally the Y scale is used so that it still covers the entire screen
                if (yScale > xScale)
                {
                    scale = yScale;
                    drawOffset.X -= (texture.Width * scale - Main.screenWidth) * 0.5f;
                }
                else
                    // The opposite is true if someone's monitor is widescreen
                    drawOffset.Y -= (texture.Height * scale - Main.screenHeight) * 0.5f;
            }
            spriteBatch.End(); //                            BLESS THIS.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            spriteBatch.Draw(texture, drawOffset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            static Color selectBubbleColor()
            {
                return Color.Lerp(Color.Lerp(Color.MediumSlateBlue, Color.DarkBlue, 0.3f), Color.Lerp(Color.MediumTurquoise, Color.PaleTurquoise, Main.rand.NextFloat()), Main.rand.NextFloat());
            }

            // Randomly add bubbles.
            for (int i = 0; i < 3; i++)
            {
                if (Main.rand.NextBool(4))
                {
                    int lifetime = Main.rand.NextBool(5) ? Main.rand.Next(400, 500) : Main.rand.Next(200, 250);
                    float depth = Main.rand.NextFloat(1.8f, 5f);
                    Vector2 startingPosition = new Vector2(Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f), Main.screenHeight * 1.05f);
                    Vector2 startingVelocity = -Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * 2f;
                    Color bubbleColor = selectBubbleColor();
                    Bubbles.Add(new Bubble(lifetime, Bubbles.Count, depth, bubbleColor, startingPosition, startingVelocity));
                }
            }

            // Update all bubbles.
            for (int i = 0; i < Bubbles.Count; i++)
            {
                Bubbles[i].Scale = Utils.GetLerpValue(Bubbles[i].Lifetime, Bubbles[i].Lifetime / 3, Bubbles[i].Time, true);
                Bubbles[i].Scale *= MathHelper.Lerp(0.2f, 0.4f, Bubbles[i].IdentityIndex % 6f / 6f);
                //Bubbles[i].DrawColor.A = (byte)Utils.Remap(Bubbles[i].Lifetime, 100f, 0f, 1f, 0f, true);
                Bubbles[i].DrawColor.A = 150;
                if (Bubbles[i].IdentityIndex % 13 == 12)
                    Bubbles[i].Scale *= 0.5f;

                Bubbles[i].Time++;
                Bubbles[i].Center += Bubbles[i].Velocity;
            }

            // Clear away all dead bubbles.
            Bubbles.RemoveAll(c => c.Time >= c.Lifetime);

            // Draw bubbles.
            Texture2D bubbleTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/PearlParticleGlow").Value;
            for (int i = 0; i < Bubbles.Count; i++)
            {
                Vector2 drawPosition = Bubbles[i].Center;
                spriteBatch.Draw(bubbleTexture, drawPosition, null, Bubbles[i].DrawColor, Main.rand.NextFloat(0f, 90f), bubbleTexture.Size() * 0.5f, Bubbles[i].Scale, 0, 0f);
            }

            // Set the logo draw color to be white and the time to be noon
            // This is because there is not a day/night cycle in this menu, and changing colors would look bad
            drawColor = Color.White;
            Main.time = 27000;
            Main.dayTime = true;

            // Adjust rotation based on secret seeds; only Drunk world and Remix touch this, with GFB leeching off those two
            // Standard rotation is none; Drunk world makes it spin out, so it can use the vanilla rotation due to disappearing
            // Remix makes it flip upside down, and in GFB it will spin forever 
            if (WorldGen.remixWorldGen)
            {
                remixLogoRotation += MathHelper.Pi / 50f;
                if (remixLogoRotation >= MathHelper.Pi && !WorldGen.everythingWorldGen)
                    remixLogoRotation = MathHelper.Pi;
            }
            else
                remixLogoRotation = 0f;
            float rotationSecretSeedAdjusted = WorldGen.remixWorldGen ? remixLogoRotation : WorldGen.drunkWorldGen ? logoRotation : 0f;

            // Draw the logo using a different spritebatch blending setting so it doesn't have a horrible yellow glow
            Vector2 drawPos = new Vector2(Main.screenWidth / 2f, 100f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            spriteBatch.Draw(Logo.Value, drawPos, null, drawColor, rotationSecretSeedAdjusted, Logo.Value.Size() * 0.5f, WorldGen.drunkWorldGen ? logoScale : 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(LogoWater.Value, drawPos, null, new Color(255, 255, 255, 0.5f) * 0.7f, rotationSecretSeedAdjusted, Logo.Value.Size() * 0.5f, WorldGen.drunkWorldGen ? logoScale : 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            return false;
        }
    }
}
