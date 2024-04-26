using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ElementalSawLingering : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/ElementalSawProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 210;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            // dies from cringe (Deadshot Brooch moment)
            if (Projectile.MaxUpdates > 1)
                Projectile.MaxUpdates = 1;

            // Timer and rotation
            Projectile.ai[1]++;
            Projectile.rotation = Projectile.ai[1] * Projectile.spriteDirection * (MathHelper.Pi / 6);

            // Make it lose velocity as it travels
            Projectile.velocity *= 0.955f;

            // Continously spawn homing bolts and small saws
            if (Projectile.ai[1] % 7 == 0 && Projectile.ai[1] > 30)
            {
                Vector2 randVelocity = Main.rand.NextVector2CircularEdge(1f, 1f);
                randVelocity.SafeNormalize(Vector2.Zero);
                randVelocity *= Main.rand.NextFloat(6f, 7.5f);
                if (Main.myPlayer == Projectile.owner)
                {
                    int projType = Main.rand.NextBool() ? ModContent.ProjectileType<ElementalSawMini>() : ModContent.ProjectileType<ElementalSawBullet>();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randVelocity, projType, (int)(Projectile.damage * 0.5f), 0f, Main.myPlayer);
                }
            }

            // Fade out at the end of its lifetime
            if (Projectile.timeLeft <= 30)
            {
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                    Projectile.Kill();
            }
        }

        public override bool? CanDamage() => Projectile.timeLeft > 30;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 180);
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 90);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SwiftSlice"), Projectile.Center);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/CeramicImpact", 2), Projectile.Center);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox) => hitbox.Inflate(70, 70);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D largeSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawLargeSlash").Value;
            Color drawColor = new Color(200f, 200f, 200f, 100f * ((255 - Projectile.alpha) / 255));
            Main.EntitySpriteDraw(largeSlashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), null, drawColor, -(Projectile.ai[1] * 7f), largeSlashTexture.Size() / 2, 1f, SpriteEffects.None);

            if (Projectile.ai[1] % 4 == 0)
            {
                Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f), Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f));
                float randomParticleScale = Main.rand.NextFloat(0.65f, 0.95f);
                Color bloomColor = Color.Lerp(new Color(29, 120, 30), new Color(56, 255, 59), (float)Math.Abs(Math.Sin(Projectile.ai[1])));
                Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : bloomColor, randomParticleScale, randomParticleScale, 4, false);
                GeneralParticleHandler.SpawnParticle(bloomCircle);
            }
            Texture2D smallSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawSmallSlash").Value;
            Color drawColorSmall = new Color(200f, 200f, 200f, 100f * ((255f - Projectile.alpha) / 255f));
            Main.EntitySpriteDraw(smallSlashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), null, drawColorSmall, Projectile.ai[1] * 7f, smallSlashTexture.Size() / 2, 1f, SpriteEffects.None);

            if (Projectile.ai[1] % 4 == 0)
            {
                Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width, Projectile.width), Main.rand.NextFloat(-Projectile.width, Projectile.width));
                float randomParticleScale = Main.rand.NextFloat(0.35f, 0.65f);
                Color bloomColor = Color.Lerp(new Color(29, 120, 30), new Color(56, 255, 59), (float)Math.Abs(Math.Sin(Projectile.ai[1])));
                Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : bloomColor, randomParticleScale, randomParticleScale, 4, false);
                GeneralParticleHandler.SpawnParticle(bloomCircle);
            }

            Texture2D outline = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawProjOutline").Value;
            Main.EntitySpriteDraw(outline, Projectile.Center - Main.screenPosition, null, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), Projectile.rotation, outline.Size() / 2, 1f, SpriteEffects.None);
            return true;
        }
    }
}
