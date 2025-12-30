using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class CosmicRainbowFront : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "Terraria/Images/Projectile_251";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 210;
            Projectile.scale = 1.25f;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override bool? CanDamage() => false;
        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                //Projectile.localAI[0]++;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, Projectile.velocity.X * 0.001f, Projectile.velocity.Y * 0.001f, ModContent.ProjectileType<CosmicRainbowTrail>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
            }

            float velocityMult = 39.75f / Projectile.velocity.Length();
            float xVel = Projectile.velocity.X * velocityMult;
            float yVel = Projectile.velocity.Y * velocityMult;
            Projectile.velocity.X = xVel;
            Projectile.velocity.Y = yVel;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
