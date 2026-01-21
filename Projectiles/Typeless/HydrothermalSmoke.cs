using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class HydrothermalSmoke : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";


        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6;
        }

        public override void AI()
        {

            if (Projectile.timeLeft == 6)
                Projectile.Center = Main.player[Projectile.owner].Center;

            if (Main.rand.NextBool(6))
            {
                int fieryDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Flare, 0f, 0f, 100, default, 0.7f);
                if (Main.rand.NextBool(4))
                {
                    Main.dust[fieryDust].scale *= 0.35f;
                }
                Main.dust[fieryDust].velocity *= 0f;
            }


            if (Main.rand.NextBool(9) && !Main.dedServ)
            {
                float upwardVariation = Main.rand.NextFloat(-4.5f, -8f);
                MediumMistParticle mist = new MediumMistParticle(Projectile.position, -Projectile.velocity + new Vector2(0.5f, upwardVariation), // This velocity makes it slowly float upward
                Main.rand.NextBool(3) ? Color.LightSteelBlue : Color.SteelBlue, Color.LightSlateGray, Main.rand.NextFloat(0.4f, 0.65f), 130); 
                GeneralParticleHandler.SpawnParticle(mist);
            }
        }

        public override bool? CanDamage() => false;
    }
}
