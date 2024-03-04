using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee
{
    public class JudgementProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public ref float time => ref Projectile.ai[0];
        public float hitboxSize = 10;
        public Color mainColor;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 132;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 450;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (time == 0)
            {
                Projectile.scale = 0.25f;
                Projectile.velocity *= 0.75f;
                mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (time < 200)
            {
                Projectile.scale += 0.008f;
                hitboxSize += 0.55f;
            }
            Projectile.velocity *= 0.995f;

            if (time < 300 && time > 20 && Main.rand.NextBool(3))
            {
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(hitboxSize, hitboxSize), 66);
                trailDust.scale = Main.rand.NextFloat(0.7f, 0.85f);
                trailDust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.85f, 1.5f);
                trailDust.color = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
                trailDust.noGravity = true;
            }

            time++;
        }

        public override void OnKill(int timeLeft)
        {
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y - 100, 0f, 0f, ModContent.ProjectileType<WhiteBoltAura>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, mainColor == Color.MediumPurple ? Color.MediumOrchid : Color.MediumPurple);
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return mainColor;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, hitboxSize, targetHitbox);
    }
}
