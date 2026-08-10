using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Metaballs;

public class CatastropheMetaball : Metaball
{
    public class Particle
    {
        public float Size;

        public Vector2 Velocity;

        public Vector2 Center;

        public Texture2D TextureToUse = null;

        public float rotation = 0;

        public float SizeScaling = 0.85f;
        bool firstFrame = true;

        public Particle(Vector2 center, Vector2 velocity, float size)
        {
            Center = center;
            Velocity = velocity;
            Size = size;
        }

        public void Update()
        {
            if (firstFrame) 
            {
                firstFrame = false;
                return;
            }
            Center += Velocity;
            Velocity *= 0.96f;
                Size *= SizeScaling;
        }
    }
    public override bool IgnoreFPS => true;
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
        LayerAsset = ModContent.Request<Texture2D>($"CalamityMod/Graphics/Metaballs/CatastropheLayer", AssetRequestMode.ImmediateLoad);
    }
    public override GeneralDrawLayer DrawLayer => GeneralDrawLayer.BeforeProjectiles;

    public override Color EdgeColor => new Color(0,255,255);

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
        return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.037f;
    }

    public override void DrawInstances()
    {
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

        foreach (Particle particle in Particles)
        {
            var texture2d = particle.TextureToUse ?? tex;
            Vector2 drawPosition = particle.Center - Main.screenPosition;
            Vector2 origin = texture2d.Size() * 0.5f;
            Vector2 scale = Vector2.One * particle.Size / texture2d.Width;
            Main.spriteBatch.Draw(texture2d, drawPosition, null, Color.White, particle.rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
