using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityMod.Projectiles.Melee
{
    public class ChronoIcicleSmall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.coldDamage = true;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.coldDamage = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.05f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = SoundID.Item12.Volume * 0.7f }, Projectile.position);
            target.AddBuff(ModContent.BuffType<TimeDistortion>(), 30);
        }

        public override void OnKill(int timeLeft)
        {
            for (int index1 = 0; index1 < 3; ++index1)
            {
                int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 76);
                Main.dust[index2].noGravity = true;
                Main.dust[index2].noLight = true;
                Main.dust[index2].scale = 0.7f;
            }
        }
    }
}
