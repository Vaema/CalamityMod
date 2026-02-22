using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Metaballs
{
    public class BloodMetaball : Metaball
    {
        public class Particle
        {
            public float Size;

            public Vector2 Velocity;

            public Vector2 Center;

            public Texture2D TextureToUse = null;

            public float rotation = 0;

            public Vector2 Scale = Vector2.One;

            public float SizeScaling = 0.85f;
            public int CurrentFrame = 0;
            public int MaxFrames = 1;

            public int ShrinkDelay = 45;
            public int TimeAlive = 0;
            public Particle(Vector2 center, Vector2 velocity, float size)
            {
                Center = center;
                Velocity = velocity;
                Size = size;
            }

            public void Update()
            {
                TimeAlive++;
                Center += Velocity;
                Velocity *= 0.96f;
                if (ShrinkDelay < TimeAlive)
                    Size *= SizeScaling;
            }
        }

        //public override bool IgnoreFPS => true;

        public override bool FixedToScreen => false;
        public static List<Particle> Particles
        {
            get;
            private set;
        } = new();

        public override bool AnythingToDraw => Particles.Any();
        public static Asset<Texture2D> LayerAsset
        {
            get;
            private set;
        }
        public override IEnumerable<Texture2D> Layers
        {
            get
            {
                yield return LayerAsset.Value;
            }
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            // Load the layer asset wrapper.
            LayerAsset = ModContent.Request<Texture2D>($"CalamityMod/Graphics/Metaballs/BloodLayer", AssetRequestMode.ImmediateLoad);
        }
        public override GeneralDrawLayer DrawLayer => GeneralDrawLayer.BeforeProjectiles;

        public override Color EdgeColor => new Color(67, 17, 17);

        public override void Update()
        {
            // Update all particle instances.
            // Once sufficiently small, they vanish.
            for (int i = 0; i < Particles.Count; i++)
                Particles[i].Update();
            Particles.RemoveAll(p => p.Size <= 2f);
        }

        public static Particle SpawnParticle(Vector2 position, Vector2 velocity, float size)
        {
            Particle particle = new(position, velocity, size);
            Particles.Add(particle);

            return particle;
        }

        // Make the texture scroll.
        public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
        {
            return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.0005f;
        }

        public override void DrawInstances()
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

            foreach (Particle particle in Particles)
            {
                var texture2d = particle.TextureToUse ?? tex;
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                var modScale = particle.Scale; 
                Vector2 scale = modScale * particle.Size / texture2d.Width;
                var frame = texture2d.Frame(1, particle.MaxFrames, 0, particle.CurrentFrame);
                Vector2 origin = frame.Size() * 0.5f;
                Main.spriteBatch.Draw(texture2d, drawPosition, frame, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.White), particle.rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
