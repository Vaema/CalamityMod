using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TriploonSpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public int TimeLeft = 1200;
        public bool HasDealtRipOutDamage = false;
        public ref float Returning => ref Projectile.ai[2]; // If this equals 1f, it's returning. This is a float referencing ai2 instead of a bool because I change it in the holdout

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TimeLeft;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Vector2 ownerDist = Main.player[Projectile.owner].Center - Projectile.Center;

            // Set the initial hit delay offset
            if (Projectile.ai[0] > 1f)
            {
                Projectile.localNPCHitCooldown += (int)Projectile.ai[0];
                Projectile.ai[0] = 0f;
            }

            if (Returning == 1f) // Returning
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                Projectile.usesIDStaticNPCImmunity = true; // Abuse of iframe shenanigans to make sure it consistently hits when ripping out
                Projectile.idStaticNPCHitCooldown = 1;

                float returnSpeed = 22f;
                if (ownerDist.Length() > 3000f)
                    Projectile.Kill();

                ownerDist.Normalize();
                ownerDist *= returnSpeed;

                // Home back in on owner
                if (Projectile.velocity.X < ownerDist.X)
                    Projectile.velocity.X = ownerDist.X;
                else if (Projectile.velocity.X > ownerDist.X)
                    Projectile.velocity.X = ownerDist.X;

                if (Projectile.velocity.Y < ownerDist.Y)
                    Projectile.velocity.Y = ownerDist.Y;
                else if (Projectile.velocity.Y > ownerDist.Y)
                    Projectile.velocity.Y = ownerDist.Y;

                // Delete the harpoon if it touches its owner
                if (Main.myPlayer == Projectile.owner)
                {
                    if (Projectile.Hitbox.Intersects(Main.player[Projectile.owner].Hitbox))
                        Projectile.Kill();
                }
            }
            else // Not returning
            {
                // Only rotates before hitting an enemy
                if (Projectile.numHits < 1)
                    Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.StickyProjAI(360);

                // Return if too far from owner
                if (ownerDist.Length() > 1000f)
                    Returning = 1f;

                // Gravity
                if (Projectile.numHits < 1)
                {
                    if (Projectile.timeLeft < TimeLeft - 30)
                    {
                        Projectile.velocity.X *= 0.98f;
                        Projectile.velocity.Y += 0.15f;
                        if (Projectile.velocity.Y > 14f)
                            Projectile.velocity.Y = 14f;
                    }
                }
            }
        }

        // The spear stops dealing damage after the rip-out damage
        public override bool? CanDamage() => Returning != 1f || !HasDealtRipOutDamage;

        // Making sure it hits consistently when ripping out, part 2
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (Returning == 1f)
                hitbox.Inflate(20, 20);
        }

        // Immediately start returning if it hits a tile
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Returning = 1f;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 120);

            if (Returning != 1f)
            {
                for (int blood = 0; blood < 6; blood++)
                {
                    Vector2 bleedVelocity = Vector2.UnitX.RotatedBy(Projectile.rotation + MathHelper.Pi).RotatedByRandom(MathHelper.PiOver4);
                    bleedVelocity *= Main.rand.NextFloat(15f, 19f);

                    Particle impaleBlood = new BloodParticle(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 22f, bleedVelocity, 15, 0.7f, Color.Maroon);
                    GeneralParticleHandler.SpawnParticle(impaleBlood);
                }
            }
            else
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/NPCHit/PerfLargeHit", 3));
                HasDealtRipOutDamage = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.ModifyHitNPCSticky(3);
            if (Returning == 1f)
            {
                modifiers.SetCrit();
                float damageMult = 1f + (Main.player[Projectile.owner].velocity.Length() / 4f); // Deals more damage on rip-out based on the player's velocity
                Main.NewText(damageMult);
                modifiers.SourceDamage *= damageMult;

                for (int bloody = 0; bloody < 2; bloody++)
                {
                    Vector2 ripOutBleedVelocity = Vector2.UnitX.RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.PiOver4);
                    ripOutBleedVelocity *= Main.rand.NextFloat(1.25f, 2f);
                    float randomSize = Main.rand.NextFloat(0.5f, 0.7f);
                    Color ripOutBloodColor = Color.Lerp(Color.Maroon, Color.Red, damageMult / 4f);

                    Particle ripOutBlood = new BloodParticle2(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 22f, ripOutBleedVelocity, 20, randomSize, ripOutBloodColor);
                    GeneralParticleHandler.SpawnParticle(ripOutBlood);
                }
            }
        }
    }
}
