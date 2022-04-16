using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Environment
{
    public class WaterAlgae : ModProjectile
    {
        public ref float Direction => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Algae");
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.CanDistortWater[Projectile.type] = false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 720;
            Projectile.penetrate = 1;
            Projectile.scale = Main.rand?.NextFloat(0.4f, 0.75f) ?? 1f;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(0f, 120f, Projectile.timeLeft, true) * Utils.GetLerpValue(720f, 660f, Projectile.timeLeft, true);
            Projectile.hide = Projectile.Opacity < 0.2f;
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);
                Projectile.localAI[0] = 1f;
            }

            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                if (Direction == 0f)
                    Direction = Projectile.identity % 2;
                if (Collision.SolidCollision(Projectile.Center + Vector2.UnitX * Direction * 100f, 1, 1))
                    Direction *= -1f;

                Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Direction * MathHelper.Lerp(0.3f, 0.7f, Projectile.identity % 9f / 9f), 0.025f);
                Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y - 0.008f, -0.4f, 0.4f);
            }
            else
            {
                Projectile.velocity.X *= 0.985f;
                Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.1f, -1f, 5f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.X *= 0.5f;
            if (Projectile.timeLeft > 210)
                Projectile.timeLeft = 210;

            return false;
        }
    }
}
