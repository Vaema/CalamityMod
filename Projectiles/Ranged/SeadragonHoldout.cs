using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Projectiles.Ranged
{
    public class SeadragonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Seadragon>();
        public override float MaxOffsetLengthFromArm => 24f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -1f;
        public override float OffsetYDownwards => 5f;

        public int Time = 0;
        public int shotCounter = 0;
        public int framesBetweenShots = 0;
        public bool swapType = false;
        public bool transEffects = false;

        public override Vector2 GunTipPosition => Projectile.Center - Vector2.UnitY + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.5f;


        public override void KillHoldoutLogic()
        {
            //If the player is dead, kill the holdout.
            if (Owner.CantUseHoldout() || HeldItem.type != AssociatedItemID)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            //Pause to give the effect of reloading
            if (Time == 30)
            {
                Player Owner = Main.player[Projectile.owner];
                SoundEngine.PlaySound(SoundID.Item149, Projectile.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    string goreType = "SeadragonMag";
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(2f * -Owner.direction) * Main.rand.NextFloat(0.6f, 0.7f), Mod.Find<ModGore>(goreType).Type);
                }
                //Set the scaling to 0 whenever it reloads
                Owner.Calamity().sharkGunDamageScaling = 0;
                //Has a 1 in 100 chance to set transEffects to true, changing the shot colors to be replaced by the transgender pride flag colors
                transEffects = Main.rand.NextBool(100);
            }
            if (Time >= 90)
            {
                if (framesBetweenShots == 0 && shotCounter <= 51)
                {
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 30;
                    #region Debug Text Display
                    //Main.NewText(shotCounter);
                    //Main.NewText(Owner.Calamity().sharkGunDamageScaling);
                    #endregion
                    #region Visuals and Sounds
                    SoundStyle fire = new("CalamityMod/Sounds/Item/GunShotTiny");
                    SoundEngine.PlaySound(fire with { Volume = 0.3f, Pitch = 0.25f , PitchVariance = 0.1f, MaxInstances = -1 }, Projectile.Center);
                    Particle sparker = new CustomPulse(GunTipPosition, Vector2.Zero, Color.HotPink, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.02f, 0.5f, 5, true, 0.8f);
                    GeneralParticleHandler.SpawnParticle(sparker);
                    GenericSparkle sparker2 = new GenericSparkle(GunTipPosition, Vector2.Zero, Main.rand.NextBool() ? Color.SeaShell : Color.Silver * 0.9f, Color.HotPink, Main.rand.NextFloat(0.8f, 1.2f), 2, 0, 2.68f);
                    GeneralParticleHandler.SpawnParticle(sparker2);
                    for (int i = 0; i <= 4; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(3) ? 303 : 244, (shootVelocity * Main.rand.NextFloat(0.2f, 1.1f)).RotatedByRandom(0.4f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    }
                    #endregion

                    //How many frames between firing projectiles, and how far the gun moves backward to give the effect of recoil. Change this number to edit fire rate
                    framesBetweenShots = 3;
                    OffsetLengthFromArm -= 2f;
                    //Here we detect which ammo the bullets will use
                    Owner.PickAmmo(Owner.HeldItem, out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _);
                    //Alternate between shooting bullets and water streams. Despite not having damage scaling, the fish gain movement speed based on the scaling, so we still need a Global Projectile
                    if (!swapType)
                    {
                        Projectile scalingShot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
                        CalamityGlobalProjectile cgp = scalingShot.Calamity();
                        cgp.sharkBullets = true;
                        //Only eject casings from the gun if bullets are fired, preventing too many at once
                        if (Main.netMode != NetmodeID.Server)
                        {
                            string goreType = "SeadragonCasing";
                            Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center + (-Projectile.velocity * 6), -Projectile.velocity * 4f, Mod.Find<ModGore>(goreType).Type);
                        }
                    }
                    if (swapType)
                    {
                        //Has a 1 in 100 chance for the shot colors to be replaced by the transgender pride flag colors
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), ModContent.ProjectileType<BlahajBlast>(), Projectile.damage, Projectile.knockBack, Projectile.owner, transEffects ? 0f : 1f);
                    }
                    swapType = !swapType;
                    shotCounter++;
                    //Allow a pause before unleashing the fish. This allows the final bullet a chance to hit before the fish are unleashed. Lowering this number reduces the delay, but may also cause the gun to become inconsistent
                    if (shotCounter == 50)
                        framesBetweenShots = 18;
                    if (Main.zenithWorld && shotCounter == 35)
                    {
                        SoundStyle joke = new("CalamityMod/Sounds/Custom/GFB/YouKnowWhatThatMeans");
                        SoundEngine.PlaySound(joke with { Volume = 1f }, Projectile.Center);
                    }
                }
                if (framesBetweenShots > 0)
                    framesBetweenShots--;
            }
            if (shotCounter == 50 && framesBetweenShots == 0)
            {
                //Reset all variables to allow left click to be held down
                Time = 2;
                shotCounter = 0;
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    //Activate all fish to rush at the cursor and home in on the nearest enemy
                    Projectile projectile = Main.projectile[x];
                    if (projectile.active && projectile.type == ModContent.ProjectileType<SeaDragonRocket>() && projectile.ai[1] < 5)
                    {
                        projectile.ai[1] = 5;
                        projectile.velocity = Utils.DirectionTo(projectile.Center, Owner.Calamity().mouseWorld) * 12;
                    }
                    if (Main.zenithWorld)
                    {
                        SoundStyle joke = new("CalamityMod/Sounds/Custom/GFB/FISH");
                        SoundEngine.PlaySound(joke with { Volume = 0.35f, MaxInstances = -1 }, projectile.Center);
                    }
                }
            }
            Time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Time < 2)
                return false;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("CalamityMod/Particles/ThinSparkle").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SeaDragonRocket>()] > 0)
            {
                if (Time == 3)
                {
                    SoundStyle hitSound = new("CalamityMod/Sounds/Item/SevensStrikerTriples");
                    SoundEngine.PlaySound(hitSound with { Volume = 0.5f, Pitch = 0.8f }, Projectile.Center);
                }
                if (Time > 2 && Time < 16)
                {
                    for (int i = 0; i <= 2; i++)
                        Main.EntitySpriteDraw(texture2, GunTipPosition - Main.screenPosition + (-Projectile.velocity.RotatedBy(0.1 * Projectile.direction) * 30f), null, Color.Lerp(Color.Goldenrod, Color.OrangeRed, 0.5f) with { A = 1 }, Time * 0.25f, texture2.Size() * 0.5f, 1.1f, flipSprite);
                }
            }
            return false;
        }
    }
}
