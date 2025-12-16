using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class GolemInfernoBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int Lifetime = 900;
        private const float Radius = 200f;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 170; // Doesn't matter because circular hitbox
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override void AI()
        {
            // Visual effects
            for (int i = 0; i < 4; i++)
            {
                Vector2 spawn = Projectile.Center + Main.rand.NextVector2Circular(Radius, Radius);
                HealingPlus test = new HealingPlus(spawn, 1f, Vector2.Zero, Color.Orange, Color.OrangeRed, 2) { Rotation = Main.rand.NextFloat(MathHelper.TwoPi) };
                GeneralParticleHandler.SpawnParticle(test);
            }

            if (Projectile.timeLeft % 3 == 0)
            {
                FlameExplosion blast = new(Projectile.Center, Vector2.Zero, Color.Orange, Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.1f, 0.17f, 15, 0.7f);
                GeneralParticleHandler.SpawnParticle(blast);
            }

            for (int i = 0; i < 3; i++)
            {
                float randomMult = Main.rand.NextFloat(0.35f, 0.9f);
                Vector2 dustSpawn = Projectile.Center + Main.rand.NextVector2CircularEdge(Radius, Radius) * randomMult;
                Vector2 dustVel = Utils.DirectionTo(Projectile.Center, dustSpawn) * Utils.Remap(randomMult, 0.35f, 0.9f, 6f, 1f);
                Dust fire = Dust.NewDustPerfect(dustSpawn, DustID.InfernoFork, dustVel, Scale: 1.25f);
                fire.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Radius, targetHitbox);
        public override bool CanHitPlayer(Player target) => Projectile.timeLeft < Lifetime - 30;
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.OnFire, 360);
    }
}
