using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class SHPL : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/LaserProj";

        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.scale = 3f;
            Projectile.friendly = true;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 0.5f, 0.2f, 0.5f);
            float timerIncr = 3f;
            Projectile.localAI[0] += timerIncr;
            if (Projectile.localAI[0] > 100f)
                Projectile.localAI[0] = 100f;
        }

        public override Color? GetAlpha(Color lightColor) => SHPB.FindColorForSoul((int)Projectile.ai[0]);

        public override bool PreDraw(ref Color lightColor) => Projectile.DrawBeam(100f, 3f, lightColor);

        public override void OnKill(int timeLeft)
        {
            AltLineParticle line = new(Projectile.Center, -Projectile.velocity.RotatedByRandom(MathHelper.Pi / 6f), false, 20, 1f, SHPB.FindColorForSoul((int)Projectile.ai[0]));
            GeneralParticleHandler.SpawnParticle(line);
        }
    }
}
