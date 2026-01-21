using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class SemiCircularSmearFade : Particle
    {
        public override string Texture => "CalamityMod/Particles/SemiCircularSmearVerticalBlank";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        public Player player = Main.LocalPlayer;
        public Color InitialColor;
        public bool PlayerCentered;
        public bool RotateToVelocity;
        public Vector2 Squish;
        public bool ProduceLight;
        public int Direction = 1;
        public SemiCircularSmearFade(Vector2 position, Vector2 velocity, Color color, float rotation, float scale, Vector2 squish, int lifetime, bool playerCentered = false, bool rotateToVelocity = false, bool produceLight = true, int direction = 1)
        {
            Position = position;
            Velocity = velocity;
            Color = InitialColor = color;
            Scale = scale;
            Squish = squish;
            Rotation = rotation;
            Lifetime = lifetime;
            PlayerCentered = playerCentered;
            RotateToVelocity = rotateToVelocity;
            ProduceLight = produceLight;
            Direction = direction;
        }
        public override void Update()
        {
            if (PlayerCentered)
                Position = player.MountedCenter;
            if (RotateToVelocity)
                Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            if (ProduceLight)
            {
                Lighting.AddLight(Position, Color.R / 255f, Color.G / 255f, Color.B / 255f);
            }

            Scale *= 0.95f;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
            Velocity *= 0.95f;
        }

        //Use custom draw for the squish
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color, Rotation, tex.Size() * 0.5f, Scale * Squish, Direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
    }
}
