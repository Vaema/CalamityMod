using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Metaballs;

public class StarsmokeMetaball : Metaball
{
    public Color borderColor = Color.Purple;
    public Vector2 goalDir = Vector2.Zero;
    public Vector2 scrollDir = Vector2.Zero;
    public class StarsmokeParticle
    {
        public float Size;

        public Vector2 Velocity;

        public Vector2 Center;
        public Vector2 BaseSquash = Vector2.One;
        public float Rotation;
        public float ShrinkSpeed = 0;
        public int Lifetime;
        public float SquashPower = 0;
        public int time = 0;

        public StarsmokeParticle(Vector2 center, Vector2 velocity, float size, int lifetime, Vector2 squash, float shrinkSpeed = 0, float velocitySquash = 0)
        {
            Center = center;
            Velocity = velocity;
            Size = size;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            BaseSquash = squash;
            Lifetime = lifetime;
            ShrinkSpeed = shrinkSpeed;
            SquashPower = velocitySquash;
        }

        public void Update()
        {
            float sine = (float)Math.Sin(time * 0.15f * SquashPower) * 0.8f;
            Center += Velocity;
                Velocity *= 0.96f;
                Velocity += Velocity.RotatedBy(0.36f * sine * SquashPower * (Math.Sign(ShrinkSpeed))) * 0.02f;
            if ((float)time / (float)Lifetime > 0.8f)
            {
                Size *= 0.9f;
            }
            BaseSquash.X *= (1 - 0.2f * ShrinkSpeed);
            BaseSquash.Y *= (1 + 0.2f * ShrinkSpeed);
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            time++;
        }
    }
    public static Asset<Texture2D> LayerAsset
    {
        get;
        private set;
    }

    public static List<StarsmokeParticle> Particles
    {
        get;
        private set;
    } = new();

    public override bool AnythingToDraw => Particles.Any();

    public override IEnumerable<Texture2D> Layers
    {
        get
        {
            yield return LayerAsset.Value;
        }
    }

    public override GeneralDrawLayer DrawLayer => GeneralDrawLayer.BeforeNPCs;
    public override void Load()
    {
        if (Main.dedServ)
            return;

        // Load the layer asset wrapper.
        LayerAsset = ModContent.Request<Texture2D>($"CalamityMod/Particles/GlowSquareParticleBig", AssetRequestMode.ImmediateLoad);
    }

    public override void Update()
    {
        if (goalDir == Vector2.Zero)
            goalDir = Vector2.One.RotatedByRandom(Math.PI);
        if ((int)(Main.GlobalTimeWrappedHourly * 50) % 5 == 0)
        {
            goalDir = goalDir.RotatedByRandom(0.13f);
        }
        scrollDir = Vector2.Lerp(scrollDir, goalDir, 0.01f);

        float rate = Main.GlobalTimeWrappedHourly * 19;
        List<Color> colors = new List<Color>()
        {
            new Color (192, 10, 111),
            Color.Coral,
            Color.DarkOrange
        };

        int colorIndex = (int)(rate / 2 % colors.Count);
        Color currentColor = colors[colorIndex];
        Color nextColor = colors[(colorIndex + 1) % colors.Count];
        borderColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

        // Update all particle instances.
        // Once sufficiently small, they vanish.
        for (int i = 0; i < Particles.Count; i++)
            Particles[i].Update();
        Particles.RemoveAll(p => p.Size <= 2f);
    }
    public override Color EdgeColor => borderColor with { A = 0 } * 0.3f;

    public static void SpawnParticle(Vector2 position, Vector2 velocity, float size, int lifetime, Vector2 squash, float shrinkSpeed = 0, float velocitySquash = 0) =>
        Particles.Add(new(position, velocity, size, lifetime, squash, shrinkSpeed, velocitySquash));

    // Make the texture scroll.
    public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
    {
        return scrollDir * 0.5f;
    }

    public override void DrawInstances()
    {
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Graphics/Metaballs/StarsmokeMetaball2").Value;

        foreach (StarsmokeParticle particle in Particles)
        {
            float sine = (float)Math.Sin(particle.time * 0.2f) * 0.5f;
            float lifetimeVal = 1 - (particle.time / particle.Lifetime);
            Vector2 altSquash = new Vector2(1f + 1 * (1 + sine) * lifetimeVal, 1f + 1 * (1 - sine) * lifetimeVal);
            Vector2 squash = Vector2.Lerp(particle.BaseSquash, new Vector2(Utils.Remap(particle.Velocity.Length(), 2, 7, 1, 0.5f), Utils.Remap(particle.Velocity.Length(), 2, 7, 1, 2.5f)), particle.SquashPower);
            Vector2 drawPosition = particle.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 scale = Vector2.One * particle.Size / tex.Size();
            Main.spriteBatch.Draw(tex, drawPosition, null, Color.White, particle.Rotation, origin, squash * scale, SpriteEffects.None, 0f);
        }
    }
}
