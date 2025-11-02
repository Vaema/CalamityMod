using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    [PierceResistException]
    public class GrandDadHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<GrandDad>();

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<GrandDad>();
        public override string Texture => "CalamityMod/Items/Weapons/Melee/GrandDad";
        public override float HitboxOutset => 125;

        public override Vector2 HitboxSize => new Vector2(190, 190);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public override Vector2 SpriteOrigin => new(0, 124);
        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = true;
        public bool postSwing = false;
        public float fadeIn = 0;
        public int useAnim;
        public int swingCount;
        public bool finalFlip = false;
        public bool swingSound = true;
        public int armoredHits = 0;

        private NPC target => Main.npc[(int)Projectile.ai[0]];

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = TrueMeleeDamageClass.Instance;
        }

        public override void WhenSpawned()
        {
            Projectile.timeLeft = Owner.HeldItem.useAnimation + 1;
            Projectile.knockBack = 0;
            Projectile.scale = 1;
            Projectile.ai[1] = -1;

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, as Grand Dad has no projectiles
            mousePos = Owner.Calamity().mouseWorld;
            aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnim = Owner.itemAnimationMax;

            if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            else Owner.direction = 1;

            FlipAsSword = Owner.direction == -1;
        }

        public override void UseStyle()
        {
            AnimationProgress = Animation % useAnim;
            DrawUnconditionally = false;

            if (CanHit || postSwing)
                mousePos = Owner.Center - aimVel;
            else
            {
                mousePos = Owner.Calamity().mouseWorld;
            }

            if (CanHit)
                fadeIn = MathHelper.Lerp(fadeIn, 1, 0.3f);
            else
                fadeIn = MathHelper.Lerp(fadeIn, 0, 0.35f);


            if (!doSwing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;

                Projectile.numHits = 0;
                mousePos = Owner.Calamity().mouseWorld;
                aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false;
                if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                else Owner.direction = 1;
                FlipAsSword = Owner.direction == -1;

                doSwing = true;
                swingCount++;
                finalFlip = false;
                swingSound = true;
                armoredHits = 0;
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


                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.AngleTo(mousePos) + MathHelper.ToRadians(45f), 0.1f);

                if (AnimationProgress < (useAnim / 1.5f))
                {
                    aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                    CanHit = false;
                    postSwing = false;
                    if (AnimationProgress == 0)
                    {
                        doSwing = false;
                        Projectile.ai[1] = -Projectile.ai[1];
                    }
                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * Projectile.ai[1] * Owner.direction * (1 + (Utils.GetLerpValue(useAnim * 0.8f, useAnim, Animation, true)) * 0.35f)), 0.2f);
                }
                else
                {
                    if (!finalFlip)
                    {
                        FlipAsSword = Owner.direction < 0;
                    }

                    float time = (AnimationProgress) - (useAnim / 3);
                    float timeMax = useAnim - (useAnim / 3);

                    if (time >= (int)(timeMax * 0.4f) && swingSound)
                    {
                        SoundStyle fire = new("CalamityMod/Sounds/Item/HeavySwing");
                        SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = Main.rand.NextFloat(0.2f, 0.32f) }, Projectile.Center);
                        swingSound = false;
                    }
                    if (time > (int)(timeMax * 0.4f) && time < (int)(timeMax * 0.7f))
                    {
                        CanHit = true;

                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 particleVel = new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                            Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(30, 165), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)));
                            GeneralParticleHandler.SpawnParticle(new LineParticle(particlePos, -particleVel.RotatedByRandom(0.2f) * 2, false, 19, Main.rand.NextFloat(0.5f, 1f), Main.rand.NextBool(4) ? Color.DodgerBlue : Color.Blue));
                            GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(particlePos, -particleVel.RotatedByRandom(0.2f) * 2, Main.rand.NextBool(4) ? Color.Black : Color.DarkBlue, 23, Main.rand.NextFloat(0.5f, 1f), 0.65f));
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
                        for (int i = 0; i < 6; i++)
                        {
                            if (Main.rand.NextBool(3))
                            {
                                Dust dust = Dust.NewDustPerfect(Owner.Center + (new Vector2(180, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), ModContent.DustType<VoidDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(1.15f, 1.5f));
                                dust.noGravity = true;
                                dust.color = Color.DodgerBlue;
                                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Owner.Center + (new Vector2(180, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), Vector2.Zero, false, 23, Main.rand.NextFloat(0.5f, 1f), Color.Black, false, false, false));
                            }
                            else
                            {
                                Dust dust = Dust.NewDustPerfect(Owner.Center + (new Vector2(180, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), ModContent.DustType<LightDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(1.15f, 1.5f));
                                dust.noGravity = true;
                                dust.color = Color.Blue;
                                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Owner.Center + (new Vector2(180, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), Vector2.Zero, false, 23, Main.rand.NextFloat(0.5f, 1f), Main.rand.NextBool(4) ? Color.DodgerBlue : Color.Blue));
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            float randRot = Main.rand.NextFloat(-30, -60);
                            Vector2 dustVel = (new Vector2(0, 15 * -Projectile.ai[1] * Owner.direction)).RotatedBy(FinalRotation + MathHelper.ToRadians(randRot));
                            Dust dust2 = Dust.NewDustPerfect(Owner.Center + (new Vector2(185, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), 278, dustVel * Main.rand.NextFloat(0.1f, 0.5f));
                            dust2.scale = Main.rand.NextFloat(0.55f, 1.05f);
                            dust2.noGravity = true;
                            dust2.color = Main.rand.NextBool(3) ? Color.Goldenrod : Color.Gold;
                        }
                    }
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if ((target.life <= 0 && target.realLife == -1) && Projectile.numHits > 0)
                Projectile.numHits -= 1;
            if (damageDone <= 2)
                armoredHits++;

            if (Main.zenithWorld && Projectile.numHits == 0 && target.type != ModContent.NPCType<PrimordialWyrmHead>() && Main.rand.NextBool(5))
            {
                SoundStyle fire3 = new("CalamityMod/Sounds/Item/GFBScreams/Scream", 8);
                SoundEngine.PlaySound(fire3 with { Volume = 0.8f }, Projectile.Center);
            }
            if (Projectile.numHits == 0)
            {
                Owner.SetScreenshake(6.5f);
                SoundStyle fire = new("CalamityMod/Sounds/NPCHit/ThanatosHitOpen1");
                SoundEngine.PlaySound(fire with { Volume = 0.75f, Pitch = -0.1f }, Projectile.Center);
                SoundStyle fire2 = new("CalamityMod/Sounds/Item/FinalDawnSlash");
                SoundEngine.PlaySound(fire2 with { Volume = 0.65f, Pitch = Main.rand.NextFloat(-0.2f, -0.3f) }, Projectile.Center);
            }

            int heal = (int)(MathHelper.Clamp(20 - Projectile.numHits * 12, 1, 20));
            if (Projectile.numHits < 10)
            {
                Owner.DoLifestealDirect(target, heal, 0.5f);
            }

            if (target.CanBeMoved(true) || Main.zenithWorld || target.type == ModContent.NPCType<PrimordialWyrmHead>() || (DownedBossSystem.downedCalamitas && DownedBossSystem.downedExoMechs))
            {
                if (target.type == ModContent.NPCType<PrimordialWyrmHead>())
                {
                    CombatText.NewText(target.Hitbox, Color.Aqua, CalamityUtils.GetTextValue("Misc.HecBoop"));
                    SoundStyle boop = new("CalamityMod/Sounds/Item/SnootBooped");
                    SoundEngine.PlaySound(boop with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.Center);
                }

                bool rightClicked = Owner.Calamity().mouseRight;

                // Make all hit enemies able to hit tiles, so you can dunk them
                target.noTileCollide = false;

                // Launch the suckers
                Vector2 launchVel = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld);
                float launchPower = (Main.zenithWorld ? 50 : 30) * (rightClicked ? 2 : 1);
                target.MoveNPC(launchVel, launchPower * 0.5f, true);

                // Remove knockback resist, just like it used to
                target.knockBackResist = 1;

                // Apply tile collison damage (is bonus on GFB and even further is both final bosses are gone)
                float damageMults = ((DownedBossSystem.downedCalamitas && DownedBossSystem.downedExoMechs) ? 5 : 1) * (Main.zenithWorld ? 77 : 1) * (rightClicked ? 3 : 1);
                int damage = (int)(Projectile.damage * damageMults);
                target.FlungNPC().ApplyCollisionDamage(target, Owner, damage, launchVel * launchPower, 5f, true);
            }

            if (Projectile.numHits < 3)
            {
                Particle spark = new VoidSparkParticle(target.Center, (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * (-45 + Projectile.numHits * 5), false, (int)(16 - Projectile.numHits * 3), 0.6f - Projectile.numHits * 0.15f, Color.DodgerBlue, 0.45f);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < MathHelper.Clamp(10 - Projectile.numHits * 2, 2, 10); i++)
            {
                Particle spark2 = new SparkParticle(target.Center, ((Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -35).RotatedByRandom(0.7) * Main.rand.NextFloat(0.2f, 1f), false, 55, Main.rand.NextFloat(0.4f, 1.5f), Main.rand.NextBool(4) ? Color.DodgerBlue : Color.Blue);
                GeneralParticleHandler.SpawnParticle(spark2);
                Dust dust = Dust.NewDustPerfect(target.Center, ModContent.DustType<VoidDust>(), ((Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -35).RotatedByRandom(0.7) * Main.rand.NextFloat(0.2f, 1f), 0, default, Main.rand.NextFloat(1.55f, 2.2f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool() ? Color.DodgerBlue : Color.Blue;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Owner.Calamity().mouseRight)
            {
                modifiers.SourceDamage *= 0;
                modifiers.FinalDamage.Flat = 0.1f;
            }
            else
            {
                float minMult = 0.5f;
                int hitsToMinMult = 15;
                float damageMult = Utils.Remap(Projectile.numHits - armoredHits, 0, hitsToMinMult, 1, minMult, true);
                modifiers.SourceDamage *= damageMult;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if ((useAnim > 0 || DrawUnconditionally) && Owner.ItemAnimationActive)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
                Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GrandDadGlow");

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                Asset<Texture2D> swoosh = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmearLarge");
                if (Animation > useAnim * 0.2f)
                    Main.EntitySpriteDraw(swoosh.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), null, Color.DodgerBlue with { A = 0 } * fadeIn * 0.65f, (FinalRotation + MathHelper.ToRadians(45)) + MathHelper.ToRadians(Projectile.ai[1] == 1 ? -90 : 90) * -Owner.direction, swoosh.Size() * 0.5f, Projectile.scale * 0.6f, SpriteEffects.None);

                for (int i = 0; i < 25; i++)
                {
                    Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GrandDadGhost").Value;
                    Color auraColor = Color.Gold with { A = 0 } * 0.15f * fadeIn;
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * 6 * fadeIn;
                    Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition + drawOffset + new Vector2(0, Owner.gfxOffY), centerTexture.Frame(1, FrameCount, 0, Frame), auraColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                }

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                Main.EntitySpriteDraw(glowTex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), glowTex.Frame(1, FrameCount, 0, Frame), Color.White, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(glowTex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));

            }
            return false;
        }
    }
}
