using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class StarfleetHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<Starfleet>();
        public override Vector2 GunTipPosition => base.GunTipPosition;
        public override float RecoilResolveSpeed => 0.1f;
        public override float MaxOffsetLengthFromArm => 25f;
        public override float OffsetXUpwards => -12f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 10f;
        public override float WeaponTurnSpeed => (0.6f);

        public int lastUseTime = 0;
        public int perfectLeniancy = 2;
        public int time = 0;
        public int goodLeniancy => perfectLeniancy + 9;
        public ref float shootingCooldown => ref Projectile.ai[0];
        public ref float starburstTimer => ref Projectile.ai[1];
        public int extendedCooldown => (int)(lastUseTime * 1.2f);
        public int naildriverCooldown => (int)(lastUseTime * 1.5f);
        public int starburstPerfectTime => 23;
        public float recoilIntensity = 0;
        public int recoilTimerMax = 0;
        public Vector2 recoilDirection;
        public bool setVel = true;
        public ref float starburstCooldown => ref Projectile.ai[2];
        public bool naildriver => ((starburstTimer < starburstPerfectTime + perfectLeniancy) && (starburstTimer > starburstPerfectTime - perfectLeniancy)); // if within perfect frame window
        public bool scattershot => !naildriver && ((starburstTimer < starburstPerfectTime + goodLeniancy) && (starburstTimer > starburstPerfectTime - goodLeniancy)); // If within early or late frame window
        public override void KillHoldoutLogic() { }
        public override void HoldoutAI()
        {
            bool doingNothing = shootingCooldown == 0 && starburstCooldown == 0 && starburstTimer == 0;
            if (lastUseTime == 0 || doingNothing)
                lastUseTime = Owner.HeldItem.useAnimation;
            if (!doingNothing)
                Owner.itemTime = Owner.itemAnimation = 2;

            if (Owner.HeldItem.type != ItemType<Starfleet>() && doingNothing)
            {
                Projectile.Kill();
                return;
            }
            bool leftShootChecks = Main.mouseLeft && !Main.mapFullscreen && !Owner.mouseInterface && shootingCooldown == 0;
            bool rightShootChecks = Owner.Calamity().mouseRight && !Main.mapFullscreen && !Owner.mouseInterface && starburstCooldown == 0 && starburstTimer == 0;
            if (leftShootChecks)
                FireShotgun();
            if (rightShootChecks)
            {
                SoundStyle test = new("CalamityMod/Sounds/Item/StarfleetStarburst");
                SoundEngine.PlaySound(test with { Volume = 1f, Pitch = 0f }, Projectile.Center);
                starburstTimer++;
            }
            if (starburstTimer > 0)
            {
                // Do wind up animation
                if (starburstTimer == starburstPerfectTime)
                    FireStarburst();
                
                starburstTimer++;
                if (starburstTimer > starburstPerfectTime + goodLeniancy + 1)
                    starburstTimer = 0;
            }
            if (shootingCooldown > 0)
                shootingCooldown--;
            if (starburstCooldown > 0)
                starburstCooldown--;
            if (recoilIntensity > 0 && (shootingCooldown > 0 || starburstCooldown > 0))
                ManageRecoil();
            time++;
        }
        public void ManageRecoil()
        {
            float slowdown = (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax / 2, recoilTimerMax, Math.Max(shootingCooldown, starburstCooldown), true), 4);
            Vector2 movement = recoilDirection * (recoilIntensity) * slowdown;
            if (Collision.SolidCollision(Owner.Center + movement, (int)(Owner.width * 1.1f), (int)(Owner.height * 1.1f)))
            {
                recoilIntensity = 0;
                return;
            }
            if (slowdown > 0.1f)
            {
                if (setVel)
                {
                    Owner.velocity = movement * 0.25f;
                    setVel = false;
                }
                Projectile.Center += movement;
                Owner.Center += movement;
            }
            else
                setVel = true;
        }
        public void FireShotgun()
        {
            // 50% chance to not consume ammo
            Owner.PickAmmo(HeldItem, out _, out _, out _, out _, out _, Main.rand.NextBool());

            SoundStyle shotgunFire = new("CalamityMod/Sounds/Item/StarmadaFire");
            SoundEngine.PlaySound(shotgunFire with { Volume = 1f, Pitch = 0f }, Projectile.Center);
            // Perfects have longer cooldown
            int cooldown = (naildriver ? naildriverCooldown : lastUseTime);
            recoilTimerMax = cooldown;
            shootingCooldown = cooldown;
            recoilDirection = -Projectile.velocity;
            Owner.Calamity().GeneralScreenShakePower = (naildriver ? 9 : scattershot ? 7 : 4);
            OffsetLengthFromArm = (naildriver ? 0 : scattershot ? 7 : 15);

            for (int i = 0; i < 12; i++)
            {
                float randomVel = Main.rand.NextFloat(0.7f, 1.1f);
                float damageMult = (naildriver || scattershot) ? 1.5f : 1f;
                Projectile shotgun = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, randomVel * Projectile.velocity.RotatedByRandom(naildriver ? 0.1f : scattershot ? 0.9f : 0.35f) * ((naildriver || scattershot) ? 8 : 6), ModContent.ProjectileType<PlasmaBlast>(), (int)(Projectile.damage * damageMult), Projectile.knockBack, Projectile.owner);
                shotgun.extraUpdates = naildriver ? 6  : scattershot ? 4 : 2;
            }
            if (naildriver)
            Main.NewText("naildriver");
            if (scattershot)
            Main.NewText("scattershot");
            recoilIntensity = (naildriver ? 75f : scattershot ? 50f : 10f);
        }
        public void FireStarburst()
        {
            if (Owner.Calamity().GeneralScreenShakePower < 7)
                Owner.Calamity().GeneralScreenShakePower = 7;
            if (OffsetLengthFromArm > 10)
                OffsetLengthFromArm = 10;
            recoilDirection = -Projectile.velocity;
            if (recoilIntensity < 25)
                recoilIntensity = 25;
            if (recoilTimerMax < extendedCooldown)
                recoilTimerMax = extendedCooldown;
            if (starburstCooldown < extendedCooldown)
                starburstCooldown = extendedCooldown;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Vector2.Zero, ModContent.ProjectileType<PartisanExplosion>(), 0, Projectile.knockBack, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
                return false;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D glowTexture = Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/StarfleetGlow").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            float glowMult = (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax / 2, recoilTimerMax, Math.Max(shootingCooldown, starburstCooldown), true), 4);
            int draws = 18;
            float sine = (float)Math.Sin(time * 0.02f);
            float fastSine = (float)Math.Sin(time * 0.2f);
            Color glowColor = Color.Lerp(Color.White, Color.Cyan, 1 + sine);
            for (int i = 0; i < draws; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / draws).ToRotationVector2().RotatedBy(time / 5) * (1.25f + (fastSine + 2f) * 0.2f + glowMult * 4);
                Main.EntitySpriteDraw(glowTexture, drawPosition + drawOffset, null, glowColor with { A = 0 } * (0.1f + 0.5f * glowMult), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
                Main.EntitySpriteDraw(glowTexture, drawPosition, null, Color.White with { A = 0 }, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            }

            return false;
        }
    }
}
