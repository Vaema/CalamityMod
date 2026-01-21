using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Particles
{
    public class DestroyerReticleTelegraph : Particle
    {
        public override string Texture => "CalamityMod/Particles/DestroyerReticleTelegraph";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool Important => true;

        private NPC NPCToFollow;
        private float OriginalScale;
        private float FinalScale;
        private float Opacity;
        private int RotationDirection;

        public DestroyerReticleTelegraph(NPC npcToFollow, Color color, float originalScale, float finalScale, int lifeTime)
        {
            NPCToFollow = npcToFollow;
            Color = color;
            OriginalScale = originalScale;
            Scale = originalScale;
            FinalScale = finalScale;
            Lifetime = lifeTime;
            RotationDirection = Main.rand.NextBool().ToDirectionInt();
        }

        public override void Update()
        {
            float pulseProgress = PiecewiseAnimation(LifetimeCompletion, new CurveSegment[] { new(EasingType.PolyOut, 0f, 0f, 1f, 4) });
            Scale = MathHelper.Lerp(OriginalScale, FinalScale, pulseProgress);
            Opacity = pulseProgress;
            Rotation += MathHelper.ToRadians(8f) * (1f - pulseProgress) * RotationDirection;

            if (NPCToFollow != null && NPCToFollow.active && !Main.tile[NPCToFollow.Center.ToSafeTileCoordinates()].IsTileSolid())
            {
                Position = NPCToFollow.Center;
                if (NPCToFollow.ai[2] == 1f)
                    Kill();
            }
            else
                Kill();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * Opacity, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}
