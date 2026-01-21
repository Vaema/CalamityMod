using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
    public class DestroyerSparkTelegraph : Particle
    {
        public override string Texture => "CalamityMod/Particles/Sparkle2";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        public override bool Important => true;

        private NPC NPCToFollow;
        private float Spin;
        private float Opacity;
        private Color Bloom;
        private float BloomScale;

        public DestroyerSparkTelegraph(NPC npcToFollow, Color color, Color bloom, float scale, int lifeTime, float rotationSpeed = 0f, float bloomScale = 1f)
        {
            NPCToFollow = npcToFollow;
            Color = color;
            Bloom = bloom;
            Scale = scale;
            Lifetime = lifeTime;
            Spin = rotationSpeed;
            BloomScale = bloomScale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Opacity = MathF.Sin(LifetimeCompletion * MathHelper.Pi);
            Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);

            if (NPCToFollow != null && NPCToFollow.active && !Main.tile[NPCToFollow.Center.ToSafeTileCoordinates()].IsTileSolid())
                Position = NPCToFollow.Center;
            else
                Kill();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D starTexture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D bloomTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

            // Ajust the bloom's texture to be the same size as the star's.
            float properBloomSize = (float)starTexture.Height / (float)bloomTexture.Height;

            spriteBatch.Draw(bloomTexture, Position - Main.screenPosition, null, Bloom * Opacity * 0.5f, 0, bloomTexture.Size() / 2f, Scale * BloomScale * properBloomSize, SpriteEffects.None, 0);
            spriteBatch.Draw(starTexture, Position - Main.screenPosition, null, Color * Opacity * 0.5f, Rotation + MathHelper.PiOver4, starTexture.Size() / 2f, Scale * 0.75f, SpriteEffects.None, 0);
            spriteBatch.Draw(starTexture, Position - Main.screenPosition, null, Color * Opacity, Rotation, starTexture.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}
