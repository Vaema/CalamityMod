using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class PureClarity : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override int AssignedItemID => ModContent.ItemType<BrokenBiomeBlade>();
        public override string Texture => "CalamityMod/Items/Weapons/Melee/BrokenBiomeBlade";
        public override float HitboxOutset => 30f;
        public override Vector2 HitboxSize => new Vector2(36, 36);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);
        public override Vector2 SpriteOrigin => new(0, 12);

        public ref float SwingDir => ref Projectile.ai[1];
        public Vector2 mousePos;
        public Vector2 aimPos;
        public bool doSwing = true;
        public bool postSwing = false;
        public int useAnimation;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SwingDir = 1f;
            base.OnSpawn(source);
            mousePos = Owner.Calamity().mouseWorld;
            aimPos = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnimation = Owner.itemAnimationMax;

            Owner.direction = (mousePos.X < Owner.Center.X) ? -1 : 1;
            FlipAsSword = Owner.direction == -1 ? true : false;
        }

        public override void UseStyle()
        {
            if (CanHit || postSwing)
                mousePos = Owner.Center - aimPos;
            else
                mousePos = Owner.Calamity().mouseWorld;

            if (!doSwing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;

                mousePos = Owner.Calamity().mouseWorld;
                aimPos = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false;
                Owner.direction = (mousePos.X < Owner.Center.X) ? -1 : 1;
                FlipAsSword = Owner.direction == -1 ? true : false;
                doSwing = true;
            }
            else
            {
                if (!CanHit && !postSwing)
                    Owner.direction = (mousePos.X < Owner.Center.X) ? -1 : 1;
                else
                    Owner.direction = ((Owner.Center - aimPos).X < Owner.Center.X) ? -1 : 1;
                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.AngleTo(mousePos) + MathHelper.ToRadians(65f), 0.1f);

                if (AnimationProgress < (useAnimation / 3))
                {
                    aimPos = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                    CanHit = false;
                    postSwing = false;
                    if (AnimationProgress == 0)
                    {
                        doSwing = false;
                        SwingDir = -SwingDir;
                    }
                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * SwingDir * Owner.direction), 0.2f);
                }
                else
                {
                    float swingTime = AnimationProgress - (useAnimation / 3);
                    float swingTimeMax = useAnimation - (useAnimation / 3);

                    if (swingTime > (int)(swingTimeMax * 0.2f) && swingTime < (int)(swingTimeMax * 0.85f))
                    {
                        CanHit = true;

                        Vector2 particleVel = new Vector2(0, 10 * -SwingDir * Owner.direction).RotatedBy(FinalRotation - MathHelper.PiOver4);
                        Vector2 particlePos = Owner.Center + new Vector2(Main.rand.Next(10, 90), 0).RotatedBy(FinalRotation - MathHelper.PiOver4);
                        Color particleColor = (Owner.HeldItem.ModItem as BrokenBiomeBlade).mainAttunement.tooltipColor;
                        if (Main.rand.NextBool())
                        {
                            GenericBloom bloom = new(particlePos, particleVel, particleColor, 0.08f, 20);
                            GeneralParticleHandler.SpawnParticle(bloom);
                        }
                        else
                        {
                            GenericSparkle sparkle = new(particlePos, particleVel, particleColor, particleColor, 0.55f, 20);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                    else
                        CanHit = false;

                    if (swingTime == (int)(swingTimeMax * 0.4f))
                    {
                        Vector2 projVel = -aimPos.SafeNormalize(Vector2.UnitX) * Owner.HeldItem.shootSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, projVel, ModContent.ProjectileType<PurityProjection>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                    }

                    RotationOffset = MathHelper.Lerp(RotationOffset,
                        MathHelper.ToRadians(MathHelper.Lerp(150f * SwingDir * Owner.direction, 120f * -SwingDir * Owner.direction, CalamityUtils.ExpInOutEasing(swingTime / swingTimeMax, 1))),
                        0.2f);

                    if (swingTime >= swingTimeMax)
                        doSwing = false;
                    if (swingTime < (int)(swingTimeMax * 0.7f))
                        postSwing = true;
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if ((useAnimation > 0 || DrawUnconditionally) && Owner.ItemAnimationActive)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
            }
            return false;
        }

        public override void ResetStyle()
        {
        }
    }
}
