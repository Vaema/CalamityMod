using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class DivineRetributionSpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public Player Owner => Main.player[Projectile.owner];
        public ref float VelocityScale => ref Projectile.ai[0];
        public ref float Time => ref Projectile.ai[1];

        public static float HomingSpeed => 20f;
        public static float HomingTime => 15f;
        public static float ReturnTime => 75f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 210;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            Time++;
            Projectile.Opacity = Utils.GetLerpValue(0f, 18f, Projectile.timeLeft, true);
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 0.7f * Projectile.Opacity);

            if (Time % 2f == 1f)
            {
                Vector2 dustOffset = (Vector2.UnitY * MathF.Sin(Projectile.timeLeft * MathHelper.Pi * 0.05f) * 16f).RotatedBy(Projectile.velocity.ToRotation());
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, ModContent.DustType<LightDust>());
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.color = Color.Gold;
                dust.scale = Main.rand.NextFloat(0.4f, 0.6f);
            }

            NPC potentialTarget = Projectile.Center.ClosestNPCAt(Time >= ReturnTime ? 480f : 320f);
            if (potentialTarget != null && Time >= HomingTime)
            {
                Vector2 idealVelocity = Projectile.SafeDirectionTo(potentialTarget.Center) * HomingSpeed * VelocityScale;
                Projectile.velocity = (Projectile.velocity * 29f + idealVelocity) / 30f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 3f);
                return;
            }
            else if (Owner.active && !Owner.dead && Time >= ReturnTime)
            {
                Vector2 idealVelocity = Projectile.SafeDirectionTo(Owner.MountedCenter) * HomingSpeed * VelocityScale * (1.2f - Utils.GetLerpValue(800f, 160f, Vector2.Distance(Projectile.Center, Owner.MountedCenter), true));
                Projectile.velocity = (Projectile.velocity * 29f + idealVelocity) / 30f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 3f);
            }
            if (!Owner.active || Owner.dead || (Vector2.Distance(Projectile.Center, Owner.MountedCenter) <= 240f && Time >= ReturnTime))
            {
                Projectile.timeLeft--;
                if (!Owner.active || Owner.dead || Vector2.Distance(Projectile.Center, Owner.MountedCenter) <= 80f)
                {
                    if (Projectile.timeLeft > 30)
                        Projectile.timeLeft = 30;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D spear = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color baseColor = Color.Gold * Projectile.Opacity;
            Color glowColor = new Color(255, 255, 150) * Projectile.Opacity;

            float squish = MathHelper.Clamp(Projectile.velocity.Length() * 0.04f, 0, 0.2f);
            Vector2 scale = new Vector2(1f + squish, 1f - squish);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    float completionRatio = i / (float)Projectile.oldPos.Length;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;

                    Color trailColor = Color.Lerp(glowColor, Color.Black, completionRatio) * 0.35f;
                    Vector2 trailScale = scale * MathHelper.Lerp(1f, 0.15f, completionRatio);

                    Main.EntitySpriteDraw(spear, trailPos, null, trailColor, Projectile.oldRot[i], spear.Size() * 0.5f, trailScale, SpriteEffects.None);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 4f;
                Main.EntitySpriteDraw(spear, drawPos + offset, null, Color.Lerp(baseColor, glowColor, 0.8f) * 0.5f, Projectile.rotation, spear.Size() * 0.5f, scale, SpriteEffects.None);
            }
            Main.spriteBatch.ExitShaderRegion();

            Main.EntitySpriteDraw(spear, drawPos, null, baseColor, Projectile.rotation, spear.Size() * 0.5f, scale, SpriteEffects.None);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);

        public override void OnKill(int timeLeft)
        {
            Projectile.ExpandHitboxBy(96);
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            for (int i = 0; i < 9; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Main.rand.NextVector2CircularEdge(5f, 5f));
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.color = Color.Gold;
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }
        }
    }
}
