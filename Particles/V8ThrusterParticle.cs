using System;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Particles
{
    public class V8ThrusterParticle : Particle
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public Vector2[] Path = new Vector2[2];
        public Vector2 DashDirection;
        private VertexPositionColorTexture[] vertices;
        private BasicEffect effect;


        public V8ThrusterParticle(Vector2 relativePosition, Vector2 velocity, /*Vector2 dashDirection,*/ int lifetime, float scale, Color color)
        {
            Position = relativePosition;
            Velocity = velocity;
            Scale = scale;
            // DashDirection = dashDirection;
            Lifetime = lifetime;
            Color = color;
        }

        public override void Update()
        {
            Path[0] = Main.LocalPlayer.Center;
            Path[^1] = Position;
        }

    }
}
