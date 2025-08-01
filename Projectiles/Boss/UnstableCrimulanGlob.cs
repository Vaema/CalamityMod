using System;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class UnstableCrimulanGlob : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.Opacity = 0.8f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = CalamityWorld.death ? 490 : CalamityWorld.revenge ? 440 : Main.expertMode ? 390 : 240;

            if (CalamityWorld.LegendaryMode)
                Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Vector3 light = new Vector3(0.5f, 0.1f, 0.1f) * Projectile.Opacity;
            Lighting.AddLight(Projectile.Center, light.X, light.Y, light.Z);

            // Fly up and then fall down
            if (Projectile.ai[0] == 1f)
            {
                if (Projectile.ai[1] < 60f)
                {
                    Projectile.ai[1] += 1f;
                }
                else
                {
                    if (Projectile.velocity.Y < 12f)
                        Projectile.velocity.Y += 0.2f;
                }
            }

            // Accelerate
            else
            {
                if (Projectile.velocity.Length() < 15f && (Main.expertMode || BossRushEvent.BossRushActive))
                {
                    float velocityMult = CalamityWorld.death ? 1.015f : CalamityWorld.revenge ? 1.0125f : Main.expertMode ? 1.01f : 1.005f;
                    Projectile.velocity *= velocityMult;
                }
            }

            if (Projectile.timeLeft < 60)
                Projectile.Opacity = MathHelper.Lerp(0f, 0.8f, Projectile.timeLeft / 60f);

            if (Main.rand.NextBool())
            {
                Color dustColor = Color.Crimson;
                dustColor.A = 150;
                int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.TintableDust, 0f, 0f, 0, dustColor);
                Main.dust[dust].noGravity = true;
            }

            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.05f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 12f, targetHitbox);

        public override bool CanHitPlayer(Player target) => Projectile.timeLeft >= 60;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.timeLeft < 60)
                return;

            target.AddBuff(BuffID.Darkness, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
