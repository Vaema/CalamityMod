using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace CalamityMod.Projectiles.Ranged
{
    public class HalleysInfernoHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<HalleysInferno>();
        public override float MaxOffsetLengthFromArm => 24f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;
        public override float WeaponTurnSpeed => 20;

        public ref float ShotTimer => ref Projectile.ai[0];
        public ref float AccuracyCounter => ref Owner.Calamity().HalleyAccuracyCounter;

        public ref float ShotCounter => ref Projectile.ai[2];

        public ref float RecoilAmount => ref Projectile.localAI[0];

        public Player Owner => Main.player[Projectile.owner];

        public Item Halley => Owner.ActiveItem();
        public override void KillHoldoutLogic()
        {
            if ((Owner.CantUseHoldout(false) || Halley.type != ModContent.ItemType<HalleysInferno>()))
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            ForcePlayerItemTime = false;
            if (Halley.type != ModContent.ItemType<HalleysInferno>())
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 60;
            if (Owner.HasAmmo(Halley) && (Owner.controlUseItem || ShotCounter > 0))
            {
                if (ShotTimer <= 0)
                {
                    ShotCounter++;
                    ShotTimer = 5;
                    if (ShotCounter >= 5)
                    {
                        ShotCounter = 0;
                        ShotTimer += 35;
                    }
                    var spawnpos = Projectile.Center;
                    Projectile.NewProjectile(Owner.GetSource_ItemUse_WithPotentialAmmo(Halley,AmmoID.Gel),spawnpos,spawnpos.DirectionTo(Main.MouseWorld) * Halley.shootSpeed,ModContent.ProjectileType<HalleysComet>(), (int)Owner.GetDamage(DamageClass.Ranged).ApplyTo(Halley.damage),Halley.knockBack,Projectile.owner);
                    RecoilAmount = 8;
                    SoundEngine.PlaySound(HalleysInferno.ShootSound);
                    //consume gel
                    Owner.PickAmmo(Halley, out _, out _, out _, out _, out _);
                }
            }
            else if (Owner.controlUseTile)
            {
                if (ShotTimer <= 0 && Owner.Calamity().AvaliableStarburst > 0)
                {
                    ShotTimer = 4;
                    var spawnpos = Projectile.Center;
                    var dir = spawnpos.DirectionTo(Main.MouseWorld);
                    Projectile.NewProjectile(Owner.GetSource_ItemUse_WithPotentialAmmo(Halley, AmmoID.Gel), spawnpos + dir * 16,dir * Halley.shootSpeed * 0.75f, ModContent.ProjectileType<HalleysStarburst>(), (int)Owner.GetDamage(DamageClass.Ranged).ApplyTo(Halley.damage), Halley.knockBack, Projectile.owner);
                    Owner.Calamity().StratusStarburst--;
                    SoundEngine.PlaySound(HalleysInferno.ShootSound);
                    RecoilAmount = 4;
                }
            }
            RecoilAmount *= 0.75f;
            if (ShotTimer > 0)
                ShotTimer--;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.myPlayer == Owner.whoAmI)
            {
                float completion = (AccuracyCounter) / (HalleysInferno.MaxAccuracy);
                var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
                var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

                Vector2 drawPos = Owner.Center - Main.screenPosition + new Vector2(0, -36) - barBG.Size() / 2;
                Rectangle frame = new Rectangle(0, 0, (int)(completion * barFG.Width), barFG.Height);

                float opacity = 1f;
                Color color = Color.Lerp(Color.DarkSlateBlue,Color.SkyBlue, completion);
                if (completion >= 1)
                    color = Color.DeepSkyBlue;

                Main.spriteBatch.Draw(barBG, drawPos, color * opacity);
                Main.spriteBatch.Draw(barFG, drawPos, frame, color * opacity * 0.8f);
            }

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 1; i <= 24; i++)
            {
                float mult = MathHelper.Max(Utils.GetLerpValue(7, 0, i), Utils.GetLerpValue(17, 24, i));
                Vector2 drawOffset = (((MathHelper.TwoPi * i / 24f).ToRotationVector2().RotatedBy(Projectile.rotation) * RecoilAmount) + Main.rand.NextVector2Circular(2, 2));
                Color auraColor = Color.Chartreuse with { A = 0 } * mult * (0.4f) * Utils.GetLerpValue(90, 135, RecoilAmount, true);
                float aimAngle = drawRotation;
                Main.EntitySpriteDraw(texture, drawPosition + drawOffset, null, auraColor, aimAngle, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            }

            Main.EntitySpriteDraw(texture, drawPosition + new Vector2(-RecoilAmount,0).RotatedBy(Projectile.rotation), null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);

            return false;
        }
    }
}
