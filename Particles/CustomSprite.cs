using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.Projectiles.Rogue.FinalDawnFlame;

namespace CalamityMod.Particles
{
    public class CustomSprite : Particle
    {
        bool addBlend = false;
        bool important = false;
        bool AffectedByGravity = false;

        string Tex = "";
        int frames = 1;
        int currentFrame = 1;

        int Timer = 0;

        float maxGravity = 0f;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => false;
        public override bool Important => important;
        public override string Texture => "CalamityMod/Particles/CuteStars";
        public override int FrameVariants => frames;

        public CustomSprite(Vector2 relativePosition, Vector2 velocity, int lifetime, string tex, float scale, Color color, float grav = 0f, bool AddativeBlend = true, bool needed = false, int frameCount = 1, int frame = 0)
        {
            maxGravity = grav;
            Position = relativePosition;
            Velocity = velocity;
            Scale = scale;
            Lifetime = lifetime;
            addBlend = AddativeBlend;
            important = needed;
            Color = color;
            Tex = tex;
            frames = frameCount;
            currentFrame = frame;
        }

        public override void Update()
        {
            Position += Velocity;

            Timer++;

            if (Timer > Lifetime - 20)
            {
                Scale *= 0.9f;
            }

            Velocity *= 0.85f;
            if (maxGravity != 0f)
            {
                if (Velocity.Length() < maxGravity)
                {
                    Velocity.X *= 0.94f;
                    Velocity.Y += maxGravity / 10f;
                }
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Tex);

            Rectangle fr = tex.Frame(1, frames, 0, currentFrame);

            Main.EntitySpriteDraw(tex.Value, Position - Main.screenPosition, fr, Color.Lerp(Color.Transparent, Color, Scale), Rotation, new Vector2(tex.Width() * 0.5f, tex.Height() / frames * 0.5f), 1f, SpriteEffects.None);
        }
    }
}
