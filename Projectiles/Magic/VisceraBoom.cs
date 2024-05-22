using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Projectiles.Magic
{
    public class VisceraBoom : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Viscera.BoomLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            for (int i = 0; i <= 2; i++)
            {
                float speed = Projectile.ai[1] > 0 ? 25 : 15;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 60 : DustID.Blood);
                dust.scale = Main.rand.NextFloat(1f, 2f) * Utils.GetLerpValue(0, Viscera.BoomLifetime, Projectile.timeLeft, true);
                dust.velocity = new Vector2(speed, speed).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.9f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BurningBlood>(), 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<BurningBlood>(), 180);
        }
    }
}
