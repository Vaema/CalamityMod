using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class RemsRevengeExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.ai[1] > 0f)
                return;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int p = 0; p < 10; p++)
            {
                Vector2 velocity = (-Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(75f)) * Main.rand.NextFloat(4f, 6f);
                float scale = Main.rand.NextFloat(0.6f, 2f);
                Particle blood = new BloodParticle(Projectile.Center, velocity, 30, scale, Color.DarkRed);
                GeneralParticleHandler.SpawnParticle(blood);
            }
            Projectile.ai[1]++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Laceration>(), 60);
    }
}
