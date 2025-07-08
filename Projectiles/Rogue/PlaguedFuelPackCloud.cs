using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class PlaguedFuelPackCloud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public int dir = 0;
        public float intensity = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 0;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 390;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ArmorPenetration = 10;
        }

        public override void AI()
        {
            if (Projectile.timeLeft < 50)
                Projectile.alpha += 5;
            if (dir == 0)
            {
                Projectile.timeLeft -= Main.rand.Next(5, 20 + 1);
                dir = Main.rand.NextBool() ? -1 : 1;
                intensity = Main.rand.NextFloat(0.2f, 1.2f);
            }

            if (Main.rand.NextBool(150))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.2f;
                Main.dust[dust].velocity.Y -= 0.15f;
            }

            if (Projectile.timeLeft < 290)
                CalamityUtils.HomeInOnSelectedNPC(Projectile, Projectile.Center.ClosestNPCAt(1000), true, 0.65f * intensity, 6, 0.98f, 0.995f, true);
            else
                Projectile.velocity = Projectile.velocity.RotatedBy(0.006f * dir * intensity) * Main.rand.NextFloat(0.985f, 1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Plague>(), 240);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Plague>(), 240);

        public override void OnKill(int timeLeft)
        {
            if (timeLeft != 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemEmerald, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100, default, 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                }
            }
        }
    }
}
