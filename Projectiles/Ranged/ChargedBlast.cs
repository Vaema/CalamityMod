using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ChargedBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/LaserProj";

        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 2;
            Projectile.alpha = 255;
            Projectile.timeLeft = 360;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
                Projectile.alpha -= 25;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            float lightScale = 1f - Projectile.alpha / 255f;
            Projectile.ai[2] = lightScale;
            Lighting.AddLight(Projectile.Center, 0f, 0.3f * lightScale, 0.7f * lightScale);

            float beamLength = 60f;
            float beamLengthGrowth = 1f;
            Projectile.localAI[0] += beamLengthGrowth;
            if (Projectile.localAI[0] > beamLength)
                Projectile.localAI[0] = beamLength;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(100, 100, 255, 0) * Projectile.ai[2];

        public override bool PreDraw(ref Color lightColor) => Projectile.DrawBeam(100f, 3f, lightColor);

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            int projectiles = 2;
            if (Projectile.owner == Main.myPlayer)
            {
                for (int k = 0; k < projectiles; k++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.ProjectileType<ChargedBlast2>(), (int)(Projectile.damage * 0.5), Projectile.knockBack * 0.8f, Main.myPlayer);
            }
        }
    }
}
