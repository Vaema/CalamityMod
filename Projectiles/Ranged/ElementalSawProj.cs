using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Weapons.Ranged;
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
    public class ElementalSawProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public static readonly SoundStyle TileCollideGFB = new("CalamityMod/Sounds/Custom/MetalPipeFalling") { Volume = 2f };

        // Controls if the saw is returning to the player.
        public bool Returning = false;
        public int ReturnTimer = 0;

        // Whether the saw is empowered by right click.
        public bool Empowered = false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1; // Saws only pierce a certain number of times before returning, and don't deal direct damage while returning
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

            // Control the saw being empowered or not
            Player Owner = Main.player[Projectile.owner];
            if (Owner.HasCooldown(ElementalSawBoost.ID))
                Empowered = true;
            else
                Empowered = false;


            // Saws automatically return 2 seconds after hitting an enemy
            if (ReturnTimer > 0 && ReturnTimer < 90)
            {
                ReturnTimer++;
                if (ReturnTimer == 90)
                    Returning = true;
            }

            if (Returning)
            {
                Projectile.tileCollide = false;
                if (ReturnTimer < 90)
                    ReturnTimer = 90;

                ReturnTimer++;
                if (ReturnTimer < 120)
                    Projectile.velocity *= 0.95f;
                else
                {
                    float returnSpeed = (ElementalSaw.ShootSpeed * 0.66f) + (0.05f * (ReturnTimer - 120));
                    Vector2 ownerDist = Owner.Center - Projectile.Center;
                    if (ownerDist.Length() > 3000f)
                        Projectile.Kill();

                    ownerDist.Normalize();
                    ownerDist *= returnSpeed;

                    // Home back in on the player.
                    if (Projectile.velocity.X < ownerDist.X)
                        Projectile.velocity.X = ownerDist.X;
                    else if (Projectile.velocity.X > ownerDist.X)
                        Projectile.velocity.X = ownerDist.X;

                    if (Projectile.velocity.Y < ownerDist.Y)
                        Projectile.velocity.Y = ownerDist.Y;
                    else if (Projectile.velocity.Y > ownerDist.Y)
                        Projectile.velocity.Y = ownerDist.Y;

                    // Delete the saw if it touches its owner.
                    if (Main.myPlayer == Projectile.owner)
                    {
                        if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                            Projectile.Kill();
                    }

                    // Spawn homing bolts as it returns. These bolts spawn more often while empowered.
                    int boltFrequency = Empowered ? 7 : 10;
                    if (ReturnTimer % boltFrequency == 0)
                    {
                        Vector2 randVelocity = -Projectile.velocity.RotatedByRandom(MathHelper.PiOver2) / 2;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randVelocity, ModContent.ProjectileType<ElementalSawBullet>(), (int)(Projectile.damage * 0.5f), 0f, Main.myPlayer);
                    }
                }
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

                Particle collisionSparks = new AltLineParticle(sparkLocation, sparkVelocity, false, 30, 0.6f, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB));
                GeneralParticleHandler.SpawnParticle(collisionSparks);
            }

            SoundEngine.PlaySound(Main.zenithWorld ? TileCollideGFB : SoundID.Item178, Projectile.Center); // Placeholder sound
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            Projectile.ai[2]--;
            if (Projectile.ai[2] <= 0)
            {
                Returning = true;
            }

            return false;
        }

        public override bool? CanDamage() => !(Returning && Projectile.ai[2] == 0);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 180);
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 90);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SwiftSlice"), Projectile.Center);

            if (Projectile.numHits < 1)
                ReturnTimer = 1;

            Projectile.ai[2]--;
            if (Projectile.ai[2] <= 0)
                Returning = true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/CeramicImpact", 2), Projectile.Center);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (Projectile.ai[0] == 2f)
                hitbox.Inflate(70, 70);
            else if (Projectile.ai[0] == 1f)
                hitbox.Inflate(32, 32);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] >= 2f) // Large slash draw
            {
                Texture2D largeSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawLargeSlash").Value;
                Color drawColor = new Color(200, 200, 200, 100);
                Main.EntitySpriteDraw(largeSlashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), null, drawColor, -(Projectile.ai[1] * 7f), largeSlashTexture.Size() / 2, 1f, SpriteEffects.None);

                if (Projectile.ai[1] % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f), Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f));
                    float randomParticleScale = Main.rand.NextFloat(0.65f, 0.95f);
                    Color bloomColor = Color.Lerp(new Color(29, 120, 30), new Color(56, 255, 59), (float)Math.Abs(Math.Sin(Projectile.ai[1])));
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : bloomColor, randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }
            if (Projectile.ai[0] >= 1f) // Small slash draw
            {
                Texture2D smallSlashTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawSmallSlash").Value;
                Color drawColor = new Color(200, 200, 200, 100);
                Main.EntitySpriteDraw(smallSlashTexture, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), null, drawColor, Projectile.ai[1] * 7f, smallSlashTexture.Size() / 2, 1f, SpriteEffects.None);

                if (Projectile.ai[1] % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width, Projectile.width), Main.rand.NextFloat(-Projectile.width, Projectile.width));
                    float randomParticleScale = Main.rand.NextFloat(0.35f, 0.65f);
                    Color bloomColor = Color.Lerp(new Color(29, 120, 30), new Color(56, 255, 59), (float)Math.Abs(Math.Sin(Projectile.ai[1])));
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : bloomColor, randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }

            if (Empowered) // Rainbow outline while empowered
            {
                Texture2D outline = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawProjOutline").Value;
                Main.EntitySpriteDraw(outline, Projectile.Center - Main.screenPosition, null, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), Projectile.rotation, outline.Size() / 2, 1f, SpriteEffects.None);
            }
            return true;
        }
    }
}
