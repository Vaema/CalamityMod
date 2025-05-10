using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Steamworks;
using Mono.Cecil;
using Newtonsoft.Json.Serialization;
using CalamityMod.Dusts;

namespace CalamityMod.Projectiles.Ranged
{
    public class VoidragonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Voidragon>();
        public override float MaxOffsetLengthFromArm => 40f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -1f;
        public override float OffsetYDownwards => 5f;
        public override Vector2 GunTipPosition => Projectile.Center + (Projectile.velocity * 55).RotatedBy(-0.05f * Projectile.direction);

        public int Time = 0;
        public int beamTimer = 500;
        public int shotCounter = 0;
        public int framesBetweenShots = 0;
        public bool swapType = false;
        public bool firingBeam = false;

        public override void KillHoldoutLogic()
        {
            //If the player is dead, kill the holdout.
            if (Owner.CantUseHoldout() || HeldItem.type != Owner.ActiveItem().type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            //Spawn dust on the holdout's eyes based on the amount of damage scaling. Currently only works from one side
            for (int i = 0; i < 2; i++)
            {
                int dustType = ModContent.DustType<VoidDustInverted>();
                float rotMulti = Main.rand.NextFloat(0.3f, 1f);
                Dust dust = Dust.NewDustPerfect(GunTipPosition + (-Projectile.velocity.RotatedBy(0.15 * Projectile.direction) * 50), dustType);
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f) * (Owner.Calamity().sharkGunDamageScaling * 0.02f) - rotMulti * 0.1f;
                dust.noGravity = true;
                dust.velocity = new Vector2(-1.2f, -2).RotatedBy(Projectile.rotation).RotatedByRandom(rotMulti * 0.8f) * (Main.rand.NextFloat(1f, 3.2f) - rotMulti) * (Owner.Calamity().sharkGunDamageScaling * 0.015f);
                dust.alpha = Main.rand.Next(90, 150);
                dust.color = Main.rand.NextBool() ? Color.Indigo : Color.DarkBlue;
            }
            //Pause to give the effect of reloading
            if (Time == 30)
            {
                Player Owner = Main.player[Projectile.owner];
                SoundEngine.PlaySound(SoundID.Item149, Projectile.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    string goreType = "MegalodonMag";
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center + (-Projectile.velocity * 18), Projectile.velocity.RotatedBy(2f * -Owner.direction) * Main.rand.NextFloat(0.6f, 0.7f), Mod.Find<ModGore>(goreType).Type);
                }
                //Set the scaling to 0 whenever it reloads
                Owner.Calamity().sharkGunDamageScaling = 0;
            }
            if (Time >= 90)
            {
                if (framesBetweenShots == 0 && shotCounter <= 50)
                {
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 28;
                    #region Debug Text Display
                    //Main.NewText(shotCounter);
                    //Main.NewText(Owner.Calamity().sharkGunDamageScaling);
                    #endregion
                    #region Visuals and Sounds
                    SoundStyle fire = new("CalamityMod/Sounds/Item/GunShotTiny");
                    SoundEngine.PlaySound(fire with { Volume = 0.4f, Pitch = 0.1f, PitchVariance = 0.1f, MaxInstances = -1 }, Projectile.Center);
                    Particle sparker = new CritSpark(GunTipPosition, Vector2.Zero, Color.Gold, Color.LightGoldenrodYellow, 1.7f, 3, 0.5f, 3f);
                    GeneralParticleHandler.SpawnParticle(sparker);
                    Particle spark2 = new GlowSparkParticle(GunTipPosition, shootVelocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(1.2f, 1.5f), false, 15, 0.017f, Main.rand.NextBool() ? Color.Indigo : Color.BlueViolet, new Vector2(1.5f, 0.7f), true, false, 1.3f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                    for (int i = 0; i <= 4; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(3) ? 303 : 244, (shootVelocity * Main.rand.NextFloat(0.2f, 1.1f)).RotatedByRandom(0.4f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    }
                    #endregion

                    //How many frames between firing projectiles, and how far the gun moves backward to give the effect of recoil. Change this number to edit fire rate
                    framesBetweenShots = 3;
                    OffsetLengthFromArm -= 2.5f;
                    //Here we detect which ammo the bullets will use
                    Owner.PickAmmo(Owner.ActiveItem(), out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, !Main.rand.NextBool(4));
                    //Alternate between shooting bullets and void blasts. We seperate it into two different projectiles due to the bullets needing to use a Global Projectile to track the damage multiplier
                    if (!swapType)
                    {
                        Projectile scalingShot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
                        CalamityGlobalProjectile cgp = scalingShot.Calamity();
                        cgp.sharkBullets = true;
                        //Only eject casings from the gun if bullets are fired, preventing too many at once
                        if (Main.netMode != NetmodeID.Server)
                        {
                            string goreType = "MegalodonCasing";
                            Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center + (-Projectile.velocity * 18), -Projectile.velocity * 4f, Mod.Find<ModGore>(goreType).Type);
                        }
                    }
                    if (swapType)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), ModContent.ProjectileType<VoidBlast>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    swapType = !swapType;
                    shotCounter++;
                    //Allow a pause before firing the laser. The long delay lets the laser's charge sound play in full
                    if (shotCounter == 50)
                    {
                        framesBetweenShots = 150;
                        shotCounter++;
                        SoundStyle charge = new("CalamityMod/Sounds/Custom/MoonLordLaserCharge");
                        SoundEngine.PlaySound(charge with { Volume = 1.5f }, Projectile.Center, _ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame());
                    }
                }
                if (framesBetweenShots > 0)
                    framesBetweenShots--;
            }
            if (shotCounter == 51 && framesBetweenShots == 0)
            {
                Main.NewText(beamTimer);
                firingBeam = true;
                if (firingBeam && beamTimer > 0)
                {
                    #region Visuals and Sounds
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 30;
                 
                    //Extremely WIP visuals for the laser source
                    float orbScale = 0.2f * Main.rand.NextFloat(0.8f, 1.1f);
                    Color smokeColor = Color.Lerp(Color.DimGray, Color.DarkGray, Main.rand.NextFloat(0.2f, 0.6f));
                    Particle orb = new CustomPulse(GunTipPosition, Vector2.Zero, smokeColor * 0.7f, "CalamityMod/ExtraTextures/GreyscaleVortex", new Vector2(1, 1), Projectile.ai[2] * 0.45f, orbScale, orbScale * 1.1f, 6, false);
                    GeneralParticleHandler.SpawnParticle(orb);
                    Particle orb3 = new CustomPulse(GunTipPosition, Vector2.Zero, Color.Indigo, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.5f, 0.15f, 6);
                    GeneralParticleHandler.SpawnParticle(orb3);
                    Particle orb2 = new CustomPulse(GunTipPosition, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.1f, 0.07f, 6);
                    GeneralParticleHandler.SpawnParticle(orb2);

                    float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2();
                        Particle cross = new GlowSparkParticle(GunTipPosition, velocity, false, 6, 0.4f, Color.BlueViolet * 0.7f, new Vector2(0.07f, 0.08f), true, false);
                        GeneralParticleHandler.SpawnParticle(cross);
                    }
                    #endregion

                    //If the player hasn't hit any shots, increase the multiplier from 0 to 1 to avoid the rocket doing 0 damage
                    if (Owner.Calamity().sharkGunDamageScaling == 0)
                    {
                        Owner.Calamity().sharkGunDamageScaling++;
                    }
                    //Spawn the laser. Change beamTimer to edit its lifetime
                    //I fucking hate this laser by the way
                    if (Main.myPlayer == Projectile.owner && beamTimer == 500)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5, ModContent.ProjectileType<AbyssalFire>(), (int)(Projectile.damage * 0.05f) * Owner.Calamity().sharkGunDamageScaling, Projectile.knockBack, Projectile.owner, 0);
                    beamTimer--;
                }
                else
                {
                    firingBeam = false;
                    Projectile.Kill();
                }
            }
            Time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Time < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
