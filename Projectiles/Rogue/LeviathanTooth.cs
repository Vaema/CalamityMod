using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class LeviathanTooth : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public ref float time => ref Projectile.ai[0];

        // Vanilla sticky code is jank, So I did my own (for better or worse)
        public NPC chosenTarget;
        public bool stuckInTarget = false;
        public bool stuckInGround = false;
        public bool canDamage = true;
        public bool canStick = true;
        public Vector2 vibrate = Vector2.Zero;
        public int stuckTimer = 120;
        public Vector2 placementCenter;
        float placementDistance;
        Vector2 placementVelocity;
        public Vector2 storedVelocity;
        public bool collideWithTiles = true;

        public bool jawline = true; // Tooth direction for the stealthstrike
        public Vector2 startingVel;
        public bool toothDirection = false;
        public Vector2 chomp = Vector2.Zero;
        public bool chomping = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (time == 0)
            {
                stuckTimer = Main.rand.Next(100, 120 + 1);
                startingVel = Projectile.velocity;
                if (Projectile.ai[1] < 4)
                    toothDirection = Main.rand.NextBool();
            }
            if (!stuckInTarget && !stuckInGround)
            {
                if (!Projectile.Calamity().stealthStrike)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    if ((time % 2 == 0 || !canStick) && time > 15)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), 5, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.8f), 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                        dust.noGravity = true;
                    }
                    if (time > 90 && canStick)
                    {
                        Projectile.velocity.Y += 0.03f;
                        if (Projectile.velocity.Y > 0)
                            Projectile.velocity.X *= 0.99f;
                    }
                }
            }
            else if (stuckInTarget)
            {
                float power = 5 * Utils.GetLerpValue(120, 0, stuckTimer, true);
                vibrate = Main.rand.NextVector2Circular(power, power);

                Projectile.rotation = (storedVelocity).SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver2;

                Vector2 impaleVel = (storedVelocity * 0.5f) * Utils.GetLerpValue(120, 0, stuckTimer, true);
                placementCenter = chosenTarget.Center + placementVelocity * placementDistance + impaleVel;

                Projectile.Center = placementCenter;

                stuckTimer--;
                if (chosenTarget.life <= 0 || chosenTarget == null)
                    stuckTimer = 0;
                if (stuckTimer == 0)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;

                    canDamage = true;
                    vibrate = Vector2.Zero;
                    canStick = false;
                    collideWithTiles = true;
                    Projectile.velocity = storedVelocity * 1.3f;
                    stuckInTarget = false;

                    for (int i = 0; i <= 7; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, 5, (storedVelocity * 2.5f).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.9f, 1.8f));
                        dust.noGravity = false;
                    }
                    for (int i = 0; i <= 3; i++)
                    {
                        Particle spark = new AltSparkParticle(Projectile.Center, (storedVelocity * 4.5f).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.8f) + new Vector2(0, -2), true, 20, 0.5f, Color.DarkRed * 0.7f);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                    SoundStyle sound = new("CalamityMod/Sounds/NPCHit/PerfSmallHit", 3);
                    SoundEngine.PlaySound(sound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.2f, -0.3f) }, Projectile.Center);
                }
                    
            }
            if (stuckInGround)
            {
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.PiOver2;
            }

            if (Projectile.Calamity().stealthStrike)
            {
                collideWithTiles = false;
                if (time > 15 && (Projectile.ai[1] == 4 ? Projectile.timeLeft > 60 : time < 40))
                {
                    for (int i = 0; i < (Projectile.ai[1] == 4 ? 2 : 1); i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), 5, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.8f), 100, default, Main.rand.NextFloat(0.8f, 1.6f));
                        dust.noGravity = true;
                    }
                }
                if (Projectile.ai[1] == 4)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                    int jawDir = jawline ? 1 : -1;
                    Vector2 spawnPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.ToRadians(90f * jawDir)) * 170;
                    Vector2 jawVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.ToRadians(90f * jawDir)) * 17;
                    if (time > 10 && Projectile.timeLeft >= 60)
                    {
                        if (time % 3 == 0)
                        {
                            Projectile tooth = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnPos, jawVel, ModContent.ProjectileType<LeviathanTooth>(), Projectile.damage, Projectile.knockBack / 12, Projectile.owner, 0, 0, Projectile.velocity.X > 0 ? 1 : -1);
                            tooth.Calamity().stealthStrike = true;
                            tooth.timeLeft = 120;
                            tooth.extraUpdates = 0;
                            tooth.usesIDStaticNPCImmunity = true;
                            tooth.usesLocalNPCImmunity = false;
                            tooth.idStaticNPCHitCooldown = 5;
                            tooth.ai[1] = Main.rand.Next(1, 3 + 1);
                            jawline = !jawline;
                        }
                        if (time % 2 == 0)
                        {
                            Particle spark = new AltSparkParticle(Projectile.Center - Projectile.velocity * 0.5f, Projectile.velocity * 0.01f, false, 15, 1f, Color.DarkRed * 0.4f);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        if (Projectile.timeLeft == 60)
                            Projectile.velocity *= 0.5f;
                    }
                    else if (Projectile.timeLeft <= 60)
                    {
                        Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 0, 60, 255, 0));
                        Projectile.velocity *= 0.95f;
                    }
                }
                else
                {
                    Projectile.rotation = startingVel.ToRotation() + MathHelper.ToRadians(270f);
                    if (Projectile.timeLeft > 90)
                    {
                        Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, -startingVel.X * 1.3f, 0.045f);
                        Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, -startingVel.Y * 1.3f, 0.045f);
                        Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 120, 90, 255, 0));
                    }
                    else
                    {
                        Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 0, 30, 255, 0));
                        Projectile.velocity *= 0.9f;
                    }
                    if (time > 40)
                    {
                        if (!chomping)
                        {
                            chomp.X = MathHelper.Lerp(chomp.X, startingVel.X * 3, 0.2f);
                            chomp.Y = MathHelper.Lerp(chomp.Y, startingVel.Y * 3, 0.2f);
                        }
                        else
                        {
                            chomp.X = MathHelper.Lerp(chomp.X, 0, 0.2f);
                            chomp.Y = MathHelper.Lerp(chomp.Y, 0, 0.2f);
                        }
                        if (time % 18 == 0)
                            chomping = !chomping;
                    }

                }
            }
            time++;

            if (collideWithTiles && Collision.SolidCollision(Projectile.Center, 4, 4)) // Vanilla tile collision messes with velocity so we use this instead
            {
                canDamage = false;
                if (Projectile.timeLeft > 180)
                    Projectile.timeLeft = 180;
                stuckInGround = true;
                storedVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Zero;
                SoundStyle sound = new("CalamityMod/Sounds/NPCHit/RavagerRockPillarHit1");
                SoundEngine.PlaySound(sound with { Volume = 0.25f, Pitch = Main.rand.NextFloat(-0.3f, -0.6f) }, Projectile.Center);
                collideWithTiles = false;

            }

            if (!Projectile.Calamity().stealthStrike)
                Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 0, 60, 255, 0));
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.Calamity().stealthStrike)
                modifiers.SourceDamage *= Projectile.ai[1] == 4 ? 0.6f : 0.5f;
            else
                modifiers.SourceDamage *= target == chosenTarget ? 1 : (canStick ? 0.2f : 0.5f);
        }
        public override bool? CanDamage() => canDamage ? null : false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target == chosenTarget || Projectile.Calamity().stealthStrike)
                target.AddBuff(ModContent.BuffType<Laceration>(), 180);
            if (!stuckInTarget && canStick && !Projectile.Calamity().stealthStrike)
            {
                if (Projectile.timeLeft < 400)
                    Projectile.timeLeft = 400;
                collideWithTiles = false;
                canDamage = false;
                placementDistance = -Vector2.Distance(target.Center, Projectile.Center);
                placementVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                placementCenter = placementVelocity * (placementDistance * 0.01f);
                chosenTarget = target;
                stuckInTarget = true;
                storedVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8;
                Projectile.velocity = Vector2.Zero;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            if (Projectile.ai[1] == 2)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/LeviathanTooth2").Value;
            if (Projectile.ai[1] == 3)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/LeviathanTooth3").Value;
            if (Projectile.ai[1] == 4)
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GreenWater").Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + vibrate + chomp, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, toothDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.Calamity().stealthStrike ? 50 : 18, targetHitbox);
    }
}
