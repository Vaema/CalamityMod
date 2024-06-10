using System;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
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
    public class GrandDadHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<GrandDad>();

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<GrandDad>();
        public override string Texture => "CalamityMod/Items/Weapons/Melee/GrandDad";
        public override float HitboxOutset => 125;

        public override Vector2 HitboxSize => new Vector2(170, 170);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public override Vector2 SpriteOrigin => new(0, 124);
        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = true;
        public bool postSwing = false;
        public float fadeIn = 0;
        public int useAnim;
        public int swingCount;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = TrueMeleeDamageClass.Instance;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 1;
            Projectile.ai[1] = 1;
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
                        SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
                    }
                    if ( time > (int)(timeMax * 0.4f) && time < (int)(timeMax * 0.7f))
                    {
                        CanHit = true;

                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 particleVel = new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                            Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(30, 170), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)));
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
                        for (int i = 0; i < 3; i++)
                        {
                            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Owner.Center + (new Vector2(170, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), Vector2.Zero, false, 24, Main.rand.NextFloat(0.5f, 1f), Main.rand.NextBool(4) ? Color.Gold : Color.Goldenrod));
                        }
                    }   
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (damageDone <= 2 && Projectile.numHits > 0)
                Projectile.numHits -= 1;

            SoundStyle fire = new("CalamityMod/Sounds/NPCHit/ThanatosHitOpen1");
            SoundEngine.PlaySound(fire with { Volume = 0.75f, Pitch = -0.1f }, Projectile.Center);
            SoundStyle fire2 = new("CalamityMod/Sounds/Item/FinalDawnSlash");
            SoundEngine.PlaySound(fire2 with { Volume = 0.65f, Pitch = Main.rand.NextFloat(-0.2f, -0.3f) }, Projectile.Center);
            if (Main.zenithWorld && Projectile.numHits == 0 && target.type != ModContent.NPCType<PrimordialWyrmHead>() && Main.rand.NextBool(5))
            {
                SoundStyle fire3 = new("CalamityMod/Sounds/Item/GFBScreams/Scream", 5);
                SoundEngine.PlaySound(fire3 with { Volume = 0.8f }, Projectile.Center);
            }
            if (Projectile.numHits == 0)
            {
                Owner.Calamity().GeneralScreenShakePower = 6.5f;
            }

            int heal = (int)(MathHelper.Clamp(12 - Projectile.numHits * 3, 1, 12));
            if (Main.player[Main.myPlayer].lifeSteal > 0f)
            {
                Owner.lifeSteal -= heal;
                Owner.statLife += heal;
                Owner.HealEffect(heal);
                if (Owner.statLife > Owner.statLifeMax2)
                    Owner.statLife = Owner.statLifeMax2;
            }

            bool isAPillar = target.type == NPCID.LunarTowerSolar || target.type == NPCID.LunarTowerVortex || target.type == NPCID.LunarTowerNebula || target.type == NPCID.LunarTowerStardust;
            if (!isAPillar && !target.boss && target.IsAnEnemy(true, true, false) && !CalamityPlayer.areThereAnyDamnBosses && target.CanBeChasedBy(Projectile, false) || Main.zenithWorld || target.type == ModContent.NPCType<PrimordialWyrmHead>())
            {
                if (target.type == ModContent.NPCType<PrimordialWyrmHead>())
                {
                    CombatText.NewText(target.Hitbox, Color.Aqua, "boop!");
                    SoundStyle boop = new("CalamityMod/Sounds/Item/SnootBooped");
                    SoundEngine.PlaySound(boop with {Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.Center);
                }

                bool rightClicked = Owner.Calamity().mouseRight;

                // Make all hit enemies able to hit tiles, so you can dunk them
                target.noTileCollide = false;

                // Launch the suckers
                Vector2 launchVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * (Main.zenithWorld ? -50 : -30) * (rightClicked ? 2 : 1);
                target.velocity = launchVel * 0.5f;

                // Remove knockback resist, just like it used to
                target.knockBackResist = 1;

                // Apply tile collison damage (is bonus on GFB and even further is both final bosses are gone)
                target.FallingNPC().ApplyFallDamage(target, Projectile.damage * (Main.zenithWorld ? (DownedBossSystem.downedCalamitas && DownedBossSystem.downedExoMechs ? 1000 : 77) : 3) * (rightClicked ? 3 : 1), launchVel, 5f, true);
            }
            Particle spark = new VoidSparkParticle(target.Center, (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -35, false, 13, 0.5f, Color.DodgerBlue);
            GeneralParticleHandler.SpawnParticle(spark);
            for (int i = 0; i < MathHelper.Clamp(10 - Projectile.numHits * 2, 2, 10); i++)
            {
                Particle spark2 = new SparkParticle(target.Center, ((Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -35).RotatedByRandom(0.7) * Main.rand.NextFloat(0.2f, 1f), true, 55, Main.rand.NextFloat(0.4f, 1.5f), Main.rand.NextBool(4) ? Color.DodgerBlue : Color.Blue);
                GeneralParticleHandler.SpawnParticle(spark2);
                if (Main.rand.NextBool(3))
                {
                    Particle spark3 = new AltSparkParticle(target.Center, ((Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -35).RotatedByRandom(0.7) * Main.rand.NextFloat(0.2f, 1f), true, 55, Main.rand.NextFloat(0.4f, 1.5f), Color.Black);
                    GeneralParticleHandler.SpawnParticle(spark3);
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Owner.Calamity().mouseRight)
                modifiers.SourceDamage *= 0.0025f;
            else
            {
                float minMult = 0.5f;
                int hitsToMinMult = 7;
                float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
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

                for (int i = 0; i < 5; i++)
                {
                    Color auraColor = Color.Lerp(Color.DarkGoldenrod, Color.Gold, Utils.GetLerpValue(0, 5, i)) * 0.5f * fadeIn;
                    Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GrandDadGhost").Value;
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
