using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class BuzzkillSaw : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public static readonly SoundStyle TileCollideGFB = new("CalamityMod/Sounds/Custom/MetalPipeFalling") { Volume = 1.5f };

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 1; // Saw pierce is set when the saw is spawned, due to it being dynamic based on charge.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            if (Projectile.ai[1] == 1f)
                Projectile.rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);

            if (Projectile.frame < 1)
                Projectile.frame = 1;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 1;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int s = 0; s < 7; s++)
            {
                Vector2 sparkVelocity = new Vector2();
                if (Projectile.velocity.X != oldVelocity.X && oldVelocity.X < 0)
                    sparkVelocity = new Vector2(6.5f, 0f);
                else if (Projectile.velocity.X != oldVelocity.X && oldVelocity.X >= 0)
                    sparkVelocity = new Vector2(-6.5f, 0f);
                else if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y < 0)
                    sparkVelocity = new Vector2(0f, 6.5f);
                else if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y >= 0)
                    sparkVelocity = new Vector2(0f, -6.5f);

                Vector2 sparkLocation = sparkVelocity.X > 0f ? new Vector2(Projectile.Center.X - Projectile.width / 2, Projectile.Center.Y) : (sparkVelocity.X < 0f ? new Vector2(Projectile.Center.X + Projectile.width / 2, Projectile.Center.Y) : (sparkVelocity.Y > 0f ? new Vector2(Projectile.Center.X, Projectile.Center.Y - Projectile.height / 2) : new Vector2(Projectile.Center.X, Projectile.Center.Y + Projectile.height / 2)));
                sparkVelocity = sparkVelocity.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2));

                Particle collisionSparks = new AltLineParticle(sparkLocation, sparkVelocity, false, 30, 0.6f, new Color(250, 250, 107));
                GeneralParticleHandler.SpawnParticle(collisionSparks);
            }

            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                SoundEngine.PlaySound(Main.zenithWorld ? TileCollideGFB : SoundID.Item178, Projectile.Center); // Placeholder sound

                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 150);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SwiftSlice"), Projectile.Center);

            for (int p = 0; p < 6; p++)
            {
                Particle hitSparks = new AltLineParticle(target.Center, new Vector2(Main.rand.NextFloat(-6.5f, 6.5f), Main.rand.NextFloat(-6.5f, 6.5f)), false, 30, 0.6f, new Color(112, 16, 16));
                GeneralParticleHandler.SpawnParticle(hitSparks);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/CeramicImpact", 2), Projectile.Center);

            // TODO - Change this dust
            for (int d = 0; d < 8; d++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                dustVel.SafeNormalize(Vector2.Zero);
                dustVel *= Main.rand.NextFloat(5f, 9f);

                Dust collisionDust = Dust.NewDustPerfect(Projectile.Center, 84, dustVel);
                collisionDust.noGravity = true;
            }

            int goreToExclude = Main.rand.Next(3);
            switch (goreToExclude)
            {
                case 0:
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw2").Type, 0.8f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw3").Type, 0.8f);
                    break;
                case 1:
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw1").Type, 0.8f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw3").Type, 0.8f);
                    break;
                case 2:
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw1").Type, 0.8f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)), Mod.Find<ModGore>("BuzzkillSaw2").Type, 0.8f);
                    break;
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (Projectile.ai[0] == 2f)
                hitbox.Inflate(65, 65);
            else if (Projectile.ai[0] == 1f)
                hitbox.Inflate(28, 28);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] >= 2f)
            {
                Texture2D largeSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawLargeSlash").Value;
                Color drawColor = new Color(200, 200, 200, 100);
                Main.EntitySpriteDraw(largeSlashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), null, drawColor, -(Projectile.ai[1] * 7f), largeSlashTexture.Size() / 2, 1f, SpriteEffects.None);

                if (Projectile.ai[1] % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f), Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f));
                    float randomParticleScale = Main.rand.NextFloat(0.65f, 0.95f);
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : new Color(112, 16, 16), randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }
            if (Projectile.ai[0] >= 1f)
            {
                Texture2D smallSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BuzzkillSawSmallSlash").Value;
                Color drawColor = new Color(200, 200, 200, 100);
                Main.EntitySpriteDraw(smallSlashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), null, drawColor, Projectile.ai[1] * 7f, smallSlashTexture.Size() / 2, 1f, SpriteEffects.None);

                if (Projectile.ai[1] % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width, Projectile.width), Main.rand.NextFloat(-Projectile.width, Projectile.width));
                    float randomParticleScale = Main.rand.NextFloat(0.35f, 0.65f);
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : new Color(112, 16, 16), randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }
            return true;
        }
    }
}
