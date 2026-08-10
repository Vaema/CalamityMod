using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class BrickFragment : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override void SetStaticDefaults()
    {
    }
    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.width = 16;
        Projectile.scale = Main.rand.NextFloat(0.4f, 0.8f);
        Projectile.height = 16;
        Projectile.DamageType = RogueDamageClass.Instance;
    }
    public override void AI()
    {
        Projectile.ai[0]++;
        //Rotation and gravity
        Projectile.rotation += MathHelper.ToRadians(3f) * Projectile.direction;
        Projectile.velocity.Y = Projectile.velocity.Y + 0.27f + MathHelper.Clamp(Projectile.ai[0] / 40, 0, 0.5f);
        Projectile.velocity.X *= 0.97f;
        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }

    }
    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(0.5f)).WithVolumeScale(0.6f), Projectile.position);
        //Dust effect
        int splash = 0;
        while (splash < 4)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Copper, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.10f, 150, default, 0.9f);
            splash += 1;
        }
        int dust_splash = 0;
        while (dust_splash < 9)
        {
            GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(8), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, 0.4f, Color.SaddleBrown, false, true));
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Copper, 0f, 0f, 0, default, 0.5f);
            dust_splash += 1;
        }
    }
}
