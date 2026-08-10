using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic;

public class GleamingBolt : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.timeLeft = 250;
        Projectile.penetrate = 1;
        Projectile.DamageType = DamageClass.Magic;
    }

    public override void AI()
    {
        Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
        Projectile.velocity *= 0.985f;
        for (int dust = 0; dust < 2; dust++)
        {
            int randomDust = Utils.SelectRandom(Main.rand, new int[]
            {
                64,
                204
            });
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, randomDust, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int k = 0; k < 5; k++)
        {
            int randomDust = Utils.SelectRandom(Main.rand, new int[]
            {
                64,
                204
            });
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, randomDust, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
        }
        if (Projectile.owner == Main.myPlayer)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 velocity = ((MathHelper.TwoPi * i / 6f) - (MathHelper.Pi / 3f - Projectile.velocity.ToRotation())).ToRotationVector2() * 3.5f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<GleamingBolt2>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }
        }
        SoundEngine.PlaySound(SoundID.Item105, Projectile.Center);
    }
}
