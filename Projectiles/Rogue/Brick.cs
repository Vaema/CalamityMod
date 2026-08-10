using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class Brick : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Items/Weapons/Rogue/ThrowingBrick";

    public override void SetDefaults()
    {
        Projectile.width = 19;
        Projectile.aiStyle = -1;
        Projectile.height = 19;
        Projectile.friendly = true;
        Projectile.DamageType = RogueDamageClass.Instance;
    }

    public override void AI()
    {
        Projectile.ai[0] = Projectile.Calamity().stealthStrike ? 1 : 0;
        if (Main.rand.NextBool(3)) GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(12), 0).RotatedBy(Vector2.Zero.AngleTo(Projectile.velocity) + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20))), false, 10, Projectile.ai[0] == 1 ? 0.4f : 0.3f, Projectile.ai[0] == 1 ? Color.OrangeRed : Color.SaddleBrown, false, true));

        Projectile.ai[1]++;
        //Constant rotation and gravity
        Projectile.rotation += 0.4f * Projectile.direction;
        Projectile.velocity.X *= 0.98f;
        Projectile.velocity.Y = Projectile.velocity.Y + MathHelper.Clamp(Projectile.ai[1] / 40, 0, 0.6f);
        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }
        //Dust trail
        if (Main.rand.NextBool(13))
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pot, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default, 0.9f);
        }
    }

    public override void OnKill(int timeLeft)
    {
        Projectile.ai[0] = Projectile.Calamity().stealthStrike ? 1 : 0;
        SoundEngine.PlaySound(SoundID.Item50, Projectile.position);
        SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(0.5f, 1f)), Projectile.position);
        SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(-1f, -0.5f)), Projectile.position);
        //Dust on impact
        int dust_splash = 0;
        while (dust_splash < 9)
        {
            GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(15), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Projectile.ai[0] == 1 ? 1.2f : 0.6f, Projectile.ai[0] == 1 ? Color.OrangeRed : Color.SaddleBrown, false, true));
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Copper, 0f, 0f, 0, default, 0.5f);
            dust_splash += 1;
        }
        // This only triggers if stealth is full
        if (Projectile.ai[0] == 1)
        {
            dust_splash = 0;
            while (dust_splash < 9)
            {
                GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(24), 0).RotatedByRandom(MathHelper.TwoPi), false, 6, 0.6f, Color.SaddleBrown, false, true));
                GeneralParticleHandler.SpawnParticle(new SmallSmokeParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), Color.SaddleBrown, Color.SaddleBrown, Main.rand.NextFloat(1f, 1.5f), 150, affectedByLight: true));
                dust_splash += 1;
            }
            int split = 0;
            while (split < 5)
            {
                //Calculate the velocity of the projectile
                Vector2 shardspeed = new Vector2(Main.rand.Next(3, 8), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));

                Vector2 speedAdd = -Projectile.velocity;
                speedAdd.Normalize();
                shardspeed += speedAdd * 9;
                //Spawn the projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position + shardspeed, shardspeed, ModContent.ProjectileType<BrickFragment>(), Projectile.damage / 2, Projectile.knockBack / 2f, Projectile.owner);
                split += 1;
            }
        }
    }
}
