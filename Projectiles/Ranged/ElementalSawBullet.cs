using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ElementalSawBullet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.scale = 0.85f;
        }

        public override void AI()
        {
            Time++;

            // Rapidly race towards the nearest target.
            NPC potentialTarget = Projectile.Center.ClosestNPCAt(480f);
            if (potentialTarget != null && Time >= 20f)
            {
                Vector2 idealVelocity = Projectile.SafeDirectionTo(potentialTarget.Center) * 20f;
                Projectile.velocity = (Projectile.velocity * 29f + idealVelocity) / 30f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 3f);
            }
            else if (Time >= 20f)
            {
                // Projectile decays faster if there's no enemy in sight
                Projectile.timeLeft--;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Emit light.
            DelegateMethods.v3_1 = Color.Lerp(Color.Lime, Color.White, 0.55f).ToVector3() * 0.35f;
            Utils.PlotTileLine(Projectile.Center - Projectile.velocity * 0.5f, Projectile.Center + Projectile.velocity * 0.5f, 16f, DelegateMethods.CastLightOpen);
        }

        public override bool? CanDamage() => Projectile.ai[0] > 20;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<ElementalMix>(), 45);

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
    }
}
