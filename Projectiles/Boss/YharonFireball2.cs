using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class YharonFireball2 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override string Texture => "CalamityMod/Projectiles/Boss/YharonFireball";

        public static readonly SoundStyle FireballSound = new("CalamityMod/Sounds/Custom/Yharon/YharonFireball", 3);

        private const float TimeBeforeFalling = 180f;
        private const float MaxUpwardVelocity = -24f;
        private const float MaxDownwardVelocity = 16f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;

            if (Projectile.ai[0] < TimeBeforeFalling)
            {
                Projectile.ai[0] += 1f;
                Projectile.velocity.Y -= 0.1f;
                if (Projectile.velocity.Y < MaxUpwardVelocity)
                    Projectile.velocity.Y = MaxUpwardVelocity;
            }
            else
            {
                Projectile.velocity.X *= 0.8f;
                Projectile.velocity.Y += 0.2f;
                if (Projectile.velocity.Y > MaxDownwardVelocity)
                    Projectile.velocity.Y = MaxDownwardVelocity;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(FireballSound, Projectile.Center);
            }

            if (Main.rand.NextBool(16))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, 0f, 0f, 200, default, 1f);
                dust.scale *= 0.7f;
                dust.velocity += Projectile.velocity * 0.25f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, Projectile.alpha);

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = SoundID.Item14.Volume * 0.5f }, Projectile.Center);
            Projectile.ExpandHitboxBy(144);
            for (int d = 0; d < 2; d++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, 0f, 0f, 100, default, 1.5f);
            }
            for (int d = 0; d < 20; d++)
            {
                int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, 0f, 0f, 0, default, 2.5f);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
                idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, 0f, 0f, 100, default, 1.5f);
                Main.dust[idx].velocity *= 2f;
                Main.dust[idx].noGravity = true;
            }
            Projectile.Damage();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Dragonfire>(), 60);
        }
    }
}
