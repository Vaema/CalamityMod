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
using CalamityMod.Projectiles.Turret;
using Mono.Cecil;
using CalamityMod.Projectiles.Rogue;

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

        public override void KillHoldoutLogic()
        {
            //If the player is dead, kill the holdout.
            if (Owner.CantUseHoldout() || HeldItem.type != Owner.ActiveItem().type)
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
                    string goreType = Main.rand.NextBool() ? "EmptyAnimosityShell" : "EmptyAnimosityShell2";
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(2f * -Owner.direction) * Main.rand.NextFloat(0.6f, 0.7f), Mod.Find<ModGore>(goreType).Type);
                }
                Owner.Calamity().sharkGunDamageScaling = 0;
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
                    SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = 0.3f , PitchVariance = 0.1f, MaxInstances = -1 }, Projectile.Center);
                    Particle sparker = new CustomPulse(GunTipPosition, Vector2.Zero, Color.HotPink, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.02f, 0.5f, 10, true, 0.8f);
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
                    framesBetweenShots = 4;
                    OffsetLengthFromArm -= 3f;
                    //Here we detect which ammo the bullets will use
                    Owner.PickAmmo(Owner.ActiveItem(), out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, !Main.rand.NextBool(4));
                    //Alternate between shooting bullets and water streams. We seperate it into two different projectiles due to the bullets needing to use a Global Projectile to track the damage multiplier
                    if (!swapType)
                    {
                        Projectile scalingShot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
                        CalamityGlobalProjectile cgp = scalingShot.Calamity();
                        cgp.sharkBullets = true;
                    }
                    if (swapType)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), ModContent.ProjectileType<BlahajBlast>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    swapType = !swapType;
                    shotCounter++;
                    if (shotCounter == 50)
                        framesBetweenShots = 18;
                }
                if (framesBetweenShots > 0)
                    framesBetweenShots--;
            }
            if (shotCounter == 50 && framesBetweenShots == 0)
            {
                if (framesBetweenShots == 0)
                {
                    SoundStyle hitSound = new("CalamityMod/Sounds/Custom/AuricMine", 3);
                    SoundEngine.PlaySound(hitSound with { Pitch = 1.1f , Volume = 2f }, Projectile.Center);
                    Particle Star = new CritSpark(GunTipPosition + (-Projectile.velocity.RotatedBy(0.1 * Projectile.direction) * 28), Vector2.Zero, Color.Goldenrod, Color.OrangeRed, 2f, 20, 0.2f, 3f);
                    GeneralParticleHandler.SpawnParticle(Star);
                    Projectile.Kill();
                    for (int x = 0; x < Main.maxProjectiles; x++)
                    {
                        Projectile projectile = Main.projectile[x];
                        if (projectile.active && projectile.type == ModContent.ProjectileType<SeaDragonRocket>() && projectile.ai[1] < 5)
                        {
                            projectile.ai[1] = 5;
                            projectile.velocity = Utils.DirectionTo(projectile.Center, Owner.Calamity().mouseWorld) * 12;
                        }
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
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
}
