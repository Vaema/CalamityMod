using System;
using System.IO;
using CalamityMod.Items.Fishing.SunkenSeaCatches;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SparklingEmpressHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<SparklingEmpress>();
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitX.RotatedBy(Projectile.rotation) * -3;
        public override float MaxOffsetLengthFromArm => 40f;
        public override float OffsetXUpwards => 0f;
        public override float BaseOffsetY => 0f;
        public override float OffsetYDownwards => 0f;
        public override float RecoilResolveSpeed => 0.5f;
        public override float WeaponTurnSpeed => 0.05f;

        public ref float shootingTimer => ref Projectile.ai[0];
        public ref float completion => ref Projectile.ai[1];
        public Color EffectsColor = Color.Cyan;
        public bool hasLetGo = false;
        public int time = 0;
        public int postFireTimer = 0;
        public bool deservesKiss = false;
        public Vector2 storedPos;
        public int cooldownTime => 20 * (deservesKiss ? 2 : 1);

        public override void KillHoldoutLogic()
        {
            if (HeldItem.type != Owner.ActiveItem().type)
                Projectile.Kill();
            if (Owner.CantUseHoldout() && !hasLetGo)
                postFireTimer = cooldownTime;
        }

        public override void HoldoutAI()
        {
            if (!hasLetGo)
            {
                if (shootingTimer >= (deservesKiss ? 5 : 3))
                {
                    if (Owner.CheckMana(HeldItem, -1, true, false))
                    {
                        Shoot();
                        shootingTimer = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                        postFireTimer = cooldownTime;
                    }
                }

                shootingTimer++;

                if (time >= 120)
                    deservesKiss = true;
                if (time == 120)
                {
                    SoundStyle f = new("CalamityMod/Sounds/Item/MeldShoot");
                    SoundEngine.PlaySound(f with { Volume = 0.5f, Pitch = 0.95f }, Projectile.Center);
                    for (int k = 0; k < 14; k++)
                    {
                        Vector2 shootVel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5.1f, 8.8f);

                        Dust dust2 = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(4) ? 267 : 66, shootVel);
                        dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                        dust2.noGravity = true;
                        dust2.color = Main.rand.NextBool() ? Color.Lerp(EffectsColor, Color.White, 0.5f) : EffectsColor;
                    }
                }
            }
            time++;
            
            if (postFireTimer > 0)
                PostFireEffect();
        }
        public void PostFireEffect()
        {
            if (!hasLetGo)
            {
                storedPos = Projectile.Center;
            }
            hasLetGo = true;

            if (deservesKiss && postFireTimer % 8 == 0)
            {
                int manaGained = 24;
                Owner.statMana += manaGained;
                if (Main.myPlayer == Owner.whoAmI)
                    Owner.ManaEffect(manaGained);

                if (Owner.statMana > Owner.statManaMax2)
                    Owner.statMana = Owner.statManaMax2;

                for (int i = 0; i < 2; i++)
                {
                    Particle spark = new CustomSpark(GunTipPosition, (-Vector2.UnitY * Main.rand.NextFloat(3f, 5f)).RotatedByRandom(0.6f), "CalamityMod/Particles/HeartParticle", false, 45, Main.rand.NextFloat(0.9f, 1.2f), Color.Lerp(Color.Aqua, Color.DodgerBlue, i * 0.5f), Vector2.One, true, true, 0, false, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                OffsetLengthFromArm -= 9f;

                SoundStyle s = new("CalamityMod/Sounds/Custom/PistolShrimpBubbleBurst");
                SoundEngine.PlaySound(s with { Volume = 0.35f, Pitch = -postFireTimer * 0.01f, MaxInstances = -1 }, Projectile.Center);
            }
            if (deservesKiss && (postFireTimer + 2) % 8 == 0)
            {
                Owner.HealPlayer(4);
            }
            if (postFireTimer == 1)
                Projectile.Kill();
            postFireTimer--;
        }
        public void Shoot()
        {
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 firingVelocity = (shootDirection * 4);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition + firingVelocity * 5, firingVelocity, ModContent.ProjectileType<SparklingLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 0);

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            for (int k = 0; k < 4; k++)
            {
                Vector2 shootVel = (shootDirection * 15).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.8f);

                Dust dust2 = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(4) ? 267 : 66, shootVel);
                dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Lerp(EffectsColor, Color.White, 0.5f) : EffectsColor;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
                return false;
            Vector2 toHead = Utils.DirectionTo(storedPos, Owner.Center - Vector2.UnitY * 4);

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;

            Main.EntitySpriteDraw(texture, drawPosition + (hasLetGo ? Vector2.Zero : Main.rand.NextVector2Circular(2, 2)), null, Projectile.GetAlpha(lightColor), drawRotation + (MathHelper.ToRadians(45f * (Projectile.spriteDirection))), rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            if (deservesKiss && !hasLetGo)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(shineTex, GunTipPosition - Main.screenPosition, null, Color.DodgerBlue with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(45 * (i == 0 ? -1 : 1)), shineTex.Size() * 0.5f, new Vector2(0.9f, 2.5f) * Main.rand.NextFloat(0.7f, 1.1f), SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
