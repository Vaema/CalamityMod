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

namespace CalamityMod.Projectiles.Ranged
{
    public class MegalodonHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Megalodon>();
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
            if (Owner.CantUseHoldout() || HeldItem.type != Owner.ActiveItem().type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
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
                if (framesBetweenShots == 0 && shotCounter <= 30)
                {
                    Main.NewText(Owner.Calamity().sharkGunDamageScaling);
                    SoundStyle fire = new("CalamityMod/Sounds/Item/GunShotTiny");
                    SoundEngine.PlaySound(fire with { Volume = 0.3f , Pitch = 0.4f, PitchVariance = 0.1f, MaxInstances = -1}, Projectile.Center);
                    Owner.PickAmmo(Owner.ActiveItem(), out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _, !Main.rand.NextBool(4));
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 19;
                    Particle sparker = new CritSpark(GunTipPosition, Vector2.Zero, Color.Gold, Color.LightGoldenrodYellow, 1.7f, 3, 0.5f, 3f);
                    GeneralParticleHandler.SpawnParticle(sparker);
                    if (!swapType)
                    {
                        Projectile scalingShot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
                        CalamityGlobalProjectile cgp = scalingShot.Calamity();
                        cgp.sharkBullets = true;
                    }
                    if (swapType)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), ModContent.ProjectileType<MegalodonShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                    for (int i = 0; i <= 4; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(3) ? 303 : 244, (shootVelocity * Main.rand.NextFloat(0.2f, 1.1f)).RotatedByRandom(0.4f));
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    }
                    swapType = !swapType;
                    framesBetweenShots = 4;
                    OffsetLengthFromArm -= 3f;
                    shotCounter++;
                }
                if (framesBetweenShots > 0)
                    framesBetweenShots--;
            }
            Time++;
            if (shotCounter == 30)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/GunShotHeavy");
                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 30;
                for (int i = 0; i <= 13; i++)
                {
                    Dust dust = Dust.NewDustPerfect(GunTipPosition, 303, shootVelocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f, 1.1f), 80, default, Main.rand.NextFloat(0.4f, 1.3f));
                    dust.noGravity = false;
                    dust.color = Color.White;

                    Dust dust2 = Dust.NewDustPerfect(GunTipPosition, 278, shootVelocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.8f));
                    dust2.noGravity = true;
                    dust2.scale = Main.rand.NextFloat(0.52f, 0.72f);
                    dust2.color = Color.Lerp(Color.Orange, Color.Gold, Main.rand.NextFloat(0f, 1f));
                }
                for (int i = 0; i < 14; i++)
                {
                    Particle smoke = new HeavySmokeParticle(GunTipPosition, shootVelocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.2f, 1.5f), Color.RoyalBlue, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.3f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                float rotation = Projectile.velocity.ToRotation();
                Particle pulse = new DirectionalPulseRing(GunTipPosition, shootVelocity / 4, Color.Gray, new Vector2(1f, 2.5f), rotation, 0.03f, 0.3f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
                SoundEngine.PlaySound(fire with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
                Owner.Calamity().sharkGunDamageScaling++;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<MegalodonRocket>(), (int)(Projectile.damage * 0.5f) * (Owner.Calamity().sharkGunDamageScaling + 1), Projectile.knockBack, Projectile.owner);
                Main.NewText(Owner.Calamity().sharkGunDamageScaling);
                Projectile.Kill();
            }
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
