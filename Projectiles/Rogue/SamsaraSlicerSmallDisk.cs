using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class SamsaraSlicerSmallDisk : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        private double rotation = 0;
        public Projectile Parent = null;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 200;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (Projectile.Calamity().stealthStrike)
            {
                Projectile.localNPCHitCooldown = 3;
                Projectile.usesLocalNPCImmunity = true;
            }

            Projectile.ai[1]++;

            Projectile.rotation += MathHelper.ToRadians(12f);

            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(8f));

            float Vel = 15;
            if (Projectile.Calamity().stealthStrike) Vel = 20;

            float length = 10;
            if (Projectile.Calamity().stealthStrike) length = 15;

            if (Projectile.ai[1] > length && Parent != null)
            {
                Vel += (Projectile.ai[1] - length);

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Parent.Center) * Vel, Projectile.Calamity().stealthStrike ? 0.25f : 0.35f);

                if (Projectile.Distance(Parent.Center) < Projectile.velocity.Length() + 3)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], new Color(0f, 1f, 0f, 0f), 2);
            return false;
        }
    }
}
