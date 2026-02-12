using System;
using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class TerratomereSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int MaxTrailPoints = 20;
        private const int Lifetime = 35;
        private const float StripWidth = 30f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = MaxTrailPoints;
        }

        public override void SetDefaults()
        {
            Projectile.width = 512;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.Opacity = 1f;
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = 2;
            Projectile.scale = 0.75f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 12;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Projectile.timeLeft / (float)Lifetime;
            if (Projectile.timeLeft == Lifetime - 1)
            {
                Particle spark2 = new GlowSparkParticle(Projectile.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 12, Main.rand.NextFloat(0.05f, 0.09f), Main.rand.NextBool() ? Terratomere.TerraColor1 : Terratomere.TerraColor2, new Vector2(2, 0.5f), true);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox);

        public override bool ShouldUpdatePosition() => true;

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Terratomere.TerraColor1, Terratomere.TerraColor2, Projectile.identity / 7f % 1f) * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            var path = new List<Vector3>();
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i];
                if (pos == Vector2.Zero)
                    break;

                Vector2 screenPos = pos + Projectile.Size * 0.5f - Main.screenPosition;
                path.Add(new Vector3(screenPos, 0f));
            }

            if (path.Count < 2)
                return false;

            Color trailColor = Color.Lerp(Terratomere.TerraColor1, Terratomere.TerraColor2, Projectile.identity / 7f % 1f) * Projectile.Opacity;

            float width = StripWidth * Projectile.scale;
            PrimitiveMesh mesh = TriangleStripBuilder.BuildStrip(
                path,
                progress => width * (1f - progress * 0.6f),
                trailColor,
                textured: true,
                smoothingSegments: 2);

            Effect shader = CalamityShaders.NanoblackSlashShader.Value;
            shader.Parameters["uTime"]?.SetValue((float)Main.gameTimeCache.TotalGameTime.TotalSeconds);
            Color tc = Terratomere.TerraColor1;
            shader.Parameters["uColor"]?.SetValue(new Vector3(tc.R / 255f, tc.G / 255f, tc.B / 255f));
            shader.Parameters["uBrightness"]?.SetValue(1f);

            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);

            Main.spriteBatch.End(out var ss);

            SanePrimitiveRenderer.DrawMesh(
                Matrix.Identity,
                view,
                projection,
                mesh,
                shader,
                blendState: BlendState.Additive);

            Main.spriteBatch.Begin(ss);

            return false;
        }
    }
}
