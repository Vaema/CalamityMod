using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.NPCs.BrimstoneElemental;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class BrimstoneHellfireballFriendly : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/Boss/BrimstoneHellfireball";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 2;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            //Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 9)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 6)
            {
                Projectile.frame = 0;
            }

            //Fade in
            if (Projectile.alpha > 5)
                Projectile.alpha -= 15;
            if (Projectile.alpha < 5)
                Projectile.alpha = 5;

            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) - MathHelper.ToRadians(90) * Projectile.direction;

            int brimDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 170, default, 1.1f);
            Main.dust[brimDust].noGravity = true;
            Main.dust[brimDust].velocity *= 0.5f;
            Main.dust[brimDust].velocity += Projectile.velocity * 0.1f;
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0.05f / 255f, (255 - Projectile.alpha) * 0.05f / 255f);

            if (Projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(BrimstoneElemental.HellfireballSound, Projectile.Center);
                Projectile.localAI[0] += 1f;
            }

            // Speed up as time goes on
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Projectile.velocity.Length() + 0.25f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int fire = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<HellfireExplosionFriendly>(), (int)(Projectile.damage * 0.66f), Projectile.knockBack, Projectile.owner);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
