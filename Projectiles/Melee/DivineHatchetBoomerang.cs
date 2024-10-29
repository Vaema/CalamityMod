using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class DivineHatchetBoomerang : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/SeekingScorcher";

        private static int Lifetime = 2000; //Abnormally long lifetime due to how the weapon functions
        public int Chargetime = 0;
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = Lifetime;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.MaxUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            //In flight effects
            if (Main.rand.NextBool(8))
            {
                Particle mark = new CustomSpark(Projectile.Center + Main.rand.NextVector2Circular(80, 80), -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 30, Main.rand.NextFloat(0.7f, 0.9f), Main.rand.NextBool() ? Color.Orchid : Color.OrangeRed, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.1f, 0.2f));
                GeneralParticleHandler.SpawnParticle(mark);
            }
            if (Main.rand.NextBool())
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.CopperCoin, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }
            // Boomerang rotation
            Projectile.rotation += Projectile.direction * 0.4f;
            //Only begin homing after a second has passed, to allow the projectile to be aimed away from targets
            if (Projectile.timeLeft < Lifetime - 60)
            {
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 5000f, 12f, 12f);
            }
            //Charge is capped at 175, so we only increment the counter when it is less than that
            if (Chargetime < 175)
            {
                Chargetime++;
            }
        }
        // Glowmask
        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 200);
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //These effects happen regardless of charge
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);
            SoundStyle explode = new("CalamityMod/Sounds/Custom/Providence/ProvidenceHolyBlastImpact");
            SoundEngine.PlaySound(explode with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.3f, 0.1f) }, Projectile.Center);
            //These effects only happen when the projectile is not at max damage
            if (Chargetime <= 174)
            {
                Particle orb1 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/SoftRoundExplosion", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 0.14f, 20, true);
                GeneralParticleHandler.SpawnParticle(orb1);
                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkOrange, "CalamityMod/Particles/BloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 2.1f, 20);
                GeneralParticleHandler.SpawnParticle(orb2);
                for (int i = 0; i < 14; i++)
                {
                    Particle mark = new CustomSpark(Projectile.Center, (new Vector2(19, 19).RotatedByRandom(100)) * Main.rand.NextFloat(0.2f, 0.6f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 30, Main.rand.NextFloat(1.15f, 1.3f), Main.rand.NextBool(4) ? Color.Orchid : Color.Orange, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.1f, 0.2f));
                    GeneralParticleHandler.SpawnParticle(mark);
                }
                for (int i = 0; i < 18; i++)
                {
                    Vector2 sparkVelocity = Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(0.6f, 0.9f);
                    int sparkLifetime = Main.rand.Next(23, 25);
                    float sparkScale = Main.rand.NextFloat(0.8f, 1f) * 0.955f;
                    SparkParticle spark = new SparkParticle(Projectile.Center, sparkVelocity, false, sparkLifetime, sparkScale, Main.rand.NextBool() ? Color.Gold : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            //These effects only happen when the projectile is at max damage
            if (Chargetime == 175)
            {
                //The explosion is not modified by the weapon's charge multiplier, and only uses base damage
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ScorcherExplosion>(), Projectile.damage, 6f, Projectile.owner);
                SoundStyle fullimpact = new("CalamityMod/Sounds/Item/BlazingCoreParry");
                SoundEngine.PlaySound(fullimpact with { Volume = 0.4f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
                Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.LightPink, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.65f + 0.1f, 25);
                GeneralParticleHandler.SpawnParticle(explosion);
                Particle explosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumVioletRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.2f, 30);
                GeneralParticleHandler.SpawnParticle(explosion2);
                Particle explosion3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.2f, 30);
                GeneralParticleHandler.SpawnParticle(explosion3);
                Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 1f, 6f, 25, true);
                GeneralParticleHandler.SpawnParticle(blastRing);
                Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/FlameExplosion", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 0.2f, 25, true, 0.9f);
                GeneralParticleHandler.SpawnParticle(blastRing2);
                for (int i = 0; i < 14; i++)
                {
                    Particle spark = new CustomSpark(Projectile.Center, new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 30, Main.rand.NextFloat(2.45f, 2.7f), Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f)), new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.35f, 0.4f));
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int i = 0; i < 24; i++)
                {
                    CritSpark spark2 = new CritSpark(Projectile.Center, new Vector2(40, 40).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f), Color.White, Color.Orchid, 0.9f, 35, 2f, 2.2f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                for (int i = 0; i < 2; i++)
                {
                    Particle blast = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.3f, 1.5f, 25, true);
                    GeneralParticleHandler.SpawnParticle(blast);
                }
                for (int i = 0; i < 2; i++)
                {
                    Particle blast2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.2f, 1.1f, 25, true);
                    GeneralParticleHandler.SpawnParticle(blast2);
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //Multiply the damage by how long the projectile has been in the air. If in the air for less than a second, do not multiply the damage as it results in very low numbers
            if (Projectile.timeLeft < Lifetime - 60)
            {
                modifiers.SourceDamage *= (Chargetime / 50);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            //Gain a glow based on how high the charge timer is
            float fade = Utils.GetLerpValue(0, Owner.itemAnimationMax, Chargetime, true);
            for (int i = 0; i < 10; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 5 * fade;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, Color.OrangeRed with { A = 0 } * fade, drawRotation, rotationPoint, Projectile.scale, flipSprite, 0f);
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
