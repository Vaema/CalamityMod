using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ElementalSawBullet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.scale = 0.85f;
        }

        public override void AI()
        {
            Time++;

            // Solid homing
            NPC potentialTarget = Projectile.Center.ClosestNPCAt(480f);
            if (potentialTarget != null && Time >= 15f)
            {
                Vector2 idealVelocity = Projectile.SafeDirectionTo(potentialTarget.Center) * 24f;
                Projectile.velocity = (Projectile.velocity * 29f + idealVelocity) / 30f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 3f);
            }
            else if (Time >= 30f)
            {
                // Projectile decays a lot faster if there's no enemy in sight
                Projectile.timeLeft -= 2;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Emit light
            DelegateMethods.v3_1 = Color.Lerp(Color.Lime, Color.White, 0.55f).ToVector3() * 0.35f;
            Utils.PlotTileLine(Projectile.Center - Projectile.velocity * 0.5f, Projectile.Center + Projectile.velocity * 0.5f, 16f, DelegateMethods.CastLightOpen);
        }

        public override bool? CanDamage() => Time >= 15f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<ElementalMix>(), 45);

        internal float WidthFunction(float completionRatio) => (1f - completionRatio) * Projectile.scale * 8f;
        internal Color ColorFunction(float completionRatio)
        {
            float hue = 0.4f + 0.2f * completionRatio * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
            Color trailColor = Main.hslToRgb(hue, 1f, 0.8f);
            return trailColor * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f), 16);
            Texture2D glow = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
