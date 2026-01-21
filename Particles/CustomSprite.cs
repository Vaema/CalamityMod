using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class CustomSprite : Particle
    {
        bool addBlend = false;
        bool important = false;

        Texture2D Tex;
        int frames = 1;
        int currentFrame = 1;

        int Timer = 0;

        float maxGravity = 0f;
        float Opacity = 1f;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => addBlend;
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
            Tex = ModContent.Request<Texture2D>(tex).Value;
            frames = frameCount;
            currentFrame = frame;
        }

        // Overload which takes a Texture2D instead of a string for the texture to use
        public CustomSprite(Vector2 relativePosition, Vector2 velocity, int lifetime, Texture2D tex, float scale, Color color, float grav = 0f, bool AddativeBlend = true, bool needed = false, int frameCount = 1, int frame = 0)
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
                Opacity *= 0.9f;
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
            Rectangle fr = Tex.Frame(1, frames, 0, currentFrame);

            Main.EntitySpriteDraw(Tex, Position - Main.screenPosition, fr, Color.Lerp(Color.Transparent, Color, Opacity), Rotation, new Vector2(Tex.Width * 0.5f, Tex.Height / frames * 0.5f), 1f, SpriteEffects.None);
        }
    }
}
