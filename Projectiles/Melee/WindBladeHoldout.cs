using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class WindBladeHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override float HitboxOutset => 60;

        public override Vector2 HitboxSize => new Vector2(70, 70);

<<<<<<< Updated upstream
        public override float HitboxRotationOffset()
        {
            return MathHelper.ToRadians(-45);
        }

        public override Vector2 SpriteOrigin()
        {
            return new(0, 80);
        }
=======
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public override Vector2 SpriteOrigin => new(0, 80);
>>>>>>> Stashed changes

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0f;
            Projectile.ai[1] = 1;
            base.OnSpawn(source);
        }

        public override void UseStyle()
        {
<<<<<<< Updated upstream
            Vector2 mousePos = Main.MouseWorld;
            // i don't know how cal handles Main.MouseWorld in netcode. todo: someone fix this up in multiplayer pls
            // - ennway
=======
            Vector2 mousePos = Owner.Calamity().mouseWorld;
>>>>>>> Stashed changes

            if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            else Owner.direction = 1;

            DrawUnconditionally = true;

            if (NumberOfAnimations % 4 < 2)
            {
                Projectile.rotation = Owner.AngleTo(mousePos) + MathHelper.ToRadians(45f);

                if (NumberOfAnimations % 4 == 0 && AnimationProgress < 10)
                    Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.4f);

                if (AnimationProgress < (Owner.itemAnimationMax / 3))
                {
                    CanHit = false;
                    if (AnimationProgress == 0)
                    {
                        Projectile.ai[1] = -Projectile.ai[1];
                    }
                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * Projectile.ai[1] * Owner.direction), 0.2f);
                }
                else
                {
                    CanHit = true;
                    if ((int)AnimationProgress == (int)(Owner.itemAnimationMax / 1.5f))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Owner.Center);
                    }
                    if ((int)AnimationProgress > (int)(Owner.itemAnimationMax / 1.5f))
                    {
<<<<<<< Updated upstream
                        GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Owner.Center + (new Vector2(Main.rand.Next(30, 100), 0).RotatedBy(FinalRotation() + MathHelper.ToRadians(-45))), new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation() + MathHelper.ToRadians(-45)), Color.LightSkyBlue, 40, 1f, 0.4f, MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)), true));
=======
                        GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Owner.Center + (new Vector2(Main.rand.Next(30, 100), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45))), new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)), Color.LightSkyBlue, 40, 1f, 0.4f, MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)), true));
>>>>>>> Stashed changes
                    }

                    float time = (AnimationProgress) - (Owner.itemAnimationMax / 3);
                    float timeMax = Owner.itemAnimationMax - (Owner.itemAnimationMax / 3);

                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(MathHelper.Lerp(150f * Projectile.ai[1] * Owner.direction, 120f * -Projectile.ai[1] * Owner.direction, CalamityUtils.ExpInOutEasing(time / timeMax, 1))),
                        0.2f);
                }

                FlipAsSword = Owner.direction == -1 ? true : false;
            }
            else if (NumberOfAnimations % 4 == 3)
            {
<<<<<<< Updated upstream
=======
                float ProjectileSpeed = 10;
                int ProjectileDamage = 10;
                float ProjectileKnockback = Projectile.knockBack;

>>>>>>> Stashed changes
                CanHit = true;
                if ((int)AnimationProgress == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFurySwing, Owner.Center);
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing.WithPitchOffset(-0.4f), Owner.Center);
<<<<<<< Updated upstream
=======

                    Projectile.NewProjectile(new EntitySource_ItemUse(Owner, Owner.HeldItem), Projectile.Hitbox.Center.ToVector2(), new Vector2(ProjectileSpeed, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)), ModContent.ProjectileType<WindBolt>(), ProjectileDamage, ProjectileKnockback, Owner.whoAmI);
>>>>>>> Stashed changes
                }
                RotationOffset = 0f;
                if (AnimationProgress < Owner.itemAnimationMax / 3)
                {
                    Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.5f, 0.4f);

<<<<<<< Updated upstream
                    float rot = FinalRotation() + MathHelper.ToRadians(-45);
=======
                    float rot = FinalRotation + MathHelper.ToRadians(-45);
>>>>>>> Stashed changes

                    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Owner.Center + new Vector2(105 * Projectile.scale, 0).RotatedBy(rot), new Vector2(5, 0).RotatedBy(rot + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20))), Color.LightSkyBlue, 30, 2f, 0.3f, MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)), true));
                    GeneralParticleHandler.SpawnParticle(new LineParticle(Owner.Center + new Vector2(0, 20).RotatedBy(rot) + new Vector2(45 * Projectile.scale, 0).RotatedBy(rot), new Vector2(5, 0).RotatedBy(rot), false, 30, 1f, Color.LightSkyBlue));
                    GeneralParticleHandler.SpawnParticle(new LineParticle(Owner.Center + new Vector2(0, -20).RotatedBy(rot) + new Vector2(45 * Projectile.scale, 0).RotatedBy(rot), new Vector2(5, 0).RotatedBy(rot), false, 30, 1f, Color.LightSkyBlue));
                }
                else
                {
                    Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.5f, -0.2f);
                }
            }
            else
            {
                CanHit = false;

                RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(MathHelper.Lerp(150f * -Projectile.ai[1] * Owner.direction, 120f * Projectile.ai[1] * Owner.direction, 0.1f)),
                    0.2f);

                Projectile.rotation = Owner.AngleTo(mousePos) + MathHelper.ToRadians(45f);
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 0f, 0.2f);
            }

            ArmRotationOffset = MathHelper.ToRadians(-90f);
        }

        public override void ResetStyle()
        {
        }
    }
}
