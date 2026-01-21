using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CraniumSmasherExplosive : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 5f)
                Projectile.tileCollide = true;

            Projectile.rotation += Projectile.velocity.X * 0.02f;
            Projectile.velocity.Y += 0.085f;
            Projectile.velocity.X *= 0.99f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);

        public override void OnKill(int timeLeft)
        {
            Projectile.ExpandHitboxBy(200);
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

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
                float edgeOffset = Main.rand.NextFloat(60f, 100f) * (Main.rand.NextBool() ? -1 : 1);
                float randOffset = Main.rand.NextFloat(-100f, 100f);
                Vector2 spawnPos = Projectile.Center + (i % 2 == 0 ? new Vector2(edgeOffset, randOffset) : new Vector2(randOffset, edgeOffset));
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.IceTorch, Vector2.Zero, 100, default, 2f);
                dust.noGravity = true;
            }
        }
    }
}
