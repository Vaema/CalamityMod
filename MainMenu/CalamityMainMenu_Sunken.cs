using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

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

        public static List<Bubble> Bubbles { get; private set; } = [];

        public static List<SunkenFishBoid> Fishes { get; private set; } = [];

        public static Asset<Texture2D> LogoWater => ModContent.Request<Texture2D>("CalamityMod/MainMenu/LogoSunken_Water");

        public int MaxBoids = 120;

        public float remixLogoRotation = 0f;
        
        /// <summary>
        /// The global save data key which dictates whether or not the player is opening the Sunken Sea Overhaul update for the first time.
        /// </summary>
        public const string FirstTimeOpeningSunkenSeaOverhaulKey = "HasOpenedTheSunkenSeaOverhaulForTheFirstTime";

        public override string DisplayName => CalamityUtils.GetTextValue("UI.MainMenuSunken");

        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("CalamityMod/MainMenu/LogoSunken");

        public override Asset<Texture2D> SunTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");

        public override Asset<Texture2D> MoonTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");

        public override int Music => CalamityMod.Instance.GetMusicFromMusicMod("SunkenSea") ?? MusicID.OceanNight;

        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<NullSurfaceBackground>();

        public static void ForceMenuStyle()
        {
            // Forcefully open this ModMenu if it's the player's first time opening the Sunken Sea Overhaul update.
            if (GlobalSaveDataSystem.IsKeyAlreadySaved(FirstTimeOpeningSunkenSeaOverhaulKey))
                return;

            FieldInfo menusInfo = typeof(MenuLoader).GetField("menus", BindingFlags.Static | BindingFlags.NonPublic);
            List<ModMenu> modMenus = (List<ModMenu>)menusInfo.GetValue(null);

            var sunkenMenu = ModContent.GetInstance<CalamityMainMenu_Sunken>();
            if (modMenus.Contains(sunkenMenu))
            {
                FieldInfo lastSelectedMenuInfo = typeof(MenuLoader).GetField("LastSelectedModMenu", BindingFlags.Static | BindingFlags.NonPublic);
                int sunkenSeaMenuIndex = modMenus.IndexOf(sunkenMenu);
                lastSelectedMenuInfo.SetValue(null, modMenus[sunkenSeaMenuIndex].FullName);
            }
        }

        public override void Update(bool isOnTitleScreen)
        {
            if (!GlobalSaveDataSystem.IsKeyAlreadySaved(FirstTimeOpeningSunkenSeaOverhaulKey))
                GlobalSaveDataSystem.SaveKey(FirstTimeOpeningSunkenSeaOverhaulKey);
        }

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            // Draw the main background for the menu.   
            DrawMenuBackground(spriteBatch);

            // Draw the fish boids passively swimming around the menu.
            DrawFishes(spriteBatch);

            // Draw the bubbles rising from the bottom of the screen.
            DrawBubbles(spriteBatch);

            // Draw the light rays at the top of the screen.
            DrawLightRays(spriteBatch);

            // Draw the logo.
            DrawLogo(spriteBatch, ref logoDrawCenter, ref logoRotation, ref logoScale, ref drawColor);
            
            return false;
        }

        private static Color SelectBubbleColor()
        {
            return Color.Lerp(Color.Lerp(Color.MediumSlateBlue, Color.DarkBlue, 0.3f), Color.Lerp(Color.MediumTurquoise, Color.PaleTurquoise, Main.rand.NextFloat()), Main.rand.NextFloat());
        }

        private void DrawMenuBackground(SpriteBatch spriteBatch)
        {
            Texture2D backgroundTexture = ModContent.Request<Texture2D>("CalamityMod/MainMenu/SunkenMenuBackground").Value;

            // Calculate the draw position offset and scale in the event that someone is using a non-16:9 monitor
            Vector2 drawOffset = Vector2.Zero;
            float xScale = (float)Main.screenWidth / backgroundTexture.Width;
            float yScale = (float)Main.screenHeight / backgroundTexture.Height;
            float scale = xScale;

            // if someone's monitor isn't in wacky dimensions, no calculations need to be performed at all
            if (xScale != yScale)
            {
                // If someone's monitor is tall, it needs to be shifted to the left so that it's still centered on screen
                // Additionally the Y scale is used so that it still covers the entire screen
                if (yScale > xScale)
                {
                    scale = yScale;
                    drawOffset.X -= (backgroundTexture.Width * scale - Main.screenWidth) * 0.5f;
                }
                else
                    // The opposite is true if someone's monitor is widescreen
                    drawOffset.Y -= (backgroundTexture.Height * scale - Main.screenHeight) * 0.5f;
            }

            // Distort the background slightly to make it appear as if it's properly underwater.
            MiscShaderData distortionShader = GameShaders.Misc["CalamityMod:BasicTextureDistortion"];
            Asset<Texture2D> distortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Swirls");

            spriteBatch.End(); //                            BLESS THIS.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            const float distortionXSpeed = -0.012f;
            const float distortionYSpeed = 0.015f;
            const float noiseScale = 0.75f;
            const float noiseStrength = 0.02f;
            Vector2 timeOffset = new Vector2(distortionXSpeed, distortionYSpeed) * Main.GlobalTimeWrappedHourly * noiseScale;
            Vector2 noiseScaleStrength = new Vector2(noiseScale, noiseStrength);
            distortionShader.Shader.Parameters["timeOffset"].SetValue(timeOffset);
            distortionShader.Shader.Parameters["noiseScaleStrength"].SetValue(noiseScaleStrength);
            distortionShader.SetShaderTexture(distortionTexture);
            distortionShader.Apply();

            spriteBatch.Draw(backgroundTexture, drawOffset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }

        private void DrawLightRays(SpriteBatch spriteBatch)
        {
            // Draw rays shining down from the top of the screen.
            MiscShaderData underwaterRaysShader = GameShaders.Misc["CalamityMod:UnderwaterRays"];
            Asset<Texture2D> underwaterRayTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Pebbles");

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            underwaterRaysShader.Shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            underwaterRaysShader.Shader.Parameters["fadeOutMargin"].SetValue(0.58f);
            underwaterRaysShader.Shader.Parameters["overallOpacity"].SetValue(1f);
            underwaterRaysShader.Shader.Parameters["pixelationAmount"].SetValue(Main.screenWidth * 0.5f);
            underwaterRaysShader.Shader.Parameters["scrollSpeedX"].SetValue(0.013f);
            underwaterRaysShader.Shader.Parameters["scrollSpeedY"].SetValue(0.006f);
            underwaterRaysShader.Shader.Parameters["noiseScale"].SetValue(new Vector2(1.25f, 0.25f));
            underwaterRaysShader.Shader.Parameters["rayColor"].SetValue(Color.LightSkyBlue.ToVector3());
            underwaterRaysShader.Apply();

            spriteBatch.Draw(underwaterRayTexture.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }

        private void DrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }

        private void DrawBubbles(SpriteBatch spriteBatch)
        {
            // Randomly add bubbles.
            for (int i = 0; i < 3; i++)
            {
                if (Main.rand.NextBool(4))
                {
                    int lifetime = Main.rand.NextBool(5) ? Main.rand.Next(400, 500) : Main.rand.Next(200, 250);
                    float depth = Main.rand.NextFloat(1.8f, 5f);
                    Vector2 startingPosition = new Vector2(Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f), Main.screenHeight * 1.05f);
                    Vector2 startingVelocity = -Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * 2f;
                    Color bubbleColor = SelectBubbleColor();
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
        }

        private void DrawFishes(SpriteBatch spriteBatch)
        {
            // Generate random schools of fishes.
            if (Main.rand.NextBool(14) && Fishes.Count < MaxBoids)
            {
                int schoolCount = Main.rand.Next(1, 4);
                for (int i = 0; i < schoolCount; i++)
                {
                    int depth = Main.rand.Next(1, 4);
                    int lifetime = Main.rand.Next(1200, 1800);
                    float scale = Main.rand.NextFloat(0.8f, 1.2f);
                    Vector2 schoolSpawnPosition = new(Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f) * depth, Main.screenHeight * Main.rand.NextFloat(-0.1f, 1.1f) * depth);

                    int fishCount = Main.rand.Next(3, 11);
                    for (int j = 0; j < fishCount; j++)
                    {
                        Vector2 individualFishSpawnPosition = schoolSpawnPosition + Main.rand.NextVector2Circular(100f, 100f);
                        SunkenFishBoid fish = new(schoolSpawnPosition, scale, lifetime, depth);
                        Fishes.Add(fish);
                    }
                }
            }

            // Remove expired fish.
            Fishes.RemoveAll(f => f.Time >= f.Lifetime);

            // Update all fish.
            for (int i = 0; i < Fishes.Count; i++)
                Fishes[i].Update();

            // Draw all fish in an order based on their depth. 
            var fishesByDepth = Fishes.OrderByDescending(f => f.Depth).ToList();
            foreach (SunkenFishBoid fish in fishesByDepth)
                fish.Draw(spriteBatch);
        }
    }
}
