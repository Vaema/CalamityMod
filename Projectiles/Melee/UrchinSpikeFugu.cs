using CalamityMod.Graphics.Primitives;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class UrchinSpikeFugu : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.noEnchantments = true;
        }

        public override void AI()
        {
            Time++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha = (int)Utils.Remap(Time, 0f, 12f, 255f, 0f);

            NPC potentialTarget = Projectile.Center.ClosestNPCAt(256f);
            if (potentialTarget != null && Time >= 12f)
            {
                Vector2 idealVelocity = Projectile.SafeDirectionTo(potentialTarget.Center) * 12f;
                Projectile.velocity = (Projectile.velocity * 20f + idealVelocity) / 21f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 2f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Poisoned, 120);

        public override bool? CanDamage() => Time < 12f ? false : base.CanDamage();

        internal float WidthFunction(float completionRatio) => (1f - completionRatio) * Projectile.scale * 4f;
        internal Color ColorFunction(float completionRatio) => new Color(91, 62, 153) * Projectile.Opacity;
        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 8);
            return true;
        }
    }
}
