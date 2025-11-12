using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CalamityMod.Enums;
using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    // Important credits for Spirit and Luminance:
    #region CREDITS

    // 06JAN2022: Iban
    // This particle system was inspired by spirit mod's own particle system, with permission granted by Yuyutsu. Love you spirit mod! -Iban

    // 05NOV2025: fryzahh
    // Particle drawing implementation was inspired by Luminance's ParticleManager.cs. See licensing information below:
    // https://github.com/LucilleKarma/Luminance/blob/main/LICENSE
    //
    // MIT License
    //
    // Copyright(c) 2024 Dominic
    //
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    //
    // The above copyright notice and this permission notice shall be included in all
    // copies or substantial portions of the Software.
    #endregion

    [Autoload(Side = ModSide.Client)]
    public sealed class GeneralParticleHandler : ModSystem
    {
        #region Fields
        /// <summary>
        /// The integer ID of each particle type defined across all mods, identified by the internal type of the respective particle.
        /// </summary>
        internal static Dictionary<Type, int> particleIDsByTypes;

        /// <summary>
        /// All <see cref="Particle"/> types defined across all mods, identified by a string for each respective particle which follows the following format: 
        /// <br><c>{Particle's Home Mod's Internal Name}.{Particle's Internal Name}</c></br>
        /// </summary>
        internal static Dictionary<string, Type> particleTypesByNames;

        /// <summary>
        /// The individual, autoloaded textures of each particle type defined across all mods, identified by the ID of the respective particle.
        /// </summary>
        internal static Dictionary<int, Texture2D> particleTexturesByIDs;

        private static Dictionary<Type, ParticleAutoDrawingOverride> particleAutoDrawingOverrides;

        private static List<Particle> activeParticles;
        private static List<Particle> particlesToKill;
        private static Dictionary<GeneralDrawLayer, Queue<Particle>> particlesToSpawnNextFrame;
        private static Dictionary<GeneralDrawLayer, Queue<Particle>> particlesToSpawnNextFrame_Pixelated;

        private static Dictionary<BlendState, List<Particle>> particlesToDraw;
        private static Dictionary<BlendState, List<Particle>> particlesToDraw_Pixelated;
        #endregion

        #region Loading and Unloading
        public override void PostSetupContent()
        {
            Type baseParticleType = typeof(Particle);
            ReflectionHelper.IterateEveryModsTypes<Particle>(action: type =>
            {
                int ID = particleIDsByTypes.Count; //Get the ID of the particle
                particleIDsByTypes[type] = ID;

                string registrationName = string.Concat(type.FullName.AsSpan(0, type.FullName.IndexOf('.')), ".", type.Name);
                particleTypesByNames[registrationName] = type;

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
                particleTexturesByIDs[ID] = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            });

            foreach (Mod mod in ModLoader.Mods)
            {
                var drawers = mod.GetContent().Where(c => c is ParticleAutoDrawingOverride).Select(c => c as ParticleAutoDrawingOverride);
                foreach (var drawer in drawers)
                {
                    if (drawer.TargetParticleTypeName != "")
                    {
                        if (particleTypesByNames.TryGetValue(drawer.TargetParticleTypeName, out Type particleType))
                            particleAutoDrawingOverrides.Add(particleType, drawer);
                        else
                            throw new Exception($"The Particle of name \"{drawer.TargetParticleTypeName}\" used in {drawer.GetType().Name} could not be found! " +
                                $"Please ensure the Particle's internal name is spelt correctly and is prefixed by its home mod's internal name.");
                    }
                }
            }
        }

        public override void Load()
        {
            particleTypesByNames = [];
            particleIDsByTypes = [];
            particleTexturesByIDs = [];

            particleAutoDrawingOverrides = [];

            activeParticles = [];
            particlesToKill = [];
            particlesToSpawnNextFrame = [];
            particlesToSpawnNextFrame_Pixelated = [];
            particlesToDraw = [];
            particlesToDraw_Pixelated = [];

            On_Main.DrawBackgroundBlackFill += DrawParticles_BeforeTiles;
            On_Main.DrawNPCs += DrawParticles_NPCs;
            On_Main.DrawProjectiles += DrawParticles_Projectiles;
            On_Main.DrawPlayers_AfterProjectiles += DrawParticles_AfterPlayers;
            On_Main.DrawDust += DrawParticles_AfterDusts;
            On_Main.DrawInfernoRings += DrawParticles_AfterEverything;
        }

        public override void Unload()
        {
            particleTypesByNames = null;
            particleIDsByTypes = null;
            particleTexturesByIDs = null;

            particleAutoDrawingOverrides = null;

            activeParticles = null;
            particlesToKill = null;
            particlesToSpawnNextFrame = null;
            particlesToSpawnNextFrame_Pixelated = null;
            particlesToDraw = null;
            particlesToDraw_Pixelated = null;
        }

        public override void OnWorldUnload()
        {
            activeParticles.Clear();
            particlesToKill.Clear();
            particlesToSpawnNextFrame.Clear();
            particlesToSpawnNextFrame_Pixelated.Clear();
            particlesToDraw.Clear();
            particlesToDraw_Pixelated.Clear();
        }
        #endregion

        /// <summary>
        /// Spawns the particle instance provided into the world. If the particle limit is reached but the particle is marked as important, it will try to replace a non important particle.
        /// </summary>
        /// <param name="pixelate">Set to true to force the particle being spawned to be drawn pixelated.</param>
        /// <param name="manualDrawLayerOverride">Only set this to a non-null value if you'd like to manually override the draw layer of the particle instance you are spawning.</param>
        public static void SpawnParticle(Particle particle, bool pixelate = false, GeneralDrawLayer? manualDrawLayerOverride = null)
        {
            // Don't queue particles if the game is paused.
            // This precedent is established with how Dust instances are created.
            // Don't spawn particles if on the server either, or if the particles dictionary is somehow null.
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;

            if (activeParticles.Count >= CalamityClientConfig.Instance.ParticleLimit && !particle.Important)
                return;

            particle.Pixelate = pixelate;
            if (manualDrawLayerOverride.HasValue)
                particle.DrawLayer = manualDrawLayerOverride.Value;

            activeParticles.Add(particle);
            AddToAssociatedDrawCollection(particle);
            particle.Type = particleIDsByTypes[particle.GetType()];
        }

        /// <summary>
        /// Queues a particle instance to be spawned from within <see cref="Update"/> a single frame after this is called.
        /// <br>Should be used in cases where you need to spawn a particle type from inside the overall particle update loop, such as inside the Update method of 
        /// another particle type.</br>
        /// <br>The single frame buffer ensures the overall particle update loop isn't altered prematurely from within the loop itself.</br>
        /// </summary>
        public static void QueueParticleForNextFrame(Particle particle, bool pixelate = false, GeneralDrawLayer? manualDrawLayerOverride = null)
        {
            // Don't queue particles if the game is paused.
            // This precedent is established with how Dust instances are created.
            // Don't spawn particles if on the server side, or if the particles dictionary is somehow null.
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;

            GeneralDrawLayer actualDrawLayer = manualDrawLayerOverride ?? particle.DrawLayer;
            if (pixelate)
            {
                if (!particlesToSpawnNextFrame_Pixelated.ContainsKey(actualDrawLayer))
                    particlesToSpawnNextFrame_Pixelated[actualDrawLayer] = [];
                particlesToSpawnNextFrame_Pixelated[actualDrawLayer].Enqueue(particle);
            }
            else
            {
                if (!particlesToSpawnNextFrame.ContainsKey(actualDrawLayer))
                    particlesToSpawnNextFrame[actualDrawLayer] = [];
                particlesToSpawnNextFrame[actualDrawLayer].Enqueue(particle);
            }
        }

        /// <summary>
        /// Removes an active particle instance from the world entirely.
        /// </summary>
        public static void RemoveParticle(Particle particle)
        {
            if (!Main.dedServ)
                particlesToKill.Add(particle);
        }

        /// <summary>
        /// Removes all active particle instances in the world of a specified type.
        /// </summary>
        public static void RemoveParticlesOfType<T>() where T : Particle
        {
            foreach (Particle particle in activeParticles)
            {
                if (particle.GetType() == typeof(T))
                    particlesToKill.Add(particle);
            }
        }

        /// <summary>
        /// Removes ALL active particle instances in the world.
        /// </summary>
        public static void RemoveAllParticles()
        {
            foreach (Particle particle in activeParticles)
                particlesToKill.Add(particle);
        }

        /// <summary>
        /// Gives you the amount of particle slots that are available. Useful when you need multiple particles at once to make an effect and dont want it to be only halfway drawn due to a lack of particle slots
        /// </summary>
        /// <returns></returns>
        public static int FreeSpacesAvailable()
        {
            //Safety check
            if (Main.dedServ || activeParticles == null)
                return 0;

            return CalamityClientConfig.Instance.ParticleLimit - activeParticles.Count();
        }

        /// <summary>
        /// Gives you the texture of the particle type. Useful for custom drawing
        /// </summary>
        public static Texture2D GetTexture(int type)
        {
            if (Main.dedServ)
                return null;

            return particleTexturesByIDs[type];
        }

        public static void Update()
        {
            if (Main.dedServ)
                return;

            // Spawn queued particles.
            foreach (var collectionsByDrawLayer in particlesToSpawnNextFrame)
            {
                while (collectionsByDrawLayer.Value.Count > 0)
                    SpawnParticle(collectionsByDrawLayer.Value.Dequeue(), false, collectionsByDrawLayer.Key);
            }

            foreach (var collectionsByDrawLayer in particlesToSpawnNextFrame_Pixelated)
            {
                while (collectionsByDrawLayer.Value.Count > 0)
                    SpawnParticle(collectionsByDrawLayer.Value.Dequeue(), true, collectionsByDrawLayer.Key);
            }

            // Update all particle instances in the world.
            foreach (Particle particle in activeParticles)
            {
                if (particle == null)
                    continue;

                particle.Position += particle.Velocity;
                particle.Time++;
                particle.Update();
            }

            //Clear out particles whose time is up
            activeParticles.RemoveAll(particle => 
            {
                if ((particle.Time >= particle.Lifetime && particle.SetLifetime) || particlesToKill.Contains(particle))
                {
                    RemoveFromAssociatedDrawCollection(particle);
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
            int drawIterations = Main.LocalPlayer.Calamity().trippy ? 4 : 1;
            for (int i = 0; i < drawIterations; i++)
            {
                // If you have shrooms, manually spoof the position of the particle for each clone location
                Vector2 positionSpoof = particle.Position;
                Vector2 positionDiff = positionSpoof - Main.LocalPlayer.Center;
                switch (i)
                {
                    case 0:
                        break;
                    case 1:
                        particle.Position = Main.LocalPlayer.Center - positionDiff;
                        break;
                    case 2:
                        particle.Position = Main.LocalPlayer.Center - Vector2.UnitY * positionDiff.Y + Vector2.UnitX * positionDiff.X;
                        break;
                    case 3:
                        particle.Position = Main.LocalPlayer.Center - Vector2.UnitX * positionDiff.X + Vector2.UnitY * positionDiff.Y;
                        break;
                }

                if (Main.LocalPlayer.Calamity().trippy)
                    particle.Color = Main.DiscoColor;

                // The actual drawing step
                if (particle.UseCustomDraw)
                {
                    particle.CustomDraw(Main.spriteBatch);
                }
                else
                {
                    Color lightColor = particle.Color;
                    if (particle.AffectedByLight)
                        lightColor = particle.Color.MultiplyRGB(Lighting.GetColor((particle.Position / 16).ToPoint()));

                    Rectangle frame = particleTexturesByIDs[particle.Type].Frame(1, particle.FrameVariants, 0, particle.Variant);
                    Main.spriteBatch.Draw(particleTexturesByIDs[particle.Type], particle.Position - Main.screenPosition, frame, lightColor, particle.Rotation, frame.Size() * 0.5f, particle.Scale, SpriteEffects.None, 0f);
                }

                // Since the switch case directly modifies the particle position, this resets it to the proper location
                particle.Position = positionSpoof;
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

            DrawParticleCollection(particlesToDraw, drawLayer);
            DrawParticleCollection(particlesToDraw_Pixelated, drawLayer, true);

            foreach (ParticleAutoDrawingOverride drawer in particleAutoDrawingOverrides.Values)
            {
                if (!drawer.ShouldDrawParticles)
                    return;

                foreach (var keyValuePair in drawer.ActiveParticleInstances)
                {
                    if (keyValuePair.Value.Count == 0)
                        return;

                    if (drawer.DrawLayerOverride.HasValue)
                    {
                        if (drawer.DrawLayerOverride.Value == drawLayer)
                            drawer.DrawAllParticles(Main.spriteBatch, keyValuePair.Value);
                    }
                    else
                    {
                        if (keyValuePair.Key == drawLayer)
                            drawer.DrawAllParticles(Main.spriteBatch, keyValuePair.Value);
                    }
                }
            }
        }

        private static List<Particle> ReturnAssociatedDrawCollection(Particle particle)
        {
            // Pixelated particles.
            if (particle.Pixelate)
            {
                if (particle.UseAdditiveBlend)
                {
                    if (!particlesToDraw_Pixelated.ContainsKey(BlendState.Additive))
                        particlesToDraw_Pixelated[BlendState.Additive] = [];
                    return particlesToDraw_Pixelated[BlendState.Additive];
                }
                else if (particle.UseHalfTransparency)
                {
                    if (!particlesToDraw_Pixelated.ContainsKey(BlendState.NonPremultiplied))
                        particlesToDraw_Pixelated[BlendState.NonPremultiplied] = [];
                    return particlesToDraw_Pixelated[BlendState.NonPremultiplied];
                }
                else
                {
                    if (!particlesToDraw_Pixelated.ContainsKey(BlendState.AlphaBlend))
                        particlesToDraw_Pixelated[BlendState.AlphaBlend] = [];
                    return particlesToDraw_Pixelated[BlendState.AlphaBlend];
                }
            }
            // Non-pixelated particles (regular).
            else
            {
                if (particle.UseAdditiveBlend)
                {
                    if (!particlesToDraw.ContainsKey(BlendState.Additive))
                        particlesToDraw[BlendState.Additive] = [];
                    return particlesToDraw[BlendState.Additive];
                }
                else if (particle.UseHalfTransparency)
                {
                    if (!particlesToDraw.ContainsKey(BlendState.NonPremultiplied))
                        particlesToDraw[BlendState.NonPremultiplied] = [];
                    return particlesToDraw[BlendState.NonPremultiplied];
                }
                else
                {
                    if (!particlesToDraw.ContainsKey(BlendState.AlphaBlend))
                        particlesToDraw[BlendState.AlphaBlend] = [];
                    return particlesToDraw[BlendState.AlphaBlend];
                }
            }
        }

        private static void AddToAssociatedDrawCollection(Particle particle)
        {
            if (particle.OverrideAutomaticDrawing && particleAutoDrawingOverrides[particle.GetType()].ShouldDrawParticles)
            {
                if (!particleAutoDrawingOverrides[particle.GetType()].ActiveParticleInstances.ContainsKey(particle.DrawLayer))
                    particleAutoDrawingOverrides[particle.GetType()].ActiveParticleInstances[particle.DrawLayer] = [];
                particleAutoDrawingOverrides[particle.GetType()].ActiveParticleInstances[particle.DrawLayer].Add(particle);
            }
            else
            {
                ReturnAssociatedDrawCollection(particle).Add(particle);
            }
        }

        private static void RemoveFromAssociatedDrawCollection(Particle particle)
        {
            if (particle.OverrideAutomaticDrawing)
                particleAutoDrawingOverrides[particle.GetType()].ActiveParticleInstances[particle.DrawLayer].Remove(particle);
            else
                ReturnAssociatedDrawCollection(particle).Remove(particle);
        }
    }
}
