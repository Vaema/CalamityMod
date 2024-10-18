using CalamityMod.Buffs.DamageOverTime;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SummonAstralExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = Main.projFrames[Type] * 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            // Bluish cyan light
            Lighting.AddLight(Projectile.Center, 66f / 255f, 189f / 255f, 181f / 255f);
            if (Projectile.timeLeft % 5f == 4f)
                Projectile.frame++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
    }
}
