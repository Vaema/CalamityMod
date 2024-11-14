using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ImmolationArrow : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public Color mainColor = Color.Lerp(Color.Chartreuse, Color.White, 0.35f);

        public NPC chosenTarget;
        public bool stuckInTarget = false;
        public bool stuckInGround = false;
        public bool canDamage = true;
        public bool canStick = true;
        public int stuckTimer = 90;
        public Vector2 placementCenter;
        float placementDistance;
        Vector2 placementVelocity;
        public Vector2 storedVelocity;
        public bool collideWithTiles = true;
        public Vector2 startingVel;

        NPC closestTarget;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 2;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (closestTarget != null && closestTarget.life <= 0)
                closestTarget = null;

            if (time == 0)
            {
                startingVel = Projectile.velocity;
                closestTarget = (Projectile.Center + Projectile.velocity * 2).ClosestNPCAt(150);
            }
            else
            {
                NPC attemptGetTarget = (Projectile.Center + Projectile.velocity * 2).ClosestNPCAt(150);
                    if (attemptGetTarget != null)
                    closestTarget = attemptGetTarget;
            }

            if (stuckInGround)
            {
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.PiOver2;
                stuckTimer--;

                if (stuckTimer <= 0)
                {
                    Projectile.Kill();
                }
            }
            if (!stuckInTarget && !stuckInGround)
            {
                storedVelocity = Projectile.velocity;
                Projectile.rotation = (storedVelocity).ToRotation() + MathHelper.PiOver2;
                if (time > 5 && !stuckInTarget)
                {
                    if (Main.rand.NextBool(3) && canStick)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? 278 : 263, -Projectile.velocity);
                        dust.scale = dust.type == 278 ? Main.rand.NextFloat(0.3f, 0.6f) : Main.rand.NextFloat(0.6f, 1.4f);
                        dust.velocity = -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.7f);
                        dust.noGravity = true;
                        dust.color = mainColor;
                    }
                    
                    if (targetDist < 1400f)
                    {
                        Particle spark = new SparkParticle(Projectile.Center, -Projectile.velocity, false, 6, 1.3f, mainColor * 0.7f);
                        GeneralParticleHandler.SpawnParticle(spark);

                        if (Main.rand.NextBool(6))
                        {
                            Vector2 placement = Projectile.Center + Main.rand.NextVector2Circular(12, 12);
                            float speed = Main.rand.NextFloat(0.2f, 0.7f);
                            Particle spark2 = new GlowOrbParticle(placement, -Projectile.velocity * speed, false, 7, Main.rand.NextFloat(0.4f, 0.7f), mainColor);
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                    }
                    if (closestTarget != null && Projectile.numHits < 1 && closestTarget.CanBeChasedBy(Projectile))
                    {
                        Vector2 moveTotarget = (closestTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                        if (Projectile.velocity.Length() < 20)
                            Projectile.velocity = Projectile.velocity * 0.95f + moveTotarget * 0.28f;
                        else
                            Projectile.velocity *= 0.8f;
                    }
                }
            }
            else if (stuckInTarget)
            {
                Projectile.rotation = (storedVelocity).SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver2;

                placementCenter = chosenTarget.Center + placementVelocity * placementDistance;

                Projectile.Center = placementCenter;

                stuckTimer--;

                if (chosenTarget.life <= 0)
                    stuckTimer = 0;

                if (stuckTimer <= 0)
                {
                    Projectile.Kill();
                }
            }
            if (stuckInTarget || stuckInGround)
            {
                if (Main.rand.NextBool(8))
                { 
                    float speed = Main.rand.NextFloat(0.2f, 1.5f);
                    Particle spark = new SparkParticle(Projectile.Center, -storedVelocity * speed, false, 23, 0.7f * speed, mainColor * 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? 278 : 263, -Projectile.velocity);
                    dust.scale = dust.type == 278 ? Main.rand.NextFloat(0.3f, 0.6f) : Main.rand.NextFloat(0.6f, 1.4f);
                    dust.velocity = (new Vector2(35, 35).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.7f)) * Utils.GetLerpValue(90, 0, stuckTimer);
                    dust.noGravity = true;
                    dust.color = mainColor;
                }
            }
            time++;

            if (collideWithTiles && Collision.SolidCollision(Projectile.Center, 4, 4)) // Vanilla tile collision messes with velocity so we use this instead
            {
                canDamage = false;
                stuckInGround = true;

                storedVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                SoundStyle sound = new("CalamityMod/Sounds/NPCHit/RavagerRockPillarHit1");
                SoundEngine.PlaySound(sound with { Volume = 0.25f, Pitch = Main.rand.NextFloat(-0.3f, -0.6f) }, Projectile.Center);
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.PiOver2;
                collideWithTiles = false;
                SoundStyle sound2 = new("CalamityMod/Sounds/Item/ImmolatorPreExplode");
                SoundEngine.PlaySound(sound2 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
                SoundStyle sound3 = new("CalamityMod/Sounds/Item/PlasmaSmall");
                SoundEngine.PlaySound(sound3 with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.3f, -0.4f) }, Projectile.Center);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.88f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;

            if (!stuckInTarget && canStick)
            {
                if (Projectile.timeLeft < 600)
                    Projectile.timeLeft = 600;
                collideWithTiles = false;
                canDamage = false;
                placementDistance = -Vector2.Distance(target.Center, Projectile.Center);
                placementVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                placementCenter = placementVelocity * (placementDistance * 0.01f);
                chosenTarget = target;
                stuckInTarget = true;
                storedVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                for (int i = 0; i <= 11; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + storedVelocity.SafeNormalize(Vector2.UnitX) * 38 + Main.rand.NextVector2Circular(12, 12), dustStyle, Projectile.velocity);
                    dust.scale = Main.rand.NextFloat(0.7f, 1.1f);
                    dust.velocity = storedVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.8f, 2.1f);
                    dust.noGravity = true;
                    dust.color = mainColor;
                }
                SoundStyle sound = new("CalamityMod/Sounds/Item/ImmolatorPreExplode");
                SoundEngine.PlaySound(sound with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
                SoundStyle sound2 = new("CalamityMod/Sounds/Item/PlasmaSmall");
                SoundEngine.PlaySound(sound2 with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.3f, -0.4f) }, Projectile.Center);
            }
        }
        public override void OnKill(int timeLeft)
        {
            float bonus = (stuckInGround ? 2f : 1);
            float explosionDamage = (stuckInGround ? 2.3f : 0.5f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ImmolationBurst>(), (int)(Projectile.damage * explosionDamage), Projectile.knockBack * 2, Projectile.owner, 0, bonus != 1 ? 1 : 0);
            Particle bolt2 = new CustomPulse(Projectile.Center, Vector2.Zero, mainColor * 0.75f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 1.2f * bonus, 26);
            GeneralParticleHandler.SpawnParticle(bolt2);

            Particle bolt3 = new CustomPulse(Projectile.Center, Vector2.Zero, mainColor * 0.75f, "CalamityMod/Particles/PlasmaExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.07f * bonus, 14);
            GeneralParticleHandler.SpawnParticle(bolt3);

            SoundStyle sound = new("CalamityMod/Sounds/Item/PlasmaBig");
            SoundEngine.PlaySound(sound with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
        }
        public override bool? CanDamage() => canDamage ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 1)
                return false;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/DraedonsArsenal/ImmolationArrow");

            if (!stuckInGround && !stuckInTarget && false)
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], mainColor with { A = 0 }, 1, tex.Value);

            float randSize = Main.rand.NextFloat(0.7f, 1.2f);
            for (int i = 0; i < 3; i++)
            {
                Vector2 scale = Projectile.scale * new Vector2(0.5f * (1 - i * 0.25f), 1) * 1.5f * (1 - i * 0.3f) * randSize;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, mainColor with { A = 0 } * 0.5f, Projectile.rotation, tex.Size() * 0.5f, scale, SpriteEffects.None);
            }
            if (stuckInTarget || stuckInGround)
            {
                Vector2 scale = 1.1f * new Vector2(0.5f, 1) * 1.5f * randSize;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * Utils.GetLerpValue(90, 30, stuckTimer, true), Projectile.rotation, tex.Size() * 0.5f, scale, SpriteEffects.None);
            }
            return false;
        }
    }
}
