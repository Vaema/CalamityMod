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

        public float bladeCharge = 0; // The charge level of the blade, ranges from 0 to 1

        public Vector2 tipPosition;
        public Vector2 savedTipPos = Vector2.Zero;
        public bool attackMode => Owner.HeldItem.type == ModContent.ItemType<SeashineHilt>(); // If not holding the weapon, the blades won't charge (no whips for you)

        // Values stored to create blade swing arc
        public Vector2 startPos;
        public Vector2 endPos;
        public bool setPos = true;

        public bool isAttacking = false; // If the blade is in the swinging animation to attack
        public bool readySound = true;
        public static int baseReturnSpeed = 18;
        public int returnSpeed = baseReturnSpeed; // How fast blades retun to you
        public float bladeValue = 0; // The delay between each sword swing
        public Color mainColor;

        public float bladeChargeSpeed = 0.0015f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            // Minion targeting feature is not active here since this isn't a normal summon weapon
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 34;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 0.5f;
            Projectile.timeLeft = 90000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 70 * Projectile.MaxUpdates;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            // Color shifting
            float rate = (Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 6;
            List<Color> eColors = new List<Color>()
            {
                Color.RoyalBlue,
                Color.Turquoise
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            mainColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            // This is the delay between each sword swing, the time bewteen gets shorter as you get more swords
            // This is so it doesn't take forever for swords to come out when you have a lot of them
            bladeValue = (Projectile.ai[1] * (6 / MathHelper.Clamp(Owner.ownedProjectileCounts[Projectile.type] * 0.1f, 1, 1000))) + 10;

            // This checks when the blade is told to attacking to see if it can attack
            if (Projectile.ai[0] == 5 && bladeCharge >= 1)
                isAttacking = true;
            else if (attackTimer == 0)
                Projectile.ai[0] = 0;

            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.6f);
            ApplyPlayerBuffs();

            tipPosition = Projectile.Center + (Vector2.UnitX * MathHelper.Clamp(bladeCharge * 103, 0, 2000)).RotatedBy(Projectile.rotation) * Projectile.scale;
            Vector2 tipVel = (savedTipPos - tipPosition);
            savedTipPos = tipPosition;

            if (isSpawning) // The effects when you first throw out a summon
            {
                if (time >= 110) // Shine effect when spawning
                {
                    if (time == 110)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(tipPosition, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(7f, 14f));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(0.85f, 1.3f);
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
                else // Spinning
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
                bladeCharge = 0.4f;
            }
            else // The actual active ai for the summon
            {
                if (isAttacking) // When commanded to attack and is fully charged
                {
                    attackTimer++;
                    if (attackTimer >= 120 + bladeValue) // Reset values after a swing
                    {
                        Projectile.netUpdate = true;
                        attackTimer = 80;
                        Projectile.ai[0] = 0;
                        setPos = true;
                        isAttacking = false;
                        readySound = true;
                        bladeCharge = 0.9f;
                        Projectile.numHits = 0;
                        Projectile.scale = 1;
                        returnSpeed = 460;
                    }

                    if (attackTimer > bladeValue && Projectile.ai[0] != 0) // The actual swing
                    {
                        Vector2 toMouse = Utils.DirectionTo(Projectile.Center, Owner.ClampedMouseWorld());
                        float lerp = Utils.GetLerpValue(30 + bladeValue, 100 + bladeValue, attackTimer, true);
                        if (setPos)
                        {
                            Projectile.netUpdate = true;
                            startPos = Projectile.Center;
                            endPos = Owner.Calamity().mouseWorld + toMouse * 480;
                            Projectile.numHits = 0;
                            setPos = false;
                        }

                        Projectile.velocity = (Vector2.Lerp(startPos, endPos, lerp) - Projectile.Center) / returnSpeed;
                        float angles = 180 * (Projectile.ai[1] % 2 == 0 ? -1 : 1);
                        float rot = Utils.DirectionTo(startPos, endPos).RotatedBy(MathHelper.Lerp(MathHelper.ToRadians(-angles), MathHelper.ToRadians(angles) / 5, (float)Math.Pow(lerp, 1.5f))).ToRotation();
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rot, 0.1f);

                    }
                    else // An altered version of the on back code to get them into position to swing
                    {
                        float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 4 / MathHelper.Pi);
                        Vector2 bonusPos = (Vector2.UnitY * 6).RotatedBy((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 7);
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Utils.DirectionTo(Owner.Center, Projectile.Center).ToRotation(), 0.05f);

                        float destLerp = Utils.GetLerpValue(0, bladeValue, attackTimer, true);
                        Vector2 destination = Owner.Center + bonusPos + (Vector2.UnitX * Owner.direction).RotatedBy(Projectile.ai[2] * Owner.direction / 2) * (-20 - 330 * (float)Math.Pow(destLerp, 2));
                        Projectile.velocity = (destination - Projectile.Center) / (returnSpeed * 0.6f);
                    }
                }
                else // Chill on the players back while charging the blade
                {
                    float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 4 / MathHelper.Pi);
                    Vector2 bonusPos = (Vector2.UnitY * 6).RotatedBy((Main.GlobalTimeWrappedHourly + Projectile.ai[1]) * 7);

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Utils.DirectionTo(Owner.Center + Vector2.UnitY * -60, Projectile.Center).ToRotation(), Utils.Remap(returnSpeed, baseReturnSpeed, 460, 0.05f, 0.005f, true));

                    Vector2 destination = Owner.Center + bonusPos - Vector2.UnitY * 40 + (Vector2.UnitX * Owner.direction).RotatedBy(Projectile.ai[2] * Owner.direction / 3) * (-15 - Projectile.ai[1] * 18);
                    Projectile.velocity = (destination - Projectile.Center) / returnSpeed;
                    if (bladeCharge < 1f)
                        bladeCharge += bladeChargeSpeed;

                    // The weapon theoretically works with projectile scale, though I ended up not using it
                    //if (Projectile.scale < 3)
                    //Projectile.scale += 0.01f;
                }
                if (readySound && bladeCharge >= 1) // Effect when the blade is fully charged
                {
                    Projectile.netUpdate = true;
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
            // This is the tip trail for the blades, it is a very consistently good looking trail, however it creates a LOT of particles
            // I do not recommend reusing this effect unless you're using it carefully
            if ((returnSpeed == baseReturnSpeed && bladeCharge >= 1 || (isAttacking && attackTimer > bladeValue + 5)) || isSpawning)
            {
                Particle orb = new CustomSpark(tipPosition, tipVel.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/BloomCircle", false, isAttacking ? 30 : 12, Utils.Remap(tipVel.Length(), 2, 15, 0.15f, 0.3f, true) * Projectile.scale, mainColor * 0.65f, new Vector2(1f, isAttacking ? 1.3f : 1), true, true, shrinkSpeed: Utils.Remap(tipVel.Length(), 1, 8, 0f, 0.4f, true), glowOpacity: 0.5f, glowCenterScale: 0.8f);
                GeneralParticleHandler.SpawnParticle(orb);
            }
            // The uncharge of the blades when you're not holding the item or after they slash
            if (!isAttacking && (attackTimer > 0 || !attackMode))
            {
                bladeCharge = MathHelper.Clamp(MathHelper.Lerp(bladeCharge, 0, 0.025f), 0.4f, 1);
                if (attackTimer > 0)
                    attackTimer--;
                readySound = true;
            }
            // How fast the blades try to reach their goal position when not activley slashing
            // Tries to get back to the base value when possible
            if (returnSpeed > baseReturnSpeed)
            {
                returnSpeed = (int)(returnSpeed * 0.99f);
            }
            else
                returnSpeed = baseReturnSpeed;

            time++;
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
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/SeashineHilt").Value;
            Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowBlade").Value;
            Texture2D tex3 = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float extremeLerp = (float)Math.Pow(bladeCharge, 10);
            for (int i = 0; i < 3; i++)
                Main.EntitySpriteDraw(tex2, drawPos - (Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.ToRadians(90)) * 59 * bladeCharge * Projectile.scale), null, Color.Lerp(mainColor, Color.White, i * 0.3f) with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(90), tex2.Size() * 0.5f, new Vector2(0.33f - i * 0.05f, (0.75f + i * 0.03f) * bladeCharge) * 0.05f * Projectile.scale, SpriteEffects.None);

            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(tex, drawPos, null, Color.Lerp(lightColor, (i == 0 ? Color.White : mainColor) with { A = 0 }, extremeLerp), Projectile.rotation + MathHelper.ToRadians(45), tex.Size() * 0.5f, (i == 0 ? 0.8f : 1) * Projectile.scale, SpriteEffects.None);
            
            if (!isAttacking && bladeCharge < 1)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(tex3, drawPos, null, mainColor with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(45), tex3.Size() * 0.5f, new Vector2(i == 0 ? 1 : 3, i == 0 ? 3 : 1) * Main.rand.NextFloat(0.8f, 1.1f) * extremeLerp, SpriteEffects.None);
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);
            if (isAttacking)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (i % 2 == 0)
                    {
                        Particle spark = new SeaPrismParticle(Projectile.Center, (Projectile.velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(10f, 30f), true, 40, Main.rand.NextFloat(0.85f, 1.3f), Color.White, Vector2.One, false, Main.rand.NextFloat(-0.3f, 0.3f), affectedByLight: false);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    Particle e = new CustomSpark(Projectile.Center, (Projectile.velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(7f, 20f), "CalamityMod/Particles/WaterFoam", false, 14, Main.rand.NextFloat(0.1f, 0.2f) * 2.5f, Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue, new Vector2(1f, 1f), true, false, shrinkSpeed: 0.4f);
                    GeneralParticleHandler.SpawnParticle(e);
                }
                for (int i = 0; i < 3; i++)
                {
                    //Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Projectile.velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(5f, 15f));
                    //dust.noGravity = false;
                    //dust.scale = Main.rand.NextFloat(0.95f, 1.4f);
                    //dust.color = mainColor;
                    //dust.noLightEmittence = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.7f, PitchVariance = 0.3f }, Projectile.Center);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Blades should do big damage on the inital hit due to the large delay between attacks
            // However, it's pierce reduction is quite severe (at least right now)
            float minMult = 0.1f;
            int hitsToMinMult = 4;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) // Custom collision since it's a spear
        {
            // Perform an AABB line collision check to check the whole spear.
            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, tipPosition, 30 * Projectile.scale, ref _);
        }
        public override bool? CanDamage() => isAttacking && attackTimer > bladeValue && Projectile.ai[0] != 0;
    }
}
