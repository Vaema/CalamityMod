using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Potions;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class DiamondOfTheDeepProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        public Color bColor = Color.White;
        public Color color1 = Color.White;
        public Color color2 = Color.White;
        public bool canDamage = false;
        public bool visuals => Owner.Calamity().dOfTheDeepVisual; // Enables/disables visuals and sounds based on accessory visibility
        public ref float time => ref Projectile.ai[0];
        public ref float energyNumber => ref Projectile.ai[1]; // The number assigned to this energy, ranging from 1 to the max number of projectiles
        public bool idle => Projectile.ai[2] == 0; // Floating around the player
        public bool healing = false; // If the projectiles should heal the player. If false they will attack
        public SlotId soundSlot;
        public NPC targeted;
        public ref float projType => ref Projectile.localAI[2];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.NoLiquidDistortion[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 400;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration = 20;
        }

        public override void AI()
        {
            if (time == 0 && idle)
            {
                Main.NewText(projType);
                switch (projType) // 0 = gravel/vines, 1 = hadal/hydrothermic, 2 = void/lumenyl
                {
                    case 0:
                        {
                            color1 = Color.MediumSeaGreen;
                            color2 = Color.DarkSlateGray;
                            break;
                        }
                    case 1:
                        {
                            color1 = Color.DarkRed;
                            color2 = Color.OrangeRed;
                            break;
                        }
                    case 2:
                        {
                            color1 = Color.MediumBlue;
                            color2 = Color.DodgerBlue;
                            break;
                        }
                }
            }

            int startTime = 100; // Time after launch when it will begin attacking/healing
            int endTime = 300; // Time after launch when it's movement code reaches it's cap on strength

            float rate = Main.GlobalTimeWrappedHourly * 5;
            List<Color> eColors = new List<Color>()
            {
                color1,
                color2
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            bColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (idle || time < startTime) // If not attacking/healing keep it alive
                Projectile.timeLeft++;

            if (Owner.dead || !Owner.Calamity().dOfTheDeep)
                Projectile.Kill();

            if (Utils.Distance(Owner.Center, Projectile.Center) > 1100) // If it's too far away either teleport to the player if idle (or kill it if needed)
            {
                if (idle)
                    Projectile.Center = Owner.Center;
                else if (targeted == null && !healing)
                    Projectile.Kill();
            }

            // Emit some light
            Lighting.AddLight(Projectile.Center, bColor.ToVector3() * 1.5f);

            if (SoundEngine.TryGetActiveSound(soundSlot, out var sound) && sound.IsPlaying)
                sound.Position = Projectile.Center;

            float velLerp = Utils.GetLerpValue(-1.5f, 3f, Projectile.velocity.Length(), true);
            if (projType == 0)
            {
                if (Main.rand.NextBool((int)(14 - 5 * velLerp)))
                {
                    Dust c = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15, ModContent.DustType<SquashDustHollow>());
                    c.velocity = -Projectile.velocity * Main.rand.NextFloat(0.3f, 1f) * velLerp;
                    c.scale = Main.rand.NextFloat(1f, 1.3f) * velLerp;
                    c.noGravity = true;
                    c.color = Main.rand.NextBool() ? Color.Aquamarine : bColor;
                    c.noLightEmittence = true;
                    c.fadeIn = 1;
                    c.alpha = 100;
                }
            }
            if (projType == 1)
            {
                if (Main.rand.NextBool((int)(10 - 5 * velLerp)))
                {
                    Dust c = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15, ModContent.DustType<SquashDust>());
                    c.velocity = -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.6f, 1.3f) * velLerp;
                    c.scale = Main.rand.NextFloat(1.2f, 1.5f) * velLerp;
                    c.noGravity = true;
                    c.color = bColor;
                    c.noLightEmittence = true;
                    c.noGravity = !Main.rand.NextBool(3);
                }
            }
            if (projType == 2)
            {
                if (Main.rand.NextBool((int)(3 - velLerp)))
                {
                    Dust c = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15, ModContent.DustType<SquashDust>());
                    c.velocity = -Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(1.4f, 3.5f) * velLerp;
                    c.scale = Main.rand.NextFloat(1.8f, 2.3f) * velLerp;
                    c.noGravity = true;
                    c.color = bColor;
                    c.noLightEmittence = true;
                    c.fadeIn = 2.3f;
                }
            }

            if (idle) // Following Ai
            {
                if (time == 0 && visuals) // On spawn fx
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        float variance = Main.rand.NextFloat(-0.5f, 0.5f);
                        Vector2 vel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedBy(variance) * Main.rand.NextFloat(0.3f, 1f) * (1 - Math.Abs(variance)) * 2;
                        float scale = (Main.rand.NextFloat(1.5f, 1.7f) - Math.Abs(variance)) * 0.35f;

                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), vel);
                        dust2.scale = scale * 3;
                        dust2.noGravity = false;
                        dust2.alpha = 180;
                        dust2.color = bColor;
                        dust2.noLight = true;
                        dust2.noLightEmittence = true;
                    }
                }
                if (time > 80) // Track the player in a natural motion simular to swimming
                {
                    float homingSpeed = Utils.Remap(Utils.Distance(Projectile.Center, Owner.Center), 200, 600, 0.07f, 0.16f) + 0.009f * energyNumber;
                    float offsetPower = Utils.GetLerpValue(1, 5, Owner.velocity.Length(), true);
                    float sine = (float)Math.Sin((time * 0.1f) / MathHelper.Pi);
                    float sine2 = (float)Math.Sin((time * 0.04f) / MathHelper.Pi);
                    Vector2 bonusMobility = (offsetPower > 0 ? ((Utils.DirectionTo(Projectile.Center, Owner.Center) * 90) * sine2).RotatedBy(0.8f * sine) * offsetPower : Vector2.Zero);
                    Vector2 goalPosition = Owner.MountedCenter + bonusMobility + ((MathHelper.TwoPi * energyNumber) / Math.Max(Owner.ownedProjectileCounts[ModContent.ProjectileType<AmuletEnergy>()], 1)).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 0.4f) * 20;

                    bool outOfRange = Utils.Distance(Projectile.Center, goalPosition) > 120;
                    if (Projectile.velocity.Length() < 6 && outOfRange)
                        Projectile.velocity = Projectile.velocity * 0.995f + Utils.DirectionTo(Projectile.Center, goalPosition) * homingSpeed;
                    else if (outOfRange)
                        Projectile.velocity *= 0.985f;
                    if (!outOfRange)
                        Projectile.velocity = Projectile.velocity.RotatedBy(0.0065f * (energyNumber % 2 == 0 ? -1 : 1)) * 1.004f;
                }
                else
                    Projectile.velocity *= 0.99f;
            }
            else // Attack/Healing Ai
            {
                if (Projectile.ai[2] == 5) // Effects for the moment they stop being idle
                {
                    time = 0;
                    healing = (Owner.statLife < Owner.statLifeMax2 * 0.5f);
                    Projectile.ai[2]++;
                    Projectile.velocity = Vector2.Lerp(Utils.DirectionTo(Owner.Center, Projectile.Center), Owner.velocity.SafeNormalize(Vector2.UnitX), 0.6f) * Main.rand.NextFloat(4.5f, 5.5f);
                }
                if (time <= startTime)
                    Projectile.velocity *= 0.99f;

                if (healing) // I wonder what this does :clueless:
                {
                    float sine = (float)Math.Sin((time * 0.3f) / MathHelper.Pi);
                    Projectile.extraUpdates = 6;
                    
                    if (time > startTime)
                    {
                        float homingSpeed = Utils.Remap(time, startTime, endTime, 0.01f, 0.1f);

                        Vector2 goalPosition = Owner.Center;

                        if (Projectile.velocity.Length() < 5)
                            Projectile.velocity = Projectile.velocity.RotatedBy(0.02f * sine) * 0.99f + Utils.DirectionTo(Projectile.Center, goalPosition) * homingSpeed;
                        else
                            Projectile.velocity *= 0.985f;

                        if (Utils.Distance(goalPosition, Projectile.Center) < 50)
                        {
                            Owner.HealPlayer(2);
                            Projectile.Kill();
                        }
                    }
                }
                else // Attacking
                {
                    canDamage = true;
                    Projectile.extraUpdates = 6;

                    if (time > startTime)
                    {
                        float homingSpeed = Utils.Remap(time, startTime, endTime, 0.01f, 0.1f);

                        targeted = Projectile.Center.ClosestNPCAt(1200);
                        CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, homingSpeed, 25, 0.99f, accelerate: true);

                        if (targeted == null)
                        {
                            Projectile.extraUpdates = 2;
                            if (Projectile.velocity.Y > -5)
                            {
                                Projectile.velocity.Y -= 0.8f * homingSpeed;
                                Projectile.velocity.X *= 0.997f;
                            }
                        }
                        else
                            Projectile.timeLeft++;
                    }
                }
            }

            float squash = Utils.GetLerpValue(1, 3, Projectile.velocity.Length(), true);
            if (squash > 0.15f && visuals)
            {
                Particle fadeInfx = new CustomSpark(Projectile.Center, Projectile.velocity * 0.01f, "CalamityMod/Particles/BloomCircle", false, 18, 0.55f * Projectile.scale, bColor * 0.4f * squash, new Vector2(1 - 0.15f * squash, 1f), true, true, shrinkSpeed: 0.3f * squash, glowOpacity: 0.5f, glowCenterScale: 0.45f);
                GeneralParticleHandler.SpawnParticle(fadeInfx);
            }

            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.7f, 0.1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 420);
        }
        public override void OnKill(int timeLeft)
        {
            if (!healing && visuals)
            {
                for (int i = 0; i <= 4; i++)
                {
                    float variance = Main.rand.NextFloat(-0.5f, 0.5f);
                    Vector2 vel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedBy(variance) * Main.rand.NextFloat(0.3f, 1f) * (1 - Math.Abs(variance)) * 4;
                    float scale = (Main.rand.NextFloat(1.5f, 1.7f) - Math.Abs(variance)) * 0.35f;

                    Particle sparks = new CustomSpark(Projectile.Center + vel, vel, "CalamityMod/Particles/BloomCircle", false, Main.rand.Next(13, 16 + 1), scale, bColor * 0.7f, new Vector2(1f, 1), true, true, 0, false, shrinkSpeed: 0.25f);
                    GeneralParticleHandler.SpawnParticle(sparks);

                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), vel);
                    dust2.scale = scale * 3;
                    dust2.noGravity = false;
                    dust2.alpha = 180;
                    dust2.color = Main.rand.NextBool(4) ? Color.Lerp(Color.Yellow, bColor, 0.5f) : bColor;
                    dust2.noLight = true;
                    dust2.noLightEmittence = true;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> orb = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
            float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 10) / MathHelper.Pi);
            Vector2 squash = new Vector2(Utils.Remap(Projectile.velocity.Length(), 1, 5, 1, 0.6f), Utils.Remap(Projectile.velocity.Length(), 1, 5, 1, 2f));

            Asset<Texture2D> block = ModContent.Request<Texture2D>("CalamityMod/Items/Placeables/Abyss/AbyssGravel");
            switch (projType) // 0 = gravel/vines, 1 = hadal/hydrothermic, 2 = void/lumenyl
            {
                case 0:
                    {
                        block = ModContent.Request<Texture2D>("CalamityMod/Items/Placeables/Abyss/AbyssGravel");
                        break;
                    }
                case 1:
                    {
                        block = ModContent.Request<Texture2D>("CalamityMod/Items/Placeables/Abyss/PyreMantle");
                        break;
                    }
                case 2:
                    {
                        block = ModContent.Request<Texture2D>("CalamityMod/Items/Placeables/Abyss/Voidstone");
                        break;
                    }
            }

            for (int i = 0; i < 6; i++)
            {
                Color orbColor = Color.Lerp(bColor, color2, (i + 1) / 6) with { A = 0 } * 0.4f * (visuals ? 1 : 0.1f);
                Vector2 scale = Projectile.scale * squash * (0.05f + i * 0.01f) * 4.3f;
                Main.EntitySpriteDraw(orb.Value, Projectile.Center - Main.screenPosition, null, Color.Lerp(orbColor, Color.White with { A = 0 }, 1 - i * 0.5f), Projectile.rotation, orb.Size() * 0.5f, scale, SpriteEffects.None);
            }

            float velLerp = Utils.GetLerpValue(0.5f, 3f, Projectile.velocity.Length(), true);
            if (projType == 0)
            {
                Asset<Texture2D> ring = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing");
                int orbs = 9;
                for (int i = 1; i < orbs + 1; i++)
                {
                    float sine2 = (float)Math.Sin((time * (i % 3 == 0 ? 0.2f : i % 2 == 0 ? 0.4f : 1f) * 0.2f) / MathHelper.Pi);

                    Vector2 placement = Projectile.Center + ((MathHelper.TwoPi * i / orbs) + 7 * sine).ToRotationVector2() * (18 + Math.Abs(sine * (i % 3 == 0 ? 19 : 11))) * velLerp;
                    Color orbColor = Color.White * (visuals ? 1 : 0.1f) * velLerp;
                    Vector2 scale = Vector2.One * Projectile.scale;

                    for (int y = 0; y < 7; y++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * y / 7).ToRotationVector2() * 4.5f;
                        Main.EntitySpriteDraw(block.Value, placement - Main.screenPosition + drawOffset, null, bColor with { A = 0 } * 0.7f * velLerp, Projectile.rotation + 0.02f * MathHelper.Lerp(i, 1, 0.75f) + 0.4f, block.Size() * 0.5f, scale, i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(block.Value, placement - Main.screenPosition, null, orbColor, Projectile.rotation + 0.02f * MathHelper.Lerp(i, 1, 0.75f) + 0.4f, block.Size() * 0.5f, scale, i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                }
                for (int i = 0; i < 3; i++)
                    Main.EntitySpriteDraw(ring.Value, Projectile.Center - Main.screenPosition, null, bColor with { A = 0 } * 0.9f * velLerp, Projectile.rotation, ring.Size() * 0.5f, 0.2f * velLerp * Projectile.scale + (0.15f * Math.Abs(sine)) + 0.02f * i, SpriteEffects.None);
            }
            if (projType == 1)
            {
                int orbs = 9;
                for (int i = 0; i < orbs; i++)
                {
                    bool outer = i > (2);
                    bool outest = i > (5);
                    float rotation = MathHelper.TwoPi * i / (3) + Main.GlobalTimeWrappedHourly * 6;
                    Vector2 placement = Projectile.Center + ((rotation).ToRotationVector2() * (outest ? 16 : outer ? 12 : 8) + (Projectile.velocity.SafeNormalize(Vector2.UnitX) * (outest ? -15f : outer ? 0f : 15))) * velLerp;
                    Color orbColor = Color.White * (visuals ? 1 : 0.1f) * velLerp;
                    Vector2 scale = new Vector2(0.35f + (outest ? 0.1f : outer ? 0.3f : 0.6f), 2.3f) * Projectile.scale * (outest ? 0.8f : outer ? 0.9f : 1) * 1.5f;

                    for (int y = 0; y < 7; y++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * y / 7).ToRotationVector2() * 2.5f;
                        Main.EntitySpriteDraw(block.Value, placement - Main.screenPosition + drawOffset, null, bColor with { A = 0 } * velLerp, Utils.DirectionTo(placement, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 65f).ToRotation() + MathHelper.PiOver2, block.Size() * 0.5f, scale, outer ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(block.Value, placement - Main.screenPosition, null, orbColor, Utils.DirectionTo(placement, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 65f).ToRotation() + MathHelper.PiOver2, block.Size() * 0.5f, scale, outer ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                }
            }
            if (projType == 2)
            {
                int orbs = 8;
                for (int i = 1; i < orbs + 1; i++)
                {
                    float sine3 = (float)Math.Sin((time * (i % 3 == 0 ? 0.2f : i % 2 == 0 ? 0.4f : 1f) * 0.07f) / MathHelper.Pi);
                    float sine2 = (float)Math.Sin((time * (i % 3 == 0 ? 0.2f : i % 2 == 0 ? 0.4f : 1f) * 0.2f) / MathHelper.Pi);

                    Vector2 velocity = Projectile.Center + ((MathHelper.TwoPi * i / orbs) + time * 0.04f).ToRotationVector2() * (23f + 8 * sine3) * Math.Abs(sine2) * velLerp;

                    Color orbColor = Color.White * (visuals ? 1 : 0.1f) * velLerp;
                    Vector2 scale = new Vector2(i % 2 == 0 ? 0.4f : 0.6f, i % 2 == 0 ? 1f : 1.3f) * Projectile.scale * 1.3f;

                    for (int y = 0; y < 7; y++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * y / 7).ToRotationVector2() * 4.5f;
                        Main.EntitySpriteDraw(block.Value, velocity - Main.screenPosition + drawOffset, null, bColor with { A = 0 } * 0.7f * velLerp, Utils.DirectionTo(velocity, Projectile.Center).ToRotation() + MathHelper.PiOver2 * sine3, block.Size() * 0.5f, scale, i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(block.Value, velocity - Main.screenPosition, null, orbColor, Utils.DirectionTo(velocity, Projectile.Center).ToRotation() + MathHelper.PiOver2 * sine3, block.Size() * 0.5f, scale, i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                }
            }

            return false;
        }
        public override bool? CanDamage() => canDamage ? null : false;
    }
}
