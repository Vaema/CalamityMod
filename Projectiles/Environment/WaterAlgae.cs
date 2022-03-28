using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Environment
{
    public class WaterAlgae : ModProjectile
    {
        public ref float Direction => ref projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Algae");
            Main.projFrames[projectile.type] = 3;
            ProjectileID.Sets.CanDistortWater[projectile.type] = false;
        }

        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 12;
            projectile.ignoreWater = true;
            projectile.timeLeft = 720;
            projectile.penetrate = 1;
            projectile.scale = Main.rand?.NextFloat(0.4f, 0.75f) ?? 1f;
            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 120f, projectile.timeLeft, true) * Utils.InverseLerp(720f, 660f, projectile.timeLeft, true);
            projectile.hide = projectile.Opacity < 0.2f;
            if (projectile.localAI[0] == 0f)
            {
                projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
                projectile.localAI[0] = 1f;
            }

            if (Collision.WetCollision(projectile.position, projectile.width, projectile.height))
            {
                if (Direction == 0f)
                    Direction = projectile.identity % 2;
                if (Collision.SolidCollision(projectile.Center + Vector2.UnitX * Direction * 100f, 1, 1))
                    Direction *= -1f;

                projectile.velocity.X = MathHelper.Lerp(projectile.velocity.X, Direction * MathHelper.Lerp(0.3f, 0.7f, projectile.identity % 9f / 9f), 0.025f);
                projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y - 0.008f, -0.4f, 0.4f);
            }
            else
            {
                projectile.velocity.X *= 0.985f;
                projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y + 0.1f, -1f, 5f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.velocity.X *= 0.5f;
            if (projectile.timeLeft > 210)
                projectile.timeLeft = 210;

            return false;
        }
    }
}
