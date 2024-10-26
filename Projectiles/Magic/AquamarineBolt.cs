using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class AquamarineBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0.25f / 255f);
            int aquaDust = Dust.NewDust(Projectile.Center, Projectile.width - 28, Projectile.height - 28, DustID.BlueCrystalShard, 0f, 0f, 100, default, 1.5f);
            Main.dust[aquaDust].noGravity = true;
            Main.dust[aquaDust].velocity *= 0.1f;
            Main.dust[aquaDust].velocity += Projectile.velocity * 0.5f;
            if (Main.rand.NextBool(16))
            {
                int aquaticDust = Dust.NewDust(Projectile.Center, Projectile.width - 32, Projectile.height - 32, DustID.BlueCrystalShard, 0f, 0f, 100, default, 1f);
                Main.dust[aquaticDust].velocity *= 0.25f;
                Main.dust[aquaticDust].velocity += Projectile.velocity * 0.5f;
            }
            Projectile.rotation += 0.3f * (float)Projectile.direction;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);
        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 200);

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
