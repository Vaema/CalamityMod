using System;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class TransformerBlob : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private float radius = 25f;
        public bool visuals => Owner.Calamity().transformerVisual;
        public ref float layer => ref Projectile.ai[0];
        public bool canDamage = false;
        public float speed = 250;
        public float rotationAngle = 0;
        public int time = 0;
        public int currentLayer = 0;
        public float sine = 1;
        public float rotSpeed = 1f;
        public int savedFrame = 0;
        public bool powered => Projectile.localAI[0] == 5;
        public int poweredTimerMax => (int)(140 + 6 * Projectile.ai[1]);
        public int poweredTimer = -1;
        public float poweredLerp => (float)Math.Pow(Utils.GetLerpValue(poweredTimerMax, 90, poweredTimer, true), 4);
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 16;
        }
        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 56;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60 * Projectile.MaxUpdates;
            Projectile.ArmorPenetration = 25;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];

            Lighting.AddLight(Projectile.Center, Color.SkyBlue.ToVector3() * 0.5f);

            sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Projectile.ai[1] % 2 == 0 ? 10 : 6) / MathHelper.Pi) * 0.4f;

            if (time == 0)
            { 
                rotationAngle = Projectile.ai[2];
                Projectile.frame = 8;
            }
            if (poweredTimer == -1 && powered)
            {
                poweredTimer = poweredTimerMax;
                savedFrame = Projectile.frame;
            }

            if (time >= 40)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter > (6) * Projectile.MaxUpdates)
                {
                    if (powered && Projectile.frame == 8)
                        Projectile.frame = 8;
                    else
                        Projectile.frame++;
                    Projectile.frameCounter = 0;
                }
                if (Projectile.frame >= 16)
                    Projectile.frame = 0;
            }
            else
                Projectile.frame = (int)MathHelper.Lerp(savedFrame, 8, poweredLerp);

            layer = (Projectile.ai[1] > 20 ? 3 : Projectile.ai[1] > 10 ? 2 : 1);
            
            if (time >= 90)
                canDamage = true;

            if (layer != currentLayer)
            {
                currentLayer = (int)layer;
                if (time > 60)
                    time = 60;
            }
            rotationAngle = MathHelper.Lerp(rotationAngle, Projectile.ai[2], 0.025f);
            if (time >= 40)
            {
                if (poweredTimer == 1)
                {
                    Projectile.numHits = 0;
                    Projectile.velocity = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld) * 12;
                    Projectile.extraUpdates = 8;
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;

                    for (int i = 0; i <= 9; i++)
                    {
                        float variance = Main.rand.NextFloat(-0.6f, 0.6f);
                        int dustStyle = 278;
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center, dustStyle, Projectile.velocity);
                        dust2.scale = Main.rand.NextFloat(0.9f, 1.2f) - Math.Abs(variance);
                        dust2.velocity = (Projectile.velocity * 2).RotatedBy(variance) * Main.rand.NextFloat(0.3f, 1f) * (1 - Math.Abs(variance));
                        dust2.noGravity = true;
                        dust2.color = Color.Cyan;
                    }

                    SoundStyle fire = new("CalamityMod/Sounds/Item/OmicronBeam");
                    SoundEngine.PlaySound(fire with { Volume = 0.3f, Pitch = Main.rand.NextFloat(0.3f, 0.5f) + Projectile.ai[1] * 0.015f, MaxInstances = -1 }, Projectile.Center);
                }
                else if (poweredTimer == 0)
                {
                    //trail shit
                    float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                    Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 20f;

                    Particle spark = new GlowSparkParticle(Projectile.Center - Projectile.velocity, -Projectile.velocity * 0.3f, false, 21, 0.04f, Color.DodgerBlue * 0.65f, new Vector2(0.6f, 0.5f), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark);
                    if (time % 2 == 0)
                    {
                        Vector2 dustVel = (-Projectile.velocity).RotatedByRandom(0.3f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, ModContent.DustType<VoidDustInverted>(), dustVel * Main.rand.NextFloat(0.1f, 0.8f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.4f, 0.6f);
                        dust.color = new Color(30, 30, 30);
                        dust.noLightEmittence = true;
                    }

                    Projectile.rotation = (Projectile.velocity.RotatedBy(MathHelper.ToRadians(-90))).ToRotation();
                }
                else
                {
                    Vector2 centerPoint = (powered ? Vector2.Lerp(Owner.Center, Vector2.Lerp(Owner.Calamity().mouseWorld, Owner.Center, 0.6f), poweredLerp) : Owner.Center);
                    float positioning = (-60 - (55 * layer) + 30 * sine) * MathHelper.Clamp((powered ? 1.25f - poweredLerp : 1), 0.25f, 1);
                    Projectile.velocity = ((centerPoint + new Vector2(0, positioning).RotatedBy(rotationAngle * -rotSpeed + Main.GlobalTimeWrappedHourly * 2.5f * (1 - layer * (powered ? 0.33f : 0.15f)) * (layer == 2 ? -1 : 1))) - Projectile.Center) / (speed);
                    Projectile.rotation = Projectile.rotation.AngleLerp(sine, 0.02f);
                }
                if (powered && poweredTimer != 0)
                {
                    rotSpeed *= 1.0067f;
                    Projectile.rotation = Projectile.rotation.AngleLerp(Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).RotatedBy(MathHelper.ToRadians(-90)).ToRotation(), poweredLerp);
                }
            }
            else
            {
                Projectile.velocity *= 0.965f;
                Projectile.rotation = (Projectile.velocity.RotatedBy(MathHelper.ToRadians(-90))).ToRotation();
            }
            speed = MathHelper.Lerp(speed, 1, (float)Math.Pow(Utils.GetLerpValue(0, 180, time), 2));

            if (!powered)
                Projectile.timeLeft++;
            if (poweredTimer > 0)
                poweredTimer--;
            time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0 && target.realLife == -1)
                Projectile.numHits--;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (poweredTimer == 0)
            {
                float minMult = 0.25f;
                int hitsToMinMult = 5;
                float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true) * (Projectile.numHits == 0 ? 1.5f : 1); // +50% damage on first hit
                modifiers.SourceDamage *= damageMult;
            }
            else
                modifiers.SourceDamage *= 0.2f;
        }
        public override void OnKill(int timeLeft)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D orbTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/TheTransformer").Value;
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Rectangle frame = orbTexture.Frame(1, 16, 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            if (powered)
            {
                Color auraColor = Color.DodgerBlue with { A = 0 } * poweredLerp;
                for (int i = 0; i < 2; i++)
                {
                    float bScale2 = 0.25f;
                    Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition, null, Color.DodgerBlue with { A = 0 }, Projectile.rotation, bTexture.Size() * 0.5f, Vector2.Lerp(new Vector2(0.6f, 1.4f), Vector2.One, MathHelper.Min(Utils.GetLerpValue(8, 15, Projectile.frame, true), Utils.GetLerpValue(8, 0, Projectile.frame, true))) * bScale2 * Projectile.scale, SpriteEffects.None, 0);
                }
            }
            Main.EntitySpriteDraw(orbTexture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            if (!powered)
            {
                for (int i = 0; i < 10; i++)
                {
                    Color auraColor = Color.DodgerBlue with { A = 0 } * sine * 0.6f;
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 4;
                    Main.EntitySpriteDraw(orbTexture, Projectile.Center - Main.screenPosition + drawOffset, frame, auraColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
                }
            }
            /*
            Texture2D rTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/ShatteredExplosion").Value;
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Color drawColor = Color.DodgerBlue;
            float rot2 = 0.3f;

            for (int i = 0; i < 5; i++)
            {
                float bScale2 = 0.75f;
                Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition, null, Color.Lerp(drawColor, Color.White, i * 0.15f) with { A = 0 }, 0, bTexture.Size() * 0.5f, (bScale2 - i * 0.15f) * rot2 * Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(rTexture, Projectile.Center - Main.screenPosition, null, drawColor with { A = 0 }, Main.rand.NextFloat(-2, 2), rTexture.Size() * 0.5f, 0.03f * rot2 * Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(rTexture, Projectile.Center - Main.screenPosition, null, drawColor with { A = 0 }, Main.rand.NextFloat(-2, 2), rTexture.Size() * 0.5f, 0.04f * rot2 * Projectile.scale, SpriteEffects.None, 0);
            */
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox);
        public override bool? CanDamage() => canDamage ? null : false;
        public override bool? CanCutTiles() => false;
    }
}
