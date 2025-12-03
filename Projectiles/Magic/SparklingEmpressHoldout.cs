using CalamityMod.Dusts;
using CalamityMod.Items.Fishing.SunkenSeaCatches;
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
        public bool deservesMana = false;

        public float GfbMult = 1;
        public int CooldownTime => 20 * (deservesMana ? 2 : 1);
        private const int FishShineTime = 150;

        public override void KillHoldoutLogic()
        {
            if (HeldItem.type != Owner.HeldItem.type || Owner.dead)
                Projectile.Kill();
            if (Owner.CantUseHoldout() && !hasLetGo)
                postFireTimer = CooldownTime;
        }

        public override void HoldoutAI()
        {
            if (Main.zenithWorld)
                GfbMult = MathHelper.Clamp((int)(time / 60), 1, 7);
            if (!hasLetGo)
            {
                int shootInterval = deservesMana && !Main.zenithWorld ? Owner.HeldItem.useTime * 2 : Owner.HeldItem.useTime;
                if (shootingTimer >= shootInterval)
                {
                    if (Owner.CheckMana(HeldItem, (int)(HeldItem.mana * GfbMult), true, false))
                    {
                        Shoot();
                        shootingTimer = 0f;
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                        postFireTimer = CooldownTime;
                    }
                }

                shootingTimer++;

                if (time >= FishShineTime)
                    deservesMana = true;
                if (time == FishShineTime)
                {
                    SoundStyle f = new("CalamityMod/Sounds/Item/MeldShoot");
                    SoundEngine.PlaySound(f with { Volume = 0.5f, Pitch = 0.95f }, Projectile.Center);
                    for (int k = 0; k < 14; k++)
                    {
                        Vector2 vel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5.1f, 8.8f);

                        Dust dust2 = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<LightDust>(), vel);
                        dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                        dust2.noGravity = true;
                        dust2.color = Main.rand.NextBool() ? Color.Lerp(EffectsColor, Color.White, 0.5f) : EffectsColor;
                    }
                }
            }
            time++;

            if (Main.zenithWorld)
            {
                if (!hasLetGo)
                {
                    if (time % 60 == 0 && time > 120 && time <= 420)
                    {
                        SoundStyle f = new("CalamityMod/Sounds/Item/MeldShoot");
                        SoundEngine.PlaySound(f with { Volume = 0.5f, Pitch = 0.95f }, Projectile.Center);
                    }
                    if (time == 420)
                    {
                        SoundStyle warnSound = new("CalamityMod/Sounds/Custom/PlagueSounds/PBGNukeWarning");
                        SoundEngine.PlaySound(warnSound with { Volume = 0.7f, Pitch = 0.8f }, Projectile.Center);
                    }
                    // gfb texts
                    if (time == 1)
                        CombatText.NewText(Projectile.Hitbox, Color.SkyBlue, CalamityUtils.GetTextValue("Misc.Empress1"));
                    if (time == 300)
                        CombatText.NewText(Projectile.Hitbox, Color.DeepSkyBlue, CalamityUtils.GetTextValue("Misc.Empress2"));
                    if (time == 510)
                        CombatText.NewText(Projectile.Hitbox, Color.DodgerBlue, CalamityUtils.GetTextValue("Misc.Empress3"));
                    Owner.velocity += -Projectile.velocity * MathHelper.Lerp(GfbMult, 1, 0.6f) * 0.1f;
                }
                
                if (time >= 540)
                {
                    if (time < 600)
                    {
                        if (time % 10 == 0)
                        {
                            Particle boom = new CustomPulse(Projectile.Center, -Projectile.velocity * 0.3f, Color.DodgerBlue, "CalamityMod/Particles/BloomRing", new Vector2(1, 1f), 0, 0, 4, 10);
                            GeneralParticleHandler.SpawnParticle(boom);
                            SoundStyle f = new("CalamityMod/Sounds/Item/MeldExplosion");
                            SoundEngine.PlaySound(f with { Volume = 0.5f, Pitch = 0.95f }, Projectile.Center);
                            for (int k = 0; k < 18; k++)
                            {
                                Projectile explode = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.One * 5).RotatedByRandom(100), ModContent.ProjectileType<SparklingLaser>(), Projectile.damage * 5, Projectile.knockBack, Projectile.owner, 0, 0);
                                explode.hostile = true;
                                explode.extraUpdates = 3;
                                explode.tileCollide = false;
                            }
                        }
                        if (time % 3 == 0)
                            CombatText.NewText(Projectile.Hitbox, Color.Cyan, CalamityUtils.GetTextValue("Misc.Empress4"));
                    }
                    if (postFireTimer == 0)
                        postFireTimer = (CooldownTime * 5);
                    deservesMana = false;
                }
            }

            if (postFireTimer > 0)
                PostFireEffect();
        }
        public void PostFireEffect()
        {
            hasLetGo = true;

            if (deservesMana && postFireTimer % 8 == 0)
            {
                int manaGained = 10;
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
            if (postFireTimer == 1)
                Projectile.Kill();
            postFireTimer--;
        }
        public void Shoot()
        {
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 firingVelocity = (shootDirection * 4);
                for (int k = 0; k < (int)(GfbMult); k++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition + firingVelocity * 5, firingVelocity.RotatedByRandom(Main.zenithWorld ? GfbMult * 0.08f : 0), ModContent.ProjectileType<SparklingLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 0);
            }

            for (int k = 0; k < 4; k++)
            {
                Vector2 shootVel = (shootDirection * 15).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.8f);

                Dust dust2 = Dust.NewDustPerfect(GunTipPosition, ModContent.DustType<LightDust>(), shootVel);
                dust2.scale = Main.rand.NextFloat(0.75f, 0.9f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Lerp(EffectsColor, Color.White, 0.5f) : EffectsColor;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
                return false;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;

            Vector2 vibration = (hasLetGo ? Vector2.Zero : (Main.rand.NextVector2Circular(2, 2) * (time >= 840 && Main.zenithWorld ? 4 : 1)));

            Main.EntitySpriteDraw(texture, drawPosition + vibration, null, Projectile.GetAlpha(lightColor), drawRotation + (MathHelper.ToRadians(45f * (Projectile.spriteDirection))), rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            if (deservesMana && !hasLetGo)
            {
                for (int i = 0; i < 2; i++)
                    Main.EntitySpriteDraw(shineTex, GunTipPosition - Main.screenPosition, null, Color.DodgerBlue with { A = 0 }, Projectile.rotation + MathHelper.ToRadians(45 * (i == 0 ? -1 : 1)), shineTex.Size() * 0.5f, new Vector2(0.9f, 2.5f) * Main.rand.NextFloat(0.7f, 1.1f) * MathHelper.Lerp(GfbMult, 1, 0.6f), SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
