using System.Collections.Generic;
using CalamityMod.Enums;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    /// <summary>
    /// Allows one to directly control how all active instances of a specified particle type in the world are drawn.
    /// <br>Useful for cases such as applying shaders to particles without resetting the SpriteBatch for every individual particle instance.</br>
    /// </summary>
    /// <remarks>
    /// Note that particles using an instance of this class <b>MUST</b> have <b><see cref="Particle.OverrideAutomaticDrawing"/></b> set to true.
    /// </remarks>
    public abstract class ParticleAutoDrawingOverride : ModType
    {
        /// <summary>
        /// All active particle instances for this drawer, identified by a particle's draw layer.
        /// </summary>
        public Dictionary<GeneralDrawLayer, List<Particle>> ActiveParticleInstances { get; private set; } = [];

        /// <summary>
        /// The internal name of the particle type that should be manually drawn.
        /// <br>Particle names should be prefixed with the internal name of the mod they are from.</br>
        /// <br>e.g. <b>"CalamityMod.PulseRing"</b> is valid, while just <b>"PulseRing"</b> is not.</br>
        /// </summary>
        /// <remarks>
        /// This is only checked once when the mod is loading. Changing this during runtime will not affect anything.
        /// </remarks>
        public abstract string TargetParticleTypeName { get; }

        /// <summary>
        /// Whether or not this drawer should draw its particles this frame.
        /// <br>Return false to prevent currently stored particles from being drawn and new ones from being added to <b><see cref="ActiveParticleInstances"/></b>.</br>
        /// </summary>
        /// <remarks>
        /// Returns true by default.
        /// </remarks>
        public virtual bool ShouldDrawParticles => true;

        /// <summary>
        /// An optional property which will override the draw layer of all particles for this drawer entirely, using the specified one here instead.
        /// </summary>
        public virtual GeneralDrawLayer? DrawLayerOverride => null;

        protected sealed override void Register() => ModTypeLookup<ParticleAutoDrawingOverride>.Register(this);

        public sealed override void SetupContent() => SetStaticDefaults();

        /// <summary>
        /// Write all relevenat draw code for your particles here.
        /// </summary>
        /// <param name="particlesToDraw">Stores particles based on their default draw layer.
        /// <br>Iterate through this collection to run individual particle drawing logic.</br></param>
        public abstract void DrawAllParticles(SpriteBatch spriteBatch, List<Particle> particlesToDraw);
    }
}
