using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CraniumSMASH : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -2;
        }

        public override void AI()
        {
            // Stealth on-death explosion is larger
            if (Projectile.ai[1] == 1f)
                Projectile.ExpandHitboxBy(300);

            if (Projectile.ai[0] == 0f)
            {
                SpawnExplosionDust(Projectile.width);
                Projectile.ai[0] = 1f;
            }
        }

        void SpawnExplosionDust(int size)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            if (!Main.dedServ)
            {
                int goreAmt = 3;
                Vector2 source = Projectile.Center - new Vector2(24f);
                for (int goreIndex = 1; goreIndex <= goreAmt; goreIndex++)
                {
                    float velocityMult = 0.33f * goreIndex;
                    int type = Main.rand.Next(61, 63 + 1);
                    Gore smoke = Gore.NewGoreDirect(Projectile.GetSource_Death(), source, default, type, 1f);
                    smoke.velocity *= velocityMult;
                    type = Main.rand.Next(61, 63 + 1);
                    smoke = Gore.NewGoreDirect(Projectile.GetSource_Death(), source, default, type, 1f);
                    smoke.velocity *= velocityMult;
                }
            }

            for (int i = 0; i < 30; i++)
            {
                float edgeOffset = Main.rand.NextFloat(size * 0.35f, size / 2) * (Main.rand.NextBool() ? -1 : 1);
                float randOffset = Main.rand.NextFloat(-size / 2, size / 2);
                Vector2 spawnPos = Projectile.Center + (i % 2 == 0 ? new Vector2(edgeOffset, randOffset) : new Vector2(randOffset, edgeOffset));
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.IceTorch, Vector2.Zero, 100, default, 2f);
                dust.noGravity = true;
            }
            if (Projectile.ai[1] == 1f)
            {
                CustomPulse boo = new(Projectile.Center, Vector2.Zero, Color.SkyBlue, "CalamityMod/Particles/GlowSquareParticleBig", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.1f, 1.1f, 10);
                GeneralParticleHandler.SpawnParticle(boo);
            }
        }
    }
}
