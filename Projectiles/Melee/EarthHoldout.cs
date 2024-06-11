using System;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class EarthHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<Earth>();

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Earth>();
        public override string Texture => "CalamityMod/Items/Weapons/Melee/Earth";
        public override float HitboxOutset => 105;

        public override Vector2 HitboxSize => new Vector2(145, 145);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public override Vector2 SpriteOrigin => new(0, 104);
        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = true;
        public bool postSwing = false;
        public float fadeIn = 0;
        public int useAnim;
        public int swingCount;
        public bool spawnBoom = true;
        public Color mainColor = Color.OrangeRed;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = TrueMeleeDamageClass.Instance;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.knockBack = 0;
            Projectile.scale = 1;
            Projectile.ai[1] = -1;
            base.OnSpawn(source);
            mousePos = Owner.Calamity().mouseWorld;
            aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnim = Owner.itemAnimationMax;

            if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            else Owner.direction = 1;

            FlipAsSword = Owner.direction == -1 ? true : false;
        }

        public override void UseStyle()
        {
            DrawUnconditionally = false;

            if (CanHit || postSwing)
                mousePos = Owner.Center - aimVel;
            else
            {
                mousePos = Owner.Calamity().mouseWorld;
            }

            if (CanHit)
                fadeIn = MathHelper.Lerp(fadeIn, 1, 0.1f);
            else
                fadeIn = MathHelper.Lerp(fadeIn, 0, 0.15f);


            if (!doSwing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;

                spawnBoom = true;
                Projectile.numHits = 0;
                mousePos = Owner.Calamity().mouseWorld;
                aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false;
                if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                else Owner.direction = 1;
                FlipAsSword = Owner.direction == -1 ? true : false;

                doSwing = true;
                swingCount++;
            }
            else
            {
                if (!CanHit && !postSwing)
                {
                    if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                    else Owner.direction = 1;
                }
                else
                {
                    if ((Owner.Center - aimVel).X < Owner.Center.X) Owner.direction = -1;
                    else Owner.direction = 1;
                }
                    
                
                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.AngleTo(mousePos) + MathHelper.ToRadians(65f), 0.1f);

                if (AnimationProgress < (useAnim / 3))
                {
                    aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                    CanHit = false;
                    postSwing = false;
                    if (AnimationProgress == 0)
                    {
                        doSwing = false;
                        Projectile.ai[1] = -Projectile.ai[1];
                    }
                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * Projectile.ai[1] * Owner.direction), 0.2f);
                }
                else
                {
                    float time = (AnimationProgress) - (useAnim / 3);
                    float timeMax = useAnim - (useAnim / 3);

                    if (time == (int)(timeMax * 0.4f))
                    {
                        SoundStyle fire = new("CalamityMod/Sounds/Item/HeavySwing");
                        SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = (Projectile.ai[1] == 1 ? 0.6f : 0.9f) }, Projectile.Center);
                    }
                    if ( time > (int)(timeMax * 0.4f) && time < (int)(timeMax * 0.9f))
                    {
                        CanHit = true;

                        for (int i = 0; i < 2; i++)
                        {
                            mainColor = Main.rand.Next(3) switch
                            {
                                0 => Color.OrangeRed,
                                1 => Color.MediumTurquoise,
                                _ => Color.LawnGreen,
                            };
                            Vector2 particleVel = new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                            Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(30, 170), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)));
                            GeneralParticleHandler.SpawnParticle(new GlowSparkParticle(particlePos, -particleVel.RotatedByRandom(0.2f) * 2, false, 12, Main.rand.NextFloat(0.03f, 0.01f), mainColor, new Vector2 (0.3f, 1f)));
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            mainColor = Main.rand.Next(3) switch
                            {
                                0 => Color.OrangeRed,
                                1 => Color.MediumTurquoise,
                                _ => Color.LawnGreen,
                            };
                            Vector2 particleVel = new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                            Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(30, 170), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)));
                            GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(particlePos, -particleVel.RotatedByRandom(0.2f) * 2, mainColor, 23, Main.rand.NextFloat(0.5f, 1f), 0.65f, 0, true));
                        }
                    }
                    else
                        CanHit = false;

                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(MathHelper.Lerp(150f * Projectile.ai[1] * Owner.direction, 120f * -Projectile.ai[1] * Owner.direction, CalamityUtils.ExpInOutEasing(time / timeMax, 1))),
                        0.2f);

                    if (time >= timeMax)
                        doSwing = false;
                    if (time < (int)(timeMax * 0.7f))
                        postSwing = true;

                    if (CanHit)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            mainColor = Main.rand.Next(3) switch
                            {
                                0 => Color.OrangeRed,
                                1 => Color.MediumTurquoise,
                                _ => Color.LawnGreen,
                            };
                            bool color = Main.rand.NextBool();
                            GenericSparkle sparker = new GenericSparkle(Owner.Center + (new Vector2(170, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), Vector2.Zero, Color.White, mainColor, Main.rand.NextFloat(0.5f, 0.7f), 11, Main.rand.NextFloat(-0.1f, 0.1f), 2.68f);
                            GeneralParticleHandler.SpawnParticle(sparker);

                            Dust dust2 = Dust.NewDustPerfect(Owner.Center + (new Vector2(170, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), 278, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 2));
                            dust2.scale = Main.rand.NextFloat(0.55f, 0.85f);
                            dust2.noGravity = true;
                            dust2.color = Color.Lerp(Color.White, mainColor, 0.5f);
                        }
                    }   
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);

            mainColor = Main.rand.Next(3) switch
            {
                0 => Color.OrangeRed,
                1 => Color.MediumTurquoise,
                _ => Color.LawnGreen,
            };
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (damageDone <= 2 && Projectile.numHits > 0)
                Projectile.numHits -= 1;

            bool isAPillar = target.type == NPCID.LunarTowerSolar || target.type == NPCID.LunarTowerVortex || target.type == NPCID.LunarTowerNebula || target.type == NPCID.LunarTowerStardust;
            if (!isAPillar && !target.boss && target.IsAnEnemy(true, true, false) && !CalamityPlayer.areThereAnyDamnBosses && target.CanBeChasedBy(Projectile, false))
            {
                // Launch
                Vector2 launchVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -17;
                target.velocity = launchVel * (target.knockBackResist == 0 ? 0 : 1f);
            }

            if (Projectile.ai[1] == -1 && spawnBoom)
            {
                Vector2 spawnSpot = target.Center + new Vector2(Main.rand.NextFloat(-450, 450), Main.rand.NextFloat(-450, -650));
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnSpot, Vector2.Zero, ModContent.ProjectileType<EarthMeteor>(), Projectile.damage * 2, Projectile.knockBack, Projectile.owner, 0, 0, 2);
                spawnBoom = false;
            }

            SoundStyle fire = new("CalamityMod/Sounds/NPCHit/ThanatosHitOpen1");
            SoundEngine.PlaySound(fire with { Volume = 0.75f, Pitch = 0.2f }, Projectile.Center);
            SoundStyle fire2 = new("CalamityMod/Sounds/Item/ExobladeBeamSlash");
            SoundEngine.PlaySound(fire2 with { Volume = 0.35f, Pitch = Main.rand.NextFloat(0.5f, 0.7f) }, Projectile.Center);

            int heal = (int)(MathHelper.Clamp(30 - Projectile.numHits * 25, 1, 30));
            if (Projectile.numHits < 30)
            {
                Owner.statLife += heal;
                Owner.HealEffect(heal);
                if (Owner.statLife > Owner.statLifeMax2)
                    Owner.statLife = Owner.statLifeMax2;
            }

            int points = 6;
            float radians = MathHelper.TwoPi / points;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f)).RotatedByRandom(100);
            for (int k = 0; k < points; k++)
            {
                Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f);
                if (k % 2 == 0)
                    velocity *= 5;
                Particle spark = new GlowSparkParticle((target.Center + velocity * 7.5f), velocity * 0.5f, false, 11, 0.08f, mainColor, new Vector2(1f, 0.4f), true);
                GeneralParticleHandler.SpawnParticle(spark);
                Particle spark2 = new GlowSparkParticle((target.Center + velocity * 7.5f), velocity * 0.5f, false, 35, 0.06f, mainColor, new Vector2(0.4f, 1.1f), false, false);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
            Particle blastRing = new CustomPulse(target.Center, Vector2.Zero, mainColor, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 2f, 1f, 15, true);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(target.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 1f, 0.5f, 15, true);
            GeneralParticleHandler.SpawnParticle(blastRing2);

            for (int i = 0; i < MathHelper.Clamp(10 - Projectile.numHits * 2, 2, 10); i++)
            {
                mainColor = Main.rand.Next(3) switch
                {
                    0 => Color.OrangeRed,
                    1 => Color.MediumTurquoise,
                    _ => Color.LawnGreen,
                };
                Particle mini = new CustomPulse(target.Center, ((Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -35).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 1f), mainColor, "CalamityMod/Particles/HealingPlus", Vector2.One, Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(0.8f, 1.4f), 0.2f, 35, true);
                GeneralParticleHandler.SpawnParticle(mini);

                Dust dust2 = Dust.NewDustPerfect(target.Center, 278, new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.7f));
                dust2.scale = Main.rand.NextFloat(0.55f, 0.85f);
                dust2.noGravity = true;
                dust2.color = Color.Lerp(Color.White, mainColor, 0.5f);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 1f;
            int hitsToMinMult = 1;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if ((useAnim > 0 || DrawUnconditionally) && Owner.ItemAnimationActive)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
                Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/EarthGlow");

                float r = (FlipAsSword ? MathHelper.ToRadians(90) : 0f);

                for (int i = 0; i < 5; i++)
                {
                    Color auraColor = mainColor * 0.5f * fadeIn;
                    Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/EarthGhost").Value;
                    Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 17f).ToRotationVector2();
                    rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 1.5f);
                    Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition + rotationalDrawOffset + new Vector2(0, Owner.gfxOffY), centerTexture.Frame(1, FrameCount, 0, Frame), auraColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                }

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                Main.EntitySpriteDraw(glowTex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), glowTex.Frame(1, FrameCount, 0, Frame), Color.White, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(glowTex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));

            }
            return false;
        }
        public override void ResetStyle()
        {
        }
    }
}
