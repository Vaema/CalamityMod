using System;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class StarfleetHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<Starfleet>();
        public override Vector2 GunTipPosition => base.GunTipPosition + Projectile.velocity.RotatedBy(MathHelper.PiOver2 * Projectile.direction) * -5;
        public override float RecoilResolveSpeed => 0.1f;
        public override float MaxOffsetLengthFromArm => 25f;
        public override float OffsetXUpwards => -12f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYDownwards => 10f;
        public override float WeaponTurnSpeed => (0.6f);

        public int lastUseTime = 0;
        public int perfectLeniancy = 2;
        public int time = 0;
        public int goodLeniancy => perfectLeniancy + 6;
        public ref float shootingCooldown => ref Projectile.ai[0];
        public ref float starburstTimer => ref Projectile.ai[1];
        public int extendedCooldown => (int)(lastUseTime * 1.2f);
        public int naildriverCooldown => (int)(lastUseTime * 1.5f);
        public int starburstPerfectTime => 23;
        public float recoilIntensity = 0;
        public int recoilTimerMax = 62;
        public Vector2 recoilDirection;
        public bool setVel = true;
        public float glowIntensity = 1;
        public Color c1 = new Color (192, 10, 111);
        public Color c2 = Color.Coral;
        public Color c3 = Color.DarkOrange;
        public ref float starburstCooldown => ref Projectile.ai[2];
        public bool naildriver => ((starburstTimer < starburstPerfectTime + perfectLeniancy) && (starburstTimer > starburstPerfectTime - perfectLeniancy)); // if within perfect frame window
        public bool scattershot => !naildriver && ((starburstTimer < starburstPerfectTime + goodLeniancy) && (starburstTimer > starburstPerfectTime - goodLeniancy)); // If within early or late frame window
        public override void KillHoldoutLogic() { }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
        }
        public override void HoldoutAI()
        {
            bool doingNothing = shootingCooldown == 0 && starburstCooldown == 0 && starburstTimer == 0;
            if (lastUseTime == 0 || doingNothing)
                lastUseTime = Owner.HeldItem.useAnimation;
            if (!doingNothing)
                Owner.itemTime = Owner.itemAnimation = 2;

            glowIntensity = MathHelper.Lerp(glowIntensity, (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax, 0, shootingCooldown, true), 5), 0.2f);
            
            if ((Owner.HeldItem.type != ItemType<Starfleet>() && doingNothing) || Owner.dead)
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
                if (starburstTimer < starburstPerfectTime / 2)
                    OffsetLengthFromArm = 25 - 12 * (1 - (float)Math.Pow(Utils.GetLerpValue(starburstPerfectTime / 2 - 1, 0, starburstTimer, true), 5));
                else
                    OffsetLengthFromArm = 18 + 20 * ((float)Math.Pow(Utils.GetLerpValue(starburstPerfectTime / 2, starburstPerfectTime - 1, starburstTimer, true), 8));

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
            for (int i = 0; i < (naildriver ? 2 : 1); i++)
                SoundEngine.PlaySound(shotgunFire with { Volume = 0.7f, Pitch = ((naildriver && i == 0) ? -0.2f : 0f), MaxInstances = 2 }, Projectile.Center);
            // Perfects have longer cooldown
            int cooldown = (naildriver ? naildriverCooldown : lastUseTime);
            recoilTimerMax = cooldown;
            shootingCooldown = cooldown;
            recoilDirection = -Projectile.velocity;
            Owner.Calamity().GeneralScreenShakePower = (naildriver ? 9 : scattershot ? 7 : 4);
            OffsetLengthFromArm = (naildriver ? 0 : scattershot ? 7 : 15);

            for (int i = 0; i < 6; i++)
            {
                float randomVel = Main.rand.NextFloat(0.7f, 1.1f);
                float damageMult = (naildriver || scattershot) ? 2.3f : 1f;
                float spread = (naildriver ? 0.06f : scattershot ? 0.9f : 0.25f);
                Projectile shotgun = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, randomVel * Projectile.velocity.RotatedByRandom(spread) * 8, ModContent.ProjectileType<PlasmaBlast>(), (int)(Projectile.damage * damageMult), Projectile.knockBack, Projectile.owner);
                shotgun.extraUpdates = naildriver ? 9 : scattershot ? 7 : 3;
            }
            for (int i = 0; i < 25; i++)
            {
                float variance = Main.rand.NextFloat(-0.7f, 0.7f);
                int dustStyle = DustType<SquashDust>();
                Dust dust = Dust.NewDustPerfect(GunTipPosition, dustStyle);
                dust.scale = (Main.rand.NextFloat(1.4f, 1.8f) - Math.Abs(variance)) * 3f;
                dust.velocity = Projectile.velocity.RotatedBy(variance) * Main.rand.NextFloat(18f, 19f) * (float)Math.Pow(1 - Math.Abs(variance), 2);
                dust.noGravity = true;
                dust.color = Color.Lerp(c1, c3, Main.rand.NextFloat(0f, 1f));
                dust.fadeIn = 4.75f;
            }


            if (naildriver)
            Main.NewText("naildriver: " + (starburstPerfectTime - starburstTimer), Color.DarkOrchid);
            if (scattershot)
            Main.NewText("scattershot: " + (starburstPerfectTime - starburstTimer), Color.Lime);
            recoilIntensity = (naildriver ? 55f : scattershot ? 20f : 0);
        }
        public void FireStarburst()
        {
            if (Owner.Calamity().GeneralScreenShakePower < 7)
                Owner.Calamity().GeneralScreenShakePower = 7;
            if (OffsetLengthFromArm > 10)
                OffsetLengthFromArm = 10;
            recoilDirection = -Projectile.velocity;
            if (recoilIntensity < 15)
                recoilIntensity = 15;
            if (recoilTimerMax < extendedCooldown)
                recoilTimerMax = extendedCooldown;
            if (starburstCooldown < extendedCooldown)
                starburstCooldown = extendedCooldown;

            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, Vector2.Zero, ModContent.ProjectileType<PartisanExplosion>(), 0, Projectile.knockBack, Projectile.owner);

            /*for (int i = 0; i < 50; i++)
            {
                Vector2 intenededVel = (MathHelper.TwoPi * i / 50f).ToRotationVector2() * 4f;
                Vector2 fxVel = new Vector2(intenededVel.X, intenededVel.Y * 2.3f).RotatedBy(Projectile.velocity.ToRotation());
                Vector2 fxPlace = GunTipPosition + fxVel.RotatedBy(Projectile.velocity.ToRotation());

                StarsmokeMetaball.SpawnParticle(fxPlace, fxVel + Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(6, 9), 40f * Main.rand.NextFloat(0.7f, 1), 70, new Vector2(1f, 1f), 0.001f * (Main.rand.NextBool() ? -1 : 1), Main.rand.NextFloat(0.3f, 2));
            }*/

            float blastSize = 140;
            float minMultiplier = 0.1f;
            int hitsToMinMult = 6;
            Projectile blast = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 3), -45, Owner.whoAmI, blastSize, minMultiplier, hitsToMinMult);
            blast.timeLeft = 15;

            for (int i = 0; i < 14; i++)
            {
                float dist = Main.rand.NextFloat(0, 3);
                Particle forwardJet = new CustomSpark(GunTipPosition + Main.rand.NextVector2CircularEdge(dist * 5, dist * 5), Projectile.velocity * Main.rand.NextFloat(4, 5) * (6 - dist * 2), "CalamityMod/Particles/ForwardSmear", false, (int)(Main.rand.Next(9, 15 + 1) + (dist * 3)), Main.rand.NextFloat(0.1f, 0.2f), Color.Lerp(c1, c3, Main.rand.NextFloat(0f, 1f)), new Vector2(1f, 1f), shrinkSpeed: 0.3f);
                GeneralParticleHandler.SpawnParticle(forwardJet);
            }
            for (int i = 0; i < 34; i++)
            {
                float rot = Main.rand.NextFloat(0.05f, 0.35f) * (Main.rand.NextBool() ? -1 : 1);
                Vector2 startVel = Projectile.velocity.RotatedBy(rot) * Main.rand.NextFloat(8, 18) * (Main.rand.NextBool(4) ? 2f : 1);
                Particle stars = new VelChangingSpark(GunTipPosition, startVel, startVel.RotatedBy(rot * 5), "CalamityMod/Particles/PulseStar", Main.rand.Next(25, 45 + 1), Main.rand.NextFloat(0.1f, 0.35f), Color.Lerp(c1, c3, Main.rand.NextFloat(0f, 1f)), new Vector2(1f, 1f), shrinkSpeed: Main.rand.NextFloat(0.02f, 0.06f), lerpRate: 0.02f, glowCenter: true);
                GeneralParticleHandler.SpawnParticle(stars);
            }
            int parts = 60;
            for (int i = 0; i < parts; i++)
            {
                Vector2 intenededVel = (MathHelper.TwoPi * i / parts).ToRotationVector2() * 4f;
                Vector2 fxVel = new Vector2(intenededVel.X, intenededVel.Y * 2.3f).RotatedBy(Projectile.velocity.ToRotation());
                Vector2 fxVelEnd = new Vector2(intenededVel.X * 0.5f, intenededVel.Y * 6f).RotatedBy(Projectile.velocity.ToRotation());
                Vector2 fxPlace = GunTipPosition + fxVel.RotatedBy(Projectile.velocity.ToRotation());

                float size = Utils.GetLerpValue(0, -4, intenededVel.X, true);
                float width = Utils.GetLerpValue(0, 4 * Math.Sign(fxVel.X), fxVel.X, true);

                Particle aura = new CustomSpark(fxPlace, fxVel * 1.2f, "CalamityMod/Particles/BloomCircle", false, (int)(15 + size * 5), 0.35f + size * 0.2f, Color.Lerp(c1, c3, Main.rand.NextFloat(0f, 1f)) * 0.7f, new Vector2(1f + width * size, 1f), glowCenter: true, glowOpacity: size * 0.85f, glowCenterScale: 0.75f);
                GeneralParticleHandler.SpawnParticle(aura);

            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
                return false;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D glowTexture = Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/StarfleetGlow").Value;
            Texture2D orb = Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float glowMult = (float)Math.Pow(Utils.GetLerpValue(recoilTimerMax / 2, recoilTimerMax, Math.Max(shootingCooldown, starburstCooldown), true), 4);
            int draws = 18;
            float sine = (float)Math.Sin(time * 0.02f);
            float attackMult = (float)Math.Pow(Utils.GetLerpValue(0, starburstPerfectTime - 1, starburstTimer, true), 2);
            float sine2 = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 55.5f / MathHelper.Pi);
            float fastSine = (float)Math.Sin(time * 0.2f);
            Color glowColor = Color.Lerp(c2, c3, 1 + sine);

            if (starburstTimer > 0 && starburstCooldown == 0)
            {
                for (int i = 0; i < draws; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / draws).ToRotationVector2().RotatedBy(time * 2);
                    Main.EntitySpriteDraw(texture, drawPosition + drawOffset * 6 * attackMult, null, c2 with { A = 0 } * 0.7f * attackMult, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
                }
            }
            
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            for (int i = 0; i < draws; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / draws).ToRotationVector2().RotatedBy(time / 5) * (1.25f + (fastSine + 2f) * 0.2f + glowMult * 4);
                Main.EntitySpriteDraw(glowTexture, drawPosition + drawOffset, null, Color.Lerp(Color.Gray * 0.15f, glowColor with { A = 0 }, glowIntensity) * (0.1f + 0.5f * glowMult), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
                Main.EntitySpriteDraw(glowTexture, drawPosition, null, Color.Lerp(Color.Gray * 0.15f, Color.White with { A = 0 }, glowIntensity), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            }
            
            if (starburstTimer > 0 && starburstCooldown == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    // the electric orb at the tip
                    Color orbColor = Color.Lerp(Color.Lerp(c1, c3, (i + 2) / 6), Color.White, i / 6) with { A = 0 } * 0.5f;
                    Vector2 scale = new Vector2(Math.Abs(sine2 * 0.5f) + 0.1f, 1) * (0.05f + i * 0.01f) * attackMult * Main.rand.NextFloat(0.9f, 1.1f) * 5;
                    Main.EntitySpriteDraw(orb, GunTipPosition - Main.screenPosition, null, orbColor, Main.rand.NextFloat(-5, 5), orb.Size() * 0.5f, scale, SpriteEffects.None);
                }
            }

            return false;
        }
    }
}
