using System;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ImmolationSpray : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public Color mainColor = Color.Lerp(Color.Chartreuse, Color.White, 0.35f);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 3;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (time > 5 && targetDist < 1400f)
            {
                Projectile.velocity.Y += 0.18f;
                Projectile.velocity.X *= 0.9835f;

                if (Main.rand.NextBool(3))
                {
                    Vector2 placement = Projectile.Center + Main.rand.NextVector2Circular(8, 8);
                    float speed = Main.rand.NextFloat(0.2f, 0.7f);
                    Particle spark = new GlowOrbParticle(placement, -Projectile.velocity * speed, false, 7, Main.rand.NextFloat(0.4f, 0.7f), mainColor);
                    GeneralParticleHandler.SpawnParticle(spark);

                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? 278 : 263, -Projectile.velocity);
                    dust.scale = dust.type == 278 ? Main.rand.NextFloat(0.2f, 0.5f) : Main.rand.NextFloat(0.4f, 1.1f);
                    dust.velocity = (new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.7f));
                    dust.noGravity = true;
                    dust.color = mainColor;
                }
            }
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SoundStyle sound = new("CalamityMod/Sounds/Item/PlasmaSmall");
            SoundEngine.PlaySound(sound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);

            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.88f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override void OnKill(int timeLeft)
        {
            
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (targetDist < 1400)
            {
                Particle spark = new GlowOrbParticle(Projectile.Center + oldVelocity.SafeNormalize(Vector2.UnitX) * 11, Projectile.velocity * 0.001f, false, 60, 1.6f, mainColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 1)
                return false;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark");

            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], mainColor with { A = 0 } * 0.3f, 1, tex.Value);
            float squash = Utils.GetLerpValue(-3, 10, Projectile.velocity.Length(), true);
            for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, mainColor with { A = 0 } * 0.6f, Projectile.rotation, tex.Size() * 0.5f, new Vector2(0.4f, squash) * 0.045f * (i == 0 ? 0.6f : 1), SpriteEffects.None);
            return false;
        }
    }
}
