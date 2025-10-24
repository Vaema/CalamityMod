using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CalamityMod.Effects;
using CalamityMod.Enums;
using CalamityMod.Graphics;
using CalamityMod.Systems.Graphic.PixelationSystem;
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
        #region Fields
        private static List<Particle> particles;
        private static Dictionary<GeneralDrawLayer, Queue<Particle>> particlesToSpawnNextFrame;
        // List containing the particles to delete
        private static List<Particle> particlesToKill;

        // Static list for details concerning every particle type
        internal static Dictionary<Type, int> particleTypes;
        internal static Dictionary<int, Texture2D> particleTextures;

        // Collections for storing and ordring particle instances for drawing.
        private static Dictionary<BlendState, List<Particle>> ParticlesToDraw;
        private static Dictionary<BlendState, List<Particle>> ParticlesToDraw_Pixelated;
        #endregion

        #region Loading and Unloading
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
        }

        public override void Load()
        {
            particles = [];
            particlesToSpawnNextFrame = [];
            particlesToKill = [];
            particleTypes = [];
            particleTextures = [];

            ParticlesToDraw = [];
            ParticlesToDraw_Pixelated = [];

            On_Main.DrawBackgroundBlackFill += DrawParticles_BeforeTiles;
            On_Main.DrawNPCs += DrawParticles_NPCs;
            On_Main.DrawProjectiles += DrawParticles_Projectiles;
            On_Main.DrawPlayers_AfterProjectiles += DrawParticles_AfterPlayers;
            On_Main.DrawDust += DrawParticles_AfterDusts;
            On_Main.DrawInfernoRings += DrawParticles_AfterEverything;
        }

        public override void Unload()
        {
            particles = null;
            particlesToSpawnNextFrame = null;
            particlesToKill = null;
            particleTypes = null;
            particleTextures = null;

            ParticlesToDraw = null;
            ParticlesToDraw_Pixelated = null;
        }
        #endregion

        /// <summary>
        /// Spawns the particle instance provided into the world. If the particle limit is reached but the particle is marked as important, it will try to replace a non important particle.
        /// </summary>
        /// <param name="manualDrawLayerOverride">Only set this to a non-null value if you'd like to manually override the draw layer of the particle instance you are spawning.</param>
        public static void SpawnParticle(Particle particle, GeneralDrawLayer? manualDrawLayerOverride = null)
        {
            // Don't queue particles if the game is paused.
            // This precedent is established with how Dust instances are created.
            // Don't spawn particles if on the server either, or if the particles dictionary is somehow null.
            if (Main.gamePaused || Main.dedServ || particles == null)
                return;

            if (particles.Count >= CalamityClientConfig.Instance.ParticleLimit && !particle.Important)
                return;

            if (manualDrawLayerOverride.HasValue)
                particle.DrawLayer = manualDrawLayerOverride.Value;

            particles.Add(particle);
            ReturnAssociatedDrawCollection(particle).Add(particle);
            particle.Type = particleTypes[particle.GetType()];
        }

        /// <summary>
        /// Queues a particle instance to be spawned from within <see cref="Update"/> a single frame after this is called.
        /// <br>Should be used in cases where you need to spawn a particle type from inside the overall particle update loop, such as inside the Update method of 
        /// another particle type.</br>
        /// <br>The single frame buffer ensures the overall particle update loop isn't altered prematurely from within the loop itself.</br>
        /// </summary>
        public static void QueueParticleForNextFrame(Particle particle, GeneralDrawLayer? manualDrawLayerOverride = null)
        {
            // Don't queue particles if the game is paused.
            // This precedent is established with how Dust instances are created.
            // Don't spawn particles if on the server side, or if the particles dictionary is somehow null.
            if (Main.gamePaused || Main.dedServ || particles == null)
                return;

            // Get the correct draw layer to spawn this particle on.
            GeneralDrawLayer actualDrawLayer = manualDrawLayerOverride ?? particle.DrawLayer;
            if (!particlesToSpawnNextFrame.ContainsKey(actualDrawLayer))
                particlesToSpawnNextFrame[actualDrawLayer] = [];

            particlesToSpawnNextFrame[actualDrawLayer].Enqueue(particle);
        }

        /// <summary>
        /// Removes an active particle instance from the world entirely.
        /// </summary>
        public static void RemoveParticle(Particle particle)
        {
            if (Main.dedServ)
                return;

            particlesToKill.Add(particle);
            ReturnAssociatedDrawCollection(particle).Remove(particle);
        }

        public static void Update()
        {
            if (Main.dedServ)
                return;

            // Spawn queued particles.
            foreach (var keyValuePair in particlesToSpawnNextFrame)
            {
                while (keyValuePair.Value.Count > 0)
                    SpawnParticle(keyValuePair.Value.Dequeue(), keyValuePair.Key);
            }

            // Update all particle instances in the world.
            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;

                particle.Position += particle.Velocity;
                particle.Time++;
                particle.Update();
            }

            //Clear out particles whose time is up
            particles.RemoveAll(particle => 
            {
                if ((particle.Time >= particle.Lifetime && particle.SetLifetime) || particlesToKill.Contains(particle))
                {
                    ReturnAssociatedDrawCollection(particle).Remove(particle);
                    return true;
                }
                return false;
            });

            particlesToKill.Clear();
        }

        private static void DrawParticles_BeforeTiles(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
        {
            Main.spriteBatch.End();
            DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.BeforeTiles);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private static void DrawParticles_NPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            if (!behindTiles)
            {
                Main.spriteBatch.End();
                DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.BeforeNPCs);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            orig(self, behindTiles);

            if (!behindTiles)
            {
                Main.spriteBatch.End();
                DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.AfterNPCs);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private static void DrawParticles_Projectiles(On_Main.orig_DrawProjectiles orig, Main self)
        {
            DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.BeforeProjectiles);

            orig(self);

            DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.AfterProjectiles);
        }

        private static void DrawParticles_AfterPlayers(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            orig(self);
            DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.AfterPlayers);
        }

        private static void DrawParticles_AfterDusts(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.AfterDusts);
        }

        private static void DrawParticles_AfterEverything(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig(self);

            Main.spriteBatch.End();
            DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer.AfterEverything);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        private static void DrawParticleInstance(Particle particle)
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

        private static void DrawParticleCollection(Dictionary<BlendState, List<Particle>> drawCollection, GeneralDrawLayer drawLayer, bool pixelated = false)
        {
            var scissorRectRasterizer = Main.Rasterizer;
            scissorRectRasterizer.ScissorTestEnable = true;
            Main.graphics.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.graphics.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            foreach (var keyValuePair in drawCollection)
            {
                if (pixelated)
                {
                    PixelationManager.AddPixelatedDrawer((pixelationMatrix) =>
                    {
                        var particlesAtSpecifiedLayer = keyValuePair.Value.Where(p => p.DrawLayer == drawLayer);
                        if (particlesAtSpecifiedLayer.Any())
                        {
                            foreach (Particle particle in particlesAtSpecifiedLayer)
                                DrawParticleInstance(particle);
                        }
                        
                    }, drawLayer, keyValuePair.Key);
                }
                else
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, keyValuePair.Key, SamplerState.LinearClamp, DepthStencilState.None, scissorRectRasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                    var particlesAtSpecifiedLayer = keyValuePair.Value.Where(p => p.DrawLayer == drawLayer);
                    if (particlesAtSpecifiedLayer.Any())
                    {
                        foreach (Particle particle in particlesAtSpecifiedLayer)
                            DrawParticleInstance(particle);
                    }

                    Main.spriteBatch.End();
                }
            }
        }

        private static void DrawParticleCollectionsAtSpecificLayer(GeneralDrawLayer drawLayer)
        {
            if (Main.dedServ)
                return;

            DrawParticleCollection(ParticlesToDraw, drawLayer);
            DrawParticleCollection(ParticlesToDraw_Pixelated, drawLayer, true);
        }

        private static List<Particle> ReturnAssociatedDrawCollection(Particle particle)
        {
            // Pixelated particles.
            if (particle.Pixelate)
            {
                if (particle.UseAdditiveBlend)
                {
                    if (!ParticlesToDraw_Pixelated.ContainsKey(BlendState.Additive))
                        ParticlesToDraw_Pixelated[BlendState.Additive] = [];
                    return ParticlesToDraw_Pixelated[BlendState.Additive];
                }
                else if (particle.UseHalfTransparency)
                {
                    if (!ParticlesToDraw_Pixelated.ContainsKey(BlendState.NonPremultiplied))
                        ParticlesToDraw_Pixelated[BlendState.NonPremultiplied] = [];
                    return ParticlesToDraw_Pixelated[BlendState.NonPremultiplied];
                }
                else
                {
                    if (!ParticlesToDraw_Pixelated.ContainsKey(BlendState.AlphaBlend))
                        ParticlesToDraw_Pixelated[BlendState.AlphaBlend] = [];
                    return ParticlesToDraw_Pixelated[BlendState.AlphaBlend];
                }
            }
            // Non-pixelated particles (regular).
            else
            {
                if (particle.UseAdditiveBlend)
                {
                    if (!ParticlesToDraw.ContainsKey(BlendState.Additive))
                        ParticlesToDraw[BlendState.Additive] = [];
                    return ParticlesToDraw[BlendState.Additive];
                }
                else if (particle.UseHalfTransparency)
                {
                    if (!ParticlesToDraw.ContainsKey(BlendState.NonPremultiplied))
                        ParticlesToDraw[BlendState.NonPremultiplied] = [];
                    return ParticlesToDraw[BlendState.NonPremultiplied];
                }
                else
                {
                    if (!ParticlesToDraw.ContainsKey(BlendState.AlphaBlend))
                        ParticlesToDraw[BlendState.AlphaBlend] = [];
                    return ParticlesToDraw[BlendState.AlphaBlend];
                }
            }
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
