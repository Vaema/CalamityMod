using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 32;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            if (Main.dedServ)
                return;

            Dust trailDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 72, Scale: Main.rand.NextFloat(0.3f, 0.6f));
            trailDust.noGravity = true;
            trailDust.noLight = true;
            trailDust.noLightEmittence = true;

            Particle orb = new GenericBloom(Projectile.Center, Vector2.Zero, Color.Pink, 0.2f, 2, false);
            GeneralParticleHandler.SpawnParticle(orb);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            int dustAmount = Main.rand.Next(15, 26);
            for (int i = 0; i < dustAmount; i++)
            {
                Dust boomDust = Dust.NewDustPerfect(Projectile.Center, 72, (MathHelper.TwoPi / dustAmount * i).ToRotationVector2() * Main.rand.NextFloat(3f, 6f), Scale: Main.rand.NextFloat(0.6f, 1f));
                boomDust.noGravity = true;
            }
        }

        private float WidthFunction(float completionRatio) => Projectile.scale * 32f * CalamityUtils.Convert01To010(completionRatio);

        private Color ColorFunction(float completionRatio) => Color.Lerp(Color.Pink, Color.Transparent, completionRatio) * Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]));
            return false;
        }
    }
}
