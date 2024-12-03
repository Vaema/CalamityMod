using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Summon;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SeashineSwordProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public Player Owner => Main.player[Projectile.owner];
        public float attackTimer = 0;
        public float time = 0;
        public bool isSpawning => time < 180;
        public float bladeFade = 0;
        public Vector2 tipPosition;
        public Vector2 savedTipPos = Vector2.Zero;
        public bool attackMode = false;
        public Vector2 startPos;
        public Vector2 endPos;
        public bool setPos = true;
        public bool isAttacking = false;
        public bool readySound = true;
        public int returnSpeed = 18;
        public Color mainColor;

        public float bladeChargeSpeed = 0.002f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            //ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 0.5f;
            Projectile.timeLeft = 90000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25 * Projectile.MaxUpdates;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            float rate = (Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 9;
            List<Color> eColors = new List<Color>()
            {
                Color.Cyan,
                Color.DodgerBlue
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            mainColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (Projectile.ai[0] == 5 && bladeFade >= 1)
                isAttacking = true;
            else if (attackTimer == 0)
                Projectile.ai[0] = 0;
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.6f);
            ApplyPlayerBuffs();

            tipPosition = Projectile.Center + (Vector2.UnitX * MathHelper.Clamp(bladeFade * 85, 25, 2000)).RotatedBy(Projectile.rotation) * Projectile.scale;
            Vector2 tipVel = (savedTipPos - tipPosition);
            savedTipPos = tipPosition;
            attackMode = Owner.HeldItem.type == ModContent.ItemType<SeashineSword>();

            if (isSpawning)
            {
                if (time >= 110)
                {
                    if (time == 110)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1f, 2.5f));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(0.65f, 1f);
                            dust.color = mainColor;
                            dust.noLightEmittence = true;
                        }
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBeamSlash") with { Volume = 0.45f, Pitch = 0.9f }, Projectile.Center);
                        for (int i = 0; i < 2; i++)
                        {
                            Particle fx = new CustomSpark(tipPosition, (i == 0 ? Vector2.UnitY : Vector2.UnitX).RotatedBy(MathHelper.ToRadians(45)) * 0.01f, "CalamityMod/Particles/ThinEndedLine", false, 18, 1, mainColor, new Vector2(3.3f, 0.6f), extraRotation: 0, shrinkSpeed: 0.9f);
                            GeneralParticleHandler.SpawnParticle(fx);
                        }
                    }
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -Vector2.UnitY.ToRotation(), 0.1f);
                    Projectile.velocity *= 0.95f;
                }
                else
                {
                    
                    Projectile.velocity *= 0.96f;
                    Dust dust = Dust.NewDustPerfect(tipPosition, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.2f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.55f, 0.7f);
                    dust.color = mainColor;
                    dust.noLightEmittence = true;
                    if (time >= 80)
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -Vector2.UnitY.ToRotation(), 0.09f);
                    else
                        Projectile.rotation += 0.15f;
                }
            }
            else
            {
                if (isAttacking)
                {
                    attackTimer++;
                    if (attackTimer >= 120 + Projectile.ai[1] * 8)
                    {
                        attackTimer = 80;
                        Projectile.ai[0] = 0;
                        setPos = true;
                        isAttacking = false;
                        readySound = true;
                        bladeFade = 0.9f;
                        Projectile.numHits = 0;
                        Projectile.scale = 1;
                        returnSpeed = 460;
                    }

                    if (attackTimer > Projectile.ai[1] * 8 && Projectile.ai[0] != 0)
                    {
                        Vector2 toMouse = Utils.DirectionTo(Projectile.Center, Owner.ClampedMouseWorld());
                        float lerp = Utils.GetLerpValue(30 + Projectile.ai[1] * 8, 100 + Projectile.ai[1] * 8, attackTimer);
                        if (setPos)
                        {
                            startPos = Projectile.Center;
                            endPos = Owner.Calamity().mouseWorld + toMouse * 480;
                            Projectile.numHits = 0;
                            setPos = false;
                        }

                        Projectile.velocity = (Vector2.Lerp(startPos, endPos, lerp) - Projectile.Center) / returnSpeed;
                        float angles = 120 * (Projectile.ai[1] % 2 == 0 ? -1 : 1);
                        float rot = Utils.DirectionTo(startPos, endPos).RotatedBy(MathHelper.Lerp(MathHelper.ToRadians(-angles), MathHelper.ToRadians(angles) / 2, lerp)).ToRotation();
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rot, 0.1f);

                    }
                    else // an altered version of the on back code to get ready to swing
                    {
                        float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 4 / MathHelper.Pi);
                        Vector2 bonusPos = (Vector2.UnitY * 6).RotatedBy((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 7);
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Utils.DirectionTo(Owner.Center, Projectile.Center).ToRotation(), 0.05f);

                        Vector2 destination = Owner.Center + bonusPos + (Vector2.UnitX * Owner.direction).RotatedBy(Projectile.ai[2] * Owner.direction / 2) * (-200);
                        Projectile.velocity = (destination - Projectile.Center) / (returnSpeed * 5);
                        if (bladeFade < 1f)
                            bladeFade += bladeChargeSpeed;
                    }
                }
                else
                {
                    float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 4 / MathHelper.Pi);
                    Vector2 bonusPos = (Vector2.UnitY * 6).RotatedBy((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 7);
                    if (bladeFade < 1 || !attackMode) // Chilling on your back
                    {
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Utils.DirectionTo(Owner.Center + Vector2.UnitY * -60, Projectile.Center).ToRotation(), Utils.Remap(returnSpeed, 18, 460, 0.05f, 0.005f, true));

                        Vector2 destination = Owner.Center + bonusPos - Vector2.UnitY * 40 + (Vector2.UnitX * Owner.direction).RotatedBy(Projectile.ai[2] * Owner.direction / 3) * (-15 - Projectile.ai[1] * 18);
                        Projectile.velocity = (destination - Projectile.Center) / returnSpeed;
                        if (bladeFade < 1f)
                            bladeFade += 0.002f;

                        // The weapon theoretically works with projectile scale, though I ended up not using it
                        //if (Projectile.scale < 3)
                        //Projectile.scale += 0.01f;
                        
                    }
                    else if (attackMode) // Ready to attack
                    {
                        Vector2 toMouse = Utils.DirectionTo(Projectile.Center, Owner.ClampedMouseWorld());
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, toMouse.ToRotation(), 0.1f);

                        Vector2 displace = Projectile.rotation.ToRotationVector2().RotatedBy(Projectile.ai[2] + Main.GlobalTimeWrappedHourly * 2) * 180;
                        Vector2 destination = Owner.ClampedMouseWorld() + bonusPos + displace - Utils.DirectionTo(Owner.Center, Owner.ClampedMouseWorld()) * 335;
                        Projectile.velocity = (destination - Projectile.Center) / (returnSpeed * 3);
                    }
                }
                if (readySound && bladeFade >= 1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.65f, 1f);
                        dust.color = mainColor;
                        dust.noLightEmittence = true;
                    }
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.8f, Pitch = Main.rand.NextFloat(0.5f, 0.65f) + Projectile.ai[1] * 0.05f }, Projectile.Center);
                    readySound = false;
                }
            }
            if ((returnSpeed == 18 && bladeFade < 1 || (isAttacking && attackTimer > Projectile.ai[1] * 8 + 5)) || isSpawning)
            {
                Particle orb = new CustomSpark(tipPosition, tipVel.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/BloomCircle", false, 25, Utils.Remap(tipVel.Length(), 2, 15, 0.15f, 0.3f, true) * Projectile.scale, mainColor * 0.65f, new Vector2(1f, 1), true, true, shrinkSpeed: Utils.Remap(tipVel.Length(), 1, 8, 0f, 0.4f, true), glowOpacity: 0.8f);
                GeneralParticleHandler.SpawnParticle(orb);
            }
            time++;

            if (!isAttacking && (attackTimer > 0 || !attackMode))
            {
                bladeFade = MathHelper.Lerp(bladeFade, 0, 0.025f);
                if (attackTimer > 0)
                    attackTimer--;
                readySound = true;
            }
            if (returnSpeed > 18)
            {
                returnSpeed = (int)(returnSpeed * 0.99f);
            }
            else
                returnSpeed = 18;
        }
        public void ApplyPlayerBuffs()
        {
            Owner.AddBuff(ModContent.BuffType<SeashineSwordBuff>(), 3600);
            if (Projectile.type == ModContent.ProjectileType<SeashineSwordProj>())
            {
                if (Owner.dead)
                    Owner.Calamity().seashineSwordBuff = false;
                if (Owner.Calamity().seashineSwordBuff)
                    Projectile.timeLeft = 2;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/SeashineSword").Value;
            Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowBlade").Value;
            Texture2D tex3 = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float extremeLerp = (float)Math.Pow(bladeFade, 10);
            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(tex2, drawPos - (Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.ToRadians(90)) * 43 * bladeFade * Projectile.scale), null, Color.Lerp(mainColor, Color.White, i * 0.3f) with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(90), tex2.Size() * 0.5f, new Vector2(0.33f - i * 0.05f, (0.75f + i * 0.03f) * bladeFade) * 0.05f * Projectile.scale, SpriteEffects.None);

            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(tex, drawPos, null, Color.Lerp(lightColor, (i == 0 ? Color.White : mainColor) with { A = 0 }, extremeLerp), Projectile.rotation + MathHelper.ToRadians(45), tex.Size() * 0.5f, i == 0 ? 0.8f : 1, SpriteEffects.None);
            
            if (!isAttacking && bladeFade < 1)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(tex3, drawPos, null, mainColor with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(45), tex3.Size() * 0.5f, new Vector2(i == 0 ? 1 : 3, i == 0 ? 3 : 1) * Main.rand.NextFloat(0.8f, 1.1f) * extremeLerp, SpriteEffects.None);
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 60);
            if (isAttacking)
            {
                for (int i = 0; i < 4; i++)
                {
                    Particle spark = new SeaPrismParticle(Projectile.Center, (Projectile.velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(10f, 30f), true, 45, Main.rand.NextFloat(0.85f, 1.1f), Color.White, Vector2.One, false, Main.rand.NextFloat(-0.3f, 0.3f), affectedByLight: false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Projectile.velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(5f, 15f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.95f, 1.4f);
                    dust.color = mainColor;
                    dust.noLightEmittence = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.7f, PitchVariance = 0.3f }, Projectile.Center);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.15f;
            int hitsToMinMult = 3;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= isAttacking ? damageMult : 0.1f;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) // Custom collision since it's a spear
        {
            // Perform an AABB line collision check to check the whole spear.
            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, tipPosition, 30 * Projectile.scale, ref _);
        }
        public override bool? CanDamage() => (bladeFade >= 1 && attackMode) ? null : false;
    }
}
