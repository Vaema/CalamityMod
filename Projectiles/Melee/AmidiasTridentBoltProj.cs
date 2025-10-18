using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.ExtraTextures;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    class AmidiasTridentBoltProj : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/ExtraTextures/BasicCircle";
        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.width = Projectile.height = 50;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.extraUpdates = 1;
        }
        bool start = false;
        public override void AI()
        {
            if (!start)
            {
                Projectile.ai[0] = MathHelper.ToRadians(Main.rand.NextFloat(-1f, 1f));
                Projectile.velocity = Projectile.velocity.RotatedBy(-Projectile.ai[0] * 10f);
                start = true;
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] / 2f);
            }

            GeneralParticleHandler.SpawnParticle(
                new SparkParticle(Projectile.Center, Projectile.velocity / 2f, false, 10, 2f * Projectile.velocity.Length() / 20f, Color.CornflowerBlue, true)
                );

            Projectile.velocity *= 0.9f;
            if (Projectile.velocity.Length() < 1f) Projectile.Kill();
        }
        public static readonly Asset<Texture2D> MainProjectileTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeSpark");
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> MainTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmear");

            float vel = Projectile.velocity.Length() / 30f;

            Main.EntitySpriteDraw(MainTexture.Value, Projectile.Center - Main.screenPosition, MainTexture.Frame(),
                Color.CornflowerBlue.MultiplyRGBA(new(1f, 1f, 1f, 0f)), Projectile.velocity.ToRotation() + MathHelper.PiOver2, MainTexture.Size() * new Vector2(0.5f, 0f), new Vector2(
                    vel, 2f
                    ) * 0.7f, SpriteEffects.None);
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(MainTexture.Value, Projectile.Center - Main.screenPosition, MainTexture.Frame(),
                    Color.CadetBlue.MultiplyRGBA(new(1f, 1f, 1f, 0f)), Projectile.velocity.ToRotation() + MathHelper.PiOver2, MainTexture.Size() * new Vector2(0.5f, 0f), new Vector2(
                        vel, 2f
                        ) * 0.4f, SpriteEffects.None);
            }

            return false;
        }
    }
}
