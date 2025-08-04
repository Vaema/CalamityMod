using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class HeavenfallenEnergy : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public bool raining => Projectile.ai[1] == 0f;

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            int coolDust;
            Projectile.ai[0]++;
            if (Projectile.ai[0] % 2 == 0)
            {
                int dustType = Projectile.ai[0] % 4 == 0 ? ModContent.DustType<AstralBlue>() : ModContent.DustType<AstralOrange>();
                coolDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Alpha: 100, Scale: 1.5f);
                Main.dust[coolDust].noGravity = true;
                Main.dust[coolDust].velocity *= 0.1f;
                Main.dust[coolDust].velocity += Projectile.velocity * 0.5f;
            }

            float homingRange = raining ? 480f : 240f;
            if (Projectile.ai[0] >= 15f || raining)
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, homingRange, 12f, 20f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
        public override bool? CanDamage() => Projectile.ai[0] >= 15f || raining;
    }
}
