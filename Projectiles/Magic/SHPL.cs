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

        public override Color? GetAlpha(Color lightColor) => new Color(255, Main.DiscoG, 155, Projectile.alpha);

        public override bool PreDraw(ref Color lightColor) => Projectile.DrawBeam(100f, 3f, lightColor);

        public override void OnKill(int timeLeft)
        {
            int dustAmt = Main.rand.Next(3, 7);
            for (int d = 0; d < dustAmt; d++)
            {
                int dustType = Utils.SelectRandom(Main.rand, new int[]
                {
                    246,
                    DustID.PinkFairy,
                    DustID.Flare_Blue
                });
                Dust idx = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity / 2f, dustType, Vector2.Zero, 100, default, 2.1f);
                idx.velocity *= 2f;
                idx.noGravity = true;
            }
        }
    }
}
