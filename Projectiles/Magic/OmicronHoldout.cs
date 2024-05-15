using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class OmicronHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Omicron>();
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitY.RotatedBy(Projectile.rotation) * 4f * Projectile.spriteDirection;
        public override float MaxOffsetLengthFromArm => 10f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;

        public Color StaticEffectsColor = Color.MediumVioletRed;

        public float FiringTime = 15;
        public float Windup = 60;
        public bool WindingUp = false;
        public int time = 0;
        public int PostFireCooldown = 0;
        public bool fireYBeam = false;
        public int maxFirerateShots = 0;

        private ref float ShootingTimer => ref Projectile.ai[0];

        public override void KillHoldoutLogic()
        {
            if (Owner is null || !Owner.active || Owner.CCed || Owner.dead || Owner.noItems)
            {
                if (PostFireCooldown <= 0)
                    Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }

        public override void HoldoutAI()
        {
            Item heldItem = Owner.ActiveItem();

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout())
            {
                if (PostFireCooldown <= 0)
                    Projectile.Kill();
            }

            if (PostFireCooldown > 0)
            {
                PostFiringCooldown();
            }

            if (Owner.Calamity().mouseRight && PostFireCooldown <= 0)
            {
                if (Owner.CheckMana(Owner.ActiveItem(), (int)(heldItem.mana * Owner.manaCost) * 16, true, false))
                {
                    PostFireCooldown = 100;
                    Shoot(heldItem, true);
                    ShootingTimer = 0;
                }
                else
                {
                    if (Projectile.soundDelay <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                        Projectile.soundDelay = 50;
                        ShootingTimer = 0;
                    }
                }
            }
            else if (ShootingTimer >= FiringTime)
            {
                if (Owner.CheckMana(Owner.ActiveItem(), -1, true, false) && PostFireCooldown <= 0)
                {
                    maxFirerateShots++;

                    if (maxFirerateShots == 5)
                    {
                        Windup = 60;
                        maxFirerateShots = 0;
                    }

                    Shoot(heldItem, false);

                    ShootingTimer = 0;

                    if (Windup > 10)
                    {
                        if (maxFirerateShots > 0)
                            Windup -= 12;
                    }
                    else
                    {
                        Windup = 10;
                    }
                }
                else if (PostFireCooldown <= 0)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                    Projectile.Kill();
                }

            }
            ShootingTimer++;
        }
        private void Shoot(Item item, bool yBeam)
        {
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);

            // Spawns the projectile.

            Vector2 firingVelocity1 = (shootDirection * 8).RotatedBy(0.1f * Utils.GetLerpValue(10, 55, Windup, true));
            Vector2 firingVelocity2 = (shootDirection * 8).RotatedBy(-0.1f * Utils.GetLerpValue(10, 55, Windup, true));
            Vector2 firingVelocity3 = (shootDirection * 10);

            if (yBeam)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/OmicronBeam");
                SoundEngine.PlaySound(fire with { Volume = 0.9f }, Projectile.Center);

                for (int k = 0; k < 6; k++)
                {
                    Particle pulse2 = new GlowSparkParticle(GunTipPosition, shootDirection * 28, false, 8, 0.087f, StaticEffectsColor, new Vector2(2.3f, 0.9f), true);
                    GeneralParticleHandler.SpawnParticle(pulse2);
                }

                Owner.Calamity().GeneralScreenShakePower = 6.5f;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, firingVelocity3, ModContent.ProjectileType<OmicronBeam>(), Projectile.damage * 32, Projectile.knockBack, Projectile.owner, 0, 0);

                for (int i = 0; i < 8; i++)
                {
                    SparkParticle spark2 = new SparkParticle(GunTipPosition + Main.rand.NextVector2Circular(10, 10), firingVelocity3 * Main.rand.NextFloat(0.7f, 1.3f), false, Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.55f), StaticEffectsColor);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/ArcNovaDiffuserBigShot");
                SoundEngine.PlaySound(fire with { Volume = 0.2f, Pitch = 0.9f }, Projectile.Center);

                for (int i = 0; i < 5; i++)
                {
                    firingVelocity3 = (shootDirection * 10).RotatedBy((0.04f * (i + 1)) * Utils.GetLerpValue(0, 55, Windup, true));
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, firingVelocity3 * (1 - i * 0.1f), ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                }
                for (int i = 0; i < 5; i++)
                {
                    firingVelocity3 = (shootDirection * 10).RotatedBy((-0.04f * (i + 1)) * Utils.GetLerpValue(0, 55, Windup, true));
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, firingVelocity3 * (1 - i * 0.1f), ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                }

                Particle pulse3 = new GlowSparkParticle(GunTipPosition, shootDirection * 18, false, 6, 0.057f, StaticEffectsColor, new Vector2(1.7f, 0.8f), true);
                GeneralParticleHandler.SpawnParticle(pulse3);
            }

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            for (int k = 0; k < 10; k++)
            {
                Vector2 shootVel = (shootDirection * 15).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.8f);

                Dust dust2 = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(4) ? 267 : 66, shootVel);
                dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Lerp(StaticEffectsColor, Color.White, 0.5f) : StaticEffectsColor;
            }

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            if (yBeam)
                OffsetLengthFromArm -= 32f;
            else
                OffsetLengthFromArm -= 6f;
        }
        private void PostFiringCooldown()
        {
            Owner.channel = true;
            if (PostFireCooldown > 0 && Main.rand.NextBool())
            {
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel, StaticEffectsColor, Main.rand.Next(30, 50 + 1), Main.rand.NextFloat(0.1f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(GunTipPosition, 303, smokeVel.RotatedByRandom(0.1f), 80, default, Main.rand.NextFloat(0.2f, 0.8f));
                dust.noGravity = false;
                dust.color = StaticEffectsColor;
            }
            ShootingTimer = 0;
            PostFireCooldown--;
        }
    }
}
