using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class HolyExplosionSupreme : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
        }

        public override void OnKill(int timeLeft)
        {
            SoundStyle HitSound = new("CalamityMod/Sounds/Custom/Providence/ProvidenceHolyBlastImpact") { Volume = 0.6f };
            SoundEngine.PlaySound(HitSound, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int i = 0; i < 14; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), new Vector2(7.5f, 7.5f).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1.9f), 0, default, Main.rand.NextFloat(1.1f, 1.9f));
                dust.shader = GameShaders.Armor.GetSecondaryShader(Owner.cShield, Owner);
                dust.noGravity = true;
                dust.color = Color.Goldenrod;
            }
            for (int j = 0; j < 12; j++)
            {
                Vector2 dustVel = new Vector2(6, 6).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.2f);

                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustVel * 2, DustID.SolarFlare, dustVel, 0, default, 1f);
                dust.shader = GameShaders.Armor.GetSecondaryShader(Owner.cShield, Owner);

                Particle spark = new CustomSpark(Projectile.Center, (new Vector2(15, 15).RotatedByRandom(100)) * Main.rand.NextFloat(0.2f, 1f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 37, Main.rand.NextFloat(1.35f, 1.5f), Main.rand.NextBool(4) ? Color.Khaki : Color.Orange, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.1f, 0.2f));
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
