using System;
using System.Drawing.Drawing2D;
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
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 90000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8 * Projectile.MaxUpdates;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 5 && bladeFade >= 1)
                isAttacking = true;
            else if (attackTimer == 0)
                Projectile.ai[0] = 0;
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.6f);
            ApplyPlayerBuffs();
            tipPosition = Projectile.Center + (Vector2.UnitX * MathHelper.Clamp(bladeFade * 85, 25, 2000)).RotatedBy(Projectile.rotation);
            Vector2 tipVel = (savedTipPos - tipPosition);
            savedTipPos = tipPosition;
            attackMode = Owner.HeldItem.type == ModContent.ItemType<SeashineSword>();

            if (isSpawning)
            {
                if (time >= 90)
                {
                    if (time == 90)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f));
                            dust.noGravity = false;
                            dust.scale = Main.rand.NextFloat(0.65f, 1f);
                            dust.color = Color.Cyan;
                            dust.noLightEmittence = true;
                        }
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBeamSlash") with { Volume = 0.45f, Pitch = 0.7f }, Projectile.Center);
                    }
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -Vector2.UnitY.ToRotation(), 0.05f);
                    Projectile.Center += Vector2.UnitY * -0.5f * Utils.GetLerpValue(180, 40, time, true);
                }
                else
                {
                    Projectile.rotation += 0.3f * Projectile.direction;
                    Projectile.velocity *= 0.96f;
                    Dust dust = Dust.NewDustPerfect(tipPosition, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.2f));
                    dust.noGravity = false;
                    dust.scale = Main.rand.NextFloat(0.55f, 0.7f);
                    dust.color = Color.Cyan;
                    dust.noLightEmittence = true;
                }
            }
            else
            {
                if (isAttacking)
                {
                    //Projectile.velocity += Utils.DirectionTo(Projectile.Center, Owner.ClampedMouseWorld());
                    attackTimer++;
                    if (attackTimer >= 70 + Projectile.ai[1] * 8)
                    {
                        attackTimer = 50;
                        Projectile.ai[0] = 0;
                        setPos = true;
                        isAttacking = false;
                        readySound = true;
                        bladeFade = 0.9f;
                        Projectile.numHits = 0;
                    }

                    if (attackTimer > Projectile.ai[1] * 8 && Projectile.ai[0] != 0)
                    {
                        Vector2 toMouse = Utils.DirectionTo(Projectile.Center, Owner.ClampedMouseWorld());
                        float lerp = Utils.GetLerpValue(30, 60 + Projectile.ai[1] * 8, attackTimer);
                        if (setPos)
                        {
                            startPos = Projectile.Center;
                            endPos = Owner.Calamity().mouseWorld + toMouse * 480;
                            Projectile.numHits = 0;
                            setPos = false;
                        }
                        //Particle orb = new CustomSpark(startPos, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 35, 0.75f, Color.OrangeRed * 0.75f, new Vector2(1f, 1));
                        //GeneralParticleHandler.SpawnParticle(orb);

                        //Particle or2b = new CustomSpark(endPos, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 35, 0.75f, Color.OrangeRed * 0.75f, new Vector2(1f, 1));
                        //GeneralParticleHandler.SpawnParticle(or2b);

                        Projectile.velocity = (Vector2.Lerp(startPos, endPos, lerp) - Projectile.Center) / 18;
                        float angles = 120 * (Projectile.ai[1] % 2 == 0 ? -1 : 1);
                        float rot = Utils.DirectionTo(startPos, endPos).RotatedBy(MathHelper.Lerp(MathHelper.ToRadians(-angles), MathHelper.ToRadians(angles), lerp)).ToRotation();
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rot, 0.1f);

                    }
                }
                else
                {
                    float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 4 / MathHelper.Pi);
                    Vector2 bonusPos = (Vector2.UnitY * 6).RotatedBy((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 7);
                    if (bladeFade < 1 || !attackMode) // Chilling on your back
                    {
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Utils.DirectionTo(Owner.Center + Vector2.UnitY * -60, Projectile.Center).ToRotation(), 0.05f);

                        Vector2 destination = Owner.Center + bonusPos - Vector2.UnitY * 40 + (Vector2.UnitX * Owner.direction).RotatedBy(Projectile.ai[2] * Owner.direction / 3) * (-15 - Projectile.ai[1] * 18);
                        Projectile.velocity = (destination - Projectile.Center) / 18;
                        if (bladeFade < 1f)
                            bladeFade += 0.002f;
                    }
                    else if (attackMode) // Ready to attack
                    {
                        Vector2 toMouse = Utils.DirectionTo(Projectile.Center, Owner.ClampedMouseWorld());
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, toMouse.ToRotation(), 0.1f);

                        Vector2 displace = Projectile.rotation.ToRotationVector2().RotatedBy(Projectile.ai[2] + Main.GlobalTimeWrappedHourly * 2) * 180;
                        Vector2 destination = Owner.ClampedMouseWorld() + bonusPos + displace - Utils.DirectionTo(Owner.Center, Owner.ClampedMouseWorld()) * 335;
                        Projectile.velocity = (destination - Projectile.Center) / 18;
                    }
                }
                if (readySound && bladeFade >= 1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.5f));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.65f, 1f);
                        dust.color = Color.Cyan;
                        dust.noLightEmittence = true;
                    }
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBeamSlash") with { Volume = 0.45f, Pitch = 0.7f }, Projectile.Center);
                    readySound = false;
                }
                if (bladeFade < 1 || (isAttacking && attackTimer > Projectile.ai[1] * 8 + 5))
                {
                    Particle orb = new CustomSpark(tipPosition, tipVel.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/BloomCircle", false, 25, Utils.Remap(tipVel.Length(), 2, 15, 0.15f, 0.3f, true), Color.Cyan * 0.65f, new Vector2(1f, 1), true, true, shrinkSpeed: Utils.Remap(tipVel.Length(), 1, 8, 0f, 0.4f, true), glowOpacity: 0.8f);
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }
            time++;

            if (!isAttacking && (attackTimer > 0 || !attackMode))
            {
                bladeFade = MathHelper.Lerp(bladeFade, 0, 0.025f);
                if (attackTimer > 0)
                    attackTimer--;
            }
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

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            //Projectile.DrawProjectileWithBackglow(Color.Cyan with { A = 0 }, lightColor, 2.5f, tex);
            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(tex2, drawPos - (Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.ToRadians(90)) * 43 * bladeFade), null, Color.Lerp(Color.Cyan, Color.White, i * 0.3f) with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(90), tex2.Size() * 0.5f, new Vector2(0.33f - i * 0.05f, (0.75f + i * 0.03f) * bladeFade) * 0.05f, SpriteEffects.None);

            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation + MathHelper.ToRadians(45), tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 60);
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
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, tipPosition, 20 * Projectile.scale, ref _);
        }
        public override bool? CanDamage() => (bladeFade >= 1 && attackMode) ? null : false;
    }
}
