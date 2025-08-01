using System;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class QueenBeenade : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Beenade}";

        private const int TimeLeft = 180;

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 14;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TimeLeft;
        }

        public override void AI()
        {
            if (Projectile.ai[0] > 5f)
            {
                if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f)
                {
                    Projectile.velocity.X *= 0.97f;
                    if (Math.Abs(Projectile.velocity.X) < 0.01f)
                    {
                        Projectile.velocity.X = 0f;
                        Projectile.netUpdate = true;
                    }
                }

                Projectile.velocity.Y += 0.2f;
            }
            else
                Projectile.ai[0] += 1f;

            Projectile.rotation += Projectile.velocity.X * 0.1f;

            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        // Using 1 just in case
        public override bool? CanDamage() => Projectile.timeLeft <= 1;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = oldVelocity.X * -0.5f;

            if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 1f)
                Projectile.velocity.Y = oldVelocity.Y * -0.5f;

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            int timer = TimeLeft - Projectile.timeLeft;
            float startWarningColorGateValue = 60f;
            float timeToReachFullIntensity = 120f;
            float timeToReachExplosion = startWarningColorGateValue + timeToReachFullIntensity;
            Color initialColor = lightColor;
            Color finalColor = Color.Lerp(new Color(125, 0, 0), Color.Red, (float)Math.Abs(Math.Sin((timer - startWarningColorGateValue) * (MathHelper.Pi / 45f))));
            finalColor.A = (byte)(255 - Projectile.alpha);
            if (timer > startWarningColorGateValue)
            {
                float colorTransitionRatio = (timer - startWarningColorGateValue) / timeToReachFullIntensity;
                Color explodeColor = Color.Lerp(initialColor, finalColor, colorTransitionRatio);
                return explodeColor;
            }
            else
                return initialColor;
        }

        public override void OnKill(int timeLeft)
        {
            int newSize = 40;
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = newSize;
            Projectile.Center = Projectile.position;
            Projectile.Damage();

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            for (int i = 0; i < 20; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 31, 0f, 0f, 100, default, 1.5f);

            if (!Main.dedServ)
            {
                int gore = Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity.X += 1f;
                Main.gore[gore].velocity.Y += 1f;
                Main.gore[gore].velocity *= 0.3f;
                gore = Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity.X -= 1f;
                Main.gore[gore].velocity.Y += 1f;
                Main.gore[gore].velocity *= 0.3f;
                gore = Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity.X += 1f;
                Main.gore[gore].velocity.Y -= 1f;
                Main.gore[gore].velocity *= 0.3f;
                gore = Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity.X -= 1f;
                Main.gore[gore].velocity.Y -= 1f;
                Main.gore[gore].velocity *= 0.3f;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                int beeAmount = Main.rand.Next(5, 8);
                int type = ModContent.ProjectileType<QueenBeenadeBee>();
                float minVelocity = 8f;
                float maxVelocity = 16f;
                for (int i = 0; i < beeAmount; i++)
                {
                    float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
                    float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(speedX, speedY), type, QueenBeeAI.BeenadeBeeDamage, 0f, Main.myPlayer, 0f, minVelocity, maxVelocity);
                }
            }
        }
    }
}
