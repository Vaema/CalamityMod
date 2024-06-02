using System.IO;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ChronoClock : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;

        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.98f;
            Player player = Main.player[Projectile.owner];
            // if the player touches a clock, they get the Haste buff and their haste level increases to a maximum of 3
            // this kills the clock
            if (player.getRect().Intersects(Projectile.getRect()))
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    player.AddBuff(ModContent.BuffType<Haste>(), 60);
                    if (player.Calamity().hasteLevel < 3)
                    {
                        player.Calamity().hasteLevel++;
                        player.Calamity().hasteCounter = 0; // reset the haste countdown
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 2 }, Projectile.Center);
                        for (int i = 0; i < 20; i++)
                        {
                            Particle sparkle = new SnowflakeSparkle(Projectile.Center, (Main.rand.NextVector2CircularEdge(100, 100)).SafeNormalize(Vector2.Zero) * 4, Color.LightBlue, Color.LightSkyBlue, Main.rand.NextFloat(0.1f, 0.6f), 60);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                    else
                    {
                        // reset the haste countdown if haste level is maxed out
                        player.Calamity().hasteCounter = 0;
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 0.2f, Pitch = 1.4f }, Projectile.Center);
                        for (int i = 0; i < 20; i++)
                        {
                            Particle sparkle = new SnowflakeSparkle(Projectile.Center, (Main.rand.NextVector2CircularEdge(100, 100)).SafeNormalize(Vector2.Zero) * 2, Color.LightBlue, Color.LightSkyBlue, Main.rand.NextFloat(0.04f, 0.3f), 30);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                    Projectile.Kill();
                }
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame % 2 == 0)
                    Projectile.netUpdate = true;
            }
            if (Projectile.frame > 7)
            {
                Projectile.frame = 0;
            }
            if (Main.rand.NextBool(5))
            {
                int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Snow);
                Main.dust[index2].noGravity = true;
                Main.dust[index2].noLight = true;
                Main.dust[index2].scale = 0.7f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int index1 = 0; index1 < 3; ++index1)
            {
                int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Snow);
                Main.dust[index2].noGravity = true;
                Main.dust[index2].noLight = true;
                Main.dust[index2].scale = 0.7f;
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
    }
}
