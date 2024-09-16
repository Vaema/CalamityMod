using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class SHPExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            float lights = Main.rand.NextFloat(0.9f, 1.1f);
            lights *= Main.essScale;
            Lighting.AddLight(Projectile.Center, 5f * lights, 1f * lights, 4f * lights);

            if (Projectile.ai[0] == 0f)
            {
                Color particleColor = new(255, Main.DiscoG, 155);
                CustomPulse inner = new(Projectile.Center, Vector2.Zero, particleColor, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.4f, 1.5f, 20);
                CustomPulse outer = new(Projectile.Center, Vector2.Zero, particleColor, "CalamityMod/Particles/BloomRing", Vector2.One, 0f, 0.4f, 2.5f, 20);
                CustomPulse explode = new(Projectile.Center, Vector2.Zero, particleColor, "CalamityMod/Particles/PlasmaExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.026f, 0.26f, 20);

                GeneralParticleHandler.SpawnParticle(inner);
                GeneralParticleHandler.SpawnParticle(outer);
                GeneralParticleHandler.SpawnParticle(explode);
                Projectile.ai[0] = 1f;
            }
        }
    }
}
