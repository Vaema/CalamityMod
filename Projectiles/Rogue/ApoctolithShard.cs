using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class ApoctolithShard : ModProjectile, ILocalizedModType
    {
        public int TimeBeforeHoming => 30;
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Rogue/AbyssalMirrorProjectile";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.width = 13;
            Projectile.scale = Main.rand.NextFloat(0.7f, 1.2f);
            Projectile.height = 13;
            Projectile.DamageType = RogueDamageClass.Instance;
        }
        public override bool? CanDamage()
        {
            if (Projectile.ai[1] < TimeBeforeHoming) return false;
            return base.CanDamage();
        }
        public override void AI()
        {
            Projectile.ai[1]++;
            //Rotation and gravity
            if (Projectile.ai[1] < TimeBeforeHoming)
            {
                Projectile.velocity *= 0.94f;
            }
            else
            {
                Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 10, 0.05f);
                CalamityUtils.HomeInOnNPC(Projectile, false, 400, Projectile.ai[2], 0.2f);
                Projectile.velocity.Y += 0.4f;
            }

        }
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, 2, Color.Lerp(ApoctolithProj.HighBlueColor, Color.Transparent, 0.8f), texture: ModContent.Request<Texture2D>(Texture).Value);
            return base.PreDraw(ref lightColor);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(0.5f)).WithVolumeScale(0.6f), Projectile.position);
            //Dust effect
            int splash = 0;
            while (splash < 4)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceRod, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.10f, 150, default, 0.9f);
                splash += 1;
            }

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, ApoctolithProj.LowBlueColor, "CalamityMod/Particles/LargeBloom", Vector2.One, 0f, 0.3f, 0f, 25));
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", Vector2.One, 0f, 0.15f, 0f, 15));

            for (int i = 0; i < 5; i++) GeneralParticleHandler.SpawnParticle(new BloodParticle2(Projectile.Center, new Vector2(Main.rand.NextFloat(6, 12), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)), 12, Main.rand.NextFloat(0.02f, 0.1f), ApoctolithProj.HighBlueColor));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 120);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 120);
        }
    }
}
