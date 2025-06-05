using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CalamityMod.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace CalamityMod.Particles
{
    [Autoload(Side = ModSide.Client)]
    public sealed class GeneralParticleHandler : ModSystem
    {
        private static List<Particle> particles;
        private static Queue<Particle> particlesToSpawnNextFrame;
        //List containing the particles to delete
        private static List<Particle> particlesToKill;
        //Static list for details concerning every particle type
        internal static Dictionary<Type, int> particleTypes;
        internal static Dictionary<int, Texture2D> particleTextures;
        //Lists used when drawing particles batched
        private static List<Particle> batchedAlphaBlendParticles;
        private static List<Particle> batchedNonPremultipliedParticles;
        private static List<Particle> batchedAdditiveBlendParticles;

        private static ManagedRenderTarget PixelationTarget_AlphaBlend;
        private static ManagedRenderTarget PixelationTarget_NonPremultiplied;
        private static ManagedRenderTarget PixelationTarget_AdditiveBlend;

        /// <summary>
        /// Whether or not the system is currently rendering any pixelated prticles to the render target or not.
        /// </summary>
        private static bool CurrentlyRendering { get; set; }

        /// <summary>
        /// The resolution ratio at which pixelated particles should draw.<br/>
        /// <c>0.5f</c> will draw at half resolution, <c>0.25f</c> will draw at quarter resolution, etc.
        /// </summary>
        private const float PixelationResolution = 0.5f;

        /// <summary>
        /// Creates a render target at half the screen's dimensions.
        /// This is done due to the fact that when the target itself drawn, it needs to be drawn at double the original scale in order to pixelate its contents
        /// </summary>
        private static RenderTarget2D CreatePixelTarget(int width, int height) => new(Main.instance.GraphicsDevice, (int)(width * PixelationResolution), (int)(height * PixelationResolution));

        public override void PostSetupContent()
        {
            Type baseParticleType = typeof(Particle);
            ReflectionHelper.IterateEveryModsTypes<Particle>(action: type =>
            {
                int ID = particleTypes.Count; //Get the ID of the particle
                particleTypes[type] = ID;

                // Flow: 2024/09/17
                // 'UnintializedObject' is allowed to use here as it's only read for Texture string Property
                // But do NOT EVER use it's instance as they are literally Uninitialized.
                // It might cause unintended behaviour if we do that.
#pragma warning disable SYSLIB0050
                Particle instance = (Particle)FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050

                string texturePath = type.Namespace.Replace('.', '/') + "/" + type.Name;
                if (instance.Texture != "")
                    texturePath = instance.Texture;
                particleTextures[ID] = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            });

            On_Main.CheckMonoliths += DrawToTarget;
        }

        public override void Load()
        {
            particles = [];
            particlesToSpawnNextFrame = [];
            particlesToKill = [];
            particleTypes = [];
            particleTextures = [];

            batchedAlphaBlendParticles = [];
            batchedNonPremultipliedParticles = [];
            batchedAdditiveBlendParticles = [];

            Main.QueueMainThreadAction(() =>
            {
                PixelationTarget_AlphaBlend = new(true, CreatePixelTarget);
                PixelationTarget_NonPremultiplied = new(true, CreatePixelTarget);
                PixelationTarget_AdditiveBlend = new(true, CreatePixelTarget);
            });
        }

        public override void Unload()
        {
            particles = null;
            particlesToSpawnNextFrame = null;
            particlesToKill = null;
            particleTypes = null;
            particleTextures = null;

            batchedAlphaBlendParticles = null;
            batchedNonPremultipliedParticles = null;
            batchedAdditiveBlendParticles = null;

            On_Main.CheckMonoliths -= DrawToTarget;
        }

        /// <summary>
        /// Spawns the particle instance provided into the world. If the particle limit is reached but the particle is marked as important, it will try to replace a non important particle.
        /// </summary>
        public static void SpawnParticle(Particle particle)
        {
            // Don't queue particles if the game is paused.
            // This precedent is established with how Dust instances are created.
            // Don't spawn particles if on the server either, or if the particles dictionary is somehow null.
            if (Main.gamePaused || Main.dedServ || particles == null)
                return;

            if (particles.Count >= CalamityClientConfig.Instance.ParticleLimit && !particle.Important)
                return;

            particles.Add(particle);
            particle.Type = particleTypes[particle.GetType()];
        }

        public static void QueueParticleForNextFrame(Particle particle)
        {
            // Don't queue particles if the game is paused.
            // This precedent is established with how Dust instances are created.
            // Don't spawn particles if on the server side, or if the particles dictionary is somehow null.
            if (Main.gamePaused || Main.dedServ || particles == null)
                return;

            particlesToSpawnNextFrame.Enqueue(particle);
        }

        public static void Update()
        {
            if (Main.dedServ)
                return;

            while (particlesToSpawnNextFrame.Count > 0)
                SpawnParticle(particlesToSpawnNextFrame.Dequeue());

            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;
                particle.Position += particle.Velocity;
                particle.Time++;
                particle.Update();
            }
            //Clear out particles whose time is up
            particles.RemoveAll(particle => (particle.Time >= particle.Lifetime && particle.SetLifetime) || particlesToKill.Contains(particle));
            particlesToKill.Clear();
        }

        public static void RemoveParticle(Particle particle)
        {
            if (Main.dedServ)
                return;

            particlesToKill.Add(particle);
        }

        private static void DrawToTarget(On_Main.orig_CheckMonoliths orig)
        {
            if (Main.gameMenu)
            {
                orig();
                return;
            }

            List<Particle> pixelatedAlphaBlendParticles = [];
            List<Particle> pixelatedNonPremultipliedParticles = [];
            List<Particle> pixelatedAdditiveBlendParticles = [];

            // Add each particle to their respective lists.
            foreach (Particle particle in particles)
            {
                if (particle != null && particle.Pixelate)
                {
                    pixelatedAlphaBlendParticles.AddWithCondition(particle, !particle.UseAdditiveBlend && !particle.UseHalfTransparency);
                    pixelatedNonPremultipliedParticles.AddWithCondition(particle, !particle.UseAdditiveBlend && particle.UseHalfTransparency);
                    pixelatedAdditiveBlendParticles.AddWithCondition(particle, particle.UseAdditiveBlend && !particle.UseHalfTransparency);
                }
            }

            CurrentlyRendering = true;

            Matrix pixelationMatrix = Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateScale(PixelationResolution / Main.GameViewMatrix.Zoom.X, PixelationResolution / Main.GameViewMatrix.Zoom.Y, 1f)
                * Matrix.CreateTranslation(Main.GameViewMatrix.Translation.X * PixelationResolution, Main.GameViewMatrix.Translation.Y * PixelationResolution, 0f);

            DrawParticlesToRenderTarget(PixelationTarget_AlphaBlend, pixelatedAlphaBlendParticles, BlendState.AlphaBlend, pixelationMatrix);
            DrawParticlesToRenderTarget(PixelationTarget_NonPremultiplied, pixelatedNonPremultipliedParticles, BlendState.NonPremultiplied, pixelationMatrix);
            DrawParticlesToRenderTarget(PixelationTarget_AdditiveBlend, pixelatedAdditiveBlendParticles, BlendState.Additive, pixelationMatrix);

            Main.instance.GraphicsDevice.SetRenderTarget(null);
            CurrentlyRendering = false;

            orig();
        }

        private static void DrawParticlesToRenderTarget(RenderTarget2D target, List<Particle> particles, BlendState blendStateToUse, Matrix pixelationMatrix)
        {
            target.SwapTo();

            if (particles.Count > 0)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendStateToUse, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, pixelationMatrix);

                foreach (Particle particle in particles)
                {
                    if (particle.UseCustomDraw)
                    {
                        particle.CustomDraw(Main.spriteBatch);
                    }
                    else
                    {
                        Color lightColor = particle.Color;
                        if (particle.AffectedByLight)
                            lightColor = particle.Color.MultiplyRGB(Lighting.GetColor((particle.Position / 16).ToPoint()));

                        Rectangle frame = particleTextures[particle.Type].Frame(1, particle.FrameVariants, 0, particle.Variant);
                        Main.spriteBatch.Draw(particleTextures[particle.Type], particle.Position - Main.screenPosition, frame, lightColor, particle.Rotation, frame.Size() * 0.5f, particle.Scale, SpriteEffects.None, 0f);
                    }
                }

                Main.spriteBatch.End();
            }
        }

        private static void DrawPixelatedParticles()
        {
            DrawScaledTarget(PixelationTarget_AlphaBlend, Main.spriteBatch, BlendState.AlphaBlend);
            DrawScaledTarget(PixelationTarget_NonPremultiplied, Main.spriteBatch, BlendState.NonPremultiplied);
            DrawScaledTarget(PixelationTarget_AdditiveBlend, Main.spriteBatch, BlendState.Additive);
        }

        private static void DrawScaledTarget(ManagedRenderTarget targetToDraw, SpriteBatch spriteBatch, BlendState blendState)
        {
            const float inversePixelationScale = 1f / PixelationResolution;

            spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(targetToDraw, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, inversePixelationScale, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        public static void DrawAllParticles(SpriteBatch sb)
        {
            if (Main.dedServ)
                return;

            if (particles.Count == 0)
                return;

            sb.End();
            var rasterizer = Main.Rasterizer;
            rasterizer.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            //Batch the particles to avoid constant restarting of the spritebatch
            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;

                if (particle.Pixelate)
                    continue;

                if (particle.UseAdditiveBlend)
                    batchedAdditiveBlendParticles.Add(particle);
                else if (particle.UseHalfTransparency)
                    batchedNonPremultipliedParticles.Add(particle);
                else
                    batchedAlphaBlendParticles.Add(particle);
            }
            if (batchedAlphaBlendParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedAlphaBlendParticles)
                {
                    if (particle.UseCustomDraw)
                        particle.CustomDraw(sb);
                    else
                    {
                        Color lightColor = particle.Color;
                        if (particle.AffectedByLight)
                        {
                            lightColor = particle.Color.MultiplyRGB(Lighting.GetColor((particle.Position / 16).ToPoint()));
                        }

                        Rectangle frame = particleTextures[particle.Type].Frame(1, particle.FrameVariants, 0, particle.Variant);
                        sb.Draw(particleTextures[particle.Type], particle.Position - Main.screenPosition, frame, lightColor, particle.Rotation, frame.Size() * 0.5f,
                            particle.Scale, SpriteEffects.None, 0f);
                    }
                }
                sb.End();
            }


            if (batchedNonPremultipliedParticles.Count > 0)
            {
                rasterizer = Main.Rasterizer;
                rasterizer.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedNonPremultipliedParticles)
                {
                    if (particle.UseCustomDraw)
                        particle.CustomDraw(sb);
                    else
                    {
                        Rectangle frame = particleTextures[particle.Type].Frame(1, particle.FrameVariants, 0, particle.Variant);
                        sb.Draw(particleTextures[particle.Type], particle.Position - Main.screenPosition, frame, particle.Color, particle.Rotation, frame.Size() * 0.5f, particle.Scale, SpriteEffects.None, 0f);
                    }
                }
                sb.End();
            }

            if (batchedAdditiveBlendParticles.Count > 0)
            {
                rasterizer = RasterizerState.CullNone;
                rasterizer.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedAdditiveBlendParticles)
                {
                    if (particle.UseCustomDraw)
                        particle.CustomDraw(sb);
                    else
                    {
                        Rectangle frame = particleTextures[particle.Type].Frame(1, particle.FrameVariants, 0, particle.Variant);
                        sb.Draw(particleTextures[particle.Type], particle.Position - Main.screenPosition, frame, particle.Color, particle.Rotation, frame.Size() * 0.5f, particle.Scale, SpriteEffects.None, 0f);
                    }
                }
                sb.End();
            }

            batchedAlphaBlendParticles.Clear();
            batchedNonPremultipliedParticles.Clear();
            batchedAdditiveBlendParticles.Clear();

            // Draw all pixelated particles.
            DrawPixelatedParticles();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        /// <summary>
        /// Gives you the amount of particle slots that are available. Useful when you need multiple particles at once to make an effect and dont want it to be only halfway drawn due to a lack of particle slots
        /// </summary>
        /// <returns></returns>
        public static int FreeSpacesAvailable()
        {
            //Safety check
            if (Main.dedServ || particles == null)
                return 0;

            return CalamityClientConfig.Instance.ParticleLimit - particles.Count();
        }

        /// <summary>
        /// Gives you the texture of the particle type. Useful for custom drawing
        /// </summary>
        public static Texture2D GetTexture(int type)
        {
            if (Main.dedServ)
                return null;

            return particleTextures[type];
        }

#pragma warning disable CS0414
        private static string noteToEveryone = "This particle system was inspired by spirit mod's own particle system, with permission granted by Yuyutsu. Love you spirit mod! -Iban";
#pragma warning restore CS0414
    }
}
