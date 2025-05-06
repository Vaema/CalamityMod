using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Enemy
{
    public class StormlionSentryBullet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.timeLeft = 160;
            Projectile.extraUpdates = 6;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Effects.StormlionEffects.EnergyColor.ToVector3() * 0.3f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool())
            {
                int dustStyle = ModContent.DustType<LightDust>();
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, dustStyle);
                dust2.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust2.velocity = Projectile.velocity * Main.rand.NextFloat(0.6f, 1f);
                dust2.noGravity = true;
                dust2.color = Effects.StormlionEffects.EnergyColor;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<StaticDischarge>(), 60);
        }
        public override void OnKill(int timeLeft)
        {
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> orb = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

            for (int i = 0; i < 4; i++)
            {
                Vector2 scale = Projectile.scale * new Vector2(0.2f, 1.4f) * (0.05f + i * 0.02f) * 4;
                Main.EntitySpriteDraw(orb.Value, Projectile.Center - Main.screenPosition, null, Effects.StormlionEffects.EnergyColor with { A = 0 }, Projectile.rotation, orb.Size() * 0.5f, scale, SpriteEffects.None);
            }
            return false;
        }
    }
}
