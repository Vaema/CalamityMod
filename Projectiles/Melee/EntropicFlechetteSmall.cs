using CalamityMod.Buffs.Potions;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class EntropicFlechetteSmall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public float sizeVariance = 2;
        public int time = 60;
        public int spinDir = 100;
        public int waveOften = 30;
        public override void SetDefaults()
        {
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 450;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (spinDir == 100)
            {
                spinDir = Main.rand.NextBool() ? 1 : -1;
                waveOften = Main.rand.Next(15, 30 + 1);
            }

            if (time % 18 == 0)
                sizeVariance = Main.rand.NextFloat(1.5f, 2.5f) * 0.7f;
            Projectile.scale = MathHelper.Lerp(Projectile.scale, sizeVariance, 0.09f);
            if (time % 2 == 0 && time > 5)
            {
                Particle spark = new AltSparkParticle(Projectile.Center, -Projectile.velocity * 0.3f, false, 6, 0.7f, Color.Black);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? 278 : 263, -Projectile.velocity);
                dust.scale = dust.type == 278 ? Main.rand.NextFloat(0.3f, 0.6f) : Main.rand.NextFloat(0.6f, 1.4f);
                dust.velocity = -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.7f);
                dust.noGravity = true;
                dust.color = Color.LightGreen;
            }
            if (time >= 5)
            {
                NPC target = Projectile.Center.ClosestNPCAt(250);

                if (target == null)
                {
                    Vector2 moveToMouse = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    if (Projectile.velocity.Length() < 12)
                        Projectile.velocity += moveToMouse * 0.2f;
                    else
                        Projectile.velocity *= 0.9f;

                    if (time % waveOften == 0)
                        spinDir *= -1;

                    Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(0.05f, 0.15f) * spinDir * Utils.GetLerpValue(60, 180, time, true));
                }
                else
                {
                    Vector2 moveToEnemy = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    if (Projectile.velocity.Length() < 12)
                        Projectile.velocity += moveToEnemy * 0.8f;
                    else
                        Projectile.velocity *= 0.7f;
                }
            }
            time++;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 9; i++)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustStyle, Projectile.velocity);
                dust.scale = Main.rand.NextFloat(0.5f, 1.2f);
                dust.velocity = Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 2.1f);
                dust.noGravity = true;
                dust.color = Color.LightGreen;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/WaterFlavored");

            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(tex.Value, generalDrawPos, null, Color.Black, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.4f, 1) * Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(tex.Value, generalDrawPos, null, Color.LightGreen with { A = 0 }, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.4f, 1) * Projectile.scale * 0.7f, SpriteEffects.None);
            Main.EntitySpriteDraw(tex.Value, generalDrawPos, null, Color.LightGreen with { A = 0 }, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.4f, 1) * Projectile.scale * 0.5f, SpriteEffects.None);
            return false;
        }
    }
}
