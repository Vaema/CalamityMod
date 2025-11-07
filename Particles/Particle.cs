using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Particles
{
    public class Particle
    {
        /// <summary>
        /// The ID of the particle type as registered by <see cref="GeneralParticleHandler"/> when the mod loads.
        /// <br>This field is set automatically when a particle instance is spawned in the world. This should NOT be set outside of that context.</br>
        /// </summary>
        public int Type;

        /// <summary>
        /// The amount of frames this particle has existed for. You shouldn't have to touch this manually.
        /// </summary>
        public int Time;

        /// <summary>
        /// The maximum amount of frames a particle may stay alive if Particle.SetLifeTime is set to true
        /// </summary>
        public int Lifetime = 0;

        /// <summary>
        /// The offset of the particle in relation to the origin of the set it belongs to. This is only used in the context of a <see cref="BaseParticleSet"/>.
        /// </summary>
        public Vector2 RelativeOffset;

        /// <summary>
        /// The inworld position of a particle. Keep in mind this isn't used in the context of a <see cref="BaseParticleSet"/>, since all the particles work off their relative position to the set's origin
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The velocity of the particle.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The point from which this particle's texture should be drawn from. 
        /// </summary>
        public Vector2 Origin;

        /// <summary>
        /// The color of the particle.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The current rotation of the particle in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The scale of the particle.
        /// </summary>
        public float Scale;

        /// <summary>
        /// The current Y-frame of this particle's spritesheet.
        /// </summary>
        public int Variant = 0;

        /// <summary>
        /// Whether your particle is affected by light levels.
        /// </summary>
        public bool AffectedByLight = false;

        /// <summary>
        /// Whether or not your particle should be drawn with a pixelated effect to match Terraria's pixel size.
        /// <br>Defaults to false.</br>
        /// </summary>
        public bool Pixelate = false;

        /// <summary>
        /// The "layer" or point at which you'd like your particle to draw in Terraria's internal draw order.
        /// <br>Defaults to <see cref="GeneralDrawLayer.AfterEverything"/>.</br>
        /// </summary>
        public GeneralDrawLayer DrawLayer = GeneralDrawLayer.AfterEverything;

        /// <summary>
        /// The path to this particle's autoloaded texture.
        /// </summary>
        /// <remarks>
        /// Can be accessed via <see cref="GeneralParticleHandler.GetTexture(int)"/>.
        /// </remarks>
        public virtual string Texture => "";

        /// <summary>
        /// The maximum amount of frames this particle's spritesheet has vertically.
        /// </summary>
        public virtual int FrameVariants => 1;

        /// <summary>
        /// An 0-1 interpolant representing how close this particle is from its <see cref="Lifetime"/>.
        /// </summary>
        public float LifetimeCompletion => Lifetime != 0 ? Time / (float)Lifetime : 0;

        /// <summary>
        /// Set this to true if you NEED the particle to render even if the particle cap is reached.
        /// </summary>
        public virtual bool Important => false;

        /// <summary>
        /// Set this to true if you want your particle to automatically get removed when its time reaches its maximum lifetime
        /// </summary>
        public virtual bool SetLifetime => false;

        /// <summary>
        /// Set this to true to make your particle use additive blending instead of alphablend.
        /// </summary>
        public virtual bool UseAdditiveBlend => false;

        /// <summary>
        /// Set this to true to make your particles work with semi transparent pixels. Is overriden if UseAdditiveBlend is set to true.
        /// </summary>
        public virtual bool UseHalfTransparency => false;

        /// <summary>
        /// Set this to true to disable default particle drawing, thus calling Particle.CustomDraw() instead.
        /// </summary>
        public virtual bool UseCustomDraw => false;

        /// <summary>
        /// Override and set this to true to disable automatic drawing for all instances of this particle entirely.
        /// <br>Drawing of this particle must be done from a <see cref="ParticleAutoDrawingOverride"/> class.</br>
        /// </summary>
        public virtual bool OverrideAutomaticDrawing => false;

        /// <summary>
        /// Use this method if you want to handle the particle drawing yourself. Only called if <b><see cref="UseCustomDraw"/></b> is set to true.
        /// </summary>
        public virtual void CustomDraw(SpriteBatch spriteBatch) { }

        /// <summary>
        /// Use this method if you want to handle particle drawing yourself in the context of a <see cref="BaseParticleSet"/>. 
        /// </summary>
        /// <param name="basePosition">The base position of the particle set.</param>
        public virtual void CustomDraw(SpriteBatch spriteBatch, Vector2 basePosition) { }

        /// <summary>
        /// Called every frame in <see cref="GeneralParticleHandler.Update"/>.
        /// The particle's velocity gets automatically added to its position, and its time automatically increases.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Removes the particle from the handler
        /// </summary>
        public void Kill() => GeneralParticleHandler.RemoveParticle(this);
    }
}
