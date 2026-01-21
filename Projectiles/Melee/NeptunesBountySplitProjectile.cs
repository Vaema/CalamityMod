using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee
{
    public class NeptunesBountySplitProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public int Time = 0;
        public int randTimer;
        public int dustType1 = 278;
        public int dustType2 = 267;
        public int spreadDust = 0;
        public Color WaterColor = Main.rand.NextBool() ? Color.DodgerBlue : Color.DeepSkyBlue;
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 110;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            float playerDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Time++;
            Projectile.velocity *= 0.988f;

            if (Projectile.timeLeft % 2 == 0 && Time > 3f && playerDist < 1400f)
            {
                Particle spark = new CustomSpark(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(0.1f, 0.5f), "CalamityMod/Particles/WaterFoam", false, Main.rand.Next(4, 7), Main.rand.NextFloat(0.15f, 0.2f), Color.DodgerBlue * 0.75f, new Vector2(1f, 1f), true, false, Main.rand.NextFloat(-10, 10));
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3 + spreadDust, 3 + spreadDust), !Main.rand.NextBool(5) ? dustType1 : dustType2, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.35f), 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                dust.noGravity = true;
                dust.color = !Main.rand.NextBool(5) ? WaterColor : Color.Aqua;
                if (dust.type == dustType1)
                    dust.scale *= 0.7f;
            }

            if (Projectile.timeLeft == 20)
            {
                WaterFlavoredParticle spark = new WaterFlavoredParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 25, 0.85f + Time * 0.013f, WaterColor * 0.15f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            else if (Projectile.timeLeft > 20)
            {
                Particle spark = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.05f, "CalamityMod/Particles/WaterFlavored", false, 2, 0.85f + Time * 0.013f, WaterColor * (1f - Time * 0.01f), new Vector2(0.2f + Time * 0.01f, 1), true);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Projectile.timeLeft < 20)
            {
                Time -= 5;
                spreadDust += 2;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HadopelagicPressure>(), 240);
        }
    }
}
