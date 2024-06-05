using System;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class ArianeFakeDust : Particle
    {
        public override string Texture => "CalamityMod/Particles/FakeDust";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        private Projectile Projectile;
        private Vector2 RelativePosition;
        private float Spin;
        private float Opacity;
        private bool Big;

        public ArianeFakeDust(Projectile proj, Vector2 relativePosition, Vector2 velocity, Color color, float scale, int lifeTime, float rotationSpeed = 1f, bool bigSize = false)
        {
            Projectile = proj;
            RelativePosition = relativePosition;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifeTime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed;
            Big = bigSize;
            Variant = Main.rand.Next(3);
        }

        public override void Update()
        {
            if (Projectile != null && Projectile.active && Projectile.type == ModContent.ProjectileType<LiliesOfFinalityAoE>())
                Position = Projectile.Center + RelativePosition;
            
            Velocity *= 0.95f;
            Scale *= 0.98f;
            Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);
            Opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D dustTexture = Big ? ModContent.Request<Texture2D>("CalamityMod/Particles/FakeDustBig").Value : ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = new Rectangle(0, (Big ? 8 : 6) * Variant, (Big ? 8 : 6), (Big ? 8 : 6));
            spriteBatch.Draw(dustTexture, Position - Main.screenPosition, frame, Color * Opacity, Rotation, frame.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}
