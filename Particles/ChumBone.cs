using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class ChumBone : Particle
    {
        public override string Texture => "CalamityMod/Particles/ChumBone1";

        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        private bool direction;
        private bool variant;
        public static string Texture2 => "CalamityMod/Particles/ChumBone2";

        public ChumBone(Vector2 position, Vector2 velocity, Color color, float rotation, float scale, int lifeTime, bool variant, bool direction)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            Lifetime = lifeTime;
            Rotation = rotation;
            Color = color;
        }

        public override void Update()
        {
            // Gravity
            Tile below = CalamityUtils.ParanoidTileRetrieval((int)(Position.X / 16), (int)(Position.Y / 16));
            if (!below.HasTile || !below.IsTileSolid() || below.Slope > 0)
            {
                if (Velocity.Y < 10)
                {
                    Velocity.Y += 1;
                }
            }
            else
            {
                Velocity.Y = 0;
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = variant ? ModContent.Request<Texture2D>(Texture2).Value : ModContent.Request<Texture2D>(Texture).Value;            
            SpriteEffects spriteDir = direction ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Lighting.GetColor(Position.ToTileCoordinates()), Rotation, new Vector2(tex.Width / 2, tex.Height), Scale, spriteDir, 0);
        }
    }
}
