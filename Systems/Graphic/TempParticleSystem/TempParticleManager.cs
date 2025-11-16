using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Threading;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.TempParticleSystem
{
    public class TempParticleManager
    {
        public delegate void ParticleUpdateFunctionDelegate(TempParticle tempParticle);

        public delegate void ParticleDrawFunctionDelegate(TempParticle tempParticle, SpriteBatch spriteBatch, Vector2 drawOffset);

        public readonly List<TempParticle> ActiveParticles;

        public readonly int MaxParticles;

        public readonly Asset<Texture2D> BaseParticleTexture;

        public readonly ParticleUpdateFunctionDelegate ParticleUpdateFunction;

        public readonly ParticleDrawFunctionDelegate ParticleDrawingOverride;

        private readonly Queue<TempParticle> ParticlePool;

        /// <summary>
        /// A temporary particle handler made for the typically short-term creation and manipulation of particle effects done by instances of a generic particle object <see cref="TempParticle"/>.
        /// <br>Should typically be used in areas where <see cref="Particles.GeneralParticleHandler"/> is less practical or cannot work at all.</br>
        /// </summary>
        /// <param name="maxParticles">The maximum amount of particles that can exist at a time in this manager.</param>
        /// <param name="baseParticleTexturePath">The base texture that particles spawned by this manager will use when drawn.</param>
        /// <param name="particleUpdateFunction">An additional update function to control how particles spawned by this manager are updated.</param>
        /// <param name="particleDrawingOverride">Set this parameter to override how particles spawned by this manager are drawn.</param>
        public TempParticleManager(int maxParticles, string baseParticleTexturePath, ParticleUpdateFunctionDelegate particleUpdateFunction = null, ParticleDrawFunctionDelegate particleDrawingOverride = null)
        {
            if (Main.dedServ)
                return;

            MaxParticles = maxParticles;
            BaseParticleTexture = ModContent.Request<Texture2D>(baseParticleTexturePath);
            ParticleUpdateFunction = particleUpdateFunction;
            ParticleDrawingOverride = particleDrawingOverride;

            ActiveParticles = [];
            if (ParticlePool is null)
            {
                ParticlePool = [];
                for (int i = 0; i < maxParticles; i++)
                    ParticlePool.Enqueue(new TempParticle());
            }
        }

        /// <summary>
        /// Spawns a <see cref="TempParticle"/> instance attached to a <see cref="TempParticleManager"/>.
        /// <br>The "extraData" parameters exist so that you can store any additional data for this manager's particles in up to four additional slots.</br>
        /// </summary>
        /// <param name="position">The initial spawn position of this particle.</param>
        /// <param name="velocity">The initial velocity of this particle.</param>
        /// <param name="scale">The initial scale of this particle.</param>
        /// <param name="drawColor">The base color this particle is drawn in.</param>
        /// <param name="lifetime">The maximum amount of time this particle can be alive for.</param>
        /// <param name="rotation">The initial rotation of this particle.</param>
        /// <param name="opacity">The initial opacity of this particle.</param>
        /// <param name="parallaxStrength">The depth at which this particle appears. Should only be used in cases where particles need to be drawn in the foreground or background.</param>
        /// <param name="frameX">The initial X-frame of this particle on its spritesheet.</param>
        /// <param name="frameY">The initial Y-frame of this particle on its spritesheet.</param>
        /// <param name="maxHorizontalFrames">The maximum amount of horizontal frames this particle has on its spritesheet.</param>
        /// <param name="maxVerticalFrames">The maximum amount of horizontal frames this particle has on its spritesheet.</param>
        /// <param name="storedPosition">An optional position which can be stored for this particle to attach to.</param>
        public void SpawnParticle(Vector2 position, Vector2 velocity, Vector2 scale, Color drawColor, int lifetime, float rotation = 0f, float opacity = 1f, float parallaxStrength = 0f, int frameX = 0, int frameY = 0, int maxHorizontalFrames = 1, int maxVerticalFrames = 1, float extraData0 = 0f, float extraData1 = 0f, float extraData2 = 0f, float extraData3 = 0f, Vector2? storedPosition = null)
        {
            if (ActiveParticles.Count >= MaxParticles || ActiveParticles.Count >= CalamityClientConfig.Instance.ParticleLimit || Main.dedServ)
                return;

            if (ParticlePool.Count > 0)
            {
                TempParticle pooledParticle = ParticlePool.Dequeue();
                pooledParticle.SetBasicParticleData(BaseParticleTexture, position, storedPosition ?? Vector2.Zero, velocity, scale, drawColor, rotation, opacity, parallaxStrength, lifetime, frameX, frameY, maxHorizontalFrames, maxVerticalFrames, extraData0, extraData1, extraData2, extraData3);
                ActiveParticles.Add(pooledParticle);
            }
        }

        /// <summary>
        /// Spawns a <see cref="TempParticle"/> instance attached to a <see cref="TempParticleManager"/>.
        /// <br>The "extraData" parameters exist so that you can store any additional data for this manager's particles in up to four additional slots.</br>
        /// </summary>
        /// <param name="position">The initial spawn position of this particle.</param>
        /// <param name="velocity">The initial velocity of this particle.</param>
        /// <param name="scale">The initial scale of this particle.</param>
        /// <param name="drawColor">The base color this particle is drawn in.</param>
        /// <param name="lifetime">The maximum amount of time this particle can be alive for.</param>
        /// <param name="rotation">The initial rotation of this particle.</param>
        /// <param name="opacity">The initial opacity of this particle.</param>
        /// <param name="parallaxStrength">The depth at which this particle appears. Should only be used in cases where particles need to be drawn in the foreground or background.</param>
        /// <param name="frameX">The initial X-frame of this particle on its spritesheet.</param>
        /// <param name="frameY">The initial Y-frame of this particle on its spritesheet.</param>
        /// <param name="maxHorizontalFrames">The maximum amount of horizontal frames this particle has on its spritesheet.</param>
        /// <param name="maxVerticalFrames">The maximum amount of horizontal frames this particle has on its spritesheet.</param>
        /// <param name="storedPosition">An optional position which can be stored for this particle to attach to.</param>
        public void SpawnParticle(Vector2 position, Vector2 velocity, float scale, Color drawColor, int lifetime, float rotation = 0f, float opacity = 1f, float parallaxStrength = 0f, int frameX = 0, int frameY = 0, int maxHorizontalFrames = 1, int maxVerticalFrames = 1, float extraData0 = 0f, float extraData1 = 0f, float extraData2 = 0f, float extraData3 = 0f, Vector2? storedPosition = null)
            => SpawnParticle(position, velocity, new Vector2(scale), drawColor, lifetime, rotation, opacity, parallaxStrength, frameX, frameY, maxHorizontalFrames, maxVerticalFrames, extraData0, extraData1, extraData2, extraData3, storedPosition); 

        /// <summary>
        /// Updates all active particles attached to this <see cref="TempParticleManager"/>.
        /// </summary>
        public void Update()
        {
            if (ActiveParticles.Count == 0)
                return;

            FastParallel.For(0, ActiveParticles.Count, (start, end, context) =>
            {
                for (int i = start; i < end; i++)
                {
                    ActiveParticles[i].Time++;
                    ActiveParticles[i].Position += ActiveParticles[i].Velocity;
                    ParticleUpdateFunction.Invoke(ActiveParticles[i]);
                }
            });

            // Return the particle to the pool upon death.
            ActiveParticles.RemoveAll(p =>
            {
                if (p.Time >= p.Lifetime)
                {
                    ParticlePool.Enqueue(p);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Draws all active particles attached to this <see cref="TempParticleManager"/>.
        /// </summary>
        /// <remarks>
        /// <b>Note you will need to start the spritebatch manually before calling this.</b>
        /// </remarks>
        public void DrawAllParticles(SpriteBatch spriteBatch, Vector2? baseOffset = null)
        {
            if (ActiveParticles.Count == 0)
                return;

            Vector2 drawOffset = baseOffset ?? -Main.screenPosition;
            if (ParticleDrawingOverride != null)
            {
                foreach (TempParticle tempParticle in ActiveParticles)
                    ParticleDrawingOverride.Invoke(tempParticle, spriteBatch, drawOffset);
            }
            else
            {
                foreach (TempParticle tempParticle in ActiveParticles)
                {
                    Vector2 drawPosition = tempParticle.Position + drawOffset + tempParticle.StoredPosition;
                    Rectangle frame = BaseParticleTexture.Frame(tempParticle.MaxHorizontalFrames, tempParticle.MaxVerticalFrames, tempParticle.FrameX, tempParticle.FrameY);
                    Vector2 origin = frame.Size() * 0.5f;

                    spriteBatch.Draw(BaseParticleTexture.Value, drawPosition, frame, tempParticle.DrawColor * tempParticle.Opacity, tempParticle.Rotation, origin, tempParticle.Scale, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Returns all active particles back to the particle pool.
        /// </summary>
        public void ReturnAll()
        {
            if (ActiveParticles.Count == 0)
                return;

            ActiveParticles.RemoveAll(p =>
            {
                ParticlePool.Enqueue(p);
                return true;
            });
        }
    }
}
