using System;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
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
    [PierceResistException]
    public class BasherHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<Basher>();

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Basher>();
        public override string Texture => "CalamityMod/Items/Weapons/Melee/Basher";
        public override float HitboxOutset => 60;

        public override Vector2 HitboxSize => new Vector2(80, 80);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public override Vector2 SpriteOrigin => new(-7, 65);
        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = true;
        public bool postSwing = false;
        public float fadeIn = 0;
        public int useAnim;
        public int swingCount;
        public bool finalFlip = false;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = TrueMeleeDamageClass.Instance;
        }
        public override void WhenSpawned()
        {
            Projectile.knockBack = 0;
            Projectile.scale = 1;
            Projectile.ai[1] = 1;
            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            mousePos = Owner.Calamity().mouseWorld;
            aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnim = Owner.itemAnimationMax;

            if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            else Owner.direction = 1;

            FlipAsSword = Owner.direction == -1 ? true : false;
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

            if (CanHit && Owner.Calamity().mouseRight)
                fadeIn = MathHelper.Lerp(fadeIn, 1, 0.1f);
            else
                fadeIn = MathHelper.Lerp(fadeIn, 0, 0.15f);


            if (!doSwing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;

                Projectile.numHits = 0;
                // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                mousePos = Owner.Calamity().mouseWorld;
                aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false;
                if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                else Owner.direction = 1;
                FlipAsSword = Owner.direction == -1 ? true : false;
                doSwing = true;
                swingCount++;
                finalFlip = false;
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

                if (AnimationProgress < (useAnim / 3))
                {
                    // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
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
                    if (!finalFlip)
                    {
                        FlipAsSword = Owner.direction < 0 ? true : false;
                    }

                    float time = (AnimationProgress) - (useAnim / 3);
                    float timeMax = useAnim - (useAnim / 3);

                    if (time == (int)(timeMax * 0.4f))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.35f, -0.55f) }, Projectile.Center);
                    }
                    if ( time > (int)(timeMax * 0.2f) && time < (int)(timeMax * 0.85f))
                    {
                        CanHit = true;

                        Vector2 particleVel = new Vector2(0, 7 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                        Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(10, 90), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)));

                        Dust dust = Dust.NewDustPerfect(particlePos, 79);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.85f, 1.3f);
                        dust.velocity = -particleVel.RotatedByRandom(0.2f);
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
                        for (int i = 0; i < (Owner.Calamity().mouseRight ? 3 : 2); i++)
                        {
                            Vector2 dustPos = Owner.Center + (new Vector2(90, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f));
                            Dust dust2 = Dust.NewDustPerfect(dustPos, i == 2 ? 278 : DustID.JungleTorch, Utils.DirectionTo(Owner.Center, dustPos) * Main.rand.NextFloat(1.2f, 2.5f));
                            dust2.scale = Main.rand.NextFloat(0.55f, 0.85f) * (Owner.Calamity().mouseRight ? 1.3f : 1);
                            dust2.noGravity = i == 2 ? false : true;
                            dust2.color = i == 2 ? Color.Chartreuse : default;
                        }
                    }   
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if ((damageDone <= 2 || (target.life <= 0 && target.realLife == -1)) && Projectile.numHits > 0)
                Projectile.numHits -= 1;

            SoundStyle fire = new("CalamityMod/Sounds/NPCHit/RavagerRockPillarHit", 3);
            SoundEngine.PlaySound(fire with { Volume = 0.65f, Pitch = -0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DrumTomHigh with { Volume = 0.45f, Pitch = -0.9f }, Projectile.Center);

            target.AddBuff(ModContent.BuffType<Irradiated>(), 300);
            target.AddBuff(BuffID.Poisoned, 90);

            Vector2 launchVel = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld);
            target.MoveNPC(launchVel, 7.5f * (Owner.Calamity().mouseRight ? 2.5f : 1), true);

            if (Owner.Calamity().mouseRight)
            {
                Owner.SetScreenshake(1.2f);

                SoundStyle fire3 = new("CalamityMod/Sounds/Item/DampExplosion");
                SoundEngine.PlaySound(fire3 with { Volume = 0.35f, Pitch = 0.7f }, Projectile.Center);
                for (int i = 0; i < MathHelper.Clamp(15 - Projectile.numHits * 3, 2, 15); i++)
                {
                    bool dType = i % 4 == 0;
                    Dust dust2 = Dust.NewDustPerfect(target.Center, dType ? 278 : DustID.JungleTorch, launchVel.RotatedByRandom(0.5f) * Main.rand.NextFloat(3.5f, 7f) * (dType ? 2 : 1));
                    dust2.scale = Main.rand.NextFloat(0.65f, 1.35f) * (dType ? 0.7f : 1f);
                    dust2.noGravity = true;
                    dust2.color = dType ? Color.Chartreuse : default;
                }
                // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                Vector2 playerLaunchVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * 10;
                Owner.velocity = playerLaunchVel;
            }

            for (int i = 0; i < MathHelper.Clamp(6 - Projectile.numHits * 2, 2, 6); i++)
            {
                Dust dust2 = Dust.NewDustPerfect(target.Center, Main.rand.NextBool(5) ? DustID.JungleTorch : 79, ((Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitY) * -20).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.7f));
                dust2.scale = Main.rand.NextFloat(0.75f, 1.25f);
                dust2.noGravity = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.3f;
            int hitsToMinMult = 5;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult * (Owner.Calamity().mouseRight ? 2.5f : 1);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if ((useAnim > 0 || DrawUnconditionally) && Owner.ItemAnimationActive)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                for (int i = 0; i < 8; i++)
                {
                    Color auraColor = Color.Chartreuse with { A = 0 } * 0.35f * fadeIn;
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 8 * fadeIn;
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), auraColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                }

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
            }
            return false;
        }
        public override void ResetStyle()
        {
        }
    }
}
