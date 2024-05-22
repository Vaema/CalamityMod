using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.Particles
{
    public class PlagueHumidifierMist : Particle
    {
        public override bool SetLifetime => true;
        public override int FrameVariants => 7;

        public override string Texture => "CalamityMod/Particles/PlagueHumidifierMist";

        public PlagueHumidifierMist(Vector2 relativePosition, int lifetime, float scale, Vector2 speed)
        {
            Velocity = speed;
            Scale = scale;
            Variant = Main.rand.Next(7);
            Lifetime = lifetime;
            Position = relativePosition;
        }

        public override void Update()
        {
            Color = Color.Lerp(Lighting.GetColor((Position / 16).ToPoint()), Color.Transparent, LifetimeCompletion);
        }

    }
}
